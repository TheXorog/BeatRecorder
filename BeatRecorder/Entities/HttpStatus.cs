namespace BeatRecorder.Entities;

internal class HttpStatus
{
    public string @event { get; set; }
    public long time { get; set; }
    public Status status { get; set; }

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

        public bool finished { get; set; }
        public bool failed { get; set; }
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
}
