using BeatRecorder.Enums;
using BeatRecorder.Util.BeatSaber;

namespace BeatRecorder.Entities;

public class SharedStatus
{
    public SharedStatus(HttpStatus.Status status, BaseBeatSaberHandler baseBeatSaberHandler)
    {
        this.GameInfo = new()
        {
            ModUsed = Mod.HttpStatus,
            ModVersion = status.game?.pluginVersion,
            GameVersion = status.game?.gameVersion,
        };

        Bitmap bitmap = null;

        try
        {
            if (!status?.beatmap?.songCover.IsNullOrWhiteSpace() ?? false)
            {
                bitmap = (Bitmap)Image.FromStream(new MemoryStream(Convert.FromBase64String(status?.beatmap?.songCover)));
            }
            else
            {

                if (!baseBeatSaberHandler.ImageCache.ContainsKey("https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg"))
                    _ = baseBeatSaberHandler.ImageCache.TryAdd("https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg", Bitmap.FromStream(new HttpClient().GetStreamAsync("https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg").Result));

                bitmap = (Bitmap)baseBeatSaberHandler.ImageCache["https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg"];
            }
        }
        catch { }

        this.BeatmapInfo = new()
        {
            Name = status.beatmap?.songName,
            SubName = status.beatmap?.songSubName,
            Author = status.beatmap?.songAuthorName,
            Creator = status.beatmap?.levelAuthorName,
            Cover = bitmap,
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
        this.GameInfo = game;

        Bitmap bitmap = null;

        try
        {
            if (!status?.mapInfoChanged?.coverRaw.IsNullOrWhiteSpace() ?? false)
            {
                bitmap = (Bitmap)Image.FromStream(new MemoryStream(Convert.FromBase64String(status?.mapInfoChanged?.coverRaw)));
            }
            else
            {

                if (!baseBeatSaberHandler.ImageCache.ContainsKey("https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg"))
                    _ = baseBeatSaberHandler.ImageCache.TryAdd("https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg", Bitmap.FromStream(new HttpClient().GetStreamAsync("https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg").Result));

                bitmap = (Bitmap)baseBeatSaberHandler.ImageCache["https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg"];
            }
        }
        catch { }

        this.BeatmapInfo = new()
        {
            Name = status.mapInfoChanged?.name,
            SubName = status.mapInfoChanged?.sub_name,
            Author = status.mapInfoChanged?.artist,
            Creator = status.mapInfoChanged?.mapper,
            Cover = bitmap,
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

        var Rank = "E";

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
        this.GameInfo = new()
        {
            ModUsed = Mod.Datapuller,
            ModVersion = main?.PluginVersion,
            GameVersion = main?.GameVersion,
        };

        if (!baseBeatSaberHandler.ImageCache.ContainsKey(main?.CoverImage ?? "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg"))
            _ = baseBeatSaberHandler.ImageCache.TryAdd(main?.CoverImage ?? "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg", Bitmap.FromStream(new HttpClient().GetStreamAsync(main?.CoverImage ?? "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg").Result));

        var image = (Bitmap)baseBeatSaberHandler.ImageCache[main?.CoverImage ?? "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg"];

        this.BeatmapInfo = new()
        {
            Name = main?.SongName,
            SubName = main?.SongSubName,
            Author = main?.SongAuthor,
            Creator = main?.Mapper,
            Cover = (Bitmap)image,
            IdOrHash = main?.Hash,
            Bpm = main?.BPM,
            NoteJumpSpeed = main?.NJS,
            Difficulty = main?.Difficulty,
            CustomDifficulty = main?.CustomDifficultyLabel
        };

        this.PerformanceInfo = new()
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
        this.GameInfo = new()
        {
            ModUsed = Mod.Datapuller,
            ModVersion = main?.PluginVersion,
            GameVersion = main?.GameVersion,
        };

        if (!baseBeatSaberHandler.ImageCache.ContainsKey(main?.coverImage ?? "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg"))
            _ = baseBeatSaberHandler.ImageCache.TryAdd(main?.coverImage ?? "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg", Bitmap.FromStream(new HttpClient().GetStreamAsync(main?.coverImage ?? "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg").Result));

        var image = (Bitmap)baseBeatSaberHandler.ImageCache[main?.coverImage ?? "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg"];

        this.BeatmapInfo = new()
        {
            Name = main?.SongName,
            SubName = main?.SongSubName,
            Author = main?.SongAuthor,
            Creator = main?.Mapper,
            Cover = (Bitmap)image,
            IdOrHash = main?.Hash,
            Bpm = main?.BPM,
            NoteJumpSpeed = main?.NJS,
            Difficulty = main?.Difficulty,
            CustomDifficulty = main?.CustomDifficultyLabel
        };

        this.PerformanceInfo = new()
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
        if (this.GameInfo.ModUsed != newStatus.GameInfo.ModUsed)
            throw new InvalidOperationException("Mod used cannot be updated.");

        this.GameInfo.ModVersion = newStatus.GameInfo.ModVersion ?? this.GameInfo.ModVersion;
        this.GameInfo.GameVersion = newStatus.GameInfo.GameVersion ?? this.GameInfo.GameVersion;

        this.BeatmapInfo.Name = newStatus.BeatmapInfo.Name ?? this.BeatmapInfo.Name;
        this.BeatmapInfo.SubName = newStatus.BeatmapInfo.SubName ?? this.BeatmapInfo.SubName;
        this.BeatmapInfo.Author = newStatus.BeatmapInfo.Author ?? this.BeatmapInfo.Author;
        this.BeatmapInfo.Creator = newStatus.BeatmapInfo.Creator ?? this.BeatmapInfo.Creator;
        this.BeatmapInfo.Cover = newStatus.BeatmapInfo.Cover ?? this.BeatmapInfo.Cover;
        this.BeatmapInfo.Bpm = newStatus.BeatmapInfo.Bpm ?? this.BeatmapInfo.Bpm;
        this.BeatmapInfo.NoteJumpSpeed = newStatus.BeatmapInfo.NoteJumpSpeed ?? this.BeatmapInfo.NoteJumpSpeed;
        this.BeatmapInfo.Difficulty = newStatus.BeatmapInfo.Difficulty ?? this.BeatmapInfo.Difficulty;
        this.BeatmapInfo.NoteCount = newStatus.BeatmapInfo.NoteCount ?? this.BeatmapInfo.NoteCount;
        this.BeatmapInfo.BombCount = newStatus.BeatmapInfo.BombCount ?? this.BeatmapInfo.BombCount;
        this.BeatmapInfo.WallCount = newStatus.BeatmapInfo.WallCount ?? this.BeatmapInfo.WallCount;
        this.BeatmapInfo.CustomDifficulty = newStatus.BeatmapInfo.CustomDifficulty ?? this.BeatmapInfo.CustomDifficulty;

        this.PerformanceInfo.Combo = newStatus.PerformanceInfo.Combo ?? this.PerformanceInfo.Combo;
        this.PerformanceInfo.RawScore = newStatus.PerformanceInfo.RawScore ?? this.PerformanceInfo.RawScore;
        this.PerformanceInfo.Score = newStatus.PerformanceInfo.Score ?? this.PerformanceInfo.Score;
        this.PerformanceInfo.Accuracy = newStatus.PerformanceInfo.Accuracy ?? this.PerformanceInfo.Accuracy;
        this.PerformanceInfo.Rank = newStatus.PerformanceInfo.Rank ?? this.PerformanceInfo.Rank;
        this.PerformanceInfo.MissedNoteCount = newStatus.PerformanceInfo.MissedNoteCount ?? this.PerformanceInfo.MissedNoteCount;
        this.PerformanceInfo.BadCutCount = newStatus.PerformanceInfo.BadCutCount ?? this.PerformanceInfo.BadCutCount;
        this.PerformanceInfo.BombHitCount = newStatus.PerformanceInfo.BombHitCount ?? this.PerformanceInfo.BombHitCount;
        this.PerformanceInfo.MaxCombo = newStatus.PerformanceInfo.MaxCombo ?? this.PerformanceInfo.MaxCombo;
        this.PerformanceInfo.SoftFailed = newStatus.PerformanceInfo.SoftFailed ?? this.PerformanceInfo.SoftFailed;
        this.PerformanceInfo.Failed = newStatus.PerformanceInfo.Failed ?? this.PerformanceInfo.Failed;
        this.PerformanceInfo.Finished = newStatus.PerformanceInfo.Finished ?? this.PerformanceInfo.Finished;
    }

    public SharedStatus Clone()
    {
        var newStatus = new SharedStatus();

        newStatus.GameInfo = new();
        newStatus.GameInfo.ModUsed = this.GameInfo.ModUsed;
        newStatus.GameInfo.ModVersion = this.GameInfo.ModVersion;
        newStatus.GameInfo.GameVersion = this.GameInfo.GameVersion;

        newStatus.BeatmapInfo = new();
        newStatus.BeatmapInfo.Name = this.BeatmapInfo.Name;
        newStatus.BeatmapInfo.SubName = this.BeatmapInfo.SubName;
        newStatus.BeatmapInfo.Author = this.BeatmapInfo.Author;
        newStatus.BeatmapInfo.Creator = this.BeatmapInfo.Creator;
        newStatus.BeatmapInfo.Cover = this.BeatmapInfo.Cover;
        newStatus.BeatmapInfo.Bpm = this.BeatmapInfo.Bpm;
        newStatus.BeatmapInfo.NoteJumpSpeed = this.BeatmapInfo.NoteJumpSpeed;
        newStatus.BeatmapInfo.Difficulty = this.BeatmapInfo.Difficulty;
        newStatus.BeatmapInfo.NoteCount = this.BeatmapInfo.NoteCount;
        newStatus.BeatmapInfo.BombCount = this.BeatmapInfo.BombCount;
        newStatus.BeatmapInfo.WallCount = this.BeatmapInfo.WallCount;
        newStatus.BeatmapInfo.CustomDifficulty = this.BeatmapInfo.CustomDifficulty;

        newStatus.PerformanceInfo = new();
        newStatus.PerformanceInfo.RawScore = this.PerformanceInfo.RawScore;
        newStatus.PerformanceInfo.Score = this.PerformanceInfo.Score;
        newStatus.PerformanceInfo.Accuracy = this.PerformanceInfo.Accuracy;
        newStatus.PerformanceInfo.Rank = this.PerformanceInfo.Rank;
        newStatus.PerformanceInfo.MissedNoteCount = this.PerformanceInfo.MissedNoteCount;
        newStatus.PerformanceInfo.BadCutCount = this.PerformanceInfo.BadCutCount;
        newStatus.PerformanceInfo.BombHitCount = this.PerformanceInfo.BombHitCount;
        newStatus.PerformanceInfo.MaxCombo = this.PerformanceInfo.MaxCombo;
        newStatus.PerformanceInfo.SoftFailed = this.PerformanceInfo.SoftFailed;
        newStatus.PerformanceInfo.Failed = this.PerformanceInfo.Failed;
        newStatus.PerformanceInfo.Finished = this.PerformanceInfo.Finished;
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
        public string NameWithSub => $"{this.Name}{(this.SubName.IsNullOrWhiteSpace() ? "" : $" {this.SubName}")}";
        public string Author { get; set; }
        public string Creator { get; set; }
        public Bitmap Cover { get; set; }
        public string IdOrHash { get; set; }
        public float? Bpm { get; set; }
        public float? NoteJumpSpeed { get; set; }
        public string Difficulty { get; set; }
        public string CustomDifficulty { get => (this._CustomDifficulty.IsNullOrWhiteSpace() ? this.Difficulty : this._CustomDifficulty); set => this._CustomDifficulty = value; }
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
        public long? CombinedMisses => this.MissedNoteCount + this.BadCutCount + this.BombHitCount;
        public long? Combo { get; set; }
        public long? MaxCombo { get; set; }
        public bool? SoftFailed { get; set; }
        public bool? Failed { get; set; }
        public bool? Finished { get; set; }
    }
}
