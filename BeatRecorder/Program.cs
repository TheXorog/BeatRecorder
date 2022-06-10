namespace BeatRecorder;

class Program
{
    public static string CurrentVersion = "1.6";

    public static Settings LoadedSettings = new();

    internal static WebsocketClient beatSaberWebSocket { get; set; }
    internal static WebsocketClient beatSaberWebSocketLiveData { get; set; }
    internal static WebsocketClient obsWebSocket { get; set; }

    static void Main(string[] args)
    {
        Program program = new();

        program.MainAsync(args).GetAwaiter().GetResult();
    }

    private async Task MainAsync(string[] args)
    {
        Console.Clear();

        Console.SetWindowSize(160, 40);

        if (!Directory.Exists("logs"))
            Directory.CreateDirectory("logs");

        StartLogger($"logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", LogLevel.DEBUG, DateTime.UtcNow.AddDays(-3), false);

        LogInfo("[BR] Loading settings..");

        _ = Task.Run(async () =>
        {
            await Task.Delay(30000);

            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                LogError("Only one instance of this application is allowed");
                Environment.Exit(0);
                return;
            }
        });
        
        if (File.Exists("Settings.json"))
        {
            try
            {
                LoadedSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("Settings.json"));
                File.WriteAllText("Settings.json", JsonConvert.SerializeObject(LoadedSettings, Formatting.Indented));

                ChangeLogLevel(LoadedSettings.ConsoleLogLevel);

                if (LoadedSettings.Mod != "http-status" && LoadedSettings.Mod != "datapuller")
                {
                    throw new Exception("Invalid Mod selected.");
                }

                if (!string.IsNullOrWhiteSpace(LoadedSettings.OBSPassword))
                    AddBlacklist(LoadedSettings.OBSPassword);
            }
            catch (Exception ex)
            {
                LogError($"[BR] Exception occured while loading config: {ex}");
                ResetSettings();
                return;
            }

