using Gdk;
using Gtk;
using JetBrains.Annotations;

namespace Client;

using UI = Builder.ObjectAttribute;

public class ClientSettingsDialog : Dialog {
    public ClientSettingsDialog() : this(new Builder("ClientSettingsDialog.glade")) {
    }

    private ClientSettingsDialog(Builder builder) : base(builder.GetRawOwnedObject("ClientSettingsDialog")) {
        builder.Autoconnect(this);
    }

}