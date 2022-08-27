using BeatRecorder.Enums;

namespace BeatRecorder.Entities;

internal class NotificationEntry
{
    public string Message { get; set; }
    public int Delay { get; set; } = 2000;
    public MessageType Type { get; set; } = MessageType.INFO;
}
