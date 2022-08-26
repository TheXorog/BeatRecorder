namespace BeatRecorder.Entities.OBS.Legacy;

internal class AuthenticationRequired
{
    public bool authRequired { get; set; }
    public string challenge { get; set; }
    public string messageid { get; set; }
    public string salt { get; set; }
    public string status { get; set; }
}
