using BeatRecorder.Entities;
using BeatRecorder.Enums;

namespace BeatRecorder.Util.BeatSaber;

internal class HttpStatusHandler : BaseBeatSaberHandler
{
    private WebsocketClient socket { get; set; }

    ConnectionTypeWarning LastWarning = ConnectionTypeWarning.Connected;

    private Program Program = null;
    internal SharedStatus CurrentStatus => new(this.Current.status, this);
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

                this.Current = _status;

                if (!this.Program.LoadedConfig.OBSMenuScene.IsNullOrWhiteSpace())
                    this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSMenuScene);
                break;
            }

            case "songStart":
            {
                _logger.LogInfo($"Started playing \"{_status.status.beatmap.songName}\" by \"{_status.status.beatmap.songAuthorName}\"");

                this.Current = _status;

                if (!this.Program.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                    this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSIngameScene);

                _ = this.Program.ObsClient.StartRecording();
                break;
            }

            case "finished":
            {
                _logger.LogInfo($"Song finished.");
                this.Current.status.performance.finished = true;
                break;
            }

            case "failed":
            {
                _logger.LogInfo($"Song failed.");
                this.Current.status.performance.failed = true;
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

            case "resume":
            {
                _logger.LogInfo($"Song resumed.");

                if (this.Program.LoadedConfig.PauseRecordingOnIngamePause)
                    this.Program.ObsClient.ResumeRecording();

                if (!this.Program.LoadedConfig.OBSIngameScene.IsNullOrWhiteSpace())
                    this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSIngameScene);

                break;
            }

            case "scoreChanged":
            {
                this.Current.status.performance = _status.status.performance;
                break;
            }

            case "menu":
            {
                this.CurrentStatus.Update(new SharedStatus(_status.status, this));
                this.LastCompletedStatus = this.CurrentStatus.Clone();
                _logger.LogInfo($"Stopped playing \"{this.LastCompletedStatus.BeatmapInfo.NameWithSub}\" by \"{this.LastCompletedStatus.BeatmapInfo.Author}\"");
                this.Current = _status;
                _ = this.Program.ObsClient.StopRecording();

                if (!this.Program.LoadedConfig.OBSMenuScene.IsNullOrWhiteSpace())
                    this.Program.ObsClient.SetCurrentScene(this.Program.LoadedConfig.OBSMenuScene);

                if (this.Program.LoadedConfig.PauseRecordingOnIngamePause)
                    this.Program.ObsClient.ResumeRecording();

                break;
            }
        }
    }

    internal void Reconnected(ReconnectionInfo msg)
    {
        if (msg.Type != ReconnectionType.Initial)
            _logger.LogInfo($"Beat Saber Connection via Http Status re-established: {msg.Type}");

        this.LastWarning = ConnectionTypeWarning.Connected;
    }

    internal void Disconnected(DisconnectionInfo msg)
    {
        try
        {
            var processCollection = Process.GetProcesses();

            if (!processCollection.Any(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")))
            {
                if (this.LastWarning != ConnectionTypeWarning.NoProcess)
                {
                    _logger.LogWarn($"Couldn't find a BeatSaber process, is BeatSaber started? ({msg.Type})");
                }
                this.LastWarning = ConnectionTypeWarning.NoProcess;
            }
            else
            {
                var FoundWebSocketDll = false;

                var InstallationDirectory = processCollection.First(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")).MainModule.FileName.Replace("\\", "/");
                InstallationDirectory = InstallationDirectory.Remove(InstallationDirectory.LastIndexOf("/"), InstallationDirectory.Length - InstallationDirectory.LastIndexOf("/"));

                if (Directory.GetDirectories(InstallationDirectory).Any(x => x.ToLower().EndsWith("plugins")))
                {
                    if (Directory.GetFiles($"{InstallationDirectory}/Plugins").Any(x => x.Contains("HTTPStatus") && x.EndsWith(".dll")))
                    {
                        FoundWebSocketDll = true;
                    }
                }
                else
                {
                    if (this.LastWarning != ConnectionTypeWarning.NotModded)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the beatsaber-http-status modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install beatsaber-http-status: https://bit.ly/3wYX3Dd) ({msg.Type})");
                    }
                    this.LastWarning = ConnectionTypeWarning.NotModded;
                    return;
                }

                if (FoundWebSocketDll)
                {
                    if (this.LastWarning != ConnectionTypeWarning.ModInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running and the beatsaber-http-status modifaction seems to be installed. Please make sure you put in the right port and you installed all of beatsaber-http-status' dependiencies! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                    }
                    this.LastWarning = ConnectionTypeWarning.ModInstalled;
                }
                else
                {
                    if (this.LastWarning != ConnectionTypeWarning.ModNotInstalled)
                    {
                        _logger.LogFatal($"Beat Saber seems to be running but the beatsaber-http-status modifaction doesn't seem to be installed. Please make sure to install beatsaber-http-status! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                    }
                    this.LastWarning = ConnectionTypeWarning.ModNotInstalled;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check if beatsaber-http-status is installed: (Disconnect Reason: {msg.Type}) {ex}");
        }
    }

    internal override bool GetIsRunning() => this.socket?.IsRunning ?? false;
}
