using OpenTK.Graphics.OpenGL;
using OpenVice.Dev;
using OpenVice.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Graphics {

	/// <summary>
	/// Internal texture file<para/>
	/// Внутренний файл текстур
	/// </summary>
	public class TextureDictionary {

		/// <summary>
		/// Base texture file<para/>
		/// Основной файл текстур
		/// </summary>
		public TextureFile File;

		/// <summary>
		/// List of dictionary's textures<para/>
		/// Список всех текстур в архиве
		/// </summary>
		public Dictionary<string, Texture> Textures;

		/// <summary>
		/// Flag that engine shouldn't clear this dictionary<para/>
		/// Флаг что движок не должен освобождать этот архив для экономии памяти
		/// </summary>
		public bool Important;

		/// <summary>
		/// Texture uploading state<para/>
		/// Состояние загрузки текстуры
		/// </summary>
		public ReadyState State { get; private set; }

		/// <summary>
		/// Use count per pipeline<para/>
		/// Количество использований
		/// </summary>
		public int UseCount;

		/// <summary>
		/// Creating dictionary from file<para/>
		/// Создание архива из существующего файла
		/// </summary>
		/// <param name="tf">TextureFile</param>
		public TextureDictionary(TextureFile tf, bool important = false, bool sendNow = false) {
			
			// If file is not ready - error
			// Если текстурный архив не прочитан - ошибка
			if (sendNow && tf.State != RenderWareFile.LoadState.Complete) {
				throw new Exception("[TextureDictionary] Trying to create texture dictionary from incomplete file");
			}
			
			File = tf;
			Important = important;

			if (sendNow) {
				BuildTextures();
				SendTextures();
			}
		}

		/// <summary>
		/// Creating textures<para/>
		/// Создание текстур
		/// </summary>
		public void BuildTextures() {
			Textures = new Dictionary<string, Texture>();
			foreach (KeyValuePair<string, TextureFile.Entry> k in File.Textures) {
				string tName = k.Key.ToLower();
				Texture t = new Texture(tName, this);
				Textures.Add(tName, t);
			}
			State = ReadyState.NotSent;
		}

		/// <summary>
		/// Send textures to GPU<para/>
		/// Отправка текстур на GPU
		/// </summary>
		public void SendTextures() {
			foreach (KeyValuePair<string, Texture> k in Textures) {
				k.Value.Send();
			}
			State = ReadyState.Complete;
		}

		/// <summary>
		/// Remove texture data<para/>
		/// Удаление данных текстур
		/// </summary>
		public void Destroy() {
			if (Textures!=null) {
				foreach (KeyValuePair<string, Texture> k in Textures) {
					k.Value.Release();
				}
				Textures = null;
			}
			State = ReadyState.Obsolette;
		}


		/// <summary>
		/// Single texture class<para/>
		/// Одна текстура из архива
		/// </summary>
		public class Texture {

			/// <summary>
			/// Associated texture archive<para/>
			/// Родительский архив текстур
			/// </summary>
			public TextureDictionary Parent { get; private set; }
			
			/// <summary>
			/// Texture uploading state<para/>
			/// Состояние загрузки текстуры
			/// </summary>
			public ReadyState State { get; private set; }

			/// <summary>
			/// Texture contains mipmaps<para/>
			/// Текстура имеет mip-уровни
			/// </summary>
			public bool Mipmapped { get; private set; }

			/// <summary>
			/// GL texture<para/>
			/// GL-текстура
			/// </summary>
			int gltex;

			/// <summary>
			/// Iternal texture name<para/>
			/// Внутреннее имя текстуры
			/// </summary>
			string name;

			/// <summary>
			/// Create texture from dictionary<para/>
			/// Создание текстуры из архива
			/// </summary>
			/// <param name="texName">Texture name<para/>Имя текстуры в архиве</param>
			/// <param name="parent">Parental texture dictionary<para/>Текстурный архив</param>
			public Texture(string texName, TextureDictionary parent) {
				Parent = parent;
				name = texName;
				State = ReadyState.NotSent;
			}

			/// <summary>
			/// Bind texture for rendering<para/>
			/// Включение текстуры в рендер
			/// </summary>
			public void Bind() {
				if (State != ReadyState.Complete) {
					return;
				}

				// Assign texture to GL
				// Включение текстуры в GL
				GL.BindTexture(TextureTarget.Texture2D, gltex);
			}

			/// <summary>
			/// Sending texture to video card<para/>
			/// Отправка текстуры на видеокарту
			/// </summary>
			public void Send() {
				if (State != ReadyState.NotSent) {
					return;
				}

				bool texEnabled = GL.IsEnabled(EnableCap.Texture2D);
				if (!texEnabled) {
					GL.Enable(EnableCap.Texture2D);
				}

				// Searching in parent
				// Поиск в родителе
				TextureFile.Entry e = Parent.File.Textures[name.ToLower()];

				// Generating texture
				// Генерация текстуры
				gltex = GL.GenTexture();
				GL.BindTexture(TextureTarget.Texture2D, gltex);
				GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.Modulate);
				
				// Sending scans
				// Отправка сканов
				int mipLevel = 0;
				byte[] texData;
				bool useCompression = false;
				PixelInternalFormat internalFormat = PixelInternalFormat.Four;
				PixelFormat pixelFormat = PixelFormat.Rgba;
				PixelType pixelType = PixelType.UnsignedByte;

				// Check for compression
				// Проверка на сжатие по DXT
				if (e.ScanCompression != TextureFile.Compression.Uncompressed) {

					// Decoding if compression is not supported
					// Раскодирование, если сжатие аппаратно не поддерживается
					if (!Managers.TextureManager.CompressionSupported) {
						useCompression = false;
					} else {
						// Compression is supported, so we can send it directlty to GPU
						// Сжатие аппаратно поддерживается, можно отсылать на видеокарту
						useCompression = true;
						internalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
						if (e.ScanCompression == TextureFile.Compression.DXT3) {
							internalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
						}
					}
				} else {

					// Common texture, determine type by texture mode
					// Обычная текстура, разбор типа по режиму
					if ((e.Flags & (int)TextureFile.RasterFlags.Format1555) != 0) {
						pixelType = PixelType.UnsignedShort5551;
					} else if ((e.Flags & (int)TextureFile.RasterFlags.Format565) != 0) {
						pixelType = PixelType.UnsignedShort565;
					} else if ((e.Flags & (int)TextureFile.RasterFlags.Format4444) != 0) {
						pixelType = PixelType.UnsignedShort565;
					} else if ((e.Flags & (int)TextureFile.RasterFlags.FormatLum8) != 0) {
						pixelType = PixelType.UnsignedByte;
						internalFormat = PixelInternalFormat.Luminance8;
						pixelFormat = PixelFormat.Luminance;
					} else if ((e.Flags & (int)TextureFile.RasterFlags.Format888) != 0) {
						pixelType = PixelType.UnsignedByte;
						internalFormat = PixelInternalFormat.Three;
						pixelFormat = PixelFormat.Rgb;
					}
				}

				// Sending scans
				// Отправка сканов
				foreach (TextureFile.Scan scan in e.Scans) {
					// Decode scans if needed
					// Раскодирование скана если требуется
					if (e.ScanCompression != TextureFile.Compression.Uncompressed && !useCompression) {
						texData = DXTDecoder.Decode((int)scan.Size.X, (int)scan.Size.Y, scan.Data, (e.ScanCompression == TextureFile.Compression.DXT3) ? DXTDecoder.CompressionType.DXT3 : DXTDecoder.CompressionType.DXT1);
					} else {
						texData = scan.Data;
					}

					// Determine compression while sending
					// Выборка сжатия при отправке
					if (useCompression) {
						GL.CompressedTexImage2D(TextureTarget.Texture2D, mipLevel, internalFormat, (int)scan.Size.X, (int)scan.Size.Y, 0, texData.Length, texData);
					} else {
						GL.TexImage2D(TextureTarget.Texture2D, mipLevel, internalFormat, (int)scan.Size.X, (int)scan.Size.Y, 0, pixelFormat, pixelType, texData);
					}
					mipLevel++;
				}
				Mipmapped = e.Scans.Length > 1;
				if (!texEnabled) {
					GL.Disable(EnableCap.Texture2D);
				}
				State = ReadyState.Complete;
			}

			/// <summary>
			/// Release texture memory
			/// Освобождение памяти, занимаемой текстурой
			/// </summary>
			public void Release() {
				Parent = null;
				if (gltex != 0) GL.DeleteTexture(gltex);
				gltex = 0;
				State = ReadyState.Obsolette;
			}

		}

		/// <summary>
		/// Texture dictionary state<para/>
		/// Состояние архива текстур
		/// </summary>
		public enum ReadyState {
			Empty,
			Reading,
			NotSent,
			Complete,
			Obsolette
		}
	}
}
