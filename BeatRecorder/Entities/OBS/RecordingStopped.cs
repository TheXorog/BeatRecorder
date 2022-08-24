namespace BeatRecorder.Entities.OBS;

public class RecordingStopped
{
    [JsonProperty("recordingFilename")]
    public string recordingFilename { get; set; }

    [JsonProperty("update-type")]
    public string UpdateType { get; set; }
}
