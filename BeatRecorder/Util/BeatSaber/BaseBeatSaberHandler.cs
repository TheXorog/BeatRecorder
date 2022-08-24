namespace BeatRecorder.Util.BeatSaber;

internal abstract class BaseBeatSaberHandler
{
    public abstract void HandleFile(string fileName, SharedStatus sharedStatus, Config loadedConfig);
}
