﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBSControl
{
    class HttpStatus
    {
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

                if (Objects.LoadedSettings.DeleteIfShorterThan + Objects.LoadedSettings.StopRecordingDelay > OBSWebSocketObjects.RecordingSeconds)
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
    }
}
