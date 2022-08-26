namespace BeatRecorder.Entities.OBS.Legacy;
internal class ResumeRecordingRequest : BaseRequest
{
    internal ResumeRecordingRequest(string id = null)
    {
        this.RequestType = "ResumeRecording";
        this.MessageId = id ?? Guid.NewGuid().ToString();
    }
}
