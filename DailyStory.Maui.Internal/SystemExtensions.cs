using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DailyStory.Maui.Internal
{
	public static class SystemExtensions
	{
		public static void RaiseEvent(this PropertyChangedEventHandler handler, object sender, [CallerMemberName] string name = "") =>
			handler?.Invoke(sender, new PropertyChangedEventArgs(name));
	}
}
