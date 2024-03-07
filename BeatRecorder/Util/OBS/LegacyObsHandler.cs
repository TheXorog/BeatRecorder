using BeatRecorder.Entities.OBS.Legacy;
using BeatRecorder.Enums;

namespace BeatRecorder.Util.OBS;

internal class LegacyObsHandler : BaseObsHandler
{
    private LegacyObsHandler() { }

    private WebsocketClient socket { get; set; } = null;

    ConnectionTypeWarning LastWarning = ConnectionTypeWarning.Connected;

    private readonly string RequiredAuthenticationGuid = Guid.NewGuid().ToString();
    private readonly string AuthenticationGuid = Guid.NewGuid().ToString();

    private bool InitialConnectionCompleted = false;

    internal bool IsRecording { get; private set; } = false;
    internal bool IsPaused { get; private set; } = false;
    internal int RecordingSeconds { get; private set; } = 0;

    internal CancellationTokenSource StopRecordingDelayCancel = new();

    private Program Program = null;

    internal static BaseObsHandler Initialize(Program program)
    {
        _logger.LogInfo("Initializing Connection to OBS..");

        LegacyObsHandler obsHandler = new()
        {
            Program = program
        };

        var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
        {
            Options =
            {
                KeepAliveInterval = TimeSpan.FromSeconds(5)
            }
        });

        obsHandler.socket = new WebsocketClient(new Uri($"ws://{obsHandler.Program.LoadedConfig.OBSUrl}:{obsHandler.Program.LoadedConfig.OBSPortLegacy}"), factory)
        {
            ReconnectTimeout = null,
            ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
        };

        _ = obsHandler.socket.MessageReceived.Subscribe(msg => _ = obsHandler.MessageReceived(msg));
        _ = obsHandler.socket.ReconnectionHappened.Subscribe(obsHandler.Reconnected);
        _ = obsHandler.socket.DisconnectionHappened.Subscribe(obsHandler.Disconnected);

        obsHandler.socket.Start().Wait();

        while (!obsHandler.socket.IsRunning)
            Thread.Sleep(50);

        _logger.LogInfo("Connection with OBS established.");

        var message = new GetAuthRequiredRequest(obsHandler.RequiredAuthenticationGuid).Build();
        _logger.LogTrace(message);

        _ = obsHandler.socket.Send(message);

        obsHandler.InitialConnectionCompleted = true;

