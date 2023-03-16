using Client.Types;
using GLib;
using Gtk;

namespace Client.Widgets;

[Template("VoiceChannelWidget.glade", true)]
[TypeName(nameof(VoiceChannelWidget))]
public class VoiceChannelWidget : Bin {
    [Child] private readonly Label _nameLabel = null!;
    [Child] private readonly ListBox _userList = null!;

    public VoiceChannelWidget(string name) {
        _nameLabel.Text = name;
    }

    public void AddUser(User user) {
        var u = new UserWidget(user);
        _userList.Add(u);
    }

    public void DeselectAll() {
        _userList.UnselectAll();
    }
}