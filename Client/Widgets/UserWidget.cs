using GLib;
using Gtk;

namespace Client.Widgets;

[Template("UserWidget.glade", true)]
[TypeName(nameof(UserWidget))]
public class UserWidget : Bin {
    
    [Child] private readonly Label _nameLabel = null!;
    [Child] private readonly EventBox _events = null!;
    
    public UserWidget(string name) {
        _nameLabel.Text = name;

        _events.ButtonPressEvent += delegate {
            Console.WriteLine("popover");
            var popover = new UserPopoverWidget(this);
            popover.ShowAll();
            popover.Popup();
        };
    }
}