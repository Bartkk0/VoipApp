namespace Server.Types;

public record TextChannel : Channel {
    public string Topic;

    public TextChannel(uint id, string name, string topic) {
        Id = id;
        Name = name;
        Topic = topic;
    }

    public override PktChannelState Serialize() {
        return new PktChannelState {
            ChannelId = Id,
            ChannelName = Name,
            Position = Position,
            TextChannel = new PktChannelState.Types.TextChannel {
                Topic = Topic
            }
        };
    }
}