namespace BeatRecorderUI;

public partial class InfoUI : Form
{
    public bool SettingsUpdated = false;
    public bool ShowConsoleAgain = false;

    bool loadedTopmost = false;

    public bool SettingsRequired = false;

    public InfoUI(string version, bool alwaysTopMost = false)
    {
        this.InitializeComponent();

        this.loadedTopmost = alwaysTopMost;

        this.VersionLabel.Text = $"v{version}";
    }

    private void InfoUI_Shown(object sender, EventArgs e) => this.TopMost = this.loadedTopmost;

    public void OpenSettings_Click(object sender, EventArgs e)
    {
        SettingsUI settingsUI = new(this.TopMost);

        if (this.SettingsRequired)
            this.Hide();

        _ = settingsUI.ShowDialog();

        if (settingsUI.SettingsUpdated || this.SettingsRequired)
        {
            this.SettingsUpdated = true;
            this.Close();
        }
    }

    private void ShowConsole_Click(object sender, EventArgs e)
    {
        this.ShowConsoleAgain = true;
        this.Close();
    }

    private void Restart_Click(object sender, EventArgs e)
    {
        this.SettingsUpdated = true;
        this.Close();
    }

    private void CheckForUpdates_Click(object sender, EventArgs e) => _ = System.Diagnostics.Process.Start("cmd", "/C start https://github.com/TheXorog/BeatRecorder/releases");
}
