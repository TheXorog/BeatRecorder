using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBSControl
{
    class DataPullerObjects
    {
        internal static DataPullerObjects.DataPullerData DataPullerLastPerformance { get; set; }
        internal static DataPullerObjects.DataPullerMain DataPullerLastBeatmap { get; set; }

        internal static DataPullerObjects.DataPullerData DataPullerCurrentPerformance { get; set; }
        internal static DataPullerObjects.DataPullerMain DataPullerCurrentBeatmap { get; set; }



        internal static int LastSongCombo = 0;
        internal static int CurrentSongCombo = 0;
        internal static bool DataPullerInLevel = false;
        internal static bool DataPullerPaused = false;

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

    }
}
