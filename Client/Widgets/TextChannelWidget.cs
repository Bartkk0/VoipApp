using Gdk;
using GLib;
using Gtk;

namespace Client.Widgets;

[Template("TextChannelWidget.glade", true)]
[TypeName(nameof(TextChannelWidget))]
public class TextChannelWidget : Bin {
    [Child] private readonly Label _nameLabel = null!;

    public TextChannelWidget(string name) {
        _nameLabel.Text = name;
    }
}