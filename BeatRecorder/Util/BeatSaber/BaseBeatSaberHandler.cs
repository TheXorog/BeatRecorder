using BeatRecorder.Entities;

namespace BeatRecorder.Util.BeatSaber;

internal abstract class BaseBeatSaberHandler
{
    public abstract BaseBeatSaberHandler Initialize(Program program);
    public abstract SharedStatus GetCurrentStatus();
    public abstract SharedStatus GetLastCompletedStatus();

    public void HandleFile(string fileName, long RecordingLength, SharedStatus sharedStatus, Config loadedConfig)
    {
        if (sharedStatus is null)
        {
            _logger.LogError($"Last completed status is null.");
            return;
        }

        _logger.LogTrace(fileName);
        _logger.LogTrace(RecordingLength.ToString());
        _logger.LogTrace(JsonConvert.SerializeObject(sharedStatus));

        bool DeleteFile = false;
        string NewName = loadedConfig.FileFormat;

        string GeneratedAccuracy = "";

        if (sharedStatus.PerformanceInfo.SoftFailed.Value)
        {
            if (loadedConfig.DeleteSoftFailed)
            {
                _logger.LogDebug("Song Soft-Failed. Deletion requested");
                DeleteFile = true;
            }

            GeneratedAccuracy = $"NF-";
        }

        if (sharedStatus.PerformanceInfo.Finished.Value)
            GeneratedAccuracy += sharedStatus.PerformanceInfo.Accuracy.ToString();
        else
        {
            if (loadedConfig.DeleteIfQuitAfterSoftFailed)
            {
                _logger.LogDebug("Song Quit. Deletion requested");
                DeleteFile = true;

                if (GeneratedAccuracy == "NF-")
                    if (!loadedConfig.DeleteIfQuitAfterSoftFailed)
                    {
                        _logger.LogDebug($"Song Soft-Failed but quit, deletion request reverted.");
                        DeleteFile = false;
                    }
            }

            GeneratedAccuracy += $"QUIT";
        }

        if (sharedStatus.PerformanceInfo.Failed.Value)
        {
            if (loadedConfig.DeleteFailed)
            {
                _logger.LogDebug("Song failed. Deletion requested");
                DeleteFile = true;
            }
            else
                DeleteFile = false;

            GeneratedAccuracy = $"FAILED";
        }

        if (loadedConfig.DeleteIfShorterThan > RecordingLength)
        {
            _logger.LogDebug("Recording too short. Deletion requested");
            DeleteFile = true;
        }

        var ShortDifficulty = sharedStatus.BeatmapInfo.Difficulty.ToLower() switch
        {
            "expert" => "EX",
            "expert+" or "expertplus" => "EX+",
            _ => sharedStatus.BeatmapInfo.Difficulty.Truncate(1),
        };

        var missesText = sharedStatus.PerformanceInfo?.CombinedMisses?.ToString() ?? "0";

        NewName = NewName.Replace("<rank>", sharedStatus.PerformanceInfo.Rank ?? "Z");
        NewName = NewName.Replace("<accuracy>", (GeneratedAccuracy.IsNullOrWhiteSpace() ? "Z" : GeneratedAccuracy));
        NewName = NewName.Replace("<max-combo>", sharedStatus.PerformanceInfo?.MaxCombo?.ToString() ?? "0");
        NewName = NewName.Replace("<score>", sharedStatus.PerformanceInfo?.Score?.ToString() ?? "0");
        NewName = NewName.Replace("<raw-score>", sharedStatus.PerformanceInfo?.RawScore?.ToString() ?? "0");
        NewName = NewName.Replace("<misses>", (missesText == "0" ? "FC" : missesText));

        NewName = NewName.Replace("<song-name>", sharedStatus.BeatmapInfo?.Name ?? "Unknown");
        NewName = NewName.Replace("<song-sub-name>", sharedStatus.BeatmapInfo?.SubName ?? "Unknown");
        NewName = NewName.Replace("<song-name-with-sub>", sharedStatus.BeatmapInfo?.NameWithSub ?? "Unknown");
        NewName = NewName.Replace("<song-author>", sharedStatus.BeatmapInfo?.Author ?? "Unknown");
        NewName = NewName.Replace("<mapper>", sharedStatus.BeatmapInfo?.Creator ?? "Unknown");
        NewName = NewName.Replace("<levelid>", sharedStatus.BeatmapInfo?.IdOrHash ?? "Unknown");
        NewName = NewName.Replace("<bpm>", sharedStatus.BeatmapInfo?.Bpm?.ToString() ?? "Unknown");
        NewName = NewName.Replace("<difficulty>", sharedStatus.BeatmapInfo?.Difficulty ?? "Unknown");
        NewName = NewName.Replace("<short-difficulty>", ShortDifficulty ?? "Unknown");

        if (!File.Exists(fileName))
        {
            _logger.LogError($"{fileName} does not exist.");
            return;
        }

        foreach (char b in Path.GetInvalidFileNameChars())
        {
            NewName = NewName.Replace(b, '_');
        }

        string FileExist = "";
        int FileExistCount = 2;

        FileInfo fileInfo = new(fileName);

        while (File.Exists($"{fileInfo.Directory.FullName}\\{NewName}{FileExist}{fileInfo.Extension}"))
        {
            FileExist = $" ({FileExistCount})";
            FileExistCount++;
        }

        string NewFileName = $"{fileInfo.Directory.FullName}\\{NewName}{FileExist}{fileInfo.Extension}";

        try
        {
            if (DeleteFile)
            {
                File.Delete(fileName);
                _logger.LogInfo("Recording deleted");
            }
            else
            {
                File.Move(fileName, NewFileName);
                _logger.LogInfo("Recording renamed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to rename or delete '{fileName}'", ex);
        }
    }
}
