namespace BeatRecorder;

internal class HTTPStatusClient
{
    private HTTPStatusClient() { }

    private GameState CurrentGameState { get; set; }
    internal GameState LastFinishedGameState { get; private set; }

    internal WebsocketClient WebSocket { get; private set; }

    internal static async Task<HTTPStatusClient> InitializeClient(Settings LoadedSettings)
    {
        var client = new HTTPStatusClient();

        var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
        {
            Options =
            {
                KeepAliveInterval = TimeSpan.FromSeconds(5)
            }
        });

        client.WebSocket = new WebsocketClient(new Uri($"ws://{LoadedSettings.BeatSaberUrl}:{LoadedSettings.BeatSaberPort}/socket"), factory)
        {
            ReconnectTimeout = null,
            ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
        };

        client.WebSocket.MessageReceived.Subscribe(msg => { client.MessageReceived(msg.Text); });
        client.WebSocket.ReconnectionHappened.Subscribe(type => { client.Reconnected(type); });
        client.WebSocket.DisconnectionHappened.Subscribe(type => { client.Disconnected(type); });

        _logger.LogInfo($"Connecting to BeatSaber via HttpStatus..");
        await client.WebSocket.Start();
        _logger.LogInfo($"Connected to BeatSaber via HttpStatus.");

        return client;
    }

    private void MessageReceived(string e)
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

                CurrentGameState.UpdateGameState(_status.status, GameEnvironment.Menu, false, false);

                try
                {
                    if (Program.LoadedSettings.OBSMenuScene != "")
                        OBSWebSocket.SetCurrentScene(Program.LoadedSettings.OBSMenuScene);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to set current scene", ex);
                    return;
                }

                _logger.LogDebug("Recveived hello object");
                break;

            case "songStart":
                _logger.LogInfo($"Started playing \"{_status.status.beatmap.songName}\" by \"{_status.status.beatmap.songAuthorName}\"");

                CurrentGameState.UpdateGameState(_status.status, GameEnvironment.Ingame, false, false);

                try
                {
                    if (Program.LoadedSettings.OBSIngameScene != "")
                        OBSWebSocket.SetCurrentScene(Program.LoadedSettings.OBSIngameScene);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to set current scene", ex);
                    return;
                }

                try
                {
                    _ = OBSWebSocket.StartRecording();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to start recording", ex);
                    return;
                }
                break;

            case "finished":
                _logger.LogInfo("Song finished.");

                CurrentGameState.UpdateGameState(_status.status, GameEnvironment.Ingame, false, true);

                try
                {
                    if (Program.LoadedSettings.OBSMenuScene != "")
                        OBSWebSocket.SetCurrentScene(Program.LoadedSettings.OBSMenuScene);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to set current scene", ex);
                    return;
                }
                break;

            case "failed":
                _logger.LogInfo("Song failed.");

                CurrentGameState.UpdateGameState(_status.status, GameEnvironment.Ingame, true, false);

                try
                {
                    if (Program.LoadedSettings.OBSMenuScene != "")
                        OBSWebSocket.SetCurrentScene(Program.LoadedSettings.OBSMenuScene);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to set current scene", ex);
                    return;
                }
                break;

            case "pause":
                _logger.LogInfo("Song paused.");

                CurrentGameState.UpdateGameState(GameEnvironment.Paused);

                try
                {
                    if (Program.LoadedSettings.PauseRecordingOnIngamePause)
                        if (Program.obsWebSocket.IsStarted)
                            OBSWebSocket.PauseRecording();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to set current scene", ex);
                    return;
                }

                try
                {
                    if (Program.LoadedSettings.OBSPauseScene != "")
                        OBSWebSocket.SetCurrentScene(Program.LoadedSettings.OBSPauseScene);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to set current scene", ex);
                    return;
                }
                break;

            case "resume":
                _logger.LogInfo("[BS-HS] Song resumed.");

                CurrentGameState.UpdateGameState(GameEnvironment.Ingame);

                try
                {
                    if (Program.LoadedSettings.PauseRecordingOnIngamePause)
                        if (Program.obsWebSocket.IsStarted)
                            OBSWebSocket.ResumeRecording();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BS-HS] {ex}");
                    return;
                }

                try
                {
                    if (Program.LoadedSettings.OBSIngameScene != "")
                        OBSWebSocket.SetCurrentScene(Program.LoadedSettings.OBSIngameScene);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to set current scene", ex);
                    return;
                }
                break;

            case "menu":
                _logger.LogDebug("[BS-HS] Menu entered.");
                _logger.LogInfo($"[BS-HS] Stopped playing \"{_status?.status?.beatmap?.songName}\" by \"{_status?.status?.beatmap?.songAuthorName}\"");

                CurrentGameState.UpdateGameState(GameEnvironment.Menu);
                LastFinishedGameState = CurrentGameState.Clone();

                _ = OBSWebSocket.StopRecording(OBSWebSocketStatus.CancelStopRecordingDelay.Token);

                try
                {
                    if (Program.LoadedSettings.OBSMenuScene != "")
                        OBSWebSocket.SetCurrentScene(Program.LoadedSettings.OBSMenuScene);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to set current scene", ex);
                    return;
                }
                break;

            case "scoreChanged":
                CurrentGameState.UpdateGameState(_status.status.performance, CurrentGameState.Performance.Failed, CurrentGameState.Performance.Finished);
                break;
        }
    }

    internal void Reconnected(ReconnectionInfo msg)
    {
        if (msg.Type != ReconnectionType.Initial)
            _logger.LogWarn($"Reconnected to BeatSaber: {msg.Type}");

        Objects.LastHttpStatusWarning = ConnectionType.Connected;
        Program.SendNotification("Connected to Beat Saber", 1000, MessageType.INFO);

        Program.BeatSaberConnectionType = ConnectionType.Connected;
    }

    internal void Disconnected(DisconnectionInfo msg)
    {
        Util.Util.CheckForMod(msg, "httpstatus", "Http Status");
    }
}
