using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OpenVice.Files {
	
	/// <summary>
	/// IMG archive handler<para/>
	/// Архив IMG
	/// </summary>
	public class ArchiveFile {

		/// <summary>
		/// Size of one CD sector<para/>
		/// Размер одного сектора CD
		/// </summary>
		public const int SectorSize = 2048;

		/// <summary>
		/// Size of one entry in dictionary<para/>
		/// Размер одного файла в словаре
		/// </summary>
		public const int EntrySize = 32;

		/// <summary>
		/// List of all files<para/>
		/// Список файлов
		/// </summary>
		Dictionary<string, Entry> entries;

		/// <summary>
		/// File stream<para/>
		/// Ссылка на файл
		/// </summary>
		BinaryReader stream;

		/// <summary>
		/// Constructor, opens file handle and reads filesystem<para/>
		/// Конструктор, открывает хендл файла и читает список
		/// </summary>
		/// <param name="path">Path to IMG file<para/>Путь к IMG-файлу</param>
		public ArchiveFile(string path) {

			// Opening file handle
			// Открытие ссылки
			if (!File.Exists(path)) {
				throw new FileNotFoundException("[ArchiveFile] IMG not found: " + Path.GetFileName(path), path);
			}
			stream = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));


			// Pick dictionary
			// Поиск словаря
			string dict = Path.ChangeExtension(path, ".dir");
			if (!File.Exists(dict)) {
				throw new FileNotFoundException("[ArchiveFile] IMG dictionary not found: " + Path.GetFileName(dict), dict);
			}

			// Reading files
			// Чтение словаря
			entries = new Dictionary<string, Entry>();
			BinaryReader f = new BinaryReader(new FileStream(dict, FileMode.Open, FileAccess.Read));
			int fileCount = (int)(f.BaseStream.Length / EntrySize);
			for (int i = 0; i < fileCount; i++) {
				// Reading entry
				// Чтение файла
				int offset = f.ReadInt32();
				int size = f.ReadInt32();
				string name = f.ReadVCString(24);
				Entry e = new Entry(name, offset, size, stream);

				// Adding to array
				// Добавляем в список
				if (entries.ContainsKey(name.ToLower())) {
					Dev.Console.Log("[ArchiveFile] Duplicate entry: "+name);
					entries[name.ToLower()] = e;
				}else{
					entries.Add(name.ToLower(), e);
				}
			}
			f.Close();
			Dev.Console.Log("[ArchiveFile] Mounted "+Path.GetFileName(path)+" ("+fileCount+" entries)");
		}

		/// <summary>
		/// Check for file in archive<para/>
		/// Есть ли указанный файл в архиве
		/// </summary>
		/// <param name="name">File name<para/>Имя файла</param>
		/// <returns>File exists<para/>Существует ли файл</returns>
		public bool Contains(string name) {
			if (entries!=null) {
				return entries.ContainsKey(name.ToLower());
			}
			return false;
		}

		/// <summary>
		/// Take file from archive<para/>
		/// Получение файла из архива
		/// </summary>
		/// <param name="name">File name<para/>Имя файла</param>
		/// <returns>File content<para/>Содержимое файла</returns>
		public byte[] this[string name] {
			get {
				name = name.ToLower();
				if (!Contains(name)) return null;
				Entry e = entries[name];
				e.Cache();
				return e.Data;
			}
		}

		/// <summary>
		/// Single file entry class<para/>
		/// Запись одного файла
		/// </summary>
		class Entry {

			/// <summary>
			/// Internal link to file stream
			/// </summary>
			BinaryReader reader;

			/// <summary>
			/// File name<para/>
			/// Имя файла
			/// </summary>
			public string Name { get; private set; }

			/// <summary>
			/// Position in archive<para/>
			/// Расположение файла
			/// </summary>
			public uint Position { get; private set; }

			/// <summary>
			/// File length<para/>
			/// Длина файла
			/// </summary>
			public uint Length { get; private set; }

			/// <summary>
			/// File data<para/>
			/// Содержимое файла
			/// </summary>
			public byte[] Data { get; private set; }

			/// <summary>
			/// Creates new entry link<para/>
			/// Создание новой записи
			/// </summary>
			/// <param name="name">File name<para/>Имя файла</param>
			/// <param name="sectorPos">File offset in sectors<para/>Отступ в секторах</param>
			/// <param name="sectorSize">File size in sectors<para/>Размер в секторах</param>
			/// <param name="file">Link to reader<para/>Ссылка на поток</param>
			public Entry(string name, int sectorPos, int sectorSize, BinaryReader file) {
				Name = name.ToLower();
				Position = (uint)(sectorPos * SectorSize);
				Length = (uint)(sectorSize * SectorSize);
				reader = file;
			}

			/// <summary>
			/// Cache file content<para/>
			/// Кеширование содержимого файла
			/// </summary>
			public void Cache() {
				if (Data == null) {
					reader.BaseStream.Position = Position;
					Data = reader.ReadBytes((int)Length);
				}
			}

			/// <summary>
			/// Uncache file content<para/>
			/// Обнуление кеша файла
			/// </summary>
			public void Uncache() {
				Data = null;
				GC.Collect();
			}
		}

	}
}
