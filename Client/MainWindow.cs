using Client.Widgets;
using Gdk;
using Gtk;
using Window = Gtk.Window;

namespace Client;
using UI = Gtk.Builder.ObjectAttribute;

public class MainWindow : Window {
    private ChatClient _client;
    
    [UI] private readonly ListBox _messageList = null!;
    [UI] private readonly Entry _messageEntry = null!;
    [UI] private readonly Button _sendButton = null!;
    [UI] private readonly ListBox _userList = null!;
    [UI] private readonly ListBox _channelList = null!;
    [UI] private readonly MenuBar _menuBar = null!;

    private VoiceChannelWidget? _lastSelectedVc;
    
    public MainWindow() : this(new Builder("MainWindow.glade")) { }

    private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow")) {
        _client = new();
        _client.Connect();
            
        builder.Autoconnect(this);

        _client.OnMessage += message => {
            Application.Invoke(delegate {
                var w = new MessageWidget(message.Name,message.Content, message.Timestamp);
                _messageList.Add(w);
                w.Show();
            });
        };

        _client.OnUserJoin += join => {
            Application.Invoke(delegate {
                var w = new JoinWidget(join.Name);
                _messageList.Add(w);
                w.Show();

                var u = new UserWidget(join.Name);
                _userList.Add(u);
                u.Show();
            });
        };
        
        _messageEntry.Activated += (_,_) => OnSend();
        _sendButton.Clicked += (_,_) => OnSend();

        for (int i = 0; i < 10; i++) {
            var c = new TextChannelWidget("#text-channel-"+i);
            _channelList.Add(c);
            c.Show();

            var v = new VoiceChannelWidget("Test voice " + i);
            for (int j = 0; j < Random.Shared.Next(-1,5); j++) {
                v.AddUser("User " + i);
            }
            _channelList.Add(v);
            v.Show();
        }
    }

    private void OnSend() {
        if(_messageEntry.Text.Length == 0) return;

        _client.SendMessage(_messageEntry.Text);
        _messageEntry.Text = "";
    }

    private void OnDelete(object sender, DeleteEventArgs args) {
        Application.Quit();
        Environment.Exit(0);
    }

    private void ChannelsRowSelected(object sender, RowSelectedArgs args) {
            _lastSelectedVc?.DeselectAll();
            if (args.Row.Child is VoiceChannelWidget vc) {
                _lastSelectedVc = vc;
            }
    }

    private void UserSettingsActivated(object sender, EventArgs args) {
        var dialog = new SettingsDialog();
        dialog.Show();
    }
}