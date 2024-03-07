using BeatRecorder.Entities;
using BeatRecorder.Enums;

namespace BeatRecorder.Util.OpenVR;

public class SteamNotifications
{
    List<NotificationEntry> NotificationList = new();
    ulong SteamNotificationId = 0;

    public static SteamNotifications Initialize()
    {
        SteamNotifications instance = new();

        _ = Task.Run(() =>
        {
            _logger.LogInfo("Loading Notification Assets..");
            Bitmap InfoIcon;
            Bitmap ErrorIcon;

            try
            {
                InfoIcon = new($"{AppDomain.CurrentDomain.BaseDirectory}Assets\\Info.png");
                ErrorIcon = new($"{AppDomain.CurrentDomain.BaseDirectory}Assets\\Error.png");
            }
            catch (Exception ex)
            {
                _logger.LogFatal("Failed load Notifaction Assets", ex);
                return;
            }

            while (true)
            {
                try
                {
                    if (instance.SteamNotificationId == 0)
                    {
                        _logger.LogDebug($"Initializing OpenVR..");
                        var Initialized = false;

                        while (!Initialized)
                        {
                            Initialized = EasyOpenVRSingleton.Instance.Init();
                            Thread.Sleep(500);
                        }

                        _logger.LogDebug($"Initialized OpenVR.");

                        _logger.LogDebug($"Initializing NotificationOverlay..");
                        instance.SteamNotificationId = EasyOpenVRSingleton.Instance.InitNotificationOverlay("BeatRecorder");
                        _logger.LogDebug($"Initialized NotificationOverlay: {instance.SteamNotificationId}");
                    }

                    while (instance.NotificationList.Count == 0)
                        Thread.Sleep(500);

                    NotificationBitmap_t NotifactionIcon;

                    foreach (var b in instance.NotificationList.ToList())
                    {
                        BitmapData TextureData = new();

                        if (b.Type == MessageType.INFO)
                            TextureData = InfoIcon.LockBits(new Rectangle(0, 0, InfoIcon.Width, InfoIcon.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        else if (b.Type == MessageType.ERROR)
                            TextureData = ErrorIcon.LockBits(new Rectangle(0, 0, ErrorIcon.Width, ErrorIcon.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                        NotifactionIcon.m_pImageData = TextureData.Scan0;
                        NotifactionIcon.m_nWidth = TextureData.Width;
                        NotifactionIcon.m_nHeight = TextureData.Height;
                        NotifactionIcon.m_nBytesPerPixel = 4;

                        var NotificationId = EasyOpenVRSingleton.Instance.EnqueueNotification(instance.SteamNotificationId, EVRNotificationType.Persistent, b.Message, EVRNotificationStyle.Application, NotifactionIcon);
                        _logger.LogDebug($"Displayed Notification {NotificationId}: {b.Message}");

                        if (b.Type == MessageType.INFO)
                            InfoIcon.UnlockBits(TextureData);
                        else if (b.Type == MessageType.ERROR)
                            ErrorIcon.UnlockBits(TextureData);

                        if (NotificationId == 0)
                            return;

                        Thread.Sleep(b.Delay);
                        _ = EasyOpenVRSingleton.Instance.DismissNotification(NotificationId, out var error);

                        if (error != EVRNotificationError.OK)
                        {
                            _logger.LogFatal($"Failed to dismiss notification {instance.SteamNotificationId}: {error}");
                        }

                        _logger.LogDebug($"Dismissed Notification {NotificationId}");

                        _ = instance.NotificationList.Remove(b);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to handle notifaction loop", ex);
                    Thread.Sleep(5000);
                    continue;
                }
            }
        });

        return instance;
    }

    public void SendNotification(string Text, int DisplayTime = 2000, MessageType messageType = MessageType.INFO) => this.NotificationList.Add(new NotificationEntry { Message = Text, Delay = DisplayTime, Type = messageType });
}
