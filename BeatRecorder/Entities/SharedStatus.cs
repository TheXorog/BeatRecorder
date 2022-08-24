namespace BeatRecorder.Entities;

internal class SharedStatus
{
    internal SharedStatus(HttpStatus.Status status)
    {
        GameInfo = new()
        {
            ModUsed = Mod.HttpStatus,
            ModVersion = status.game.gameVersion,
            GameVersion = status.game.pluginVersion
        };

        BeatmapInfo = new()
        {
            Name = status.beatmap.songName,
            SubName = status.beatmap.songSubName,
            Author = status.beatmap.songAuthorName,
            Creator = status.beatmap.levelAuthorName,
            Cover = (Bitmap)Bitmap.FromStream(new MemoryStream(Convert.FromBase64String(status.beatmap.songCover))),
            IdOrHash = status.beatmap.levelId,
            Bpm = status.beatmap.songBPM,
            NoteJumpSpeed = status.beatmap.noteJumpSpeed,
            Difficulty = status.beatmap.difficulty,
            BombCount = status.beatmap.bombsCount,
            NoteCount = status.beatmap.notesCount,
            WallCount = status.beatmap.obstaclesCount
        };

        PerformanceInfo = new()
        {
            RawScore = status.performance.rawScore,
            Score = status.performance.score,
            Accuracy = Math.Round((double)((status.performance.score * 100) / status.beatmap.maxScore), 2),
            Rank = status.performance.rank,
            MissedNoteCount = status.performance.passedNotes,
            BadCutCount = status.performance.missedNotes,
            BombHitCount = status.performance.hitBombs,
            Failed = status.performance.failed,
            Finished = status.performance.finished,
            MaxCombo = status.performance.maxCombo,
            SoftFailed = status.performance.softFailed
        };
    }

    private SharedStatus() { }

    public void Update(SharedStatus newStatus)
    {
        if (GameInfo.ModUsed != newStatus.GameInfo.ModUsed)
            throw new InvalidOperationException("Mod used cannot be updated.");

        GameInfo.ModVersion = newStatus.GameInfo.ModVersion ?? GameInfo.ModVersion;
        GameInfo.GameVersion = newStatus.GameInfo.GameVersion ?? GameInfo.GameVersion;

        BeatmapInfo.Name = newStatus.BeatmapInfo.Name ?? BeatmapInfo.Name;
        BeatmapInfo.SubName = newStatus.BeatmapInfo.SubName ?? BeatmapInfo.SubName;
        BeatmapInfo.Author = newStatus.BeatmapInfo.Author ?? BeatmapInfo.Author;
        BeatmapInfo.Creator = newStatus.BeatmapInfo.Creator ?? BeatmapInfo.Creator;
        BeatmapInfo.Cover = newStatus.BeatmapInfo.Cover ?? BeatmapInfo.Cover;
        BeatmapInfo.Bpm = newStatus.BeatmapInfo.Bpm ?? BeatmapInfo.Bpm;
        BeatmapInfo.NoteJumpSpeed = newStatus.BeatmapInfo.NoteJumpSpeed ?? BeatmapInfo.NoteJumpSpeed;
        BeatmapInfo.Difficulty = newStatus.BeatmapInfo.Difficulty ?? BeatmapInfo.Difficulty;
        BeatmapInfo.NoteCount = newStatus.BeatmapInfo.NoteCount ?? BeatmapInfo.NoteCount;
        BeatmapInfo.BombCount = newStatus.BeatmapInfo.BombCount ?? BeatmapInfo.BombCount;
        BeatmapInfo.WallCount = newStatus.BeatmapInfo.WallCount ?? BeatmapInfo.WallCount;
        BeatmapInfo.CustomDifficulty = newStatus.BeatmapInfo.CustomDifficulty ?? BeatmapInfo.CustomDifficulty;

        PerformanceInfo.RawScore = newStatus.PerformanceInfo.RawScore ?? PerformanceInfo.RawScore;
        PerformanceInfo.Score = newStatus.PerformanceInfo.Score ?? PerformanceInfo.Score;
        PerformanceInfo.Accuracy = newStatus.PerformanceInfo.Accuracy ?? PerformanceInfo.Accuracy;
        PerformanceInfo.Rank = newStatus.PerformanceInfo.Rank ?? PerformanceInfo.Rank;
        PerformanceInfo.MissedNoteCount = newStatus.PerformanceInfo.MissedNoteCount ?? PerformanceInfo.MissedNoteCount;
        PerformanceInfo.BadCutCount = newStatus.PerformanceInfo.BadCutCount ?? PerformanceInfo.BadCutCount;
        PerformanceInfo.BombHitCount = newStatus.PerformanceInfo.BombHitCount ?? PerformanceInfo.BombHitCount;
        PerformanceInfo.MaxCombo = newStatus.PerformanceInfo.MaxCombo ?? PerformanceInfo.MaxCombo;
        PerformanceInfo.SoftFailed = newStatus.PerformanceInfo.SoftFailed ?? PerformanceInfo.SoftFailed;
        PerformanceInfo.Failed = newStatus.PerformanceInfo.Failed ?? PerformanceInfo.Failed;
        PerformanceInfo.Finished = newStatus.PerformanceInfo.Finished ?? PerformanceInfo.Finished;
    }

