namespace BeatRecorder.Entities;

public class DataPullerData
{
    public int Score { get; set; }
    public int ScoreWithMultipliers { get; set; }
    public int MaxScore { get; set; }
    public int MaxScoreWithMultipliers { get; set; }
    public string Rank { get; set; }
    public bool FullCombo { get; set; }
    public int NotesSpawned { get; set; }
    public int Combo { get; set; }
    public int Misses { get; set; }
    public float Accuracy { get; set; }
    public Blockhitscore BlockHitScore { get; set; }
    public float PlayerHealth { get; set; }
    public int ColorType { get; set; }
    public int TimeElapsed { get; set; }
    public int EventTrigger { get; set; }
    public long UnixTimestamp { get; set; }

    public class Blockhitscore
    {
        public int PreSwing { get; set; }
        public int PostSwing { get; set; }
        public int CenterSwing { get; set; }
    }

}
