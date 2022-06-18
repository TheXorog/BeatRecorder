namespace BeatRecorder.Util;

internal class Util
{
    public static void CheckForMod(DisconnectionInfo msg, string modName, string humanReadableName)
    {
        try
        {
            modName = modName.ToLower();

            Process[] processCollection = Process.GetProcesses();

            if (!processCollection.Any(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")))
            {
                if (Objects.LastHttpStatusWarning != ConnectionType.NoProcess)
                {
                    _logger.LogWarn($"Couldn't find a BeatSaber process, is BeatSaber started? ({msg.Type})");
                    Program.SendNotification("Couldn't connect to BeatSaber, is it even running?", 5000, MessageType.ERROR);
                }
                Objects.LastHttpStatusWarning = ConnectionType.NoProcess;
                return;
            }

            string InstallationDirectory = processCollection.First(x => x.ProcessName.ToLower().Replace(" ", "").StartsWith("beatsaber")).MainModule.FileName;
            InstallationDirectory = InstallationDirectory[..InstallationDirectory.LastIndexOf("\\")];

            if (!Directory.GetDirectories(InstallationDirectory).Any(x => x.ToLower() == "plugins"))
            {
                if (Objects.LastHttpStatusWarning != ConnectionType.NotModded)
                {
                    _logger.LogFatal($"Beat Saber seems to be running but the {humanReadableName} modifaction doesn't seem to be installed. Is your game even modded? (If haven't modded it, please do it: https://bit.ly/2TAvenk. If already modded, install {humanReadableName}: https://bit.ly/3HdIvqg) ({msg.Type})");
                    Program.SendNotification("Couldn't connect to Beat Saber. Have you modded your game?", 10000, MessageType.ERROR);
                }
                Objects.LastHttpStatusWarning = ConnectionType.NotModded;
                return;
            }

            if (Directory.GetFiles($"{InstallationDirectory}\\Plugins").Any(x => x.ToLower().Contains(modName) && x.ToLower().EndsWith(".dll")))
            {
                if (Objects.LastHttpStatusWarning != ConnectionType.ModInstalled)
                {
                    _logger.LogFatal($"Beat Saber seems to be running and the {humanReadableName} modifaction seems to be installed. Please make sure you put in the right port and you installed all of {humanReadableName}' dependiencies! (If not installed, please install it: https://bit.ly/3HdIvqg) ({msg.Type})");
                    Program.SendNotification("Couldn't connect to Beat Saber. Please make sure you selected the right port.", 10000, MessageType.ERROR);
                }
                Objects.LastHttpStatusWarning = ConnectionType.ModInstalled;
                return;
            }

            if (Objects.LastHttpStatusWarning != ConnectionType.ModNotInstalled)
            {
                _logger.LogFatal($"Beat Saber seems to be running but the {humanReadableName} modifaction doesn't seem to be installed. Please make sure to install {humanReadableName}! (If not installed, please install it: https://bit.ly/3HdIvqg) ({msg.Type})");
                Program.SendNotification("Couldn't connect to Beat Saber. Please make sure DataPuller is installed.", 10000, MessageType.ERROR);
            }
            Objects.LastHttpStatusWarning = ConnectionType.ModNotInstalled;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check if {humanReadableName} is installed. (Disconnect Reason: {msg.Type})", ex);
        }
    }
}
