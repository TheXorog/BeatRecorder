namespace BeatRecorder.Entities.OBS;

internal class ResumeRecord : BaseRequest
{
    internal ResumeRecord()
    {
        this.op = 6;
        this.d = new JObject
        {
            ["requestType"] = "PauseRecord",
            ["requestId"] = Guid.NewGuid().ToString(),
        };
    }
}
