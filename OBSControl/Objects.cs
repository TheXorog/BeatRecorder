using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBSControl
{
    class Objects
    {
        // Settings

        public static Settings LoadedSettings = new Settings();

        public class Settings
        {
            public string README { get; set; }
            public int ConfigVersion { get; set; }
            public int ConsoleLogLevel { get; set; }
            public string Mod { get; set; }
            public string BeatSaberUrl { get; set; }
            public string BeatSaberPort { get; set; }
            public string OBSUrl { get; set; }
            public string OBSPort { get; set; }
            public string OBSPassword { get; set; }
            public int MininumWaitUntilRecordingCanStart { get; set; }
            public bool AskToSaveOBSPassword { get; set; }
            public bool PauseRecordingOnIngamePause { get; set; }
            public string FileFormat { get; set; }
            public int StopRecordingDelay { get; set; }
            public int DeleteIfShorterThan { get; set; }
            public bool DeleteQuit { get; set; }
            public bool DeleteIfQuitAfterSoftFailed { get; set; }
            public bool DeleteFailed { get; set; }
            public bool DeleteSoftFailed { get; set; }
        }

        // BeatSaber Websocket

        public class BeatSaberEvent
        {
            public string @event { get; set; }
            public long time { get; set; }
            public Status status { get; set; }
        }

        public static bool FinishedLastSong = false;
        public static bool FailedLastSong = false;

        public static bool FinishedCurrentSong = false;
        public static bool FailedCurrentSong = false;

        public class Status
        {
            public Game game { get; set; }
            public Beatmap beatmap { get; set; }
            public Performance performance { get; set; }
            public Mod mod { get; set; }
            public PlayerSettings playerSettings { get; set; }
        }

        public class Game
        {
            public string pluginVersion { get; set; }
            public string gameVersion { get; set; }
            public string scene { get; set; }
            public string mode { get; set; }
        }

        public class Beatmap
        {
            public string songName { get; set; }
            public string songSubName { get; set; }
            public string songAuthorName { get; set; }
            public string levelAuthorName { get; set; }
            public string songCover { get; set; }
            public string levelId { get; set; }
            public float songBPM { get; set; }
            public float noteJumpSpeed { get; set; }
            public int songTimeOffset { get; set; }
            public long start { get; set; }
            public object paused { get; set; }
            public int length { get; set; }
            public string difficulty { get; set; }
            public int notesCount { get; set; }
            public int bombsCount { get; set; }
            public int obstaclesCount { get; set; }
            public int maxScore { get; set; }
            public string maxRank { get; set; }
            public string environmentName { get; set; }
        }

        public class Performance
        {
            public int rawScore { get; set; }
            public int score { get; set; }
            public int currentMaxScore { get; set; }
            public string rank { get; set; }
            public int passedNotes { get; set; }
            public int hitNotes { get; set; }
            public int missedNotes { get; set; }
            public int lastNoteScore { get; set; }
            public int passedBombs { get; set; }
            public int hitBombs { get; set; }
            public int combo { get; set; }
            public int maxCombo { get; set; }
            public int multiplier { get; set; }
            public float multiplierProgress { get; set; }
            public object batteryEnergy { get; set; }
            public bool softFailed { get; set; }
        }

        public class Mod
        {
            public float multiplier { get; set; }
            public string obstacles { get; set; }
            public bool instaFail { get; set; }
            public bool noFail { get; set; }
            public bool batteryEnergy { get; set; }
            public object batteryLives { get; set; }
            public bool disappearingArrows { get; set; }
            public bool noBombs { get; set; }
            public string songSpeed { get; set; }
            public float songSpeedMultiplier { get; set; }
            public bool noArrows { get; set; }
            public bool ghostNotes { get; set; }
            public bool failOnSaberClash { get; set; }
            public bool strictAngles { get; set; }
            public bool fastNotes { get; set; }
            public bool smallNotes { get; set; }
            public bool proMode { get; set; }
            public bool zenMode { get; set; }
        }

        public class PlayerSettings
        {
            public bool staticLights { get; set; }
            public bool leftHanded { get; set; }
            public float playerHeight { get; set; }
            public float sfxVolume { get; set; }
            public bool reduceDebris { get; set; }
            public bool noHUD { get; set; }
            public bool advancedHUD { get; set; }
            public bool autoRestart { get; set; }
            public float saberTrailIntensity { get; set; }
            public string environmentEffects { get; set; }
            public bool hideNoteSpawningEffect { get; set; }
        }

        // DataPuller

        public static int LastSongCombo = 0;

        public static int CurrentSongCombo = 0;

        public class DataPullerMain
        {
            public string GameVersion { get; set; }
            public string PluginVersion { get; set; }
            public bool InLevel { get; set; }
            public bool LevelPaused { get; set; }
            public bool LevelFinished { get; set; }
            public bool LevelFailed { get; set; }
            public bool LevelQuit { get; set; }
            public object Hash { get; set; }
            public string SongName { get; set; }
            public string SongSubName { get; set; }
            public string SongAuthor { get; set; }
            public string Mapper { get; set; }
            public object BSRKey { get; set; }
            public object coverImage { get; set; }
            public int Length { get; set; }
            public float TimeScale { get; set; }
            public string MapType { get; set; }
            public string Difficulty { get; set; }
            public object CustomDifficultyLabel { get; set; }
            public int BPM { get; set; }
            public float NJS { get; set; }
            public Modifiers Modifiers { get; set; }
            public float ModifiersMultiplier { get; set; }
            public bool PracticeMode { get; set; }
            public Practicemodemodifiers PracticeModeModifiers { get; set; }
            public float PP { get; set; }
            public float Star { get; set; }
            public bool IsMultiplayer { get; set; }
            public int PreviousRecord { get; set; }
            public object PreviousBSR { get; set; }
        }

        public class Modifiers
        {
            public bool noFailOn0Energy { get; set; }
            public bool oneLife { get; set; }
            public bool fourLives { get; set; }
            public bool noBombs { get; set; }
            public bool noWalls { get; set; }
            public bool noArrows { get; set; }
            public bool ghostNotes { get; set; }
            public bool disappearingArrows { get; set; }
            public bool smallNotes { get; set; }
            public bool proMode { get; set; }
            public bool strictAngles { get; set; }
            public bool zenMode { get; set; }
            public bool slowerSong { get; set; }
            public bool fasterSong { get; set; }
            public bool superFastSong { get; set; }
        }

        public class Practicemodemodifiers
        {
            public float songSpeedMul { get; set; }
            public float startInAdvanceAndClearNotes { get; set; }
            public float startSongTime { get; set; }
        }


        public class DataPullerData
        {
            public int Score { get; set; }
            public int ScoreWithMultipliers { get; set; }
            public int MaxScore { get; set; }
            public int MaxScoreWithMultipliers { get; set; }
            public string Rank { get; set; }
            public bool FullCombo { get; set; }
            public int Combo { get; set; }
            public int Misses { get; set; }
            public float Accuracy { get; set; }
            public int[] BlockHitScore { get; set; }
            public float PlayerHealth { get; set; }
            public int TimeElapsed { get; set; }
        }

        // OBS Websocket

        public class AuthenticationRequired
        {
            public bool authRequired { get; set; }
            public string challenge { get; set; }
            public string messageid { get; set; }
            public string salt { get; set; }
            public string status { get; set; }
        }

        public class RecordingStatus
        {
            public bool isRecording { get; set; }
            public bool isRecordingPaused { get; set; }
        }

        public class RecordingStopped
        {
            [JsonProperty("recordingFilename")]
            public string recordingFilename { get; set; }

            [JsonProperty("update-type")]
            public string UpdateType { get; set; }
        }

        public class RecordingFolder
        {
            [JsonProperty("message-id")]
            public string MessageId { get; set; }

            [JsonProperty("rec-folder")]
            public string RecFolder { get; set; }
            public string status { get; set; }
        }

        // Shedule-based logger

        public static List<LogEntry> LogsToPost = new List<LogEntry>();

        public class LogEntry
        {
            public DateTime TimeOfEvent { get; set; }
            public int LogLevel { get; set; }
            public int LogCount { get; set; }
            public string Message { get; set; }
        }

    }
}
