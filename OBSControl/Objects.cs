using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBSControl
{
    class Objects
    {
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

        public static Settings LoadedSettings = new Settings();

        public class Settings
        {
            public string README { get; set; } = "!! Please check https://github.com/XorogVEVO/OBSControl for more info and explainations for each config options !!";
            public int ConsoleLogLevel { get; set; } = 3;
            public string Mod { get; set; } = "http-status";
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

    }
}
