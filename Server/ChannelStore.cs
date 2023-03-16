using Microsoft.Data.Sqlite;
using Server.Types;

namespace Server;

public class ChannelStore {
    private readonly Dictionary<uint, TextChannel> _channels = new();
    private readonly Dictionary<uint, List<ChatMessage>> _messages = new();
    private readonly SqliteConnection _sqlite;
    private uint _nextId;

    public ChannelStore() {
        _sqlite = new SqliteConnection("Data Source=database.db");
        _sqlite.Open();

        // var cmd = _sqlite.CreateCommand("CREATE TABLE ");
        // cmd.ExecuteNonQuery();
    }

    public void Add(string name, string topic = "") {
        var tc = new TextChannel(_nextId++, name, topic);
        _channels.Add(tc.Id, tc);
        _messages.Add(tc.Id, new List<ChatMessage>());
        // var cmd = _sqlite.CreateCommand();
    }

    public void AddMessage(ChatMessage message) {
        _messages[message.ChannelId].Add(message);
    }

    public IEnumerable<TextChannel> GetChannelEnumerator() {
        return _channels.Values.AsEnumerable();
    }

    public IEnumerable<ChatMessage> GetMessages(uint channelId) {
        return _messages[channelId].AsEnumerable();
    }
}