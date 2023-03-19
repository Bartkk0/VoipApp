using Client.Types;
using Client.Widgets;
using GLib;
using Gtk;
using JetBrains.Annotations;
using Application = Gtk.Application;

namespace Client;

using UI = Builder.ObjectAttribute;

public class MainWindow : Window {
    [UI] private readonly ListBox _channelList = null!;
    private readonly ChatClient _client;
    [UI] private readonly Label _connectionLabel = null!;
    [UI] private readonly MenuBar _menuBar = null!;
    [UI] private readonly Entry _messageEntry = null!;

    [UI] private readonly ListBox _messageList = null!;
    [UI] private readonly Button _sendButton = null!;
    [UI] private readonly Label _tcLabel = null!;
    [UI] private readonly Label _topicLabel = null!;
    [UI] private readonly ListBox _userList = null!;

    private VoiceChannelWidget? _lastSelectedVc;
    private TextChannel? _selectedTextChannel;
    private NotificationManager _notificationManager = new();

    public MainWindow() : this(new Builder("MainWindow.glade")) {
    }

    private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow")) {
        builder.Autoconnect(this);

        _connectionLabel.Text = "Connecting to server...";
        _client = new ChatClient("127.0.0.1", 3621, "Bartkk " + Random.Shared.Next(100, 999));
        _client.Connect();

        _client.OnMessage += message => {
            Application.Invoke(delegate {
                if (message.ChannelId != _selectedTextChannel?.ChannelId)
                    return;

                var w = new MessageWidget(message, _client.GetUser(message.UserId));
                _messageList.Add(w);
                w.Show();
                
                _notificationManager.Message(message);
            });
        };

        _client.OnUserJoin += user => {
            Application.Invoke(delegate {
                var u = new UserWidget(user);
                _userList.Add(u);
                u.Show();
            });
        };

        _client.OnUserLeave += user => {
            Application.Invoke(delegate {
                var u = _userList.Children.First(w => ((UserWidget)((ListBoxRow)w).Child).User.UserId == user);
                _userList.Remove(u);
            });
        };

        _client.OnChannelCreated += channel => {
            Application.Invoke(delegate {
                _channelList.Add(new TextChannelWidget(channel));
                if (_selectedTextChannel == null) {
                    ChangeChannel(channel);
                }
            });
        };

        _client.OnConnected += delegate {
            Application.Invoke(delegate { _connectionLabel.Text = $"Connected to {_client.Hostname}"; });
        };


        _client.OnPing += delegate(int ping) {
            Application.Invoke(delegate { _connectionLabel.Text = $"Connected to {_client.Hostname} ({ping}ms)"; });
        };

        _messageEntry.Activated += (_, _) => OnSend();
        _sendButton.Clicked += (_, _) => OnSend();
    }

    private void OnSend() {
        if (_messageEntry.Text.Length == 0) return;
        if (_selectedTextChannel == null) return;

        _client.SendMessage(_messageEntry.Text, _selectedTextChannel);
        _messageEntry.Text = "";
    }

    [UsedImplicitly]
    private void OnDelete(object sender, DeleteEventArgs args) {
        Application.Quit();
        Environment.Exit(0);
    }

    [UsedImplicitly]
    private void ChannelsRowSelected(object sender, RowSelectedArgs args) {
        _lastSelectedVc?.DeselectAll();
        if (args.Row.Child is VoiceChannelWidget vc) _lastSelectedVc = vc;

        if (args.Row.Child is TextChannelWidget tc) ChangeChannel(tc.Channel);
    }

    [UsedImplicitly]
    private void ServerSettingsActivated(object sender, EventArgs args) {
        var dialog = new SettingsDialog();

        dialog.OnSave += delegate(byte[] image) { _client.SetProfileImage(image); };

        dialog.Show();
    }
    
    [UsedImplicitly]
    private void ClientSettingsActivated(object sender, EventArgs args) {
        var dialog = new ClientSettingsDialog();

        dialog.Show();
    }

    private void ChangeChannel(TextChannel channel) {
        _selectedTextChannel = channel;
        _tcLabel.Text = channel.Name;
        _topicLabel.Text = channel.Topic;

        foreach (var o in _messageList.Children) _messageList.Remove(o);

        var ch = _client.Messages[channel.ChannelId];
        foreach (var chatMessage in ch)
            _messageList.Add(new MessageWidget(chatMessage, _client.GetUser(chatMessage.UserId)));
    }
}