using Gdk;
using Gtk;
using JetBrains.Annotations;

namespace Client;

using UI = Builder.ObjectAttribute;

public class SettingsDialog : Dialog {
    [UI] private readonly FileChooserButton _profileChooser = null!;
    [UI] private readonly Image _profileImage = null!;

    public SettingsDialog() : this(new Builder("SettingsDialog.glade")) {
    }

    private SettingsDialog(Builder builder) : base(builder.GetRawOwnedObject("SettingsDialog")) {
        builder.Autoconnect(this);
    }

    public event Action<byte[]>? OnSave;

    [UsedImplicitly]
    private void FileChosen(object sender, EventArgs args) {
        var pixbuf = new Pixbuf(File.OpenRead(_profileChooser.Filename));
        _profileImage.Pixbuf =
            pixbuf?.ScaleSimple(_profileImage.WidthRequest, _profileImage.HeightRequest, InterpType.Bilinear);
    }

    [UsedImplicitly]
    private void OnSaveClicked(object sender, EventArgs args) {
        Console.WriteLine("Saving");
        OnSave?.Invoke(_profileImage.Pixbuf.SaveToBuffer("png"));
        Hide();
        Dispose();
    }

    [UsedImplicitly]
    private void OnCloseClicked(object sender, EventArgs args) {
        Console.WriteLine("Close");
        Hide();
        Dispose();
    }
}