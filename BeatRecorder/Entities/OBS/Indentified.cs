namespace BeatRecorder.Entities.OBS;

internal class Indentified
{
    public int op { get; set; }
    public D d { get; set; }

    public class D
    {
        public int negotiatedRpcVersion { get; set; }
    }
}
