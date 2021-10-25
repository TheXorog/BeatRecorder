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

        internal static WebsocketClient BeatSaberWebSocket { get; set; }
        internal static WebsocketClient BeatSaberWebSocketLiveData { get; set; }
        internal static WebsocketClient OBSWebSocket { get; set; }

        // DataPuller


        static bool DataPullerInLevel = false;
        static bool DataPullerPaused = false;

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

                    BeatSaberWebSocket = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.BeatSaberUrl}:{Objects.LoadedSettings.BeatSaberPort}/BSDataPuller/MapData"), factory);
                    BeatSaberWebSocket.ReconnectTimeout = null;
                    BeatSaberWebSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(10);

                    BeatSaberWebSocket.MessageReceived.Subscribe(msg =>
                    {
                        BeatSaberDataPullerMapData_MessageRecieved(msg.Text);
                    });

                    BeatSaberWebSocket.ReconnectionHappened.Subscribe(type =>
                    {
                        if (type.Type != ReconnectionType.Initial)
                            _logger.LogWarn($"[BS-DP1] Reconnected: {type.Type}");
                    });

                    BeatSaberWebSocket.DisconnectionHappened.Subscribe(type =>
                    {
                        try
                        {
                            Process[] processCollection = Process.GetProcesses();

                            if (!processCollection.Any(x => x.ProcessName.ToLower().StartsWith("beat")))
                            {
                                _logger.LogWarn($"[BS-DP1] Couldn't find a BeatSaber process, is BeatSaber started? ({type.Type})");
                            }
                            else
                            {
                                bool FoundWebSocketDll = false;

                                string InstallationDirectory = processCollection.First(x => x.ProcessName.ToLower().StartsWith("beat")).MainModule.FileName;
                                InstallationDirectory = InstallationDirectory.Remove(InstallationDirectory.LastIndexOf("\\"), InstallationDirectory.Length - InstallationDirectory.LastIndexOf("\\"));

                                if (Directory.GetDirectories(InstallationDirectory).Any(x => x.ToLower().EndsWith("plugins")))
                                {
                                    if (Directory.GetFiles($"{InstallationDirectory}\\Plugins").Any(x => x.Contains("DataPuller") && x.EndsWith(".dll")))
                                    {
                                        FoundWebSocketDll = true;
                                    }
                                }
                                else
                                {
                                    _logger.LogCritical($"[BS-DP1] Beat Saber seems to be running but the BSDataPuller modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install BSDataPuller: https://bit.ly/3mcvC7g) ({type.Type})");
                                }

                                if (FoundWebSocketDll)
                                    _logger.LogCritical($"[BS-DP1] Beat Saber seems to be running and the BSDataPuller modifaction seems to be installed. Please make sure you put in the right port and you installed all of BSDataPuller' dependiencies! (If not installed, please install it: https://bit.ly/3mcvC7g) ({type.Type})");
                                else
                                    _logger.LogCritical($"[BS-DP1] Beat Saber seems to be running but the BSDataPuller modifaction doesn't seem to be installed. Please make sure to install BSDataPuller! (If not installed, please install it: https://bit.ly/3mcvC7g) ({type.Type})");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"[BS-DP1] Failed to check if BSDataPuller is installed: (Disconnect Reason: {type.Type}) {ex}");
                        }
                    });

                    _logger.LogInfo($"[BS-DP1] Connecting..");
                    BeatSaberWebSocket.Start().Wait();
                    _logger.LogInfo("[BS-DP1] Connected.");
                });

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

                        BeatSaberWebSocketLiveData = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.BeatSaberUrl}:{Objects.LoadedSettings.BeatSaberPort}/BSDataPuller/LiveData"), factory);
                        BeatSaberWebSocketLiveData.ReconnectTimeout = null;
                        BeatSaberWebSocketLiveData.ErrorReconnectTimeout = TimeSpan.FromSeconds(10);

                        BeatSaberWebSocketLiveData.MessageReceived.Subscribe(msg =>
                        {
                            BeatSaberDataPullerLiveData_MessageRecieved(msg.Text);
                        });

                        BeatSaberWebSocketLiveData.ReconnectionHappened.Subscribe(type =>
                        {
                            if (type.Type != ReconnectionType.Initial)
                                _logger.LogWarn($"[BS-DP2] Reconnected: {type.Type}");
                        });

                        BeatSaberWebSocketLiveData.DisconnectionHappened.Subscribe(type =>
                        {
                            if (BeatSaberWebSocket.IsRunning)
                                _logger.LogError($"[BS-DP2] Disconnected: {type.Type}");
                            else
                                _logger.LogDebug($"[BS-DP2] Disconnected: {type.Type}");
                        });

                        _logger.LogDebug($"[BS-DP2] Connecting..");
                        BeatSaberWebSocketLiveData.Start().Wait();
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

                    BeatSaberWebSocket = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.BeatSaberUrl}:{Objects.LoadedSettings.BeatSaberPort}/socket"), factory);BeatSaberWebSocket.ReconnectTimeout = null;
                    BeatSaberWebSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(10);

                    BeatSaberWebSocket.MessageReceived.Subscribe(msg =>
                    {
                        BeatSaberHttpStatus_MessageReceived(msg.Text);
                    });

                    BeatSaberWebSocket.ReconnectionHappened.Subscribe(type =>
                    {
                        if (type.Type != ReconnectionType.Initial)
                            _logger.LogWarn($"[BS-HS] Reconnected: {type.Type}");
                    });

                    BeatSaberWebSocket.DisconnectionHappened.Subscribe(type =>
                    {
                        try
                        {
                            Process[] processCollection = Process.GetProcesses();

                            if (!processCollection.Any(x => x.ProcessName.ToLower().StartsWith("beat")))
                            {
                                _logger.LogWarn($"[BS-HS] Couldn't find a BeatSaber process, is BeatSaber started? ({type.Type})");
                            }
                            else
                            {
                                bool FoundWebSocketDll = false;

                                string InstallationDirectory = processCollection.First(x => x.ProcessName.ToLower().StartsWith("beat")).MainModule.FileName;
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
                                    _logger.LogCritical($"[BS-HS] Beat Saber seems to be running but the beatsaber-http-status modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install beatsaber-http-status: https://bit.ly/3wYX3Dd) ({type.Type})");
                                }

                                if (FoundWebSocketDll)
                                    _logger.LogCritical($"[BS-HS] Beat Saber seems to be running and the beatsaber-http-status modifaction seems to be installed. Please make sure you put in the right port and you installed all of beatsaber-http-status' dependiencies! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({type.Type})");
                                else
                                    _logger.LogCritical($"[BS-HS] Beat Saber seems to be running but the beatsaber-http-status modifaction doesn't seem to be installed. Please make sure to install beatsaber-http-status! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({type.Type})");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"[BS-HS] Failed to check if beatsaber-http-status is installed: (Disconnect Reason: {type.Type}) {ex}");
                        }
                    });

                    _logger.LogInfo($"[BS-HS] Connecting..");
                    BeatSaberWebSocket.Start().Wait();
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

                OBSWebSocket = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.OBSUrl}:{Objects.LoadedSettings.OBSPort}"), factory);
                OBSWebSocket.ReconnectTimeout = null;
                OBSWebSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(10);

                OBSWebSocket.MessageReceived.Subscribe(msg => { _ = OBSWebSocketEvents.MessageReceived(msg); });
                OBSWebSocket.ReconnectionHappened.Subscribe(type => { OBSWebSocketEvents.Reconnected(type); });
                OBSWebSocket.DisconnectionHappened.Subscribe(type => { OBSWebSocketEvents.Disconnected(type); });

                _logger.LogInfo($"[OBS] Connecting..");
                OBSWebSocket.Start().Wait();

                OBSWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{OBSWebSocketEvents.RequiredAuthenticationGuid}\"}}");

                _logger.LogInfo($"[OBS] Connected.");
            });

            // Leave this thread running to not disconnect from the websocket

            await Task.Delay(-1);
        }

        private static void BeatSaberDataPullerMapData_MessageRecieved(string e)
        {
            DataPullerObjects.DataPullerMain _status = new DataPullerObjects.DataPullerMain();

            try
            {
                _status = JsonConvert.DeserializeObject<DataPullerObjects.DataPullerMain>(e);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"[BS-DP1] Unable to convert BSDataPuller message into an dictionary: {ex}");
                return;
            }

            if (DataPullerInLevel != _status.InLevel)
            {
                if (!DataPullerInLevel && _status.InLevel)
                {
                    DataPullerInLevel = true;
                    _logger.LogInfo("[BS-DP1] Song started.");

                    DataPullerObjects.DataPullerCurrentBeatmap = _status;

                    try
                    {
                        DataPullerObjects.CurrentSongCombo = 0;
                        _ = StartRecording();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BS-DP1] {ex}");
                        return;
                    }
                }
                else if (DataPullerInLevel && !_status.InLevel)
                {
                    DataPullerInLevel = false;
                    DataPullerPaused = false;
                    _logger.LogInfo("[BS-DP1] Menu entered.");

                    try
                    {
                        DataPullerObjects.DataPullerCurrentBeatmap = _status;

                        DataPullerObjects.DataPullerLastPerformance = DataPullerObjects.DataPullerCurrentPerformance;
                        DataPullerObjects.DataPullerLastBeatmap = DataPullerObjects.DataPullerCurrentBeatmap;
                        DataPullerObjects.LastSongCombo = DataPullerObjects.CurrentSongCombo;

                        _ = StopRecording(OBSWebSocketObjects.CancelStopRecordingDelay.Token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BS-DP1] {ex}");
                        return;
                    }
                }
            }

            if (_status.InLevel)
            {
                if (DataPullerPaused != _status.LevelPaused)
                {
                    if (!DataPullerPaused && _status.LevelPaused)
                    {
                        DataPullerPaused = true;
                        _logger.LogInfo("[BS-DP1] Song paused.");

                        try
                        {
                            if (Objects.LoadedSettings.PauseRecordingOnIngamePause)
                                if (OBSWebSocket.IsStarted)
                                    OBSWebSocket.Send($"{{\"request-type\":\"PauseRecording\", \"message-id\":\"PauseRecording\"}}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"[BS-DP1] {ex}");
                            return;
                        }
                    }
                    else if (DataPullerPaused && !_status.LevelPaused)
                    {
                        DataPullerPaused = false;
                        _logger.LogInfo("[BS-DP1] Song resumed.");

                        try
                        {
                            if (Objects.LoadedSettings.PauseRecordingOnIngamePause)
                                if (OBSWebSocket.IsStarted)
                                    OBSWebSocket.Send($"{{\"request-type\":\"ResumeRecording\", \"message-id\":\"ResumeRecording\"}}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"[BS-DP1] {ex}");
                            return;
                        }
                    }
                }
            }
        }

        private static void BeatSaberDataPullerLiveData_MessageRecieved(string e)
        {
            DataPullerObjects.DataPullerData _status = new DataPullerObjects.DataPullerData();

            try
            {
                _status = JsonConvert.DeserializeObject<DataPullerObjects.DataPullerData>(e);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"[BS-DP2] Unable to convert BSDataPuller message into an dictionary: {ex}");
                return;
            }

            if (DataPullerInLevel)
                DataPullerObjects.DataPullerCurrentPerformance = _status;
            else
                DataPullerObjects.DataPullerLastPerformance = _status;

            if (DataPullerObjects.CurrentSongCombo < _status.Combo)
                DataPullerObjects.CurrentSongCombo = _status.Combo;
        }

        private static void BeatSaberHttpStatus_MessageReceived(string e)
        {
            HttpStatusObjects.BeatSaberEvent _status = new HttpStatusObjects.BeatSaberEvent();

            try
            {
                _status = JsonConvert.DeserializeObject<HttpStatusObjects.BeatSaberEvent>(e);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"[BS-HS] Unable to convert beatsaber-http-status message into an dictionary: {ex}");
                return;
            }

            switch (_status.@event)
            {
                case "hello":
                    _logger.LogInfo("[BS-HS] Connected.");
                    break;

                case "songStart":
                    _logger.LogInfo("[BS-HS] Song started.");

                    HttpStatusObjects.FailedCurrentSong = false;
                    HttpStatusObjects.FinishedCurrentSong = false;
                    HttpStatusObjects.HttpStatusCurrentBeatmap = _status.status.beatmap;
                    HttpStatusObjects.HttpStatusCurrentPerformance = _status.status.performance;

                    try
                    {
                        _ = StartRecording();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BS-HS] {ex}");
                        return;
                    }
                    break;

                case "finished":
                    _logger.LogInfo("[BS-HS] Song finished.");

                    HttpStatusObjects.HttpStatusCurrentPerformance = _status.status.performance;
                    HttpStatusObjects.HttpStatusLastPerformance = HttpStatusObjects.HttpStatusCurrentPerformance;
                    HttpStatusObjects.FinishedCurrentSong = true;
                    break;

                case "failed":
                    _logger.LogInfo("[BS-HS] Song failed.");

                    HttpStatusObjects.HttpStatusCurrentPerformance = _status.status.performance;
                    HttpStatusObjects.HttpStatusLastPerformance = HttpStatusObjects.HttpStatusCurrentPerformance;
                    HttpStatusObjects.FailedCurrentSong = true;

                    break;

                case "pause":
                    _logger.LogInfo("[BS-HS] Song paused.");

                    try
                    {
                        if (Objects.LoadedSettings.PauseRecordingOnIngamePause)
                            if (OBSWebSocket.IsStarted)
                                OBSWebSocket.Send($"{{\"request-type\":\"PauseRecording\", \"message-id\":\"PauseRecording\"}}");
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
                        if (Objects.LoadedSettings.PauseRecordingOnIngamePause)
                            if (OBSWebSocket.IsStarted)
                                OBSWebSocket.Send($"{{\"request-type\":\"ResumeRecording\", \"message-id\":\"ResumeRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BS-HS] {ex}");
                        return;
                    }
                    break;

                case "menu":
                    _logger.LogInfo("[BS-HS] Menu entered.");

                    try
                    {
                        HttpStatusObjects.HttpStatusLastPerformance = HttpStatusObjects.HttpStatusCurrentPerformance;
                        HttpStatusObjects.HttpStatusLastBeatmap = HttpStatusObjects.HttpStatusCurrentBeatmap;

                        HttpStatusObjects.FinishedLastSong = HttpStatusObjects.FinishedCurrentSong;
                        HttpStatusObjects.FailedLastSong = HttpStatusObjects.FailedCurrentSong;
                        _ = StopRecording(OBSWebSocketObjects.CancelStopRecordingDelay.Token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BS-HS] {ex}");
                        return;
                    }
                    break;

                case "scoreChanged":
                    HttpStatusObjects.HttpStatusCurrentPerformance = _status.status.performance;
                    break;
            }
        }

        private static async Task StartRecording()
        {
            if (OBSWebSocket.IsStarted)
            {
                if (OBSWebSocketObjects.OBSRecording)
                {
                    OBSWebSocketObjects.CancelStopRecordingDelay.Cancel();
                    await StopRecording(OBSWebSocketObjects.CancelStopRecordingDelay.Token, true);
                }

                OBSWebSocketObjects.CancelStopRecordingDelay = new CancellationTokenSource();

                while (OBSWebSocketObjects.OBSRecording)
                {
                    Thread.Sleep(20);
                }

                if (Objects.LoadedSettings.MininumWaitUntilRecordingCanStart > 199 || Objects.LoadedSettings.MininumWaitUntilRecordingCanStart < 2001)
                    Thread.Sleep(Objects.LoadedSettings.MininumWaitUntilRecordingCanStart);
                else
                {
                    _logger.LogError("The MininumWaitUntilRecordingCanStart has to be between 200ms and 2000ms. Defaulting to a wait time of 800ms.");
                    Thread.Sleep(800);
                }

                OBSWebSocket.Send($"{{\"request-type\":\"StartRecording\", \"message-id\":\"StartRecording\"}}");
            }
            else
            {
                _logger.LogError("[OBS] The WebSocket isn't connected, no recording can be started.");
            }
        }

        private static async Task StopRecording(CancellationToken CancelToken, bool ForceStop = false)
        {
            if (OBSWebSocket.IsStarted)
            {
                if (OBSWebSocketObjects.OBSRecording)
                {
                    if (!ForceStop)
                    {
                        if (Objects.LoadedSettings.StopRecordingDelay > 0 && Objects.LoadedSettings.StopRecordingDelay < 21)
                        {
                            try
                            {
                                await Task.Delay(Objects.LoadedSettings.StopRecordingDelay * 1000, CancelToken);
                            }
                            catch (OperationCanceledException)
                            {
                                return;
                            }
                        }
                        else
                            _logger.LogError("[OBS] The specified delay is not in between 1 and 20 seconds. The delay will be skipped.");
                    }

                    OBSWebSocket.Send($"{{\"request-type\":\"StopRecording\", \"message-id\":\"StopRecording\"}}");
                    return;
                }
            }
            else
            {
                _logger.LogError("[OBS] The WebSocket isn't connected, no recording can be stopped.");
            }
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
