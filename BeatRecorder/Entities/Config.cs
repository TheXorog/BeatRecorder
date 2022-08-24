namespace BeatRecorder.Entities;

internal class Config
{
    /// <summary>
    /// Instructions to go to the repository for help
    /// </summary>
    public string README { get; set; } = "!! Please check https://github.com/TheXorog/BeatRecorder for more info and explainations for each config options !!";

    /// <summary>
    /// The current log level
    /// </summary>
    public LogLevel ConsoleLogLevel { get; set; } = LogLevel.INFO;

    /// <summary>
    /// What mod to connect to
    /// </summary>
    public string Mod { get; set; } = "datapuller";

    /// <summary>
    /// Whether to display the GUI
    /// </summary>
    public bool DisplayUI { get; set; } = true;

    /// <summary>
    /// Whether the GUI should always be above other windows
    /// </summary>
    public bool DisplayUITopmost { get; set; } = true;

    /// <summary>
    /// Whether the Console should be hidden entirely or just minimized upon loading the GUI
    /// </summary>
    public bool HideConsole { get; set; } = true;

    /// <summary>
    /// Record automatically
    /// </summary>
    public bool AutomaticRecording { get; set; } = true;

    /// <summary>
    /// Display Steam Notifications
    /// </summary>
    public bool DisplaySteamNotifications { get; set; } = false;

    /// <summary>
    /// The websocket server to connect to for fetching the ingame status
    /// </summary>
    public string BeatSaberUrl { get; set; } = "127.0.0.1";

    /// <summary>
    /// The port of the websocket server for the ingame status
    /// </summary>
    public string BeatSaberPort { get; set; } = "6557";

    /// <summary>
    /// The websocket server to connect to for obs
    /// </summary>
    public string OBSUrl { get; set; } = "127.0.0.1";

    /// <summary>
    /// The port of the websocket for obs
    /// </summary>
    public string OBSPort { get; set; } = "4444";

    /// <summary>
    /// The password for the obs websocket
    /// </summary>
    public string OBSPassword { get; set; } = "";

    /// <summary>
    /// How long to wait before starting another recording, used to avoid an issue where the recording doesn't start again after stopping
    /// </summary>
    public int MininumWaitUntilRecordingCanStart { get; set; } = 500;

    /// <summary>
    /// Only used when GUI is disabled. Whether to ask to save the password each time it was put in
    /// </summary>
    public bool AskToSaveOBSPassword { get; set; } = true;

    /// <summary>
    /// Whether the recording should be paused when the game has been paused
    /// </summary>
    public bool PauseRecordingOnIngamePause { get; set; } = false;

    /// <summary>
    /// The file name
    /// </summary>
    public string FileFormat { get; set; } = "[<rank>][<accuracy>][<max-combo>x] <song-name-with-sub> - <song-author> [<mapper>]";

    /// <summary>
    /// How long to wait before actually stopping the recording after exiting a song
    /// </summary>
    public int StopRecordingDelay { get; set; } = 5;

    /// <summary>
    /// Delete a song if shorter than x seconds
    /// </summary>
    public int DeleteIfShorterThan { get; set; } = 0;

    /// <summary>
    /// Whether to delete recordings where the song has been quit midway through
    /// </summary>
    public bool DeleteQuit { get; set; } = false;

    /// <summary>
    /// Whether to delete recordings where the song has been quit midway through, after failing with no-fail on
    /// </summary>
    public bool DeleteIfQuitAfterSoftFailed { get; set; } = false;

    /// <summary>
    /// Whether to delete recordings where the song has been failed
    /// </summary>
    public bool DeleteFailed { get; set; } = false;

    /// <summary>
    /// Whether to delete recordings where the song was failed with no-fail on
    /// </summary>
    public bool DeleteSoftFailed { get; set; } = false;

    /// <summary>
    /// What scene to switch to when in the menu
    /// </summary>
    public string OBSMenuScene { get; set; } = "";

    /// <summary>
    /// What scene to switch to when going ingame
    /// </summary>
    public string OBSIngameScene { get; set; } = "";

    /// <summary>
    /// What scene to switch to when pausing a song
    /// </summary>
    public string OBSPauseScene { get; set; } = "";

}
