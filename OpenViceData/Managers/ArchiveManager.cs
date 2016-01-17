using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenVice.Files;

namespace OpenVice.Managers {
	
	/// <summary>
	/// Manager for all game archives<para/>
	/// Менеджер внутриигровых архивов
	/// </summary>
	public static class ArchiveManager {

		/// <summary>
		/// List of all mounted archives<para/>
		/// Список подключенных архивов
		/// </summary>
		static ArchiveFile[] archives;

		/// <summary>
		/// Mount all specified game archives<para/>
		/// Подключение всех игровых архивов
		/// </summary>
		public static void Mount() {

			// +1 because of hardcoded archive
			// +1 из-за жёстко зашитого архива
			archives = new ArchiveFile[FileManager.ArchiveFiles.Length+1];
			
			// Base GTA3.IMG archive
			// Основной архив
			archives[0] = new ArchiveFile(PathManager.GetAbsolute("models/gta3.img"));

			// Mounting all extension archives
			// Подключение дополнительных архивов
			for (int i = 0; i < FileManager.ArchiveFiles.Length; i++) {
				archives[i + 1] = new ArchiveFile(PathManager.GetAbsolute(FileManager.ArchiveFiles[i]));
			}

			Dev.Console.Log("[ArchiveManager] Succesfully mounted "+archives.Length+" IMG archives");
		}

		/// <summary>
		/// Check is file exist in mounted archives<para/>
		/// Проверка, существует ли файл в смонтированных архивах
		/// </summary>
		/// <param name="name">File name<para/>Имя файла</param>
		/// <returns>True if file exist<para/>True если файл существует</returns>
		public static bool Contains(string name) {
			for (int i = archives.Length-1; i >= 0; i--) {
				if (archives[i].Contains(name)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Get file from mounted archives<para/>
		/// Получение файла из архивов
		/// </summary>
		/// <param name="name">File name<para/>Имя файла</param>
		/// <returns>File content<para/>Содержимое файла</returns>
		public static byte[] Get(string name) {
			for (int i = archives.Length - 1; i >= 0; i--) {
				if (archives[i].Contains(name)) {
					return archives[i][name];
				}
			}
			return null;
		}
	}
}
