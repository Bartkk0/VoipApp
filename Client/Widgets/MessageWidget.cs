using Gdk;
using GLib;
using Gtk;
using DateTime = System.DateTime;

namespace Client.Widgets;

[Template("MessageWidget.glade", true)]
[TypeName(nameof(MessageWidget))]
public class MessageWidget : Bin {
    [Child] private readonly Label _nameLabel = null!;
    [Child] private readonly Label _textLabel = null!;
    [Child] private readonly Label _timeLabel = null!;
    [Child] private readonly Image _authorImage = null!;

    public MessageWidget(string name, string text, long time) {
        _nameLabel.Text = name;
        _textLabel.Text = text;
        _timeLabel.Text = DateTimeOffset.FromUnixTimeSeconds(time).ToString("T");
        LoadProfile(name);
    }

    private async void LoadProfile(string name) {
        var pixbuf = await PixbufLoader.GetFromUrl(
            Constants.ProfilePictures[name.ToCharArray().Sum(x => x) % Constants.ProfilePictures.Length]);
        _authorImage.Pixbuf =
            pixbuf.ScaleSimple(_authorImage.WidthRequest, _authorImage.HeightRequest, InterpType.Bilinear);
    }
}