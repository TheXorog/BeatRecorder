namespace BeatRecorder.Entities.OBS;

internal class SetCurrentProgramScene : BaseRequest
{
    internal SetCurrentProgramScene(string scene)
    {
        this.op = 6;
        this.d = new JObject
        {
            ["requestType"] = "SetCurrentProgramScene",
            ["requestId"] = Guid.NewGuid().ToString(),
            ["requestData"] = new JObject
            {
                ["sceneName"] = scene
            }
        };
    }
}
