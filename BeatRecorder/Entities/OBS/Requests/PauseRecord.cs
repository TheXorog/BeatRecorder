namespace BeatRecorder.Entities.OBS;

internal class PauseRecord : BaseRequest
{
    internal PauseRecord()
    {
        this.op = 6;
        this.d = new JObject
        {
            ["requestType"] = "PauseRecord",
            ["requestId"] = Guid.NewGuid().ToString(),
        };
    }
}
