namespace BeatRecorder.Entities;

internal class DataPullerData
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

    public int maxCombo { get; set; }
}
