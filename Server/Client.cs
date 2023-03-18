using System.Net.Sockets;
using Google.Protobuf;
using Server.Types;

namespace Server;

public class Client {
    private readonly Server _server;
    private readonly NetworkStream _stream;
    private readonly TcpClient _tcp;
    private readonly ImageStore _imageStore = new();
    private Thread _myThread = null!;

    public Client(TcpClient tcpClient, Server server) {
        _server = server;
        _tcp = tcpClient;
        _stream = tcpClient.GetStream();
    }

    public bool Authenticated { get; private set; }

    public uint UserId { get; private set; }
    public string? Username { get; private set; }
    public string? ImageHash { get; private set; }

    public void Start() {
        _myThread = new Thread(ClientThread);
        _myThread.Start();
    }

    public void Stop() {
        if (_tcp.Connected) _tcp.Close();

        _myThread?.Interrupt();
    }

    public void SendPacket(ContainerPacket packet) {
        Console.WriteLine($"Sending {packet.MessageCase}");
        packet.WriteDelimitedTo(_stream);
    }

    private void ClientThread() {
        try {
            while (true) {
                var packet = ContainerPacket.Parser.ParseDelimitedFrom(_stream);
                Console.WriteLine(packet);

                switch (packet.MessageCase) {
                    case ContainerPacket.MessageOneofCase.None:
                        Console.WriteLine("Received packet none");
                        break;
                    case ContainerPacket.MessageOneofCase.Register:
                        RegisterUser(packet.Register);
                        break;
                    case ContainerPacket.MessageOneofCase.Auth:
                        AuthenticateUser(packet.Auth);
                        break;
                    case ContainerPacket.MessageOneofCase.UserState:
                        UpdateUser(packet.UserState);
                        break;
                    case ContainerPacket.MessageOneofCase.UserRemove:
                        RemoveUser(packet.UserRemove);
                        break;
                    case ContainerPacket.MessageOneofCase.ChannelState:
                        UpdateChannel(packet.ChannelState);
                        break;
                    case ContainerPacket.MessageOneofCase.ChannelRemove:
                        RemoveChannel(packet.ChannelRemove);
                        break;
                    case ContainerPacket.MessageOneofCase.ChatMessage:
                        ChatMessage(packet.ChatMessage);
                        break;
                    case ContainerPacket.MessageOneofCase.Ping:
                        HandlePing(packet.Ping);
                        break;
                    case ContainerPacket.MessageOneofCase.QueryUsers:
                        QueryUsers(packet.QueryUsers);
                        break;
                    case ContainerPacket.MessageOneofCase.BlobData:
                        BlobData(packet.BlobData);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
            _server.DisconnectClient(this, "Connection error");
        }
    }

    private void BlobData(PktBlobData packet) {
        if (packet.Upload)
            switch (packet.BlobType) {
                case PktBlobData.Types.BlobType.ProfilePicture:
                    var hash = _imageStore.Store(packet.Data.ToByteArray());
                    packet.Hash = hash;
                    SendPacket(new ContainerPacket {
                        BlobData = packet
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        else {
            if (packet.Hash == "") return;
            switch (packet.BlobType) {
                case PktBlobData.Types.BlobType.ProfilePicture:
                    var data = _imageStore.GetByHash(packet.Hash);
                    packet.Data = ByteString.CopyFrom(data);
                    SendPacket(new ContainerPacket {
                        BlobData = packet
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void QueryUsers(PktQueryUsers packet) {
        throw new NotImplementedException();
    }

    private void HandlePing(PktPing packet) {
        SendPacket(new ContainerPacket {
            Ping = packet
        });
    }

    private void ChatMessage(PktChatMessage packet) {
        RequireAuthentication();

        var msg = new ChatMessage(
            packet.ChannelId,
            UserId,
            packet.Content,
            DateTimeOffset.Now
        );

        _server.Channels.AddMessage(msg);

        _server.Broadcast(new ContainerPacket {
            ChatMessage = msg.Serialize()
        });
    }

    private void RemoveChannel(PktChannelRemove packet) {
        throw new NotImplementedException();
    }

    private void UpdateChannel(PktChannelState packet) {
        throw new NotImplementedException();
    }

    private void RemoveUser(PktUserRemove packet) {
        RequireAuthentication();

        _server.Broadcast(new ContainerPacket {
            UserRemove = packet
        });
    }

    private void UpdateUser(PktUserState packet) {
        if (packet.HasImageHash) ImageHash = packet.ImageHash;

        _server.Broadcast(new ContainerPacket {
            UserState = packet
        });
    }

    private void AuthenticateUser(PktAuth packet) {
        // TODO: Do actual authentication
        Authenticated = true;
        Username = packet.Username;
        UserId = (uint)_server.Clients.Count;

        // Send successful authentication packet
        SendPacket(new ContainerPacket {
            AuthAccept = new PktAuthAccept {
                UserId = (uint)_server.Clients.Count, // TODO: Use registered user id
                Motd = "Very good testing server" // TODO: Use config
            }
        });

        var state = new PktUserState {
            UserId = UserId,
            Username = Username,
            Online = true
        };
        
        if (ImageHash != null) state.ImageHash = ImageHash;


        _server.Broadcast(new ContainerPacket {
            UserState = state
        });

        // Send currently online users to the client
        foreach (var client in _server.Clients) {
            state = new PktUserState {
                UserId = client.UserId,
                Username = client.Username,
                Online = true
            };

            if (client.ImageHash != null) state.ImageHash = client.ImageHash;

            SendPacket(new ContainerPacket {
                UserState = state
            });
        }

        // Send all channels to the client
        foreach (var textChannel in _server.Channels.GetChannelEnumerator()) {
            SendPacket(new ContainerPacket {
                ChannelState = new PktChannelState {
                    ChannelId = textChannel.Id,
                    ChannelName = textChannel.Name,
                    Position = textChannel.Position,
                    TextChannel = new PktChannelState.Types.TextChannel {
                        Topic = textChannel.Topic
                    }
                }
            });
            foreach (var chatMessage in _server.Channels.GetMessages(textChannel.Id))
                SendPacket(new ContainerPacket {
                    ChatMessage = chatMessage.Serialize()
                });
        }
    }

    private void RegisterUser(PktRegister packet) {
        RequireAuthentication();

        throw new NotImplementedException();
    }

    private void RequireAuthentication() {
        if (!Authenticated)
            throw new UserUnauthenticatedException();
    }

    public bool IsConnected() {
        return _tcp.Connected;
    }

    public void Disconnect(string reason) {
        _server.Broadcast(new ContainerPacket {
            UserRemove = new PktUserRemove {
                UserId = UserId,
                Reason = reason
            }
        });
        _tcp.Close();
    }

    private class UserUnauthenticatedException : Exception {
    }
}