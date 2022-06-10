namespace BeatRecorder;

class OBSWebSocket
{
    internal static async Task StartRecording()
    {
        if (!Program.LoadedSettings.AutomaticRecording)
            return;

        if (Program.obsWebSocket.IsStarted)
        {
            if (OBSWebSocketStatus.OBSRecording)
            {
                OBSWebSocketStatus.CancelStopRecordingDelay.Cancel();
                await StopRecording(OBSWebSocketStatus.CancelStopRecordingDelay.Token, true);
            }

            OBSWebSocketStatus.CancelStopRecordingDelay = new CancellationTokenSource();

            while (OBSWebSocketStatus.OBSRecording)
            {
                Thread.Sleep(20);
            }

            if (Program.LoadedSettings.MininumWaitUntilRecordingCanStart > 199 || Program.LoadedSettings.MininumWaitUntilRecordingCanStart < 2001)
                Thread.Sleep(Program.LoadedSettings.MininumWaitUntilRecordingCanStart);
            else
            {
                LogError("The MininumWaitUntilRecordingCanStart has to be between 200ms and 2000ms. Defaulting to a wait time of 800ms.");
                Thread.Sleep(800);
            }

            Program.obsWebSocket.Send($"{{\"request-type\":\"StartRecording\", \"message-id\":\"StartRecording\"}}");
        }
        else
        {
            LogError("[OBS] The WebSocket isn't connected, no recording can be started.");
        }
    }

    internal static async Task StopRecording(CancellationToken CancelToken, bool ForceStop = false)
    {
        if (!Program.LoadedSettings.AutomaticRecording)
            return;

        if (Program.obsWebSocket.IsStarted)
        {
            if (OBSWebSocketStatus.OBSRecording)
            {
                if (!ForceStop)
                {
                    if (Program.LoadedSettings.StopRecordingDelay > 0 && Program.LoadedSettings.StopRecordingDelay < 21)
                    {
                        try
                        {
                            await Task.Delay(Program.LoadedSettings.StopRecordingDelay * 1000, CancelToken);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                    }
                    else
                        LogError("[OBS] The specified delay is not in between 1 and 20 seconds. The delay will be skipped.");
                }

                Program.obsWebSocket.Send($"{{\"request-type\":\"StopRecording\", \"message-id\":\"StopRecording\"}}");
                return;
            }
        }
        else
        {
            LogError("[OBS] The WebSocket isn't connected, no recording can be stopped.");
        }
    }
}
