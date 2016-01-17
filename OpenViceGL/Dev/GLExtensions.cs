using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Dev {

	/// <summary>
	/// Supported GL extensions library<para/>
	/// Список всех расширений GL
	/// </summary>
	public static class GLExtensions {
		/// <summary>
		/// Internal list that contains all the extensions<para/>
		/// Внутренний список, содержащий весь список расширений
		/// </summary>
		static List<string> exts;

		/// <summary>
		/// Check for extension<para/>
		/// Проверка расширения
		/// </summary>
		/// <param name="name">Extension name<para/>Имя расширения</param>
		/// <returns>True if extension supported<para/>True если расширение имеется</returns>
		public static bool Supported(string name) {
			// Caching extensions
			// Кеширование расширений
			if (exts==null) {
				Cache();
			}
			
			// Picking name in list
			// Выборка по имени
			return exts.Contains(name.ToLower());
		}

		/// <summary>
		/// Cache all GL extensions<para/>
		/// Кеширование всех расширений
		/// </summary>
		static void Cache() {
			string extensions = GL.GetString(StringName.Extensions);
			Dev.Console.Log("[Extensions] Supported by GL: "+extensions);
			exts = new List<string>();
			foreach (string n in extensions.Split(' ')) {
				exts.Add(n.ToLower());
			}
		}
	}
}
