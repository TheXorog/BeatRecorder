namespace BeatRecorder.Entities.OBS;
internal class RecordStateChanged
{
    public int op { get; set; }
    public D d { get; set; }

    public class D
    {
        public Eventdata eventData { get; set; }
        public int eventIntent { get; set; }
        public string eventType { get; set; }
    }

    public class Eventdata
    {
        public bool outputActive { get; set; }
        public string outputPath { get; set; }
        public string outputState { get; set; }
    }

}
