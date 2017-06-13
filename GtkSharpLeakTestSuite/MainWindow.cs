using System;
using Gtk;
using GtkSharpLeakTestSuite;

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
		MainClass.gobjectDict.Remove(Handle);
		MainClass.gobjectDict.Remove(ReadyLabel.Handle);
		a.RetVal = true;
	}
}
