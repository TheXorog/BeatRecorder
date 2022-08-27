using BeatRecorder.Entities;
using BeatRecorder.Enums;

namespace BeatRecorder.Util.BeatSaber;

internal class DataPullerHandler : BaseBeatSaberHandler
{
    private Program Program = null;

    private WebsocketClient mainSocket { get; set; }
    private WebsocketClient dataSocket { get; set; }

    internal SharedStatus CurrentStatus => new(CurrentMain, CurrentData, CurrentMaxCombo);
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

        Task mainConn = Task.Run(() =>
        {
            mainSocket = new WebsocketClient(new Uri($"ws://{Program.status.LoadedConfig.BeatSaberUrl}:{Program.status.LoadedConfig.BeatSaberPort}/BSDataPuller/MapData"), factory)
            {
                ReconnectTimeout = null,
                ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
            };

            mainSocket.MessageReceived.Subscribe(msg => { MainMessageRecieved(msg.Text); });
            mainSocket.ReconnectionHappened.Subscribe(type => { Reconnected(type); });
            mainSocket.DisconnectionHappened.Subscribe(type => { Disconnected(type); });

            mainSocket.Start().Wait();

            while (!mainSocket.IsRunning)
                Thread.Sleep(50);

            _logger.LogInfo($"Connected to Beat Saber via DataPuller Main Socket");
        });

        Task dataConn = Task.Run(() =>
        {
            dataSocket = new WebsocketClient(new Uri($"ws://{Program.status.LoadedConfig.BeatSaberUrl}:{Program.status.LoadedConfig.BeatSaberPort}/BSDataPuller/LiveData"), factory)
            {
                ReconnectTimeout = null,
                ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
            };

            dataSocket.MessageReceived.Subscribe(msg => { DataMessageRecieved(msg.Text); });
            dataSocket.ReconnectionHappened.Subscribe(type => { Reconnected(type); });
            dataSocket.DisconnectionHappened.Subscribe(type => { Disconnected(type); });

            dataSocket.Start().Wait();

            while (!dataSocket.IsRunning)
                Thread.Sleep(50);

            _logger.LogInfo($"Connected to Beat Saber via DataPuller Data Socket");
        });

        while (!mainConn.IsCompleted || !dataConn.IsCompleted)
            Thread.Sleep(50);

        return this;
    }

    public override SharedStatus GetCurrentStatus() => CurrentStatus;
    public override SharedStatus GetLastCompletedStatus() => LastCompletedStatus;

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

        if (InLevel != _status.InLevel)
        {
            if (!InLevel && _status.InLevel)
            {
                InLevel = true;
                _logger.LogInfo($"Started playing \"{_status.SongName}\" by \"{_status.SongAuthor}\"");

                CurrentMain = _status;
                CurrentMaxCombo = 0;

                if (!Program.status.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                    Program.ObsClient.SetCurrentScene(Program.status.LoadedConfig.OBSIngameScene);

                _ = Program.ObsClient.StartRecording();
            }
            else if (InLevel && !_status.InLevel)
            {
                Thread.Sleep(500);
                InLevel = false;
                IsPaused = false;

                _logger.LogInfo($"Stopped playing \"{_status.SongName}\" by \"{_status.SongAuthor}\"");

                CurrentMain = _status;

                CurrentStatus.Update(new SharedStatus(CurrentMain, CurrentData, CurrentMaxCombo));
                LastCompletedStatus = CurrentStatus.Clone();

                _ = Program.ObsClient.StopRecording();

                if (!Program.status.LoadedConfig.OBSMenuScene.IsNullOrWhiteSpace())
                    Program.ObsClient.SetCurrentScene(Program.status.LoadedConfig.OBSMenuScene);

                if (Program.status.LoadedConfig.PauseRecordingOnIngamePause)
                    Program.ObsClient.ResumeRecording();
            }
        }

        if (_status.InLevel)
        {
            if (IsPaused != _status.LevelPaused)
            {
                if (IsPaused && _status.LevelPaused)
                {
                    IsPaused = true;
                    _logger.LogInfo("Song paused.");

                    if (Program.status.LoadedConfig.PauseRecordingOnIngamePause)
                        Program.ObsClient.PauseRecording();

                    if (!Program.status.LoadedConfig.OBSPauseScene.IsNullOrWhiteSpace())
                        Program.ObsClient.SetCurrentScene(Program.status.LoadedConfig.OBSPauseScene);
                }
                else if (IsPaused && !_status.LevelPaused)
                {
                    IsPaused = false;
                    _logger.LogInfo("Song resumed.");

                    if (Program.status.LoadedConfig.PauseRecordingOnIngamePause)
                        Program.ObsClient.ResumeRecording();

                    if (!Program.status.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                        Program.ObsClient.SetCurrentScene(Program.status.LoadedConfig.OBSIngameScene);
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

        if (InLevel && CurrentData.unixTimestamp < _status.unixTimestamp)
            CurrentData = _status;

        if (CurrentMaxCombo < _status.Combo)
            CurrentMaxCombo = _status.Combo;
    }

    private void Disconnected(DisconnectionInfo msg)
    {
        try
        {
            Process[] processCollection = Process.GetProcesses();

            if (!processCollection.Any(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")))
            {
                if (LastWarning != ConnectionTypeWarning.NoProcess)
                {
                    _logger.LogWarn($"Couldn't find a BeatSaber process, is BeatSaber started? ({msg.Type})");
                }
                LastWarning = ConnectionTypeWarning.NoProcess;
            }
            else
            {
                bool FoundWebSocketDll = false;

                string InstallationDirectory = processCollection.First(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")).MainModule.FileName;
                InstallationDirectory = InstallationDirectory.Remove(InstallationDirectory.LastIndexOf("\\"), InstallationDirectory.Length - InstallationDirectory.LastIndexOf("\\"));

                if (Directory.GetDirectories(InstallationDirectory).Any(x => x.ToLower().EndsWith("plugins")))
                {
                    if (Directory.GetFiles($"{InstallationDirectory}\\Plugins").Any(x => x.Contains("DataPuller") && x.EndsWith(".dll")))
                    {
                        FoundWebSocketDll = true;
                    }
                }
                else
                {
                    if (LastWarning != ConnectionTypeWarning.NotModded)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the BSDataPuller modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install BSDataPuller: https://bit.ly/3mcvC7g) ({msg.Type})");
                    }
                    LastWarning = ConnectionTypeWarning.NotModded;
                }

                if (FoundWebSocketDll)
                {
                    if (LastWarning != ConnectionTypeWarning.ModInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running and the BSDataPuller modifaction seems to be installed. Please make sure you put in the right port and you installed all of BSDataPuller' dependiencies! (If not installed, please install it: https://bit.ly/3mcvC7g) ({msg.Type})");
                    }
                    LastWarning = ConnectionTypeWarning.ModInstalled;
                }
                else
                {
                    if (LastWarning != ConnectionTypeWarning.ModNotInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the BSDataPuller modifaction doesn't seem to be installed. Please make sure to install BSDataPuller! (If not installed, please install it: https://bit.ly/3mcvC7g) ({msg.Type})");
                    }
                    LastWarning = ConnectionTypeWarning.ModNotInstalled;
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
        if (msg.Type != ReconnectionType.Initial)
            _logger.LogInfo($"Beat Saber Connection via DataPuller re-established: {msg.Type}");

        LastWarning = ConnectionTypeWarning.Connected;
    }
}
