using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK;

namespace OpenVice.Files {

	/// <summary>
	/// Base texture dictionary file class<para/>
	/// Основной класс файла текстур
	/// </summary>
	public class TextureFile : RenderWareFile {

		/// <summary>
		/// Create TextureFile from file<para/>
		/// Создание TextureFile из файла
		/// </summary>
		/// <param name="name">File name<para/>Имя файла</param>
		/// <param name="readNow">Read file immediately instead of threaded reading<para/>Прочитать файл сейчас же, не полагаясь на потоковый загрузчик</param>
		public TextureFile(string name, bool readNow = false) : base(name, readNow) {}

		/// <summary>
		/// Create TextureFile from byte stream<para/>
		/// Создание TextureFile из потока байтов
		/// </summary>
		/// <param name="data">File content<para/>Содержимое файла</param>
		/// <param name="readNow">Read file immediately instead of threaded reading<para/>Прочитать файл сейчас же, не полагаясь на потоковый загрузчик</param>
		public TextureFile(byte[] data, bool readNow = false) : base(data, readNow) {}

		/// <summary>
		/// List of all textures in file<para/>
		/// Список всех текстур в файле
		/// </summary>
		public Dictionary<string, Entry> Textures { get; private set; }

		/// <summary>
		/// Read all the data from stream<para/>
		/// Чтение файла из потока
		/// </summary>
		public override void ReadData() {
			
			// Protect from multiple reading
			// Защита от многоразового прочтения
			if (State != LoadState.None) {
				return;
			}
			State = LoadState.Reading;

			BinaryReader f = new BinaryReader(QueuedStream, Encoding.ASCII);
			Textures = new Dictionary<string, Entry>();

			ChunkHeader h;

			// TexDictionary chunk
			// Чанк TexDictionary
			h = ReadHeader(f);
			if (h.Type != ChunkType.TexDictionary) {
				throw new Exception("[TextureFile] Unexpected chunk: "+h.Type);
			}

			// STRUCT
			h = ReadHeader(f);
			if (h.Type != ChunkType.Struct) {
				throw new Exception("[TextureFile] Unexpected chunk: " + h.Type);
			}

			// Texture count
			// Количество текстур
			int texNum = f.ReadUInt16();

			// Skip stuff
			// Пропуск данных
			f.BaseStream.Position += 2;

			// Parsing textures
			// Чтение текстур
			for (int i = 0; i < texNum; i++) {

				// TextureNative chunk
				// Чанк TextureNative
				h = ReadHeader(f);
				if (h.Type != ChunkType.TextureNative) {
					throw new Exception("[TextureFile] Unexpected chunk: " + h.Type);
				}

				// Struct
				h = ReadHeader(f);
				if (h.Type != ChunkType.Struct) {
					throw new Exception("[TextureFile] Unexpected chunk: " + h.Type);
				}

				// Reading texture data
				// Чтение текстуры
				Entry tx = ReadTexture(f);

				// Adding to list
				// Добавляем в список
				Textures.Add(tx.Name, tx);

				// Reading extension
				// Чтение чанка расширений
				h = ReadHeader(f);
				f.BaseStream.Position += h.Size;
			}

			// Reading extension
			// Чтение чанка расширений
			h = ReadHeader(f);
			if (h.Type != ChunkType.Extension) {
				throw new Exception("[TextureFile] Unexpected chunk: " + h.Type);
			}

			f.Close();
			State = LoadState.Complete;
		}

