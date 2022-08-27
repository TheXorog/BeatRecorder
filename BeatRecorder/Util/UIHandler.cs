using BeatRecorderUI;
using System.Windows.Forms;
using static Xorog.UniversalExtensions.UniversalExtensionsEnums;

namespace BeatRecorder.Util;

public class UIHandler
{
    InfoUI infoUI { get; set; } = null;

    internal static UIHandler Initialize(Program program)
    {
        var instance = new UIHandler();

        Thread.Sleep(500);

        if (!program.LoadedConfig.HideConsole)
            ConsoleHelper.ShowWindow(ConsoleHelper.GetConsoleWindow(), 2);
        else
            ConsoleHelper.ShowWindow(ConsoleHelper.GetConsoleWindow(), 0);

        instance.infoUI = new InfoUI(Program.Version, program.LoadedConfig.DisplayUITopmost);

        _ = Task.Run(async () =>
        {
            var infoUI = instance.infoUI;

            Action UpdateUI = new(() =>
            {
                string WarningText = "";

                if (program.RunningPrerelease)
                    WarningText = "You're running a Pre-Release. Please expect bugs or unfinished features.\n";

                if (program.LoadedConfig.Mod == "beatsaberplus")
                    WarningText += "You're using BeatSaberPlus, filenames will not reflect whether you finished a song or not.";

                if (infoUI.label1.Text != WarningText)
                    infoUI.label1.Text = WarningText;

                if (program.ObsClient?.GetIsRunning() ?? false)
                {
                    if (program.ObsClient.GetIsRecording())
                    {
                        if (infoUI.OBSConnectionLabel.Text != "(REC) OBS")
                            infoUI.OBSConnectionLabel.Text = "(REC) OBS";

                        if (infoUI.OBSConnectionLabel.BackColor != Color.Orange)
                            infoUI.OBSConnectionLabel.BackColor = Color.Orange;
                    }
                    else
                    {
                        if (infoUI.OBSConnectionLabel.Text != "OBS")
                            infoUI.OBSConnectionLabel.Text = "OBS";

                        if (infoUI.OBSConnectionLabel.BackColor != Color.DarkGreen)
                            infoUI.OBSConnectionLabel.BackColor = Color.DarkGreen;
                    }
                }
                else
                {
                    if (infoUI.OBSConnectionLabel.BackColor != Color.DarkRed)
                        infoUI.OBSConnectionLabel.BackColor = Color.DarkRed;
                }

                if (program.BeatSaberClient?.GetIsRunning() ?? false)
                {
                    if (infoUI.BeatSaberConnectionLabel.BackColor != Color.DarkGreen)
                        infoUI.BeatSaberConnectionLabel.BackColor = Color.DarkGreen;

                    var cur = program.BeatSaberClient.GetCurrentStatus();

                    infoUI.SongNameLabel.Text = cur.BeatmapInfo.Name ?? "Title";
                    infoUI.SongAuthorLabel.Text = cur.BeatmapInfo.Author ?? "Artist";
                    infoUI.SongAuthorLabel.Location = new Point(infoUI.SongNameLabel.Location.X, infoUI.SongNameLabel.Location.Y + infoUI.SongNameLabel.Height);
                    infoUI.MapperLabel.Text = cur.BeatmapInfo.Creator ?? "Mapper";

                    infoUI.ProgressLabel.Text = $"{TimeSpan.FromSeconds(program.ObsClient.GetRecordingSeconds()).GetShortHumanReadable(TimeFormat.MINUTES)}";

                    infoUI.ScoreLabel.Text = String.Format("{0:n0}", cur.PerformanceInfo.Score);
                    infoUI.ComboLabel.Text = $"{cur.PerformanceInfo?.Combo ?? 0}x";
                    infoUI.AccuracyLabel.Text = $"{cur.PerformanceInfo.Accuracy}%";
                    infoUI.MissesLabel.Text = $"{cur.PerformanceInfo.CombinedMisses ?? 0} Misses";

                    if (cur.BeatmapInfo.Cover != null && infoUI.pictureBox1.Image != cur.BeatmapInfo.Cover)
                        infoUI.pictureBox1.Image = cur.BeatmapInfo.Cover;

                }
                else
                {
                    if (infoUI.BeatSaberConnectionLabel.BackColor != Color.DarkRed)
                        infoUI.BeatSaberConnectionLabel.BackColor = Color.DarkRed;
                }
            });

            while (true)
            {
                try
                {
                    if (infoUI.InvokeRequired)
                        infoUI.Invoke(UpdateUI);
                    else
                        UpdateUI();

                    await Task.Delay(1000);
                }
                catch { }
            }
        });

        _ = Task.Run(() =>
        {
            _logger.LogDebug("Initializing GUI..");

            instance.infoUI.ShowDialog();
            ConsoleHelper.ShowWindow(ConsoleHelper.GetConsoleWindow(), 5);

            if (instance.infoUI.ShowConsoleAgain)
            {
                instance.infoUI.ShowConsoleAgain = false;
                instance.infoUI.ShowConsole.Visible = false;

                instance.infoUI.ShowDialog();
            }

            if (instance.infoUI.SettingsUpdated)
            {
                _logger.LogDebug("Settings updated via GUI");
                Process.Start(Environment.ProcessPath);
                Thread.Sleep(500);
                Environment.Exit(0);
            }

            _logger.LogDebug("InfoUI closed");
            Environment.Exit(0);
        });

        return instance;
    }

    internal void ShowNotification(string Description, string Title = "", MessageBoxButtons messageBoxButtons = MessageBoxButtons.OK, MessageBoxIcon messageBoxIcon = MessageBoxIcon.None)
    {
        Action action = new(() =>
        {
            MessageBox.Show(Description, Title, messageBoxButtons, messageBoxIcon);
        });

        if (infoUI.InvokeRequired)
            infoUI.Invoke(action);
        else
            action();
    }
    
    internal void ShowSettings(bool Required = false)
    {
        Action action = new(() =>
        {
            infoUI.SettingsRequired = Required;
            infoUI.OpenSettings_Click(null, null);            
        });

        if (infoUI.InvokeRequired)
            infoUI.Invoke(action);
        else
            action();
    }
}
