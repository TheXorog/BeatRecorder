﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;
using Websocket.Client.Models;

namespace OBSControl
{
    class OBSWebSocketEvents
    {
        internal static string RequiredAuthenticationGuid = Guid.NewGuid().ToString();
        internal static string AuthenticationGuid = Guid.NewGuid().ToString();
        internal static string CheckIfRecording = Guid.NewGuid().ToString();

        internal static async Task MessageReceived(ResponseMessage msg)
        {
            if (msg.Text.Contains($"\"message-id\":\"{RequiredAuthenticationGuid}\""))
            {
                OBSWebSocketObjects.AuthenticationRequired required = JsonConvert.DeserializeObject<OBSWebSocketObjects.AuthenticationRequired>(msg.Text);

                if (required.authRequired)
                {
                    _logger.LogInfo("[OBS] Authenticating..");

                    if (Objects.LoadedSettings.OBSPassword == "")
                    {
                        await Task.Delay(1000);
                        _logger.LogInfo("[OBS] A password is required to log into your obs websocket.");
                        await Task.Delay(1000);
                        Console.Write("> ");

                        // I was to lazy to write my own.. https://stackoverflow.com/questions/3404421/password-masking-console-application

                        string Password = "";

                        ConsoleKey key = ConsoleKey.A;

                        while (key != ConsoleKey.Enter || key != ConsoleKey.Escape)
                        {
                            var keyInfo = Console.ReadKey(intercept: true);
                            key = keyInfo.Key;

                            if (key == ConsoleKey.Backspace && Password.Length > 0)
                            {
                                Console.Write("\b \b");
                                Password = Password[0..^1];
                            }
                            else if (!char.IsControl(keyInfo.KeyChar))
                            {
                                Console.Write("*");
                                Password += keyInfo.KeyChar;
                            }
                            else if (key == ConsoleKey.Escape)
                            {
                                _logger.LogInfo("[OBS] Cancelled. Press any key to exit.");
                                Console.ReadKey();
                                Environment.Exit(0);
                                return;
                            }
                            else if (key == ConsoleKey.Enter)
                            {
                                Console.Write("\r                                              \r");
                                break;
                            }
                        }

                        if (key == ConsoleKey.Enter)
                        {
                            if (Objects.LoadedSettings.AskToSaveOBSPassword)
                            {
                                key = ConsoleKey.A;

                                _logger.LogWarn("[OBS] Do you want to save this password in the config? (THIS WILL STORE THE PASSWORD IN PLAIN-TEXT, THIS CAN BE ACCESSED BY ANYONE WITH ACCESS TO YOUR FILES. THIS IS NOT RECOMMENDED!)");
                                while (key != ConsoleKey.Enter || key != ConsoleKey.Escape || key != ConsoleKey.Y || key != ConsoleKey.N)
                                {
                                    await Task.Delay(1000);
                                    Console.Write("[OBS] y/N > ");

                                    var keyInfo = Console.ReadKey(intercept: true);
                                    Console.Write("\r                                              \r");
                                    key = keyInfo.Key;

                                    if (key == ConsoleKey.Escape)
                                    {
                                        _logger.LogWarn("[OBS] Cancelled. Press any key to exit.");
                                        Console.ReadKey();
                                        Environment.Exit(0);
                                        return;
                                    }
                                    else if (key == ConsoleKey.Y)
                                    {
                                        _logger.LogInfo("[OBS] Your password is now saved in the Settings.json.");
                                        Objects.LoadedSettings.OBSPassword = Password;
                                        Objects.LoadedSettings.AskToSaveOBSPassword = true;

                                        File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Objects.LoadedSettings, Formatting.Indented));
                                        break;
                                    }
                                    else if (key == ConsoleKey.N || key == ConsoleKey.Enter)
                                    {
                                        _logger.LogInfo("[OBS] Your password will not be saved. This wont be asked in the feature.");
                                        _logger.LogInfo("[OBS] To re-enable this prompt, set AskToSaveOBSPassword to true in the Settings.json.");
                                        Objects.LoadedSettings.OBSPassword = "";
                                        Objects.LoadedSettings.AskToSaveOBSPassword = false;

                                        File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Objects.LoadedSettings, Formatting.Indented));
                                        break;
                                    }
                                }
                            }

                            Objects.LoadedSettings.OBSPassword = Password;
                        }
                    }

                    string secret = Extensions.HashEncode(Objects.LoadedSettings.OBSPassword + required.salt);
                    string authResponse = Extensions.HashEncode(secret + required.challenge);

