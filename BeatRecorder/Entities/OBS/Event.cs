namespace BeatRecorder.Entities.OBS;

internal class Event
{
    public int op { get; set; }
    public D d { get; set; }

    public class D
    {
        public string eventType { get; set; }
        public int eventIntent { get; set; }
        public JObject eventData { get; set; }
    }
}
