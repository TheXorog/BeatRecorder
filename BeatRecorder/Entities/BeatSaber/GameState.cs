namespace BeatRecorder.Entities;

internal class GameState
{
    private GameState() { }

    public GameState(HttpStatusStatus.Status status, GameEnvironment environment)
    {
        this.Game = new();
        this.Beatmap = new();
        this.Performance = new();

        this.Game.Mod = Mod.HttpStatus;
        this.Game.GameVersion = status.game.gameVersion;
        this.Game.ModVersion = status.game.pluginVersion;

        UpdateGameState(status, environment, false, false);
    }

    public GameState(DataPullerStatus.DataPullerMain beatmap, DataPullerStatus.DataPullerData performance, long MaxCombo)
    {
        this.Game = new();
        this.Beatmap = new();
        this.Performance = new();

        this.Game.Mod = Mod.DataPuller;
        this.Game.GameVersion = beatmap.GameVersion;
        this.Game.ModVersion = beatmap.PluginVersion;

        UpdateGameState(beatmap, performance, MaxCombo);
    }

    public void UpdateGameState(GameEnvironment environment)
    {
        this.GameEnvironment = environment;
    }

    public void UpdateGameState(HttpStatusStatus.Beatmap beatmap)
    {
        this.Beatmap.SongName = beatmap.songName;
        this.Beatmap.SongSubName = beatmap.songSubName;
        this.Beatmap.SongAuthorName = beatmap.songAuthorName;
        this.Beatmap.SongCoverArt = new(CoverArtType.Base64, beatmap.songCover);
        this.Beatmap.LevelAuthorName = beatmap.levelAuthorName;
        this.Beatmap.Difficulty = beatmap.difficulty;
        this.Beatmap.CustomDifficulty = null;
        this.Beatmap.LevelIdOrHash = beatmap.levelId;
        this.Beatmap.NotesCount = beatmap.notesCount;
        this.Beatmap.BombsCount = beatmap.bombsCount;
        this.Beatmap.WallsCount = beatmap.obstaclesCount;
        this.Beatmap.MaxScore = beatmap.maxScore;
        this.Beatmap.LevelLength = TimeSpan.FromMilliseconds(beatmap.length);
    }

    public void UpdateGameState(HttpStatusStatus.Performance performance, bool Failed, bool Finished)
    {
        this.Performance.Score = performance.score;
        this.Performance.Accuracy = (double)Math.Round((float)(((float)performance.score * (float)100) / (float)performance.currentMaxScore), 2);
        this.Performance.Combo = performance.combo;
        this.Performance.CurrentMaxScore = performance.currentMaxScore;
        this.Performance.Misses = performance.passedNotes + performance.hitBombs + performance.missedNotes;
        this.Performance.MaxCombo = performance.maxCombo;
        this.Performance.Rank = performance.rank;
        this.Performance.RawScore = performance.rawScore;
        this.Performance.SoftFailed = performance.softFailed;
        this.Performance.Failed = Failed;
        this.Performance.Finished = Finished;
    }

    public void UpdateGameState(HttpStatusStatus.Status status, GameEnvironment environment, bool Failed, bool Finished)
    {
        UpdateGameState(environment);
        UpdateGameState(status.beatmap);
        UpdateGameState(status.performance, Failed, Finished);
    }

