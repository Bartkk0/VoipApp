using System.Net.Sockets;
using Client.Types;
using Common;
using Google.Protobuf;

namespace Client;

public class ChatClient {
    public readonly Dictionary<uint, TextChannel> Channels = new();
    public readonly string Hostname;
    public readonly Dictionary<uint, List<ChatMessage>> Messages = new();
    public readonly int Port;
    public readonly string Username;
    public readonly Dictionary<uint, User> Users = new();
    private uint? _profilePictureCookie;
    private NetworkStream _stream = null!;
    private readonly Dictionary<uint, Pair<SemaphoreSlim, byte[]?>> _waitingBlobs = new();
    public CacheStore<string, byte[]> ProfileStore;

    public ChatClient(string hostname, int port, string username) {
        Username = username;
        Hostname = hostname;
        Port = port;
        ProfileStore = new CacheStore<string, byte[]>(ResolveProfileImage);
    }

    public int Latency { get; private set; }

    public uint UserId { get; private set; }

    private async Task<byte[]> ResolveProfileImage(string hash) {
        var cookie = (uint)Random.Shared.Next();
        var semaphore = new SemaphoreSlim(0);
        var pair = new Pair<SemaphoreSlim, byte[]>(semaphore, null);
        _waitingBlobs.Add(cookie, pair);

        SendPacket(new ContainerPacket {
            BlobData = new PktBlobData {
                Cookie = cookie,
                Hash = hash,
                BlobType = PktBlobData.Types.BlobType.ProfilePicture,
                Upload = false
            }
        });

        await semaphore.WaitAsync();
        _waitingBlobs.Remove(cookie);
        return pair.Second;
    }


    public event Action<ChatMessage>? OnMessage;
    public event Action<User>? OnUserJoin;
    public event Action<uint>? OnUserLeave;
    public event Action<int>? OnPing;
    public event Action? OnConnected;
    public event Action<TextChannel>? OnChannelCreated;

    public async void Connect() {
        var client = new TcpClient();
        while (!client.Connected)
            try {
                await Task.Delay(1000);
                await client.ConnectAsync(Hostname, Port);
            }
            catch (SocketException e) {
                Console.WriteLine("Connection failed! Retrying in 1 second");
            }

        Console.WriteLine("Connected to server");

        _stream = client.GetStream();
        new Thread(ReadLoop).Start();
    }

    public void SendMessage(string message, TextChannel channel) {
        SendPacket(new ContainerPacket {
            ChatMessage = new PktChatMessage {
                ChannelId = channel.ChannelId,
                UserId = UserId,
                Content = message
            }
        });
    }

    public void SetProfileImage(byte[] image) {
        _profilePictureCookie = (uint)Random.Shared.Next();
        SendPacket(new ContainerPacket {
            BlobData = new PktBlobData {
                Data = ByteString.CopyFrom(image),
                Upload = true,
                Cookie = (uint)_profilePictureCookie,
                BlobType = PktBlobData.Types.BlobType.ProfilePicture
            }
        });
    }

    public async Task<User> GetUser(uint userId) {
        if (Users.ContainsKey(userId)) return Users[userId];

        // TODO: Implement fetching not existing users
        return null;
    }

    private void SendPacket(ContainerPacket message) {
        message.WriteDelimitedTo(_stream);
    }

    private async void PingTask() {
        while (true) {
            Console.WriteLine("Ping");
            SendPacket(new ContainerPacket {
                Ping = new PktPing {
                    Timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            });
            await Task.Delay(5000);
        }
    }


    private void ReadLoop() {
        PingTask();

        SendPacket(new ContainerPacket {
            Auth = new PktAuth {
                Username = Username
            }
        });

        while (true) {
            var packet = ContainerPacket.Parser.ParseDelimitedFrom(_stream);
            Console.WriteLine(packet);

            switch (packet.MessageCase) {
                case ContainerPacket.MessageOneofCase.None:
                    break;
                case ContainerPacket.MessageOneofCase.RegisterResult:
                    throw new NotImplementedException();
                    break;
                case ContainerPacket.MessageOneofCase.AuthAccept:
                    HandleAuthAccept(packet.AuthAccept);
                    break;
                case ContainerPacket.MessageOneofCase.AuthDeny:
                    throw new NotImplementedException();
                    break;
                case ContainerPacket.MessageOneofCase.UserState:
                    HandleUserState(packet.UserState);
                    break;
                case ContainerPacket.MessageOneofCase.UserRemove:
                    HandleUserRemove(packet.UserRemove);
                    break;
                case ContainerPacket.MessageOneofCase.ChannelState:
                    HandleChannelState(packet.ChannelState);
                    break;
                case ContainerPacket.MessageOneofCase.ChannelRemove:
                    throw new NotImplementedException();
                    break;
                case ContainerPacket.MessageOneofCase.ChatMessage:
                    HandleChatMessage(packet.ChatMessage);
                    break;
                case ContainerPacket.MessageOneofCase.Ping:
                    HandlePingPacket(packet.Ping);
                    break;
                case ContainerPacket.MessageOneofCase.BlobData:
                    HandleBlob(packet.BlobData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void HandleChannelState(PktChannelState packet) {
        if (Channels.ContainsKey(packet.ChannelId)) throw new NotImplementedException();

        var channel = new TextChannel {
            ChannelId = packet.ChannelId,
            Name = packet.ChannelName,
            Position = packet.Position,
            Topic = packet.TextChannel.Topic
        };
        Channels.Add(packet.ChannelId, channel);
        Messages.Add(packet.ChannelId, new List<ChatMessage>());
        OnChannelCreated?.Invoke(channel);
    }

    private void HandleAuthAccept(PktAuthAccept packet) {
        UserId = packet.UserId;
        OnConnected?.Invoke();
    }

    private void HandleUserRemove(PktUserRemove packet) {
        Users.Remove(packet.UserId);
        OnUserLeave?.Invoke(packet.UserId);
    }

    private void HandleUserState(PktUserState packet) {
        if (Users.ContainsKey(packet.UserId)) {
            // TODO: Implement user modification
            if (packet.HasImageHash) Users[packet.UserId].ImageHash = packet.ImageHash;
        }
        else {
            var user = new User {
                Client = this,
                Username = packet.Username,
                UserId = packet.UserId,
                ImageHash = packet.ImageHash,
                Online = packet.Online
            };
            Users.Add(packet.UserId, user);
            OnUserJoin?.Invoke(user);
        }
    }

    private void HandleChatMessage(PktChatMessage packet) {
        var msg = new ChatMessage {
            UserId = packet.UserId,
            Content = packet.Content,
            ChannelId = packet.ChannelId,
            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)packet.Timestamp)
        };

        Messages[packet.ChannelId].Add(msg);
        OnMessage?.Invoke(msg);
    }

    private void HandlePingPacket(PktPing packet) {
        Latency = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - (long)packet.Timestamp);
        OnPing?.Invoke(Latency);
    }

    private void HandleBlob(PktBlobData packet) {
        if (_waitingBlobs.ContainsKey(packet.Cookie)) {
            _waitingBlobs[packet.Cookie].Second = packet.Data.ToByteArray();
            _waitingBlobs[packet.Cookie].First.Release();
        }

        if (_profilePictureCookie == packet.Cookie)
            SendPacket(new ContainerPacket {
                UserState = new PktUserState {
                    UserId = UserId,
                    ImageHash = packet.Hash
                }
            });
        // TODO: Handle other blobs
    }
}