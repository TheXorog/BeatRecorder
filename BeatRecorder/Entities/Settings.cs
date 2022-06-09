namespace BeatRecorder.Entities;

internal class Settings
{
    public string README { get; set; } = "!! Please check https://github.com/TheXorog/BeatRecorder for more info and explainations for each config options !!";
    public LogLevel ConsoleLogLevel { get; set; } = LogLevel.INFO;
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

}
