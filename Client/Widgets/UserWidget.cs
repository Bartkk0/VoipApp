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

        _events.ButtonPressEvent += (o, args) => {
            Console.WriteLine(args.Event.Button);
            if (args.Event.Button == 3) {
                var popover = new UserPopoverWidget(this,name);
                popover.ShowAll();
                popover.Popup();
            }
        };
    }
}