using BeatRecorder.Entities;
using BeatRecorder.Entities.OBS;
using BeatRecorder.Enums;

namespace BeatRecorder.Util;

internal class ObsHandler
{
    private ObsHandler() { }

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

    internal static ObsHandler Initialize(Program program)
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

        obsHandler.socket = new WebsocketClient(new Uri($"ws://{obsHandler.Program.status.LoadedConfig.OBSUrl}:{obsHandler.Program.status.LoadedConfig.OBSPort}"), factory)
        {
            ReconnectTimeout = null,
            ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
        };

        obsHandler.socket.MessageReceived.Subscribe(msg => { _ = obsHandler.MessageReceived(msg); });
        obsHandler.socket.ReconnectionHappened.Subscribe(type => { obsHandler.Reconnected(type); });
        obsHandler.socket.DisconnectionHappened.Subscribe(type => { obsHandler.Disconnected(type); });

        obsHandler.socket.Start().Wait();

        while (!obsHandler.socket.IsRunning)
            Thread.Sleep(50);

        _logger.LogInfo("Connection with OBS established.");

        var message = new GetAuthRequiredRequest(obsHandler.RequiredAuthenticationGuid).Build();
        _logger.LogTrace(message);

        obsHandler.socket.Send(message);

        obsHandler.InitialConnectionCompleted = true;

