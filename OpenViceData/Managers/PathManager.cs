using System;
using System.IO;

namespace OpenVice.Managers {

	/// <summary>
	/// Game directory path manager<para/>
	/// Менеджер пути до папки с игрой
	/// </summary>
	public static class PathManager {

		/// <summary>
		/// Game directory path<para/>
		/// Ссылка на папку игры
		/// </summary>
		public static string GamePath;

		/// <summary>
		/// Get link to game file<para/>
		/// Получение пути игрового файла
		/// </summary>
		/// <param name="file">Relative path<para/>Относительный путь</param>
		/// <returns>Absolute path<para/>Полный путь</returns>
		public static string GetAbsolute(string file) {
			if (!Path.IsPathRooted(file)) {
				return Path.Combine(GamePath, file);
			}
			return file;
		}
	}
}
