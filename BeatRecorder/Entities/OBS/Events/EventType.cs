namespace BeatRecorder.Entities.OBS;

internal class EventType
{
    public int op { get; set; }
    public D d { get; set; }

    public class D
    {
        public string eventType { get; set; }
    }
}
