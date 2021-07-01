using Newtonsoft.Json;
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
        static WebsocketClient BeatSaberWebSocket { get; set; }
        static WebsocketClient OBSWebSocket { get; set; }
        static HttpClient OBSHttpClient { get; set; }

        
        static Objects.Performance LastPerformance {get; set;}
        static Objects.Beatmap LastBeatmap {get; set;}

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(true).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("[OBSC] Loading settings..");

            if (File.Exists("Settings.json"))
            {
                try
                {
                    Objects.LoadedSettings = JsonConvert.DeserializeObject<Objects.Settings>(File.ReadAllText("Settings.json"));
                }
                catch (Exception)
                {
                    ResetSettings();
                    return;
                }

                if (Objects.LoadedSettings.ConfigVersion != 1)
                {
                    ResetSettings();
                    return;
                }

                Console.WriteLine("[OBSC] Settings loaded.");
            }
            else
            {
                ResetSettings();
                return;
            }

            _ = Task.Run(async () =>
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
                        Console.WriteLine($"[BS] Reconnected: {type.Type}");
                });

                BeatSaberWebSocket.DisconnectionHappened.Subscribe(type =>
                {
                    Console.WriteLine($"[BS] Disconnected: {type.Type}");
                });

                Console.WriteLine($"[BS] Connecting..");
                BeatSaberWebSocket.Start().Wait();
            });

            _ = Task.Run(async () =>
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

                OBSWebSocket.MessageReceived.Subscribe(async msg =>
                {
                    if (msg.Text.Contains($"\"message-id\":\"{RequiredAuthenticationGuid}\""))
                    {
                        Objects.AuthenticationRequired required = JsonConvert.DeserializeObject<Objects.AuthenticationRequired>(msg.Text);

                        if (required.authRequired)
                        {
                            Console.WriteLine("[OBS] Authenticating..");

                            if (Objects.LoadedSettings.OBSPassword == "")
                            {
                                await Task.Delay(1000);
                                Console.WriteLine("[OBS] A password is required to log into your obs websocket.");
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
                                        Console.WriteLine("[OBS] Cancelled. Press any key to exit.");
                                        Console.ReadKey();
                                        Environment.Exit(0);
                                        return;
                                    }
                                    else if (key == ConsoleKey.Enter)
                                    {
                                        break;
                                    }
                                }

                                Console.WriteLine();
                                if (key == ConsoleKey.Enter)
                                {
                                    if (Objects.LoadedSettings.AskToSaveOBSPassword)
                                    {
                                        key = ConsoleKey.A;

                                        Console.WriteLine("[OBS] Do you want to save this password in the config? (THIS WILL STORE THE PASSWORD IN PLAIN-TEXT, THIS CAN BE ACCESSED BY ANYONE WITH ACCESS TO YOUR FILES. THIS IS NOT RECOMMENDED!)");
                                        while (key != ConsoleKey.Enter || key != ConsoleKey.Escape || key != ConsoleKey.Y || key != ConsoleKey.N)
                                        {
                                            Console.Write("[OBS] y/N > ");

                                            var keyInfo = Console.ReadKey(intercept: true);
                                            key = keyInfo.Key;

                                            if (key == ConsoleKey.Escape)
                                            {
                                                Console.WriteLine("[OBS] Cancelled. Press any key to exit.");
                                                Console.ReadKey();
                                                Environment.Exit(0);
                                                return;
                                            }
                                            else if (key == ConsoleKey.Y)
                                            {
                                                Console.WriteLine("[OBS] Your password is now saved in the Settings.json.");
                                                Objects.LoadedSettings.OBSPassword = Password;
                                                Objects.LoadedSettings.AskToSaveOBSPassword = true;

                                                File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Objects.LoadedSettings, Formatting.Indented));
                                                break;
                                            }
                                            else if (key == ConsoleKey.N || key == ConsoleKey.Enter)
                                            {
                                                Console.WriteLine("[OBS] Your password will not be saved. This wont be asked in the feature.");
                                                Console.WriteLine("[OBS] To re-enable this prompt, set AskToSaveOBSPassword to true in the Settings.json.");
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
                    }

                    if (msg.Text.Contains($"\"message-id\":\"{AuthenticationGuid}\""))
                    {
                        Objects.AuthenticationRequired required = JsonConvert.DeserializeObject<Objects.AuthenticationRequired>(msg.Text);

                        if (required.status == "ok")
                        {
                            Console.WriteLine("[OBS] Authenticated.");
                        }
                        else
                        {
                            Console.WriteLine("[OBS] Failed to authenticate.");
                            await OBSWebSocket.Stop(WebSocketCloseStatus.NormalClosure, "Shutting down");

                            await Task.Delay(1000);

                            Console.WriteLine("[OBS] Re-trying..");
                            await OBSWebSocket.Start();
                            OBSWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{RequiredAuthenticationGuid}\"}}");
                        }
                    }

                    if (msg.Text.Contains("\"update-type\":\"RecordingStopped\""))
                    {
                        Objects.RecordingStopped RecordingStopped = JsonConvert.DeserializeObject<Objects.RecordingStopped>(msg.Text);

                        if (LastBeatmap != null)
                        {
                            bool DeleteFile = false;
                            string NewName = Objects.LoadedSettings.FileFormat;

                            if (LastPerformance != null)
                            {
                                // Generate FileName-based on Config File

                                if (NewName.Contains("<rank>"))
                                    NewName = NewName.Replace("<rank>", LastPerformance.rank);

                                if (NewName.Contains("<accuracy>"))
                                {
                                    string GeneratedAccuracy = "";

                                    if (LastPerformance.softFailed)
                                    {
                                        if (Objects.LoadedSettings.DeleteSoftFailed)
                                            DeleteFile = true;

                                        GeneratedAccuracy = $"NF-";
                                    }

                                    if (Objects.FinishedLastSong)
                                        GeneratedAccuracy += $"{Math.Round((float)(((float)LastPerformance.score * (float)100) / (float)LastBeatmap.maxScore), 2)}";
                                    else
                                    {
                                        if (Objects.LoadedSettings.DeleteQuit)
                                        {
                                            DeleteFile = true;

                                            if (GeneratedAccuracy == "NF-")
                                                if (!Objects.LoadedSettings.DeleteIfQuitAfterSoftFailed)
                                                {
                                                    DeleteFile = false;
                                                }
                                        }

                                        GeneratedAccuracy = $"QUIT";
                                    }

                                    if (Objects.FailedLastSong)
                                    {
                                        if (Objects.LoadedSettings.DeleteFailed)
                                            DeleteFile = true;

                                        GeneratedAccuracy = $"FAILED";
                                    }

                                    NewName = NewName.Replace("<accuracy>", GeneratedAccuracy);
                                }

                                if (NewName.Contains("<max-combo>"))
                                    NewName = NewName.Replace("<max-combo>", $"{LastPerformance.maxCombo}");

                                if (NewName.Contains("<score>"))
                                    NewName = NewName.Replace("<score>", $"{LastPerformance.score}");

                                if (NewName.Contains("<raw-score>"))
                                    NewName = NewName.Replace("<raw-score>", $"{LastPerformance.rawScore}");
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

                            if (NewName.Contains("<song-name>"))
                                NewName = NewName.Replace("<song-name>", LastBeatmap.songName);

                            if (NewName.Contains("<song-author>"))
                                NewName = NewName.Replace("<song-author>", LastBeatmap.songAuthorName);

                            if (NewName.Contains("<song-sub-name>"))
                                NewName = NewName.Replace("<song-sub-name>", LastBeatmap.songSubName);

                            if (NewName.Contains("<mapper>"))
                                NewName = NewName.Replace("<mapper>", LastBeatmap.levelAuthorName);

                            if (NewName.Contains("<levelid>"))
                                NewName = NewName.Replace("<levelid>", LastBeatmap.levelId);

                            if (NewName.Contains("<bpm>"))
                                NewName = NewName.Replace("<bpm>", LastBeatmap.songBPM.ToString());

                            LastPerformance = null;
                            LastBeatmap = null;

                            await Task.Delay(500);

                            if (File.Exists($"{RecordingStopped.recordingFilename}"))
                            {

                                string FileExist = "";

                                FileInfo fileInfo = new FileInfo(RecordingStopped.recordingFilename);

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
                                        Console.WriteLine($"[OBSC] Renaming \"{fileInfo.Name}\" to \"{NewName}{fileInfo.Extension}\"..");
                                        File.Move(RecordingStopped.recordingFilename, NewFileName);
                                        Console.WriteLine($"[OBSC] Successfully renamed.");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[OBSC] Deleting \"{fileInfo.Name}\"..");
                                        File.Delete(RecordingStopped.recordingFilename);
                                        Console.WriteLine($"[OBSC] Successfully deleted.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[OBSC] {ex}.");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[OBSC] {RecordingStopped.recordingFilename} doesn't exist.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[OBSC] Last recorded file can't be renamed.");
                        }
                    }
                });

                OBSWebSocket.ReconnectionHappened.Subscribe(type =>
                {
                    if (type.Type != ReconnectionType.Initial)
                    {
                        Console.WriteLine($"[OBS] Reconnected: {type.Type}");

                        OBSWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{RequiredAuthenticationGuid}\"}}");
                    }
                });

                OBSWebSocket.DisconnectionHappened.Subscribe(type =>
                {
                    Console.WriteLine($"[OBS] Disconnected: {type.Type}");
                });

                Console.WriteLine($"[OBS] Connecting..");
                OBSWebSocket.Start().Wait();

                OBSWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{RequiredAuthenticationGuid}\"}}");

                Console.WriteLine($"[OBS] Connected.");
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
                Console.WriteLine($"[BS] {ex}");
                return;
            }

            switch (_status.@event)
            {
                case "hello":
                    Console.WriteLine("[BS] Connected.");
                    break;

                case "songStart":
                    Console.WriteLine("[BS] Song started.");

                    Objects.FailedLastSong = false;
                    Objects.FinishedLastSong = false;
                    LastBeatmap = _status.status.beatmap;
                    LastPerformance = _status.status.performance;

                    try
                    {
                        if (OBSWebSocket.IsStarted)
                            OBSWebSocket.Send($"{{\"request-type\":\"StartRecording\", \"message-id\":\"StartRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BS] {ex}");
                        return;
                    }
                    break;

                case "finished":
                    Console.WriteLine("[BS] Song finished.");

                    LastPerformance = _status.status.performance;
                    Objects.FinishedLastSong = true;

                    try
                    {
                        return;
                        if (OBSWebSocket.IsStarted)
                            OBSWebSocket.Send($"{{\"request-type\":\"StopRecording\", \"message-id\":\"StopRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BS] {ex}");
                        return;
                    }
                    break;

                case "failed":
                    Console.WriteLine("[BS] Song failed.");
                    
                    LastPerformance = _status.status.performance;
                    Objects.FailedLastSong = true;

                    try
                    {
                        return;
                        if (OBSWebSocket.IsStarted)
                            OBSWebSocket.Send($"{{\"request-type\":\"StopRecording\", \"message-id\":\"StopRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BS] {ex}");
                        return;
                    }
                    break;

                case "pause":
                    Console.WriteLine("[BS] Song paused.");

                    try
                    {
                        if (Objects.LoadedSettings.PauseRecordingOnIngamePause)
                            if (OBSWebSocket.IsStarted)
                                OBSWebSocket.Send($"{{\"request-type\":\"PauseRecording\", \"message-id\":\"PauseRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BS] {ex}");
                        return;
                    }
                    break;

                case "resume":
                    Console.WriteLine("[BS] Song resumed.");

                    try
                    {
                        if (Objects.LoadedSettings.PauseRecordingOnIngamePause)
                            if (OBSWebSocket.IsStarted)
                                OBSWebSocket.Send($"{{\"request-type\":\"ResumeRecording\", \"message-id\":\"ResumeRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BS] {ex}");
                        return;
                    }
                    break;

                case "menu":
                    Console.WriteLine("[BS] Menu entered.");

                    try
                    {
                        if (OBSWebSocket.IsStarted)
                            OBSWebSocket.Send($"{{\"request-type\":\"StopRecording\", \"message-id\":\"StopRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BS] {ex}");
                        return;
                    }
                    break;

                case "scoreChanged":
                    LastPerformance = _status.status.performance;
                    break;
            }
        }

        private static void ResetSettings()
        {
            Objects.LoadedSettings.README = "!! Please check https://github.com/XorogVEVO/OBSControl for more info !!";
            Objects.LoadedSettings.ConfigVersion = 1;
            Objects.LoadedSettings.BeatSaberUrl = "127.0.0.1";
            Objects.LoadedSettings.BeatSaberPort = "6557";
            Objects.LoadedSettings.OBSUrl = "127.0.0.1";
            Objects.LoadedSettings.OBSPort = "4444";
            Objects.LoadedSettings.OBSPassword = "";
            Objects.LoadedSettings.AskToSaveOBSPassword = true;
            Objects.LoadedSettings.PauseRecordingOnIngamePause = false;
            Objects.LoadedSettings.FileFormat = "[<rank>][<accuracy>][<max-combo>x] <song-name> - <song-author> [<mapper>]";
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

            Console.WriteLine($"Please configure the application using the newly created Settings.json.");
            Console.WriteLine($"");
            Console.WriteLine($"Press any key to close application.");
            Process.Start("notepad", "Settings.json");
            Console.ReadKey();
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
