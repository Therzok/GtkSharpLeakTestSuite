using System;
using System.Reflection;

namespace GtkSharpLeakTestSuite
{
	public static class HackFixify
	{
		public static bool Skip(ConstructorInfo info)
		{
			var name = info.DeclaringType.FullName;
			var par = info.GetParameters();

			// It expects a filename, probably do smarter heuristics on what parameter to pass.
			if (name == "Gtk.StatusIcon" && par.Length == 1 && par[0].ParameterType == typeof(string))
			{
				Console.WriteLine("Skipped {0} - native crash on destroy", info.PrettyPrint());
				return true;
			}

			return false;
		}

		public static T Fixify<T> (object arg)
		{
			return (T)arg;
		}

		public static void RemoveKnownStaticGtkInstances ()
		{
			LeakCheckSafeHandle.alive.Remove(Gdk.Keymap.Default.Handle);
		}
	}
}
