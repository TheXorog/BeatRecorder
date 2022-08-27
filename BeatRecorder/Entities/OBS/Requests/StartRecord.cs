namespace BeatRecorder.Entities.OBS;

internal class StartRecord : BaseRequest
{
    internal StartRecord()
    {
        this.op = 6;
        this.d = new JObject
        {
            ["requestType"] = "StartRecord",
            ["requestId"] = Guid.NewGuid().ToString(),
        };
    }
}
