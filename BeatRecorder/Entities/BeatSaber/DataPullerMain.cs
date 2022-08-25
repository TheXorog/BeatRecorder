namespace BeatRecorder.Entities;

internal class DataPullerMain
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
    public string coverImage { get; set; }
    public int Length { get; set; }
    public float TimeScale { get; set; }
    public string MapType { get; set; }
    public string Difficulty { get; set; }
    public string CustomDifficultyLabel { get; set; }
    public int BPM { get; set; }
    public float NJS { get; set; }
    public Modifier Modifiers { get; set; }
    public float ModifiersMultiplier { get; set; }
    public bool PracticeMode { get; set; }
    public Practicemodemodifiers PracticeModeModifiers { get; set; }
    public float PP { get; set; }
    public float Star { get; set; }
    public bool IsMultiplayer { get; set; }
    public int PreviousRecord { get; set; }
    public string PreviousBSR { get; set; }

    public class Modifier
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
}
