namespace BeatRecorder.Entities.OBS.Legacy;
internal class GetAuthRequiredRequest : BaseRequest
{
    internal GetAuthRequiredRequest(string id = null)
    {
        this.RequestType = "GetAuthRequired";
        this.MessageId = id ?? Guid.NewGuid().ToString();
    }
}
