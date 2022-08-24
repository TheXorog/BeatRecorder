namespace BeatRecorder.Entities.OBS;

internal class ObsResponse
{
    [JsonProperty("message-id")]
    public string MessageId { get; set; }
    
    [JsonProperty("update-type")]
    public string UpdateType { get; set; }
    
    [JsonProperty("status")]
    public string Status { get; set; }
}
