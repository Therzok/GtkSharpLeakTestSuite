using System;
using System.Collections.Generic;
using System.Reflection;

namespace GtkSharpLeakTestSuite
{
	static class GtkConstructors
	{
		static GtkConstructors()
		{
			Assembly[] assemblies = {
				typeof(Atk.Global).Assembly,
				typeof(Gdk.Global).Assembly,
				typeof(Glade.Global).Assembly,
				typeof(GLib.Global).Assembly,
				typeof(Gtk.Global).Assembly,
				typeof(Pango.Global).Assembly,
			};

			foreach (var asm in assemblies)
				foreach (var ctor in Helpers.GetAllConstructors(asm, typeof(GLib.Object)))
					mappedGtkObjectConstructors[ctor] = false;
		}

		static Func<GLib.Object> CreateWrapper (ConstructorInfo info, params object[] values)
		{
			return new Func<GLib.Object>(() =>
			{
				try
				{
					if (HackFixify.Skip(info))
						return null;
					return HackFixify.Fixify(info.Invoke(values));
				}
				catch (Exception ex)
				{
					failedConstructors.Add(info, ex);
					return null;
				}
			});
		}

		static readonly Dictionary<ConstructorInfo, Exception> failedConstructors = new Dictionary<ConstructorInfo, Exception>();
		static readonly Dictionary<ConstructorInfo, bool> mappedGtkObjectConstructors = new Dictionary<ConstructorInfo, bool>();

		static IEnumerable<Func<GLib.Object>> GetDefaultConstructors ()
		{
			return Helpers.Mark (
				Helpers.GetDefaultConstructors(mappedGtkObjectConstructors, typeof(GLib.Object)),
				mappedGtkObjectConstructors,
				info => CreateWrapper(info, null));
		}

		static IEnumerable<Func<GLib.Object>> GetOneParameterConstructors<T>(T value)
		{
			return Helpers.Mark(
				Helpers.GetOneParameterConstructors(mappedGtkObjectConstructors, typeof(GLib.Object), typeof(T)),
				mappedGtkObjectConstructors,
				info => CreateWrapper(info, value));
		}

		public static IEnumerable<Func<GLib.Object>> GetConstructors()
		{
			foreach (var ctor in GetDefaultConstructors())
				yield return ctor;

			foreach (var ctor in GetOneParameterConstructors("test"))
				yield return ctor;
		}

		public static IEnumerable<(ConstructorInfo ctor, Exception ex)> GetFailures ()
		{
			foreach (var item in failedConstructors)
				yield return (item.Key, item.Value);
		}
	}
}