        return obsHandler;
    }

    internal async Task StartRecording()
    {
        if (!Program.status.LoadedConfig.AutomaticRecording)
            return;

        if (!socket.IsRunning)
            throw new ArgumentException("Connection with OBS is not established.");

        if (IsRecording)
        {
            await StopRecording(true);

            while (IsRecording)
            {
                Thread.Sleep(20);
            }
        }

        if (Program.status.LoadedConfig.MininumWaitUntilRecordingCanStart < 200 || Program.status.LoadedConfig.MininumWaitUntilRecordingCanStart > 2000)
        {
            _logger.LogWarn("MininumWaitUntilRecordingCanStart was reset to 800. Allowed range for value is between 200 and 2000");
            Program.status.LoadedConfig.MininumWaitUntilRecordingCanStart = 800;
        }

        Thread.Sleep(Program.status.LoadedConfig.MininumWaitUntilRecordingCanStart);
        socket.Send(new StartRecordingRequest().Build());
    }

    internal async Task StopRecording(bool ForceStop = false)
    {
        if (!Program.status.LoadedConfig.AutomaticRecording)
            return;

        if (!socket.IsRunning)
            throw new ArgumentException("Connection with OBS is not established.");

        if (!ForceStop)
        {
            if (Program.status.LoadedConfig.StopRecordingDelay < 0 || Program.status.LoadedConfig.StopRecordingDelay > 20)
            {
                _logger.LogWarn("StopRecordingDelay was reset to 5. Allowed range for value is between 0 and 20");
                Program.status.LoadedConfig.StopRecordingDelay = 5;
            }

            try
            {
                var millisecondsDelay = Program.status.LoadedConfig.StopRecordingDelay;
                await Task.Delay((millisecondsDelay <= 0 ? 1 : millisecondsDelay) * 1000, this.StopRecordingDelayCancel.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
        else
        {
            StopRecordingDelayCancel.Cancel();
            StopRecordingDelayCancel = new();
        }

        socket.Send(new StopRecordingRequest().Build());
    }

    internal void PauseRecording()
    {
        socket.Send(new PauseRecordingRequest().Build());
    }
    
    internal void ResumeRecording()
    {
        socket.Send(new ResumeRecordingRequest().Build());
    }
    
    internal void SetCurrentScene(string scene)
    {
        socket.Send(new SetCurrentScene(scene).Build());
    }

    private async Task MessageReceived(ResponseMessage msg)
    {
        _logger.LogTrace(msg.Text);
        var Message = JsonConvert.DeserializeObject<ObsResponse>(msg.Text);

        if (Message.MessageId == RequiredAuthenticationGuid)
        {
            AuthenticationRequired required = JsonConvert.DeserializeObject<AuthenticationRequired>(msg.Text);

            if (required.authRequired)
            {
                if (Program.status.LoadedConfig.OBSPassword.IsNullOrWhiteSpace())
                {
                    await Task.Delay(1000);
                    _logger.LogInfo("A password is required to log into your obs websocket.");
                    await Task.Delay(1000);
                    Console.Write("> ");

                    // I was to lazy to write my own.. https://stackoverflow.com/questions/3404421/password-masking-console-application

                    string Password = "";

                    ConsoleKey key = ConsoleKey.A;

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
                        if (Program.status.LoadedConfig.AskToSaveOBSPassword)
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
                                    Console.ReadKey();
                                    Environment.Exit(0);
                                    return;
                                }
                                else if (key == ConsoleKey.Y)
                                {
                                    _logger.LogInfo("Your password is now saved in the Settings.json.");
                                    Program.status.LoadedConfig.OBSPassword = Password;
                                    Program.status.LoadedConfig.AskToSaveOBSPassword = true;

                                    File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Program.status.LoadedConfig, Formatting.Indented));
                                    break;
                                }
                                else if (key == ConsoleKey.N || key == ConsoleKey.Enter)
                                {
                                    _logger.LogInfo("Your password will not be saved. This wont be asked in the feature.");
                                    _logger.LogInfo("To re-enable this prompt, set AskToSaveOBSPassword to true in the Settings.json.");
                                    Program.status.LoadedConfig.OBSPassword = "";
                                    Program.status.LoadedConfig.AskToSaveOBSPassword = false;

                                    File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Program.status.LoadedConfig, Formatting.Indented));
                                    break;
                                }
                            }
                        }

                        Program.status.LoadedConfig.OBSPassword = Password;
                    }
                }

                _logger.LogInfo("Connection with OBS requires authentication. Attempting log in..");

                string secret = Extensions.HashEncode(Program.status.LoadedConfig.OBSPassword + required.salt);
                string authResponse = Extensions.HashEncode(secret + required.challenge);

                socket.Send(new AuthenticateRequest(authResponse, AuthenticationGuid).Build());
            }
        }
        else if (Message.MessageId == AuthenticationGuid)
        {
            if (Message.Status == "ok")
            {
                _logger.LogInfo("Authentication with OBS successful.");
            }
            else
            {
                _logger.LogError("Failed to authenticate with OBS. Please check your password or wait a few seconds to automatically retry authenticating.");
                await socket.Stop(WebSocketCloseStatus.NormalClosure, "Shutting down");

                await Task.Delay(2000);
                _logger.LogInfo("Re-trying authentication with OBS..");
                await socket.Start();

                var message = new GetAuthRequiredRequest(RequiredAuthenticationGuid).Build();
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
            IsRecording = true;

            _logger.LogInfo("Recording started.");

            while (IsRecording)
            {
                if (!IsPaused)
                    RecordingSeconds++;

                await Task.Delay(1000);
            }
            await Task.Delay(2000);
            RecordingSeconds = 0;
        }
        else if (Message.UpdateType == "RecordingStopped")
        {
            IsRecording = false;
            IsPaused = false;

            _logger.LogInfo("Recording stopped.");

            RecordingStopped RecordingStopped = JsonConvert.DeserializeObject<RecordingStopped>(msg.Text);
            Program.BeatSaberClient.HandleFile(RecordingStopped.recordingFilename, RecordingSeconds, Program.BeatSaberClient.GetLastCompletedStatus(), Program.status.LoadedConfig);
        }
        else if (Message.UpdateType == "RecordingPaused")
        {
            IsPaused = true;

            _logger.LogInfo("Recording paused.");
        }
        else if (Message.UpdateType == "RecordingResumed")
        {
            IsPaused = false;

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
            if (InitialConnectionCompleted)
            {
                var message = new GetAuthRequiredRequest(RequiredAuthenticationGuid).Build();
                _logger.LogTrace(message);

                socket.Send(message);
            }
        }

        LastWarning = ConnectionTypeWarning.Connected;
    }

    private void Disconnected(DisconnectionInfo msg)
    {
        try
        {
            Process[] processCollection = Process.GetProcesses();

            if (!processCollection.Any(x => x.ProcessName.ToLower().StartsWith("obs64") || x.ProcessName.ToLower().StartsWith("obs32")))
            {
                if (LastWarning != ConnectionTypeWarning.NoProcess)
                {
                    _logger.LogWarn($"Couldn't find an OBS process, is your OBS running? ({msg.Type})");
                }
                LastWarning = ConnectionTypeWarning.NoProcess;
            }
            else
            {
                bool FoundWebSocketDll = false;

                string OBSInstallationDirectory = processCollection.First(x => x.ProcessName.ToLower().StartsWith("obs64") || x.ProcessName.ToLower().StartsWith("obs32")).MainModule.FileName;
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
                    if (LastWarning != ConnectionTypeWarning.ModInstalled)
                    {
                        _logger.LogFatal($"OBS seems to be running but the obs-websocket server isn't running. Please make sure you have the obs-websocket server activated! (Tools -> WebSocket Server Settings) ({msg.Type})");
                    }
                    LastWarning = ConnectionTypeWarning.ModInstalled;
                }
                else
                {
                    if (LastWarning != ConnectionTypeWarning.ModNotInstalled)
                    {
                        _logger.LogFatal($"OBS seems to be running but the obs-websocket server isn't installed. Please make sure you have the obs-websocket server installed! (To install, follow this link: https://bit.ly/3BCXfeS) ({msg.Type})");
                    }
                    LastWarning = ConnectionTypeWarning.ModNotInstalled;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check if obs-websocket is installed: (Disconnect Reason: {msg.Type}) {ex}");
        }
    }
}
