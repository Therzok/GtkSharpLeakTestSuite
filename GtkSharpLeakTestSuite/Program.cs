using System;
using System.Diagnostics;
using Gtk;

namespace GtkSharpLeakTestSuite
{
	class MainClass
	{
		const bool debug = false;
		const bool verbose = false;

		public static void Main(string[] args)
		{
			Xwt.Application.Initialize(Xwt.ToolkitType.Gtk);
			var field = typeof(GLib.SafeObjectHandle).GetField("InternalCreateHandle", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			field.SetValue(null, new Func<IntPtr, GLib.SafeObjectHandle> (arg => new LeakCheckSafeHandle(arg)));

			Application.Init();
			CreateObjectsGtk();
			CreateObjectsXwt();

			DoMain();

			HackFixify.RemoveKnownStaticGtkInstances();

			if (LeakCheckSafeHandle.alive.Count != 0)
				Console.WriteLine("Found {0} leaks", LeakCheckSafeHandle.alive.Count.ToString());
			
			foreach (var item in LeakCheckSafeHandle.alive) {
				Console.WriteLine("!!!!!!!!!!!!!!!!!!!!LEAK!!!!!!!!!!!!!!!!!!!!");
				Console.WriteLine(item.Value);
				Console.WriteLine("============================================");
			}

			if (debug) {
				foreach (var item in GtkConstructors.GetUnmappedConstructors())
					Console.WriteLine("Unmapped {0}", item.PrettyPrint());

				foreach (var item in GtkConstructors.GetFailures())
				{
					Console.WriteLine("Failed {0}", item.ctor.PrettyPrint());
					if (verbose)
						Console.WriteLine(item.ex);
				}
			}
		}

		static void DoMain()
		{
			var window = new MainWindow();
			window.Show();
			window.Present();
			GLib.Timeout.Add(100, HandleTimeoutHandler);
			Application.Run();
			window.Destroy();
		}

		static int remainingEqualChangedCount = 5;
		static bool HandleTimeoutHandler()
		{
			int value = LeakCheckSafeHandle.alive.Count;

			var toplevels = Window.ListToplevels();
			var wnd = (MainWindow)toplevels[0];
			GC.Collect();
			GC.Collect();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			bool changed = value != LeakCheckSafeHandle.alive.Count;
			if (!changed)
			{
				remainingEqualChangedCount--;
				if (remainingEqualChangedCount == 0)
					return false;
				wnd.Ready.Text = "Close the window to get results";
			} else {
				remainingEqualChangedCount = 5;
			}

			return true;
		}

		static void CreateObjectsGtk()
		{
			foreach (var ctor in GtkConstructors.GetConstructors<GLib.Object>()) {
				GLib.Object obj = ctor.Invoke();
				// Constructor threw an exception, i.e. Gtk.Builder
				if (obj == null)
					continue;

				if (obj is Gtk.Object gtk)
					gtk.Destroy();
				else
				{
					obj.Dispose();
				}
			}
		}

		static void CreateObjectsXwt()
		{
			foreach (var ctor in GtkConstructors.GetConstructors<Xwt.XwtComponent>())
			{
				Xwt.XwtComponent obj = ctor.Invoke();
				// Constructor threw an exception, i.e. Gtk.Builder
				if (obj == null)
					continue;

				obj.Dispose();
			}
		}
	}
}
