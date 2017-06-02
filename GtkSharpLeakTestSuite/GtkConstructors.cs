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
					unmappedConstructors[ctor] = true;

			foreach (var ctor in Helpers.GetAllConstructors(typeof(Xwt.Application).Assembly, typeof(Xwt.XwtComponent)))
				unmappedConstructors[ctor] = true;
		}

		static Func<T> CreateWrapper<T> (ConstructorInfo info, params object[] values) where T:class
		{
			return new Func<T>(() =>
			{
				try
				{
					if (HackFixify.Skip(info))
						return null;
					return HackFixify.Fixify<T>(info.Invoke(values));
				}
				catch (Exception ex)
				{
					failedConstructors.Add(info, ex);
					return null;
				}
			});
		}

		static readonly Dictionary<ConstructorInfo, Exception> failedConstructors = new Dictionary<ConstructorInfo, Exception>();
		static readonly Dictionary<ConstructorInfo, bool> unmappedConstructors = new Dictionary<ConstructorInfo, bool>();

		static IEnumerable<Func<T>> GetDefaultConstructors<T> () where T:class
		{
			return Helpers.Mark (
				Helpers.GetDefaultConstructors(unmappedConstructors, typeof(T)),
				unmappedConstructors,
				info => CreateWrapper<T>(info, null));
		}

		static IEnumerable<Func<T>> GetOneParameterConstructors<T, TValue>(TValue value) where T:class
		{
			return Helpers.Mark(
				Helpers.GetOneParameterConstructors(unmappedConstructors, typeof(T), typeof(TValue)),
				unmappedConstructors,
				info => CreateWrapper<T>(info, value));
		}

		public static IEnumerable<Func<T>> GetConstructors<T>() where T:class
		{
			foreach (var ctor in GetDefaultConstructors<T>())
				yield return ctor;

			foreach (var ctor in GetOneParameterConstructors<T, string>("test"))
				yield return ctor;
		}

		public static IEnumerable<ConstructorInfo> GetUnmappedConstructors()
		{
			foreach (var item in unmappedConstructors)
				yield return item.Key;
		}

		public static IEnumerable<(ConstructorInfo ctor, Exception ex)> GetFailures ()
		{
			foreach (var item in failedConstructors)
				yield return (item.Key, item.Value);
		}
	}
}
