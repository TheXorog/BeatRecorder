namespace BeatRecorderUI;

public partial class InfoUI : Form
{
    public bool SettingsUpdated = false;
    public bool ShowConsoleAgain = false;

    bool loadedTopmost = false;

    public InfoUI(bool alwaysTopMost = false, double Transparency = 1.0)
    {
        InitializeComponent();

        loadedTopmost = alwaysTopMost;
    }

    private void InfoUI_Shown(object sender, EventArgs e)
    {
        this.TopMost = loadedTopmost;
    }

    private void OpenSettings_Click(object sender, EventArgs e)
    {
        SettingsUI settingsUI = new SettingsUI(this.TopMost);
        settingsUI.ShowDialog();

        if (settingsUI.SettingsUpdated)
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
}
