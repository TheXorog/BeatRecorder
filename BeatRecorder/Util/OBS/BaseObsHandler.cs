namespace BeatRecorder.Util.OBS;

internal abstract class BaseObsHandler
{
    internal abstract Task StartRecording();
    internal abstract Task StopRecording(bool ForceStop = false);
    internal abstract void PauseRecording();
    internal abstract void ResumeRecording();
    internal abstract void SetCurrentScene(string scene);
}
