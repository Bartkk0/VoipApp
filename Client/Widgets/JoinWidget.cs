using Client.Types;
using GLib;
using Gtk;

namespace Client.Widgets;

[Template("JoinWidget.glade", true)]
[TypeName(nameof(JoinWidget))]
public class JoinWidget : Bin {
    [Child] private readonly Label _nameLabel = null!;

    public JoinWidget(User user) {
        _nameLabel.Text = user.Username;
    }
}