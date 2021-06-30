using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace OBSControl
{
    class Program
    {
        static WebsocketClient BeatSaberWebSocket { get; set; }
        static WebsocketClient OBSWebSocket { get; set; }
        static HttpClient OBSHttpClient { get; set; }

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

            _ = Task.Run(async () =>
            {
                var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
                {
                    Options =
                            {
                                KeepAliveInterval = TimeSpan.FromSeconds(5)
                            }
                });

                OBSWebSocket = new WebsocketClient(new Uri("ws://127.0.0.1:4444"), factory);
                OBSWebSocket.IsReconnectionEnabled = false;
                OBSWebSocket.ReconnectTimeout = TimeSpan.FromSeconds(5);
                OBSWebSocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(5);

                string RequiredAuthenticationGuid = Guid.NewGuid().ToString();
                string AuthenticationGuid = Guid.NewGuid().ToString();

                OBSWebSocket.MessageReceived.Subscribe(msg =>
                {
                    Console.WriteLine($"Message recieved: {msg}");

                    if (msg.Text.Contains($"\"message-id\":\"{RequiredAuthenticationGuid}\""))
                    {
                        Objects.AuthenticationRequired required = JsonConvert.DeserializeObject<Objects.AuthenticationRequired>(msg.Text);

                        if (required.authRequired)
                        {
                            string secret_string = "Fabian2016!" + required.salt;
                            Console.WriteLine(secret_string);
                            string secret_hash = ComputeSha256Hash(secret_string);
                            Console.WriteLine(secret_hash);
                            string secret = Base64Encode(secret_hash);
                            Console.WriteLine(secret);

                            string auth_response_string = secret + required.challenge;
                            Console.WriteLine(auth_response_string);
                            string auth_response_hash = ComputeSha256Hash(auth_response_string);
                            Console.WriteLine(auth_response_hash);
                            string auth_response = Base64Encode(auth_response_hash);
                            Console.WriteLine(auth_response);

                            OBSWebSocket.Send($"{{\"request-type\":\"Authenticate\", \"message-id\":\"{AuthenticationGuid}\", \"auth\":\"{auth_response}\"}}");
                        }
                    }
                });

                OBSWebSocket.ReconnectionHappened.Subscribe(type =>
                {
                    Console.WriteLine($"Reconnect happened: {type.Type}");
                });

                OBSWebSocket.DisconnectionHappened.Subscribe(type =>
                {
                    Console.WriteLine($"Disconnect happened: {type.Type}");
                });

                Console.WriteLine($"Connecting to OBS..");
                OBSWebSocket.Start().Wait();

                OBSWebSocket.Send($"{{\"request-type\":\"GetAuthRequired\", \"message-id\":\"{RequiredAuthenticationGuid}\"}}");

                Console.WriteLine($"Connected to OBS.");
            });

            await Task.Delay(-1);
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
                            OBSWebSocket.Send($"{{\"request-type\":\"StartRecording\", \"message-id\":\"StartRecording\"}}");
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
                            OBSWebSocket.Send($"{{\"request-type\":\"StopRecording\", \"message-id\":\"StartRecording\"}}");
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
                            OBSWebSocket.Send($"{{\"request-type\":\"StopRecording\", \"message-id\":\"StartRecording\"}}");
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
                            OBSWebSocket.Send($"{{\"request-type\":\"StopRecording\", \"message-id\":\"StartRecording\"}}");
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
                            OBSWebSocket.Send($"{{\"request-type\":\"StopRecording\", \"message-id\":\"StartRecording\"}}");
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
                            // obs.ResumeRecording();
                        }
                        catch { }
                    });
                }
            }
            catch { }
        }

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
