using BeatRecorder.Entities;
using BeatRecorder.Util;
using BeatRecorder.Util.BeatSaber;
using BeatRecorder.Util.OBS;

namespace BeatRecorder;

class Program
{
    public static readonly string Version = "2.0-alpha_dev";

    public Status status { get; set; } = new();

    public BaseObsHandler ObsClient { get; set; }

    public BaseBeatSaberHandler BeatSaberClient { get; set; } = null;


    static void Main(string[] args)
    {
        Program program = new();

        program.MainAsync(args).GetAwaiter().GetResult();
    }

    private async Task MainAsync(string[] args)
    {
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
            status.LoadedConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Settings.json"));

            if (status.LoadedConfig.Mod is not "http-status" and not "datapuller" and not "beatsaberplus")
                throw new ArgumentException($"Invalid mod selected: {status.LoadedConfig.Mod}");

            _logger.ChangeLogLevel(status.LoadedConfig.ConsoleLogLevel);

            #if DEBUG
            _logger.ChangeLogLevel(LogLevel.TRACE);
            #endif

            if (!string.IsNullOrWhiteSpace(status.LoadedConfig.OBSPassword))
                _logger.AddBlacklist(status.LoadedConfig.OBSPassword);

            _logger.AddBlacklist(Environment.UserName);
            _logger.AddBlacklist(Environment.UserDomainName);
            _logger.AddBlacklist(Environment.MachineName);

            _logger.LogInfo("Settings loaded");
            _logger.LogDebug($"{JsonConvert.SerializeObject(status.LoadedConfig)}");
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

        _ = Task.Run(async () =>
        {
            HttpClient httpClient = new();
            httpClient.Timeout = TimeSpan.FromSeconds(3);

            async Task<bool> UseModernSocket()
            {
                try
                {
                    _logger.LogDebug($"Checking if obs-websocket v5 is available at {this.status.LoadedConfig.OBSUrl}:{this.status.LoadedConfig.OBSPortModern}..");
                    var response = await httpClient.GetAsync($"http://{this.status.LoadedConfig.OBSUrl}:{this.status.LoadedConfig.OBSPortModern}");
                    _logger.LogDebug($"obs-websocket v5 is available");
                    return true;
                }
                catch (Exception)
                {
                    _logger.LogWarn($"obs-websocket is not available, attempting fall back to legacy..");

                    try
                    {
                        var response = await httpClient.GetAsync($"http://{this.status.LoadedConfig.OBSUrl}:{this.status.LoadedConfig.OBSPortLegacy}");
                        _logger.LogDebug($"obs-websocket v4 is available");
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
            switch (status.LoadedConfig.Mod)
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
