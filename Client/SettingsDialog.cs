using Gtk;

namespace Client; 

public class SettingsDialog : Dialog {
    public SettingsDialog() : this(new Builder("SettingsDialog.glade")) { }

    private SettingsDialog(Builder builder) : base(builder.GetRawOwnedObject("SettingsDialog")) {
    }
}