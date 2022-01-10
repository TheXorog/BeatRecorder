namespace BeatRecorderUI;

public partial class InfoUI : Form
{
    public bool SettingsUpdated = false;

    public InfoUI(bool alwaysTopMost = false)
    {
        InitializeComponent();

        this.TopMost = alwaysTopMost;
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
}