        return obsHandler;
    }

    internal override async Task StartRecording()
    {
        if (!this.Program.LoadedConfig.AutomaticRecording)
            return;

        if (!this.socket.IsRunning)
            throw new ArgumentException("Connection with OBS is not established.");

        if (this.IsRecording)
        {
            await this.StopRecording(true);

            while (this.IsRecording)
            {
                Thread.Sleep(20);
            }
        }

        if (this.Program.LoadedConfig.MininumWaitUntilRecordingCanStart < 200 || this.Program.LoadedConfig.MininumWaitUntilRecordingCanStart > 2000)
        {
            _logger.LogWarn("MininumWaitUntilRecordingCanStart was reset to 800. Allowed range for value is between 200 and 2000");
            this.Program.LoadedConfig.MininumWaitUntilRecordingCanStart = 800;
        }

        Thread.Sleep(this.Program.LoadedConfig.MininumWaitUntilRecordingCanStart);
        _ = this.socket.Send(new StartRecordingRequest().Build());
    }

    internal override async Task StopRecording(bool ForceStop = false)
    {
        if (!this.Program.LoadedConfig.AutomaticRecording)
            return;

        if (!this.socket.IsRunning)
            throw new ArgumentException("Connection with OBS is not established.");

        if (!ForceStop)
        {
            if (this.Program.LoadedConfig.StopRecordingDelay < 0 || this.Program.LoadedConfig.StopRecordingDelay > 20)
            {
                _logger.LogWarn("StopRecordingDelay was reset to 5. Allowed range for value is between 0 and 20");
                this.Program.LoadedConfig.StopRecordingDelay = 5;
            }

            try
            {
                var millisecondsDelay = this.Program.LoadedConfig.StopRecordingDelay;
                await Task.Delay((millisecondsDelay <= 0 ? 1 : millisecondsDelay) * 1000, this.StopRecordingDelayCancel.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
        else
        {
            this.StopRecordingDelayCancel.Cancel();
            this.StopRecordingDelayCancel = new();
        }

        _ = this.socket.Send(new StopRecordingRequest().Build());
    }

    internal override void PauseRecording() => this.socket.Send(new PauseRecordingRequest().Build());

    internal override void ResumeRecording() => this.socket.Send(new ResumeRecordingRequest().Build());

    internal override void SetCurrentScene(string scene) => this.socket.Send(new SetCurrentScene(scene).Build());

    private async Task MessageReceived(ResponseMessage msg)
    {
        _logger.LogTrace(msg.Text);
        var Message = JsonConvert.DeserializeObject<ObsResponse>(msg.Text);

        if (Message.MessageId == this.RequiredAuthenticationGuid)
        {
            var required = JsonConvert.DeserializeObject<AuthenticationRequired>(msg.Text);

            if (required.authRequired)
            {
                if (this.Program.LoadedConfig.OBSPassword.IsNullOrWhiteSpace())
                {
                    if (this.Program.LoadedConfig.DisplayUI)
                    {
                        Thread.Sleep(3000);

                        this.Program.GUI.ShowNotification("A password is required to log into your obs websocket.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        this.Program.GUI.ShowSettings(true);
                        return;
                    }

                    await Task.Delay(1000);
                    _logger.LogInfo("A password is required to log into your obs websocket.");
                    await Task.Delay(1000);
                    Console.Write("> ");

                    // I was to lazy to write my own.. https://stackoverflow.com/questions/3404421/password-masking-console-application

                    var Password = "";

                    var key = ConsoleKey.A;

                    while (key != ConsoleKey.Enter || key != ConsoleKey.Escape)
                    {
                        var keyInfo = Console.ReadKey(intercept: true);
                        key = keyInfo.Key;

                        if (key == ConsoleKey.Backspace && Password.Length > 0)
                        {
                            Console.Write("\b \b");
                            Password = Password[0..^1];
                        }
                        else if (!char.IsControl(keyInfo.KeyChar))
                        {
                            Console.Write("*");
                            Password += keyInfo.KeyChar;
                        }
                        else if (key == ConsoleKey.Escape)
                        {
                            _logger.LogError("Cancelled password input. Cannot continue without.");
                            await Task.Delay(1000);
                            Environment.Exit(0);
                            return;
                        }
                        else if (key == ConsoleKey.Enter)
                        {
                            Console.Write("\r                                              \r");
                            break;
                        }
                    }

                    if (key == ConsoleKey.Enter)
                    {
                        if (this.Program.LoadedConfig.AskToSaveOBSPassword)
                        {
                            key = ConsoleKey.A;

                            _logger.LogWarn("Do you want to save this password in the config? (THIS WILL STORE THE PASSWORD IN PLAIN-TEXT, THIS CAN BE ACCESSED BY ANYONE WITH ACCESS TO YOUR FILES. THIS IS NOT RECOMMENDED!)");
                            while (key != ConsoleKey.Enter || key != ConsoleKey.Escape || key != ConsoleKey.Y || key != ConsoleKey.N)
                            {
                                await Task.Delay(1000);
                                Console.Write("y/N > ");

                                var keyInfo = Console.ReadKey(intercept: true);
                                Console.Write("\r                                              \r");
                                key = keyInfo.Key;

                                if (key == ConsoleKey.Escape)
                                {
                                    _logger.LogWarn("Cancelled. Press any key to exit.");
                                    _ = Console.ReadKey();
                                    Environment.Exit(0);
                                    return;
                                }
                                else if (key == ConsoleKey.Y)
                                {
                                    _logger.LogInfo("Your password is now saved in the Settings.json.");
                                    this.Program.LoadedConfig.OBSPassword = Password;
                                    this.Program.LoadedConfig.AskToSaveOBSPassword = true;

                                    File.WriteAllText("Settings.json", JsonConvert.SerializeObject(this.Program.LoadedConfig, Formatting.Indented));
                                    break;
                                }
                                else if (key == ConsoleKey.N || key == ConsoleKey.Enter)
                                {
                                    _logger.LogInfo("Your password will not be saved. This wont be asked in the future.");
                                    _logger.LogInfo("To re-enable this prompt, set AskToSaveOBSPassword to true in the Settings.json.");
                                    this.Program.LoadedConfig.OBSPassword = "";
                                    this.Program.LoadedConfig.AskToSaveOBSPassword = false;

                                    File.WriteAllText("Settings.json", JsonConvert.SerializeObject(this.Program.LoadedConfig, Formatting.Indented));
                                    break;
                                }
                            }
                        }

                        this.Program.LoadedConfig.OBSPassword = Password;
                    }
                }

                _logger.LogInfo("Connection with OBS requires authentication. Attempting log in..");

                var secret = Extensions.HashEncode(this.Program.LoadedConfig.OBSPassword + required.salt);
                var authResponse = Extensions.HashEncode(secret + required.challenge);

                _ = this.socket.Send(new AuthenticateRequest(authResponse, this.AuthenticationGuid).Build());
            }
        }
        else if (Message.MessageId == this.AuthenticationGuid)
        {
            if (Message.Status == "ok")
            {
                _logger.LogInfo("Authentication with OBS successful.");
                this.Program.steamNotifications?.SendNotification("Connected to OBS", 1000, MessageType.INFO);
            }
            else
            {
                _logger.LogError("Failed to authenticate with OBS. Please check your password or wait a few seconds to automatically retry authenticating.");
                _ = await this.socket.Stop(WebSocketCloseStatus.NormalClosure, "Shutting down");

                await Task.Delay(2000);
                _logger.LogInfo("Re-trying authentication with OBS..");
                await this.socket.Start();

                var message = new GetAuthRequiredRequest(this.RequiredAuthenticationGuid).Build();
                _logger.LogTrace(message);
            }
        }
        else
        {
            _logger.LogTrace($"Received unknown message id: {Message.MessageId}");
        }

        if (Message.UpdateType is null)
            return;

        if (Message.UpdateType == "RecordingStarted")
        {
            this.Program.steamNotifications?.SendNotification("Recording started", 1000, MessageType.INFO);

            this.IsRecording = true;

            _logger.LogInfo("Recording started.");

            while (this.IsRecording)
            {
                if (!this.IsPaused)
                    this.RecordingSeconds++;

                await Task.Delay(1000);
            }
            await Task.Delay(2000);
            this.RecordingSeconds = 0;
        }
        else if (Message.UpdateType == "RecordingStopped")
        {
            this.Program.steamNotifications?.SendNotification("Recording stopped", 1000, MessageType.INFO);

            this.IsRecording = false;
            this.IsPaused = false;

            _logger.LogInfo("Recording stopped.");

            var RecordingStopped = JsonConvert.DeserializeObject<RecordingStopped>(msg.Text);
            this.Program.BeatSaberClient.HandleFile(RecordingStopped.recordingFilename, this.RecordingSeconds, this.Program.BeatSaberClient.GetLastCompletedStatus(), this.Program);
        }
        else if (Message.UpdateType == "RecordingPaused")
        {
            this.Program.steamNotifications?.SendNotification("Recording paused", 1000, MessageType.INFO);

            this.IsPaused = true;

            _logger.LogInfo("Recording paused.");
        }
        else if (Message.UpdateType == "RecordingResumed")
        {
            this.Program.steamNotifications?.SendNotification("Recording resumed", 1000, MessageType.INFO);

            this.IsPaused = false;

            _logger.LogInfo("Recording resumed.");
        }
        else
        {
            _logger.LogTrace($"Received unknown update type: {Message.MessageId}");
        }
    }

    private void Reconnected(ReconnectionInfo msg)
    {
        if (msg.Type != ReconnectionType.Initial)
        {
            if (this.InitialConnectionCompleted)
            {
                var message = new GetAuthRequiredRequest(this.RequiredAuthenticationGuid).Build();
                _logger.LogTrace(message);

                _ = this.socket.Send(message);
            }
        }

        this.LastWarning = ConnectionTypeWarning.Connected;
    }

    private void Disconnected(DisconnectionInfo msg)
    {
        this.Program.steamNotifications?.SendNotification("Disconnected from OBS", 1000, MessageType.ERROR);

        try
        {
            var processCollection = Process.GetProcesses();

            if (!processCollection.Any(x => x.ProcessName.StartsWith("obs64", StringComparison.CurrentCultureIgnoreCase) || x.ProcessName.StartsWith("obs32", StringComparison.CurrentCultureIgnoreCase)))
            {
                if (this.LastWarning != ConnectionTypeWarning.NoProcess)
                {
                    _logger.LogWarn($"Couldn't find an OBS process, is your OBS running? ({msg.Type})");
                }
                this.LastWarning = ConnectionTypeWarning.NoProcess;
            }
            else
            {
                var FoundWebSocketDll = false;

                var OBSInstallationDirectory = processCollection.First(x => x.ProcessName.StartsWith("obs64", StringComparison.CurrentCultureIgnoreCase) || x.ProcessName.StartsWith("obs32", StringComparison.CurrentCultureIgnoreCase)).MainModule.FileName;
                OBSInstallationDirectory = OBSInstallationDirectory.Remove(OBSInstallationDirectory.LastIndexOf("\\bin"), OBSInstallationDirectory.Length - OBSInstallationDirectory.LastIndexOf("\\bin"));

                if (Directory.GetDirectories(OBSInstallationDirectory).Any(x => x.ToLower().EndsWith("obs-plugins")))
                {
                    foreach (var b in Directory.GetDirectories($"{OBSInstallationDirectory}\\obs-plugins"))
                    {
                        if (Directory.GetFiles(b).Any(x => x.Contains("obs-websocket") && x.EndsWith(".dll")))
                        {
                            FoundWebSocketDll = true;
                            break;
                        }
                    }
                }

                if (FoundWebSocketDll)
                {
                    if (this.LastWarning != ConnectionTypeWarning.ModInstalled)
                    {
                        _logger.LogFatal($"OBS seems to be running but the obs-websocket server isn't running. Please make sure you have the obs-websocket server activated! (Tools -> WebSocket Server Settings) ({msg.Type})");
                    }
                    this.LastWarning = ConnectionTypeWarning.ModInstalled;
                }
                else
                {
                    if (this.LastWarning != ConnectionTypeWarning.ModNotInstalled)
                    {
                        _logger.LogFatal($"OBS seems to be running but the obs-websocket server isn't installed. Please make sure you have the obs-websocket server installed! (To install, follow this link: https://bit.ly/3BCXfeS) ({msg.Type})");
                    }
                    this.LastWarning = ConnectionTypeWarning.ModNotInstalled;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check if obs-websocket is installed: (Disconnect Reason: {msg.Type}) {ex}");
        }
    }

    internal override bool GetIsRunning() => this.socket?.IsRunning ?? false;

    internal override bool GetIsRecording() => this.IsRecording;

    internal override bool GetIsPaused() => this.IsPaused;

    internal override int GetRecordingSeconds() => this.RecordingSeconds;
}
