namespace BeatRecorder;

class Objects
{
    public static bool SettingsRequired = false;
    public static bool UpdateAvailable = false;
    public static string UpdateText = "";

    public static ulong SteamNotificationId = 0;

    public static ConnectionTypeWarning LastDP1Warning { get; set; }
    public static ConnectionTypeWarning LastHttpStatusWarning { get; set; }
    public static ConnectionTypeWarning LastOBSWarning { get; set; }
}
