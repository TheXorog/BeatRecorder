namespace BeatRecorder.Entities.OBS.Legacy;

internal abstract class BaseRequest
{
    [JsonProperty("request-type")]
    public string RequestType { get; set; }

    [JsonProperty("message-id")]
    public string MessageId { get; set; }

    internal string Build() => JsonConvert.SerializeObject(this);
}
