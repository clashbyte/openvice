using OpenVice.Files;
using OpenVice.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace OpenVice.Managers {

	/// <summary>
	/// Base mesh manager<para/>
	/// Менеджер всех моделей
	/// </summary>
	public static class ModelManager {
		/// <summary>
		/// Cached models list<para/>
		/// Список кешированных моделей
		/// </summary>
		public static ConcurrentDictionary<string, Model> Cached = new ConcurrentDictionary<string, Model>();

		/// <summary>
		/// List of cached model files<para/>
		/// Список кешированных файлов
		/// </summary>
		public static ConcurrentDictionary<string, ModelFile> CachedFiles = new ConcurrentDictionary<string, ModelFile>();

		/// <summary>
		/// Models that needs to be built from modelfile<para/>
		/// Модели, которые необходимо построить в фоновом потоке
		/// </summary>
		public static ConcurrentQueue<Model> ModelProcessQueue = new ConcurrentQueue<Model>();

		/// <summary>
		/// Modelfile that needs to be read in background<para/>
		/// Файлы моделей, которые необходимо прочитать в фоновом потоке
		/// </summary>
		public static ConcurrentQueue<ModelFile> ModelFileProcessQueue = new ConcurrentQueue<ModelFile>();

		/// <summary>
		/// Models that needs to be sent to gpu<para/>
		/// Модели, которые необходимо отправить на видеокарту
		/// </summary>
		public static ConcurrentQueue<Model> ReadyModelQueue = new ConcurrentQueue<Model>();

		/// <summary>
		/// Background ModelFile reader<para/>
		/// Фоновый читальщик файлов
		/// </summary>
		static Thread fileReaderThread;

		/// <summary>
		/// Background Model builder<para/>
		/// Фоновый постройщик моделей
		/// </summary>
		static Thread modelBuilderThread;

		/// <summary>
		/// Initialize predefined models<para/>
		/// Инициализация предзаданных моделей
		/// </summary>
		public static void Init() {

			// Load DAT-defined texture dictionaries
			// Загрузка предопределенных архивов текстур
			foreach (string DFF in FileManager.ModelFiles) {
				string name = Path.GetFileNameWithoutExtension(DFF).ToLower();
				string path = PathManager.GetAbsolute(DFF);
				ModelFile f = new ModelFile(path, true);
				CachedFiles.TryAdd(name, f);
				Model m = new Model(f, true, true);
				Cached.TryAdd(name, m);
			}

			// Starting ModelFile thread
			// Запуск потока чтения ModelFile
			fileReaderThread = new Thread(FileLoaderProcess);
			fileReaderThread.IsBackground = true;
			fileReaderThread.Priority = ThreadPriority.BelowNormal;
			fileReaderThread.Start();

			// Starting model builder thread
			// Запуск потока постройки Model
			modelBuilderThread = new Thread(ModelBuilderProcess);
			modelBuilderThread.IsBackground = true;
			modelBuilderThread.Priority = ThreadPriority.BelowNormal;
			modelBuilderThread.Start();

		}

		/// <summary>
		/// Send complete models to GPU<para/>
		/// Отправка готовых моделей на GPU
		/// </summary>
		public static void SendComplete() {
			int SendQuota = 8;
			while (ReadyModelQueue.Count>0) {
				if (SendQuota == 0) {
					break;
				}
				Model md = null;
				if (ReadyModelQueue.TryDequeue(out md)) {
					md.SendSubMeshes();
				}
				SendQuota--;
			}
		}

		/// <summary>
		/// Detect and remove unused meshes<para/>
		/// Определить и удалить неиспользуемые меши
		/// </summary>
		public static void CheckUnused() {
			Dictionary<string, Model> models = new Dictionary<string, Model>(Cached);
			foreach (KeyValuePair<string, Model> m in models) {
				if (!m.Value.Important) {
					if (m.Value.UseCount == 0 && m.Value.State == Model.ReadyState.Complete) {
						m.Value.Destroy();
						Model temp;
						if (!Cached.TryRemove(m.Key, out temp)) {
							throw new Exception("[ModelManager] Unable to release model - " + m.Key);
						}
					}
				}
			}
		}

		/// <summary>
		/// Check if currently background thread is processing this model<para/>
		/// Проверка, обрабатывается ли указанная модель
		/// </summary>
		/// <param name="model">Model to check<para/>Модель для проверки</param>
		/// <returns>True if model is processing<para/>True если модель обрабатывается</returns>
		public static bool IsProcessing(Model model) {
			return ModelProcessQueue.Contains(model) || ReadyModelQueue.Contains(model);
		}

		/// <summary>
		/// ModelFile reading process<para/>
		/// Процесс чтения ModelFile
		/// </summary>
		static void FileLoaderProcess() {
			while (true) {
				if (ModelFileProcessQueue.Count > 0) {
					ModelFile mf = null;
					while (ModelFileProcessQueue.Count > 0) {
						if (ModelFileProcessQueue.TryDequeue(out mf)) {
							mf.ReadData();
						}
						Thread.Sleep(0);
					}
				} else {
					Thread.Sleep(10);
				}
			}
		}

		/// <summary>
		/// Model building process<para/>
		/// Процесс построения Model
		/// </summary>
		static void ModelBuilderProcess() {
			while (true) {
				if (ModelProcessQueue.Count > 0) {
					Model md = null;
					while (ModelProcessQueue.Count > 0) {
						if (ModelProcessQueue.TryDequeue(out md)) {
							md.BuildSubMeshes();
							ReadyModelQueue.Enqueue(md);
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
