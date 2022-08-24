namespace BeatRecorder.Entities.OBS;
internal class PauseRecordingRequest : BaseRequest
{
    internal PauseRecordingRequest(string id = null)
    {
        this.RequestType = "PauseRecording";
        this.MessageId = id ?? Guid.NewGuid().ToString();
    }
}
