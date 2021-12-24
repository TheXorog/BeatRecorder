﻿using Newtonsoft.Json;
using Octokit;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using BOLL7708;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace BeatRecorder
{
    class Program
    {
        public static string CurrentVersion = "1.5.1";
        public static int ConfigVersion = 3;

        internal static WebsocketClient beatSaberWebSocket { get; set; }
        internal static WebsocketClient beatSaberWebSocketLiveData { get; set; }
        internal static WebsocketClient obsWebSocket { get; set; }

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(true).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            Console.Clear();
            _logger.StartLogger();
            
            _logger.LogInfo($"[BR] Writing to file {_logger.FileName}");

            _logger.LogInfo("[BR] Loading settings..");

            if (File.Exists("Settings.json"))
            {
                try
                {
                    Objects.LoadedSettings = JsonConvert.DeserializeObject<Objects.Settings>(File.ReadAllText("Settings.json"));
                    File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Objects.LoadedSettings, Formatting.Indented));

                    if (Objects.LoadedSettings.ConsoleLogLevel > 2)
                    {
                        _logger.LogLevel = Objects.LoadedSettings.ConsoleLogLevel;
                    }
                    else
                        throw new Exception("Invalid Console Log Level.");

                    if (Objects.LoadedSettings.Mod != "http-status" && Objects.LoadedSettings.Mod != "datapuller")
                    {
                        throw new Exception("Invalid Mod selected.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BR] Exception occured while loading config: {ex}");
                    ResetSettings();
                    return;
                }

                _logger.LogInfo("[BR] Settings loaded.");
            }
            else
            {
                ResetSettings();
                return;
            }

            _logger.LogDebug($"Loaded settings:\n\n{JsonConvert.SerializeObject(Objects.LoadedSettings, Formatting.Indented)}\n");
            _logger.LogDebug($"{AppDomain.CurrentDomain.BaseDirectory}");
            _logger.LogDebug($"{Environment.CurrentDirectory}");

            OBSWebSocketObjects.CancelStopRecordingDelay = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                try
                {
                    var github = new GitHubClient(new ProductHeaderValue("BeatRecorderUpdateCheck"));
                    var repo = await github.Repository.Release.GetLatest("TheXorog", "BeatRecorder");

                    _logger.LogInfo($"[BR] Current latest release is \"{repo.TagName}\". You're currently running: \"{CurrentVersion}\"");

                    if (repo.TagName != CurrentVersion)
                    {
                        _logger.LogCritical($"[BR] You're running an outdated version of BeatRecorder, please update at https://github.com/TheXorog/BeatRecorder/releases/latest." +
                                            $"\n\nWhat changed in the new version:\n\n" +
                                            $"{repo.Body}\n");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BR] Unable to get latest version: {ex}");
                }
            });

            if (Objects.LoadedSettings.Mod == "datapuller")
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

                    beatSaberWebSocket = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.BeatSaberUrl}:{Objects.LoadedSettings.BeatSaberPort}/BSDataPuller/MapData"), factory)
                    {
                        ReconnectTimeout = null,
                        ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
                    };

                    beatSaberWebSocket.MessageReceived.Subscribe(msg => { DataPuller.MapDataMessageRecieved(msg.Text); });
                    beatSaberWebSocket.ReconnectionHappened.Subscribe(type => { DataPuller.MapDataReconnected(type); });
                    beatSaberWebSocket.DisconnectionHappened.Subscribe(type => { DataPuller.MapDataDisconnected(type); });

                    _logger.LogInfo($"[BS-DP1] Connecting..");
                    beatSaberWebSocket.Start().Wait();
                    _logger.LogInfo("[BS-DP1] Connected.");
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

                        beatSaberWebSocketLiveData = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.BeatSaberUrl}:{Objects.LoadedSettings.BeatSaberPort}/BSDataPuller/LiveData"), factory)
                        {
                            ReconnectTimeout = null,
                            ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
                        };

                        beatSaberWebSocketLiveData.MessageReceived.Subscribe(msg => { DataPuller.LiveDataMessageRecieved(msg.Text); });
                        beatSaberWebSocketLiveData.ReconnectionHappened.Subscribe(type => { DataPuller.LiveDataReconnected(type); });
                        beatSaberWebSocketLiveData.DisconnectionHappened.Subscribe(type => { DataPuller.LiveDataDisconnected(type); });

                        _logger.LogDebug($"[BS-DP2] Connecting..");
                        beatSaberWebSocketLiveData.Start().Wait();
                        _logger.LogDebug("[BS-DP2] Connected.");
                    });
            }
            else if (Objects.LoadedSettings.Mod == "http-status")
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

                    beatSaberWebSocket = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.BeatSaberUrl}:{Objects.LoadedSettings.BeatSaberPort}/socket"), factory)
                    {
                        ReconnectTimeout = null,
                        ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
                    };
                    beatSaberWebSocket.MessageReceived.Subscribe(msg => { HttpStatus.MessageReceived(msg.Text); });
                    beatSaberWebSocket.ReconnectionHappened.Subscribe(type => { HttpStatus.Reconnected(type); });
                    beatSaberWebSocket.DisconnectionHappened.Subscribe(type => { HttpStatus.Disconnected(type); });

                    _logger.LogInfo($"[BS-HS] Connecting..");
                    beatSaberWebSocket.Start().Wait();
                });
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

                obsWebSocket = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.OBSUrl}:{Objects.LoadedSettings.OBSPort}"), factory)
                {
                    ReconnectTimeout = null,
                    ErrorReconnectTimeout = TimeSpan.FromSeconds(3)
                };

                obsWebSocket.MessageReceived.Subscribe(msg => { _ = OBSWebSocketEvents.MessageReceived(msg); });
                obsWebSocket.ReconnectionHappened.Subscribe(type => { OBSWebSocketEvents.Reconnected(type); });
                obsWebSocket.DisconnectionHappened.Subscribe(type => { OBSWebSocketEvents.Disconnected(type); });

                _logger.LogInfo($"[OBS] Connecting..");
                obsWebSocket.Start().Wait();

                obsWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{OBSWebSocketEvents.RequiredAuthenticationGuid}\"}}");

                _logger.LogInfo($"[OBS] Connected.");

                SendNotification("Connected to OBS", 1000, Objects.MessageType.INFO);
            });

            SendNotification("Note: Using Steam Notifications is still experimental. If you run into issues, please make sure to report them on GitHub.", 20000, Objects.MessageType.ERROR);

            await Task.Delay(5000);
            NotifcationLoop();

            // Don't close the application
            await Task.Delay(-1);
        }

        private static void ResetSettings()
        {
            Objects.LoadedSettings = new();

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

            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Objects.LoadedSettings, Formatting.Indented));

            SendNotification("Your settings were reset due to an error. Please check your desktop.", 10000, Objects.MessageType.ERROR);
            _logger.LogInfo($"Please configure BeatRecorder using the config file that was just opened. If you're done, save and quit notepad and BeatRecorder will restart for you.");

            var _Process = Process.Start("notepad", "Settings.json");
            _Process.WaitForExit();

            Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Environment.Exit(0);
        }

        public static Dictionary<int, Objects.NotificationEntry> NotificationList = new();

        public static void NotifcationLoop()
        {
            if (Objects.LoadedSettings.DisplaySteamNotifications)
            {
                _logger.LogCritical($"\n\nUsing Steam Notifications is still experimental. Issues might include:\n" +
                                $"- notifications not dissapearing\n" +
                                $"- notifications might interrupt your vision during gameplay if you look up while starting/resuming a song\n\n");


                _ = Task.Run(() =>
                {
                    Bitmap Info_notification_bitmap = new($"{AppDomain.CurrentDomain.BaseDirectory}Info.png");
                    Bitmap Error_notification_bitmap = new($"{AppDomain.CurrentDomain.BaseDirectory}Error.png");

                    while (true)
                    {
                        try
                        {
                            if (Objects.SteamNotificationId == 0)
                            {
                                _logger.LogDebug($"[BR] Initializing OpenVR..");
                                bool Initialized = false;

                                while (!Initialized)
                                {
                                    Initialized = EasyOpenVRSingleton.Instance.Init();
                                    Thread.Sleep(500);
                                }

                                _logger.LogDebug($"[BR] Initialized OpenVR.");

                                _logger.LogDebug($"[BR] Initializing NotificationOverlay..");
                                Objects.SteamNotificationId = EasyOpenVRSingleton.Instance.InitNotificationOverlay("BeatRecorder");
                                _logger.LogDebug($"[BR] Initialized NotificationOverlay: {Objects.SteamNotificationId}");
                            }

                            while (NotificationList.Count == 0)
                                Thread.Sleep(500);

                            Valve.VR.NotificationBitmap_t notification_icon;

                            foreach (var b in NotificationList.ToList())
                            {
                                BitmapData TextureData = new();

                                if (b.Value.Type == Objects.MessageType.INFO)
                                    TextureData = Info_notification_bitmap.LockBits(
                                            new Rectangle(0, 0, Info_notification_bitmap.Width, Info_notification_bitmap.Height),
                                            System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                else if (b.Value.Type == Objects.MessageType.ERROR)
                                    TextureData = Error_notification_bitmap.LockBits(
                                            new Rectangle(0, 0, Error_notification_bitmap.Width, Error_notification_bitmap.Height),
                                            System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                                notification_icon.m_pImageData = TextureData.Scan0;
                                notification_icon.m_nWidth = TextureData.Width;
                                notification_icon.m_nHeight = TextureData.Height;
                                notification_icon.m_nBytesPerPixel = 4;

                                var NotificationId = EasyOpenVRSingleton.Instance.EnqueueNotification(Objects.SteamNotificationId, Valve.VR.EVRNotificationType.Persistent, b.Value.Message, Valve.VR.EVRNotificationStyle.Application, notification_icon);
                                _logger.LogDebug($"[BR] Displayed Notification {NotificationId}: {b.Value.Message}");

                                if (b.Value.Type == Objects.MessageType.INFO)
                                    Info_notification_bitmap.UnlockBits(TextureData);
                                else if (b.Value.Type == Objects.MessageType.ERROR)
                                    Error_notification_bitmap.UnlockBits(TextureData);

                                if (NotificationId == 0)
                                    return;

                                Thread.Sleep(b.Value.Delay);
                                EasyOpenVRSingleton.Instance.DismissNotification(NotificationId, out var error);

                                if (error != Valve.VR.EVRNotificationError.OK)
                                {
                                    _logger.LogError($"Failed to dismiss notification {Objects.SteamNotificationId}: {error}");
                                }

                                _logger.LogDebug($"[BR] Dismissed Notification {NotificationId}");

                                NotificationList.Remove(b.Key);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.ToString());
                            Thread.Sleep(5000);
                            continue;
                        }
                    }
                });
            }
        }

        public static void SendNotification(string Text, int DisplayTime = 2000, Objects.MessageType messageType = Objects.MessageType.INFO)
        {
            int A = 0;

            while (NotificationList.ContainsKey(A))
                A = new Random().Next();

            NotificationList.Add(A, new Objects.NotificationEntry { Message = Text, Delay = DisplayTime, Type = messageType });
        }
    }
}