using System;
using System.Collections.Generic;

namespace GtkSharpLeakTestSuite
{
	public class LeakCheckSafeHandle : GLib.SafeObjectHandle
	{
		public static Dictionary<IntPtr, string> alive = new Dictionary<IntPtr, string>();

		public LeakCheckSafeHandle(IntPtr handle) : base(handle)
		{
			alive.Add(handle, Environment.StackTrace);
		}

		protected override bool ReleaseHandle()
		{
			alive.Remove(handle);
			return base.ReleaseHandle();
		}
	}
}