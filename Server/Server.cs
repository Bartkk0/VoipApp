using System.Net;
using System.Net.Sockets;

namespace Server;

public class Server {
    private readonly TcpListener _listener;
    public readonly ChannelStore Channels = new();
    public readonly List<Client> Clients = new();

    public Server(IPAddress address, int port) {
        _listener = new TcpListener(address, port);

        Channels.Add("gaming-room", "Only real gamers in here");
        Channels.Add("test-room", "Testing room");
        Channels.Add("memes", "Post meymeys");
        Channels.Add("anime-rp");
    }

    public void Start() {
        _listener.Start();
        while (true) {
            var tcpClient = _listener.AcceptTcpClient();
            var client = new Client(tcpClient, this);
            Clients.Add(client);
            client.Start();
        }
    }

    public void Broadcast(ContainerPacket message) {
        Console.WriteLine($"Broadcasting {message.MessageCase}");
        foreach (var client in Clients) {
            if (!client.IsConnected())
                // TODO: Remove client when disconnected
                continue;

            if (!client.Authenticated)
                continue;

            client.SendPacket(message);
        }
    }

    public void DisconnectClient(Client client, string reason) {
        client.Disconnect(reason);
        client.Stop();
        Clients.Remove(client);
    }
}