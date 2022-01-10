#pragma warning disable CS8601 // Possible null reference assignment.

namespace BeatRecorderUI;
public partial class SettingsUI : Form
{
    bool beTopMost = false;

    internal bool SettingsUpdated = false;

    public SettingsUI(bool topMost)
    {
        InitializeComponent();

        beTopMost = topMost;
    }

    BeatRecorder.Objects.Settings? _loadedSettings = null;

    private void SettingsUI_Shown(object sender, EventArgs e)
    {
        this.TopMost = beTopMost;

        this.Size = new Size(400, 600);

        BeatSaberIpBox.Enabled = false;
        BeatSaberPortBox.Enabled = false;

        OBSIpBox.Enabled = false;
        OBSPortBox.Enabled = false;

        DisplayUserInterfaceCheck.Enabled = false;
        AutomaticRecordingCheck.Enabled = false;

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
        PauseOnIngamePauseCheck.Checked = _loadedSettings.PauseRecordingOnIngamePause;
        EntirelyHideConsoleCheck.Checked = _loadedSettings.HideConsole;
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
            this.Size = new Size(800, 600);

            BeatSaberIpBox.Enabled = true;
            BeatSaberPortBox.Enabled = true;

            OBSIpBox.Enabled = true;
            OBSPortBox.Enabled = true;

            DisplayUserInterfaceCheck.Enabled = true;
            AutomaticRecordingCheck.Enabled = true;
        }
        else
        {
            this.Size = new Size(400, 600);

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

    private void ModSelectionBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!ModSelectionBox.Items.Contains(ModSelectionBox.Text))
        {
            ModSelectionBox.SelectedIndex = 0;
        }

        if (ModSelectionBox.Text == "datapuller")
            BeatSaberPortBox.Value = 2946;

        if (ModSelectionBox.Text == "http-status")
            BeatSaberPortBox.Value = 6557;
    }

    private void ModSelectionBox_TextChanged(object sender, EventArgs e)
    {

    }

    private void ModSelectionBox_TextUpdate(object sender, EventArgs e)
    {
        if (!ModSelectionBox.Items.Contains(ModSelectionBox.Text))
        {
            ModSelectionBox.SelectedIndex = 0;
        }

        if (ModSelectionBox.Text == "datapuller")
            BeatSaberPortBox.Value = 2946;

        if (ModSelectionBox.Text == "http-status")
            BeatSaberPortBox.Value = 6557;
    }

    private void Cancel_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    private void Save_Click(object sender, EventArgs e)
    {
        if (_loadedSettings is null)
            return;

        _loadedSettings.Mod = ModSelectionBox.Text;
        _loadedSettings.OBSPassword = OBSPasswordBox.Text;
        _loadedSettings.AskToSaveOBSPassword = AskForPassOBSPasswordCheck.Checked;
        _loadedSettings.FileFormat = FileFormatBox.Text;
        _loadedSettings.StopRecordingDelay = Convert.ToInt32(StopRecordingDelay.Value);

        _loadedSettings.DeleteIfShorterThan = Convert.ToInt32(DeleteIfShorterThan.Value);
        _loadedSettings.DeleteQuit = DeleteIfQuit.Checked;
        _loadedSettings.DeleteIfQuitAfterSoftFailed = DeleteIfQuitAfterSoftFailCheck.Checked;
        _loadedSettings.DeleteSoftFailed = DeleteIfSoftFailedCheck.Checked;
        _loadedSettings.DeleteFailed = DeleteIfFailedCheck.Checked;

        _loadedSettings.DisplaySteamNotifications = DisplaySteamNotificationsCheck.Checked;
        _loadedSettings.DisplayUITopmost = AlwaysTopMostCheck.Checked;

        _loadedSettings.BeatSaberUrl = BeatSaberIpBox.Text;
        _loadedSettings.BeatSaberPort = BeatSaberPortBox.Text;
        _loadedSettings.OBSUrl = OBSIpBox.Text;
        _loadedSettings.OBSPort = OBSPortBox.Text;
        _loadedSettings.DisplayUI = DisplayUserInterfaceCheck.Checked;
        _loadedSettings.AutomaticRecording = AutomaticRecordingCheck.Checked;
        _loadedSettings.PauseRecordingOnIngamePause = PauseOnIngamePauseCheck.Checked;
        _loadedSettings.HideConsole = EntirelyHideConsoleCheck.Checked;

        File.WriteAllText("Settings.json", JsonConvert.SerializeObject(_loadedSettings, Formatting.Indented));
        SettingsUpdated = true;
        this.Close();
    }
}
