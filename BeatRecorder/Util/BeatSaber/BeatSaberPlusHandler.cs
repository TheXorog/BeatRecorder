using BeatRecorder.Entities;
using BeatRecorder.Enums;

namespace BeatRecorder.Util.BeatSaber;

internal class BeatSaberPlusHandler : BaseBeatSaberHandler
{
    private WebsocketClient socket { get; set; }

    ConnectionTypeWarning LastWarning = ConnectionTypeWarning.Connected;

    private Program Program = null;
    internal SharedStatus CurrentStatus => new(this.Current, this.GameInfo, this.CurrentMaxCombo, this);
    internal SharedStatus LastCompletedStatus { get; set; }

    private SharedStatus.Game GameInfo { get; set; } = null;

    private BeatSaberPlus Current = new();

    int CurrentMaxCombo = 0;
    bool IsPlaying = false;

    public override BaseBeatSaberHandler Initialize(Program program)
    {
        _logger.LogInfo("Initializing Connection to Beat Saber via BeatSaberPlus..");

        this.Program = program;

        var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
        {
            Options =
            {
            KeepAliveInterval = TimeSpan.FromSeconds(5)
            }
        });

        this.socket = new WebsocketClient(new Uri($"ws://{program.LoadedConfig.BeatSaberUrl}:{program.LoadedConfig.BeatSaberPort}/socket"), factory)
        {
            ReconnectTimeout = null,
            ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
        };

        _ = this.socket.MessageReceived.Subscribe(msg => this.MessageReceived(msg.Text));
        _ = this.socket.ReconnectionHappened.Subscribe(this.Reconnected);
        _ = this.socket.DisconnectionHappened.Subscribe(this.Disconnected);

        this.socket.Start().Wait();
        return this;
    }

    public override SharedStatus GetCurrentStatus() => this.CurrentStatus;

    public override SharedStatus GetLastCompletedStatus() => this.LastCompletedStatus;

    private void MessageReceived(string text)
    {
        BeatSaberPlus _status;

        try
        {
            _status = JsonConvert.DeserializeObject<BeatSaberPlus>(text);
        }
        catch (Exception ex)
        {
            _logger.LogFatal($"Unable to convert message into object", ex);
            return;
        }

        if (_status.mapInfoChanged is not null)
            this.Current.mapInfoChanged = _status.mapInfoChanged;

        switch (_status._type)
        {
            case "handshake":
            {
                _logger.LogInfo($"Connected to Beat Saber via BeatSaberPlus");

                this.GameInfo = new SharedStatus.Game
                {
                    ModUsed = Mod.BeatSaberPlus,
                    ModVersion = _status.protocolVersion.ToString(),
                    GameVersion = _status.gameVersion
                };
                break;
            }
            case "event":
            {
                switch (_status._event)
                {
                    case "gameState":
                    {
                        if (_status.gameStateChanged.Equals("menu", StringComparison.CurrentCultureIgnoreCase) && this.IsPlaying)
                        {
                            this.IsPlaying = false;

                            this.CurrentStatus.Update(new SharedStatus(_status, this.GameInfo, this.CurrentMaxCombo, this));
                            this.LastCompletedStatus = this.CurrentStatus.Clone();
                            _logger.LogInfo($"Stopped playing \"{this.LastCompletedStatus.BeatmapInfo.NameWithSub}\" by \"{this.LastCompletedStatus.BeatmapInfo.Author}\"");
                            _ = this.Program.ObsClient.StopRecording();

                            if (!this.Program.LoadedConfig.OBSMenuScene.IsNullOrWhiteSpace())
                                this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSMenuScene);

                            if (this.Program.LoadedConfig.PauseRecordingOnIngamePause)
                                this.Program.ObsClient.ResumeRecording();

                            break;
                        }
                        else if (_status.gameStateChanged.Equals("playing", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.IsPlaying = true;
                            _logger.LogInfo($"Started playing \"{this.Current.mapInfoChanged.name}\" by \"{this.Current.mapInfoChanged.artist}\"");

                            this.CurrentMaxCombo = 0;
                            this.Current.scoreEvent = new();

                            if (!this.Program.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                                this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSIngameScene);

                            _ = this.Program.ObsClient.StartRecording();
                            break;
                        }
                        break;
                    }
                    case "mapInfo":
                    {
                        this.Current.mapInfoChanged = _status.mapInfoChanged;
                        break;
                    }
                    case "score":
                    {
                        if ((this.Current.scoreEvent?.time ?? 0f) < _status.scoreEvent?.time && _status.scoreEvent is not null)
                        {
                            this.Current.scoreEvent = _status.scoreEvent;
                        }

                        if (this.CurrentMaxCombo < _status.scoreEvent.combo)
                            this.CurrentMaxCombo = _status.scoreEvent.combo;

                        break;
                    }
                    case "resume":
                    {
                        _logger.LogInfo($"Song resumed.");

                        if (this.Program.LoadedConfig.PauseRecordingOnIngamePause)
                            this.Program.ObsClient.ResumeRecording();

                        if (!this.Program.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                            this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSIngameScene);

                        break;
                    }
                    case "pause":
                    {
                        _logger.LogInfo($"Song paused.");

                        if (this.Program.LoadedConfig.PauseRecordingOnIngamePause)
                            this.Program.ObsClient.PauseRecording();

                        if (!this.Program.LoadedConfig.OBSPauseScene.IsNullOrWhiteSpace())
                            this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSPauseScene);

                        break;
                    }
                }
                break;
            }
        }
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
                    if (Directory.GetFiles($"{InstallationDirectory}\\Plugins").Any(x => x.Contains("BeatSaberPlus") && x.EndsWith(".dll")))
                    {
                        FoundWebSocketDll = true;
                    }
                }
                else
                {
                    if (this.LastWarning != ConnectionTypeWarning.NotModded)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the BeatSaberPlus modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install BeatSaberPlus: https://bit.ly/3wYX3Dd) ({msg.Type})");
                        this.Program.steamNotifications?.SendNotification("Disconnected from Beat Saber", 1000, MessageType.ERROR);
                    }
                    this.LastWarning = ConnectionTypeWarning.NotModded;
                    return;
                }

                if (FoundWebSocketDll)
                {
                    if (this.LastWarning != ConnectionTypeWarning.ModInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running and the BeatSaberPlus modifaction seems to be installed. Please make sure you put in the right port and you installed all of BeatSaberPlus' dependiencies! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                        this.Program.steamNotifications?.SendNotification("Disconnected from Beat Saber", 1000, MessageType.ERROR);
                    }
                    this.LastWarning = ConnectionTypeWarning.ModInstalled;
                }
                else
                {
                    if (this.LastWarning != ConnectionTypeWarning.ModNotInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the BeatSaberPlus modifaction doesn't seem to be installed. Please make sure to install BeatSaberPlus! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                        this.Program.steamNotifications?.SendNotification("Disconnected from Beat Saber", 1000, MessageType.ERROR);
                    }
                    this.LastWarning = ConnectionTypeWarning.ModNotInstalled;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check if BeatSaberPlus is installed: (Disconnect Reason: {msg.Type}) {ex}");
        }
    }

    private void Reconnected(ReconnectionInfo msg)
    {
        this.Program.steamNotifications?.SendNotification("Connected to Beat Saber", 1000, MessageType.INFO);

        if (msg.Type != ReconnectionType.Initial)
            _logger.LogInfo($"Beat Saber Connection via BeatSaberPlus re-established: {msg.Type}");

        this.LastWarning = ConnectionTypeWarning.Connected;
    }

    internal override bool GetIsRunning() => this.socket?.IsRunning ?? false;
}
