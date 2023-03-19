using Client.Types;
using GLib;

namespace Client;

public class NotificationManager
{
    public void Message(ChatMessage message)
    {
        var notif = new Notification($"New message");
        notif.Body = message.Content;
        Application.Default.SendNotification(message.Content,notif);
    }
}