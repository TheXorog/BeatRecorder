namespace BeatRecorderUI;

public partial class SettingsUI : Form
{
    bool beTopMost = false;

    internal bool SettingsUpdated = false;

    internal bool SettingsRequired = false;

    public SettingsUI(bool topMost, bool settingsRequired = false)
    {
        this.InitializeComponent();

        this.beTopMost = topMost;
        this.SettingsRequired = settingsRequired;
    }

    BeatRecorder.Entities.Config _loadedSettings = null;

    private void SettingsUI_Shown(object sender, EventArgs e)
    {
        this.TopMost = this.beTopMost;

        this.Size = new Size(400, 730);

        this.BeatSaberIpBox.Enabled = false;
        this.BeatSaberPortBox.Enabled = false;

        this.OBSIpBox.Enabled = false;
        this.OBSPortBox.Enabled = false;
        this.OBSLegacyPortBox.Enabled = false;

        this.DisplayUserInterfaceCheck.Enabled = false;
        this.AutomaticRecordingCheck.Enabled = false;

        if (File.Exists("Settings.json"))
            this._loadedSettings = JsonConvert.DeserializeObject<BeatRecorder.Entities.Config>(File.ReadAllText("Settings.json"));

        if (this._loadedSettings == null)
        {
            this.Hide();
            _ = MessageBox.Show($"Failed to load Settings", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
            return;
        }

        try
        {
            this.ModSelectionBox.SelectedIndex = this.ModSelectionBox.Items.IndexOf(this._loadedSettings.Mod);
            this.OBSPasswordBox.Text = this._loadedSettings.OBSPassword;
            this.FileFormatBox.Text = this._loadedSettings.FileFormat;
            this.StopRecordingDelay.Value = this._loadedSettings.StopRecordingDelay;

            this.DeleteIfShorterThan.Value = this._loadedSettings.DeleteIfShorterThan;
            this.DeleteIfQuit.Checked = this._loadedSettings.DeleteQuit;
            this.DeleteIfQuitAfterSoftFailCheck.Checked = this._loadedSettings.DeleteIfQuitAfterSoftFailed;
            this.DeleteIfSoftFailedCheck.Checked = this._loadedSettings.DeleteSoftFailed;
            this.DeleteIfFailedCheck.Checked = this._loadedSettings.DeleteFailed;

            this.IngameSceneBox.Text = this._loadedSettings.OBSIngameScene;
            this.MenuSceneBox.Text = this._loadedSettings.OBSMenuScene;
            this.PauseSceneBox.Text = this._loadedSettings.OBSPauseScene;

            this.DisplaySteamNotificationsCheck.Checked = this._loadedSettings.DisplaySteamNotifications;
            this.AlwaysTopMostCheck.Checked = this._loadedSettings.DisplayUITopmost;

            this.BeatSaberIpBox.Text = this._loadedSettings.BeatSaberUrl;
            this.BeatSaberPortBox.Text = this._loadedSettings.BeatSaberPort;
            this.OBSIpBox.Text = this._loadedSettings.OBSUrl;
            this.OBSPortBox.Text = this._loadedSettings.OBSPortModern;
            this.OBSLegacyPortBox.Text = this._loadedSettings.OBSPortLegacy;
            this.DisplayUserInterfaceCheck.Checked = this._loadedSettings.DisplayUI;
            this.AutomaticRecordingCheck.Checked = this._loadedSettings.AutomaticRecording;
            this.PauseOnIngamePauseCheck.Checked = this._loadedSettings.PauseRecordingOnIngamePause;
            this.EntirelyHideConsoleCheck.Checked = this._loadedSettings.HideConsole;
        }
        catch (Exception)
        {
            this.Hide();
            _ = MessageBox.Show($"Failed to load settings, please avoid manually editing the config file from now on.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
            return;
        }
    }

    private void ShowAdvancedSettings_Click(object sender, EventArgs e)
    {
        if (this.ShowAdvancedSettings.Checked)
        {
            if (MessageBox.Show("Please only modify these settings if you know what you're doing or you've been instructed to.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                this.ShowAdvancedSettings.Checked = false;
            }
        }

        if (this.ShowAdvancedSettings.Checked)
        {
            this.Size = new Size(800, 730);

            this.BeatSaberIpBox.Enabled = true;
            this.BeatSaberPortBox.Enabled = true;

            this.OBSIpBox.Enabled = true;
            this.OBSPortBox.Enabled = true;
            this.OBSLegacyPortBox.Enabled = true;

            this.DisplayUserInterfaceCheck.Enabled = true;
            this.AutomaticRecordingCheck.Enabled = true;
            this.EntirelyHideConsoleCheck.Enabled = true;
        }
        else
        {
            this.Size = new Size(400, 730);

            this.BeatSaberIpBox.Enabled = false;
            this.BeatSaberPortBox.Enabled = false;

            this.OBSIpBox.Enabled = false;
            this.OBSPortBox.Enabled = false;
            this.OBSLegacyPortBox.Enabled = false;

            this.DisplayUserInterfaceCheck.Enabled = false;
            this.AutomaticRecordingCheck.Enabled = false;
            this.EntirelyHideConsoleCheck.Enabled = false;
        }
    }

    private void AutomaticRecording_Click(object sender, EventArgs e)
    {
        if (!this.AutomaticRecordingCheck.Checked)
        {
            if (MessageBox.Show("This will disable automatic recording of your gameplay.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                this.AutomaticRecordingCheck.Checked = true;
            }
        }

        if (!this.AutomaticRecordingCheck.Checked)
            if (this.ModSelectionBox.Text == "http-status")
                _ = MessageBox.Show("When using http-status, disabling this option will prevent seconds from being displayed correctly in the info ui", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void ModSelectionBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!this.ModSelectionBox.Items.Contains(this.ModSelectionBox.Text))
        {
            this.ModSelectionBox.SelectedIndex = 0;
        }

        if (this.ModSelectionBox.Text == "datapuller")
            this.BeatSaberPortBox.Value = 2946;
        else if (this.ModSelectionBox.Text == "http-status")
            this.BeatSaberPortBox.Value = 6557;
        else if (this.ModSelectionBox.Text == "beatsaberplus")
        {
            _ = Task.Run(() => _ = MessageBox.Show("BeatSaberPlus Integration is currently incomplete. Filenames will always appear as if you finished the song.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning));
            this.BeatSaberPortBox.Value = 2947;
        }
    }

    private void ModSelectionBox_TextChanged(object sender, EventArgs e)
    {

    }

    private void ModSelectionBox_TextUpdate(object sender, EventArgs e)
    {
        if (!this.ModSelectionBox.Items.Contains(this.ModSelectionBox.Text))
        {
            this.ModSelectionBox.SelectedIndex = 0;
        }

        if (this.ModSelectionBox.Text == "datapuller")
            this.BeatSaberPortBox.Value = 2946;
        else if (this.ModSelectionBox.Text == "http-status")
            this.BeatSaberPortBox.Value = 6557;
        else if (this.ModSelectionBox.Text == "beatsaberplus")
        {
            _ = Task.Run(() => _ = MessageBox.Show("BeatSaberPlus Integration is currently incomplete. Filenames will always appear as if you finished the song.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning));
            this.BeatSaberPortBox.Value = 2947;
        }
    }

    private void Cancel_Click(object sender, EventArgs e) => this.Close();

    private void Save_Click(object sender, EventArgs e)
    {
        if (this._loadedSettings is null)
            return;

        this._loadedSettings.Mod = this.ModSelectionBox.Text;
        this._loadedSettings.OBSPassword = this.OBSPasswordBox.Text;
        this._loadedSettings.FileFormat = this.FileFormatBox.Text;
        this._loadedSettings.StopRecordingDelay = Convert.ToInt32(this.StopRecordingDelay.Value);

        this._loadedSettings.DeleteIfShorterThan = Convert.ToInt32(this.DeleteIfShorterThan.Value);
        this._loadedSettings.DeleteQuit = this.DeleteIfQuit.Checked;
        this._loadedSettings.DeleteIfQuitAfterSoftFailed = this.DeleteIfQuitAfterSoftFailCheck.Checked;
        this._loadedSettings.DeleteSoftFailed = this.DeleteIfSoftFailedCheck.Checked;
        this._loadedSettings.DeleteFailed = this.DeleteIfFailedCheck.Checked;

        this._loadedSettings.OBSIngameScene = this.IngameSceneBox.Text;
        this._loadedSettings.OBSMenuScene = this.MenuSceneBox.Text;
        this._loadedSettings.OBSPauseScene = this.PauseSceneBox.Text;

        this._loadedSettings.DisplaySteamNotifications = this.DisplaySteamNotificationsCheck.Checked;
        this._loadedSettings.DisplayUITopmost = this.AlwaysTopMostCheck.Checked;

        this._loadedSettings.BeatSaberUrl = this.BeatSaberIpBox.Text;
        this._loadedSettings.BeatSaberPort = this.BeatSaberPortBox.Text;
        this._loadedSettings.OBSUrl = this.OBSIpBox.Text;
        this._loadedSettings.OBSPortModern = this.OBSPortBox.Text;
        this._loadedSettings.OBSPortLegacy = this.OBSLegacyPortBox.Text;
        this._loadedSettings.DisplayUI = this.DisplayUserInterfaceCheck.Checked;
        this._loadedSettings.AutomaticRecording = this.AutomaticRecordingCheck.Checked;
        this._loadedSettings.PauseRecordingOnIngamePause = this.PauseOnIngamePauseCheck.Checked;
        this._loadedSettings.HideConsole = this.EntirelyHideConsoleCheck.Checked;

        File.WriteAllText("Settings.json", JsonConvert.SerializeObject(this._loadedSettings, Formatting.Indented));
        this.SettingsUpdated = true;
        this.Close();
    }

    private void difficultyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(preEdit, "<difficulty>");

        this.FileFormatBox.SelectionStart = preEdit + "<difficulty>".Length;
    }

    private void shortDifficultyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<short-difficulty>");

        this.FileFormatBox.SelectionStart = preEdit + "<short-difficulty>".Length;
    }

    private void songNameToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<song-name>");

        this.FileFormatBox.SelectionStart = preEdit + "<song-name>".Length;
    }

    private void songNameWithSubNameToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<song-name-with-sub>");

        this.FileFormatBox.SelectionStart = preEdit + "<song-name-with-sub>".Length;
    }

    private void songAuthorToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<song-author>");

        this.FileFormatBox.SelectionStart = preEdit + "<song-author>".Length;
    }

    private void songSubNameToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<song-sub-name>");

        this.FileFormatBox.SelectionStart = preEdit + "<song-sub-name>".Length;
    }

    private void mapperToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<mapper>");

        this.FileFormatBox.SelectionStart = preEdit + "<mapper>".Length;
    }

    private void levelIdHashToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<levelid>");

        this.FileFormatBox.SelectionStart = preEdit + "<levelid>".Length;
    }