    public void UpdateGameState(DataPullerStatus.DataPullerMain beatmap, DataPullerStatus.DataPullerData performance, long MaxCombo)
    {
        if (!DataPullerStatus.DataPullerInLevel && beatmap.InLevel)
            this.GameEnvironment = GameEnvironment.Ingame;
        else if (DataPullerStatus.DataPullerInLevel && !beatmap.InLevel)
            this.GameEnvironment = GameEnvironment.Menu;

        if (beatmap.LevelPaused)
            this.GameEnvironment = GameEnvironment.Paused;

        

        this.Beatmap.SongName = beatmap.SongName;
        this.Beatmap.SongSubName = beatmap.SongSubName;
        this.Beatmap.SongAuthorName = beatmap.SongAuthor;
        this.Beatmap.SongCoverArt = new(CoverArtType.Url, beatmap.coverImage);
        this.Beatmap.LevelAuthorName = beatmap.Mapper;
        this.Beatmap.Difficulty = beatmap.Difficulty;
        this.Beatmap.CustomDifficulty = beatmap.CustomDifficultyLabel;
        this.Beatmap.LevelIdOrHash = beatmap.Hash;
        this.Beatmap.NotesCount = 0;
        this.Beatmap.BombsCount = 0;
        this.Beatmap.WallsCount = 0;
        this.Beatmap.MaxScore = 0;
        this.Beatmap.LevelLength = TimeSpan.FromSeconds(beatmap.Length);

        this.Performance.Score = performance.ScoreWithMultipliers;
        this.Performance.Accuracy = Convert.ToDouble(performance.Accuracy);
        this.Performance.Combo = performance.Combo;
        this.Performance.CurrentMaxScore = performance.MaxScore;
        this.Performance.Misses = performance.Misses;
        this.Performance.MaxCombo = MaxCombo;
        this.Performance.Rank = performance.Rank;
        this.Performance.RawScore = performance.Score;
        this.Performance.SoftFailed = ((beatmap.LevelFailed || performance.PlayerHealth <= 0) && beatmap.Modifiers.noFailOn0Energy);
    }

    /// <summary>
    /// The current Environment the game is in
    /// </summary>
    public GameEnvironment GameEnvironment { get; set; }

    public GameInfo Game { get; set; }
    public StateInfo State { get; set; }
    public BeatmapInfo Beatmap { get; set; }
    public PerformanceInfo Performance { get; set; }

    public class GameInfo
    {
        /// <summary>
        /// What mod is currently used to connect to BeatSaber
        /// </summary>
        public Mod Mod { get; set; }

        /// <summary>
        /// What version of the game is currently being used
        /// </summary>
        public string GameVersion { get; set; }

        /// <summary>
        /// What version of the mod is currently being used
        /// </summary>
        public string ModVersion { get; set; }
    }

    public class BeatmapInfo
    {
        /// <summary>
        /// The name of the song
        /// </summary>
        public string SongName { get; set; }

        /// <summary>
        /// The sub name of the song
        /// </summary>
        public string SongSubName { get; set; }

        /// <summary>
        /// The song name combined with the sub name
        /// </summary>
        public string SongNameWithSubname { get
            {
                return $"{SongName}{(!string.IsNullOrWhiteSpace(SongSubName) ? $" {SongSubName}" : "")}";
            } 
        }

        /// <summary>
        /// The artist who made this song
        /// </summary>
        public string SongAuthorName { get; set; }

        /// <summary>
        /// The mapper who made this Beatmap
        /// </summary>
        public string LevelAuthorName { get; set; }

        /// <summary>
        /// The difficulty currently being played
        /// </summary>
        public string Difficulty { get; set; }

        /// <summary>
        /// The custom difficulty name, if present
        /// </summary>
        public string CustomDifficulty { get; set; }

        /// <summary>
        /// The Cover Art
        /// </summary>
        public SongCoverArtInfo SongCoverArt { get; set; }

        /// <summary>
        /// The level id or hash, what kind this is depends on the mod used
        /// </summary>
        public string LevelIdOrHash { get; set; }

        /// <summary>
        /// How long the map is
        /// </summary>
        public TimeSpan LevelLength { get; set; }

        /// <summary>
        /// How many notes there are in this map
        /// </summary>
        public long NotesCount { get; set; }

        /// <summary>
        /// How many bombs there are in this map
        /// </summary>
        public long BombsCount { get; set; }

        /// <summary>
        /// How many walls there are in this map
        /// </summary>
        public long WallsCount { get; set; }

        /// <summary>
        /// The maximum possible score achievable
        /// </summary>
        public long MaxScore { get; set; }

        public class SongCoverArtInfo
        {
            internal SongCoverArtInfo() { }

