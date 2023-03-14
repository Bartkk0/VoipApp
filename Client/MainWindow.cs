using Client.Widgets;
using Gtk;

namespace Client;
using UI = Gtk.Builder.ObjectAttribute;

public class MainWindow : Window {
    private ChatClient _client;
    
    [UI] private ListBox _messageList = null!;
    [UI] private Entry _messageEntry = null!;
    [UI] private Button _sendButton = null!;
    [UI] private ListBox _userList = null!;
    [UI] private ListBox _channelList = null!;

    private VoiceChannelWidget? _lastSelectedVc;
    
    public MainWindow() : this(new Builder("MainWindow.glade")) { }

    private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow")) {
        _client = new();
        _client.Connect();

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
        
        
        builder.Autoconnect(this);

        _messageEntry.Activated += (_,_) => OnSend();
        _sendButton.Clicked += (_,_) => OnSend();

        _channelList.RowSelected += (sender, args) => {
            _lastSelectedVc?.DeselectAll();
            if (args.Row.Child is VoiceChannelWidget vc) {
                _lastSelectedVc = vc;
            }
        };

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

        // var dialog = new SettingsDialog();
        // dialog.Show();
    }

    private void OnSend() {
        if(_messageEntry.Text.Length == 0) return;

        _client.SendMessage(_messageEntry.Text);
        _messageEntry.Text = "";
    }
}