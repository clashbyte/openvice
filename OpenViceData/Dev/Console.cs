using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Dev {

	/// <summary>
	/// Debug console<para/>
	/// Консоль разработчика
	/// </summary>
	public static class Console {

		/// <summary>
		/// Emit object into console<para/>
		/// Вывод объекта в консоль
		/// </summary>
		/// <param name="o">Object to emit<para/>Объект для вывода</param>
		public static void Log(object o) {
			Log(o.ToString());
		}

		/// <summary>
		/// Emit string into console<para/>
		/// Вывод строки в консоль
		/// </summary>
		/// <param name="o">String to emit<para/>Строка для вывода</param>
		public static void Log(string o) {

			System.Diagnostics.Debug.WriteLine(o);

		}

	}
}
