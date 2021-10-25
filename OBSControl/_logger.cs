using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace OBSControl
{
    class _logger
    {
        public static int TotalLogs = 0;
        public static int LogLevel = 4;
        public static string FileName = "";
        public static FileStream OpenedFile { get; set; }

        public static void StartLogger()
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");

            _logger.FileName = $"logs/{DateTime.UtcNow.ToString("dd-MM-yyyy_HH-mm-ss")}.log";
            _logger.OpenedFile = File.Open(_logger.FileName, FileMode.Append);

            foreach (var b in Directory.GetFiles("logs"))
            {
                try
                {
                    FileInfo fi = new(b);
                    if (fi.CreationTimeUtc < DateTime.UtcNow.AddDays(-3))
                    {
                        fi.Delete();
                        _logger.LogDebug($"{fi.Name} deleted");
                    }
                }
                catch (Exception)
                {
                    _logger.LogError($"Couldn't delete log file {b}");
                }
            }

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (Objects.LogsToPost.Count() == 0)
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        foreach (var b in Objects.LogsToPost.ToList())
                        {
                            string LogLevelText = "";
                            ConsoleColor LogLevelColor = ConsoleColor.Gray;

                            switch (b.LogLevel)
                            {
                                case 4:
                                    LogLevelText = "DEBUG";
                                    LogLevelColor = ConsoleColor.Gray;
                                    break;
                                case 3:
                                    LogLevelText = "INFO";
                                    LogLevelColor = ConsoleColor.Cyan;
                                    break;
                                case 2:
                                    LogLevelText = "WARN";
                                    LogLevelColor = ConsoleColor.Yellow;
                                    break;
                                case 1:
                                    LogLevelText = "ERROR";
                                    LogLevelColor = ConsoleColor.Red;
                                    break;
                                case 0:
                                    LogLevelText = "CRITICAL";
                                    LogLevelColor = ConsoleColor.DarkRed;
                                    break;
                                default:
                                    LogLevelText = "UNKNOWN";
                                    LogLevelColor = ConsoleColor.Gray;
                                    break;
                            }

                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    Byte[] FileWrite = Encoding.UTF8.GetBytes($"[{b.TimeOfEvent.ToString("dd.MM.yyyy HH:mm:ss")}] [{LogLevelText}] {b.Message}\n");
                                    await OpenedFile.WriteAsync(FileWrite, 0, FileWrite.Length);
                                    _logger.OpenedFile.Flush();
                                }
                                catch (Exception ex)
                                {
                                    Console.Write($"[{DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss")}] ");
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.Write($"[critical] ");
                                    Console.ResetColor();
                                    Console.WriteLine($"Failed to write log to file: {ex}");
                                }
                            });

                            switch (b.LogLevel)
                            {
                                case 4:
                                    if (_logger.LogLevel > 3)
                                    {
                                        Console.Write($"[{b.LogCount}] [{b.TimeOfEvent.ToString("dd.MM.yyyy HH:mm:ss")}] ");
                                        Console.ForegroundColor = LogLevelColor;
                                        Console.Write($"[{LogLevelText.ToLower()}] ");
                                        Console.ResetColor();
                                        Console.WriteLine(b.Message);
                                    }
                                    break;
                                case 3:
                                    if (_logger.LogLevel > 2)
                                    {
                                        Console.Write($"[{b.LogCount}] [{b.TimeOfEvent.ToString("dd.MM.yyyy HH:mm:ss")}] ");
                                        Console.ForegroundColor = LogLevelColor;
                                        Console.Write($"[{LogLevelText.ToLower()}] ");
                                        Console.ResetColor();
                                        Console.WriteLine(b.Message);
                                    }
                                    break;
                                case 2:
                                    if (_logger.LogLevel > 1)
                                    {
                                        Console.Write($"[{b.LogCount}] [{b.TimeOfEvent.ToString("dd.MM.yyyy HH:mm:ss")}] ");
                                        Console.ForegroundColor = LogLevelColor;
                                        Console.Write($"[{LogLevelText.ToLower()}] ");
                                        Console.ResetColor();
                                        Console.WriteLine(b.Message);
                                    }
                                    break;
                                case 1:
                                    if (_logger.LogLevel > 0)
                                    {
                                        Console.Write($"[{b.LogCount}] [{b.TimeOfEvent.ToString("dd.MM.yyyy HH:mm:ss")}] ");
                                        Console.ForegroundColor = LogLevelColor;
                                        Console.Write($"[{LogLevelText.ToLower()}] ");
                                        Console.ResetColor();
                                        Console.WriteLine(b.Message);
                                    }
                                    break;
                                default:
                                    Console.Write($"[{b.LogCount}] [{b.TimeOfEvent.ToString("dd.MM.yyyy HH:mm:ss")}] ");
                                    Console.ForegroundColor = LogLevelColor;
                                    Console.Write($"[{LogLevelText.ToLower()}] ");
                                    Console.ResetColor();
                                    Console.WriteLine(b.Message);
                                    break;
                            }

                            Objects.LogsToPost.Dequeue();
                        }
                    }
                    catch (Exception)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                }
            });
        }

        public static void LogDebug(string message)
        {
            TotalLogs++;

            Objects.LogsToPost.Enqueue(new Objects.LogEntry
            {
                TimeOfEvent = DateTime.UtcNow,
                LogLevel = 4,
                LogCount = TotalLogs,
                Message = message
            });
        }

        public static void LogInfo(string message)
        {
            TotalLogs++;
            
            Objects.LogsToPost.Enqueue(new Objects.LogEntry
            {
                TimeOfEvent = DateTime.UtcNow,
                LogLevel = 3,
                LogCount = TotalLogs,
                Message = message
            });
        }

        public static void LogWarn(string message)
        {
            TotalLogs++;
            
            Objects.LogsToPost.Enqueue(new Objects.LogEntry
            {
                TimeOfEvent = DateTime.UtcNow,
                LogLevel = 2,
                LogCount = TotalLogs,
                Message = message
            });
        }

        public static void LogError(string message)
        {
            TotalLogs++;

            Objects.LogsToPost.Enqueue(new Objects.LogEntry
            {
                TimeOfEvent = DateTime.UtcNow,
                LogLevel = 1,
                LogCount = TotalLogs,
                Message = message
            });
        }

        public static void LogCritical(string message)
        {
            TotalLogs++;

            Objects.LogsToPost.Enqueue(new Objects.LogEntry
            {
                TimeOfEvent = DateTime.UtcNow,
                LogLevel = 0,
                LogCount = TotalLogs,
                Message = message
            });
        }
    }
}
