using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;
using Websocket.Client.Models;

namespace OBSControl
{
    class HttpStatus
    {
        internal static void MessageReceived(string e)
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
                    _logger.LogDebug("[BS-HS] Song started.");
                    _logger.LogInfo($"[BS-HS] Started playing \"{_status.status.beatmap.songName}\" by \"{_status.status.beatmap.songAuthorName}\"");

                    HttpStatusObjects.FailedCurrentSong = false;
                    HttpStatusObjects.FinishedCurrentSong = false;
                    HttpStatusObjects.HttpStatusCurrentBeatmap = _status.status.beatmap;
                    HttpStatusObjects.HttpStatusCurrentPerformance = _status.status.performance;

                    try
                    {
                        _ = OBSWebSocket.StartRecording();
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
                            if (Program.obsWebSocket.IsStarted)
                                Program.obsWebSocket.Send($"{{\"request-type\":\"PauseRecording\", \"message-id\":\"PauseRecording\"}}");
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
                            if (Program.obsWebSocket.IsStarted)
                                Program.obsWebSocket.Send($"{{\"request-type\":\"ResumeRecording\", \"message-id\":\"ResumeRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BS-HS] {ex}");
                        return;
                    }
                    break;

                case "menu":
                    _logger.LogDebug("[BS-HS] Menu entered.");
                    _logger.LogInfo($"[BS-HS] Stopped playing \"{_status.status.beatmap.songName}\" by \"{_status.status.beatmap.songAuthorName}\"");

