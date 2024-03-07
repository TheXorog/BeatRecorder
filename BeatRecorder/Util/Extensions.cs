namespace BeatRecorder.Util;

internal static class Extensions
{
    /// <summary>
    /// Used for authentication with OBS Websocket if authentication is required ("Borrowed" from https://github.com/BarRaider/obs-websocket-dotnet/blob/268b7f6c52d8daf8e8d08cf517812009c6f9cc26/obs-websocket-dotnet/OBSWebsocket.cs#L797)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    internal static string HashEncode(this string input)
    {
        var textBytes = Encoding.ASCII.GetBytes(input);
        var hash = SHA256.HashData(textBytes);

        return System.Convert.ToBase64String(hash);
    }
}
