using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GtkSharpLeakTestSuite
{
	public static class Helpers
	{
		public static IEnumerable<ConstructorInfo> GetAllConstructors(Assembly asm, Type t)
		{
			foreach (var type in asm.ExportedTypes)
			{
				if (!type.IsSubclassOf(t))
					continue;

				var ctorInfos = type.GetConstructors();
				foreach (var ctorInfo in ctorInfos)
				{
					var par = ctorInfo.GetParameters();
					if (par.Length == 1 && par[0].ParameterType == typeof(IntPtr))
						continue;

					yield return ctorInfo;
				}
			}
		}

		public static IEnumerable<ConstructorInfo> GetDefaultConstructors (Dictionary<ConstructorInfo, bool> map, Type t)
		{
			foreach (var ctorInfo in map.Keys.ToArray()) {
				var type = ctorInfo.DeclaringType;
				if (!type.IsSubclassOf(t))
					continue;

				var par = ctorInfo.GetParameters();
				if (par.Length == 0)
					yield return ctorInfo;
			}
		}

		public static IEnumerable<ConstructorInfo> GetOneParameterConstructors (Dictionary<ConstructorInfo, bool> map, Type t, Type paramType)
		{
			foreach (var ctorInfo in map.Keys.ToArray())
			{
				var type = ctorInfo.DeclaringType;
				if (!type.IsSubclassOf(t))
					continue;

				var par = ctorInfo.GetParameters();
				if (par.Length == 1 && par[0].ParameterType == paramType)
					yield return ctorInfo;
			}
		}

		public static IEnumerable<T> Mark<T>(IEnumerable<ConstructorInfo> info, Dictionary<ConstructorInfo, bool> map, Func<ConstructorInfo, T> transform)
		{
			foreach (var ctor in info) {
				if (!map.Remove(ctor))
					Console.WriteLine("Error on removal?!");
				
				yield return transform (ctor);
			}
		}
	}
}
