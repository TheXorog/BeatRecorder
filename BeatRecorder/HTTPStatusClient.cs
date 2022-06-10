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

        LogInfo($"Connecting to BeatSaber via HttpStatus..");
        await WebSocket.Start();

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
            LogFatal($"Unable to parse the HttpStatus Event", ex);
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
                    LogError($"[BS-HS] {ex}");
                    return;
                }

                LogInfo("[BS-HS] Connected.");
                break;

            case "songStart":
                LogDebug("[BS-HS] Song started.");
                LogInfo($"[BS-HS] Started playing \"{_status.status.beatmap.songName}\" by \"{_status.status.beatmap.songAuthorName}\"");

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
                    LogError($"[BS-HS] {ex}");
                    return;
                }

                try
                {
                    _ = OBSWebSocket.StartRecording();
                }
                catch (Exception ex)
                {
                    LogError($"[BS-HS] {ex}");
                    return;
                }
                break;

            case "finished":
                LogInfo("[BS-HS] Song finished.");

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
                    LogError($"[BS-HS] {ex}");
                    return;
                }
                break;

            case "failed":
                LogInfo("[BS-HS] Song failed.");

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
                    LogError($"[BS-HS] {ex}");
                    return;
                }
                break;

            case "pause":
                LogInfo("[BS-HS] Song paused.");

                try
                {
                    if (Program.LoadedSettings.PauseRecordingOnIngamePause)
                        if (Program.obsWebSocket.IsStarted)
                            Program.obsWebSocket.Send($"{{\"request-type\":\"PauseRecording\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    LogError($"[BS-HS] {ex}");
                    return;
                }

                try
                {
                    if (Program.LoadedSettings.OBSPauseScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSPauseScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    LogError($"[BS-HS] {ex}");
                    return;
                }
                break;

            case "resume":
                LogInfo("[BS-HS] Song resumed.");

                try
                {
                    if (Program.LoadedSettings.PauseRecordingOnIngamePause)
                        if (Program.obsWebSocket.IsStarted)
                            Program.obsWebSocket.Send($"{{\"request-type\":\"ResumeRecording\", \"message-id\":\"ResumeRecording\"}}");
                }
                catch (Exception ex)
                {
                    LogError($"[BS-HS] {ex}");
                    return;
                }

                try
                {
                    if (Program.LoadedSettings.OBSIngameScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSIngameScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    LogError($"[BS-HS] {ex}");
                    return;
                }
                break;

            case "menu":
                LogDebug("[BS-HS] Menu entered.");
                LogInfo($"[BS-HS] Stopped playing \"{_status?.status?.beatmap?.songName}\" by \"{_status?.status?.beatmap?.songAuthorName}\"");

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
                    LogError($"[BS-HS] {ex}");
                    return;
                }

                try
                {
                    if (Program.LoadedSettings.OBSMenuScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSMenuScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    LogError($"[BS-HS] {ex}");
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
            LogWarn($"[BS-HS] Reconnected: {msg.Type}");

        Objects.LastHttpStatusWarning = ConnectionTypeWarning.CONNECTED;
        Program.SendNotification("Connected to Beat Saber", 1000, MessageType.INFO);
    }

    internal static void Disconnected(DisconnectionInfo msg)
    {
        try
        {
            Process[] processCollection = Process.GetProcesses();

            if (!processCollection.Any(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")))
            {
                if (Objects.LastHttpStatusWarning != ConnectionTypeWarning.NO_PROCESS)
                {
                    LogWarn($"[BS-HS] Couldn't find a BeatSaber process, is BeatSaber started? ({msg.Type})");
                    Program.SendNotification("Couldn't connect to BeatSaber, is it even running?", 5000, MessageType.ERROR);
                }
                Objects.LastHttpStatusWarning = ConnectionTypeWarning.NO_PROCESS;
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
                    if (Objects.LastHttpStatusWarning != ConnectionTypeWarning.NOT_MODDED)
                    {
                        LogFatal($"[BS-HS] Beat Saber seems to be running but the beatsaber-http-status modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install beatsaber-http-status: https://bit.ly/3wYX3Dd) ({msg.Type})");
                        Program.SendNotification("Couldn't connect to Beat Saber. Have you modded your game?", 10000, MessageType.ERROR);
                    }
                    Objects.LastHttpStatusWarning = ConnectionTypeWarning.NOT_MODDED;
                }

                if (FoundWebSocketDll)
                {
                    if (Objects.LastHttpStatusWarning != ConnectionTypeWarning.MOD_INSTALLED)
                    {
                        LogFatal($"[BS-HS] Beat Saber seems to be running and the beatsaber-http-status modifaction seems to be installed. Please make sure you put in the right port and you installed all of beatsaber-http-status' dependiencies! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                        Program.SendNotification("Couldn't connect to Beat Saber. Please make sure you selected the right port.", 10000, MessageType.ERROR);
                    }
                    Objects.LastHttpStatusWarning = ConnectionTypeWarning.MOD_INSTALLED;
                }
                else
                {
                    if (Objects.LastHttpStatusWarning != ConnectionTypeWarning.MOD_NOT_INSTALLED)
                    {
                        LogFatal($"[BS-HS] Beat Saber seems to be running but the beatsaber-http-status modifaction doesn't seem to be installed. Please make sure to install beatsaber-http-status! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                        Program.SendNotification("Couldn't connect to Beat Saber. Please make sure DataPuller is installed.", 10000, MessageType.ERROR);
                    }
                    Objects.LastHttpStatusWarning = ConnectionTypeWarning.MOD_NOT_INSTALLED;
                }
            }
        }
        catch (Exception ex)
        {
            LogError($"[BS-HS] Failed to check if beatsaber-http-status is installed: (Disconnect Reason: {msg.Type}) {ex}");
        }
    }
}
