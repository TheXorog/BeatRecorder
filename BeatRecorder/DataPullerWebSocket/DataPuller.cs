namespace BeatRecorder;

class DataPuller
{
    internal static void MapDataMessageRecieved(string e)
    {
        DataPullerStatus.DataPullerMain _status;

        try
        {
            _status = JsonConvert.DeserializeObject<DataPullerStatus.DataPullerMain>(e);
        }
        catch (Exception ex)
        {
            LogFatal($"[BS-DP1] Unable to convert BSDataPuller message into an dictionary: {ex}");
            return;
        }

        if (DataPullerStatus.DataPullerInLevel != _status.InLevel)
        {
            if (!DataPullerStatus.DataPullerInLevel && _status.InLevel)
            {
                DataPullerStatus.DataPullerInLevel = true;
                LogDebug("[BS-DP1] Song started.");
                LogInfo($"[BS-DP1] Started playing \"{_status.SongName}\" by \"{_status.SongAuthor}\"");

                DataPullerStatus.DataPullerCurrentBeatmap = _status;

                try
                {
                    DataPullerStatus.CurrentSongCombo = 0;
                    _ = OBSWebSocket.StartRecording();
                }
                catch (Exception ex)
                {
                    LogError($"[BS-DP1] {ex}");
                    return;
                }

                try
                {
                    if (Program.LoadedSettings.OBSIngameScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSIngameScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    LogError($"[BS-DP1] {ex}");
                    return;
                }
            }
            else if (DataPullerStatus.DataPullerInLevel && !_status.InLevel)
            {
                Thread.Sleep(500);
                DataPullerStatus.DataPullerInLevel = false;
                DataPullerStatus.DataPullerPaused = false;
                LogDebug("[BS-DP1] Menu entered.");
                LogInfo($"[BS-DP1] Stopped playing \"{_status.SongName}\" by \"{_status.SongAuthor}\"");

                try
                {
                    DataPullerStatus.DataPullerCurrentBeatmap = _status;

                    DataPullerStatus.DataPullerLastPerformance = DataPullerStatus.DataPullerCurrentPerformance;
                    DataPullerStatus.DataPullerLastBeatmap = DataPullerStatus.DataPullerCurrentBeatmap;
                    DataPullerStatus.LastSongCombo = DataPullerStatus.CurrentSongCombo;

                    _ = OBSWebSocket.StopRecording(OBSWebSocketStatus.CancelStopRecordingDelay.Token);
                }
                catch (Exception ex)
                {
                    LogError($"[BS-DP1] {ex}");
                    return;
                }

                try
                {
                    if (Program.LoadedSettings.OBSMenuScene != "")
                        Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSMenuScene}\", \"message-id\":\"PauseRecording\"}}");
                }
                catch (Exception ex)
                {
                    LogError($"[BS-DP1] {ex}");
                    return;
                }
            }
        }

        if (_status.InLevel)
        {
            if (DataPullerStatus.DataPullerPaused != _status.LevelPaused)
            {
                if (!DataPullerStatus.DataPullerPaused && _status.LevelPaused)
                {
                    DataPullerStatus.DataPullerPaused = true;
                    LogInfo("[BS-DP1] Song paused.");

                    try
                    {
                        if (Program.LoadedSettings.PauseRecordingOnIngamePause)
                            if (Program.obsWebSocket.IsStarted)
                                Program.obsWebSocket.Send($"{{\"request-type\":\"PauseRecording\", \"message-id\":\"PauseRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        LogError($"[BS-DP1] {ex}");
                        return;
                    }

                    try
                    {
                        if (Program.LoadedSettings.OBSPauseScene != "")
                            Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSPauseScene}\", \"message-id\":\"PauseRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        LogError($"[BS-DP1] {ex}");
                        return;
                    }
                }
                else if (DataPullerStatus.DataPullerPaused && !_status.LevelPaused)
                {
                    DataPullerStatus.DataPullerPaused = false;
                    LogInfo("[BS-DP1] Song resumed.");

                    try
                    {
                        if (Program.LoadedSettings.PauseRecordingOnIngamePause)
                            if (Program.obsWebSocket.IsStarted)
                                Program.obsWebSocket.Send($"{{\"request-type\":\"ResumeRecording\", \"message-id\":\"ResumeRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        LogError($"[BS-DP1] {ex}");
                        return;
                    }

                    try
                    {
                        if (Program.LoadedSettings.OBSIngameScene != "")
                            Program.obsWebSocket.Send($"{{\"request-type\":\"SetCurrentScene\", \"scene-name\":\"{Program.LoadedSettings.OBSIngameScene}\", \"message-id\":\"PauseRecording\"}}");
                    }
                    catch (Exception ex)
                    {
                        LogError($"[BS-DP1] {ex}");
                        return;
                    }
                }
            }
        }
    }

