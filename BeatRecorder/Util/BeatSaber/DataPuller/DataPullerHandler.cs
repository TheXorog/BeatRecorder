using BeatRecorder.Entities;
using BeatRecorder.Enums;

namespace BeatRecorder.Util.BeatSaber;

internal class DataPullerHandler : BaseBeatSaberHandler
{
    private Program Program = null;

    private WebsocketClient mainSocket { get; set; }
    private WebsocketClient dataSocket { get; set; }

    internal SharedStatus CurrentStatus => new(this.CurrentMain, this.CurrentData, this.CurrentMaxCombo, this);
    internal SharedStatus LastCompletedStatus { get; set; }

    private DataPullerMain CurrentMain = null;
    private DataPullerData CurrentData = null;

    ConnectionTypeWarning LastWarning = ConnectionTypeWarning.Connected;

    int CurrentMaxCombo = 0;
    private bool InLevel = false;
    private bool IsPaused = false;

    public override BaseBeatSaberHandler Initialize(Program program)
    {
        _logger.LogInfo("Initializing Connection to Beat Saber via DataPuller..");

        this.Program = program;

        var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
        {
            Options =
            {
            KeepAliveInterval = TimeSpan.FromSeconds(5)
            }
        });

        var mainConn = Task.Run(() =>
        {
            this.mainSocket = new WebsocketClient(new Uri($"ws://{this.Program.LoadedConfig.BeatSaberUrl}:{this.Program.LoadedConfig.BeatSaberPort}/BSDataPuller/MapData"), factory)
            {
                ReconnectTimeout = null,
                ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
            };

            _ = this.mainSocket.MessageReceived.Subscribe(msg => this.MainMessageRecieved(msg.Text));
            _ = this.mainSocket.ReconnectionHappened.Subscribe(type => this.Reconnected(type));
            _ = this.mainSocket.DisconnectionHappened.Subscribe(type => this.Disconnected(type));

            this.mainSocket.Start().Wait();

            while (!this.mainSocket.IsRunning)
                Thread.Sleep(50);

            _logger.LogInfo($"Connected to Beat Saber via DataPuller Main Socket");
        });

        var dataConn = Task.Run(() =>
        {
            this.dataSocket = new WebsocketClient(new Uri($"ws://{this.Program.LoadedConfig.BeatSaberUrl}:{this.Program.LoadedConfig.BeatSaberPort}/BSDataPuller/LiveData"), factory)
            {
                ReconnectTimeout = null,
                ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
            };

            _ = this.dataSocket.MessageReceived.Subscribe(msg => this.DataMessageRecieved(msg.Text));
            _ = this.dataSocket.ReconnectionHappened.Subscribe(type => this.Reconnected(type));
            _ = this.dataSocket.DisconnectionHappened.Subscribe(type => this.Disconnected(type));

            this.dataSocket.Start().Wait();

            while (!this.dataSocket.IsRunning)
                Thread.Sleep(50);

            _logger.LogInfo($"Connected to Beat Saber via DataPuller Data Socket");
        });

        while (!mainConn.IsCompleted || !dataConn.IsCompleted)
            Thread.Sleep(50);

