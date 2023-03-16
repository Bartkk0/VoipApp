namespace Client.Types;

public class ChatMessage {
    public uint ChannelId;
    public string Content;
    public DateTimeOffset Timestamp;
    public uint UserId;
}