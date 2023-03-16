using Client;
using Gdk;
using GLib;
using Gtk;
using Application = Gtk.Application;

internal class Program {
    [STAThread]
    public static void Main(string[] args) {
        Application.Init();

        var app = new Application("pl.bartkk.voipapp", ApplicationFlags.None);
        // app.Register(GLib.Cancellable.Current);
        var css = new CssProvider();
        css.LoadFromResource("style.css");
        StyleContext.AddProviderForScreen(Screen.Default, css, StyleProviderPriority.Application);

        var win = new MainWindow();
        app.AddWindow(win);

        win.Show();
        Application.Run();
    }
}