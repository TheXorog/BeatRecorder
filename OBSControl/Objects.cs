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
            public string BeatSaberUrl { get; set; }
            public string BeatSaberPort { get; set; }
            public string OBSUrl { get; set; }
            public string OBSPort { get; set; }
            public string OBSPassword { get; set; }
            public bool AskToSaveOBSPassword { get; set; }
            public bool PauseRecordingOnIngamePause { get; set; }
            public string FileFormat { get; set; }
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

        // OBS Websocket

        public class AuthenticationRequired
        {
            public bool authRequired { get; set; }
            public string challenge { get; set; }
            public string messageid { get; set; }
            public string salt { get; set; }
            public string status { get; set; }
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

    }
}
