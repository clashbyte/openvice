using OpenVice.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OpenVice.Managers {

	/// <summary>
	/// Class for overall city loading and parsing in background<para/>
	/// Класс для загрузки города в фоновом потоке
	/// </summary>
	public static class CityManager {

		/// <summary>
		/// Value that indicates current loading state progress<para/>
		/// Текущее состояние загрузки данных
		/// </summary>
		public static float LoadingProgress { get; private set; }

		/// <summary>
		/// Current loading state<para/>
		/// Текущее состояние загрузки
		/// </summary>
		public static CityState State { get; private set; }

		public static ItemPlacement.Interior Interior = ItemPlacement.Interior.World;

		/// <summary>
		/// Background loading thread<para/>
		/// Фоновый поток загрузки города
		/// </summary>
		static Thread thread;

		/// <summary>
		/// Load city from confings and etc.<para/>
		/// Загрузка города
		/// </summary>
		public static void Init() {

			// Checking for second call
			// Проверка на второй вызов
			if (CityManager.State != CityState.Empty) {
				throw new Exception("[CityManager] City is already cached!");
			}

			// Creating background thread
			// Запуск фонового потока
			thread = new Thread(ThreadedLoading);
			thread.Priority = ThreadPriority.Lowest;
			thread.IsBackground = true;
			thread.Start();
		}

		/// <summary>
		/// Background operations<para/>
		/// Фоновые операции
		/// </summary>
		static void ThreadedLoading() {

			// Reading DAT files
			// Чтение настроек
			State = CityState.ReadingConfigs;
			TimeCycleManager.Init();

			// Reading definitions and placements
			// Чтение IDE и IPL
			State = CityState.ReadingDefinitions;
			ObjectManager.ReadDefinitions();
			State = CityState.ReadingPlacements;
			ObjectManager.ReadPlacements();

			// Reading collision files
			// Чтение файлов коллизии
			State = CityState.ReadingCollisions;
			// TODODODODDODDODDOODOD

			// TODODODODOODODDOODDODO

			State = CityState.CreatingPlacements;
			StaticManager.Init();

			// Loading completed
			// Загрузка города завершена
			State = CityState.Complete;
		}


		/// <summary>
		/// State of city parsing<para/>
		/// Состояние разбора данных о городе
		/// </summary>
		public enum CityState : int {
			Empty				= 0,
			ReadingConfigs		= 1,
			ReadingDefinitions	= 2,
			ReadingPlacements	= 3,
			ReadingCollisions	= 4,
			CreatingPlacements	= 5,
			Complete			= 6
		}

	}
}