    private void bPMToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<bpm>");

        this.FileFormatBox.SelectionStart = preEdit + "<bpm>".Length;
    }

    private void rankToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<rank>");

        this.FileFormatBox.SelectionStart = preEdit + "<rank>".Length;
    }

    private void accuracyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<accuracy>");

        this.FileFormatBox.SelectionStart = preEdit + "<accuracy>".Length;
    }

    private void missesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<misses>");

        this.FileFormatBox.SelectionStart = preEdit + "<misses>".Length;
    }

    private void maxComboToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<max-combo>");

        this.FileFormatBox.SelectionStart = preEdit + "<max-combo>".Length;
    }

    private void scoreToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<score>");

        this.FileFormatBox.SelectionStart = preEdit + "<score>".Length;
    }

    private void rawScoreToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var preEdit = this.FileFormatBox.SelectionStart;

        this.FileFormatBox.Text = this.FileFormatBox.Text.Insert(this.FileFormatBox.SelectionStart, "<raw-score>");

        this.FileFormatBox.SelectionStart = preEdit + "<raw-score>".Length;
    }

    private void SettingsUI_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (this.SettingsRequired)
        {
            e.Cancel = true;
            this.SettingsRequired = false;
            this.Save.PerformClick();
        }
    }
}
