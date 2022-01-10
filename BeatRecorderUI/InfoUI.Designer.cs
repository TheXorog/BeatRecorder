namespace BeatRecorderUI;

partial class InfoUI
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.SongNameLabel = new System.Windows.Forms.Label();
            this.SongAuthorLabel = new System.Windows.Forms.Label();
            this.MapperLabel = new System.Windows.Forms.Label();
            this.BSRLabel = new System.Windows.Forms.Label();
            this.OpenSettings = new System.Windows.Forms.Button();
            this.ScoreLabel = new System.Windows.Forms.Label();
            this.ComboLabel = new System.Windows.Forms.Label();
            this.AccuracyLabel = new System.Windows.Forms.Label();
            this.MissesLabel = new System.Windows.Forms.Label();
            this.ProgressLabel = new System.Windows.Forms.Label();
            this.BeatSaberConnectionLabel = new System.Windows.Forms.Label();
            this.OBSConnectionLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(237, 237);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // SongNameLabel
            // 
            this.SongNameLabel.AutoSize = true;
            this.SongNameLabel.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.SongNameLabel.Location = new System.Drawing.Point(255, 12);
            this.SongNameLabel.Name = "SongNameLabel";
            this.SongNameLabel.Size = new System.Drawing.Size(118, 30);
            this.SongNameLabel.TabIndex = 1;
            this.SongNameLabel.Text = "Songname";
            // 
            // SongAuthorLabel
            // 
            this.SongAuthorLabel.AutoSize = true;
            this.SongAuthorLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.SongAuthorLabel.Location = new System.Drawing.Point(257, 42);
            this.SongAuthorLabel.Name = "SongAuthorLabel";
            this.SongAuthorLabel.Size = new System.Drawing.Size(86, 20);
            this.SongAuthorLabel.TabIndex = 2;
            this.SongAuthorLabel.Text = "Songauthor";
            // 
            // MapperLabel
            // 
            this.MapperLabel.AutoSize = true;
            this.MapperLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.MapperLabel.Location = new System.Drawing.Point(255, 234);
            this.MapperLabel.Name = "MapperLabel";
            this.MapperLabel.Size = new System.Drawing.Size(48, 15);
            this.MapperLabel.TabIndex = 3;
            this.MapperLabel.Text = "Mapper";
            // 
            // BSRLabel
            // 
            this.BSRLabel.AutoSize = true;
            this.BSRLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.BSRLabel.Location = new System.Drawing.Point(255, 219);
            this.BSRLabel.Name = "BSRLabel";
            this.BSRLabel.Size = new System.Drawing.Size(27, 15);
            this.BSRLabel.TabIndex = 4;
            this.BSRLabel.Text = "BSR";
            // 
            // OpenSettings
            // 
            this.OpenSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(84)))), ((int)(((byte)(0)))), ((int)(((byte)(84)))));
            this.OpenSettings.FlatAppearance.BorderSize = 0;
            this.OpenSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OpenSettings.Location = new System.Drawing.Point(689, 253);
            this.OpenSettings.Name = "OpenSettings";
            this.OpenSettings.Size = new System.Drawing.Size(83, 27);
            this.OpenSettings.TabIndex = 5;
            this.OpenSettings.Text = "Settings..";
            this.OpenSettings.UseVisualStyleBackColor = false;
            // 
            // ScoreLabel
            // 
            this.ScoreLabel.AutoSize = true;
            this.ScoreLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ScoreLabel.Location = new System.Drawing.Point(257, 101);
            this.ScoreLabel.Name = "ScoreLabel";
            this.ScoreLabel.Size = new System.Drawing.Size(36, 15);
            this.ScoreLabel.TabIndex = 6;
            this.ScoreLabel.Text = "Score";
            // 
            // ComboLabel
            // 
            this.ComboLabel.AutoSize = true;
            this.ComboLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ComboLabel.Location = new System.Drawing.Point(257, 116);
            this.ComboLabel.Name = "ComboLabel";
            this.ComboLabel.Size = new System.Drawing.Size(47, 15);
            this.ComboLabel.TabIndex = 7;
            this.ComboLabel.Text = "Combo";
            // 
            // AccuracyLabel
            // 
            this.AccuracyLabel.AutoSize = true;
            this.AccuracyLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.AccuracyLabel.Location = new System.Drawing.Point(257, 131);
            this.AccuracyLabel.Name = "AccuracyLabel";
            this.AccuracyLabel.Size = new System.Drawing.Size(56, 15);
            this.AccuracyLabel.TabIndex = 8;
            this.AccuracyLabel.Text = "Accuracy";
            // 
            // MissesLabel
            // 
            this.MissesLabel.AutoSize = true;
            this.MissesLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.MissesLabel.Location = new System.Drawing.Point(257, 146);
            this.MissesLabel.Name = "MissesLabel";
            this.MissesLabel.Size = new System.Drawing.Size(42, 15);
            this.MissesLabel.TabIndex = 9;
            this.MissesLabel.Text = "Misses";
            // 
            // ProgressLabel
            // 
            this.ProgressLabel.AutoSize = true;
            this.ProgressLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.ProgressLabel.Location = new System.Drawing.Point(15, 221);
            this.ProgressLabel.Name = "ProgressLabel";
            this.ProgressLabel.Size = new System.Drawing.Size(118, 25);
            this.ProgressLabel.TabIndex = 10;
            this.ProgressLabel.Text = "00:00/00:00";
            // 
            // BeatSaberConnectionLabel
            // 
            this.BeatSaberConnectionLabel.BackColor = System.Drawing.Color.DarkRed;
            this.BeatSaberConnectionLabel.Location = new System.Drawing.Point(12, 250);
            this.BeatSaberConnectionLabel.Name = "BeatSaberConnectionLabel";
            this.BeatSaberConnectionLabel.Size = new System.Drawing.Size(118, 30);
            this.BeatSaberConnectionLabel.TabIndex = 11;
            this.BeatSaberConnectionLabel.Text = "BeatSaber";
            this.BeatSaberConnectionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // OBSConnectionLabel
            // 
            this.OBSConnectionLabel.BackColor = System.Drawing.Color.DarkRed;
            this.OBSConnectionLabel.Location = new System.Drawing.Point(130, 250);
            this.OBSConnectionLabel.Name = "OBSConnectionLabel";
            this.OBSConnectionLabel.Size = new System.Drawing.Size(119, 30);
            this.OBSConnectionLabel.TabIndex = 12;
            this.OBSConnectionLabel.Text = "OBS";
            this.OBSConnectionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // InfoUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(23)))), ((int)(((byte)(23)))));
            this.ClientSize = new System.Drawing.Size(784, 291);
            this.Controls.Add(this.OBSConnectionLabel);
            this.Controls.Add(this.BeatSaberConnectionLabel);
            this.Controls.Add(this.ProgressLabel);
            this.Controls.Add(this.MissesLabel);
            this.Controls.Add(this.AccuracyLabel);
            this.Controls.Add(this.ComboLabel);
            this.Controls.Add(this.ScoreLabel);
            this.Controls.Add(this.OpenSettings);
            this.Controls.Add(this.BSRLabel);
            this.Controls.Add(this.MapperLabel);
            this.Controls.Add(this.SongAuthorLabel);
            this.Controls.Add(this.SongNameLabel);
            this.Controls.Add(this.pictureBox1);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "InfoUI";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "BeatRecorder";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private PictureBox pictureBox1;
    private Label SongNameLabel;
    private Label SongAuthorLabel;
    private Label MapperLabel;
    private Label BSRLabel;
    private Button OpenSettings;
    private Label ScoreLabel;
    private Label ComboLabel;
    private Label AccuracyLabel;
    private Label MissesLabel;
    private Label ProgressLabel;
    private Label BeatSaberConnectionLabel;
    private Label OBSConnectionLabel;
}