                    Program.obsWebSocket.Send($"{{\"request-type\":\"Authenticate\", \"message-id\":\"{AuthenticationGuid}\", \"auth\":\"{authResponse}\"}}");
                }
                else
                {
                    Program.obsWebSocket.Send($"{{\"request-type\":\"GetRecordingStatus\", \"message-id\":\"{CheckIfRecording}\"}}");
                }
            }
            else if (msg.Text.Contains($"\"message-id\":\"{AuthenticationGuid}\""))
            {
                OBSWebSocketObjects.AuthenticationRequired required = JsonConvert.DeserializeObject<OBSWebSocketObjects.AuthenticationRequired>(msg.Text);

                if (required.status == "ok")
                {
                    _logger.LogInfo("[OBS] Authenticated.");

                    Program.obsWebSocket.Send($"{{\"request-type\":\"GetRecordingStatus\", \"message-id\":\"{CheckIfRecording}\"}}");
                }
                else
                {
                    _logger.LogError("[OBS] Failed to authenticate. Please check your password or wait a few seconds to try authentication again.");
                    await Program.obsWebSocket.Stop(WebSocketCloseStatus.NormalClosure, "Shutting down");

                    await Task.Delay(1000);

                    _logger.LogInfo("[OBS] Re-trying..");
                    await Program.obsWebSocket.Start();
                    Program.obsWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{RequiredAuthenticationGuid}\"}}");
                }
            }
            else if (msg.Text.Contains($"\"message-id\":\"{CheckIfRecording}\""))
            {
                OBSWebSocketObjects.RecordingStatus recordingStatus = JsonConvert.DeserializeObject<OBSWebSocketObjects.RecordingStatus>(msg.Text);

                OBSWebSocketObjects.OBSRecording = recordingStatus.isRecording;
                OBSWebSocketObjects.OBSRecordingPaused = recordingStatus.isRecordingPaused;

                if (recordingStatus.isRecording)
                    _logger.LogWarn($"[OBS] A recording is already running.");
            }

            if (msg.Text.Contains("\"update-type\":\"RecordingStopped\""))
            {
                OBSWebSocketObjects.RecordingStopped RecordingStopped = JsonConvert.DeserializeObject<OBSWebSocketObjects.RecordingStopped>(msg.Text);

                _logger.LogInfo($"[OBS] Recording stopped.");
                OBSWebSocketObjects.OBSRecording = false;

                if (Objects.LoadedSettings.Mod == "http-status")
                    HttpStatus.HandleFile(HttpStatusObjects.HttpStatusLastBeatmap, HttpStatusObjects.HttpStatusLastPerformance, RecordingStopped.recordingFilename, HttpStatusObjects.FinishedLastSong, HttpStatusObjects.FailedLastSong);
                else if (Objects.LoadedSettings.Mod == "datapuller")
                    DataPuller.HandleFile(DataPullerObjects.DataPullerLastBeatmap, DataPullerObjects.DataPullerLastPerformance, RecordingStopped.recordingFilename, DataPullerObjects.LastSongCombo);
            }
            else if (msg.Text.Contains("\"update-type\":\"RecordingStarted\""))
            {
                _logger.LogInfo($"[OBS] Recording started.");
                OBSWebSocketObjects.OBSRecording = true;
                while (OBSWebSocketObjects.OBSRecording)
                {
                    await Task.Delay(1000);

                    if (!OBSWebSocketObjects.OBSRecordingPaused)
                    {
                        OBSWebSocketObjects.RecordingSeconds++;
                    }
                }
                OBSWebSocketObjects.RecordingSeconds = 0;
            }
            else if (msg.Text.Contains("\"update-type\":\"RecordingPaused\""))
            {
                _logger.LogInfo($"[OBS] Recording paused.");
                OBSWebSocketObjects.OBSRecordingPaused = true;
            }
            else if (msg.Text.Contains("\"update-type\":\"RecordingResumed\""))
            {
                _logger.LogInfo($"[OBS] Recording resumed.");
                OBSWebSocketObjects.OBSRecordingPaused = false;
            }
        }

        internal static void Reconnected(ReconnectionInfo msg)
        {
            if (msg.Type != ReconnectionType.Initial)
            {
                _logger.LogInfo($"[OBS] Reconnected: {msg.Type}");

                Program.obsWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{RequiredAuthenticationGuid}\"}}");
            }
        }

        internal static void Disconnected(DisconnectionInfo msg)
        {
            try
            {
                Process[] processCollection = Process.GetProcesses();

                if (!processCollection.Any(x => x.ProcessName.ToLower().StartsWith("obs64") || x.ProcessName.ToLower().StartsWith("obs32")))
                {
                    _logger.LogWarn($"[OBS] Couldn't find an OBS process, is your OBS running? ({msg.Type})");
                }
                else
                {
                    bool FoundWebSocketDll = false;

                    string OBSInstallationDirectory = processCollection.First(x => x.ProcessName.ToLower().StartsWith("obs64") || x.ProcessName.ToLower().StartsWith("obs32")).MainModule.FileName;
                    OBSInstallationDirectory = OBSInstallationDirectory.Remove(OBSInstallationDirectory.LastIndexOf("\\bin"), OBSInstallationDirectory.Length - OBSInstallationDirectory.LastIndexOf("\\bin"));

                    if (Directory.GetDirectories(OBSInstallationDirectory).Any(x => x.ToLower().EndsWith("obs-plugins")))
                    {
                        foreach (var b in Directory.GetDirectories($"{OBSInstallationDirectory}\\obs-plugins"))
                        {
                            if (Directory.GetFiles(b).Any(x => x.Contains("obs-websocket") && x.EndsWith(".dll")))
                            {
                                FoundWebSocketDll = true;
                                break;
                            }
                        }
                    }

                    if (FoundWebSocketDll)
                        _logger.LogCritical($"[OBS] OBS seems to be running but the obs-websocket server isn't running. Please make sure you have the obs-websocket server activated! (Tools -> WebSocket Server Settings) ({msg.Type})");
                    else
                        _logger.LogCritical($"[OBS] OBS seems to be running but the obs-websocket server isn't installed. Please make sure you have the obs-websocket server installed! (To install, follow this link: https://bit.ly/3BCXfeS) ({msg.Type})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to check if obs-websocket is installed: (Disconnect Reason: {msg.Type}) {ex}");
            }
        }
    }
}
