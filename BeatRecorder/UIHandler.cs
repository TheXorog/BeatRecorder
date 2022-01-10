namespace BeatRecorder;

internal class UIHandler
{
    public static bool OBSPasswordRequired = false;

    internal async Task HandleUI()
    {
        await Task.Delay(500);

        if (!Objects.LoadedSettings.HideConsole)
            Program.ShowWindow(Program.GetConsoleWindow(), 2);
        else
            Program.ShowWindow(Program.GetConsoleWindow(), 0);

        LogDebug($"Displaying InfoUI");

        var infoUI = new InfoUI(Objects.LoadedSettings.DisplayUITopmost, Objects.SettingsRequired);

        _ = Task.Run(async () =>
        {
            string lastCover = "";
            Image coverArt = null;

            Action UpdateUI = new Action(() =>
            {
                try
                {
                    if (OBSPasswordRequired)
                    {
                        Thread.Sleep(3000);
                        LogDebug($"Trying to display password prompt");
                        infoUI.Hide();

                        MessageBox.Show($"A password is required to log into the obs websocket.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        var infoUI2 = new InfoUI(Objects.LoadedSettings.DisplayUITopmost, true);
                        infoUI2.ShowDialog();
                        Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                        Thread.Sleep(2000);
                        Environment.Exit(0);
                        return;
                    }

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

                    switch (Objects.LoadedSettings.Mod)
                    {
                        case "http-status":

                        if (HttpStatusObjects.HttpStatusCurrentBeatmap != null)
                        {
                            infoUI.SongNameLabel.Text = $"{HttpStatusObjects.HttpStatusCurrentBeatmap.songName}{(HttpStatusObjects.HttpStatusCurrentBeatmap.songSubName != "" ? $" {HttpStatusObjects.HttpStatusCurrentBeatmap.songSubName}" : "")}";
                            infoUI.SongAuthorLabel.Text = HttpStatusObjects.HttpStatusCurrentBeatmap.songAuthorName;
                            infoUI.SongAuthorLabel.Location = new Point(infoUI.SongNameLabel.Location.X, infoUI.SongNameLabel.Location.Y + infoUI.SongNameLabel.Height);
                            infoUI.BSRLabel.Text = $"";
                            infoUI.MapperLabel.Text = $"";

                            infoUI.ProgressLabel.Text = $"{TimeSpan.FromSeconds(OBSWebSocketObjects.RecordingSeconds).GetShortTimeFormat(Extensions.TimeFormat.MINUTES)}/{TimeSpan.FromSeconds(HttpStatusObjects.HttpStatusCurrentBeatmap.length / 1000).GetShortTimeFormat(Extensions.TimeFormat.MINUTES)}";

                            if (HttpStatusObjects.HttpStatusCurrentBeatmap?.songCover != null)
                            {
                                if (lastCover != HttpStatusObjects.HttpStatusCurrentBeatmap?.songCover)
                                {
                                    LogDebug($"Generating cover art from base64 string..");

                                    lastCover = HttpStatusObjects.HttpStatusCurrentBeatmap?.songCover;

                                    byte[] byteBuffer = Convert.FromBase64String(HttpStatusObjects.HttpStatusCurrentBeatmap?.songCover);
                                    MemoryStream memoryStream = new MemoryStream(byteBuffer);
                                    memoryStream.Position = 0;

                                    Bitmap bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream);
                                    coverArt = bmpReturn;
                                }
                            }
                            else
                            {
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
                        }

                        if (HttpStatusObjects.HttpStatusCurrentPerformance != null)
                        {
                            infoUI.ScoreLabel.Text = String.Format("{0:n0}", HttpStatusObjects.HttpStatusCurrentPerformance.score);
                            infoUI.ComboLabel.Text = $"{HttpStatusObjects.HttpStatusCurrentPerformance?.combo}x";
                            infoUI.AccuracyLabel.Text = $"{Math.Round((decimal)((decimal)((decimal)HttpStatusObjects.HttpStatusCurrentPerformance.score / (decimal)HttpStatusObjects.HttpStatusCurrentPerformance.currentMaxScore) * 100), 2)}%";
                            infoUI.MissesLabel.Text = $"{HttpStatusObjects.HttpStatusCurrentPerformance.missedNotes} Misses";

                            if (coverArt != null && infoUI.pictureBox1.Image != coverArt)
                            {
                                infoUI.pictureBox1.Image = coverArt;
                            }
                        }

                        break;

                        case "datapuller":

                        if (DataPullerObjects.DataPullerCurrentBeatmap != null)
                        {
                            infoUI.SongNameLabel.Text = $"{DataPullerObjects.DataPullerCurrentBeatmap.SongName}{(DataPullerObjects.DataPullerCurrentBeatmap.SongSubName != "" ? $" {DataPullerObjects.DataPullerCurrentBeatmap.SongSubName}" : "")}";
                            infoUI.SongAuthorLabel.Text = DataPullerObjects.DataPullerCurrentBeatmap.SongAuthor;
                            infoUI.SongAuthorLabel.Location = new Point(infoUI.SongNameLabel.Location.X, infoUI.SongNameLabel.Location.Y + infoUI.SongNameLabel.Height);
                            infoUI.BSRLabel.Text = $"BSR: {DataPullerObjects.DataPullerCurrentBeatmap.BSRKey?.ToString().TrimEnd()}";
                            infoUI.MapperLabel.Text = $"Mapper: {DataPullerObjects.DataPullerCurrentBeatmap.Mapper}";

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

                while (OBSPasswordRequired)
                    await Task.Delay(1000);
            }
        });

        infoUI.ShowDialog();
        Program.ShowWindow(Program.GetConsoleWindow(), 5);

        while (OBSPasswordRequired)
            await Task.Delay(1000);

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
