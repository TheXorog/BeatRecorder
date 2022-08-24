namespace BeatRecorder.Entities.OBS;

internal class SetCurrentScene : BaseRequest
{
    internal SetCurrentScene(string SceneName, string id = null)
    {
        this.SceneName = SceneName;
        this.RequestType = "SetCurrentScene";
        this.MessageId = id ?? Guid.NewGuid().ToString();
    }

    [JsonProperty("scene-name")]
    public string SceneName { get; set; }
}
