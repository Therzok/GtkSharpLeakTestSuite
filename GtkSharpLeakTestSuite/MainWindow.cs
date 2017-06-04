using System;
using Gtk;

public partial class MainWindow : Gtk.Window
{
	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();
	}

	public Label Ready => ReadyLabel;

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();

		// Force remove these to ensure we don't report them as leaked.
		GtkSharpLeakTestSuite.LeakCheckSafeHandle.alive.Remove(Handle);
		GtkSharpLeakTestSuite.LeakCheckSafeHandle.alive.Remove(ReadyLabel.Handle);
		a.RetVal = true;
	}
}
