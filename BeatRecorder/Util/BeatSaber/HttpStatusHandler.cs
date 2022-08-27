using BeatRecorder.Entities;
using BeatRecorder.Enums;

namespace BeatRecorder.Util.BeatSaber;

internal class HttpStatusHandler : BaseBeatSaberHandler
{
    private WebsocketClient socket { get; set; }

    ConnectionTypeWarning LastWarning = ConnectionTypeWarning.Connected;

    private Program Program = null;
    internal SharedStatus CurrentStatus => new(Current.status);
    internal SharedStatus LastCompletedStatus { get; set; }

    private HttpStatus Current = null;

    public override BaseBeatSaberHandler Initialize(Program program)
    {
        _logger.LogInfo("Initializing Connection to Beat Saber via HttpStatus..");

        this.Program = program;

        var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
        {
            Options =
            {
            KeepAliveInterval = TimeSpan.FromSeconds(5)
            }
        });

        socket = new WebsocketClient(new Uri($"ws://{program.LoadedConfig.BeatSaberUrl}:{program.LoadedConfig.BeatSaberPort}/socket"), factory)
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

    internal void MessageReceived(string e)
    {
        HttpStatus _status;

        try
        {
            _status = JsonConvert.DeserializeObject<HttpStatus>(e);
        }
        catch (Exception ex)
        {
            _logger.LogFatal($"Unable to convert message into object", ex);
            return;
        }

        switch (_status.@event)
        {
            case "hello":
            {
                _logger.LogInfo($"Connected to Beat Saber via Http Status");

                Current = _status;

                if (!Program.LoadedConfig.OBSMenuScene.IsNullOrWhiteSpace())
                    Program.ObsClient.SetCurrentScene(Program.LoadedConfig.OBSMenuScene);
                break;
            }

            case "songStart":
            {
                _logger.LogInfo($"Started playing \"{_status.status.beatmap.songName}\" by \"{_status.status.beatmap.songAuthorName}\"");

                Current = _status;

                if (!Program.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                    Program.ObsClient.SetCurrentScene(Program.LoadedConfig.OBSIngameScene);

                _ = Program.ObsClient.StartRecording();
                break;
            }

            case "finished":
            {
                _logger.LogInfo($"Song finished.");
                Current.status.performance.finished = true;
                break;
            }

            case "failed":
            {
                _logger.LogInfo($"Song failed.");
                Current.status.performance.failed = true;
                break;
            }
            
            case "pause":
            {
                _logger.LogInfo($"Song paused.");

                if (Program.LoadedConfig.PauseRecordingOnIngamePause)
                    Program.ObsClient.PauseRecording();

                if (!Program.LoadedConfig.OBSPauseScene.IsNullOrWhiteSpace())
                    Program.ObsClient.SetCurrentScene(Program.LoadedConfig.OBSPauseScene);

                break;
            }

            case "resume":
            {
                _logger.LogInfo($"Song resumed.");

                if (Program.LoadedConfig.PauseRecordingOnIngamePause)
                    Program.ObsClient.ResumeRecording();

                if (!Program.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                    Program.ObsClient.SetCurrentScene(Program.LoadedConfig.OBSIngameScene);

                break;
            }

            case "scoreChanged":
            {
                Current.status.performance = _status.status.performance;
                break;
            }

            case "menu":
            {
                CurrentStatus.Update(new SharedStatus(_status.status));
                LastCompletedStatus = CurrentStatus.Clone();
                _logger.LogInfo($"Stopped playing \"{LastCompletedStatus.BeatmapInfo.NameWithSub}\" by \"{LastCompletedStatus.BeatmapInfo.Author}\"");
                Current = _status;
                _ = Program.ObsClient.StopRecording();

                if (!Program.LoadedConfig.OBSMenuScene.IsNullOrWhiteSpace())
                    Program.ObsClient.SetCurrentScene(Program.LoadedConfig.OBSMenuScene);

                if (Program.LoadedConfig.PauseRecordingOnIngamePause)
                    Program.ObsClient.ResumeRecording();

                break;
            }
        }
    }

    internal void Reconnected(ReconnectionInfo msg)
    {
        Program.steamNotifications?.SendNotification("Connected to Beat Saber", 1000, MessageType.INFO);

        if (msg.Type != ReconnectionType.Initial)
            _logger.LogInfo($"Beat Saber Connection via Http Status re-established: {msg.Type}");

        LastWarning = ConnectionTypeWarning.Connected;
    }

    internal void Disconnected(DisconnectionInfo msg)
    {
        try
        {
            Process[] processCollection = Process.GetProcesses();

            if (!processCollection.Any(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")))
            {
                if (LastWarning != ConnectionTypeWarning.NoProcess)
                {
                    _logger.LogWarn($"Couldn't find a BeatSaber process, is BeatSaber started? ({msg.Type})");
                    Program.steamNotifications?.SendNotification("Disconnected from Beat Saber", 1000, MessageType.ERROR);
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
                    if (Directory.GetFiles($"{InstallationDirectory}\\Plugins").Any(x => x.Contains("HTTPStatus") && x.EndsWith(".dll")))
                    {
                        FoundWebSocketDll = true;
                    }
                }
                else
                {
                    if (LastWarning != ConnectionTypeWarning.NotModded)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the beatsaber-http-status modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install beatsaber-http-status: https://bit.ly/3wYX3Dd) ({msg.Type})");
                        Program.steamNotifications?.SendNotification("Disconnected from Beat Saber", 1000, MessageType.ERROR);
                    }
                    LastWarning = ConnectionTypeWarning.NotModded;
                    return;
                }

                if (FoundWebSocketDll)
                {
                    if (LastWarning != ConnectionTypeWarning.ModInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running and the beatsaber-http-status modifaction seems to be installed. Please make sure you put in the right port and you installed all of beatsaber-http-status' dependiencies! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                        Program.steamNotifications?.SendNotification("Disconnected from Beat Saber", 1000, MessageType.ERROR);
                    }
                    LastWarning = ConnectionTypeWarning.ModInstalled;
                }
                else
                {
                    if (LastWarning != ConnectionTypeWarning.ModNotInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the beatsaber-http-status modifaction doesn't seem to be installed. Please make sure to install beatsaber-http-status! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                        Program.steamNotifications?.SendNotification("Disconnected from Beat Saber", 1000, MessageType.ERROR);
                    }
                    LastWarning = ConnectionTypeWarning.ModNotInstalled;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check if beatsaber-http-status is installed: (Disconnect Reason: {msg.Type}) {ex}");
        }
    }
}