    internal static void LiveDataMessageRecieved(string e)
    {
        DataPullerStatus.DataPullerData _status;

        try
        {
            _status = JsonConvert.DeserializeObject<DataPullerStatus.DataPullerData>(e);
        }
        catch (Exception ex)
        {
            LogFatal($"[BS-DP2] Unable to convert BSDataPuller message into an dictionary: {ex}");
            return;
        }

        if (DataPullerStatus.DataPullerInLevel)
            DataPullerStatus.DataPullerCurrentPerformance = _status;

        if (DataPullerStatus.CurrentSongCombo < _status.Combo)
            DataPullerStatus.CurrentSongCombo = _status.Combo;
    }

    internal static void MapDataReconnected(ReconnectionInfo msg)
    {
        Program.SendNotification("Connected to Beat Saber", 1000, MessageType.INFO);
        if (msg.Type != ReconnectionType.Initial)
        {
            LogWarn($"[BS-DP1] Reconnected: {msg.Type}");
            Objects.LastDP1Warning = ConnectionTypeWarning.CONNECTED;
        }
    }

    internal static void LiveDataReconnected(ReconnectionInfo msg)
    {
        if (msg.Type != ReconnectionType.Initial)
        {
            LogWarn($"[BS-DP2] Reconnected: {msg.Type}");
        }
    }