            LogInfo("[BR] Settings loaded.");
        }
        else
        {
            ResetSettings();
            return;
        }

        LogDebug($"Enviroment Details\n\n" +
                $"Dotnet Version: {Environment.Version}\n" +
                $"OS & Version: {Environment.OSVersion}\n\n" +
                $"OS 64x: {Environment.Is64BitOperatingSystem}\n" +
                $"Process 64x: {Environment.Is64BitProcess}\n\n" +
                $"Current Directory: {Environment.CurrentDirectory}\n" +
                $"Commandline: {Environment.CommandLine}\n");

        LogDebug($"Loaded settings:\n\n{JsonConvert.SerializeObject(LoadedSettings, Formatting.Indented)}\n");
        LogDebug($"{AppDomain.CurrentDomain.BaseDirectory}");
        LogDebug($"{Environment.CurrentDirectory}");

        OBSWebSocketStatus.CancelStopRecordingDelay = new CancellationTokenSource();

        await Task.Run(async () =>
        {
            try
            {
                var github = new GitHubClient(new ProductHeaderValue("BeatRecorderUpdateCheck"));
                var repo = await github.Repository.Release.GetLatest("TheXorog", "BeatRecorder");

                LogInfo($"[BR] Current latest release is \"{repo.TagName}\". You're currently running: \"{CurrentVersion}\"");

                if (repo.TagName != CurrentVersion)
                {
                    LogFatal($"[BR] You're running an outdated version of BeatRecorder, please update at https://github.com/TheXorog/BeatRecorder/releases/latest." +
                            $"\n\nWhat changed in the new version:\n\n" +
                            $"{repo.Body}\n");

                    Objects.UpdateText = repo.Body;
                    Objects.UpdateAvailable = true;
                }
            }
            catch (Exception ex)
            {
                LogError($"[BR] Unable to get latest version: {ex}");
            }
        });

        switch (LoadedSettings.Mod)
        {
            case "datapuller":
            {
                // Connect to MapData Endpoint of DataPuller's WebSocket

                _ = Task.Run(() =>
                {
                    var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
                    {
                        Options =
                            {
                            KeepAliveInterval = TimeSpan.FromSeconds(5)
                            }
                    });

                    beatSaberWebSocket = new WebsocketClient(new Uri($"ws://{LoadedSettings.BeatSaberUrl}:{LoadedSettings.BeatSaberPort}/BSDataPuller/MapData"), factory)
                    {
                        ReconnectTimeout = null,
                        ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
                    };

                    beatSaberWebSocket.MessageReceived.Subscribe(msg => { DataPuller.MapDataMessageRecieved(msg.Text); });
                    beatSaberWebSocket.ReconnectionHappened.Subscribe(type => { DataPuller.MapDataReconnected(type); });
                    beatSaberWebSocket.DisconnectionHappened.Subscribe(type => { DataPuller.MapDataDisconnected(type); });

                    LogInfo($"[BS-DP1] Connecting..");
                    beatSaberWebSocket.Start().Wait();
                    LogInfo("[BS-DP1] Connected.");
                });

                // Connect to LiveData Endpoint of DataPuller's WebSocket or HttpStatus' Endpoint

                _ = Task.Run(() =>
                    {
                    // https://github.com/kOFReadie/BSDataPuller

                        var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
                        {
                            Options =
                                    {
                                    KeepAliveInterval = TimeSpan.FromSeconds(5)
                                    }
                        });

                        beatSaberWebSocketLiveData = new WebsocketClient(new Uri($"ws://{LoadedSettings.BeatSaberUrl}:{LoadedSettings.BeatSaberPort}/BSDataPuller/LiveData"), factory)
                        {
                            ReconnectTimeout = null,
                            ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
                        };

                        beatSaberWebSocketLiveData.MessageReceived.Subscribe(msg => { DataPuller.LiveDataMessageRecieved(msg.Text); });
                        beatSaberWebSocketLiveData.ReconnectionHappened.Subscribe(type => { DataPuller.LiveDataReconnected(type); });
                        beatSaberWebSocketLiveData.DisconnectionHappened.Subscribe(type => { DataPuller.LiveDataDisconnected(type); });

                        LogDebug($"[BS-DP2] Connecting..");
                        beatSaberWebSocketLiveData.Start().Wait();
                        LogDebug("[BS-DP2] Connected.");
                    });
                break;
            }

            case "http-status":
            {
                _ = Task.Run(() =>
                {
                // https://github.com/opl-/beatsaber-http-status/blob/master/protocol.md
                // https://github.com/kOFReadie/BSDataPuller

                    var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
                    {
                        Options =
                            {
                            KeepAliveInterval = TimeSpan.FromSeconds(5)
                            }
                    });

                    beatSaberWebSocket = new WebsocketClient(new Uri($"ws://{LoadedSettings.BeatSaberUrl}:{LoadedSettings.BeatSaberPort}/socket"), factory)
                    {
                        ReconnectTimeout = null,
                        ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
                    };
                    beatSaberWebSocket.MessageReceived.Subscribe(msg => { HttpStatus.MessageReceived(msg.Text); });
                    beatSaberWebSocket.ReconnectionHappened.Subscribe(type => { HttpStatus.Reconnected(type); });
                    beatSaberWebSocket.DisconnectionHappened.Subscribe(type => { HttpStatus.Disconnected(type); });

                    LogInfo($"[BS-HS] Connecting..");
                    beatSaberWebSocket.Start().Wait();
                });
                break;
            }
        }

        _ = Task.Run(() =>
        {
            // https://github.com/Palakis/obs-websocket/blob/4.x-current/docs/generated/protocol.md

            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options =
                        {
                            KeepAliveInterval = TimeSpan.FromSeconds(5)
                        }
            });

            obsWebSocket = new WebsocketClient(new Uri($"ws://{LoadedSettings.OBSUrl}:{LoadedSettings.OBSPort}"), factory)
            {
                ReconnectTimeout = null,
                ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
            };

            obsWebSocket.MessageReceived.Subscribe(msg => { _ = OBSWebSocketEvents.MessageReceived(msg); });
            obsWebSocket.ReconnectionHappened.Subscribe(type => { OBSWebSocketEvents.Reconnected(type); });
            obsWebSocket.DisconnectionHappened.Subscribe(type => { OBSWebSocketEvents.Disconnected(type); });

            LogInfo($"[OBS] Connecting..");
            obsWebSocket.Start().Wait();

            obsWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{OBSWebSocketEvents.RequiredAuthenticationGuid}\"}}");

            LogInfo($"[OBS] Connected.");

            SendNotification("Connected to OBS", 1000, MessageType.INFO);
        });

        if (LoadedSettings.DisplayUI)
        {
            UIHandler handler = new();
            _ = handler.HandleUI();
        }

        NotifcationLoop();

        // Don't close the application
        await Task.Delay(-1);
    }

    private static void ResetSettings()
    {
        LoadedSettings = new();

        if (File.Exists("Settings.json"))
        {
            try
            {
                if (File.Exists("Settings.json.old"))
                    File.Delete("Settings.json.old");

                File.Copy("Settings.json", "Settings.json.old");
            }
            catch { }
        }

        File.WriteAllText("Settings.json", JsonConvert.SerializeObject(LoadedSettings, Formatting.Indented));

        SendNotification("Your settings were reset due to an error. Please check your desktop.", 10000, MessageType.ERROR);
        LogInfo($"Please configure BeatRecorder using the config file that was just opened. If you're done, save and quit notepad and BeatRecorder will restart for you.");

        Objects.SettingsRequired = true;
        var infoUI = new InfoUI(CurrentVersion, LoadedSettings.DisplayUITopmost, Objects.SettingsRequired);
        infoUI.ShowDialog();
        LogDebug("Settings updated via UI");
        Process.Start(Environment.ProcessPath);
        Thread.Sleep(2000);
        Environment.Exit(0);
        return;
    }

    public static List<NotificationEntry> NotificationList = new();

    public static void NotifcationLoop()
    {
        if (LoadedSettings.DisplaySteamNotifications)
        {
            _ = Task.Run(() =>
            {
                LogInfo("Loading Notification Assets..");
                Bitmap InfoIcon;
                Bitmap ErrorIcon;

                try
                {
                    InfoIcon = new($"{AppDomain.CurrentDomain.BaseDirectory}Assets\\Info.png");
                    ErrorIcon = new($"{AppDomain.CurrentDomain.BaseDirectory}Assets\\Error.png");
                }
                catch (Exception ex)
                {
                    LogFatal("Failed load Notifaction Assets", ex);
                    return;
                }

                while (true)
                {
                    try
                    {
                        if (Objects.SteamNotificationId == 0)
                        {
                            LogDebug($"[BR] Initializing OpenVR..");
                            bool Initialized = false;

                            while (!Initialized)
                            {
                                Initialized = EasyOpenVRSingleton.Instance.Init();
                                Thread.Sleep(500);
                            }

                            LogDebug($"[BR] Initialized OpenVR.");

                            LogDebug($"[BR] Initializing NotificationOverlay..");
                            Objects.SteamNotificationId = EasyOpenVRSingleton.Instance.InitNotificationOverlay("BeatRecorder");
                            LogDebug($"[BR] Initialized NotificationOverlay: {Objects.SteamNotificationId}");
                        }

                        while (NotificationList.Count == 0)
                            Thread.Sleep(500);

                        NotificationBitmap_t NotifactionIcon;

                        foreach (var b in NotificationList.ToList())
                        {
                            BitmapData TextureData = new();

                            if (b.Type == MessageType.INFO)
                                TextureData = InfoIcon.LockBits(new Rectangle(0, 0, InfoIcon.Width, InfoIcon.Height), ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
                            else if (b.Type == MessageType.ERROR)
                                TextureData = ErrorIcon.LockBits(new Rectangle(0, 0, ErrorIcon.Width, ErrorIcon.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                            NotifactionIcon.m_pImageData = TextureData.Scan0;
                            NotifactionIcon.m_nWidth = TextureData.Width;
                            NotifactionIcon.m_nHeight = TextureData.Height;
                            NotifactionIcon.m_nBytesPerPixel = 4;

                            var NotificationId = EasyOpenVRSingleton.Instance.EnqueueNotification(Objects.SteamNotificationId, EVRNotificationType.Persistent, b.Message, EVRNotificationStyle.Application, NotifactionIcon);
                            LogDebug($"[BR] Displayed Notification {NotificationId}: {b.Message}");

                            if (b.Type == MessageType.INFO)
                                InfoIcon.UnlockBits(TextureData);
                            else if (b.Type == MessageType.ERROR)
                                ErrorIcon.UnlockBits(TextureData);

                            if (NotificationId == 0)
                                return;

                            Thread.Sleep(b.Delay);
                            EasyOpenVRSingleton.Instance.DismissNotification(NotificationId, out var error);

                            if (error != EVRNotificationError.OK)
                            {
                                LogFatal($"Failed to dismiss notification {Objects.SteamNotificationId}: {error}");
                            }

                            LogDebug($"[BR] Dismissed Notification {NotificationId}");

                            NotificationList.Remove(b);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.ToString());
                        Thread.Sleep(5000);
                        continue;
                    }
                }
            });
        }
    }

    public static void SendNotification(string Text, int DisplayTime = 2000, MessageType messageType = MessageType.INFO)
    {
        NotificationList.Add(new NotificationEntry { Message = Text, Delay = DisplayTime, Type = messageType });
    }
}
