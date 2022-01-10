namespace BeatRecorder;

internal class UIHandler
{
    internal async Task HandleUI()
    {
        await Task.Delay(500);

        if (!Objects.LoadedSettings.HideConsole)
            Program.ShowWindow(Program.GetConsoleWindow(), 2);
        else
            Program.ShowWindow(Program.GetConsoleWindow(), 0);

        LogDebug($"Displaying InfoUI");

        var infoUI = new InfoUI(Objects.LoadedSettings.DisplayUITopmost);

        _ = Task.Run(async () =>
        {
            string lastCover = "";
            Image coverArt = null;

            Action UpdateUI = new Action(() =>
            {
            try
            {
                    if (Program.obsWebSocket.IsRunning)
                    {
                        if (infoUI.OBSConnectionLabel.BackColor != Color.DarkGreen && infoUI.OBSConnectionLabel.BackColor != Color.Orange)
                            infoUI.OBSConnectionLabel.BackColor = Color.DarkGreen;
                    }
                    else
                    {
                        if (infoUI.OBSConnectionLabel.BackColor != Color.DarkRed)
                            infoUI.OBSConnectionLabel.BackColor = Color.DarkRed;
                    }

                    if (Program.beatSaberWebSocket.IsRunning)
                    {
                        if (infoUI.BeatSaberConnectionLabel.BackColor != Color.DarkGreen)
                            infoUI.BeatSaberConnectionLabel.BackColor = Color.DarkGreen;
                    }
                    else
                    {
                        if (infoUI.BeatSaberConnectionLabel.BackColor != Color.DarkRed)
                            infoUI.BeatSaberConnectionLabel.BackColor = Color.DarkRed;
                    }

                    if (OBSWebSocketObjects.OBSRecording)
                    {
                        infoUI.OBSConnectionLabel.Text = "(REC) OBS";

                        if (infoUI.OBSConnectionLabel.BackColor != Color.Orange)
                            infoUI.OBSConnectionLabel.BackColor = Color.Orange;
                    }
                    else
                    {
                        infoUI.OBSConnectionLabel.Text = "OBS";

                        if (infoUI.OBSConnectionLabel.BackColor != Color.DarkGreen)
                            infoUI.OBSConnectionLabel.BackColor = Color.DarkGreen;
                    }

                    if (!Program.beatSaberWebSocket.IsRunning)
                        return;

                    if (HttpStatusObjects.HttpStatusCurrentPerformance is null && DataPullerObjects.DataPullerCurrentBeatmap is null)
                        return;

                    if (DataPullerObjects.DataPullerCurrentBeatmap?.coverImage != null)
                    {
                        if (lastCover != DataPullerObjects.DataPullerCurrentBeatmap?.coverImage?.ToString())
                        {
                            Stopwatch sc = new Stopwatch();
                            sc.Start();

                            LogDebug($"Downloading cover art from '{DataPullerObjects.DataPullerCurrentBeatmap.coverImage.ToString()}'..");

                            lastCover = DataPullerObjects.DataPullerCurrentBeatmap.coverImage.ToString();

                            new HttpClient().GetStreamAsync(DataPullerObjects.DataPullerCurrentBeatmap.coverImage.ToString()).ContinueWith(t =>
                            {
                                coverArt = Bitmap.FromStream(t.Result);
                                LogDebug($"Downloaded cover art from '{DataPullerObjects.DataPullerCurrentBeatmap.coverImage.ToString()}' in {sc.ElapsedMilliseconds}ms");

                                sc.Stop();
                            });
                        }
                    }
                    else
                    {
                        // TODO: BeatSaver Details sometimes dont load which causes the cover fall back to default

                        if (lastCover != "https://readie.global-gaming.co/bsdp-overlay/assets/images/BeatSaberIcon.jpg")
                        {
                            Stopwatch sc = new Stopwatch();
                            sc.Start();

                            LogWarn($"Failed to get cover art from song.");
                            LogDebug($"Downloading default cover art from 'https://readie.global-gaming.co/bsdp-overlay/assets/images/BeatSaberIcon.jpg'..");

                            lastCover = "https://readie.global-gaming.co/bsdp-overlay/assets/images/BeatSaberIcon.jpg";

                            new HttpClient().GetStreamAsync("https://readie.global-gaming.co/bsdp-overlay/assets/images/BeatSaberIcon.jpg").ContinueWith(t =>
                            {
                                coverArt = Bitmap.FromStream(t.Result);

                                LogDebug($"Downloaded default cover art from 'https://readie.global-gaming.co/bsdp-overlay/assets/images/BeatSaberIcon.jpg' in {sc.ElapsedMilliseconds}ms");

                                sc.Stop();
                            });
                        }
                    }

                    switch (Objects.LoadedSettings.Mod)
                    {
                        case "http-status":

                            infoUI.SongNameLabel.Text = $"{HttpStatusObjects.HttpStatusCurrentBeatmap.songName}{(HttpStatusObjects.HttpStatusCurrentBeatmap.songSubName != "" ? $" {HttpStatusObjects.HttpStatusCurrentBeatmap.songName}" : "")}";
                            infoUI.SongAuthorLabel.Text = HttpStatusObjects.HttpStatusCurrentBeatmap.levelAuthorName;

                            break;
                        case "datapuller":

                            if (DataPullerObjects.DataPullerCurrentBeatmap != null)
                            {
                                infoUI.SongNameLabel.Text = $"{DataPullerObjects.DataPullerCurrentBeatmap.SongName}{(DataPullerObjects.DataPullerCurrentBeatmap.SongSubName != "" ? $" {DataPullerObjects.DataPullerCurrentBeatmap.SongSubName}" : "")}";
                                infoUI.SongAuthorLabel.Text = DataPullerObjects.DataPullerCurrentBeatmap.SongAuthor;
                                infoUI.SongAuthorLabel.Location = new Point(infoUI.SongNameLabel.Location.X, infoUI.SongNameLabel.Location.Y + infoUI.SongNameLabel.Height);
                                infoUI.BSRLabel.Text = $"BSR: {DataPullerObjects.DataPullerCurrentBeatmap.BSRKey?.ToString().TrimEnd()}";
                                infoUI.MapperLabel.Text = $"Mapper: {DataPullerObjects.DataPullerCurrentBeatmap.Mapper}";
                            }

                            if (DataPullerObjects.DataPullerCurrentPerformance != null)
                            {
                                infoUI.ScoreLabel.Text = String.Format("{0:n0}", DataPullerObjects.DataPullerCurrentPerformance.Score);
                                infoUI.ComboLabel.Text = $"{DataPullerObjects.DataPullerCurrentPerformance?.Combo}x";
                                infoUI.AccuracyLabel.Text = $"{Math.Round((decimal)DataPullerObjects.DataPullerCurrentPerformance.Accuracy, 2)}%";
                                infoUI.MissesLabel.Text = $"{DataPullerObjects.DataPullerCurrentPerformance.Misses} Misses";

                                infoUI.ProgressLabel.Text = $"{TimeSpan.FromSeconds(DataPullerObjects.DataPullerCurrentPerformance.TimeElapsed).GetShortTimeFormat(Extensions.TimeFormat.MINUTES)}/{TimeSpan.FromSeconds(DataPullerObjects.DataPullerCurrentBeatmap.Length).GetShortTimeFormat(Extensions.TimeFormat.MINUTES)}";

                                if (coverArt != null && infoUI.pictureBox1.Image != coverArt)
                                {
                                    infoUI.pictureBox1.Image = coverArt;
                                }
                            }

                            break;
                        default:
                            return;
                    }
                }
                catch { }
            });

            await Task.Delay(500);

            while (true)
            {
                if (infoUI.InvokeRequired)
                    infoUI.Invoke(UpdateUI);
                else
                    UpdateUI();

                await Task.Delay(250);
            }
        });

        infoUI.ShowDialog();

        if (infoUI.ShowConsoleAgain)
        {
            infoUI.ShowConsoleAgain = false;
            infoUI.ShowConsole.Visible = false;

            Program.ShowWindow(Program.GetConsoleWindow(), 5);
            infoUI.ShowDialog();
        }

        Program.ShowWindow(Program.GetConsoleWindow(), 5);

        if (infoUI.SettingsUpdated)
        {
            LogDebug("Settings updated via UI");
            Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            await Task.Delay(5000);
            Environment.Exit(0);
            return;
        }
        LogDebug("InfoUI closed");
        Environment.Exit(0);
    }
}
