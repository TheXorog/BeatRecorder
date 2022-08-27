using BeatRecorder.Entities;
using BeatRecorder.Enums;

namespace BeatRecorder.Util.BeatSaber;

internal class BeatSaberPlusHandler : BaseBeatSaberHandler
{
    private WebsocketClient socket { get; set; }

    ConnectionTypeWarning LastWarning = ConnectionTypeWarning.Connected;

    private Program Program = null;
    internal SharedStatus CurrentStatus => new(Current, GameInfo, CurrentMaxCombo);
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

        socket = new WebsocketClient(new Uri($"ws://{program.status.LoadedConfig.BeatSaberUrl}:{program.status.LoadedConfig.BeatSaberPort}/socket"), factory)
        {
            ReconnectTimeout = null,
            ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
        };

        socket.MessageReceived.Subscribe(msg => { MessageReceived(msg.Text); });
        socket.ReconnectionHappened.Subscribe(type => { Reconnected(type); });
        socket.DisconnectionHappened.Subscribe(type => { Disconnected(type); });

        socket.Start().Wait();
        return this;
    }

    public override SharedStatus GetCurrentStatus() => CurrentStatus;

    public override SharedStatus GetLastCompletedStatus() => LastCompletedStatus;

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
            Current.mapInfoChanged = _status.mapInfoChanged;

        switch (_status._type)
        {
            case "handshake":
            {
                _logger.LogInfo($"Connected to Beat Saber via BeatSaberPlus");

                GameInfo = new SharedStatus.Game
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
                        if (_status.gameStateChanged.ToLower() == "menu" && IsPlaying)
                        {
                            IsPlaying = false;

                            CurrentStatus.Update(new SharedStatus(_status, GameInfo, CurrentMaxCombo));
                            LastCompletedStatus = CurrentStatus.Clone();
                            _logger.LogInfo($"Stopped playing \"{LastCompletedStatus.BeatmapInfo.NameWithSub}\" by \"{LastCompletedStatus.BeatmapInfo.Author}\"");
                            _ = Program.ObsClient.StopRecording();

                            if (!Program.status.LoadedConfig.OBSMenuScene.IsNullOrWhiteSpace())
                                Program.ObsClient.SetCurrentScene(Program.status.LoadedConfig.OBSMenuScene);

                            if (Program.status.LoadedConfig.PauseRecordingOnIngamePause)
                                Program.ObsClient.ResumeRecording();

                            break;
                        }
                        else if (_status.gameStateChanged.ToLower() == "playing")
                        {
                            IsPlaying = true;
                            _logger.LogInfo($"Started playing \"{Current.mapInfoChanged.name}\" by \"{Current.mapInfoChanged.artist}\"");

                            CurrentMaxCombo = 0;
                            Current.scoreEvent = new();

                            if (!Program.status.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                                Program.ObsClient.SetCurrentScene(Program.status.LoadedConfig.OBSIngameScene);

                            _ = Program.ObsClient.StartRecording();
                            break;
                        }
                        break;
                    }
                    case "mapInfo":
                    {
                        Current.mapInfoChanged = _status.mapInfoChanged;
                        break;
                    }
                    case "score":
                    {
                        if ((Current.scoreEvent?.time ?? 0f) < _status.scoreEvent?.time && _status.scoreEvent is not null)
                        {
                            Current.scoreEvent = _status.scoreEvent;
                        }

                        if (CurrentMaxCombo < _status.scoreEvent.combo)
                            CurrentMaxCombo = _status.scoreEvent.combo;

                        break;
                    }
                    case "resume":
                    {
                        _logger.LogInfo($"Song resumed.");

                        if (Program.status.LoadedConfig.PauseRecordingOnIngamePause)
                            Program.ObsClient.ResumeRecording();

                        if (!Program.status.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                            Program.ObsClient.SetCurrentScene(Program.status.LoadedConfig.OBSIngameScene);

                        break;
                    }
                    case "pause":
                    {
                        _logger.LogInfo($"Song paused.");

                        if (Program.status.LoadedConfig.PauseRecordingOnIngamePause)
                            Program.ObsClient.PauseRecording();

                        if (!Program.status.LoadedConfig.OBSPauseScene.IsNullOrWhiteSpace())
                            Program.ObsClient.SetCurrentScene(Program.status.LoadedConfig.OBSPauseScene);

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
                    if (Directory.GetFiles($"{InstallationDirectory}\\Plugins").Any(x => x.Contains("BeatSaberPlus") && x.EndsWith(".dll")))
                    {
                        FoundWebSocketDll = true;
                    }
                }
                else
                {
                    if (LastWarning != ConnectionTypeWarning.NotModded)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the BeatSaberPlus modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install BeatSaberPlus: https://bit.ly/3wYX3Dd) ({msg.Type})");
                    }
                    LastWarning = ConnectionTypeWarning.NotModded;
                }

                if (FoundWebSocketDll)
                {
                    if (LastWarning != ConnectionTypeWarning.ModInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running and the BeatSaberPlus modifaction seems to be installed. Please make sure you put in the right port and you installed all of BeatSaberPlus' dependiencies! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                    }
                    LastWarning = ConnectionTypeWarning.ModInstalled;
                }
                else
                {
                    if (LastWarning != ConnectionTypeWarning.ModNotInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the BeatSaberPlus modifaction doesn't seem to be installed. Please make sure to install BeatSaberPlus! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                    }
                    LastWarning = ConnectionTypeWarning.ModNotInstalled;
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
        if (msg.Type != ReconnectionType.Initial)
            _logger.LogInfo($"Beat Saber Connection via BeatSaberPlus re-established: {msg.Type}");

        LastWarning = ConnectionTypeWarning.Connected;
    }
}
