using Newtonsoft.Json;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace OBSControl
{
    class Program
    {
        static WebsocketClient BeatSaberWebSocket { get; set; }
        static OBSWebsocket obs { get; set; }

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(true).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            _ = Task.Run(async () =>
            {
                var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
                {
                    Options =
                            {
                                KeepAliveInterval = TimeSpan.FromSeconds(5)
                            }
                });


                BeatSaberWebSocket = new WebsocketClient(new Uri("ws://127.0.0.1:6557/socket"), factory);
                BeatSaberWebSocket.IsReconnectionEnabled = false;
                BeatSaberWebSocket.ReconnectTimeout = TimeSpan.FromSeconds(5);
                BeatSaberWebSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(5);

                BeatSaberWebSocket.MessageReceived.Subscribe(msg =>
                {
                    BeatSaberWebSocket_MessageReceived(msg.Text);
                });

                BeatSaberWebSocket.ReconnectionHappened.Subscribe(type =>
                {
                    Console.WriteLine($"Reconnect happened: {type.Type}");
                });

                BeatSaberWebSocket.DisconnectionHappened.Subscribe(type =>
                {
                    Console.WriteLine($"Disconnect happened: {type.Type}");
                });

                Console.WriteLine($"Connecting to BeatSaber..");
                BeatSaberWebSocket.Start().Wait();
                Console.WriteLine($"Connected to BeatSaber.");
            });

            Console.WriteLine("Connecting to OBSWebSocket..");

            obs = new OBSWebsocket();

            obs.Connected += onConnect;
            obs.Disconnected += onDisconnect;

            obs.Connect("ws://127.0.0.1:4444", "Fabian2016!");

            await Task.Delay(-1);
        }

        private static void onRecordingStateChange(OBSWebsocket sender, OutputState type)
        {
            Console.WriteLine($"Recording state changed: {sender.GetRecordingStatus()}");
        }

        private static void onDisconnect(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private static void onConnect(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to OBS.");
        }

        private static void BeatSaberWebSocket_MessageReceived(string e)
        {
            try
            {
                Objects.Root _status = new Objects.Root();

                _status = JsonConvert.DeserializeObject<Objects.Root>(e);

                if (_status.@event.ToLower() != "notecut" && _status.@event.ToLower() != "notefullycut" && _status.@event.ToLower() != "beatmapevent" && _status.@event.ToLower() != "scorechanged")
                    Console.WriteLine($"Update recieved: {_status.@event}");

                if (_status.@event == "hello")
                {
                    Console.WriteLine("Connected to BeatSaberWebSocket.");
                }
                else if (_status.@event == "songStart")
                {
                    Console.WriteLine("Song started.");

                    _ = Task.Run(() =>
                    {
                        try
                        {
                            obs.StartRecording();
                        }
                        catch { }
                    });
                }
                else if (_status.@event == "finished")
                {
                    Console.WriteLine("Song finished.");

                    _ = Task.Run(() =>
                    {
                        try
                        {
                            obs.StopRecording();
                        }
                        catch { }
                    });
                }
                else if (_status.@event == "failed")
                {
                    Console.WriteLine("Song failed.");

                    _ = Task.Run(() =>
                    {
                        try
                        {
                            obs.StopRecording();
                        }
                        catch { }
                    });
                }
                else if (_status.@event == "softFailed")
                {
                    Console.WriteLine("Song soft-failed.");
                }
                else if (_status.@event == "menu")
                {
                    Console.WriteLine("Menu entered.");

                    _ = Task.Run(() =>
                    {
                        try
                        {
                            obs.StopRecording();
                        }
                        catch { }
                    });
                }
                else if (_status.@event == "pause")
                {
                    Console.WriteLine("Song paused.");

                    _ = Task.Run(() =>
                    {
                        try
                        {
                            obs.PauseRecording();
                        }
                        catch { }
                    });
                }
                else if (_status.@event == "resume")
                {
                    Console.WriteLine("Song resumed.");

                    _ = Task.Run(() =>
                    {
                        try
                        {
                            obs.ResumeRecording();
                        }
                        catch { }
                    });
                }
            }
            catch { }
        }
    }
}
