namespace BeatRecorder;

class OBSWebSocketStatus
{
    internal static bool OBSRecording = false;
    internal static bool OBSRecordingPaused = false;
    internal static int RecordingSeconds = 0;

    internal static CancellationTokenSource CancelStopRecordingDelay { get; set; }
}
