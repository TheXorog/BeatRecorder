namespace BeatRecorder.Entities;

public class DataPullerMain
{
    public string GameVersion { get; set; }
    public string PluginVersion { get; set; }
    public bool InLevel { get; set; }
    public bool LevelPaused { get; set; }
    public bool LevelFinished { get; set; }
    public bool LevelFailed { get; set; }
    public bool LevelQuit { get; set; }
    public string Hash { get; set; }
    public string SongName { get; set; }
    public string SongSubName { get; set; }
    public string SongAuthor { get; set; }
    public string Mapper { get; set; }
    public string BSRKey { get; set; }
    public string CoverImage { get; set; }
    public int Duration { get; set; }
    public string MapType { get; set; }
    public string Difficulty { get; set; }
    public string CustomDifficultyLabel { get; set; }
    public int BPM { get; set; }
    public float NJS { get; set; }
    public ModifierObject Modifiers { get; set; }
    public float ModifiersMultiplier { get; set; }
    public bool PracticeMode { get; set; }
    public Practicemodemodifiers PracticeModeModifiers { get; set; }
    public float PP { get; set; }
    public float Star { get; set; }
    public bool IsMultiplayer { get; set; }
    public int PreviousRecord { get; set; }
    public string PreviousBSR { get; set; }
    public long UnixTimestamp { get; set; }

    public class ModifierObject
    {
        public bool NoFailOn0Energy { get; set; }
        public bool OneLife { get; set; }
        public bool FourLives { get; set; }
        public bool NoBombs { get; set; }
        public bool NoWalls { get; set; }
        public bool NoArrows { get; set; }
        public bool GhostNotes { get; set; }
        public bool DisappearingArrows { get; set; }
        public bool SmallNotes { get; set; }
        public bool ProMode { get; set; }
        public bool StrictAngles { get; set; }
        public bool ZenMode { get; set; }
        public bool SlowerSong { get; set; }
        public bool FasterSong { get; set; }
        public bool SuperFastSong { get; set; }
    }

    public class Practicemodemodifiers
    {
        public float SongSpeedMul { get; set; }
        public bool StartInAdvanceAndClearNotes { get; set; }
        public float SongStartTime { get; set; }
    }
}
