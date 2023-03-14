using Gdk;
using GLib;
using Gtk;

namespace Client.Widgets;

[Template("UserPopoverWidget.glade", true)]
[TypeName(nameof(UserPopoverWidget))]
public class UserPopoverWidget : Popover {
    [Child] private readonly Image _profileImage = null!;
    [Child] private readonly Label _nameLabel = null!;

    public UserPopoverWidget(Widget relativeTo, string name) : base(relativeTo) {
        _nameLabel.Text = name;
        LoadProfile(name);
    }
    
    private async void LoadProfile(string name) {
        var pixbuf = await PixbufLoader.GetFromUrl(
            Constants.ProfilePictures[name.ToCharArray().Sum(x => x) % Constants.ProfilePictures.Length]);
        _profileImage.Pixbuf =
            pixbuf.ScaleSimple(_profileImage.WidthRequest, _profileImage.HeightRequest, InterpType.Bilinear);
    }

}