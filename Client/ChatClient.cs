using System.Net.Sockets;
using Common;
using Google.Protobuf;

class ChatClient {
    private NetworkStream _stream;
    public event Action<ChatMessage>? OnMessage;
    public event Action<JoinMessage>? OnUserJoin;
    private string _username = "Bartkk " + Random.Shared.Next(100);

    public void Connect() {
        var client = new TcpClient();
        while (!client.Connected) {
            try {
                Thread.Sleep(1000);
                client.Connect("127.0.0.1", 3621);
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
        Console.WriteLine("Client connected");
    
        _stream = client.GetStream();
        new Thread(ReadLoop).Start();

        SendMsg(MessageCreator.CreateJoinMessage(new () {
            Name =_username
        }));
        
    }
    
    void SendMsg(ContainerMessage message) {
        message.WriteDelimitedTo(_stream);
    }
    
    void ReadLoop() {
        while (true) {
            var container = ContainerMessage.Parser.ParseDelimitedFrom(_stream);
            switch (container.Type) {
                case MessageType.ChatMessage:
                    var msg = container.ChatMessage;
                    Console.WriteLine($"< {msg.Name}: {msg.Content}");
                    OnMessage?.Invoke(msg);
                    break;
                case MessageType.JoinMessage:
                    var join = container.JoinMessage;
                    Console.WriteLine($"+ {join.Name}");
                    OnUserJoin?.Invoke(join);
                    break;
            }
        }
    }


    public void SendMessage(string message) {
        SendMsg(MessageCreator.CreateChatMessage(new ChatMessage() {
            Name = _username,
            Content = message
        }));
    }

}