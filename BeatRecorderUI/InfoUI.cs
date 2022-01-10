namespace BeatRecorderUI;

public partial class InfoUI : Form
{
    public InfoUI(bool alwaysTopMost = false)
    {
        InitializeComponent();

        this.TopMost = alwaysTopMost;
    }

    private void OpenSettings_Click(object sender, EventArgs e)
    {
        SettingsUI settingsUI = new SettingsUI(this.TopMost);
        settingsUI.ShowDialog();
    }
}
