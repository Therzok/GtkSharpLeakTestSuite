using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gtk;

namespace GtkSharpLeakTestSuite
{
	class MainClass
	{
		const bool debug = false;
		const bool verbose = false;
		const bool profile = false;

		internal static Dictionary<IntPtr, string> gobjectDict = new Dictionary<IntPtr, string>();
		public static void Main(string[] args)
		{
			Xwt.Application.Initialize(Xwt.ToolkitType.Gtk);

			var type = typeof(GLib.Object).Assembly.GetType("GLib.PointerWrapper");
			if (type == null)
			{
				return;
			}

			var field = type.GetField("ObjectCreated", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			field.SetValue(null, new Action<IntPtr>(arg => {
				lock (gobjectDict)
					gobjectDict.Add(arg, Environment.StackTrace);
			}));

			field = type.GetField("ObjectDestroyed", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			field.SetValue(null, new Action<IntPtr>(arg => {
				lock (gobjectDict)
					gobjectDict.Remove(arg);
			}));

			Application.Init();
			CreateObjectsGtk();
			CreateObjectsXwt();

			DoMain();

			HackFixify.RemoveKnownStaticGtkInstances();

			if (gobjectDict.Count != 0)
				Console.WriteLine("Found {0} leaks", gobjectDict.Count.ToString());
			
			foreach (var item in gobjectDict) {
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

			if (profile)
				System.Threading.Thread.Sleep(10000);
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
			int value;
			lock (gobjectDict)
				value = gobjectDict.Count;

			var toplevels = Window.ListToplevels();
			var wnd = toplevels.OfType<MainWindow> ().Single();
			GC.Collect();
			GC.Collect();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			int newValue;
			lock (gobjectDict)
				newValue = gobjectDict.Count;
			bool changed = value != newValue;
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
