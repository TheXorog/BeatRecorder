namespace Xorog.Logger;

public class LoggerObjects
{
    internal List<LogEntry> LogsToPost = new();

    internal class LogEntry
    {
        public DateTime TimeOfEvent { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
    }

    public enum LogLevel
    {
        FATAL,
        ERROR,
        WARN,
        INFO,
        DEBUG,
        NONE
    }
}
