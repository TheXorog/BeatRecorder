namespace BeatRecorder.Entities.OBS.Legacy;
internal class StartRecordingRequest : BaseRequest
{
    internal StartRecordingRequest(string id = null)
    {
        this.RequestType = "StartRecording";
        this.MessageId = id ?? Guid.NewGuid().ToString();
    }
}
