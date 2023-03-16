namespace Client.Types;

public record TextChannel {
    public uint ChannelId;
    public string Name;
    public int Position;
    public string Topic;
}