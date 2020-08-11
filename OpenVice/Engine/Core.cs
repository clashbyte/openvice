using OpenVice.Graphics;
using OpenVice.Managers;
using OpenVice.Audio;
using OpenVice.Controls;
using OpenVice.World;

namespace OpenVice.Engine {

	/// <summary>
	/// Engine core<para/>
	/// Ядро движка
	/// </summary>
	public static class Core {

		/// <summary>
		/// Initialize engine<para/>
		/// Инициализация движка
		/// </summary>
		public static void Init() {

			Dev.Console.Log("[Core] Engine started");

			// Initialize audio system
			// Инициализация аудио
			AudioManager.Init();

			// Initialize physics engine
			// Инициализация физического движка
			PhysicsManager.Init();

			// Setting game path
			// Установка пути игры
			PathManager.GamePath = "D:/Games/Vice City/";

			// Reading GTA_VC.DAT
			// Чтение основных данных
			FileManager.InitData();

			// Mount all archives
			// Подключение архивов
			ArchiveManager.Mount();

			// Initialize texture and model managers
			// Инициализация менеджеров моделей и текстур
			TextureManager.Init();
			ModelManager.Init();

			// Starting city loading
			// Запуск загрузки города
			CityManager.Init();

			// Update debug stuff
			// Обновление дебага
			Dev.DebugStuff.Init();
			 
		}

		/// <summary>
		/// Update frame logic<para/>
		/// Обновление игровой логики
		/// </summary>
		/// <param name="delta">Tween</param>
		public static void Update(float delta) {
			
			// Updating controls
			// Обновление управления
			Input.Update();

			// Environment update
			// Обновление окружения
			Environment.Update(delta);

			// Update city if it's loaded
			// Обработка города если он загружен
			if (CityManager.State == CityManager.CityState.Complete) {
				StaticManager.Update(delta);
			}

			Controls.FreeLook.Control();

			// Update physics
			// Обновление физики
			PhysicsManager.Update(delta);

			// Updating audio engine
			// Обновление аудиодвижка
			AudioManager.Update();

			// Update debug stuff
			// Обновление дебага
			Dev.DebugStuff.Update();
		}

		/// <summary>
		/// Render frame<para/>
		/// Отрисовка одного кадра
		/// </summary>
		/// <param name="delta">Tween</param>
		public static void Render(float delta) {

			// Render city if it's loaded
			// Подготовка города если он загружен
			if (CityManager.State == CityManager.CityState.Complete) {
				StaticManager.Render(delta);
			}

			// Update debug stuff
			// Обновление дебага
			Dev.DebugStuff.Render();

			// Rendering single frame
			// Отрисовка одного кадра
			Renderer.RenderFrame();
		}


	}
}