    internal static void MapDataDisconnected(DisconnectionInfo msg)
    {
        try
        {
            Process[] processCollection = Process.GetProcesses();

            if (!processCollection.Any(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")))
            {
                if (Objects.LastDP1Warning != ConnectionTypeWarning.NO_PROCESS)
                {
                    LogWarn($"[BS-DP1] Couldn't find a BeatSaber process, is BeatSaber started? ({msg.Type})");
                    Program.SendNotification("Couldn't connect to BeatSaber, is it even running?", 5000, MessageType.ERROR);
                }
                Objects.LastDP1Warning = ConnectionTypeWarning.NO_PROCESS;
            }
            else
            {
                bool FoundWebSocketDll = false;

                string InstallationDirectory = processCollection.First(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")).MainModule.FileName;
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
                    if (Objects.LastDP1Warning != ConnectionTypeWarning.NOT_MODDED)
                    {
                        LogFatal($"[BS-DP1] Beat Saber seems to be running but the BSDataPuller modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install BSDataPuller: https://bit.ly/3mcvC7g) ({msg.Type})");
                        Program.SendNotification("Couldn't connect to Beat Saber. Have you modded your game?", 10000, MessageType.ERROR);
                    }
                    Objects.LastDP1Warning = ConnectionTypeWarning.NOT_MODDED;
                }

                if (FoundWebSocketDll)
                {
                    if (Objects.LastDP1Warning != ConnectionTypeWarning.MOD_INSTALLED)
                    {
                        LogFatal($"[BS-DP1] Beat Saber seems to be running and the BSDataPuller modifaction seems to be installed. Please make sure you put in the right port and you installed all of BSDataPuller' dependiencies! (If not installed, please install it: https://bit.ly/3mcvC7g) ({msg.Type})");
                        Program.SendNotification("Couldn't connect to Beat Saber. Please make sure you selected the right port.", 10000, MessageType.ERROR);
                    }
                    Objects.LastDP1Warning = ConnectionTypeWarning.MOD_INSTALLED;
                }
                else
                {
                    if (Objects.LastDP1Warning != ConnectionTypeWarning.MOD_NOT_INSTALLED)
                    {
                        LogFatal($"[BS-DP1] Beat Saber seems to be running but the BSDataPuller modifaction doesn't seem to be installed. Please make sure to install BSDataPuller! (If not installed, please install it: https://bit.ly/3mcvC7g) ({msg.Type})");
                        Program.SendNotification("Couldn't connect to Beat Saber. Please make sure DataPuller is installed.", 10000, MessageType.ERROR);
                    }
                    Objects.LastDP1Warning = ConnectionTypeWarning.MOD_NOT_INSTALLED;
                }
            }
        }
        catch (Exception ex)
        {
            LogError($"[BS-DP1] Failed to check if BSDataPuller is installed: (Disconnect Reason: {msg.Type}) {ex}");
        }
    }

    internal static void LiveDataDisconnected(DisconnectionInfo msg)
    {
        if (Program.beatSaberWebSocket.IsRunning)
            LogError($"[BS-DP2] Disconnected: {msg.Type}");
        else
            LogDebug($"[BS-DP2] Disconnected: {msg.Type}");
    }

    internal static void HandleFile(DataPullerStatus.DataPullerMain BeatmapInfo, DataPullerStatus.DataPullerData PerformanceInfo, string OldFileName, int HighestCombo)
    {
        if (BeatmapInfo != null)
        {
            LogDebug($"[BR] BeatmapInfo: {JsonConvert.SerializeObject(BeatmapInfo)}");
            LogDebug($"[BR] PerformanceInfo: {JsonConvert.SerializeObject(PerformanceInfo)}");
            LogDebug($"[BR] OldFileName: {OldFileName}");
            LogDebug($"[BR] HighestCombo: {HighestCombo}");
            bool DeleteFile = false;
            string NewName = Program.LoadedSettings.FileFormat;

            if (PerformanceInfo != null)
            {
                // Generate FileName-based on Config File

                if (NewName.Contains("<rank>"))
                {
                    NewName = PerformanceInfo.Rank is not "" and not null
                        ? NewName.Replace("<rank>", PerformanceInfo.Rank)
                        : NewName.Replace("<rank>", "E");
                }

                if (NewName.Contains("<accuracy>"))
                {
                    string GeneratedAccuracy = "";

                    if ((BeatmapInfo.LevelFailed || PerformanceInfo.PlayerHealth <= 0) && BeatmapInfo.Modifiers.noFailOn0Energy)
                    {
                        LogDebug($"[BR] Soft-Failed.");
                        if (Program.LoadedSettings.DeleteSoftFailed)
                        {
                            LogDebug($"[BR] Soft-Failed. Deletion requested.");
                            DeleteFile = true;
                        }

                        GeneratedAccuracy = $"NF-";
                    }

                    if (BeatmapInfo.LevelFinished)
                    {
                        LogDebug($"[BR] Level finished");
                        GeneratedAccuracy += $"{Math.Round(PerformanceInfo.Accuracy, 2)}";
                    }
                    else if (BeatmapInfo.LevelQuit)
                    {
                        LogDebug($"[BR] Level quit");
                        if (Program.LoadedSettings.DeleteQuit)
                        {
                            LogDebug($"[BR] Quit. Deletion requested.");
                            DeleteFile = true;

                            if (GeneratedAccuracy == "NF-")
                                if (!Program.LoadedSettings.DeleteIfQuitAfterSoftFailed)
                                {
                                    LogDebug($"[BR] Soft-Failed but quit, deletion request reverted.");
                                    DeleteFile = false;
                                }
                        }

                        GeneratedAccuracy += $"QUIT";
                    }
                    else if (BeatmapInfo.LevelFailed && !BeatmapInfo.Modifiers.noFailOn0Energy)
                    {
                        LogDebug($"[BR] Level failed.");
                        if (Program.LoadedSettings.DeleteFailed)
                        {
                            LogDebug($"[BR] Failed. Deletion requested.");
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
                            LogDebug($"[BR] Level restarted");
                            if (Program.LoadedSettings.DeleteQuit)
                            {
                                LogDebug($"[BR] Quit. Deletion requested.");
                                DeleteFile = true;

                                if (GeneratedAccuracy == "NF-")
                                    if (!Program.LoadedSettings.DeleteIfQuitAfterSoftFailed)
                                    {
                                        LogDebug($"[BR] Soft-Failed but quit, deletion request reverted.");
                                        DeleteFile = false;
                                    }
                            }

                            GeneratedAccuracy += $"QUIT";
                        }
                        else
                        {
                            LogDebug($"[BR] Level finished?");
                            GeneratedAccuracy += $"{Math.Round(PerformanceInfo.Accuracy, 2)}";
                        }
                    }

                    LogDebug($"[BR] {GeneratedAccuracy}");
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

            if (Program.LoadedSettings.DeleteIfShorterThan > OBSWebSocketStatus.RecordingSeconds)
            {
                LogDebug($"[BR] The recording is too short. Deletion requested.");
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
                NewName = BeatmapInfo.Difficulty.ToLower() switch
                {
                    "expertplus" => NewName.Replace("<difficulty>", "Expert+"),
                    _ => NewName.Replace("<difficulty>", BeatmapInfo.Difficulty),
                };
            }

            if (NewName.Contains("<short-difficulty>"))
            {
                NewName = BeatmapInfo.Difficulty.ToLower() switch
                {
                    "expert" => NewName.Replace("<short-difficulty>", "EX"),
                    "expert+" or "expertplus" => NewName.Replace("<short-difficulty>", "EX+"),
                    _ => NewName.Replace("<short-difficulty>", BeatmapInfo.Difficulty.Remove(1, BeatmapInfo.Difficulty.Length - 1)),
                };
            }

            if (File.Exists($"{OldFileName}"))
            {

                string FileExist = "";

                FileInfo fileInfo = new(OldFileName);

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
                        LogInfo($"[BR] Renaming \"{fileInfo.Name}\" to \"{NewName}{FileExists}{fileInfo.Extension}\"..");
                        File.Move(OldFileName, NewFileName);
                        LogInfo($"[BR] Successfully renamed.");
                        Program.SendNotification("Recording renamed.", 1000, MessageType.INFO);
                    }
                    else
                    {
                        LogInfo($"[BR] Deleting \"{fileInfo.Name}\"..");
                        File.Delete(OldFileName);
                        LogInfo($"[BR] Successfully deleted.");
                        Program.SendNotification("Recording deleted.", 1000, MessageType.INFO);
                    }
                }
                catch (Exception ex)
                {
                    LogError($"[BR] {ex}.");
                }
            }
            else
            {
                LogError($"[BR] {OldFileName} doesn't exist.");
            }
        }
        else
        {
            LogError($"[BR] Last recorded file can't be renamed.");
        }
    }
}
