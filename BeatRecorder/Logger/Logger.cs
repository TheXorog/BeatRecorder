using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Xorog.Logger
{
    public class Logger
    {
        private static bool loggerStarted = false;
        private static LoggerObjects.LogLevel maxLogLevel = LoggerObjects.LogLevel.DEBUG;

        private static string FileName = "";
        private static FileStream OpenedFile { get; set; }

        private readonly static LoggerObjects _loggerObjects = new();

        private static Task RunningLogger = null;



        /// <summary>
        /// Starts the logger with specified settings
        /// </summary>
        /// <param name="filePath">Where the current logs should be saved to, leave blank if logs shouldnt be saved</param>
        /// <param name="level">The loglevel that should be displayed in the console, does not affect whats written to file</param>
        /// <param name="cleanUpBefore">Clean up old logs before a datetime</param>
        /// <returns>A bool stating if the logger was started</returns>
        public static void StartLogger(string filePath = "", LoggerObjects.LogLevel level = LoggerObjects.LogLevel.DEBUG, DateTime cleanUpBefore = new DateTime(), bool ThrowOnFailedDeletion = false)
        {
            GC.KeepAlive(_loggerObjects.LogsToPost);

            if (loggerStarted)
                throw new Exception($"The logger is already started");

            if (filePath is not "")
            {
                FileName = filePath;
                OpenedFile = File.Open(FileName, FileMode.Append);
            }

            loggerStarted = true;
            maxLogLevel = level;

            if (cleanUpBefore != new DateTime())
            {
                foreach (var b in Directory.GetFiles("logs"))
                {
                    try
                    {
                        FileInfo fi = new(b);
                        if (fi.CreationTimeUtc < cleanUpBefore)
                        {
                            fi.Delete();
                            LogDebug($"{fi.Name} deleted");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!ThrowOnFailedDeletion)
                            LogError($"Couldn't delete log file {b}: {ex}");
                        else
                            throw new Exception($"Failed to delete {b}: {ex}");
                    }
                }
            }

            RunningLogger = Task.Run(async () =>
            {
                while (loggerStarted)
                {
                    try
                    {
                        if (_loggerObjects.LogsToPost.Count == 0)
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        foreach (var b in _loggerObjects.LogsToPost.ToList())
                        {
                            GC.KeepAlive(b);

                            string LogLevelText = b.LogLevel.ToString();
                            ConsoleColor LogLevelColor = ConsoleColor.Gray;

                            LogLevelColor = b.LogLevel switch
                            {
                                LoggerObjects.LogLevel.DEBUG => ConsoleColor.Gray,
                                LoggerObjects.LogLevel.INFO => ConsoleColor.Green,
                                LoggerObjects.LogLevel.WARN => ConsoleColor.Yellow,
                                LoggerObjects.LogLevel.ERROR => ConsoleColor.Red,
                                LoggerObjects.LogLevel.FATAL => ConsoleColor.DarkRed,
                                _ => ConsoleColor.Gray
                            };

                            if (b.LogLevel == LoggerObjects.LogLevel.DEBUG)
                            {
                                if (maxLogLevel == LoggerObjects.LogLevel.DEBUG)
                                {
                                    Console.ResetColor(); Console.Write($"[{b.TimeOfEvent:dd.MM.yyyy HH:mm:ss}] ");
                                    Console.ForegroundColor = LogLevelColor; Console.Write($"[{LogLevelText}] ");
                                    Console.ResetColor(); Console.WriteLine(b.Message);
                                    _loggerObjects.LogsToPost.Remove(b); 
                                }
                            }
                            else if (b.LogLevel == LoggerObjects.LogLevel.INFO)
                            {
                                if (maxLogLevel == LoggerObjects.LogLevel.DEBUG || maxLogLevel == LoggerObjects.LogLevel.INFO)
                                {
                                    Console.ResetColor(); Console.Write($"[{b.TimeOfEvent:dd.MM.yyyy HH:mm:ss}] ");
                                    Console.ForegroundColor = LogLevelColor; Console.Write($"[{LogLevelText}] ");
                                    Console.ResetColor(); Console.WriteLine(b.Message);
                                    _loggerObjects.LogsToPost.Remove(b); 
                                }
                            }
                            else if (b.LogLevel == LoggerObjects.LogLevel.WARN)
                            {
                                if (maxLogLevel == LoggerObjects.LogLevel.DEBUG || maxLogLevel == LoggerObjects.LogLevel.INFO || maxLogLevel == LoggerObjects.LogLevel.WARN)
                                {
                                    Console.ResetColor(); Console.Write($"[{b.TimeOfEvent:dd.MM.yyyy HH:mm:ss}] ");
                                    Console.ForegroundColor = LogLevelColor; Console.Write($"[{LogLevelText}] ");
                                    Console.ResetColor(); Console.WriteLine(b.Message);
                                    _loggerObjects.LogsToPost.Remove(b); 
                                }
                            }
                            else if (b.LogLevel == LoggerObjects.LogLevel.ERROR)
                            {
                                Console.ResetColor(); Console.Write($"[{b.TimeOfEvent:dd.MM.yyyy HH:mm:ss}] ");
                                Console.ForegroundColor = LogLevelColor; Console.Write($"[{LogLevelText}] ");
                                Console.ResetColor(); Console.WriteLine(b.Message);
                                _loggerObjects.LogsToPost.Remove(b);
                            }
                            else if (b.LogLevel == LoggerObjects.LogLevel.FATAL)
                            {
                                Console.ResetColor();
                                Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = LogLevelColor; Console.Write($"[{b.TimeOfEvent:dd.MM.yyyy HH:mm:ss}] ");
                                Console.Write($"[{LogLevelText}]");
                                Console.ResetColor(); Console.WriteLine($" {b.Message}");
                                _loggerObjects.LogsToPost.Remove(b);
                            }
                            else
                            {
                                Console.ResetColor(); Console.Write($"[{b.TimeOfEvent:dd.MM.yyyy HH:mm:ss}] ");
                                Console.ForegroundColor = LogLevelColor; Console.Write($"[{LogLevelText}] ");
                                Console.ResetColor(); Console.WriteLine(b.Message);
                                _loggerObjects.LogsToPost.Remove(b);
                            }

                            try
                            {
                                Byte[] FileWrite = Encoding.UTF8.GetBytes($"[{b.TimeOfEvent:dd.MM.yyyy HH:mm:ss}] [{LogLevelText}] {b.Message}\n");
                                if (OpenedFile != null)
                                {
                                    await OpenedFile.WriteAsync(FileWrite.AsMemory(0, FileWrite.Length));
                                    OpenedFile.Flush();
                                }
                            }
                            catch (Exception ex)
                            {
                                LogFatal($"Couldn't write log to file: {ex}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ResetColor(); Console.Write($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss} | ??] ");
                        Console.ForegroundColor = ConsoleColor.DarkRed; Console.Write($"[FATAL] ");
                        Console.ResetColor(); Console.WriteLine($"An error occured while logging: {ex}");
                        await Task.Delay(1000);
                        continue;
                    }
                }
            });
        }



        /// <summary>
        /// Stops the logger
        /// </summary>
        public static void StopLogger()
        {
            loggerStarted = false;
            maxLogLevel = LoggerObjects.LogLevel.DEBUG;
            FileName = "";

            Thread.Sleep(500);

            if (RunningLogger is not null)
                RunningLogger.Dispose();

            RunningLogger = null;

            if (OpenedFile is not null)
                OpenedFile.Close();
        }



        /// <summary>
        /// Changes the log level
        /// </summary>
        /// <param name="level"></param>
        public static void ChangeLogLevel(LoggerObjects.LogLevel level)
        {
            maxLogLevel = level;
        }



        /// <summary>
        /// Log without any LogLevel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            _loggerObjects.LogsToPost.Add(new LoggerObjects.LogEntry
            {
                TimeOfEvent = DateTime.Now,
                LogLevel = LoggerObjects.LogLevel.NONE,
                Message = message
            });
        }



        /// <summary>
        /// Log with debug log level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public static void LogDebug(string message)
        {
            _loggerObjects.LogsToPost.Add(new LoggerObjects.LogEntry
            {
                TimeOfEvent = DateTime.Now,
                LogLevel = LoggerObjects.LogLevel.DEBUG,
                Message = message
            });
        }



        /// <summary>
        /// Log with info log level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public static void LogInfo(string message)
        {
            _loggerObjects.LogsToPost.Add(new LoggerObjects.LogEntry
            {
                TimeOfEvent = DateTime.Now,
                LogLevel = LoggerObjects.LogLevel.INFO,
                Message = message
            });
        }



        /// <summary>
        /// Log with warn log level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public static void LogWarn(string message)
        {
            _loggerObjects.LogsToPost.Add(new LoggerObjects.LogEntry
            {
                TimeOfEvent = DateTime.Now,
                LogLevel = LoggerObjects.LogLevel.WARN,
                Message = message
            });
        }



        /// <summary>
        /// Log with error log level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public static void LogError(string message)
        {
            _loggerObjects.LogsToPost.Add(new LoggerObjects.LogEntry
            {
                TimeOfEvent = DateTime.Now,
                LogLevel = LoggerObjects.LogLevel.ERROR,
                Message = message
            });
        }



        /// <summary>
        /// Log with fatal log level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public static void LogFatal(string message)
        {
            _loggerObjects.LogsToPost.Add(new LoggerObjects.LogEntry
            {
                TimeOfEvent = DateTime.Now,
                LogLevel = LoggerObjects.LogLevel.FATAL,
                Message = message
            });
        }
    }

}