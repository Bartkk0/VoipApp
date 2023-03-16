using Client.Types;
using GLib;
using Gtk;

namespace Client.Widgets;

[Template("TextChannelWidget.glade", true)]
[TypeName(nameof(TextChannelWidget))]
public class TextChannelWidget : Bin {
    [Child] private readonly Label _nameLabel = null!;
    public TextChannel Channel;

    public TextChannelWidget(TextChannel channel) {
        Channel = channel;
        _nameLabel.Text = channel.Name;
    }
}