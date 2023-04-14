using BeatRecorder.Enums;
using BeatRecorder.Util.BeatSaber;

namespace BeatRecorder.Entities;

public class SharedStatus
{
    public SharedStatus(HttpStatus.Status status, BaseBeatSaberHandler baseBeatSaberHandler)
    {
        GameInfo = new()
        {
            ModUsed = Mod.HttpStatus,
            ModVersion = status.game?.pluginVersion,
            GameVersion = status.game?.gameVersion,
        };

        this.BeatmapInfo = new()
        {
            Name = status.beatmap?.songName,
            SubName = status.beatmap?.songSubName,
            Author = status.beatmap?.songAuthorName,
            Creator = status.beatmap?.levelAuthorName,
            IdOrHash = status.beatmap?.levelId,
            Bpm = status.beatmap?.songBPM,
            NoteJumpSpeed = status.beatmap?.noteJumpSpeed,
            Difficulty = status.beatmap?.difficulty,
            BombCount = status.beatmap?.bombsCount,
            NoteCount = status.beatmap?.notesCount,
            WallCount = status.beatmap?.obstaclesCount
        };

        double? v = null;

        try
        {
            v = Math.Round((double)((status.performance?.score * 100) / status.beatmap?.maxScore), 2);
        }
        catch { }

        this.PerformanceInfo = new()
        {
            RawScore = status.performance?.rawScore,
            Score = status.performance?.score,
            Accuracy = v ?? 00.00,
            Rank = status.performance?.rank,
            MissedNoteCount = status.performance?.passedNotes,
            BadCutCount = status.performance?.missedNotes,
            BombHitCount = status.performance?.hitBombs,
            Failed = status.performance?.failed,
            Finished = status.performance?.finished,
            MaxCombo = status.performance?.maxCombo,
            Combo = status.performance?.combo,
            SoftFailed = status.performance?.softFailed
        };
    }
    
    public SharedStatus(BeatSaberPlus status, Game game, int MaxCombo, BaseBeatSaberHandler baseBeatSaberHandler)
    {
        GameInfo = game;

        this.BeatmapInfo = new()
        {
            Name = status.mapInfoChanged?.name,
            SubName = status.mapInfoChanged?.sub_name,
            Author = status.mapInfoChanged?.artist,
            Creator = status.mapInfoChanged?.mapper,
            IdOrHash = status.mapInfoChanged?.level_id,
            Bpm = status.mapInfoChanged?.BPM,
            Difficulty = status.mapInfoChanged?.difficulty,
        };

        double? v = null;

        try
        {
            v = Math.Round((status.scoreEvent?.accuracy ?? 0) * 100f, 2);
        }
        catch { }

        string Rank = "E";

        if (v >= 90)
            Rank = "SS";
        else if (v >= 80)
            Rank = "S";
        else if (v >= 65)
            Rank = "A";
        else if (v >= 50)
            Rank = "B";
        else if (v >= 35)
            Rank = "C";
        else if (v >= 20)
            Rank = "D";
        
        

        this.PerformanceInfo = new()
        {
            RawScore = status.scoreEvent?.score,
            Score = status.scoreEvent?.score,
            Accuracy = v ?? 00.00,
            Rank = Rank,
            MissedNoteCount = status.scoreEvent?.missCount,
            Failed = false,
            Finished = true,
            MaxCombo = MaxCombo,
            Combo = status.scoreEvent?.combo,
            SoftFailed = status.scoreEvent?.currentHealth == 0f
        };
    }

    public SharedStatus(DataPullerMain main, DataPullerData data, int MaxCombo, BaseBeatSaberHandler baseBeatSaberHandler)
    {
        GameInfo = new()
        {
            ModUsed = Mod.Datapuller,
            ModVersion = main?.PluginVersion,
            GameVersion = main?.GameVersion,
        };

        BeatmapInfo = new()
        {
            Name = main?.SongName,
            SubName = main?.SongSubName,
            Author = main?.SongAuthor,
            Creator = main?.Mapper,
            IdOrHash = main?.Hash,
            Bpm = main?.BPM,
            NoteJumpSpeed = main?.NJS,
            Difficulty = main?.Difficulty,
            CustomDifficulty = main?.CustomDifficultyLabel
        };

        PerformanceInfo = new()
        {
            RawScore = data?.Score,
            Score = data?.ScoreWithMultipliers,
            Accuracy = (double)Math.Round(data?.Accuracy ?? 0, 2),
            Rank = data?.Rank,
            MissedNoteCount = data?.Misses,
            Failed = main?.LevelFailed,
            Finished = main?.LevelFinished,
            MaxCombo = MaxCombo,
            Combo = data?.Combo,
            SoftFailed = ((main?.LevelFailed ?? false) || (data?.PlayerHealth ?? 0) <= 0) && (main?.Modifiers.NoFailOn0Energy ?? false)
        };
    }

