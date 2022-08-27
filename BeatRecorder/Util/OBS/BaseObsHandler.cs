namespace BeatRecorder.Util.OBS;

public abstract class BaseObsHandler
{
    internal abstract Task StartRecording();
    internal abstract Task StopRecording(bool ForceStop = false);
    internal abstract void PauseRecording();
    internal abstract void ResumeRecording();
    internal abstract void SetCurrentScene(string scene);

    internal abstract bool GetIsRunning();
    internal abstract bool GetIsRecording();
    internal abstract bool GetIsPaused();
    internal abstract int GetRecordingSeconds();
}
