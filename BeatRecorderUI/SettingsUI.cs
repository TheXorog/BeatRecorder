#pragma warning disable CS8601 // Possible null reference assignment.

namespace BeatRecorderUI;
public partial class SettingsUI : Form
{
    bool beTopMost = false;

    public SettingsUI(bool topMost)
    {
        InitializeComponent();

        beTopMost = topMost;
    }

    private void SettingsUI_Shown(object sender, EventArgs e)
    {
        this.TopMost = beTopMost;

        this.Size = new Size(400, 500);

        BeatSaberIpBox.Enabled = false;
        BeatSaberPortBox.Enabled = false;

        OBSIpBox.Enabled = false;
        OBSPortBox.Enabled = false;

        DisplayUserInterfaceCheck.Enabled = false;
        AutomaticRecordingCheck.Enabled = false;


        BeatRecorder.Objects.Settings? _loadedSettings = null;

        if (File.Exists("Settings.json"))
            _loadedSettings = JsonConvert.DeserializeObject<BeatRecorder.Objects.Settings>(File.ReadAllText("Settings.json"));

        if (_loadedSettings == null)
        {
            this.Hide();
            MessageBox.Show($"Failed to load Settings", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
            return;
        }

        ModSelectionBox.SelectedIndex = ModSelectionBox.Items.IndexOf(_loadedSettings.Mod);
        OBSPasswordBox.Text = _loadedSettings.OBSPassword;
        AskForPassOBSPasswordCheck.Checked = _loadedSettings.AskToSaveOBSPassword;
        FileFormatBox.Text = _loadedSettings.FileFormat;
        StopRecordingDelay.Value = _loadedSettings.StopRecordingDelay;

        DeleteIfShorterThan.Value = _loadedSettings.DeleteIfShorterThan;
        DeleteIfQuit.Checked = _loadedSettings.DeleteQuit;
        DeleteIfQuitAfterSoftFailCheck.Checked = _loadedSettings.DeleteIfQuitAfterSoftFailed;
        DeleteIfSoftFailedCheck.Checked = _loadedSettings.DeleteSoftFailed;
        DeleteIfFailedCheck.Checked = _loadedSettings.DeleteFailed;

        DisplaySteamNotificationsCheck.Checked = _loadedSettings.DisplaySteamNotifications;
        AlwaysTopMostCheck.Checked = _loadedSettings.DisplayUITopmost;

        BeatSaberIpBox.Text = _loadedSettings.BeatSaberUrl;
        BeatSaberPortBox.Text = _loadedSettings.BeatSaberPort;
        OBSIpBox.Text = _loadedSettings.OBSUrl;
        OBSPortBox.Text = _loadedSettings.OBSPort;
        DisplayUserInterfaceCheck.Checked = _loadedSettings.DisplayUI;
        AutomaticRecordingCheck.Checked = _loadedSettings.AutomaticRecording;
    }

    private void ShowAdvancedSettings_Click(object sender, EventArgs e)
    {
        if (ShowAdvancedSettings.Checked)
        {
            if (MessageBox.Show("Please only modify these settings if you know what you're doing or you've been instructed to.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                ShowAdvancedSettings.Checked = false;
            }
        }

        if (ShowAdvancedSettings.Checked)
        {
            this.Size = new Size(800, 500);

            BeatSaberIpBox.Enabled = true;
            BeatSaberPortBox.Enabled = true;

            OBSIpBox.Enabled = true;
            OBSPortBox.Enabled = true;

            DisplayUserInterfaceCheck.Enabled = true;
            AutomaticRecordingCheck.Enabled = true;
        }
        else
        {
            this.Size = new Size(400, 500);

            BeatSaberIpBox.Enabled = false;
            BeatSaberPortBox.Enabled = false;

            OBSIpBox.Enabled = false;
            OBSPortBox.Enabled = false;

            DisplayUserInterfaceCheck.Enabled = false;
            AutomaticRecordingCheck.Enabled = false;
        }
    }

    private void AutomaticRecording_Click(object sender, EventArgs e)
    {
        if (!AutomaticRecordingCheck.Checked)
        {
            if (MessageBox.Show("This will disable automatic recording of your gameplay.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                AutomaticRecordingCheck.Checked = true;
            }
        }
    }
}
