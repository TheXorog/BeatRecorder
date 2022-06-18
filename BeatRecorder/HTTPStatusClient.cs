namespace BeatRecorder;

internal class HTTPStatusClient
{
    private HTTPStatusClient() { }

    internal static WebsocketClient WebSocket { get; set; }

    internal static async Task<HTTPStatusClient> InitializeClient(Settings LoadedSettings)
    {
        var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
        {
            Options =
            {
                KeepAliveInterval = TimeSpan.FromSeconds(5)
            }
        });

        WebSocket = new WebsocketClient(new Uri($"ws://{LoadedSettings.BeatSaberUrl}:{LoadedSettings.BeatSaberPort}/socket"), factory)
        {
            ReconnectTimeout = null,
            ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
        };

        WebSocket.MessageReceived.Subscribe(msg => { MessageReceived(msg.Text); });
        WebSocket.ReconnectionHappened.Subscribe(type => { Reconnected(type); });
        WebSocket.DisconnectionHappened.Subscribe(type => { Disconnected(type); });

        _logger.LogInfo($"Connecting to BeatSaber via HttpStatus..");
        await WebSocket.Start();
        _logger.LogInfo($"Connected to BeatSaber via HttpStatus.");

        return new HTTPStatusClient();
    }

    private static void MessageReceived(string e)
    {
        HttpStatusStatus.BeatSaberEvent _status;

        try
        {
            _status = JsonConvert.DeserializeObject<HttpStatusStatus.BeatSaberEvent>(e);
        }
        catch (Exception ex)
        {
            _logger.LogFatal($"Unable to parse the HttpStatus Event", ex);
            return;
        }

        switch (_status.@event)
        {
            case "hello":

                try { HttpStatusStatus.HttpStatusCurrentBeatmap = _status.status.beatmap; } catch { }
                try { HttpStatusStatus.HttpStatusCurrentPerformance = _status.status.performance; } catch { }

                try
                {
                    if (Program.LoadedSettings.OBSMenuScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSMenuScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }

                _logger.LogInfo("[BS-HS] Connected.");
                break;

            case "songStart":
                _logger.LogDebug("[BS-HS] Song started.");
                _logger.LogInfo($"[BS-HS] Started playing \"{_status.status.beatmap.songName}\" by \"{_status.status.beatmap.songAuthorName}\"");

                HttpStatusStatus.FailedCurrentSong = false;
                HttpStatusStatus.FinishedCurrentSong = false;
                HttpStatusStatus.HttpStatusCurrentBeatmap = _status.status.beatmap;
                HttpStatusStatus.HttpStatusCurrentPerformance = _status.status.performance;

                try
                {
                    if (Program.LoadedSettings.OBSIngameScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSIngameScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }

                try
                {
                    _ = OBSWebSocket.StartRecording();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }
                break;

            case "finished":
                _logger.LogInfo("[BS-HS] Song finished.");

                HttpStatusStatus.HttpStatusCurrentPerformance = _status.status.performance;
                HttpStatusStatus.HttpStatusLastPerformance = HttpStatusStatus.HttpStatusCurrentPerformance;
                HttpStatusStatus.FinishedCurrentSong = true;

                try
                {
                    if (Program.LoadedSettings.OBSMenuScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSMenuScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }
                break;

            case "failed":
                _logger.LogInfo("[BS-HS] Song failed.");

                HttpStatusStatus.HttpStatusCurrentPerformance = _status.status.performance;
                HttpStatusStatus.HttpStatusLastPerformance = HttpStatusStatus.HttpStatusCurrentPerformance;
                HttpStatusStatus.FailedCurrentSong = true;

                try
                {
                    if (Program.LoadedSettings.OBSMenuScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSMenuScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }
                break;

            case "pause":
                _logger.LogInfo("[BS-HS] Song paused.");

                try
                {
                    if (Program.LoadedSettings.PauseRecordingOnIngamePause)
                        if (Program.obsWebSocket.IsStarted)
                            Program.obsWebSocket.Send($"{{\"request-type\":\"PauseRecording\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }

                try
                {
                    if (Program.LoadedSettings.OBSPauseScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSPauseScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }
                break;

            case "resume":
                _logger.LogInfo("[BS-HS] Song resumed.");

                try
                {
                    if (Program.LoadedSettings.PauseRecordingOnIngamePause)
                        if (Program.obsWebSocket.IsStarted)
                            Program.obsWebSocket.Send($"{{\"request-type\":\"ResumeRecording\", \"message-id\":\"ResumeRecording\"}}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }

                try
                {
                    if (Program.LoadedSettings.OBSIngameScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSIngameScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }
                break;

            case "menu":
                _logger.LogDebug("[BS-HS] Menu entered.");
                _logger.LogInfo($"[BS-HS] Stopped playing \"{_status?.status?.beatmap?.songName}\" by \"{_status?.status?.beatmap?.songAuthorName}\"");

                try
                {
                    HttpStatusStatus.HttpStatusLastPerformance = HttpStatusStatus.HttpStatusCurrentPerformance;
                    HttpStatusStatus.HttpStatusLastBeatmap = HttpStatusStatus.HttpStatusCurrentBeatmap;

                    HttpStatusStatus.FinishedLastSong = HttpStatusStatus.FinishedCurrentSong;
                    HttpStatusStatus.FailedLastSong = HttpStatusStatus.FailedCurrentSong;
                    _ = OBSWebSocket.StopRecording(OBSWebSocketStatus.CancelStopRecordingDelay.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }

                try
                {
                    if (Program.LoadedSettings.OBSMenuScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSMenuScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }
                break;

            case "scoreChanged":
                HttpStatusStatus.HttpStatusCurrentPerformance = _status.status.performance;
                break;
        }
    }

    internal static void Reconnected(ReconnectionInfo msg)
    {
        if (msg.Type != ReconnectionType.Initial)
            _logger.LogWarn($"Reconnected to BeatSaber: {msg.Type}");

        Objects.LastHttpStatusWarning = ConnectionTypeWarning.Connected;
        Program.SendNotification("Connected to Beat Saber", 1000, MessageType.INFO);
    }

    internal static void Disconnected(DisconnectionInfo msg)
    {
        Util.Util.CheckForMod(msg, "httpstatus", "Http Status");
    }
}
