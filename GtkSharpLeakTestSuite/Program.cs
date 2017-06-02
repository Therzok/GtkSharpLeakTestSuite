﻿using System;
using System.Diagnostics;
using Gtk;

namespace GtkSharpLeakTestSuite
{
	class MainClass
	{
		const bool verbose = false;

		public static void Main(string[] args)
		{
			Xwt.Application.Initialize(Xwt.ToolkitType.Gtk);
			GLib.SafeObjectHandle.InternalCreateHandle = (arg) => new LeakCheckSafeHandle (arg);

			Application.Init();
			CreateObjectsGtk();
			CreateObjectsXwt();

			DoMain();

			GC.Collect();
			GC.Collect(); 
			GC.Collect();
			GC.WaitForPendingFinalizers();

			foreach (var item in LeakCheckSafeHandle.alive) {
				System.Diagnostics.Debugger.Break();
			}

			foreach (var item in GtkConstructors.GetUnmappedConstructors())
				Console.WriteLine("Unmapped {0}", item.PrettyPrint());

			foreach (var item in GtkConstructors.GetFailures()) {
				Console.WriteLine("Failed {0}", item.ctor.PrettyPrint());
				if (verbose)
					Console.WriteLine(item.ex);
			}
		}

		static void DoMain()
		{
			var window = new MainWindow();
			Application.Run();
			window.Destroy();
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
