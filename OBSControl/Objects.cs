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
        // Settings

        public static Settings LoadedSettings = new Settings();

        public class Settings
        {
            public string README { get; set; }
            public int ConfigVersion { get; set; }
            public int ConsoleLogLevel { get; set; }
            public string Mod { get; set; }
            public string BeatSaberUrl { get; set; }
            public string BeatSaberPort { get; set; }
            public string OBSUrl { get; set; }
            public string OBSPort { get; set; }
            public string OBSPassword { get; set; }
            public int MininumWaitUntilRecordingCanStart { get; set; }
            public bool AskToSaveOBSPassword { get; set; }
            public bool PauseRecordingOnIngamePause { get; set; }
            public string FileFormat { get; set; }
            public int StopRecordingDelay { get; set; }
            public int DeleteIfShorterThan { get; set; }
            public bool DeleteQuit { get; set; }
            public bool DeleteIfQuitAfterSoftFailed { get; set; }
            public bool DeleteFailed { get; set; }
            public bool DeleteSoftFailed { get; set; }
        }

        // Shedule-based logger

        public static List<LogEntry> LogsToPost = new List<LogEntry>();

        public class LogEntry
        {
            public DateTime TimeOfEvent { get; set; }
            public int LogLevel { get; set; }
            public int LogCount { get; set; }
            public string Message { get; set; }
        }

    }
}
