namespace BeatRecorder.Entities.OBS;

internal class Hello
{
    public D d { get; set; }
    public int op { get; set; }

    public class D
    {
        public Authentication authentication { get; set; }
        public string obsWebSocketVersion { get; set; }
        public int rpcVersion { get; set; }
    }

    public class Authentication
    {
        public string challenge { get; set; }
        public string salt { get; set; }
    }
}