                    try
                    {
                        HttpStatusObjects.HttpStatusLastPerformance = HttpStatusObjects.HttpStatusCurrentPerformance;
                        HttpStatusObjects.HttpStatusLastBeatmap = HttpStatusObjects.HttpStatusCurrentBeatmap;

                        HttpStatusObjects.FinishedLastSong = HttpStatusObjects.FinishedCurrentSong;
                        HttpStatusObjects.FailedLastSong = HttpStatusObjects.FailedCurrentSong;
                        _ = OBSWebSocket.StopRecording(OBSWebSocketObjects.CancelStopRecordingDelay.Token);
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

        internal static void Reconnected(ReconnectionInfo msg)
        {
            if (msg.Type != ReconnectionType.Initial)
                _logger.LogWarn($"[BS-HS] Reconnected: {msg.Type}");

            Objects.LastHttpStatusWarning = Objects.ConnectionTypeWarning.CONNECTED;
            Program.SendNotification("Connected to Beat Saber", 1000, Objects.MessageType.INFO);
        }

        internal static void Disconnected(DisconnectionInfo msg)
        {
            try
            {
                Process[] processCollection = Process.GetProcesses();

                if (!processCollection.Any(x => x.ProcessName.ToLower().StartsWith("beat")))
                {
                    if (Objects.LastHttpStatusWarning != Objects.ConnectionTypeWarning.NO_PROCESS)
                    {
                        _logger.LogWarn($"[BS-HS] Couldn't find a BeatSaber process, is BeatSaber started? ({msg.Type})");
                        Program.SendNotification("Couldn't connect to BeatSaber, is it even running?", 5000, Objects.MessageType.ERROR);
                    }
                    Objects.LastHttpStatusWarning = Objects.ConnectionTypeWarning.NO_PROCESS;
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
                        if (Objects.LastHttpStatusWarning != Objects.ConnectionTypeWarning.NOT_MODDED)
                        {
                            _logger.LogCritical($"[BS-HS] Beat Saber seems to be running but the beatsaber-http-status modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install beatsaber-http-status: https://bit.ly/3wYX3Dd) ({msg.Type})");
                            Program.SendNotification("Couldn't connect to Beat Saber. Have you modded your game?", 10000, Objects.MessageType.ERROR);
                        }
                        Objects.LastHttpStatusWarning = Objects.ConnectionTypeWarning.NOT_MODDED;
                    }

                    if (FoundWebSocketDll)
                    {
                        if (Objects.LastHttpStatusWarning != Objects.ConnectionTypeWarning.MOD_INSTALLED)
                        {
                            _logger.LogCritical($"[BS-HS] Beat Saber seems to be running and the beatsaber-http-status modifaction seems to be installed. Please make sure you put in the right port and you installed all of beatsaber-http-status' dependiencies! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                            Program.SendNotification("Couldn't connect to Beat Saber. Please make sure you selected the right port.", 10000, Objects.MessageType.ERROR);
                        }
                        Objects.LastHttpStatusWarning = Objects.ConnectionTypeWarning.MOD_INSTALLED;
                    }
                    else
                    {
                        if (Objects.LastHttpStatusWarning != Objects.ConnectionTypeWarning.MOD_NOT_INSTALLED)
                        {
                            _logger.LogCritical($"[BS-HS] Beat Saber seems to be running but the beatsaber-http-status modifaction doesn't seem to be installed. Please make sure to install beatsaber-http-status! (If not installed, please install it: https://bit.ly/3wYX3Dd) ({msg.Type})");
                            Program.SendNotification("Couldn't connect to Beat Saber. Please make sure DataPuller is installed.", 10000, Objects.MessageType.ERROR);
                        }
                        Objects.LastHttpStatusWarning = Objects.ConnectionTypeWarning.MOD_NOT_INSTALLED;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[BS-HS] Failed to check if beatsaber-http-status is installed: (Disconnect Reason: {msg.Type}) {ex}");
            }
        }

        internal static void HandleFile(HttpStatusObjects.Beatmap BeatmapInfo, HttpStatusObjects.Performance PerformanceInfo, string OldFileName, bool FinishedLast, bool FailedLast)
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

                    if (NewName.Contains("<misses>"))
                        NewName = NewName.Replace("<misses>", $"{PerformanceInfo.missedNotes}");
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

                    if (NewName.Contains("<misses>"))
                        NewName = NewName.Replace("<misses>", $"0");
                }

                if (Objects.LoadedSettings.DeleteIfShorterThan > OBSWebSocketObjects.RecordingSeconds)
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
                    if (BeatmapInfo.difficulty.ToLower() == "expertplus")
                        NewName = NewName.Replace("<difficulty>", "Expert+");
                    else
                        NewName = NewName.Replace("<difficulty>", BeatmapInfo.difficulty);
                }

                if (NewName.Contains("<short-difficulty>"))
                {
                    if (BeatmapInfo.difficulty.ToLower() == "expert")
                        NewName = NewName.Replace("<short-difficulty>", "EX");
                    else if (BeatmapInfo.difficulty.ToLower() == "expert+" || BeatmapInfo.difficulty.ToLower() == "expertplus")
                        NewName = NewName.Replace("<short-difficulty>", "EX+");
                    else
                        NewName = NewName.Replace("<short-difficulty>", BeatmapInfo.difficulty.Remove(1, BeatmapInfo.difficulty.Length - 1));
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

                    string FileExists = "";
                    int FileExistsCount = 2;

                    string NewFileName = $"{fileInfo.Directory.FullName}\\{NewName}{FileExist}{FileExists}{fileInfo.Extension}";

                    while (File.Exists(NewFileName))
                    {
                        FileExist = $" ({FileExistsCount})";
                        NewFileName = $"{fileInfo.Directory.FullName}\\{NewName}{FileExist}{FileExists}{fileInfo.Extension}";
                        FileExistsCount++;
                    }

                    try
                    {
                        if (!DeleteFile)
                        {
                            _logger.LogInfo($"[OBSC] Renaming \"{fileInfo.Name}\" to \"{NewName}{FileExists}{fileInfo.Extension}\"..");
                            File.Move(OldFileName, NewFileName);
                            _logger.LogInfo($"[OBSC] Successfully renamed.");
                            Program.SendNotification("Recording renamed.", 1000, Objects.MessageType.INFO);
                        }
                        else
                        {
                            _logger.LogInfo($"[OBSC] Deleting \"{fileInfo.Name}\"..");
                            File.Delete(OldFileName);
                            _logger.LogInfo($"[OBSC] Successfully deleted.");
                            Program.SendNotification("Recording deleted.", 1000, Objects.MessageType.INFO);
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
    }
}
