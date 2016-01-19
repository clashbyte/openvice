using System;
using System.IO;

namespace OpenVice.Files {

	/// <summary>
	/// RenderWare chunked file<para/>
	/// Чанковый файл RenderWare
	/// </summary>
	public abstract class RenderWareFile {

		/// <summary>
		/// RW chunk version for VC<para/>
		/// Версия чанка для файлов VC
		/// </summary>
		internal const uint ViceCityChunkVersion = 0x0C02FFFF;

		/// <summary>
		/// Stored stream for threaded loading
		/// </summary>
		protected Stream QueuedStream; 

		/// <summary>
		/// Internal file reading state<para/>
		/// Внутреннее состояние чтения файла
		/// </summary>
		public LoadState State { get; protected set; }

		/// <summary>
		/// Create RenderWareFile from file<para/>
		/// Создание RenderWareFile из файла
		/// </summary>
		/// <param name="name">File name<para/>Имя файла</param>
		/// <param name="readNow">Read file immediately instead of threaded reading<para/>Прочитать файл сейчас же, не полагаясь на потоковый загрузчик</param>
		public RenderWareFile(string name, bool readNow = false) {
			if (!File.Exists(name)) {
				throw new FileNotFoundException("[RenderWareFile] File not found: "+Path.GetFileName(name), name);
			}
			QueuedStream = new FileStream(name, FileMode.Open, FileAccess.Read);
			State = LoadState.None;
			if (readNow) {
				ReadData();
			}
		}

		/// <summary>
		/// Create RenderWareFile from byte data<para/>
		/// Создание RenderWareFile из массива байт
		/// </summary>
		/// <param name="data">File content<para/>Содержимое файла</param>
		/// <param name="readNow">Read file immediately instead of threaded reading<para/>Прочитать файл сейчас же, не полагаясь на потоковый загрузчик</param>
		public RenderWareFile(byte[] data, bool readNow = false) {
			QueuedStream = new MemoryStream(data, false);
			State = LoadState.None;
			if (readNow) {
				ReadData();
			}
		}

		/// <summary>
		/// Read all the data from stream<para/>
		/// Чтение файла из потока
		/// </summary>
		public abstract void ReadData();

		/// <summary>
		/// Internal structure for RW chunk<para/>
		/// Внутренняя структура для чанка RW
		/// </summary>
		protected struct ChunkHeader {
			/// <summary>
			/// Chunk identifier<para/>
			/// Идентификатор чанка
			/// </summary>
			public ChunkType Type;

			/// <summary>
			/// Chunk size without header<para/>
			/// Размер чанка без заголовка
			/// </summary>
			public uint Size;

			/// <summary>
			/// Building toolkit version<para/>
			/// Версия сборщика
			/// </summary>
			public uint Toolkit;

			/// <summary>
			/// Chunk version<para/>
			/// Версия чанка
			/// </summary>
			public uint Version;
		}

		/// <summary>
		/// Read RW header<para/>
		/// Чтение заголовка
		/// </summary>
		/// <param name="f">Existing BinaryReader<para/>Открытый BinaryReader</param>
		/// <returns></returns>
		protected ChunkHeader ReadHeader(BinaryReader f) {
			ChunkHeader h = new ChunkHeader();
			h.Type = (ChunkType)f.ReadUInt32();
			h.Size = f.ReadUInt32();
			h.Toolkit = f.ReadUInt16();
			h.Version = f.ReadUInt16();
			System.Threading.Thread.Sleep(0);
			return h;
		}

		/// <summary>
		/// Internal RW chunk type<para/>
		/// Внутренний тип чанка
		/// </summary>
		protected enum ChunkType : uint {
			NAObject			= 0x0,
			Struct				= 0x1,
			String				= 0x2,
			Extension			= 0x3,
			Camera				= 0x5,
			Texture				= 0x6,
			Material			= 0x7,
			MaterialList		= 0x8,
			AtomicSect			= 0x9,
			PlaneSect			= 0xA,
			World				= 0xB,
			Spline				= 0xC,
			Matrix				= 0xD,
			FrameList			= 0xE,
			Geometry			= 0xF,
			Clump				= 0x10,
			Light				= 0x12,
			UnicodeString		= 0x13,
			Atomic				= 0x14,
			TextureNative		= 0x15,
			TexDictionary		= 0x16,
			AnimDataBase		= 0x17,
			Image				= 0x18,
			SkinAnimation		= 0x19,
			GeometryList		= 0x1A,
			AnimAnimation		= 0x1B,
			HAnimAnimation		= 0x1B,
			Team				= 0x1C,
			Crowd				= 0x1D,
			RightToRender		= 0x1F,
			MTEffectNative		= 0x20,
			MTEffectDict		= 0x21,
			TeamDictionary		= 0x22,
			PITexDictionary		= 0x23,
			TOC					= 0x24,
			PRTSTDGlobalData 	= 0x25,
			AltPipe				= 0x26,
			PIPeds				= 0x27,
			PatchMesh			= 0x28,
			ChunkGroupStart		= 0x29,
			ChunkGroupEnd		= 0x2A,
			UVAnimDict			= 0x2B,
			CollTree			= 0x2C,
			Environment			= 0x2D,
			CorePluginIDMax 	= 0x2E,

			Morph				= 0x105,
			SkyMipmap			= 0x110,
			Skin				= 0x116,
			Particles			= 0x118,
			HAnim				= 0x11E,
			MaterialEffects 	= 0x120,
			PDSPLG				= 0x131,
			ADCPLG				= 0x134,
			UVAnimPLG			= 0x135,
			BinMesh				= 0x50E,
			NativeData			= 0x510,
			VertexFormat		= 0x510,

			PipelineSet			= 0x253F2F3,
			SpecularMat		 	= 0x253F2F6,
			FX2D			 	= 0x253F2F8,
			NightVertexColor 	= 0x253F2F9,
			CollisionModel	 	= 0x253F2FA,
			ReflectionMat		= 0x253F2FC,
			MeshExtension	 	= 0x253F2FD,
			Frame			 	= 0x253F2FE
		}

		/// <summary>
		/// Background loading state flags<para/>
		/// Ассоциированые флаги фоновой загрузки
		/// </summary>
		public enum LoadState {
			None,
			Reading,
			Complete
		}

	}
}
