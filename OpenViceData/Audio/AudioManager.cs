using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenVice.Managers;
using System.Threading;

namespace OpenVice.Audio {
	
	/// <summary>
	/// Manager for all audio work<para/>
	/// Менеджер для аудиоданных
	/// </summary>
	public static class AudioManager {

		/// <summary>
		/// Current audio context<para/>
		/// Текущий аудиоконтекст
		/// </summary>
		public static AudioContext Context { get; private set; }

		/// <summary>
		/// All streaming buffers<para/>
		/// Все фоновые стримы
		/// </summary>
		public static List<StreamingAudio> Streaming { get; private set; }

		/// <summary>
		/// Background thread for stream processing<para/>
		/// Фоновый поток для обработки стриминга
		/// </summary>
		static Thread thread;

		/// <summary>
		/// Audio subsystem initialization<para/>
		/// Инициализация аудио подсистемы
		/// </summary>
		public static void Init() {

			// Creating context
			// Создание контекста
			Context = new AudioContext();
			Context.MakeCurrent();

			// Creating streaming sources list
			// Создание списка стримящихся источников
			Streaming = new List<StreamingAudio>();

			thread = new Thread(ThreadedUpdate);
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.BelowNormal;
			//thread.Start();

			Dev.Console.Log("[AudioManager] Audio initialized");
		}

		/// <summary>
		/// Update audio system<para/>
		/// Обработка аудиосистемы
		/// </summary>
		public static void Update() {
			// Updating streaming
			// Обработка стриминга
			List<StreamingAudio> audios = new List<StreamingAudio>(Streaming);
			foreach (StreamingAudio a in audios) {
				a.BufferUpdate();
			}
		}

		/// <summary>
		/// Background threaded work<para/>
		/// Фоновая потоковая работа
		/// </summary>
		static void ThreadedUpdate() {
			while (true) {
				// Updating streaming
				// Обработка стриминга
				List<StreamingAudio> audios = new List<StreamingAudio>(Streaming);
				foreach (StreamingAudio a in audios) {
					a.ThreadedUpdate();
					Thread.Sleep(5);
				}
			}
		}

	
	}
}
