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
            Combo = status.performance.combo,
            CurrentMaxScore = status.performance.currentMaxScore,
            FailedNotes = status.performance.passedNotes,
            HitBombs = status.performance.hitBombs,
            MaxCombo = status.performance.maxCombo,
            MissedNotes = status.performance.missedNotes,
            PassedBombs = status.performance.passedBombs,
            Rank = status.performance.rank,
            RawScore = status.performance.rawScore,
            SoftFailed = status.performance.softFailed
        };
    }

    public GameState(DataPullerStatus.DataPullerMain status)
    {
        if (!DataPullerStatus.DataPullerInLevel && status.InLevel)
        {
            this.GameEnvironment = GameEnvironment.Ingame;
        }
        else if (DataPullerStatus.DataPullerInLevel && !status.InLevel)
        {
            this.GameEnvironment = GameEnvironment.Menu;
        }
        
        this.Game = new()
        {
            Mod = Mod.DataPuller,
            GameVersion = status.GameVersion,
            ModVersion = status.PluginVersion
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
            Combo = status.performance.combo,
            CurrentMaxScore = status.performance.currentMaxScore,
            FailedNotes = status.performance.passedNotes,
            HitBombs = status.performance.hitBombs,
            MaxCombo = status.performance.maxCombo,
            MissedNotes = status.performance.missedNotes,
            PassedBombs = status.performance.passedBombs,
            Rank = status.performance.rank,
            RawScore = status.performance.rawScore,
            SoftFailed = status.performance.softFailed
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
        /// The current maximum amount of score achievable
        /// </summary>
        public long CurrentMaxScore { get; set; }

        /// <summary>
        /// The current rank
        /// </summary>
        public string Rank { get; set; }

        /// <summary>
        /// How many notes have been missed
        /// </summary>
        public long MissedNotes { get; set; }

        /// <summary>
        /// How many notes have been hit incorrectly
        /// </summary>
        public long FailedNotes { get; set; }

        /// <summary>
        /// How many bombs have been hit
        /// </summary>
        public long HitBombs { get; set; }

        /// <summary>
        /// How many bombs have been missed
        /// </summary>
        public long PassedBombs { get; set; }

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
