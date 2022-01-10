#pragma warning disable CS8601 // Possible null reference assignment.

namespace BeatRecorderUI;
public partial class SettingsUI : Form
{
    bool beTopMost = false;

    internal bool SettingsUpdated = false;

    internal bool SettingsRequired = false;

    public SettingsUI(bool topMost, bool settingsRequired = false)
    {
        InitializeComponent();

        beTopMost = topMost;
        SettingsRequired = settingsRequired;
    }

    BeatRecorder.Objects.Settings? _loadedSettings = null;

    private void SettingsUI_Shown(object sender, EventArgs e)
    {
        this.TopMost = beTopMost;

        this.Size = new Size(400, 730);

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

        try
        {
            ModSelectionBox.SelectedIndex = ModSelectionBox.Items.IndexOf(_loadedSettings.Mod);
            OBSPasswordBox.Text = _loadedSettings.OBSPassword;
            FileFormatBox.Text = _loadedSettings.FileFormat;
            StopRecordingDelay.Value = _loadedSettings.StopRecordingDelay;

            DeleteIfShorterThan.Value = _loadedSettings.DeleteIfShorterThan;
            DeleteIfQuit.Checked = _loadedSettings.DeleteQuit;
            DeleteIfQuitAfterSoftFailCheck.Checked = _loadedSettings.DeleteIfQuitAfterSoftFailed;
            DeleteIfSoftFailedCheck.Checked = _loadedSettings.DeleteSoftFailed;
            DeleteIfFailedCheck.Checked = _loadedSettings.DeleteFailed;

            IngameSceneBox.Text = _loadedSettings.OBSIngameScene;
            MenuSceneBox.Text = _loadedSettings.OBSMenuScene;
            PauseSceneBox.Text = _loadedSettings.OBSPauseScene;

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
        catch (Exception)
        {
            this.Hide();
            MessageBox.Show($"Failed to load settings, please avoid manually editing the config file from now on.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
            return;
        }
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
            this.Size = new Size(800, 730);

            BeatSaberIpBox.Enabled = true;
            BeatSaberPortBox.Enabled = true;

            OBSIpBox.Enabled = true;
            OBSPortBox.Enabled = true;

            DisplayUserInterfaceCheck.Enabled = true;
            AutomaticRecordingCheck.Enabled = true;
            EntirelyHideConsoleCheck.Enabled = true;
        }
        else
        {
            this.Size = new Size(400, 730);

            BeatSaberIpBox.Enabled = false;
            BeatSaberPortBox.Enabled = false;

            OBSIpBox.Enabled = false;
            OBSPortBox.Enabled = false;

            DisplayUserInterfaceCheck.Enabled = false;
            AutomaticRecordingCheck.Enabled = false;
            EntirelyHideConsoleCheck.Enabled = false;
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

        if (!AutomaticRecordingCheck.Checked)
            if (ModSelectionBox.Text == "http-status")
                MessageBox.Show("When using http-status, disabling this option will prevent seconds from being displayed correctly in the info ui", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        _loadedSettings.FileFormat = FileFormatBox.Text;
        _loadedSettings.StopRecordingDelay = Convert.ToInt32(StopRecordingDelay.Value);

        _loadedSettings.DeleteIfShorterThan = Convert.ToInt32(DeleteIfShorterThan.Value);
        _loadedSettings.DeleteQuit = DeleteIfQuit.Checked;
        _loadedSettings.DeleteIfQuitAfterSoftFailed = DeleteIfQuitAfterSoftFailCheck.Checked;
        _loadedSettings.DeleteSoftFailed = DeleteIfSoftFailedCheck.Checked;
        _loadedSettings.DeleteFailed = DeleteIfFailedCheck.Checked;

        _loadedSettings.OBSIngameScene = IngameSceneBox.Text;
        _loadedSettings.OBSMenuScene = MenuSceneBox.Text;
        _loadedSettings.OBSPauseScene = PauseSceneBox.Text;

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

    private void difficultyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(preEdit, "<difficulty>");

        FileFormatBox.SelectionStart = preEdit + "<difficulty>".Length;
    }

    private void shortDifficultyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<short-difficulty>");

        FileFormatBox.SelectionStart = preEdit + "<short-difficulty>".Length;
    }

    private void songNameToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<song-name>");

        FileFormatBox.SelectionStart = preEdit + "<song-name>".Length;
    }

    private void songAuthorToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<song-author>");

        FileFormatBox.SelectionStart = preEdit + "<song-author>".Length;
    }

    private void songSubNameToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<song-sub-name>");

        FileFormatBox.SelectionStart = preEdit + "<song-sub-name>".Length;
    }

    private void mapperToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<mapper>");

        FileFormatBox.SelectionStart = preEdit + "<mapper>".Length;
    }

    private void levelIdHashToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<levelid>");

        FileFormatBox.SelectionStart = preEdit + "<levelid>".Length;
    }

    private void bPMToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<bpm>");

        FileFormatBox.SelectionStart = preEdit + "<bpm>".Length;
    }

    private void rankToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<rank>");

        FileFormatBox.SelectionStart = preEdit + "<rank>".Length;
    }

    private void accuracyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<accuracy>");

        FileFormatBox.SelectionStart = preEdit + "<accuracy>".Length;
    }

    private void missesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<misses>");

        FileFormatBox.SelectionStart = preEdit + "<misses>".Length;
    }

    private void maxComboToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<max-combo>");

        FileFormatBox.SelectionStart = preEdit + "<max-combo>".Length;
    }

    private void scoreToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<score>");

        FileFormatBox.SelectionStart = preEdit + "<score>".Length;
    }

    private void rawScoreToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int preEdit = FileFormatBox.SelectionStart;

        FileFormatBox.Text = FileFormatBox.Text.Insert(FileFormatBox.SelectionStart, "<raw-score>");

        FileFormatBox.SelectionStart = preEdit + "<raw-score>".Length;
    }

    private void SettingsUI_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (SettingsRequired)
        {
            e.Cancel = true;
            SettingsRequired = false;
            Save.PerformClick();
        }
    }
}
