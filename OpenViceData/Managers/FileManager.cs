using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenVice.Files;

namespace OpenVice.Managers {

	/// <summary>
	/// Class that handles all data hierarchy<para/>
	/// Класс, обрабатывающий всю файловую структуру
	/// </summary>
	public static class FileManager {

		/// <summary>
		/// Definitions of all IDE files<para/>
		/// Все обявления файлов IDE
		/// </summary>
		public static string[] DefinitionFiles;

		/// <summary>
		/// Definitions of all IPL files<para/>
		/// Все обявления файлов IPL
		/// </summary>
		public static string[] PlacementFiles;

		/// <summary>
		/// Definition of all COL files<para/>
		/// Все объясвления COL-файлов
		/// </summary>
		public static string[] CollisionFiles;

		/// <summary>
		/// Definition of generic TXD files<para/>
		/// Все объявления generic-текстур
		/// </summary>
		public static string[] TextureFiles;

		/// <summary>
		/// Definition of generic DFF files<para/>
		/// Все объявления generic-моделей
		/// </summary>
		public static string[] ModelFiles;

		/// <summary>
		/// Definition of all additive IMG files<para/>
		/// Все объявления дополнительных IMG-архивов
		/// </summary>
		public static string[] ArchiveFiles;

		/// <summary>
		/// Main data initialization<para/>
		/// Инициализация основных данных
		/// </summary>
		public static void InitData() {

			// Initialize data files
			// Инициализация dat-файлов
			ReadDataFiles();
		}

		/// <summary>
		/// Open GTA_VC.dat and default.dat<para/>
		/// Чтение GTA_VC.dat и default.dat
		/// </summary>
		static void ReadDataFiles() {
			List<string> ide = new List<string>(), ipl = new List<string>(), col = new List<string>(), tex = new List<string>(), mod = new List<string>(), img = new List<string>();

			TextFile d;
			for (int i = 0; i < 2; i++) {
				
				// First gta_vc, then default
				// Сначала gta_vc, потом default
				if (i == 0) {
					d = new TextFile(PathManager.GetAbsolute("data/gta_vc.dat"), false, false);
				} else {
					d = new TextFile(PathManager.GetAbsolute("data/default.dat"), false, false);
				}

				// Parsing directives
				// Обработка директив
				foreach (TextFile.Line l in d.Lines) {
					switch (l.Text[0].ToLower()) {
						// Item Definition
						// Файл IDE
						case "ide":
							ide.Add(l.Text[1]);
							break;

						// Item placement
						// Файл IPL
						case "ipl":
							ipl.Add(l.Text[1]);
							break;

						// Collision files
						// Файлы коллизий
						case "colfile":
							col.Add(l.Text[2]);
							break;

						// Texture dictionary
						// Тектстурный архив
						case "texdiction":
							tex.Add(l.Text[1]);
							break;

						// Generic DFF file
						// DFF файл
						case "modelfile":
							mod.Add(l.Text[1]);
							break;

						// Additive IMG archive
						// Дополнительный IMG-архив
						case "cdimage":
							img.Add(l.Text[1]);
							break;

						// Splash image (skip)
						// Сплэш (пропускаем)
						case "splash":
							break;

						default:
							throw new Exception("[FileManager] Unknown directive in data file: " + l.Text[0]);
					}
				}
			}

			// Saving data
			// Сохранение данных
			DefinitionFiles	= ide.ToArray();
			PlacementFiles	= ipl.ToArray();
			CollisionFiles	= col.ToArray();
			ModelFiles		= mod.ToArray();
			TextureFiles	= tex.ToArray();
			ArchiveFiles	= img.ToArray();
		}

	}
}
