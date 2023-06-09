namespace Server.Types;

public abstract record Channel {
    public uint Id;
    public string Name = null!;
    public int Position = 0;

    public abstract PktChannelState Serialize();
}