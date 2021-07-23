using Newtonsoft.Json;
using Octokit;
using System;
using System.Diagnostics;
using System.IO;
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
        public static string CurrentVersion = "1.1.0-RC2";

        static WebsocketClient BeatSaberWebSocket { get; set; }
        static WebsocketClient OBSWebSocket { get; set; }

        
        static Objects.Performance LastPerformance { get; set; }
        static Objects.Beatmap LastBeatmap { get; set; }

        static Objects.Performance CurrentPerformance { get; set; }
        static Objects.Beatmap CurrentBeatmap { get; set; }


        static bool OBSRecording = false;
        static bool OBSRecordingPaused = false;
        static int RecordingSeconds = 0;
        static CancellationTokenSource CancelStopRecordingDelay { get; set; }


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
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[OBSC] Exception occured while loading config: {ex}");
                    ResetSettings();
                    return;
                }

                if (Objects.LoadedSettings.ConfigVersion != 2)
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

            CancelStopRecordingDelay = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                try
                {
                    var github = new GitHubClient(new ProductHeaderValue("OBSControlUpdateCheck"));
                    var repo = await github.Repository.Release.GetLatest("XorogVEVO", "OBSControl");

                    _logger.LogInfo($"[OBSC] Current latest release is \"{repo.TagName}\". You're currently running: \"{CurrentVersion}\"");

                    if (!CurrentVersion.Contains($"RC"))
                    {
                        if (repo.TagName != CurrentVersion)
                        {
                            _logger.LogCritical($"[OBSC] You're running an outdated version of OBSControl, please update at https://github.com/XorogVEVO/OBSControl/releases/latest.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[OBSC] Unable to get latest version: {ex}");
                }
            });

            _ = Task.Run(() =>
            {
                // https://github.com/opl-/beatsaber-http-status/blob/master/protocol.md

                var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
                {
                    Options =
                            {
                                KeepAliveInterval = TimeSpan.FromSeconds(5)
                            }
                });


                BeatSaberWebSocket = new WebsocketClient(new Uri($"ws://{Objects.LoadedSettings.BeatSaberUrl}:{Objects.LoadedSettings.BeatSaberPort}/socket"), factory);
                BeatSaberWebSocket.ReconnectTimeout = null;
                BeatSaberWebSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(5);

                BeatSaberWebSocket.MessageReceived.Subscribe(msg =>
                {
                    BeatSaberWebSocket_MessageReceived(msg.Text);
                });

                BeatSaberWebSocket.ReconnectionHappened.Subscribe(type =>
                {
                    if (type.Type != ReconnectionType.Initial)
                        _logger.LogWarn($"[BS] Reconnected: {type.Type}");
                });

                BeatSaberWebSocket.DisconnectionHappened.Subscribe(type =>
                {
                    _logger.LogError($"[BS] Disconnected: {type.Type}");
                });

                _logger.LogInfo($"[BS] Connecting..");
                BeatSaberWebSocket.Start().Wait();
            });

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
                OBSWebSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(5);

                string RequiredAuthenticationGuid = Guid.NewGuid().ToString();
                string AuthenticationGuid = Guid.NewGuid().ToString();
                string CheckIfRecording = Guid.NewGuid().ToString();

                OBSWebSocket.MessageReceived.Subscribe(async msg =>
                {
                    if (msg.Text.Contains($"\"message-id\":\"{RequiredAuthenticationGuid}\""))
                    {
                        Objects.AuthenticationRequired required = JsonConvert.DeserializeObject<Objects.AuthenticationRequired>(msg.Text);

                        if (required.authRequired)
                        {
                            _logger.LogInfo("[OBS] Authenticating..");

                            if (Objects.LoadedSettings.OBSPassword == "")
                            {
                                await Task.Delay(1000);
                                _logger.LogInfo("[OBS] A password is required to log into your obs websocket.");
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
                                        _logger.LogInfo("[OBS] Cancelled. Press any key to exit.");
                                        Console.ReadKey();
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
                                    if (Objects.LoadedSettings.AskToSaveOBSPassword)
                                    {
                                        key = ConsoleKey.A;

                                        _logger.LogWarn("[OBS] Do you want to save this password in the config? (THIS WILL STORE THE PASSWORD IN PLAIN-TEXT, THIS CAN BE ACCESSED BY ANYONE WITH ACCESS TO YOUR FILES. THIS IS NOT RECOMMENDED!)");
                                        while (key != ConsoleKey.Enter || key != ConsoleKey.Escape || key != ConsoleKey.Y || key != ConsoleKey.N)
                                        {
                                            await Task.Delay(1000);
                                            Console.Write("[OBS] y/N > ");

                                            var keyInfo = Console.ReadKey(intercept: true);
                                            Console.Write("\r                                              \r");
                                            key = keyInfo.Key;

                                            if (key == ConsoleKey.Escape)
                                            {
                                                _logger.LogWarn("[OBS] Cancelled. Press any key to exit.");
                                                Console.ReadKey();
                                                Environment.Exit(0);
                                                return;
                                            }
                                            else if (key == ConsoleKey.Y)
                                            {
                                                _logger.LogInfo("[OBS] Your password is now saved in the Settings.json.");
                                                Objects.LoadedSettings.OBSPassword = Password;
                                                Objects.LoadedSettings.AskToSaveOBSPassword = true;

                                                File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Objects.LoadedSettings, Formatting.Indented));
                                                break;
                                            }
                                            else if (key == ConsoleKey.N || key == ConsoleKey.Enter)
                                            {
                                                _logger.LogInfo("[OBS] Your password will not be saved. This wont be asked in the feature.");
                                                _logger.LogInfo("[OBS] To re-enable this prompt, set AskToSaveOBSPassword to true in the Settings.json.");
                                                Objects.LoadedSettings.OBSPassword = "";
                                                Objects.LoadedSettings.AskToSaveOBSPassword = false;

                                                File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Objects.LoadedSettings, Formatting.Indented));
                                                break;
                                            }
                                        }
                                    }

                                    Objects.LoadedSettings.OBSPassword = Password;
                                }
                            }

                            string secret = HashEncode(Objects.LoadedSettings.OBSPassword + required.salt);
                            string authResponse = HashEncode(secret + required.challenge);

                            OBSWebSocket.Send($"{{\"request-type\":\"Authenticate\", \"message-id\":\"{AuthenticationGuid}\", \"auth\":\"{authResponse}\"}}");
                        }
                        else
                        {
                            OBSWebSocket.Send($"{{\"request-type\":\"GetRecordingStatus\", \"message-id\":\"{CheckIfRecording}\"}}");
                        }
                    }
                    else if (msg.Text.Contains($"\"message-id\":\"{AuthenticationGuid}\""))
                    {
                        Objects.AuthenticationRequired required = JsonConvert.DeserializeObject<Objects.AuthenticationRequired>(msg.Text);

                        if (required.status == "ok")
                        {
                            _logger.LogInfo("[OBS] Authenticated.");

                            OBSWebSocket.Send($"{{\"request-type\":\"GetRecordingStatus\", \"message-id\":\"{CheckIfRecording}\"}}");
                        }
                        else
                        {
                            _logger.LogError("[OBS] Failed to authenticate. Please check your password or wait a few seconds to try authentication again.");
                            await OBSWebSocket.Stop(WebSocketCloseStatus.NormalClosure, "Shutting down");

                            await Task.Delay(1000);

                            _logger.LogInfo("[OBS] Re-trying..");
                            await OBSWebSocket.Start();
                            OBSWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{RequiredAuthenticationGuid}\"}}");
                        }
                    }
                    else if (msg.Text.Contains($"\"message-id\":\"{CheckIfRecording}\""))
                    {
                        Objects.RecordingStatus recordingStatus = JsonConvert.DeserializeObject<Objects.RecordingStatus>(msg.Text);

                        OBSRecording = recordingStatus.isRecording;
                        OBSRecordingPaused = recordingStatus.isRecordingPaused;

                        if (recordingStatus.isRecording)
                            _logger.LogWarn($"[OBS] A Recording is already running.");
                    }

                    if (msg.Text.Contains("\"update-type\":\"RecordingStopped\""))
                    {
                        Objects.RecordingStopped RecordingStopped = JsonConvert.DeserializeObject<Objects.RecordingStopped>(msg.Text);

                        _logger.LogInfo($"[OBS] Recording stopped.");
                        OBSRecording = false;

                        HandleFile(LastBeatmap, LastPerformance, RecordingStopped.recordingFilename, Objects.FinishedLastSong, Objects.FailedLastSong);
                    }
                    else if (msg.Text.Contains("\"update-type\":\"RecordingStarted\""))
                    {
                        _logger.LogInfo($"[OBS] Recording started.");
                        OBSRecording = true;
                        while (OBSRecording)
                        {
                            await Task.Delay(1000);

                            if (!OBSRecordingPaused)
                            {
                                RecordingSeconds++;
                            }
                        }
                        RecordingSeconds = 0;
                    }
                    else if (msg.Text.Contains("\"update-type\":\"RecordingPaused\""))
                    {
                        _logger.LogInfo($"[OBS] Recording paused.");
                        OBSRecordingPaused = true;
                    }
                    else if (msg.Text.Contains("\"update-type\":\"RecordingResumed\""))
                    {
                        _logger.LogInfo($"[OBS] Recording resumed.");
                        OBSRecordingPaused = false;
                    }
                });

                OBSWebSocket.ReconnectionHappened.Subscribe(type =>
                {
                    if (type.Type != ReconnectionType.Initial)
                    {
                        _logger.LogInfo($"[OBS] Reconnected: {type.Type}");

                        OBSWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{RequiredAuthenticationGuid}\"}}");
                    }
                });

                OBSWebSocket.DisconnectionHappened.Subscribe(type =>
                {
                    _logger.LogError($"[OBS] Disconnected: {type.Type}");
                });

                _logger.LogInfo($"[OBS] Connecting..");
                OBSWebSocket.Start().Wait();

                OBSWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{RequiredAuthenticationGuid}\"}}");

                _logger.LogInfo($"[OBS] Connected.");
            });

            await Task.Delay(-1);
        }

        private static void BeatSaberWebSocket_MessageReceived(string e)
        {
            Objects.BeatSaberEvent _status = new Objects.BeatSaberEvent();

            try
            {
                _status = JsonConvert.DeserializeObject<Objects.BeatSaberEvent>(e);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"[BS] Unable to convert HttpStatus Message into an dictionary: {ex}");
                return;
            }

            switch (_status.@event)
            {
                case "hello":
                    _logger.LogInfo("[BS] Connected.");
                    break;

                case "songStart":
                    _logger.LogInfo("[BS] Song started.");

                    Objects.FailedCurrentSong = false;
                    Objects.FinishedCurrentSong = false;
                    CurrentBeatmap = _status.status.beatmap;
                    CurrentPerformance = _status.status.performance;

                    try
                    {
                        _ = StartRecording();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BS] {ex}");
                        return;
                    }
                    break;

                case "finished":
                    _logger.LogInfo("[BS] Song finished.");

                    CurrentPerformance = _status.status.performance;
                    Objects.FinishedCurrentSong = true;
                    break;

                case "failed":
                    _logger.LogInfo("[BS] Song failed.");

                    CurrentPerformance = _status.status.performance;
                    Objects.FailedCurrentSong = true;

                    break;

                case "pause":
                    _logger.LogInfo("[BS] Song paused.");

                    try
                    {
                        if (Objects.LoadedSettings.PauseRecordingOnIngamePause)
                            if (OBSWebSocket.IsStarted)
                                OBSWebSocket.Send($"{{\"request-type\":\"PauseRecording\", \"message-id\":\"PauseRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BS] {ex}");
                        return;
                    }
                    break;

                case "resume":
                    _logger.LogInfo("[BS] Song resumed.");

                    try
                    {
                        if (Objects.LoadedSettings.PauseRecordingOnIngamePause)
                            if (OBSWebSocket.IsStarted)
                                OBSWebSocket.Send($"{{\"request-type\":\"ResumeRecording\", \"message-id\":\"ResumeRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BS] {ex}");
                        return;
                    }
                    break;

                case "menu":
                    _logger.LogInfo("[BS] Menu entered.");

                    try
                    {
                        LastPerformance = CurrentPerformance;
                        LastBeatmap = CurrentBeatmap;

                        Objects.FinishedLastSong = Objects.FinishedCurrentSong;
                        Objects.FailedLastSong = Objects.FailedCurrentSong;
                        _ = StopRecording(CancelStopRecordingDelay.Token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BS] {ex}");
                        return;
                    }
                    break;

                case "scoreChanged":
                    CurrentPerformance = _status.status.performance;
                    break;
            }
        }

        private static async Task StartRecording()
        {
            if (OBSWebSocket.IsStarted)
            {
                if (OBSRecording)
                {
                    CancelStopRecordingDelay.Cancel();
                    await StopRecording(CancelStopRecordingDelay.Token, true);
                }

                CancelStopRecordingDelay = new CancellationTokenSource();

                while (OBSRecording)
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
                if (OBSRecording)
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

        private static void HandleFile(Objects.Beatmap BeatmapInfo, Objects.Performance PerformanceInfo, string OldFileName, bool FinishedLast, bool FailedLast)
        {
            if (BeatmapInfo != null)
            {
                bool DeleteFile = false;
                string NewName = Objects.LoadedSettings.FileFormat;

                if (PerformanceInfo != null)
                {
                    // Generate FileName-based on Config File

                    if (NewName.Contains("<rank>"))
                        NewName = NewName.Replace("<rank>", PerformanceInfo.rank);

                    if (NewName.Contains("<accuracy>"))
                    {
                        string GeneratedAccuracy = "";

                        if (PerformanceInfo.softFailed)
                        {
                            if (Objects.LoadedSettings.DeleteSoftFailed)
                            {
                                _logger.LogDebug($"[OBSC] Soft-Failed. Deletion requested.");
                                DeleteFile = true;
                            }

                            GeneratedAccuracy = $"NF-";
                        }

                        if (FinishedLast)
                            GeneratedAccuracy += $"{Math.Round((float)(((float)PerformanceInfo.score * (float)100) / (float)BeatmapInfo.maxScore), 2)}";
                        else
                        {
                            if (Objects.LoadedSettings.DeleteQuit)
                            {
                                _logger.LogDebug($"[OBSC] Quit. Deletion requested.");
                                DeleteFile = true;

                                if (GeneratedAccuracy == "NF-")
                                    if (!Objects.LoadedSettings.DeleteIfQuitAfterSoftFailed)
                                    {
                                        _logger.LogDebug($"[OBSC] Soft-Failed but quit, deletion request reverted.");
                                        DeleteFile = false;
                                    }
                            }

                            GeneratedAccuracy += $"QUIT";
                        }

                        if (FailedLast)
                        {
                            if (Objects.LoadedSettings.DeleteFailed)
                            {
                                _logger.LogDebug($"[OBSC] Failed. Deletion requested.");
                                DeleteFile = true;
                            }
                            else
                                DeleteFile = false;

                            GeneratedAccuracy = $"FAILED";
                        }

                        NewName = NewName.Replace("<accuracy>", GeneratedAccuracy);
                    }

                    if (NewName.Contains("<max-combo>"))
                        NewName = NewName.Replace("<max-combo>", $"{PerformanceInfo.maxCombo}");

                    if (NewName.Contains("<score>"))
                        NewName = NewName.Replace("<score>", $"{PerformanceInfo.score}");

                    if (NewName.Contains("<raw-score>"))
                        NewName = NewName.Replace("<raw-score>", $"{PerformanceInfo.rawScore}");
                }
                else
                {
                    // Generate FileName-based on Config File (but without performance stats)

                    if (NewName.Contains("<rank>"))
                        NewName = NewName.Replace("<rank>", "Z");

                    if (NewName.Contains("<accuracy>"))
                        NewName = NewName.Replace("<accuracy>", "00.00");

                    if (NewName.Contains("<max-combo>"))
                        NewName = NewName.Replace("<max-combo>", $"0");

                    if (NewName.Contains("<score>"))
                        NewName = NewName.Replace("<score>", $"0");

                    if (NewName.Contains("<raw-score>"))
                        NewName = NewName.Replace("<raw-score>", $"0");
                }

                if (Objects.LoadedSettings.DeleteIfShorterThan + Objects.LoadedSettings.StopRecordingDelay > RecordingSeconds)
                {
                    _logger.LogDebug($"[OBSC] The recording is too short. Deletion requested.");
                    DeleteFile = true;
                }

                if (NewName.Contains("<song-name>"))
                    NewName = NewName.Replace("<song-name>", BeatmapInfo.songName);

                if (NewName.Contains("<song-author>"))
                    NewName = NewName.Replace("<song-author>", BeatmapInfo.songAuthorName);

                if (NewName.Contains("<song-sub-name>"))
                    NewName = NewName.Replace("<song-sub-name>", BeatmapInfo.songSubName);

                if (NewName.Contains("<mapper>"))
                    NewName = NewName.Replace("<mapper>", BeatmapInfo.levelAuthorName);

                if (NewName.Contains("<levelid>"))
                    NewName = NewName.Replace("<levelid>", BeatmapInfo.levelId);

                if (NewName.Contains("<bpm>"))
                    NewName = NewName.Replace("<bpm>", BeatmapInfo.songBPM.ToString());

                if (NewName.Contains("<difficulty>"))
                {
                    if (BeatmapInfo.difficulty.ToLower() == "easy")
                        NewName = NewName.Replace("<long-difficulty>", "Easy");

                    if (BeatmapInfo.difficulty.ToLower() == "normal")
                        NewName = NewName.Replace("<long-difficulty>", "Normal");

                    if (BeatmapInfo.difficulty.ToLower() == "hard")
                        NewName = NewName.Replace("<long-difficulty>", "Hard");

                    if (BeatmapInfo.difficulty.ToLower() == "expert")
                        NewName = NewName.Replace("<long-difficulty>", "Expert");

                    if (BeatmapInfo.difficulty.ToLower() == "expertplus")
                        NewName = NewName.Replace("<long-difficulty>", "Expert+");
                }

                if (NewName.Contains("<short-difficulty>"))
                {
                    if (BeatmapInfo.difficulty.ToLower() == "easy")
                        NewName = NewName.Replace("<long-difficulty>", "E");

                    if (BeatmapInfo.difficulty.ToLower() == "normal")
                        NewName = NewName.Replace("<long-difficulty>", "N");

                    if (BeatmapInfo.difficulty.ToLower() == "hard")
                        NewName = NewName.Replace("<long-difficulty>", "H");

                    if (BeatmapInfo.difficulty.ToLower() == "expert")
                        NewName = NewName.Replace("<long-difficulty>", "EX");

                    if (BeatmapInfo.difficulty.ToLower() == "expertplus")
                        NewName = NewName.Replace("<long-difficulty>", "EX+");
                }

                if (File.Exists($"{OldFileName}"))
                {

                    string FileExist = "";

                    FileInfo fileInfo = new FileInfo(OldFileName);

                    while (File.Exists($"{fileInfo.Directory.FullName}\\{NewName}{FileExist}{fileInfo.Extension}"))
                    {
                        FileExist += "_";
                    }

                    foreach (char b in Path.GetInvalidFileNameChars())
                    {
                        NewName = NewName.Replace(b, '_');
                    }

                    string NewFileName = $"{fileInfo.Directory.FullName}\\{NewName}{FileExist}{fileInfo.Extension}";

                    try
                    {
                        if (!DeleteFile)
                        {
                            _logger.LogInfo($"[OBSC] Renaming \"{fileInfo.Name}\" to \"{NewName}{fileInfo.Extension}\"..");
                            File.Move(OldFileName, NewFileName);
                            _logger.LogInfo($"[OBSC] Successfully renamed.");
                        }
                        else
                        {
                            _logger.LogInfo($"[OBSC] Deleting \"{fileInfo.Name}\"..");
                            File.Delete(OldFileName);
                            _logger.LogInfo($"[OBSC] Successfully deleted.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[OBSC] {ex}.");
                    }
                }
                else
                {
                    _logger.LogError($"[OBSC] {OldFileName} doesn't exist.");
                }
            }
            else
            {
                _logger.LogError($"[OBSC] Last recorded file can't be renamed.");
            }
        }

        private static void ResetSettings()
        {
            Objects.LoadedSettings.README = "!! Please check https://github.com/XorogVEVO/OBSControl for more info and explainations for each config options !!";
            Objects.LoadedSettings.ConfigVersion = 2;
            Objects.LoadedSettings.ConsoleLogLevel = 3;
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
                } catch { }
            }

            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Objects.LoadedSettings, Formatting.Indented));

            _logger.LogInfo($"Please configure OBSControl using the config file that was just opened. If you're done, save and quit notepad and OBSControl will restart for you.");

            var _Process = Process.Start("notepad", "Settings.json");
            _Process.WaitForExit();

            Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Environment.Exit(0);
        }

        // Used for authentication with OBS Websocket if authentication is required ("Borrowed" from https://github.com/BarRaider/obs-websocket-dotnet/blob/268b7f6c52d8daf8e8d08cf517812009c6f9cc26/obs-websocket-dotnet/OBSWebsocket.cs#L797)
        protected static string HashEncode(string input)
        {
            using var sha256 = new SHA256Managed();

            byte[] textBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = sha256.ComputeHash(textBytes);

            return System.Convert.ToBase64String(hash);
        }
    }
}