    public SharedStatus(Entities.Legacy.DataPullerMain main, Entities.Legacy.DataPullerData data, int MaxCombo, BaseBeatSaberHandler baseBeatSaberHandler)
    {
        GameInfo = new()
        {
            ModUsed = Mod.Datapuller,
            ModVersion = main?.PluginVersion,
            GameVersion = main?.GameVersion,
        };

        BeatmapInfo = new()
        {
            Name = main?.SongName,
            SubName = main?.SongSubName,
            Author = main?.SongAuthor,
            Creator = main?.Mapper,
            IdOrHash = main?.Hash,
            Bpm = main?.BPM,
            NoteJumpSpeed = main?.NJS,
            Difficulty = main?.Difficulty,
            CustomDifficulty = main?.CustomDifficultyLabel
        };

        PerformanceInfo = new()
        {
            RawScore = data?.Score,
            Score = data?.ScoreWithMultipliers,
            Accuracy = (double)Math.Round(data?.Accuracy ?? 0, 2),
            Rank = data?.Rank,
            MissedNoteCount = data?.Misses,
            Failed = main?.LevelFailed,
            Finished = main?.LevelFinished,
            MaxCombo = MaxCombo,
            Combo = data?.Combo,
            SoftFailed = ((main?.LevelFailed ?? false) || (data?.PlayerHealth ?? 0) <= 0) && (main?.Modifiers.noFailOn0Energy ?? false)
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

        PerformanceInfo.Combo = newStatus.PerformanceInfo.Combo ?? PerformanceInfo.Combo;
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

        newStatus.GameInfo = new();
        newStatus.GameInfo.ModUsed = GameInfo.ModUsed;
        newStatus.GameInfo.ModVersion = GameInfo.ModVersion;
        newStatus.GameInfo.GameVersion = GameInfo.GameVersion;

        newStatus.BeatmapInfo = new();
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

        newStatus.PerformanceInfo = new();
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

    public Game GameInfo { get; set; }
    public Beatmap BeatmapInfo { get; set; }

    public Performance PerformanceInfo { get; set; }

    public class Game
    {
        public Mod ModUsed { get; set; }
        public string ModVersion { get; set; }
        public string GameVersion { get; set; }
    }

    public class Beatmap
    {
        public string Name { get; set; }
        public string SubName { get; set; }
        public string NameWithSub => $"{Name}{(SubName.IsNullOrWhiteSpace() ? "" : $" {SubName}")}";
        public string Author { get; set; }
        public string Creator { get; set; }
        public Bitmap Cover { get; set; }
        public string IdOrHash { get; set; }
        public float? Bpm { get; set; }
        public float? NoteJumpSpeed { get; set; }
        public string Difficulty { get; set; }
        public string CustomDifficulty { get => (_CustomDifficulty.IsNullOrWhiteSpace() ? Difficulty : _CustomDifficulty); set { _CustomDifficulty = value; } }
        public long? NoteCount { get; set; }
        public long? BombCount { get; set; }
        public long? WallCount { get; set; }

        private string _CustomDifficulty { get; set; }
    }

    public class Performance
    {
        public long? RawScore { get; set; }
        public long? Score { get; set; }
        public double? Accuracy { get; set; }
        public string Rank { get; set; }
        public long? MissedNoteCount { get; set; } = 0;
        public long? BadCutCount { get; set; } = 0;
        public long? BombHitCount { get; set; } = 0;
        public long? CombinedMisses { get => MissedNoteCount + BadCutCount + BombHitCount; }
        public long? Combo { get; set; }
        public long? MaxCombo { get; set; }
        public bool? SoftFailed { get; set; }
        public bool? Failed { get; set; }
        public bool? Finished { get; set; }
    }
}