        return this;
    }

    public override SharedStatus GetCurrentStatus() => this.CurrentStatus;
    public override SharedStatus GetLastCompletedStatus() => this.LastCompletedStatus;

    private void MainMessageRecieved(string text)
    {
        DataPullerMain _status;

        try
        {
            _status = JsonConvert.DeserializeObject<DataPullerMain>(text);
        }
        catch (Exception ex)
        {
            _logger.LogFatal($"Unable to convert message into object", ex);
            return;
        }

        if (this.InLevel != _status.InLevel)
        {
            if (!this.InLevel && _status.InLevel)
            {
                this.InLevel = true;
                _logger.LogInfo($"Started playing \"{_status.SongName}\" by \"{_status.SongAuthor}\"");

                this.CurrentMain = _status;
                this.CurrentMaxCombo = 0;

                if (!this.Program.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                    this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSIngameScene);

                _ = this.Program.ObsClient.StartRecording();
            }
            else if (this.InLevel && !_status.InLevel)
            {
                Thread.Sleep(500);
                this.InLevel = false;
                this.IsPaused = false;

                _logger.LogInfo($"Stopped playing \"{_status.SongName}\" by \"{_status.SongAuthor}\"");

                this.CurrentMain = _status;

                this.CurrentStatus.Update(new SharedStatus(this.CurrentMain, this.CurrentData, this.CurrentMaxCombo, this));
                this.LastCompletedStatus = this.CurrentStatus.Clone();

                _ = this.Program.ObsClient.StopRecording();

                if (!this.Program.LoadedConfig.OBSMenuScene.IsNullOrWhiteSpace())
                    this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSMenuScene);

                if (this.Program.LoadedConfig.PauseRecordingOnIngamePause)
                    this.Program.ObsClient.ResumeRecording();
            }
        }

        if (_status.InLevel)
        {
            if (this.IsPaused != _status.LevelPaused)
            {
                if (this.IsPaused && _status.LevelPaused)
                {
                    this.IsPaused = true;
                    _logger.LogInfo("Song paused.");

                    if (this.Program.LoadedConfig.PauseRecordingOnIngamePause)
                        this.Program.ObsClient.PauseRecording();

                    if (!this.Program.LoadedConfig.OBSPauseScene.IsNullOrWhiteSpace())
                        this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSPauseScene);
                }
                else if (this.IsPaused && !_status.LevelPaused)
                {
                    this.IsPaused = false;
                    _logger.LogInfo("Song resumed.");

                    if (this.Program.LoadedConfig.PauseRecordingOnIngamePause)
                        this.Program.ObsClient.ResumeRecording();

                    if (!this.Program.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                        this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSIngameScene);
                }
            }
        }
    }

    private void DataMessageRecieved(string text)
    {
        DataPullerData _status;

        try
        {
            _status = JsonConvert.DeserializeObject<DataPullerData>(text);
        }
        catch (Exception ex)
        {
            _logger.LogFatal($"Unable to convert message into object", ex);
            return;
        }

        if (this.InLevel && (this.CurrentData?.UnixTimestamp ?? 0) < _status.UnixTimestamp)
            this.CurrentData = _status;

        if (this.CurrentMaxCombo < _status.Combo)
            this.CurrentMaxCombo = _status.Combo;
    }

    private void Disconnected(DisconnectionInfo msg)
    {
        try
        {
            var processCollection = Process.GetProcesses();

            if (!processCollection.Any(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")))
            {
                if (this.LastWarning != ConnectionTypeWarning.NoProcess)
                {
                    _logger.LogWarn($"Couldn't find a BeatSaber process, is BeatSaber started? ({msg.Type})");
                    this.Program.steamNotifications?.SendNotification("Disconnected from Beat Saber", 1000, MessageType.ERROR);
                }
                this.LastWarning = ConnectionTypeWarning.NoProcess;
            }
            else
            {
                var FoundWebSocketDll = false;

                var InstallationDirectory = processCollection.First(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")).MainModule.FileName;
                InstallationDirectory = InstallationDirectory.Remove(InstallationDirectory.LastIndexOf('\\'), InstallationDirectory.Length - InstallationDirectory.LastIndexOf('\\'));

                if (Directory.GetDirectories(InstallationDirectory).Any(x => x.ToLower().EndsWith("plugins")))
                {
                    if (Directory.GetFiles($"{InstallationDirectory}\\Plugins").Any(x => x.Contains("DataPuller") && x.EndsWith(".dll")))
                    {
                        FoundWebSocketDll = true;
                    }
                }
                else
                {
                    if (this.LastWarning != ConnectionTypeWarning.NotModded)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the BSDataPuller modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install BSDataPuller: https://bit.ly/3mcvC7g) ({msg.Type})");
                        this.Program.steamNotifications?.SendNotification("Disconnected from Beat Saber", 1000, MessageType.ERROR);
                    }
                    this.LastWarning = ConnectionTypeWarning.NotModded;
                    return;
                }

                if (FoundWebSocketDll)
                {
                    if (this.LastWarning != ConnectionTypeWarning.ModInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running and the BSDataPuller modifaction seems to be installed. Please make sure you put in the right port and you installed all of BSDataPuller' dependiencies! (If not installed, please install it: https://bit.ly/3mcvC7g) ({msg.Type})");
                        this.Program.steamNotifications?.SendNotification("Disconnected from Beat Saber", 1000, MessageType.ERROR);
                    }
                    this.LastWarning = ConnectionTypeWarning.ModInstalled;
                }
                else
                {
                    if (this.LastWarning != ConnectionTypeWarning.ModNotInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the BSDataPuller modifaction doesn't seem to be installed. Please make sure to install BSDataPuller! (If not installed, please install it: https://bit.ly/3mcvC7g) ({msg.Type})");
                        this.Program.steamNotifications?.SendNotification("Disconnected from Beat Saber", 1000, MessageType.ERROR);
                    }
                    this.LastWarning = ConnectionTypeWarning.ModNotInstalled;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check if BSDataPuller is installed: (Disconnect Reason: {msg.Type}) {ex}");
        }
    }

    private void Reconnected(ReconnectionInfo msg)
    {
        this.Program.steamNotifications?.SendNotification("Connected to Beat Saber", 1000, MessageType.INFO);

        if (msg.Type != ReconnectionType.Initial)
            _logger.LogInfo($"Beat Saber Connection via DataPuller re-established: {msg.Type}");

        this.LastWarning = ConnectionTypeWarning.Connected;
    }

    internal override bool GetIsRunning() => (this.mainSocket?.IsRunning ?? false) && (this.dataSocket?.IsRunning ?? false);
}
