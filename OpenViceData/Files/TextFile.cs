using System.Collections.Generic;
using System.IO;

namespace OpenVice.Files {

	/// <summary>
	/// Base class for IDE, IPL and DAT<para/>
	/// Основа для файлов IDE, IPL, DAT и др.
	/// </summary>
	public class TextFile {

		/// <summary>
		/// File content<para/>
		/// Содержимое файла
		/// </summary>
		public Line[] Lines { get; private set; }

		/// <summary>
		/// File using sections (like IDE)<para/>
		/// Файл использует секции
		/// </summary>
		public bool UseSections { get; private set; }

		/// <summary>
		/// Every parameter separated with comma<para/>
		/// Каждый параметр разделен запятой
		/// </summary>
		public bool CommaSeparated { get; private set; }

		/// <summary>
		/// Parsing config filss<para/>
		/// Разбор конфигов
		/// </summary>
		/// <param name="path">File path<para/>Путь к файлу</param>
		/// <param name="sections">Use sectioned parser<para/>Использовать секционный разбор</param>
		/// <param name="commas">Lines separated by commas instead of spaces<para/>Параметры разделены запятыми вместо пробелов</param>
		public TextFile(string path, bool sections=false, bool commas=false) {

			// Setting parameters
			// Установка параметров
			UseSections = sections;
			CommaSeparated = commas;

			// Reading file
			// Чтение файла
			if (!File.Exists(path)) {
				throw new FileNotFoundException("[TextFile] File not found: "+Path.GetFileName(path), path);
			}
			string section = "";
			StreamReader f = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read));
            Dev.Console.Log("Loaded file: " + path);
			List<Line> lines = new List<Line>();
			while (!f.EndOfStream) {

				// Cleaning line content
				// Очистка данных от суррогата
				string l = f.ReadLine();
				if (l.Contains("#")) {
					l = l.Substring(0, l.IndexOf("#"));
				}
				if (l.Contains("//")) {
					l = l.Substring(0, l.IndexOf("//"));
                }
                //HACK for ViceCry mod that does not conform to the format.
                //This parser should be more robust.
                /*if (l.Contains(",	0"))
                    l = l.Replace(",	0", ",	0,");
                if (l.Contains("1	1	1"))
                    l = l.Replace("1	1	1", "1,	1,	1,");*/
                //ENDHACK

                l = l.Replace("\t", " ");
				while (l.Contains("  ")) {
					l = l.Replace("  ", " ");
				}
				l = l.Trim();

				// Reading line
				// Чтение линии
				if (l.Length>0) {
					string[] p;
					// Check for comma separated line
					// Проверка на разделенную запятой линию
					if (CommaSeparated) {
						p = l.Split(',');

                        for (int i = 0; i < p.Length; i++)
                        {
                            p[i] = p[i].Trim();
                        }
					}else{
						p = l.Split(' ');
					}

					// Check for sections
					// Проверка на секции
					if (UseSections) {
						if (section=="") {
							section = p[0];
						}else if(section!="" && p[0].ToLower()=="end"){
							section = "";
						} else {
							lines.Add(new Line(p, section));
						}
					}else{
						lines.Add(new Line(p, section));
					}
				}
			}
			Lines = lines.ToArray();
			f.Close();
		}

		/// <summary>
		/// Single entry line<para/>
		/// Одна строка из файла
		/// </summary>
		public class Line {
			/// <summary>
			/// Content<para/>
			/// Содержимое
			/// </summary>
			public string[] Text { get; private set; }

			/// <summary>
			/// File section<para/>
			/// Секция в файле
			/// </summary>
			public string Section { get; private set; }

			/// <summary>
			/// Line constructor<para/>
			/// Конструктор линии
			/// </summary>
			/// <param name="txt">Text array<para/>Массив текста</param>
			/// <param name="sect">Section<para/>Секция</param>
			public Line(string[] txt, string sect) {
				Text = txt;
				Section = sect;
			}
		}
	}
}
