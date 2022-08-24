namespace BeatRecorder.Entities.OBS;

internal class AuthenticateRequest : BaseRequest
{
    internal AuthenticateRequest(string auth, string id = null)
    {
        this.Auth = auth;
        this.RequestType = "Authenticate";
        this.MessageId = id ?? Guid.NewGuid().ToString();
    }

    [JsonProperty("auth")]
    internal string Auth { get; set; }
}
