using OpenVice.Files;
using OpenVice.Graphics;
using OpenVice.Dev;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System;

namespace OpenVice.Managers {
	
	/// <summary>
	/// Base texture manager<para/>
	/// Основной менеджер текстур
	/// </summary>
	public static class TextureManager {

		/// <summary>
		/// Is DXT compression supported<para/>
		/// Поддреживается ли DXT-сжатие
		/// </summary>
		public static bool CompressionSupported { get; private set; }

		/// <summary>
		/// Is frame buffer supported<para/>
		/// Поддерживаются ли фреймбуфферы
		/// </summary>
		public static bool FrameBufferSupported { get; private set; }

		/// <summary>
		/// Cached texture dictionares list<para/>
		/// Список кешированных архивов текстур
		/// </summary>
		public static ConcurrentDictionary<string, TextureDictionary> Cached = new ConcurrentDictionary<string, TextureDictionary>();

		/// <summary>
		/// List of cached texture files<para/>
		/// Список кешированных файлов
		/// </summary>
		public static ConcurrentDictionary<string, TextureFile> CachedFiles = new ConcurrentDictionary<string, TextureFile>();

		/// <summary>
		/// Texture dictionaries that needs to be built from texturefile<para/>
		/// Архивы, которые необходимо построить в фоновом потоке
		/// </summary>
		public static ConcurrentQueue<TextureDictionary> TextureProcessQueue = new ConcurrentQueue<TextureDictionary>();

		/// <summary>
		/// TextureFile that needs to be read in background<para/>
		/// Файлы текстур, которые необходимо прочитать в фоновом потоке
		/// </summary>
		public static ConcurrentQueue<TextureFile> TextureFileProcessQueue = new ConcurrentQueue<TextureFile>();

		/// <summary>
		/// Textures that needs to be sent to gpu<para/>
		/// Архивы текстур, которые необходимо отправить на видеокарту
		/// </summary>
		public static ConcurrentQueue<TextureDictionary> ReadyTextureQueue = new ConcurrentQueue<TextureDictionary>();

		/// <summary>
		/// Background ModelFile reader<para/>
		/// Фоновый читальщик файлов
		/// </summary>
		static Thread fileReaderThread;

		/// <summary>
		/// Background Model builder<para/>
		/// Фоновый постройщик моделей
		/// </summary>
		static Thread textureBuilderThread;


		/// <summary>
		/// Initialize predefined textures<para/>
		/// Инициализация предзаданных текстур
		/// </summary>
		public static void Init() {
			
			// Checking for extensions
			// Проверка расширений
			CompressionSupported = GLExtensions.Supported("GL_EXT_Texture_Compression_S3TC");
			FrameBufferSupported = GLExtensions.Supported("GL_ARB_Framebuffer_Object");

			// Load DAT-defined texture dictionaries
			// Загрузка предопределенных архивов текстур
			foreach (string TXD in FileManager.TextureFiles) {
				string name = Path.GetFileNameWithoutExtension(TXD).ToLower();
				string path = PathManager.GetAbsolute(TXD);
				TextureFile f = new TextureFile(path, true);
				CachedFiles.TryAdd(name, f);
				TextureDictionary td = new TextureDictionary(f, true, true);
				Cached.TryAdd(name, td);
			}

			// Starting TextureFile thread
			// Запуск потока чтения TextureFile
			fileReaderThread = new Thread(FileLoaderProcess);
			fileReaderThread.IsBackground = true;
			fileReaderThread.Priority = ThreadPriority.BelowNormal;
			fileReaderThread.Start();

			// Starting model builder thread
			// Запуск потока постройки Model
			textureBuilderThread = new Thread(TextureBuilderProcess);
			textureBuilderThread.IsBackground = true;
			textureBuilderThread.Priority = ThreadPriority.BelowNormal;
			textureBuilderThread.Start();
		}


		/// <summary>
		/// Send complete textures to GPU<para/>
		/// Отправка готовых архивов на GPU
		/// </summary>
		public static void SendComplete() {
			int SendQuota = 4;
			while (ReadyTextureQueue.Count > 0) {
				if (SendQuota == 0) {
					break;
				}
				TextureDictionary td = null;
				if (ReadyTextureQueue.TryDequeue(out td)) {
					td.SendTextures();
				}
				SendQuota--;
			}
		}

		/// <summary>
		/// Detect and remove unused textures<para/>
		/// Определить и удалить неиспользуемые текстурные архивы
		/// </summary>
		public static void CheckUnused() {
			Dictionary<string, TextureDictionary> texs = new Dictionary<string, TextureDictionary>(Cached);
			foreach (KeyValuePair<string, TextureDictionary> t in texs) {
				if (!t.Value.Important) {
					if (t.Value.UseCount == 0 && t.Value.State == TextureDictionary.ReadyState.Complete) {
						t.Value.Destroy();
						TextureDictionary temp;
						if (!Cached.TryRemove(t.Key, out temp)) {
							throw new Exception("[TextureManager] Unable to release texture - " + t.Key);
						}
					}
				}
			}
		}

		/// <summary>
		/// Check if currently background thread is processing this dictionary<para/>
		/// Проверка, обрабатывается ли указанный текстурный архив
		/// </summary>
		/// <param name="texture">Archive to check<para/>Архив для проверки</param>
		/// <returns>True if archive is processing<para/>True если архив обрабатывается</returns>
		public static bool IsProcessing(TextureDictionary texture) {
			return TextureProcessQueue.Contains(texture) || ReadyTextureQueue.Contains(texture);
		}

		/// <summary>
		/// TextureFile reading process<para/>
		/// Процесс чтения TextureFile
		/// </summary>
		static void FileLoaderProcess() {
			while (true) {
				if (TextureFileProcessQueue.Count > 0) {
					TextureFile tf = null;
					while (TextureFileProcessQueue.Count > 0) {
						if (TextureFileProcessQueue.TryDequeue(out tf)) {
							tf.ReadData();
						}
						Thread.Sleep(0);
					}
				} else {
					Thread.Sleep(10);
				}
			}
		}

		/// <summary>
		/// TextureDictionary building process<para/>
		/// Процесс построения TextureDictionary
		/// </summary>
		static void TextureBuilderProcess() {
			while (true) {
				if (TextureProcessQueue.Count > 0) {
					TextureDictionary td = null;
					while (TextureProcessQueue.Count > 0) {
						if (TextureProcessQueue.TryDequeue(out td)) {
							td.BuildTextures();
							ReadyTextureQueue.Enqueue(td);
						}
						Thread.Sleep(0);
					}
				} else {
					Thread.Sleep(10);
				}
			}
		}

	}
}
