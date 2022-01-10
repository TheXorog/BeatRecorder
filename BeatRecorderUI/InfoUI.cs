namespace BeatRecorderUI;

public partial class InfoUI : Form
{
    public InfoUI()
    {
        InitializeComponent();

        this.TopMost = true;
    }

    private void OpenSettings_Click(object sender, EventArgs e)
    {
        SettingsUI settingsUI = new SettingsUI(this.TopMost);
        settingsUI.ShowDialog();
    }
}
