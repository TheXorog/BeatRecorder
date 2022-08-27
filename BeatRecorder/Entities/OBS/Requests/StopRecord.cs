namespace BeatRecorder.Entities.OBS;

internal class StopRecord : BaseRequest
{
    internal StopRecord()
    {
        this.op = 6;
        this.d = new JObject
        {
            ["requestType"] = "StopRecord",
            ["requestId"] = Guid.NewGuid().ToString(),
        };
    }
}
