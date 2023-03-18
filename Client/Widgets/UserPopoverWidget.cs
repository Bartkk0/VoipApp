using Client.Types;
using Gdk;
using GLib;
using Gtk;
using Application = Gtk.Application;

namespace Client.Widgets;

[Template("UserPopoverWidget.glade", true)]
[TypeName(nameof(UserPopoverWidget))]
public class UserPopoverWidget : Popover {
    [Child] private readonly Label _nameLabel = null!;
    [Child] private readonly Image _profileImage = null!;

    public UserPopoverWidget(Widget relativeTo, User user) : base(relativeTo) {
        GetUserData(user);
    }

    private async void GetUserData(User user) {
        _nameLabel.Text = user.Username;
        LoadImage(user);

        user.OnProfileImageChanged += delegate {
            Application.Invoke(delegate {
                LoadImage(user);
            });
        };
    }

    private async void LoadImage(User user) {
        var pixbuf =  await user.GetResizedImage(_profileImage.WidthRequest, _profileImage.HeightRequest);
        if (pixbuf != null) {
            _profileImage.Pixbuf = pixbuf;
        }
    }
}