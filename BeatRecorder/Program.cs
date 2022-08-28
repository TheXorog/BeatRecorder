using BeatRecorder.Entities;
using BeatRecorder.Util;
using BeatRecorder.Util.BeatSaber;
using BeatRecorder.Util.OBS;
using BeatRecorder.Util.OpenVR;

namespace BeatRecorder;

public class Program
{
    public static string Version = "2.0.1";

    public bool RunningPrerelease = false;

    internal Config LoadedConfig { get; set; } = null;

    public BaseObsHandler ObsClient { get; set; }

    public BaseBeatSaberHandler BeatSaberClient { get; set; } = null;

    public SteamNotifications steamNotifications { get; set; } = null;

    public UIHandler GUI { get; set; } = null;


    static void Main(string[] args)
    {
        Program program = new();

        program.MainAsync(args).GetAwaiter().GetResult();
    }

    private async Task MainAsync(string[] args)
    {
        Console.ResetColor();

        if (!Directory.Exists("logs"))
            Directory.CreateDirectory("logs");
        _logger = StartLogger($"logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", Xorog.Logger.Enums.LogLevel.INFO, DateTime.UtcNow.AddDays(-3), false);

        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);

            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                _logger.LogError("Only one instance of this application is allowed");
                Environment.Exit(0);
                return;
            }
        });

        if (!File.Exists("Settings.json"))
            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(new Config()));

        try
        {
            LoadedConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Settings.json"));

            if (LoadedConfig.Mod is not "http-status" and not "datapuller" and not "beatsaberplus")
                throw new ArgumentException($"Invalid mod selected: {LoadedConfig.Mod}");

            _logger.ChangeLogLevel(LoadedConfig.ConsoleLogLevel);

            #if DEBUG
            _logger.ChangeLogLevel(LogLevel.TRACE);
            #endif

            if (!string.IsNullOrWhiteSpace(LoadedConfig.OBSPassword))
                _logger.AddBlacklist(LoadedConfig.OBSPassword);

            _logger.AddBlacklist(Environment.UserName);
            _logger.AddBlacklist(Environment.UserDomainName);
            _logger.AddBlacklist(Environment.MachineName);

            _logger.LogInfo("Settings loaded");
            _logger.LogDebug($"{JsonConvert.SerializeObject(LoadedConfig)}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load config", ex);
            await Task.Delay(500);
            Environment.Exit(1);
            throw;
        }

        _logger.LogDebug($"Enviroment Details\n\n" +
                         $"Dotnet Version: {Environment.Version}\n" +
                         $"OS & Version: {Environment.OSVersion}\n\n" +
                         $"OS 64x: {Environment.Is64BitOperatingSystem}\n" +
                         $"Process 64x: {Environment.Is64BitProcess}\n\n" +
                         $"Current Directory: {Environment.CurrentDirectory}\n" +
                         $"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}\n" +
                         $"Commandline: {Environment.CommandLine}\n");

        if (LoadedConfig.DisplayUI)
            GUI = UIHandler.Initialize(this);

        _ = Task.Run(() =>
        {
            if (LoadedConfig.DisplaySteamNotifications)
                steamNotifications = SteamNotifications.Initialize();
        });

        _ = Task.Run(async () =>
        {
            try
            {
                var github = new GitHubClient(new ProductHeaderValue("BeatRecorderUpdateCheck"));
                IReadOnlyList<Release> releases = await github.Repository.Release.GetAll("TheXorog", "BeatRecorder");

                #if DEBUG
                Version += "-dev";
                #endif

                RunningPrerelease = (Version.ToLower().Contains("dev") || Version.ToLower().Contains("rc") || Version.ToLower().Contains("beta") || Version.ToLower().Contains("alpha"));

                if (RunningPrerelease)
                    _logger.LogWarn("You're running a pre-release version of BeatRecorder. If you find any bugs, please report them at https://github.com/TheXorog/BeatRecorder");

                Release repo = null;

                foreach (var rel in releases)
                {
                    if (rel.Prerelease && RunningPrerelease)
                    {
                        repo = rel;
                        break;
                    }

                    if (!rel.Prerelease)
                    {
                        repo = rel;
                        break;
                    }
                }

                if (repo is null)
                    throw new Exception("Failed to get latest version.");

                _logger.LogInfo($"Current latest release is \"{repo.TagName}\". You're currently running: \"{Version}\"");

                if (repo.TagName != Version && !Version.Contains("dev"))
                {
                    _logger.LogFatal($"You're running an outdated version of BeatRecorder, please update at https://github.com/TheXorog/BeatRecorder/releases/latest." +
                            $"\n\nWhat changed in the new version:\n\n" +
                            $"{repo.Body}\n");

                    _ = Task.Run(() => { GUI.ShowNotification($"You're running an outdated version of BeatRecorder.\nVersion {repo.TagName} is available.\n\n{repo.Body}", "New version available", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning); });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to get latest version", ex);
            }
        });

        _ = Task.Run(async () =>
        {
            HttpClient httpClient = new();
            httpClient.Timeout = TimeSpan.FromSeconds(3);

            async Task<bool> UseModernSocket()
            {
                try
                {
                    _logger.LogDebug($"Checking if obs-websocket v5 is available at {this.LoadedConfig.OBSUrl}:{this.LoadedConfig.OBSPortModern}..");
                    var response = await httpClient.GetAsync($"http://{this.LoadedConfig.OBSUrl}:{this.LoadedConfig.OBSPortModern}");
                    _logger.LogDebug($"obs-websocket v5 is available");
                    return true;
                }
                catch (Exception)
                {
                    _logger.LogWarn($"obs-websocket is not available, attempting fall back to legacy..");

                    try
                    {
                        var response = await httpClient.GetAsync($"http://{this.LoadedConfig.OBSUrl}:{this.LoadedConfig.OBSPortLegacy}");
                        _logger.LogWarn($"obs-websocket v4 is available. While still supported, you should update to obs websocket v5 here: https://github.com/obsproject/obs-websocket/releases");
                        return false;
                    }
                    catch (Exception)
                    {
                        _logger.LogWarn($"obs-websocket v4 is not available, re-checking..");
                        return await UseModernSocket();
                    }
                }
            }

            if (await UseModernSocket())
                this.ObsClient = ObsHandler.Initialize(this);
            else
                this.ObsClient = LegacyObsHandler.Initialize(this);
        });

        _ = Task.Run(async () =>
        {
            while (ObsClient is null)
                await Task.Delay(100);

            switch (LoadedConfig.Mod)
            {
                case "http-status":
                {
                    BeatSaberClient = new HttpStatusHandler().Initialize(this); // 6557
                    break;
                }
                case "datapuller":
                {
                    BeatSaberClient = new DataPullerHandler().Initialize(this); // 2946 
                    break;
                }
                case "beatsaberplus":
                {
                    _ = Task.Run(() => { GUI.ShowNotification("BeatSaberPlus Integration is currently incomplete. Filenames will always appear as if you finished the song.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning); });
                    _logger.LogFatal("BeatSaberPlus Integration is currently incomplete. BSP does not provide a way of knowing if a song was failed or finished, making filenames always seem like the song was finished.");
                    _logger.LogFatal("To continue anyways, wait 10 seconds.");
                    await Task.Delay(10000);
                    BeatSaberClient = new BeatSaberPlusHandler().Initialize(this); // 2947
                    break;
                }
            }
        });

        await Task.Delay(-1);
    }
}
