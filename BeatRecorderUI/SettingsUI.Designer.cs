namespace BeatRecorderUI;

partial class SettingsUI
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsUI));
            this.BeatSaberIpBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.BeatSaberPortBox = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.OBSIpBox = new System.Windows.Forms.TextBox();
            this.OBSPortBox = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.ModSelectionBox = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.OBSPasswordBox = new System.Windows.Forms.TextBox();
            this.DisplaySteamNotificationsCheck = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.FileFormatBox = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.difficultyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shortDifficultyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.songNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.songAuthorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.songSubNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.levelIdHashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bPMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rankToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.accuracyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.missesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.maxComboToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rawScoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StopRecordingDelay = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.DeleteIfShorterThan = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.DeleteIfQuit = new System.Windows.Forms.CheckBox();
            this.DeleteIfQuitAfterSoftFailCheck = new System.Windows.Forms.CheckBox();
            this.DeleteIfFailedCheck = new System.Windows.Forms.CheckBox();
            this.DeleteIfSoftFailedCheck = new System.Windows.Forms.CheckBox();
            this.ShowAdvancedSettings = new System.Windows.Forms.CheckBox();
            this.DisplayUserInterfaceCheck = new System.Windows.Forms.CheckBox();
            this.AlwaysTopMostCheck = new System.Windows.Forms.CheckBox();
            this.AutomaticRecordingCheck = new System.Windows.Forms.CheckBox();
            this.Save = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.PauseOnIngamePauseCheck = new System.Windows.Forms.CheckBox();
            this.MenuSceneBox = new System.Windows.Forms.TextBox();
            this.IngameSceneBox = new System.Windows.Forms.TextBox();
            this.PauseSceneBox = new System.Windows.Forms.TextBox();
            this.EntirelyHideConsoleCheck = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.BeatSaberPortBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OBSPortBox)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.StopRecordingDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeleteIfShorterThan)).BeginInit();
            this.SuspendLayout();
            // 
            // BeatSaberIpBox
            // 
            this.BeatSaberIpBox.BackColor = System.Drawing.Color.Black;
            this.BeatSaberIpBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BeatSaberIpBox.Enabled = false;
            this.BeatSaberIpBox.ForeColor = System.Drawing.Color.White;
            this.BeatSaberIpBox.Location = new System.Drawing.Point(412, 31);
            this.BeatSaberIpBox.Name = "BeatSaberIpBox";
            this.BeatSaberIpBox.Size = new System.Drawing.Size(360, 23);
            this.BeatSaberIpBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(412, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Beat Saber IP";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(412, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Beat Saber Port";
            // 
            // BeatSaberPortBox
            // 
            this.BeatSaberPortBox.BackColor = System.Drawing.Color.Black;
            this.BeatSaberPortBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BeatSaberPortBox.Enabled = false;
            this.BeatSaberPortBox.ForeColor = System.Drawing.Color.White;
            this.BeatSaberPortBox.Location = new System.Drawing.Point(412, 77);
            this.BeatSaberPortBox.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.BeatSaberPortBox.Name = "BeatSaberPortBox";
            this.BeatSaberPortBox.Size = new System.Drawing.Size(360, 23);
            this.BeatSaberPortBox.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(412, 103);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "OBS Websocket IP";
            // 
            // OBSIpBox
            // 
            this.OBSIpBox.BackColor = System.Drawing.Color.Black;
            this.OBSIpBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OBSIpBox.Enabled = false;
            this.OBSIpBox.ForeColor = System.Drawing.Color.White;
            this.OBSIpBox.Location = new System.Drawing.Point(412, 125);
            this.OBSIpBox.Name = "OBSIpBox";
            this.OBSIpBox.Size = new System.Drawing.Size(360, 23);
            this.OBSIpBox.TabIndex = 7;
            // 
            // OBSPortBox
            // 
            this.OBSPortBox.BackColor = System.Drawing.Color.Black;
            this.OBSPortBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OBSPortBox.Enabled = false;
            this.OBSPortBox.ForeColor = System.Drawing.Color.White;
            this.OBSPortBox.Location = new System.Drawing.Point(412, 171);
            this.OBSPortBox.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.OBSPortBox.Name = "OBSPortBox";
            this.OBSPortBox.Size = new System.Drawing.Size(360, 23);
            this.OBSPortBox.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(412, 151);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(134, 17);
            this.label5.TabIndex = 9;
            this.label5.Text = "OBS Websocket Port";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(12, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 17);
            this.label6.TabIndex = 11;
            this.label6.Text = "Mod";
            // 
            // ModSelectionBox
            // 
            this.ModSelectionBox.BackColor = System.Drawing.Color.Black;
            this.ModSelectionBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ModSelectionBox.ForeColor = System.Drawing.Color.White;
            this.ModSelectionBox.FormattingEnabled = true;
            this.ModSelectionBox.Items.AddRange(new object[] {
            "http-status",
            "datapuller"});
            this.ModSelectionBox.Location = new System.Drawing.Point(12, 29);
            this.ModSelectionBox.Name = "ModSelectionBox";
            this.ModSelectionBox.Size = new System.Drawing.Size(360, 23);
            this.ModSelectionBox.TabIndex = 12;
            this.toolTip1.SetToolTip(this.ModSelectionBox, "What mod should BeatRecorder connect to?");
            this.ModSelectionBox.SelectedIndexChanged += new System.EventHandler(this.ModSelectionBox_SelectedIndexChanged);
            this.ModSelectionBox.TextUpdate += new System.EventHandler(this.ModSelectionBox_TextUpdate);
            this.ModSelectionBox.TextChanged += new System.EventHandler(this.ModSelectionBox_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(12, 56);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(166, 17);
            this.label7.TabIndex = 14;
            this.label7.Text = "OBS Websocket Password";
            // 
            // OBSPasswordBox
            // 
            this.OBSPasswordBox.BackColor = System.Drawing.Color.Black;
            this.OBSPasswordBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OBSPasswordBox.ForeColor = System.Drawing.Color.White;
            this.OBSPasswordBox.Location = new System.Drawing.Point(12, 78);
            this.OBSPasswordBox.Name = "OBSPasswordBox";
            this.OBSPasswordBox.PasswordChar = '*';
            this.OBSPasswordBox.Size = new System.Drawing.Size(360, 23);
            this.OBSPasswordBox.TabIndex = 13;
            this.toolTip1.SetToolTip(this.OBSPasswordBox, "The password BeatRecorder should use, if required, to connect to OBS Websocket");
            this.OBSPasswordBox.UseSystemPasswordChar = true;
            // 
            // DisplaySteamNotificationsCheck
            // 
            this.DisplaySteamNotificationsCheck.AutoSize = true;
            this.DisplaySteamNotificationsCheck.FlatAppearance.BorderSize = 0;
            this.DisplaySteamNotificationsCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DisplaySteamNotificationsCheck.ForeColor = System.Drawing.Color.White;
            this.DisplaySteamNotificationsCheck.Location = new System.Drawing.Point(12, 419);
            this.DisplaySteamNotificationsCheck.Name = "DisplaySteamNotificationsCheck";
            this.DisplaySteamNotificationsCheck.Size = new System.Drawing.Size(248, 19);
            this.DisplaySteamNotificationsCheck.TabIndex = 16;
            this.DisplaySteamNotificationsCheck.Text = "Display Steam Notifications (Experimental)";
            this.toolTip1.SetToolTip(this.DisplaySteamNotificationsCheck, resources.GetString("DisplaySteamNotificationsCheck.ToolTip"));
            this.DisplaySteamNotificationsCheck.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label8.ForeColor = System.Drawing.Color.White;
            this.label8.Location = new System.Drawing.Point(12, 104);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 17);
            this.label8.TabIndex = 18;
            this.label8.Text = "File Format";
            // 
            // FileFormatBox
            // 
            this.FileFormatBox.BackColor = System.Drawing.Color.Black;
            this.FileFormatBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FileFormatBox.ContextMenuStrip = this.contextMenuStrip1;
            this.FileFormatBox.ForeColor = System.Drawing.Color.White;
            this.FileFormatBox.Location = new System.Drawing.Point(12, 126);
            this.FileFormatBox.Name = "FileFormatBox";
            this.FileFormatBox.Size = new System.Drawing.Size(360, 23);
            this.FileFormatBox.TabIndex = 17;
            this.toolTip1.SetToolTip(this.FileFormatBox, "How your saved file should be formatted. Right-Click for fill-in options");
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.difficultyToolStripMenuItem,
            this.shortDifficultyToolStripMenuItem,
            this.songNameToolStripMenuItem,
            this.songAuthorToolStripMenuItem,
            this.songSubNameToolStripMenuItem,
            this.mapperToolStripMenuItem,
            this.levelIdHashToolStripMenuItem,
            this.bPMToolStripMenuItem,
            this.rankToolStripMenuItem,
            this.accuracyToolStripMenuItem,
            this.missesToolStripMenuItem,
            this.maxComboToolStripMenuItem,
            this.scoreToolStripMenuItem,
            this.rawScoreToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(160, 312);
            // 
            // difficultyToolStripMenuItem
            // 
            this.difficultyToolStripMenuItem.Name = "difficultyToolStripMenuItem";
            this.difficultyToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.difficultyToolStripMenuItem.Text = "Difficulty";
            this.difficultyToolStripMenuItem.Click += new System.EventHandler(this.difficultyToolStripMenuItem_Click);
            // 
            // shortDifficultyToolStripMenuItem
            // 
            this.shortDifficultyToolStripMenuItem.Name = "shortDifficultyToolStripMenuItem";
            this.shortDifficultyToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.shortDifficultyToolStripMenuItem.Text = "Short Difficulty";
            this.shortDifficultyToolStripMenuItem.Click += new System.EventHandler(this.shortDifficultyToolStripMenuItem_Click);
            // 
            // songNameToolStripMenuItem
            // 
            this.songNameToolStripMenuItem.Name = "songNameToolStripMenuItem";
            this.songNameToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.songNameToolStripMenuItem.Text = "Song Name";
            this.songNameToolStripMenuItem.Click += new System.EventHandler(this.songNameToolStripMenuItem_Click);
            // 
            // songAuthorToolStripMenuItem
            // 
            this.songAuthorToolStripMenuItem.Name = "songAuthorToolStripMenuItem";
            this.songAuthorToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.songAuthorToolStripMenuItem.Text = "Song Author";
            this.songAuthorToolStripMenuItem.Click += new System.EventHandler(this.songAuthorToolStripMenuItem_Click);
            // 
            // songSubNameToolStripMenuItem
            // 
            this.songSubNameToolStripMenuItem.Name = "songSubNameToolStripMenuItem";
            this.songSubNameToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.songSubNameToolStripMenuItem.Text = "Song Sub Name";
            this.songSubNameToolStripMenuItem.Click += new System.EventHandler(this.songSubNameToolStripMenuItem_Click);
            // 
            // mapperToolStripMenuItem
            // 
            this.mapperToolStripMenuItem.Name = "mapperToolStripMenuItem";
            this.mapperToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.mapperToolStripMenuItem.Text = "Mapper";
            this.mapperToolStripMenuItem.Click += new System.EventHandler(this.mapperToolStripMenuItem_Click);
            // 
            // levelIdHashToolStripMenuItem
            // 
            this.levelIdHashToolStripMenuItem.Name = "levelIdHashToolStripMenuItem";
            this.levelIdHashToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.levelIdHashToolStripMenuItem.Text = "Level Id/Hash";
            this.levelIdHashToolStripMenuItem.Click += new System.EventHandler(this.levelIdHashToolStripMenuItem_Click);
            // 
            // bPMToolStripMenuItem
            // 
            this.bPMToolStripMenuItem.Name = "bPMToolStripMenuItem";
            this.bPMToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.bPMToolStripMenuItem.Text = "BPM";
            this.bPMToolStripMenuItem.Click += new System.EventHandler(this.bPMToolStripMenuItem_Click);
            // 
            // rankToolStripMenuItem
            // 
            this.rankToolStripMenuItem.Name = "rankToolStripMenuItem";
            this.rankToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.rankToolStripMenuItem.Text = "Rank";
            this.rankToolStripMenuItem.Click += new System.EventHandler(this.rankToolStripMenuItem_Click);
            // 
            // accuracyToolStripMenuItem
            // 
            this.accuracyToolStripMenuItem.Name = "accuracyToolStripMenuItem";
            this.accuracyToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.accuracyToolStripMenuItem.Text = "Accuracy";
            this.accuracyToolStripMenuItem.Click += new System.EventHandler(this.accuracyToolStripMenuItem_Click);
            // 
            // missesToolStripMenuItem
            // 
            this.missesToolStripMenuItem.Name = "missesToolStripMenuItem";
            this.missesToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.missesToolStripMenuItem.Text = "Misses";
            this.missesToolStripMenuItem.Click += new System.EventHandler(this.missesToolStripMenuItem_Click);
            // 
            // maxComboToolStripMenuItem
            // 
            this.maxComboToolStripMenuItem.Name = "maxComboToolStripMenuItem";
            this.maxComboToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.maxComboToolStripMenuItem.Text = "Max-Combo";
            this.maxComboToolStripMenuItem.Click += new System.EventHandler(this.maxComboToolStripMenuItem_Click);
            // 
            // scoreToolStripMenuItem
            // 
            this.scoreToolStripMenuItem.Name = "scoreToolStripMenuItem";
            this.scoreToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.scoreToolStripMenuItem.Text = "Score";
            this.scoreToolStripMenuItem.Click += new System.EventHandler(this.scoreToolStripMenuItem_Click);
            // 
            // rawScoreToolStripMenuItem
            // 
            this.rawScoreToolStripMenuItem.Name = "rawScoreToolStripMenuItem";
            this.rawScoreToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.rawScoreToolStripMenuItem.Text = "Raw Score";
            this.rawScoreToolStripMenuItem.Click += new System.EventHandler(this.rawScoreToolStripMenuItem_Click);
            // 
            // StopRecordingDelay
            // 
            this.StopRecordingDelay.BackColor = System.Drawing.Color.Black;
            this.StopRecordingDelay.ForeColor = System.Drawing.Color.White;
            this.StopRecordingDelay.Location = new System.Drawing.Point(12, 172);
            this.StopRecordingDelay.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.StopRecordingDelay.Name = "StopRecordingDelay";
            this.StopRecordingDelay.Size = new System.Drawing.Size(297, 23);
            this.StopRecordingDelay.TabIndex = 20;
            this.toolTip1.SetToolTip(this.StopRecordingDelay, "How long BeatRecorder should wait after a song to stop the recording");
            this.StopRecordingDelay.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label9.ForeColor = System.Drawing.Color.White;
            this.label9.Location = new System.Drawing.Point(12, 152);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(141, 17);
            this.label9.TabIndex = 19;
            this.label9.Text = "Stop Recording Delay";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label10.ForeColor = System.Drawing.Color.White;
            this.label10.Location = new System.Drawing.Point(315, 174);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(57, 17);
            this.label10.TabIndex = 21;
            this.label10.Text = "seconds";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label11.ForeColor = System.Drawing.Color.White;
            this.label11.Location = new System.Drawing.Point(315, 220);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(57, 17);
            this.label11.TabIndex = 24;
            this.label11.Text = "seconds";
            // 
            // DeleteIfShorterThan
            // 
            this.DeleteIfShorterThan.BackColor = System.Drawing.Color.Black;
            this.DeleteIfShorterThan.ForeColor = System.Drawing.Color.White;
            this.DeleteIfShorterThan.Location = new System.Drawing.Point(12, 218);
            this.DeleteIfShorterThan.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.DeleteIfShorterThan.Name = "DeleteIfShorterThan";
            this.DeleteIfShorterThan.Size = new System.Drawing.Size(297, 23);
            this.DeleteIfShorterThan.TabIndex = 23;
            this.toolTip1.SetToolTip(this.DeleteIfShorterThan, "How long a recording needs to be for BeatRecorder to keep it");
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label12.ForeColor = System.Drawing.Color.White;
            this.label12.Location = new System.Drawing.Point(12, 198);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(204, 17);
            this.label12.TabIndex = 22;
            this.label12.Text = "Delete recording if shorter than";
            // 
            // DeleteIfQuit
            // 
            this.DeleteIfQuit.AutoSize = true;
            this.DeleteIfQuit.FlatAppearance.BorderSize = 0;
            this.DeleteIfQuit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteIfQuit.ForeColor = System.Drawing.Color.White;
            this.DeleteIfQuit.Location = new System.Drawing.Point(12, 247);
            this.DeleteIfQuit.Name = "DeleteIfQuit";
            this.DeleteIfQuit.Size = new System.Drawing.Size(196, 19);
            this.DeleteIfQuit.TabIndex = 25;
            this.DeleteIfQuit.Text = "Delete recording if song was quit";
            this.DeleteIfQuit.UseVisualStyleBackColor = true;
            // 
            // DeleteIfQuitAfterSoftFailCheck
            // 
            this.DeleteIfQuitAfterSoftFailCheck.AutoSize = true;
            this.DeleteIfQuitAfterSoftFailCheck.FlatAppearance.BorderSize = 0;
            this.DeleteIfQuitAfterSoftFailCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteIfQuitAfterSoftFailCheck.ForeColor = System.Drawing.Color.White;
            this.DeleteIfQuitAfterSoftFailCheck.Location = new System.Drawing.Point(12, 272);
            this.DeleteIfQuitAfterSoftFailCheck.Name = "DeleteIfQuitAfterSoftFailCheck";
            this.DeleteIfQuitAfterSoftFailCheck.Size = new System.Drawing.Size(284, 19);
            this.DeleteIfQuitAfterSoftFailCheck.TabIndex = 26;
            this.DeleteIfQuitAfterSoftFailCheck.Text = "Delete recording if song was quit after soft-failing";
            this.DeleteIfQuitAfterSoftFailCheck.UseVisualStyleBackColor = true;
            // 
            // DeleteIfFailedCheck
            // 
            this.DeleteIfFailedCheck.AutoSize = true;
            this.DeleteIfFailedCheck.FlatAppearance.BorderSize = 0;
            this.DeleteIfFailedCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteIfFailedCheck.ForeColor = System.Drawing.Color.White;
            this.DeleteIfFailedCheck.Location = new System.Drawing.Point(12, 322);
            this.DeleteIfFailedCheck.Name = "DeleteIfFailedCheck";
            this.DeleteIfFailedCheck.Size = new System.Drawing.Size(204, 19);
            this.DeleteIfFailedCheck.TabIndex = 27;
            this.DeleteIfFailedCheck.Text = "Delete recording if song was failed";
            this.DeleteIfFailedCheck.UseVisualStyleBackColor = true;
            // 
            // DeleteIfSoftFailedCheck
            // 
            this.DeleteIfSoftFailedCheck.AutoSize = true;
            this.DeleteIfSoftFailedCheck.FlatAppearance.BorderSize = 0;
            this.DeleteIfSoftFailedCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DeleteIfSoftFailedCheck.ForeColor = System.Drawing.Color.White;
            this.DeleteIfSoftFailedCheck.Location = new System.Drawing.Point(12, 297);
            this.DeleteIfSoftFailedCheck.Name = "DeleteIfSoftFailedCheck";
            this.DeleteIfSoftFailedCheck.Size = new System.Drawing.Size(177, 19);
            this.DeleteIfSoftFailedCheck.TabIndex = 28;
            this.DeleteIfSoftFailedCheck.Text = "Delete recording if soft-failed";
            this.DeleteIfSoftFailedCheck.UseVisualStyleBackColor = true;
            // 
            // ShowAdvancedSettings
            // 
            this.ShowAdvancedSettings.AutoSize = true;
            this.ShowAdvancedSettings.FlatAppearance.BorderSize = 0;
            this.ShowAdvancedSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ShowAdvancedSettings.ForeColor = System.Drawing.Color.White;
            this.ShowAdvancedSettings.Location = new System.Drawing.Point(296, 656);
            this.ShowAdvancedSettings.Name = "ShowAdvancedSettings";
            this.ShowAdvancedSettings.Size = new System.Drawing.Size(76, 19);
            this.ShowAdvancedSettings.TabIndex = 29;
            this.ShowAdvancedSettings.Text = "Advanced";
            this.ShowAdvancedSettings.UseVisualStyleBackColor = true;
            this.ShowAdvancedSettings.Click += new System.EventHandler(this.ShowAdvancedSettings_Click);
            // 
            // DisplayUserInterfaceCheck
            // 
            this.DisplayUserInterfaceCheck.AutoSize = true;
            this.DisplayUserInterfaceCheck.Enabled = false;
            this.DisplayUserInterfaceCheck.FlatAppearance.BorderSize = 0;
            this.DisplayUserInterfaceCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DisplayUserInterfaceCheck.ForeColor = System.Drawing.Color.White;
            this.DisplayUserInterfaceCheck.Location = new System.Drawing.Point(412, 201);
            this.DisplayUserInterfaceCheck.Name = "DisplayUserInterfaceCheck";
            this.DisplayUserInterfaceCheck.Size = new System.Drawing.Size(189, 19);
            this.DisplayUserInterfaceCheck.TabIndex = 30;
            this.DisplayUserInterfaceCheck.Text = "Display Graphical User Interface";
            this.DisplayUserInterfaceCheck.UseVisualStyleBackColor = true;
            // 
            // AlwaysTopMostCheck
            // 
            this.AlwaysTopMostCheck.AutoSize = true;
            this.AlwaysTopMostCheck.FlatAppearance.BorderSize = 0;
            this.AlwaysTopMostCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AlwaysTopMostCheck.ForeColor = System.Drawing.Color.White;
            this.AlwaysTopMostCheck.Location = new System.Drawing.Point(12, 394);
            this.AlwaysTopMostCheck.Name = "AlwaysTopMostCheck";
            this.AlwaysTopMostCheck.Size = new System.Drawing.Size(211, 19);
            this.AlwaysTopMostCheck.TabIndex = 31;
            this.AlwaysTopMostCheck.Text = "Always display BeatRecorder on top";
            this.toolTip1.SetToolTip(this.AlwaysTopMostCheck, "Always display BeatRecorder in front of everything else");
            this.AlwaysTopMostCheck.UseVisualStyleBackColor = true;
            // 
            // AutomaticRecordingCheck
            // 
            this.AutomaticRecordingCheck.AutoSize = true;
            this.AutomaticRecordingCheck.Enabled = false;
            this.AutomaticRecordingCheck.FlatAppearance.BorderSize = 0;
            this.AutomaticRecordingCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AutomaticRecordingCheck.ForeColor = System.Drawing.Color.White;
            this.AutomaticRecordingCheck.Location = new System.Drawing.Point(412, 226);
            this.AutomaticRecordingCheck.Name = "AutomaticRecordingCheck";
            this.AutomaticRecordingCheck.Size = new System.Drawing.Size(134, 19);
            this.AutomaticRecordingCheck.TabIndex = 32;
            this.AutomaticRecordingCheck.Text = "Automatically record";
            this.AutomaticRecordingCheck.UseVisualStyleBackColor = true;
            this.AutomaticRecordingCheck.Click += new System.EventHandler(this.AutomaticRecording_Click);
            // 
            // Save
            // 
            this.Save.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(0)))), ((int)(((byte)(84)))));
            this.Save.FlatAppearance.BorderSize = 0;
            this.Save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Save.ForeColor = System.Drawing.Color.White;
            this.Save.Location = new System.Drawing.Point(13, 652);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(83, 27);
            this.Save.TabIndex = 33;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = false;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // Cancel
            // 
            this.Cancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(0)))), ((int)(((byte)(84)))));
            this.Cancel.FlatAppearance.BorderSize = 0;
            this.Cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Cancel.ForeColor = System.Drawing.Color.White;
            this.Cancel.Location = new System.Drawing.Point(102, 652);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(83, 27);
            this.Cancel.TabIndex = 34;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = false;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // PauseOnIngamePauseCheck
            // 
            this.PauseOnIngamePauseCheck.AutoSize = true;
            this.PauseOnIngamePauseCheck.FlatAppearance.BorderSize = 0;
            this.PauseOnIngamePauseCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PauseOnIngamePauseCheck.ForeColor = System.Drawing.Color.White;
            this.PauseOnIngamePauseCheck.Location = new System.Drawing.Point(12, 359);
            this.PauseOnIngamePauseCheck.Name = "PauseOnIngamePauseCheck";
            this.PauseOnIngamePauseCheck.Size = new System.Drawing.Size(203, 19);
            this.PauseOnIngamePauseCheck.TabIndex = 35;
            this.PauseOnIngamePauseCheck.Text = "Pause recording if game is paused";
            this.toolTip1.SetToolTip(this.PauseOnIngamePauseCheck, "Always display BeatRecorder in front of everything else");
            this.PauseOnIngamePauseCheck.UseVisualStyleBackColor = true;
            // 
            // MenuSceneBox
            // 
            this.MenuSceneBox.BackColor = System.Drawing.Color.Black;
            this.MenuSceneBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MenuSceneBox.ContextMenuStrip = this.contextMenuStrip1;
            this.MenuSceneBox.ForeColor = System.Drawing.Color.White;
            this.MenuSceneBox.Location = new System.Drawing.Point(12, 470);
            this.MenuSceneBox.Name = "MenuSceneBox";
            this.MenuSceneBox.PlaceholderText = "Don\'t switch scene when going into the menu";
            this.MenuSceneBox.Size = new System.Drawing.Size(360, 23);
            this.MenuSceneBox.TabIndex = 37;
            this.toolTip1.SetToolTip(this.MenuSceneBox, "The obs scene that should be displayed while in the menu");
            // 
            // IngameSceneBox
            // 
            this.IngameSceneBox.BackColor = System.Drawing.Color.Black;
            this.IngameSceneBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.IngameSceneBox.ContextMenuStrip = this.contextMenuStrip1;
            this.IngameSceneBox.ForeColor = System.Drawing.Color.White;
            this.IngameSceneBox.Location = new System.Drawing.Point(12, 518);
            this.IngameSceneBox.Name = "IngameSceneBox";
            this.IngameSceneBox.PlaceholderText = "Don\'t switch scene when going ingame";
            this.IngameSceneBox.Size = new System.Drawing.Size(360, 23);
            this.IngameSceneBox.TabIndex = 39;
            this.toolTip1.SetToolTip(this.IngameSceneBox, "The obs scene that should be displayed while in a song");
            // 
            // PauseSceneBox
            // 
            this.PauseSceneBox.BackColor = System.Drawing.Color.Black;
            this.PauseSceneBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PauseSceneBox.ContextMenuStrip = this.contextMenuStrip1;
            this.PauseSceneBox.ForeColor = System.Drawing.Color.White;
            this.PauseSceneBox.Location = new System.Drawing.Point(12, 566);
            this.PauseSceneBox.Name = "PauseSceneBox";
            this.PauseSceneBox.PlaceholderText = "Don\'t switch scene when pausing the game";
            this.PauseSceneBox.Size = new System.Drawing.Size(360, 23);
            this.PauseSceneBox.TabIndex = 41;
            this.toolTip1.SetToolTip(this.PauseSceneBox, "The obs scene that should be displayed while in the pause menu");
            // 
            // EntirelyHideConsoleCheck
            // 
            this.EntirelyHideConsoleCheck.AutoSize = true;
            this.EntirelyHideConsoleCheck.Enabled = false;
            this.EntirelyHideConsoleCheck.FlatAppearance.BorderSize = 0;
            this.EntirelyHideConsoleCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.EntirelyHideConsoleCheck.ForeColor = System.Drawing.Color.White;
            this.EntirelyHideConsoleCheck.Location = new System.Drawing.Point(412, 251);
            this.EntirelyHideConsoleCheck.Name = "EntirelyHideConsoleCheck";
            this.EntirelyHideConsoleCheck.Size = new System.Drawing.Size(136, 19);
            this.EntirelyHideConsoleCheck.TabIndex = 36;
            this.EntirelyHideConsoleCheck.Text = "Entirely Hide Console";
            this.EntirelyHideConsoleCheck.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label13.ForeColor = System.Drawing.Color.White;
            this.label13.Location = new System.Drawing.Point(12, 448);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(166, 17);
            this.label13.TabIndex = 38;
            this.label13.Text = "Menu Scene (exact name)";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label14.ForeColor = System.Drawing.Color.White;
            this.label14.Location = new System.Drawing.Point(12, 496);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(177, 17);
            this.label14.TabIndex = 40;
            this.label14.Text = "Ingame Scene (exact name)";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label15.ForeColor = System.Drawing.Color.White;
            this.label15.Location = new System.Drawing.Point(12, 544);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(167, 17);
            this.label15.TabIndex = 42;
            this.label15.Text = "Pause Scene (exact name)";
            // 
            // label16
            // 
            this.label16.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label16.ForeColor = System.Drawing.Color.Gray;
            this.label16.Location = new System.Drawing.Point(12, 632);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(360, 17);
            this.label16.TabIndex = 43;
            this.label16.Text = "You can hover over items to get more details";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SettingsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(23)))), ((int)(((byte)(23)))));
            this.ClientSize = new System.Drawing.Size(784, 691);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.PauseSceneBox);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.IngameSceneBox);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.MenuSceneBox);
            this.Controls.Add(this.EntirelyHideConsoleCheck);
            this.Controls.Add(this.PauseOnIngamePauseCheck);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.AutomaticRecordingCheck);
            this.Controls.Add(this.AlwaysTopMostCheck);
            this.Controls.Add(this.DisplayUserInterfaceCheck);
            this.Controls.Add(this.ShowAdvancedSettings);
            this.Controls.Add(this.DeleteIfSoftFailedCheck);
            this.Controls.Add(this.DeleteIfFailedCheck);
            this.Controls.Add(this.DeleteIfQuitAfterSoftFailCheck);
            this.Controls.Add(this.DeleteIfQuit);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.DeleteIfShorterThan);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.StopRecordingDelay);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.FileFormatBox);
            this.Controls.Add(this.DisplaySteamNotificationsCheck);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.OBSPasswordBox);
            this.Controls.Add(this.ModSelectionBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.OBSPortBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.OBSIpBox);
            this.Controls.Add(this.BeatSaberPortBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BeatSaberIpBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsUI";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BeatRecorder Setttings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsUI_FormClosing);
            this.Shown += new System.EventHandler(this.SettingsUI_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.BeatSaberPortBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OBSPortBox)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.StopRecordingDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeleteIfShorterThan)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private TextBox BeatSaberIpBox;
    private Label label2;
    private Label label3;
    private NumericUpDown BeatSaberPortBox;
    private Label label4;
    private TextBox OBSIpBox;
    private NumericUpDown OBSPortBox;
    private Label label5;
    private Label label6;
    private ComboBox ModSelectionBox;
    private Label label7;
    private TextBox OBSPasswordBox;
    private CheckBox DisplaySteamNotificationsCheck;
    private Label label8;
    private TextBox FileFormatBox;
    private NumericUpDown StopRecordingDelay;
    private Label label9;
    private Label label10;
    private Label label11;
    private NumericUpDown DeleteIfShorterThan;
    private Label label12;
    private CheckBox DeleteIfQuit;
    private CheckBox DeleteIfQuitAfterSoftFailCheck;
    private CheckBox DeleteIfFailedCheck;
    private CheckBox DeleteIfSoftFailedCheck;
    private CheckBox ShowAdvancedSettings;
    private CheckBox DisplayUserInterfaceCheck;
    private CheckBox AlwaysTopMostCheck;
    private CheckBox AutomaticRecordingCheck;
    private Button Save;
    private Button Cancel;
    private ToolTip toolTip1;
    private CheckBox PauseOnIngamePauseCheck;
    private CheckBox EntirelyHideConsoleCheck;
    private ContextMenuStrip contextMenuStrip1;
    private ToolStripMenuItem difficultyToolStripMenuItem;
    private ToolStripMenuItem shortDifficultyToolStripMenuItem;
    private ToolStripMenuItem songNameToolStripMenuItem;
    private ToolStripMenuItem songAuthorToolStripMenuItem;
    private ToolStripMenuItem songSubNameToolStripMenuItem;
    private ToolStripMenuItem mapperToolStripMenuItem;
    private ToolStripMenuItem levelIdHashToolStripMenuItem;
    private ToolStripMenuItem bPMToolStripMenuItem;
    private ToolStripMenuItem rankToolStripMenuItem;
    private ToolStripMenuItem accuracyToolStripMenuItem;
    private ToolStripMenuItem missesToolStripMenuItem;
    private ToolStripMenuItem maxComboToolStripMenuItem;
    private ToolStripMenuItem scoreToolStripMenuItem;
    private ToolStripMenuItem rawScoreToolStripMenuItem;
    private TextBox MenuSceneBox;
    private Label label13;
    private Label label14;
    private TextBox IngameSceneBox;
    private Label label15;
    private TextBox PauseSceneBox;
    public Label label16;
}
