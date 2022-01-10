#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace BeatRecorder;

class Objects
{
    public static bool SettingsRequired = false;

    public static ulong SteamNotificationId = 0;

    public static ConnectionTypeWarning LastDP1Warning { get; set; }
    public static ConnectionTypeWarning LastHttpStatusWarning { get; set; }
    public static ConnectionTypeWarning LastOBSWarning { get; set; }

    public enum ConnectionTypeWarning
    {
        CONNECTED,
        MOD_INSTALLED,
        MOD_NOT_INSTALLED,
        NOT_MODDED,
        NO_PROCESS
    }

    public static Settings LoadedSettings = new();

    public class Settings
    {
        public string README { get; set; } = "!! Please check https://github.com/TheXorog/BeatRecorder for more info and explainations for each config options !!";
        public LogLevel ConsoleLogLevelEnum { get; set; } = LogLevel.INFO;
        public string Mod { get; set; } = "http-status";
        public bool DisplayUI { get; set; } = true;
        public bool DisplayUITopmost { get; set; } = true;
        public bool HideConsole { get; set; } = true;
        public bool AutomaticRecording { get; set; } = true;
        public bool DisplaySteamNotifications { get; set; } = false;
        public string BeatSaberUrl { get; set; } = "127.0.0.1";
        public string BeatSaberPort { get; set; } = "6557";
        public string OBSUrl { get; set; } = "127.0.0.1";
        public string OBSPort { get; set; } = "4444";
        public string OBSPassword { get; set; } = "";
        public int MininumWaitUntilRecordingCanStart { get; set; } = 500;
        public bool AskToSaveOBSPassword { get; set; } = true;
        public bool PauseRecordingOnIngamePause { get; set; } = false;
        public string FileFormat { get; set; } = "[<rank>][<accuracy>][<max-combo>x] <song-name> - <song-author> [<mapper>]";
        public int StopRecordingDelay { get; set; } = 5;
        public int DeleteIfShorterThan { get; set; } = 0;
        public bool DeleteQuit { get; set; } = false;
        public bool DeleteIfQuitAfterSoftFailed { get; set; } = false;
        public bool DeleteFailed { get; set; } = false;
        public bool DeleteSoftFailed { get; set; } = false;
        public string OBSMenuScene { get; set; } = "";
        public string OBSIngameScene { get; set; } = "";
        public string OBSPauseScene { get; set; } = "";

        // Ignore old Console Log Level
        [JsonProperty("ConsoleLogLevel")]
        private int MigrationConsoleLogLevel { set { } }
    }

    // Shedule-based logger

    public static List<LogEntry> LogsToPost = new();

    public class LogEntry
    {
        public DateTime TimeOfEvent { get; set; }
        public int LogLevel { get; set; }
        public int LogCount { get; set; }
        public string Message { get; set; }
    }

    public enum MessageType
    {
        ERROR,
        INFO
    }

    public class NotificationEntry
    {
        public string Message { get; set; }
        public int Delay { get; set; } = 2000;
        public MessageType Type { get; set; } = MessageType.INFO;
    }
}
