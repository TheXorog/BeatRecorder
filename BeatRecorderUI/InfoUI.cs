namespace BeatRecorderUI;

public partial class InfoUI : Form
{
    public bool SettingsUpdated = false;
    public bool ShowConsoleAgain = false;

    bool loadedTopmost = false;

    public bool SettingsRequired = false;

    public InfoUI(string version, bool alwaysTopMost = false)
    {
        InitializeComponent();

        loadedTopmost = alwaysTopMost;

        VersionLabel.Text = $"v{version}";
    }

    private void InfoUI_Shown(object sender, EventArgs e)
    {
        this.TopMost = loadedTopmost;
    }

    public void OpenSettings_Click(object sender, EventArgs e)
    {
        SettingsUI settingsUI = new(this.TopMost);

        if (SettingsRequired)
            this.Hide();

        settingsUI.ShowDialog();

        if (settingsUI.SettingsUpdated || SettingsRequired)
        {
            SettingsUpdated = true;
            this.Close();
        }
    }

    private void ShowConsole_Click(object sender, EventArgs e)
    {
        ShowConsoleAgain = true;
        this.Close();
    }

    private void Restart_Click(object sender, EventArgs e)
    {
        SettingsUpdated = true;
        this.Close();
    }

    private void CheckForUpdates_Click(object sender, EventArgs e)
    {
        System.Diagnostics.Process.Start("cmd", "/C start https://github.com/TheXorog/BeatRecorder/releases");
    }
}
