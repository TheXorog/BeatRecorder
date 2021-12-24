using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using Websocket.Client.Models;

namespace BeatRecorder
{
    class DataPuller
    {
        internal static void MapDataMessageRecieved(string e)
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

            if (DataPullerObjects.DataPullerInLevel != _status.InLevel)
            {
                if (!DataPullerObjects.DataPullerInLevel && _status.InLevel)
                {
                    DataPullerObjects.DataPullerInLevel = true;
                    _logger.LogDebug("[BS-DP1] Song started.");
                    _logger.LogInfo($"[BS-DP1] Started playing \"{_status.SongName}\" by \"{_status.SongAuthor}\"");

                    DataPullerObjects.DataPullerCurrentBeatmap = _status;

                    try
                    {
                        DataPullerObjects.CurrentSongCombo = 0;
                        _ = OBSWebSocket.StartRecording();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[BS-DP1] {ex}");
                        return;
                    }
                }
                else if (DataPullerObjects.DataPullerInLevel && !_status.InLevel)
                {
                    Thread.Sleep(500);
                    DataPullerObjects.DataPullerInLevel = false;
                    DataPullerObjects.DataPullerPaused = false;
                    _logger.LogDebug("[BS-DP1] Menu entered.");
                    _logger.LogInfo($"[BS-DP1] Stopped playing \"{_status.SongName}\" by \"{_status.SongAuthor}\"");

                    try
                    {
                        DataPullerObjects.DataPullerCurrentBeatmap = _status;

                        DataPullerObjects.DataPullerLastPerformance = DataPullerObjects.DataPullerCurrentPerformance;
                        DataPullerObjects.DataPullerLastBeatmap = DataPullerObjects.DataPullerCurrentBeatmap;
                        DataPullerObjects.LastSongCombo = DataPullerObjects.CurrentSongCombo;

                        _ = OBSWebSocket.StopRecording(OBSWebSocketObjects.CancelStopRecordingDelay.Token);
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
                if (DataPullerObjects.DataPullerPaused != _status.LevelPaused)
                {
                    if (!DataPullerObjects.DataPullerPaused && _status.LevelPaused)
                    {
                        DataPullerObjects.DataPullerPaused = true;
                        _logger.LogInfo("[BS-DP1] Song paused.");

                        try
                        {
                            if (Objects.LoadedSettings.PauseRecordingOnIngamePause)
                                if (Program.obsWebSocket.IsStarted)
                                    Program.obsWebSocket.Send($"{{\"request-type\":\"PauseRecording\", \"message-id\":\"PauseRecording\"}}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"[BS-DP1] {ex}");
                            return;
                        }
                    }
                    else if (DataPullerObjects.DataPullerPaused && !_status.LevelPaused)
                    {
                        DataPullerObjects.DataPullerPaused = false;
                        _logger.LogInfo("[BS-DP1] Song resumed.");

                        try
                        {
                            if (Objects.LoadedSettings.PauseRecordingOnIngamePause)
                                if (Program.obsWebSocket.IsStarted)
                                    Program.obsWebSocket.Send($"{{\"request-type\":\"ResumeRecording\", \"message-id\":\"ResumeRecording\"}}");
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

        internal static void LiveDataMessageRecieved(string e)
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

            if (DataPullerObjects.DataPullerInLevel)
                DataPullerObjects.DataPullerCurrentPerformance = _status;

            if (DataPullerObjects.CurrentSongCombo < _status.Combo)
                DataPullerObjects.CurrentSongCombo = _status.Combo;
        }

        internal static void MapDataReconnected(ReconnectionInfo msg)
        {
            Program.SendNotification("Connected to Beat Saber", 1000, Objects.MessageType.INFO);
            if (msg.Type != ReconnectionType.Initial)
            {
                _logger.LogWarn($"[BS-DP1] Reconnected: {msg.Type}");
                Objects.LastDP1Warning = Objects.ConnectionTypeWarning.CONNECTED;
            }
        }

        internal static void LiveDataReconnected(ReconnectionInfo msg)
        {
            if (msg.Type != ReconnectionType.Initial)
            {
                _logger.LogWarn($"[BS-DP2] Reconnected: {msg.Type}");
            }
        }

        internal static void MapDataDisconnected(DisconnectionInfo msg)
        {
            try
            {
                Process[] processCollection = Process.GetProcesses();

                if (!processCollection.Any(x => x.ProcessName.ToLower().StartsWith("beat")))
                {
                    if (Objects.LastDP1Warning != Objects.ConnectionTypeWarning.NO_PROCESS)
                    {
                        _logger.LogWarn($"[BS-DP1] Couldn't find a BeatSaber process, is BeatSaber started? ({msg.Type})");
                        Program.SendNotification("Couldn't connect to BeatSaber, is it even running?", 5000, Objects.MessageType.ERROR);
                    }
                    Objects.LastDP1Warning = Objects.ConnectionTypeWarning.NO_PROCESS;
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
                        if (Objects.LastDP1Warning != Objects.ConnectionTypeWarning.NOT_MODDED)
                        {
                            _logger.LogCritical($"[BS-DP1] Beat Saber seems to be running but the BSDataPuller modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install BSDataPuller: https://bit.ly/3mcvC7g) ({msg.Type})");
                            Program.SendNotification("Couldn't connect to Beat Saber. Have you modded your game?", 10000, Objects.MessageType.ERROR);
                        }
                        Objects.LastDP1Warning = Objects.ConnectionTypeWarning.NOT_MODDED;
                    }

                    if (FoundWebSocketDll)
                    {
                        if (Objects.LastDP1Warning != Objects.ConnectionTypeWarning.MOD_INSTALLED)
                        {
                            _logger.LogCritical($"[BS-DP1] Beat Saber seems to be running and the BSDataPuller modifaction seems to be installed. Please make sure you put in the right port and you installed all of BSDataPuller' dependiencies! (If not installed, please install it: https://bit.ly/3mcvC7g) ({msg.Type})");
                            Program.SendNotification("Couldn't connect to Beat Saber. Please make sure you selected the right port.", 10000, Objects.MessageType.ERROR);
                        }
                        Objects.LastDP1Warning = Objects.ConnectionTypeWarning.MOD_INSTALLED;
                    }
                    else
                    {
                        if (Objects.LastDP1Warning != Objects.ConnectionTypeWarning.MOD_NOT_INSTALLED)
                        {
                            _logger.LogCritical($"[BS-DP1] Beat Saber seems to be running but the BSDataPuller modifaction doesn't seem to be installed. Please make sure to install BSDataPuller! (If not installed, please install it: https://bit.ly/3mcvC7g) ({msg.Type})");
                            Program.SendNotification("Couldn't connect to Beat Saber. Please make sure DataPuller is installed.", 10000, Objects.MessageType.ERROR);
                        }
                        Objects.LastDP1Warning = Objects.ConnectionTypeWarning.MOD_NOT_INSTALLED;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[BS-DP1] Failed to check if BSDataPuller is installed: (Disconnect Reason: {msg.Type}) {ex}");
            }
        }

        internal static void LiveDataDisconnected(DisconnectionInfo msg)
        {
            if (Program.beatSaberWebSocket.IsRunning)
                _logger.LogError($"[BS-DP2] Disconnected: {msg.Type}");
            else
                _logger.LogDebug($"[BS-DP2] Disconnected: {msg.Type}");
        }

        internal static void HandleFile(DataPullerObjects.DataPullerMain BeatmapInfo, DataPullerObjects.DataPullerData PerformanceInfo, string OldFileName, int HighestCombo)
        {
            if (BeatmapInfo != null)
            {
                _logger.LogDebug($"[OBSC] BeatmapInfo: {JsonConvert.SerializeObject(BeatmapInfo)}");
                _logger.LogDebug($"[OBSC] PerformanceInfo: {JsonConvert.SerializeObject(PerformanceInfo)}");
                _logger.LogDebug($"[OBSC] OldFileName: {OldFileName}");
                _logger.LogDebug($"[OBSC] HighestCombo: {HighestCombo}");
                bool DeleteFile = false;
                string NewName = Objects.LoadedSettings.FileFormat;

                if (PerformanceInfo != null)
                {
                    // Generate FileName-based on Config File

                    if (NewName.Contains("<rank>"))
                    {
                        if (PerformanceInfo.Rank != "" && PerformanceInfo.Rank != null)
                            NewName = NewName.Replace("<rank>", PerformanceInfo.Rank);
                        else
                            NewName = NewName.Replace("<rank>", "E");
                    }

                    if (NewName.Contains("<accuracy>"))
                    {
                        string GeneratedAccuracy = "";

                        if ((BeatmapInfo.LevelFailed || PerformanceInfo.PlayerHealth <= 0) && BeatmapInfo.Modifiers.noFailOn0Energy)
                        {
                            _logger.LogDebug($"[OBSC] Soft-Failed.");
                            if (Objects.LoadedSettings.DeleteSoftFailed)
                            {
                                _logger.LogDebug($"[OBSC] Soft-Failed. Deletion requested.");
                                DeleteFile = true;
                            }

                            GeneratedAccuracy = $"NF-";
                        }

                        if (BeatmapInfo.LevelFinished)
                        {
                            _logger.LogDebug($"[OBSC] Level finished");
                            GeneratedAccuracy += $"{Math.Round(PerformanceInfo.Accuracy, 2)}";
                        }
                        else if (BeatmapInfo.LevelQuit)
                        {
                            _logger.LogDebug($"[OBSC] Level quit");
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
                        else if (BeatmapInfo.LevelFailed && !BeatmapInfo.Modifiers.noFailOn0Energy)
                        {
                            _logger.LogDebug($"[OBSC] Level failed.");
                            if (Objects.LoadedSettings.DeleteFailed)
                            {
                                _logger.LogDebug($"[OBSC] Failed. Deletion requested.");
                                DeleteFile = true;
                            }
                            else
                                DeleteFile = false;

                            GeneratedAccuracy = $"FAILED";
                        }
                        else
                        {
                            if (!BeatmapInfo.LevelQuit && !BeatmapInfo.LevelFinished)
                            {
                                _logger.LogDebug($"[OBSC] Level restarted");
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
                            else
                            {
                                _logger.LogDebug($"[OBSC] Level finished?");
                                GeneratedAccuracy += $"{Math.Round(PerformanceInfo.Accuracy, 2)}";
                            }
                        }

                        _logger.LogDebug($"[OBSC] {GeneratedAccuracy}");
                        NewName = NewName.Replace("<accuracy>", GeneratedAccuracy);
                    }

                    if (NewName.Contains("<max-combo>"))
                        NewName = NewName.Replace("<max-combo>", $"{HighestCombo}");

                    if (NewName.Contains("<score>"))
                        NewName = NewName.Replace("<score>", $"{PerformanceInfo.ScoreWithMultipliers}");

                    if (NewName.Contains("<raw-score>"))
                        NewName = NewName.Replace("<raw-score>", $"{PerformanceInfo.Score}");

                    if (NewName.Contains("<misses>"))
                        NewName = NewName.Replace("<misses>", $"{PerformanceInfo.Misses}");
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
                    NewName = NewName.Replace("<song-name>", BeatmapInfo.SongName);

                if (NewName.Contains("<song-author>"))
                    NewName = NewName.Replace("<song-author>", BeatmapInfo.SongAuthor);

                if (NewName.Contains("<song-sub-name>"))
                    NewName = NewName.Replace("<song-sub-name>", BeatmapInfo.SongSubName);

                if (NewName.Contains("<mapper>"))
                    NewName = NewName.Replace("<mapper>", BeatmapInfo.Mapper);

                if (NewName.Contains("<levelid>") && BeatmapInfo.Hash != null)
                    NewName = NewName.Replace("<levelid>", BeatmapInfo.Hash.ToString());

                if (NewName.Contains("<bpm>"))
                    NewName = NewName.Replace("<bpm>", BeatmapInfo.BPM.ToString());

                if (NewName.Contains("<difficulty>"))
                {
                    if (BeatmapInfo.Difficulty.ToLower() == "expertplus")
                        NewName = NewName.Replace("<difficulty>", "Expert+");
                    else
                        NewName = NewName.Replace("<difficulty>", BeatmapInfo.Difficulty);
                }

                if (NewName.Contains("<short-difficulty>"))
                {
                    if (BeatmapInfo.Difficulty.ToLower() == "expert")
                        NewName = NewName.Replace("<short-difficulty>", "EX");
                    else if (BeatmapInfo.Difficulty.ToLower() == "expert+" || BeatmapInfo.Difficulty.ToLower() == "expertplus")
                        NewName = NewName.Replace("<short-difficulty>", "EX+");
                    else
                        NewName = NewName.Replace("<short-difficulty>", BeatmapInfo.Difficulty.Remove(1, BeatmapInfo.Difficulty.Length - 1));
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
