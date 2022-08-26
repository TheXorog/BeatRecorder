namespace BeatRecorder.Entities.OBS;

internal abstract class BaseRequest
{
    public int op { get; set; }
    public object d { get; set; }

    internal string Build() => JsonConvert.SerializeObject(this);
}
