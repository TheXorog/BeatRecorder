namespace BeatRecorder.Entities;
public class BeatSaberPlus
{
    public string _type { get; set; }
    public string _event { get; set; }
    public int protocolVersion { get; set; }
    public string gameVersion { get; set; }
    public Mapinfochanged mapInfoChanged { get; set; }
    public string gameStateChanged { get; set; }
    public Scoreevent scoreEvent { get; set; }

    public class Mapinfochanged
    {
        public string level_id { get; set; }
        public string name { get; set; }
        public string sub_name { get; set; }
        public string artist { get; set; }
        public string mapper { get; set; }
        public string characteristic { get; set; }
        public string difficulty { get; set; }
        public int duration { get; set; }
        public float BPM { get; set; }
        public float PP { get; set; }
        public string BSRKey { get; set; }
        public string coverRaw { get; set; }
    }

    public class Scoreevent
    {
        public float time { get; set; }
        public int score { get; set; }
        public float accuracy { get; set; }
        public int combo { get; set; }
        public int missCount { get; set; }
        public float currentHealth { get; set; }
    }
}
