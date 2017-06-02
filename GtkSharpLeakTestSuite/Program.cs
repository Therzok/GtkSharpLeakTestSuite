using System;
using Gtk;

namespace GtkSharpLeakTestSuite
{
	class MainClass
	{
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
				Console.WriteLine("Leaked {0} from:\n {1}", item.Key, item.Value);
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
			new Gtk.Button().Destroy();
		}

		static void CreateObjectsXwt()
		{
			new Xwt.Button().Dispose();
		}
	}
}
