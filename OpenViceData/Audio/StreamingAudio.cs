using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MP3Sharp;
using OpenTK.Audio.OpenAL;
using System.Collections.Concurrent;
using OpenVice.Files;

namespace OpenVice.Audio {

	/// <summary>
	/// Streamable audio stream<para/>
	/// Подгружаемый аудиострим
	/// </summary>
	public class StreamingAudio {

		/// <summary>
		/// Number of looping buffers<para/>
		/// Число буфферов обработки звука
		/// </summary>
		const int LoopBuffers = 16;

		/// <summary>
		/// Single streamer chunk size<para/>
		/// Размер чанка для стриминга
		/// </summary>
		const int ChunkSize = 65536;

		/// <summary>
		/// Allow file looping<para/>
		/// Зацикливание файла
		/// </summary>
		public bool Loop { get; set; }

		/// <summary>
		/// Is stream playing<para/>
		/// Проигрывается ли стрим
		/// </summary>
		public bool Playing { get; private set; }

		/// <summary>
		/// Is stream paused<para/>
		/// Стрим на паузе
		/// </summary>
		public bool Paused { get; private set; }

		/// <summary>
		/// Stream position<para/>
		/// Позиция потока
		/// </summary>
		public long Position {
			get {
				return stream.Position;
			}
			set {
				stream.Position = value;
				buffersReady = false;
				if (Playing) {
					RebuildBuffers();
					if (!Paused) {
						AL.SourcePlay(source);
					}
				}
			}
		}

		/// <summary>
		/// Streaming buffers<para/>
		/// Буфферы подгрузки
		/// </summary>
		int[] buffers;

		/// <summary>
		/// Streamed raw data<para/>
		/// Подгруженная голая дата
		/// </summary>
		ConcurrentQueue<byte[]> readyData;

		/// <summary>
		/// Current working buffer<para/>
		/// Текущий буффер
		/// </summary>
		int currentBuffer = 0;

		/// <summary>
		/// Number of needed streaming buffers<para/>
		/// Число нужных буфферов
		/// </summary>
		int buffersNeeded = 0;

		/// <summary>
		/// All the buffers filled<para/>
		/// Заполнены все буфферы
		/// </summary>
		bool buffersReady = false;

		/// <summary>
		/// Internal AL source<para/>
		/// Внутренний источник
		/// </summary>
		int source;
		
		/// <summary>
		/// Binary source for audio data<para/>
		/// Стрим аудиоданных
		/// </summary>
		MP3Stream stream;

		public StreamingAudio(string file, bool isADF = false) {

			// Creating AL data
			// Создание AL данных
			buffers = AL.GenBuffers(LoopBuffers);
			source = AL.GenSource();
			AL.Source(source, ALSourcef.Pitch, 1f);
			AL.Source(source, ALSourcef.Gain, 0.5f);
			AL.Source(source, ALSourceb.Looping, false);
			AL.Source(source, ALSourceb.SourceRelative, true);

			readyData = new ConcurrentQueue<byte[]>();
			Stream s = new FileStream(file, FileMode.Open, FileAccess.Read);
			if (isADF) {
				s = new AdfStream(s);
			}
			stream = new MP3Stream(s);

			AudioManager.Streaming.Add(this);
		}

		/// <summary>
		/// Resume or start playing<para/>
		/// Запуск проигрывания
		/// </summary>
		public void Play() {
			if (!buffersReady) {
				RebuildBuffers();
			}
			Playing = true;
			Paused = false;
			AL.SourcePlay(source);
		}

		/// <summary>
		/// Pause stream<para/>
		/// Поставить на паузу
		/// </summary>
		public void Pause() {
			Paused = true;
			AL.SourcePause(source);
		}

		/// <summary>
		/// Stop stream<para/>
		/// Остановить поток
		/// </summary>
		public void Stop() {
			Paused = false;
			Playing = false;
			stream.Position = 0;
			buffersReady = false;
			AL.SourceStop(source);
		}

		/// <summary>
		/// Create buffers from scratch<para/>
		/// Реинициализация буфферов
		/// </summary>
		void RebuildBuffers() {
			buffersNeeded = 0;
			readyData = new ConcurrentQueue<byte[]>();
			
			AL.SourceStop(source);
			int bc = 0;
			AL.GetSource(source, ALGetSourcei.BuffersQueued, out bc);
			for (int i = 0; i < bc; i++) {
				AL.SourceUnqueueBuffer(source);
			}
			// Preparing starting buffers
			// Подготовка начальных буфферов
			for (int i = 0; i < LoopBuffers; i++) {
				QueueData();
				byte[] data;
				if (readyData.TryDequeue(out data)) {
					AL.BufferData(buffers[i], ALFormat.Stereo16, data, data.Length, stream.Frequency);
					AL.SourceQueueBuffer(source, buffers[i]);
				}
			}
			buffersReady = true;
		}

		/// <summary>
		/// Read single chunk from file<para/>
		/// Чтение одного чанка из файла
		/// </summary>
		void QueueData() {
			byte[] data = new byte[ChunkSize];
			int bytesRead = stream.Read(data, 0, ChunkSize);
			if (bytesRead==0) {
				if (Loop) {
					stream.Position = 0;
					stream.Read(data, 0, ChunkSize);
				}else{
					Playing = false;
				}
			}
			if (bytesRead>0) {
				readyData.Enqueue(data);
			}
		}

		/// <summary>
		/// Background update for buffers<para/>
		/// Внутреннее обновление буфферов
		/// </summary>
		public void ThreadedUpdate() {
			while (buffersNeeded>0) {
				QueueData();
				if (!Playing) {
					buffersNeeded = 0;
					break;
				}
				buffersNeeded--;
			}
		}

		/// <summary>
		/// Updating buffers in main thread<para/>
		/// Обновление буфферов в основном потоке
		/// </summary>
		public void BufferUpdate() {
			if (Playing) {
				int c = 0;
				AL.GetSource(source, ALGetSourcei.BuffersProcessed, out c);
				buffersNeeded += c;
				while (c>0) {
					if (readyData.Count>0) {
						byte[] data;
						if (readyData.TryDequeue(out data)) {
							AL.SourceUnqueueBuffer(source);
							AL.BufferData(buffers[currentBuffer], ALFormat.Stereo16, data, data.Length, stream.Frequency);
							AL.SourceQueueBuffer(source, buffers[currentBuffer]);
							currentBuffer = (currentBuffer + 1) % LoopBuffers;
						} else {
							break;
						}
					} else {
						break;
					}
					c--;
				}
			}
		}

	}
}
