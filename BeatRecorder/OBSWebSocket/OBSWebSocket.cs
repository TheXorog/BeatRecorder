using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xorog.Logger;
using static Xorog.Logger.Logger;
using static Xorog.Logger.LoggerObjects;

namespace BeatRecorder
{
    class OBSWebSocket
    {
        internal static async Task StartRecording()
        {
            if (Program.obsWebSocket.IsStarted)
            {
                if (OBSWebSocketObjects.OBSRecording)
                {
                    OBSWebSocketObjects.CancelStopRecordingDelay.Cancel();
                    await StopRecording(OBSWebSocketObjects.CancelStopRecordingDelay.Token, true);
                }

                OBSWebSocketObjects.CancelStopRecordingDelay = new CancellationTokenSource();

                while (OBSWebSocketObjects.OBSRecording)
                {
                    Thread.Sleep(20);
                }

                if (Objects.LoadedSettings.MininumWaitUntilRecordingCanStart > 199 || Objects.LoadedSettings.MininumWaitUntilRecordingCanStart < 2001)
                    Thread.Sleep(Objects.LoadedSettings.MininumWaitUntilRecordingCanStart);
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
            if (Program.obsWebSocket.IsStarted)
            {
                if (OBSWebSocketObjects.OBSRecording)
                {
                    if (!ForceStop)
                    {
                        if (Objects.LoadedSettings.StopRecordingDelay > 0 && Objects.LoadedSettings.StopRecordingDelay < 21)
                        {
                            try
                            {
                                await Task.Delay(Objects.LoadedSettings.StopRecordingDelay * 1000, CancelToken);
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
}
