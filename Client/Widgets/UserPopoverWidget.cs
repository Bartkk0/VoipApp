using GLib;
using Gtk;

namespace Client.Widgets;

[Template("UserPopoverWidget.glade", true)]
[TypeName(nameof(UserPopoverWidget))]
public class UserPopoverWidget : Popover {
    public UserPopoverWidget(Widget relative_to) : base(relative_to) {
    }

}