    public SharedStatus Clone()
    {
        var newStatus = new SharedStatus();

        newStatus.GameInfo.ModUsed = GameInfo.ModUsed;
        newStatus.GameInfo.ModVersion = GameInfo.ModVersion;
        newStatus.GameInfo.GameVersion = GameInfo.GameVersion;

        newStatus.BeatmapInfo.Name = BeatmapInfo.Name;
        newStatus.BeatmapInfo.SubName = BeatmapInfo.SubName;
        newStatus.BeatmapInfo.Author = BeatmapInfo.Author;
        newStatus.BeatmapInfo.Creator = BeatmapInfo.Creator;
        newStatus.BeatmapInfo.Cover = BeatmapInfo.Cover;
        newStatus.BeatmapInfo.Bpm = BeatmapInfo.Bpm;
        newStatus.BeatmapInfo.NoteJumpSpeed = BeatmapInfo.NoteJumpSpeed;
        newStatus.BeatmapInfo.Difficulty = BeatmapInfo.Difficulty;
        newStatus.BeatmapInfo.NoteCount = BeatmapInfo.NoteCount;
        newStatus.BeatmapInfo.BombCount = BeatmapInfo.BombCount;
        newStatus.BeatmapInfo.WallCount = BeatmapInfo.WallCount;
        newStatus.BeatmapInfo.CustomDifficulty = BeatmapInfo.CustomDifficulty;

        newStatus.PerformanceInfo.RawScore = PerformanceInfo.RawScore;
        newStatus.PerformanceInfo.Score = PerformanceInfo.Score;
        newStatus.PerformanceInfo.Accuracy = PerformanceInfo.Accuracy;
        newStatus.PerformanceInfo.Rank = PerformanceInfo.Rank;
        newStatus.PerformanceInfo.MissedNoteCount = PerformanceInfo.MissedNoteCount;
        newStatus.PerformanceInfo.BadCutCount = PerformanceInfo.BadCutCount;
        newStatus.PerformanceInfo.BombHitCount = PerformanceInfo.BombHitCount;
        newStatus.PerformanceInfo.MaxCombo = PerformanceInfo.MaxCombo;
        newStatus.PerformanceInfo.SoftFailed = PerformanceInfo.SoftFailed;
        newStatus.PerformanceInfo.Failed = PerformanceInfo.Failed;
        newStatus.PerformanceInfo.Finished = PerformanceInfo.Finished;
        return newStatus;
    }

    internal Game GameInfo { get; set; }
    internal Beatmap BeatmapInfo { get; set; }
    internal Performance PerformanceInfo { get; set; }

    internal class Game
    {
        internal Mod ModUsed { get; set; }
        internal string ModVersion { get; set; }
        internal string GameVersion { get; set; }
    }

    internal class Beatmap
    {
        internal string Name { get; set; }
        internal string SubName { get; set; }
        internal string NameWithSub => $"{Name}{(SubName.IsNullOrWhiteSpace() ? "" : $" {SubName}")}";
        internal string Author { get; set; }
        internal string Creator { get; set; }
        internal Bitmap Cover { get; set; }
        internal string IdOrHash { get; set; }
        internal float? Bpm { get; set; }
        internal float? NoteJumpSpeed { get; set; }
        internal string Difficulty { get; set; }
        internal string CustomDifficulty { get => (CustomDifficulty.IsNullOrWhiteSpace() ? Difficulty : _CustomDifficulty); set { _CustomDifficulty = value; } }
        internal long? NoteCount { get; set; }
        internal long? BombCount { get; set; }
        internal long? WallCount { get; set; }

        private string _CustomDifficulty { get; set; }
    }

    internal class Performance
    {
        internal long? RawScore { get; set; }
        internal long? Score { get; set; }
        internal double? Accuracy { get; set; }
        internal string Rank { get; set; }
        internal long? MissedNoteCount { get; set; } = 0;
        internal long? BadCutCount { get; set; } = 0;
        internal long? BombHitCount { get; set; } = 0;
        internal long? CombinedMisses { get => MissedNoteCount + BadCutCount + BombHitCount; }
        internal long? MaxCombo { get; set; }
        internal bool? SoftFailed { get; set; }
        internal bool? Failed { get; set; }
        internal bool? Finished { get; set; }
    }
}
