namespace BeatRecorder;

class OBSWebSocketObjects
{
    internal static bool OBSRecording = false;
    internal static bool OBSRecordingPaused = false;
    internal static int RecordingSeconds = 0;
    internal static CancellationTokenSource CancelStopRecordingDelay { get; set; }

    public class AuthenticationRequired
    {
        public bool authRequired { get; set; }
        public string challenge { get; set; }
        public string messageid { get; set; }
        public string salt { get; set; }
        public string status { get; set; }
    }

    public class RecordingStatus
    {
        public bool isRecording { get; set; }
        public bool isRecordingPaused { get; set; }
    }

    public class RecordingStopped
    {
        [JsonProperty("recordingFilename")]
        public string recordingFilename { get; set; }

        [JsonProperty("update-type")]
        public string UpdateType { get; set; }
    }

    public class RecordingFolder
    {
        [JsonProperty("message-id")]
        public string MessageId { get; set; }

        [JsonProperty("rec-folder")]
        public string RecFolder { get; set; }
        public string status { get; set; }
    }

}
