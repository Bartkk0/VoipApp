using System.Net;
using System.Net.Sockets;
using Google.Protobuf;

internal class Program {
    static List<Client> clients = new();

    public static void Main(string[] args) {
        var listener = new TcpListener(IPAddress.Any, 3621);
        listener.Start();

        while (true) {
            Console.WriteLine("Waiting for TCP client...");
            var tcpClient = listener.AcceptTcpClient();
            var client = new Client(tcpClient);
            clients.Add(client);
            new Thread(client.Listen).Start();
        }
    }

    public static void Broadcast(ContainerMessage message) {
        Console.WriteLine("Broadcasting message");
        foreach (var client in clients) {
            Console.WriteLine("Sending to " + client);
            client.SendMessage(message);
        }
    }
}

public class Client {
    private readonly TcpClient _tcp;
    private readonly NetworkStream _stream;

    public Client(TcpClient tcpClient) {
        _tcp = tcpClient;
        _stream = tcpClient.GetStream();
    }

    public void SendMessage(ContainerMessage message) {
        message.WriteDelimitedTo(_stream);
    }

    public void Listen() {
        try {
            while (true) {
                var container = ContainerMessage.Parser.ParseDelimitedFrom(_stream);
                Console.WriteLine(container);

                switch (container.Type) {
                    case MessageType.ChatMessage:
                        var msg = container.ChatMessage;
                        msg.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        Console.WriteLine(msg);
                        Program.Broadcast(container);
                        break;
                    case MessageType.JoinMessage:
                        var join = container.JoinMessage;
                        Console.WriteLine("User joined: " + join.Name);
                        Program.Broadcast(container);
                        break;
                    case MessageType.UserUpdate:
                        Program.Broadcast(container);
                        break;
                }
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return;
        }
    }
}