            /// <summary>
            /// Constructs a new covert art object
            /// </summary>
            /// <param name="coverArtType">What type of cover art is supplied</param>
            /// <param name="coverArt">The cover art data</param>
            public SongCoverArtInfo(CoverArtType coverArtType, string coverArt)
            {
                CoverArtType = coverArtType;

                switch (coverArtType)
                {
                    case CoverArtType.Url:
                    {
                        SongCoverArtUrl = coverArt;
                        break;
                    }
                    case CoverArtType.Base64:
                    {
                        SongCoverArtBase64 = coverArt;
                        break;
                    }
                }
            }


            /// <summary>
            /// What type of Cover Art this is
            /// </summary>
            public CoverArtType CoverArtType { get; set; }

            /// <summary>
            /// The url of the Cover Art, if the Type is Url
            /// </summary>
            public string SongCoverArtUrl { get; set; }

            /// <summary>
            /// The Cover Art encoded in base64, if the Type is Base64
            /// </summary>
            public string SongCoverArtBase64 { get; set; }
        }
    }

    public class StateInfo
    {
        /// <summary>
        /// The DateTime when the map was started
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// How many seconds into the song was progressed
        /// </summary>
        public long SecondsElapsed { get; set; }
    }

    public class PerformanceInfo
    {
        /// <summary>
        /// The score without multipliers applied
        /// </summary>
        public long RawScore { get; set; }

        /// <summary>
        /// The processed score with multipliers applied
        /// </summary>
        public long Score { get; set; }

        /// <summary>
        /// The accuracy.
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        /// The current maximum amount of score achievable
        /// </summary>
        public long CurrentMaxScore { get; set; }

        /// <summary>
        /// The current rank
        /// </summary>
        public string Rank { get; set; }

        /// <summary>
        /// How many notes have been missed and bombs hit
        /// </summary>
        public long Misses { get; set; }

        /// <summary>
        /// The current combo
        /// </summary>
        public long Combo { get; set; }

        /// <summary>
        /// The biggest combo in the current play
        /// </summary>
        public long MaxCombo { get; set; }

        /// <summary>
        /// The map was failed
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        /// The map was played to the end
        /// </summary>
        public bool Finished { get; set; }

        /// <summary>
        /// Energy dropped to 0 while NoFail is turned on
        /// </summary>
        public bool SoftFailed { get; set; }
    }

    public GameState Clone()
    {
        return new GameState
        {
            Beatmap = new()
            {
                LevelAuthorName = this.Beatmap.LevelAuthorName,
                SongAuthorName = this.Beatmap.SongAuthorName,
                SongCoverArt = new()
                {
                    CoverArtType = this.Beatmap.SongCoverArt.CoverArtType,
                    SongCoverArtBase64 = this.Beatmap.SongCoverArt.SongCoverArtBase64,
                    SongCoverArtUrl = this.Beatmap.SongCoverArt.SongCoverArtUrl
                },
                BombsCount = this.Beatmap.BombsCount,
                CustomDifficulty = this.Beatmap.CustomDifficulty,
                Difficulty = this.Beatmap.Difficulty,
                LevelIdOrHash = this.Beatmap.LevelIdOrHash,
                LevelLength = this.Beatmap.LevelLength,
                MaxScore = this.Beatmap.MaxScore,
                NotesCount = this.Beatmap.NotesCount,
                SongName = this.Beatmap.SongName,
                SongSubName = this.Beatmap.SongSubName,
                WallsCount = this.Beatmap.WallsCount
            },
            Game = new()
            {
                GameVersion = this.Game.GameVersion,
                Mod = this.Game.Mod,
                ModVersion = this.Game.ModVersion
            },
            GameEnvironment = this.GameEnvironment,
            Performance = new()
            {
                Accuracy = this.Performance.Accuracy,
                Combo = this.Performance.Combo,
                CurrentMaxScore = this.Performance.CurrentMaxScore,
                Failed = this.Performance.Failed,
                Finished = this.Performance.Finished,
                MaxCombo = this.Performance.MaxCombo,
                Misses = this.Performance.Misses,
                Rank = this.Performance.Rank,
                RawScore = this.Performance.RawScore,
                Score = this.Performance.Score,
                SoftFailed = this.Performance.SoftFailed
            },
            State = new()
            {
                SecondsElapsed = this.State.SecondsElapsed,
                StartTime = this.State.StartTime
            }
        };
    }
}
