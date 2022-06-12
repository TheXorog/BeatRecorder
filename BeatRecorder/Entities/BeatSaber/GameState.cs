namespace BeatRecorder.Entities;

internal class GameState
{
    public GameState(HttpStatusStatus.Status status, GameEnvironment environment)
    {
        this.GameEnvironment = environment;

        this.Game = new()
        {
            Mod = Mod.HttpStatus,
            GameVersion = status.game.gameVersion,
            ModVersion = status.game.pluginVersion
        };

        this.Beatmap = new()
        {
            SongName = status.beatmap.songName,
            SongSubName = status.beatmap.songSubName,
            SongAuthorName = status.beatmap.songAuthorName,
            SongCoverArt = new(CoverArtType.Base64, status.beatmap.songCover),
            LevelAuthorName = status.beatmap.levelAuthorName,
            Difficulty = status.beatmap.difficulty,
            CustomDifficulty = null,
            LevelIdOrHash = status.beatmap.levelId,
            NotesCount = status.beatmap.notesCount,
            BombsCount = status.beatmap.bombsCount,
            WallsCount = status.beatmap.obstaclesCount,
            MaxScore = status.beatmap.maxScore,
            LevelLength = TimeSpan.FromMilliseconds(status.beatmap.length)
        };

        this.Performance = new()
        {
            Score = status.performance.score,
            Accuracy = (double)Math.Round((float)(((float)status.performance.score * (float)100) / (float)status.performance.currentMaxScore), 2),
            Combo = status.performance.combo,
            CurrentMaxScore = status.performance.currentMaxScore,
            Misses = status.performance.passedNotes + status.performance.hitBombs + status.performance.missedNotes,
            MaxCombo = status.performance.maxCombo,
            Rank = status.performance.rank,
            RawScore = status.performance.rawScore,
            SoftFailed = status.performance.softFailed
        };
    }

    public GameState(DataPullerStatus.DataPullerMain beatmap, DataPullerStatus.DataPullerData performance, long MaxCombo)
    {
        if (!DataPullerStatus.DataPullerInLevel && beatmap.InLevel)
        {
            this.GameEnvironment = GameEnvironment.Ingame;
        }
        else if (DataPullerStatus.DataPullerInLevel && !beatmap.InLevel)
        {
            this.GameEnvironment = GameEnvironment.Menu;
        }
        
        this.Game = new()
        {
            Mod = Mod.DataPuller,
            GameVersion = beatmap.GameVersion,
            ModVersion = beatmap.PluginVersion
        };

        this.Beatmap = new()
        {
            SongName = beatmap.SongName,
            SongSubName = beatmap.SongSubName,
            SongAuthorName = beatmap.SongAuthor,
            SongCoverArt = new(CoverArtType.Url, beatmap.coverImage),
            LevelAuthorName = beatmap.Mapper,
            Difficulty = beatmap.Difficulty,
            CustomDifficulty = beatmap.CustomDifficultyLabel,
            LevelIdOrHash = beatmap.Hash,
            NotesCount = 0,
            BombsCount = 0,
            WallsCount = 0,
            MaxScore = 0,
            LevelLength = TimeSpan.FromSeconds(beatmap.Length)
        };

        this.Performance = new()
        {
            Score = performance.ScoreWithMultipliers,
            Accuracy = Convert.ToDouble(performance.Accuracy),
            Combo = performance.Combo,
            CurrentMaxScore = performance.MaxScore,
            Misses = performance.Misses,
            MaxCombo = MaxCombo,
            Rank = performance.Rank,
            RawScore = performance.Score,
            SoftFailed = ((beatmap.LevelFailed || performance.PlayerHealth <= 0) && beatmap.Modifiers.noFailOn0Energy)
        };
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
        /// Energy dropped to 0 while NoFail is turned on
        /// </summary>
        public bool SoftFailed { get; set; }
    }
}