		/// <summary>
		/// Take single image from stream<para/>
		/// Получение текстуры из потока
		/// </summary>
		/// <param name="f">Stream<para/>Поток</param>
		/// <returns>Texture entry<para/>Запись текстуры</returns>
		Entry ReadTexture(BinaryReader f) {
			Entry tx = new Entry();

			// Texture platform flag
			// Платформа текстуры
			uint platform = f.ReadUInt32();
			if (platform != 8) {
				throw new Exception("[TextureFile] Specified platform does not match VC: " + platform);
			}

			// Filtering flags
			// Флаги фильтрации
			tx.Filtering = (FilterMode)f.ReadByte();
			byte wrapd = f.ReadByte();
			tx.AddressU = (AddressMode)(wrapd & 0x0F);
			tx.AddressV = (AddressMode)(wrapd >> 4);
			f.BaseStream.Position += 2;

			// Texture and mask names
			// Имена текстуры и её маски
			tx.Name = f.ReadVCString(32).ToLower();
			tx.Mask = f.ReadVCString(32).ToLower();

			// Raster format
			// Формат растра
			tx.Flags = f.ReadUInt32();

			// Does alpha present
			// Есть ли альфаканал
			uint hasAlpha = f.ReadUInt32();

			// Texture dimensions
			// Размеры текстуры
			int texW = f.ReadUInt16();
			int texH = f.ReadUInt16();

			// Bit depth
			// Глубина цвета
			int depth = f.ReadByte();

			// Mipmap number
			// Количество MIP-уровней
			int mipcount = f.ReadByte();

			// Raster type (must be 4)
			// Тип растра (должен быть 4)
			int type = f.ReadByte();

			// DXT compression
			// DXT-сжатие
			tx.ScanCompression = (Compression)f.ReadByte();

			// Special palette
			// Палитра
			byte[] palette = null;

			List<Scan> scans = new List<Scan>();

			// Reading palette, if needed
			// Чтение палитры если требуется
			if ((tx.Flags & (int)RasterFlags.Palette8) > 0 || (tx.Flags & (int)RasterFlags.Palette4) > 0) {
				int paletteSize = 1024;
				if ((tx.Flags & (int)RasterFlags.Palette4) > 0) {
					paletteSize = 64;
				}
				palette = f.ReadBytes(paletteSize);
			}

			// Reading data for all mipmaps
			// Чтение MIP-уровней
			for (int mip = 0; mip < mipcount; mip++) {

				// Size of data (may be 0)
				// Размер данных (может быть равен 0)
				int dataSize = (int)f.ReadUInt32();
				if (dataSize == 0) {
					// Computing size
					// Вычисление размера
					if ((tx.Flags & (int)RasterFlags.Palette8) > 0 || (tx.Flags & (int)RasterFlags.Palette4) > 0) {
						// Format is paletted
						// Формат задан палитрой
						dataSize = texW * texH;
						if ((tx.Flags & (int)RasterFlags.Palette4) > 0) {
							dataSize /= 2;
						}
					} else if (tx.ScanCompression != Compression.Uncompressed) {
						// Format is compressed
						// Кадры подвержены сжатию
						int ttw = texW;
						int tth = texH;
						if (ttw < 4) ttw = 4;
						if (tth < 4) tth = 4;
						if (tx.ScanCompression == Compression.DXT3) {
							dataSize = (ttw / 4) * (tth / 4) * 16;
						} else {
							dataSize = (ttw / 4) * (tth / 4) * 8;
						}
					}
				}

				// Reading raw data, corresponding to size
				// Чтение данных
				byte[] data = f.ReadBytes(dataSize);
				Scan scan = new Scan();
				scan.Size = new Vector2(texW, texH);

				if ((tx.Flags & (int)RasterFlags.Palette8) > 0) {
					// Decoding paletted textures
					// Раскодирование текстур с палитрой
					byte[] output = new byte[texW * texH * 4];
					for (int i = 0; i < data.Length; i++) {
						Array.Copy(palette, data[i] * 4, output, i * 4, 4);
					}
					scan.Data = output;
				} else {
					// Generic textures, may be compressed
					// Обычные текстуры, возмножно сжатые
					scan.Data = data;
				}
				scans.Add(scan);

				texW /= 2;
				texH /= 2;
			}
			tx.Scans = scans.ToArray();

			return tx;
		}

		/// <summary>
		/// Single texture entry<para/>
		/// Одна текстура из словаря
		/// </summary>
		public class Entry {

			/// <summary>
			/// Texture name<para/>
			/// Имя текстуры
			/// </summary>
			public string Name;

			/// <summary>
			/// Texture mask name<para/>
			/// Имя маски текстуры
			/// </summary>
			public string Mask;

			/// <summary>
			/// Texture frames<para/>
			/// Кадры текстуры
			/// </summary>
			public Scan[] Scans;

			/// <summary>
			/// Raster flags<para/>
			/// Флаги растра
			/// </summary>
			public uint Flags;

			/// <summary>
			/// Texture filtering mode<para/>
			/// Режим фильтрации текстуры
			/// </summary>
			public FilterMode Filtering;

			/// <summary>
			/// Texture addressing in U<para/>
			/// Горизонтальная адресация текстуры
			/// </summary>
			public AddressMode AddressU;

			/// <summary>
			/// Texture addressing in V<para/>
			/// Вертикальная адресация текстуры
			/// </summary>
			public AddressMode AddressV;

			/// <summary>
			/// Frames compression<para/>
			/// Сжатие кадров
			/// </summary>
			public Compression ScanCompression;
		}

		/// <summary>
		/// Single texture frame<para/>
		/// Один кадр текстуры
		/// </summary>
		public class Scan {
			/// <summary>
			/// Image data<para/>
			/// Данные изображения
			/// </summary>
			public byte[] Data;

			/// <summary>
			/// Scan size<para/>
			/// Размер кадра
			/// </summary>
			public Vector2 Size;
		}

		/// <summary>
		/// Texture filtering mode<para/>
		/// Тип фильтрации текстуры
		/// </summary>
		public enum FilterMode : int {
			None				= 0,
			Nearest				= 1,
			Linear				= 2,
			MipNearest			= 3,
			MipLinear			= 4,
			LinearMipNearest	= 5,
			LinearMipLinear		= 6
		}

		/// <summary>
		/// Texture addressing mode<para/>
		/// Тип адресации текстуры
		/// </summary>
		public enum AddressMode : int {
			None	= 0,
			Repeat	= 1,
			Mirror	= 2,
			Clamp	= 3
		}

		/// <summary>
		/// Texture raster flags<para/>
		/// Флаги растра текстуры
		/// </summary>
		public enum RasterFlags : int {
			Default		= 0x0000,
			Format1555	= 0x0100,
			Format565	= 0x0200,
			Format4444	= 0x0300,
			FormatLum8	= 0x0400,
			Format8888	= 0x0500,
			Format888	= 0x0600,

			AutoMipmap	= 0x1000,
			Palette8	= 0x2000,
			Palette4	= 0x4000,
			Mipmapped	= 0x8000
		}


		/// <summary>
		/// Scan compression<para/>
		/// Сжатие кадров
		/// </summary>
		public enum Compression : int {
			Uncompressed	= 0,
			DXT1			= 1,
			DXT3			= 3
		}

	}
}
