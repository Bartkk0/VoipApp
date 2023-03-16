using Client.Types;
using GLib;
using Gtk;

namespace Client.Widgets;

[Template("UserWidget.glade", true)]
[TypeName(nameof(UserWidget))]
public class UserWidget : Bin {
    [Child] private readonly EventBox _events = null!;

    [Child] private readonly Label _nameLabel = null!;
    public User User;

    public UserWidget(User user) {
        User = user;
        _nameLabel.Text = user.Username;

        _events.ButtonPressEvent += (o, args) => {
            Console.WriteLine(args.Event.Button);
            if (args.Event.Button == 3) {
                var popover = new UserPopoverWidget(this, user);
                popover.ShowAll();
                popover.Popup();
            }
        };
    }
}