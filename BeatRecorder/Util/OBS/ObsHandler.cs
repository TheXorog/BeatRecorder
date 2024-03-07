using BeatRecorder.Entities.OBS;
using BeatRecorder.Enums;
using Newtonsoft.Json.Linq;

namespace BeatRecorder.Util.OBS;

internal class ObsHandler : BaseObsHandler
{
    private WebsocketClient socket { get; set; } = null;

    ConnectionTypeWarning LastWarning = ConnectionTypeWarning.Connected;

    internal bool IsRecording { get; private set; } = false;
    internal bool IsPaused { get; private set; } = false;
    internal int RecordingSeconds { get; private set; } = 0;

    internal CancellationTokenSource StopRecordingDelayCancel = new();

    private Program Program = null;

    bool AttemptedToIdentify = false;

    internal static BaseObsHandler Initialize(Program program)
    {
        _logger.LogInfo("Initializing Connection to OBS..");

        ObsHandler obsHandler = new()
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

        obsHandler.socket = new WebsocketClient(new Uri($"ws://{obsHandler.Program.LoadedConfig.OBSUrl}:{obsHandler.Program.LoadedConfig.OBSPortModern}"), factory)
        {
            ReconnectTimeout = null,
            ErrorReconnectTimeout = TimeSpan.FromSeconds(3),
            IsReconnectionEnabled = false,
        };

        _ = obsHandler.socket.MessageReceived.Subscribe(msg => _ = obsHandler.MessageReceived(msg));
        _ = obsHandler.socket.ReconnectionHappened.Subscribe(obsHandler.Reconnected);
        _ = obsHandler.socket.DisconnectionHappened.Subscribe(obsHandler.Disconnected);

        obsHandler.socket.Start().Wait();

        return obsHandler;
    }

    private async Task MessageReceived(ResponseMessage msg)
    {
        _logger.LogTrace(msg.Text);

        var obsResponse = JsonConvert.DeserializeObject<ObsResponse>(msg.Text);

        switch (obsResponse.op)
        {
            case 0:
            {
                var required = JsonConvert.DeserializeObject<Hello>(msg.Text);

                if (required.d.authentication is not null)
                {
                    if (this.Program.LoadedConfig.OBSPassword.IsNullOrWhiteSpace())
                    {
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

                    _logger.LogInfo("Connection with OBS requires authentication. Identifying..");

                    var secret = Extensions.HashEncode(this.Program.LoadedConfig.OBSPassword + required.d.authentication.salt);
                    var authResponse = Extensions.HashEncode(secret + required.d.authentication.challenge);

                    this.AttemptedToIdentify = true;
                    _ = this.socket.Send(new Indentify(authResponse).Build());
                }
                else
                {
                    _logger.LogInfo("Connection with OBS does not require authentication. Identifying..");

                    this.AttemptedToIdentify = true;
                    _ = this.socket.Send(new Indentify().Build());
                }

                break;
            }
            case 2:
            {
                var indentified = JsonConvert.DeserializeObject<Indentified>(msg.Text);
                if (indentified.d.negotiatedRpcVersion != 1)
                    _logger.LogWarn("Negotiated Rpc Version does not match 1. Please expect possible bugs.");

                _logger.LogInfo("Successfully identified to websocket.");
                this.AttemptedToIdentify = false;
                break;
            }
            case 5:
            {
                var eventType = JsonConvert.DeserializeObject<EventType>(msg.Text);

                switch (eventType.d.eventType)
                {
                    case "RecordStateChanged":
                    {
                        var recordStateChanged = JsonConvert.DeserializeObject<RecordStateChanged>(msg.Text);

                        switch (recordStateChanged.d.eventData.outputState)
                        {
                            case "OBS_WEBSOCKET_OUTPUT_STARTED":
                            {
                                IsRecording = true;

                                _logger.LogInfo("Recording started.");

                                while (this.IsRecording)
                                {
                                    if (!this.IsPaused)
                                        this.RecordingSeconds++;

                                    await Task.Delay(1000);
                                }
                                await Task.Delay(2000);
                                this.RecordingSeconds = 0;
                                break;
                            }
                            case "OBS_WEBSOCKET_OUTPUT_STOPPED":
                            {
                                IsRecording = false;
                                IsPaused = false;

                                _logger.LogInfo("Recording stopped.");

                                this.Program.BeatSaberClient.HandleFile(recordStateChanged.d.eventData.outputPath, this.RecordingSeconds, this.Program.BeatSaberClient.GetLastCompletedStatus(), this.Program);
                                break;
                            }
                            case "OBS_WEBSOCKET_OUTPUT_PAUSED":
                            {
                                IsPaused = true;

                                _logger.LogInfo("Recording paused.");
                                break;
                            }
                            case "OBS_WEBSOCKET_OUTPUT_RESUMED":
                            {
                                IsPaused = false;

                                _logger.LogInfo("Recording resumed.");
                                break;
                            }
                        }

                        break;
                    }
                }
                break;
            }
        }
    }

    private void Reconnected(ReconnectionInfo msg) => this.LastWarning = ConnectionTypeWarning.Connected;

    private void Disconnected(DisconnectionInfo msg)
    {
        try
        {
            _ = Task.Delay(2000).ContinueWith(_ => _ = this.socket.Start());

            if (this.AttemptedToIdentify)
            {
                _logger.LogWarn("Failed to identify with websocket. Your password might be incorrect, retrying in 2 seconds..");
                return;
            }

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

                var OBSInstallationDirectory = processCollection.First(x => x.ProcessName.ToLower().StartsWith("obs64") || x.ProcessName.ToLower().StartsWith("obs32")).MainModule.FileName.Replace("\\", "/");
                OBSInstallationDirectory = OBSInstallationDirectory.Remove(OBSInstallationDirectory.LastIndexOf("/bin"), OBSInstallationDirectory.Length - OBSInstallationDirectory.LastIndexOf("/bin"));

                if (Directory.GetDirectories(OBSInstallationDirectory).Any(x => x.ToLower().EndsWith("obs-plugins")))
                {
                    foreach (var b in Directory.GetDirectories($"{OBSInstallationDirectory}/obs-plugins"))
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
        _ = this.socket.Send(new StartRecord().Build());
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

        _ = this.socket.Send(new StopRecord().Build());
    }

    internal override void PauseRecording() => this.socket.Send(new PauseRecord().Build());

    internal override void ResumeRecording() => this.socket.Send(new ResumeRecord().Build());

    internal override void SetCurrentScene(string scene) => this.socket.Send(new SetCurrentProgramScene(scene).Build());

    internal override bool GetIsRunning() => this.socket?.IsRunning ?? false;

    internal override bool GetIsRecording() => this.IsRecording;

    internal override bool GetIsPaused() => this.IsPaused;

    internal override int GetRecordingSeconds() => this.RecordingSeconds;
}
