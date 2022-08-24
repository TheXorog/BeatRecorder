namespace BeatRecorder;

internal class UIHandler
{
    public static bool OBSPasswordRequired = false;

    internal async Task HandleUI()
    {
        await Task.Delay(500);

        bool DisplayedUpdateNotice = false;

        if (!Program.LoadedSettings.HideConsole)
            ConsoleHelper.ShowWindow(ConsoleHelper.GetConsoleWindow(), 2);
        else
            ConsoleHelper.ShowWindow(ConsoleHelper.GetConsoleWindow(), 0);

        _logger.LogDebug($"Displaying InfoUI");

        var infoUI = new InfoUI(Program.CurrentVersion, Program.LoadedSettings.DisplayUITopmost, Objects.SettingsRequired);

        _ = Task.Run(async () =>
        {
            string lastCover = "";
            Image coverArt = null;

            Action UpdateUI = new(() =>
            {
                try
                {
                    if (OBSPasswordRequired)
                    {
                        Thread.Sleep(3000);
                        _logger.LogDebug($"Trying to display password prompt");
                        infoUI.Hide();

                        MessageBox.Show($"A password is required to log into the obs websocket.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        var infoUI2 = new InfoUI(Program.CurrentVersion, Program.LoadedSettings.DisplayUITopmost, true);
                        infoUI2.ShowDialog();
                        Process.Start(Environment.ProcessPath);
                        Thread.Sleep(2000);
                        Environment.Exit(0);
                        return;
                    }

                    if (Objects.UpdateAvailable && !DisplayedUpdateNotice)
                    {
                        DisplayedUpdateNotice = true;
                        MessageBox.Show($"There's a new version available.\n\n{Objects.UpdateText}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                    if (OBSWebSocketStatus.OBSRecording)
                    {
                        infoUI.OBSConnectionLabel.Text = "(REC) OBS";

                        if (infoUI.OBSConnectionLabel.BackColor != Color.Orange)
                            infoUI.OBSConnectionLabel.BackColor = Color.Orange;
                    }
                    else
                    {
                        infoUI.OBSConnectionLabel.Text = "OBS";

                        if (!Program.obsWebSocket.IsRunning)
                        {
                            if (infoUI.OBSConnectionLabel.BackColor != Color.DarkRed)
                                infoUI.OBSConnectionLabel.BackColor = Color.DarkRed;
                        }
                        else
                        {
                            if (infoUI.OBSConnectionLabel.BackColor != Color.DarkGreen)
                                infoUI.OBSConnectionLabel.BackColor = Color.DarkGreen;
                        }
                    }

                    if (!Program.beatSaberWebSocket.IsRunning)
                        return;

                    if (HttpStatusStatus.HttpStatusCurrentPerformance is null && DataPullerStatus.DataPullerCurrentBeatmap is null)
                        return;

                    switch (Program.LoadedSettings.Mod)
                    {
                        case "http-status":
                        {
                            if (HttpStatusStatus.HttpStatusCurrentBeatmap != null)
                            {
                                infoUI.SongNameLabel.Text = $"{HttpStatusStatus.HttpStatusCurrentBeatmap.songName}{(HttpStatusStatus.HttpStatusCurrentBeatmap.songSubName != "" ? $" {HttpStatusStatus.HttpStatusCurrentBeatmap.songSubName}" : "")}";
                                infoUI.SongAuthorLabel.Text = HttpStatusStatus.HttpStatusCurrentBeatmap.songAuthorName;
                                infoUI.SongAuthorLabel.Location = new Point(infoUI.SongNameLabel.Location.X, infoUI.SongNameLabel.Location.Y + infoUI.SongNameLabel.Height);
                                infoUI.BSRLabel.Text = $"";
                                infoUI.MapperLabel.Text = $"";

                                infoUI.ProgressLabel.Text = $"{TimeSpan.FromSeconds(OBSWebSocketStatus.RecordingSeconds).GetShortTimeFormat(Extensions.TimeFormat.MINUTES)}/{TimeSpan.FromSeconds(HttpStatusStatus.HttpStatusCurrentBeatmap.length / 1000).GetShortTimeFormat(Extensions.TimeFormat.MINUTES)}";

                                if (HttpStatusStatus.HttpStatusCurrentBeatmap?.songCover != null)
                                {
                                    if (lastCover != HttpStatusStatus.HttpStatusCurrentBeatmap?.songCover)
                                    {
                                        _logger.LogDebug($"Generating cover art from base64 string..");

                                        lastCover = HttpStatusStatus.HttpStatusCurrentBeatmap?.songCover;

                                        byte[] byteBuffer = Convert.FromBase64String(HttpStatusStatus.HttpStatusCurrentBeatmap?.songCover);
                                        MemoryStream memoryStream = new(byteBuffer)
                                        {
                                            Position = 0
                                        };

                                        Bitmap bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream);
                                        coverArt = bmpReturn;
                                    }
                                }
                                else
                                {
                                    if (lastCover != "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg")
                                    {
                                        Stopwatch sc = new();
                                        sc.Start();

                                        _logger.LogWarn($"Failed to get cover art from song.");
                                        _logger.LogDebug($"Downloading default cover art from 'https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg'..");

                                        lastCover = "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg";

                                        new HttpClient().GetStreamAsync("https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg").ContinueWith(t =>
                                        {
                                            sc.Stop();

                                            if (!t.IsCompletedSuccessfully)
                                            {
                                                _logger.LogError($"Failed to download default cover art", t.Exception);
                                                return;
                                            }

                                            coverArt = Bitmap.FromStream(t.Result);

                                            _logger.LogDebug($"Downloaded default cover art from 'https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg' in {sc.ElapsedMilliseconds}ms");
                                        });
                                    }
                                }
                            }

                            if (HttpStatusStatus.HttpStatusCurrentPerformance != null)
                            {
                                infoUI.ScoreLabel.Text = String.Format("{0:n0}", HttpStatusStatus.HttpStatusCurrentPerformance.score);
                                infoUI.ComboLabel.Text = $"{HttpStatusStatus.HttpStatusCurrentPerformance?.combo}x";
                                infoUI.AccuracyLabel.Text = $"{Math.Round((decimal)((decimal)((decimal)HttpStatusStatus.HttpStatusCurrentPerformance.score / (decimal)HttpStatusStatus.HttpStatusCurrentPerformance.currentMaxScore) * 100), 2)}%";
                                infoUI.MissesLabel.Text = $"{HttpStatusStatus.HttpStatusCurrentPerformance.missedNotes} Misses";

                                if (coverArt != null && infoUI.pictureBox1.Image != coverArt)
                                {
                                    infoUI.pictureBox1.Image = coverArt;
                                }
                            }

                            break;
                        }
                        case "datapuller":
                        {
                            if (DataPullerStatus.DataPullerCurrentBeatmap != null)
                            {
                                infoUI.SongNameLabel.Text = $"{DataPullerStatus.DataPullerCurrentBeatmap.SongName}{(DataPullerStatus.DataPullerCurrentBeatmap.SongSubName != "" ? $" {DataPullerStatus.DataPullerCurrentBeatmap.SongSubName}" : "")}";
                                infoUI.SongAuthorLabel.Text = DataPullerStatus.DataPullerCurrentBeatmap.SongAuthor;
                                infoUI.SongAuthorLabel.Location = new Point(infoUI.SongNameLabel.Location.X, infoUI.SongNameLabel.Location.Y + infoUI.SongNameLabel.Height);
                                infoUI.BSRLabel.Text = $"BSR: {DataPullerStatus.DataPullerCurrentBeatmap.BSRKey?.ToString().TrimEnd()}";
                                infoUI.MapperLabel.Text = $"Mapper: {DataPullerStatus.DataPullerCurrentBeatmap.Mapper}";

                                if (DataPullerStatus.DataPullerCurrentBeatmap?.coverImage != null)
                                {
                                    if (lastCover != DataPullerStatus.DataPullerCurrentBeatmap?.coverImage?.ToString())
                                    {
                                        Stopwatch sc = new();
                                        sc.Start();

                                        _logger.LogDebug($"Downloading cover art from '{DataPullerStatus.DataPullerCurrentBeatmap.coverImage}'..");

                                        lastCover = DataPullerStatus.DataPullerCurrentBeatmap.coverImage.ToString();

                                        new HttpClient().GetStreamAsync(DataPullerStatus.DataPullerCurrentBeatmap.coverImage.ToString()).ContinueWith(t =>
                                        {
                                            coverArt = Bitmap.FromStream(t.Result);
                                            _logger.LogDebug($"Downloaded cover art from '{DataPullerStatus.DataPullerCurrentBeatmap.coverImage}' in {sc.ElapsedMilliseconds}ms");

                                            sc.Stop();
                                        });
                                    }
                                }
                                else
                                {
                                    // TODO: BeatSaver Details sometimes dont load which causes the cover fall back to default

                                    if (lastCover != "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg")
                                    {
                                        Stopwatch sc = new();
                                        sc.Start();

                                        _logger.LogWarn($"Failed to get cover art from song.");
                                        _logger.LogDebug($"Downloading default cover art from 'https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg'..");

                                        lastCover = "https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg";

                                        new HttpClient().GetStreamAsync("https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg").ContinueWith(t =>
                                        {
                                            sc.Stop();

                                            if (!t.IsCompletedSuccessfully)
                                            {
                                                _logger.LogError($"Failed to download default cover art", t.Exception);
                                                return;
                                            }

                                            coverArt = Bitmap.FromStream(t.Result);

                                            _logger.LogDebug($"Downloaded default cover art from 'https://raw.githubusercontent.com/TheXorog/BeatRecorder/main/BeatRecorder/Assets/BeatSaberIcon.jpg' in {sc.ElapsedMilliseconds}ms");
                                        });
                                    }
                                }
                            }

                            if (DataPullerStatus.DataPullerCurrentPerformance != null)
                            {
                                infoUI.ScoreLabel.Text = String.Format("{0:n0}", DataPullerStatus.DataPullerCurrentPerformance.Score);
                                infoUI.ComboLabel.Text = $"{DataPullerStatus.DataPullerCurrentPerformance?.Combo}x";
                                infoUI.AccuracyLabel.Text = $"{Math.Round((decimal)DataPullerStatus.DataPullerCurrentPerformance.Accuracy, 2)}%";
                                infoUI.MissesLabel.Text = $"{DataPullerStatus.DataPullerCurrentPerformance.Misses} Misses";

                                infoUI.ProgressLabel.Text = $"{TimeSpan.FromSeconds(DataPullerStatus.DataPullerCurrentPerformance.TimeElapsed).GetShortTimeFormat(Extensions.TimeFormat.MINUTES)}/{TimeSpan.FromSeconds(DataPullerStatus.DataPullerCurrentBeatmap.Length).GetShortTimeFormat(Extensions.TimeFormat.MINUTES)}";

                                if (coverArt != null && infoUI.pictureBox1.Image != coverArt)
                                {
                                    infoUI.pictureBox1.Image = coverArt;
                                }
                            }

                            break;
                        }
                        default:
                            throw new Exception("Invalid Mod");
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
        ConsoleHelper.ShowWindow(ConsoleHelper.GetConsoleWindow(), 5);

        while (OBSPasswordRequired)
            await Task.Delay(1000);

        if (infoUI.ShowConsoleAgain)
        {
            infoUI.ShowConsoleAgain = false;
            infoUI.ShowConsole.Visible = false;

            ConsoleHelper.ShowWindow(ConsoleHelper.GetConsoleWindow(), 5);
            infoUI.ShowDialog();
        }

        ConsoleHelper.ShowWindow(ConsoleHelper.GetConsoleWindow(), 5);

        if (infoUI.SettingsUpdated)
        {
            _logger.LogDebug("Settings updated via UI");
            Process.Start(Environment.ProcessPath);
            await Task.Delay(5000);
            Environment.Exit(0);
            return;
        }
        _logger.LogDebug("InfoUI closed");
        Environment.Exit(0);
    }
}
