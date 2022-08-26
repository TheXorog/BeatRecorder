namespace BeatRecorder.Entities.OBS.Legacy;
internal class StopRecordingRequest : BaseRequest
{
    internal StopRecordingRequest(string id = null)
    {
        this.RequestType = "StopRecording";
        this.MessageId = id ?? Guid.NewGuid().ToString();
    }
}
