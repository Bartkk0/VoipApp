using Client.Types;
using Gdk;
using GLib;
using Gtk;

namespace Client.Widgets;

[Template("UserPopoverWidget.glade", true)]
[TypeName(nameof(UserPopoverWidget))]
public class UserPopoverWidget : Popover {
    [Child] private readonly Label _nameLabel = null!;
    [Child] private readonly Image _profileImage = null!;

    public UserPopoverWidget(Widget relativeTo, User user) : base(relativeTo) {
        _nameLabel.Text = user.Username;
        LoadProfile(user.Username);
    }

    private async void LoadProfile(string name) {
        // TODO: Implement using actual user profile picture
        var pixbuf = await PixbufLoader.GetFromUrl(
            Constants.ProfilePictures[name.ToCharArray().Sum(x => x) % Constants.ProfilePictures.Length]);

        _profileImage.Pixbuf =
            pixbuf?.ScaleSimple(_profileImage.WidthRequest, _profileImage.HeightRequest, InterpType.Bilinear);
    }
}