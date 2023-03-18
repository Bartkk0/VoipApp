using Client.Types;
using Gdk;
using GLib;
using Gtk;
using Application = Gtk.Application;

namespace Client.Widgets;

[Template("MessageWidget.glade", true)]
[TypeName(nameof(MessageWidget))]
public class MessageWidget : Bin {
    [Child] private readonly Image _authorImage = null!;
    [Child] private readonly EventBox _events = null!;
    [Child] private readonly Label _nameLabel = null!;
    [Child] private readonly Label _textLabel = null!;
    [Child] private readonly Label _timeLabel = null!;

    public MessageWidget(ChatMessage message, Task<User> user) {
        _textLabel.Markup = message.Content;
        _timeLabel.Text = message.Timestamp.ToString("T");
        GetUserData(user);
    }

    private async void GetUserData(Task<User> user) {
        var u = await user;
        _nameLabel.Text = user.Result.Username;
        LoadImage(u);

        u.OnProfileImageChanged += delegate {
            Application.Invoke(async delegate {
                LoadImage(u);
            });
        };
        
        _events.ButtonPressEvent += (o, args) => {
            Console.WriteLine(args.Event.Button);
            if (args.Event.Button == 3) {
                var popover = new UserPopoverWidget(_authorImage,u);
                popover.ShowAll();
                popover.Popup();
            }
        };
    }

    private async void LoadImage(User user) {
        var pixbuf =  await user.GetResizedImage(_authorImage.WidthRequest, _authorImage.HeightRequest);
        if (pixbuf != null) {
            _authorImage.Pixbuf = pixbuf;
        }
    }
}