using Newtonsoft.Json;
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

namespace OBSControl
{
    class Program
    {
        public static string CurrentVersion = "1.3.0-RC3";
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

            _logger.LogInfo($"[OBSC] Writing to file {_logger.FileName}");

            _logger.LogWarn("[OBSC] This application uses the UTC (+00:00) time offset.");

            _logger.LogInfo("[OBSC] Loading settings..");

            if (File.Exists("Settings.json"))
            {
                try
                {
                    Objects.LoadedSettings = JsonConvert.DeserializeObject<Objects.Settings>(File.ReadAllText("Settings.json"));

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
                    _logger.LogError($"[OBSC] Exception occured while loading config: {ex}");
                    ResetSettings();
                    return;
                }

                if (Objects.LoadedSettings.ConfigVersion != ConfigVersion)
                {
                    _logger.LogError($"[OBSC] Old Config detected. Resetting..");
                    ResetSettings();
                    return;
                }

                _logger.LogInfo("[OBSC] Settings loaded.");
            }
            else
            {
                ResetSettings();
                return;
            }

            _logger.LogDebug($"Loaded settings:\n\n{JsonConvert.SerializeObject(Objects.LoadedSettings, Formatting.Indented)}\n");

            OBSWebSocketObjects.CancelStopRecordingDelay = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                try
                {
                    var github = new GitHubClient(new ProductHeaderValue("OBSControlUpdateCheck"));
                    var repo = await github.Repository.Release.GetLatest("XorogVEVO", "OBSControl");

                    _logger.LogInfo($"[OBSC] Current latest release is \"{repo.TagName}\". You're currently running: \"{CurrentVersion}\"");

                    if (repo.TagName != CurrentVersion)
                    {
                        _logger.LogCritical($"[OBSC] You're running an outdated version of OBSControl, please update at https://github.com/XorogVEVO/OBSControl/releases/latest." +
                                            $"\n\nWhat changed in the new version:\n\n" +
                                            $"{repo.Body}\n");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[OBSC] Unable to get latest version: {ex}");
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

                    beatSaberWebSocket = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.BeatSaberUrl}:{Objects.LoadedSettings.BeatSaberPort}/BSDataPuller/MapData"), factory);
                    beatSaberWebSocket.ReconnectTimeout = null;
                    beatSaberWebSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(10);

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

                        beatSaberWebSocketLiveData = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.BeatSaberUrl}:{Objects.LoadedSettings.BeatSaberPort}/BSDataPuller/LiveData"), factory);
                        beatSaberWebSocketLiveData.ReconnectTimeout = null;
                        beatSaberWebSocketLiveData.ErrorReconnectTimeout = TimeSpan.FromSeconds(10);

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

                    beatSaberWebSocketLiveData = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.BeatSaberUrl}:{Objects.LoadedSettings.BeatSaberPort}/socket"), factory); beatSaberWebSocketLiveData.ReconnectTimeout = null;
                    beatSaberWebSocketLiveData.ErrorReconnectTimeout = TimeSpan.FromSeconds(10);

                    beatSaberWebSocketLiveData.MessageReceived.Subscribe(msg => { HttpStatus.MessageReceived(msg.Text); });
                    beatSaberWebSocketLiveData.ReconnectionHappened.Subscribe(type => { HttpStatus.Reconnected(type); });
                    beatSaberWebSocketLiveData.DisconnectionHappened.Subscribe(type => { HttpStatus.Disconnected(type); });

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

                obsWebSocket = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.OBSUrl}:{Objects.LoadedSettings.OBSPort}"), factory);
                obsWebSocket.ReconnectTimeout = null;
                obsWebSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(10);

                obsWebSocket.MessageReceived.Subscribe(msg => { _ = OBSWebSocketEvents.MessageReceived(msg); });
                obsWebSocket.ReconnectionHappened.Subscribe(type => { OBSWebSocketEvents.Reconnected(type); });
                obsWebSocket.DisconnectionHappened.Subscribe(type => { OBSWebSocketEvents.Disconnected(type); });

                _logger.LogInfo($"[OBS] Connecting..");
                obsWebSocket.Start().Wait();

                obsWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{OBSWebSocketEvents.RequiredAuthenticationGuid}\"}}");

                _logger.LogInfo($"[OBS] Connected.");
            });

            // Leave this thread running to not disconnect from the websocket
            await Task.Delay(-1);
        }

        private static void ResetSettings()
        {
            Objects.LoadedSettings.README = "!! Please check https://github.com/XorogVEVO/OBSControl for more info and explainations for each config options !!";
            Objects.LoadedSettings.ConfigVersion = ConfigVersion;
            Objects.LoadedSettings.ConsoleLogLevel = 3;
            Objects.LoadedSettings.Mod = "http-status";
            Objects.LoadedSettings.BeatSaberUrl = "127.0.0.1";
            Objects.LoadedSettings.BeatSaberPort = "6557";
            Objects.LoadedSettings.OBSUrl = "127.0.0.1";
            Objects.LoadedSettings.OBSPort = "4444";
            Objects.LoadedSettings.OBSPassword = "";
            Objects.LoadedSettings.AskToSaveOBSPassword = true;
            Objects.LoadedSettings.MininumWaitUntilRecordingCanStart = 500;
            Objects.LoadedSettings.PauseRecordingOnIngamePause = false;
            Objects.LoadedSettings.FileFormat = "[<rank>][<accuracy>][<max-combo>x] <song-name> - <song-author> [<mapper>]";
            Objects.LoadedSettings.StopRecordingDelay = 5;
            Objects.LoadedSettings.DeleteIfShorterThan = 0;
            Objects.LoadedSettings.DeleteQuit = false;
            Objects.LoadedSettings.DeleteFailed = false;
            Objects.LoadedSettings.DeleteIfQuitAfterSoftFailed = false;
            Objects.LoadedSettings.DeleteSoftFailed = false;

            if (File.Exists("Settings.json"))
            {
                try
                {
                    File.Copy("Settings.json", "Settings.json.old");
                }
                catch { }
            }

            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Objects.LoadedSettings, Formatting.Indented));

            _logger.LogInfo($"Please configure OBSControl using the config file that was just opened. If you're done, save and quit notepad and OBSControl will restart for you.");

            var _Process = Process.Start("notepad", "Settings.json");
            _Process.WaitForExit();

            Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Environment.Exit(0);
        }
    }
}
