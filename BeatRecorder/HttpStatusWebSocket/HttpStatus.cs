namespace BeatRecorder;

class HttpStatus
{
    

    internal static void HandleFile(HttpStatusStatus.Beatmap BeatmapInfo, HttpStatusStatus.Performance PerformanceInfo, string OldFileName, bool FinishedLast, bool FailedLast)
    {
        if (BeatmapInfo != null)
        {
            bool DeleteFile = false;
            string NewName = Program.LoadedSettings.FileFormat;

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
                        if (Program.LoadedSettings.DeleteSoftFailed)
                        {
                            _logger.LogDebug($"[BR] Soft-Failed. Deletion requested.");
                            DeleteFile = true;
                        }

                        GeneratedAccuracy = $"NF-";
                    }

                    if (FinishedLast)
                        GeneratedAccuracy += $"{Math.Round((float)(((float)PerformanceInfo.score * (float)100) / (float)BeatmapInfo.maxScore), 2)}";
                    else
                    {
                        if (Program.LoadedSettings.DeleteQuit)
                        {
                            _logger.LogDebug($"[BR] Quit. Deletion requested.");
                            DeleteFile = true;

                            if (GeneratedAccuracy == "NF-")
                                if (!Program.LoadedSettings.DeleteIfQuitAfterSoftFailed)
                                {
                                    _logger.LogDebug($"[BR] Soft-Failed but quit, deletion request reverted.");
                                    DeleteFile = false;
                                }
                        }

                        GeneratedAccuracy += $"QUIT";
                    }

                    if (FailedLast)
                    {
                        if (Program.LoadedSettings.DeleteFailed)
                        {
                            _logger.LogDebug($"[BR] Failed. Deletion requested.");
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

            if (Program.LoadedSettings.DeleteIfShorterThan > OBSWebSocketStatus.RecordingSeconds)
            {
                _logger.LogDebug($"[BR] The recording is too short. Deletion requested.");
                DeleteFile = true;
            }

            if (NewName.Contains("<song-name>"))
                NewName = NewName.Replace("<song-name>", BeatmapInfo.songName);
            
            if (NewName.Contains("<song-name-with-sub>"))
                NewName = NewName.Replace("<song-name-with-sub>", $"{BeatmapInfo.songName}{(!string.IsNullOrWhiteSpace(BeatmapInfo.songSubName) ? $" {BeatmapInfo.songSubName}" : "")}");

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
                switch (BeatmapInfo.difficulty.ToLower())
                {
                    case "expertplus":
                        NewName = NewName.Replace("<difficulty>", "Expert+");
                        break;
                    default:
                        NewName = NewName.Replace("<difficulty>", BeatmapInfo.difficulty);
                        break;
                }
            }

            if (NewName.Contains("<short-difficulty>"))
            {
                switch (BeatmapInfo.difficulty.ToLower())
                {
                    case "expert":
                        NewName = NewName.Replace("<short-difficulty>", "EX");
                        break;
                    case "expert+":
                    case "expertplus":
                        NewName = NewName.Replace("<short-difficulty>", "EX+");
                        break;
                    default:
                        NewName = NewName.Replace("<short-difficulty>", BeatmapInfo.difficulty.Remove(1, BeatmapInfo.difficulty.Length - 1));
                        break;
                }
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
                        _logger.LogInfo($"[BR] Renaming \"{fileInfo.Name}\" to \"{NewName}{FileExists}{fileInfo.Extension}\"..");
                        File.Move(OldFileName, NewFileName);
                        _logger.LogInfo($"[BR] Successfully renamed.");
                        Program.SendNotification("Recording renamed.", 1000, MessageType.INFO);
                    }
                    else
                    {
                        _logger.LogInfo($"[BR] Deleting \"{fileInfo.Name}\"..");
                        File.Delete(OldFileName);
                        _logger.LogInfo($"[BR] Successfully deleted.");
                        Program.SendNotification("Recording deleted.", 1000, MessageType.INFO);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[BR] {ex}.");
                }
            }
            else
            {
                _logger.LogError($"[BR] {OldFileName} doesn't exist.");
            }
        }
        else
        {
            _logger.LogError($"[BR] Last recorded file can't be renamed.");
        }
    }
}
