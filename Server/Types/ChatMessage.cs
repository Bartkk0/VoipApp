namespace Server.Types;

public record ChatMessage(uint ChannelId, uint UserId, string Content, DateTimeOffset Timestamp) {
    public uint ChannelId = ChannelId;
    public string Content = Content;
    public DateTimeOffset Timestamp = Timestamp;
    public uint UserId = UserId;

    public PktChatMessage Serialize() {
        return new PktChatMessage {
            ChannelId = ChannelId,
            UserId = UserId,
            Content = Content,
            Timestamp = (ulong)Timestamp.ToUnixTimeSeconds()
        };
    }
}