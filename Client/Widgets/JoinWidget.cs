using GLib;
using Gtk;

namespace Client.Widgets;

[Template("JoinWidget.glade", true)]
[TypeName(nameof(JoinWidget))]
public class JoinWidget : Bin {
    
    [Child] private Label _nameLabel = null!;
    
    public JoinWidget(string name)  {
        _nameLabel.Text = name;
    }
}