using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenVice.Files {

	/// <summary>
	/// Base model file class<para/>
	/// Основной класс файла модели
	/// </summary>
	public class ModelFile : RenderWareFile {
		
		/// <summary>
		/// Create ModelFile from file<para/>
		/// Создание ModelFile из файла
		/// </summary>
		/// <param name="name">File name<para/>Имя файла</param>
		public ModelFile(string name, bool readNow = false) : base(name, readNow) {}

		/// <summary>
		/// Create ModelFile from byte stream<para/>
		/// Создание ModelFile из потока байтов
		/// </summary>
		/// <param name="data">File content<para/>Содержимое файла</param>
		/// <param name="readNow">Read file immediately instead of threaded reading<para/>Прочитать файл сейчас же, не полагаясь на потоковый загрузчик</param>
		public ModelFile(byte[] data, bool readNow = false) : base(data, readNow) {}

		/// <summary>
		/// List of frames<para/>
		/// Список кадров
		/// </summary>
		public Frame[] Frames;

		/// <summary>
		/// List of surfaces<para/>
		/// Список поверхностей
		/// </summary>
		public Geometry[] Surfaces;

		/// <summary>
		/// Read all the data from stream<para/>
		/// Чтение файла из потока
		/// </summary>
		/// <param name="stream">Existing stream<para/>Открытый поток</param>
		public override void ReadData() {

			// Protect from multiple reading
			// Защита от многоразового прочтения
			if (State != LoadState.None) {
				return;
			}
			State = LoadState.Reading;

			
			BinaryReader f = new BinaryReader(QueuedStream, Encoding.ASCII);
			ChunkHeader h;

			// Reading clump header
			// Чтение основного чанка
			h = ReadHeader(f);
			if (h.Type != ChunkType.Clump) {
				throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
			}

			// STRUCT
			h = ReadHeader(f);
			if (h.Type != ChunkType.Struct) {
				throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
			}

			// Number of atomically linked objects
			// Количество слинкованных объектов
			int numAtomics = f.ReadInt32();
			// Lights and cameras (unused)
			// Светильники и камеры
			int numLights = 0;
			int numCameras = 0;
			if (h.Size == 12) {
				numLights = f.ReadInt32();
				numCameras = f.ReadInt32();
			}

			// Framelist chunk
			// Чанк списка кадров
			h = ReadHeader(f);
			if (h.Type != ChunkType.FrameList) {
				throw new Exception("[ModelFile] Unexpected chunk: " + h.Type.ToString());
			}
			h = ReadHeader(f);
			if (h.Type != ChunkType.Struct) {
				throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
			}

			// Number of frames
			// Количество кадров
			int numFrames = f.ReadInt32();

			// Reading frames
			// Чтение кадров
			ReadFrames(f, numFrames);

			// Geometry list
			// Список геометрических данных
			h = ReadHeader(f);
			if (h.Type != ChunkType.GeometryList) {
				throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
			}
			h = ReadHeader(f);
			if (h.Type != ChunkType.Struct) {
				throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
			}

			// Number of geometries
			// Количество геометрических данных
			int numGeometry = f.ReadInt32();

			// Reading geometry
			// Чтение геометрии
			ReadGeometry(f, numGeometry);

			// Atomic list
			// Список ссылок
			ReadAtomics(f, numAtomics);


			// Close stream
			// Закрытие потока
			f.Close();
			State = LoadState.Complete;

		}

		/// <summary>
		/// Reading frame data<para/>
		/// Чтение кадров анимации
		/// </summary>
		/// <param name="f">Stream<para/>Поток</param>
		/// <param name="numFrames">Frame count<para/>Количество кадров</param>
		void ReadFrames(BinaryReader f, int numFrames) {
			Frames = new Frame[numFrames];
			
			// Reading struct part
			// Чтение структуры
			for (int i = 0; i < numFrames; i++) {
				Frame fr = new Frame();

				// Frame rotation
				// Поворот кадра
				fr.Rotation = new float[9];
				for (int r = 0; r < 9; r++) {
					fr.Rotation[r] = f.ReadSingle();
				}

				// Frame position
				// Позиция кадра
				fr.Position = new float[3];
				for (int p = 0; p < 3; p++) {
					fr.Position[p] = f.ReadSingle();
				}

				// Parent index
				// Индекс родителя
				fr.Parent = f.ReadInt32();

				// Skip data
				// Пропуск данных
				f.BaseStream.Position += 4;

				Frames[i] = fr;
			}

			// Reading extension
			// Чтение расширения
			for (int i = 0; i < numFrames; i++) {
				ChunkHeader h = ReadHeader(f);
				if (h.Type != ChunkType.Extension) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}
				if (h.Size > 0) {
					int endps = (int)(f.BaseStream.Position + h.Size);
					while (true) {
						if (f.BaseStream.Position >= endps) {
							break;
						}
						h = ReadHeader(f);
						switch (h.Type) {
							case ChunkType.Frame:
								Frames[i].Name = f.ReadVCString((int)h.Size);
								break;
							default:
								f.BaseStream.Position += h.Size;
								break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Read surface data<para/>
		/// Чтение поверхностей
		/// </summary>
		/// <param name="f">Stream<para/>Поток</param>
		/// <param name="numGeoms">Surface count<para/>Количество поверхностей</param>
		void ReadGeometry(BinaryReader f, int numGeoms) {
			Surfaces = new Geometry[numGeoms];
			for (int i = 0; i < numGeoms; i++) {
				Geometry g = new Geometry();

				// Header data
				// Заголовок
				ChunkHeader h = ReadHeader(f);
				if (h.Type != ChunkType.Geometry) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}
				h = ReadHeader(f);
				if (h.Type != ChunkType.Struct) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}

				// Surface flags
				// Флаги поверхности
				g.Flags = f.ReadUInt16();

				// Texture coordinates
				// Текстурные координаты
				int texCoords = f.ReadByte();
				if ((g.Flags & (int)GeometryFlags.TextureCoords) > 0) {
					texCoords = 1;
				}
				bool nativeGeom = f.ReadBoolean();

				int triCount = f.ReadInt32();
				int vertCount = f.ReadInt32();
				f.BaseStream.Position += 4;

				// Skip some vertex data
				// Пропуск некоторой информации
				if (h.Version<0x1003) {
					f.BaseStream.Position += 12;
				}

				if (!nativeGeom) {
					// Vertex colors
					// Цвета вершин
					if ((g.Flags & (int)GeometryFlags.Colors) > 0) {
						g.Colors = f.ReadBytes(vertCount * 4);
					}

					// First texcoord set
					// Первый набор текстурных координат
					if ((g.Flags & (int)GeometryFlags.TextureCoords) > 0) {
						g.TextureCoords = new float[vertCount * 2];
						for (int c = 0; c < g.TextureCoords.Length; c++) {
							g.TextureCoords[c] = f.ReadSingle();
						}
					}

					// Second coord set
					// Второй набор координат
					if ((g.Flags & (int)GeometryFlags.SecondTexCoords) > 0) {
						for (int cd = 0; cd < texCoords; cd++) {
							float[] coords = new float[vertCount * 2];
							for (int c = 0; c < coords.Length; c++) {
								coords[c] = f.ReadSingle();
							}
							if (cd == 0) {
								g.TextureCoords = coords;
							} else if (cd == 1) {
								g.SecondTextureCoords = coords;
							}
						}
					}

					// Indices
					// Вершинные индексы
					g.Indices = new ushort[triCount * 4];
					for (int c = 0; c < g.Indices.Length; c++) {
						g.Indices[c] = f.ReadUInt16();
					}
				}
				

				// Bounding sphere
				// Сфера для отсечения
				g.SpherePos = new float[3];
				for (int c = 0; c < 3; c++) {
					g.SpherePos[c] = f.ReadSingle();
				}
				g.SphereRadius = f.ReadSingle();

				// Skipping vertex flags
				// Пропускаем флаги вершин
				f.BaseStream.Position += 8;

				if (!nativeGeom) {
					// Reading vertex positions
					// Чтение позиций вершин
					g.Vertices = new float[vertCount * 3];
					for (int v = 0; v < g.Vertices.Length; v++) {
						g.Vertices[v] = f.ReadSingle();
					}

					// Reading normals
					// Чтение нормалей
					if ((g.Flags & (int)GeometryFlags.Normals) > 0) {
						g.Normals = new float[vertCount * 3];
						for (int vn = 0; vn < vertCount * 3; vn++) {
							g.Normals[vn] = f.ReadSingle();
						}
					}
				}

				// Reading materials
				// Чтение материалов
				h = ReadHeader(f);
				if (h.Type != ChunkType.MaterialList) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}
				h = ReadHeader(f);
				if (h.Type != ChunkType.Struct) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}

				// Material count
				// Количество материалов
				int materialNum = f.ReadInt32();
				f.BaseStream.Position += (materialNum * 4);

				// Reading materials
				// Чтение материалов
				ReadMaterials(g, f, materialNum);

				// Reading extension
				// Чтение расширения
				h = ReadHeader(f);
				if (h.Type != ChunkType.Extension) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}
				if (h.Size > 0) {
					int ops = (int)(f.BaseStream.Position + h.Size);
					while (true) {
						if (f.BaseStream.Position >= ops) {
							break;
						}
						h = ReadHeader(f);
						switch (h.Type) {

							// Binary mesh
							case ChunkType.BinMesh:
								int splitMode = f.ReadInt32();
								if (splitMode != 0 && splitMode != 1) {
									throw new Exception("[ModelFile] Unknown splitting mode: " + splitMode);
								}
								int numSplits = f.ReadInt32();
								f.BaseStream.Position += 4;
								g.Binary = new BinaryMesh[numSplits];
								bool hasData = h.Size > 12 + numSplits * 8;
								for (int sp = 0; sp < numSplits; sp++) {
									int numInds = f.ReadInt32();
									int matIndex = f.ReadInt32();
									BinaryMesh bn = new BinaryMesh();
									bn.BinaryMaterial = g.Materials[matIndex];
									bn.Mode = (SplitMode)splitMode;
									if (hasData) {
										bn.Indices = new ushort[numInds];
										for (int vr = 0; vr < numInds; vr++) {
											bn.Indices[vr] = (ushort)f.ReadUInt32();
										}
									}
									g.Binary[sp] = bn;
								}
								break;
							default:
								f.BaseStream.Position += h.Size;
								break;
						}
					}
				}

				Surfaces[i] = g;
			}
		}

		/// <summary>
		/// Reading material data<para/>
		/// Чтение материалов
		/// </summary>
		/// <param name="g">Surface<para/>Поверхность</param>
		/// <param name="f">BinaryReader</param>
		/// <param name="numMaterials">Materials count<para/>Количество материалов</param>
		void ReadMaterials(Geometry g, BinaryReader f, int numMaterials) {
			g.Materials = new Material[numMaterials];
			for (int mt = 0; mt < numMaterials; mt++) {
				// Material header
				ChunkHeader h = ReadHeader(f);
				if (h.Type != ChunkType.Material) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}
				h = ReadHeader(f);
				if (h.Type != ChunkType.Struct) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}

				
				Material m = new Material();
				
				// Reading material flags
				// Чтение флагов
				m.Flags = f.ReadInt32();

				// Material tint color
				// Цвет материала
				m.Color = f.ReadBytes(4);
				if (m.Color[3]<254f) {
					m.HasAlpha = true;
				}

				// Skip some data
				// Пропуск данных
				f.BaseStream.Position += 4;

				// Reading surface props
				// Чтение параметров поверхности
				int texHave = f.ReadInt32();
				m.Props = new float[3];
				for (int pg = 0; pg < 3; pg++) {
					m.Props[pg] = f.ReadSingle();
				}

				// Reading texture data
				// Чтение данных текстуры
				if (texHave > 0) {
					h = ReadHeader(f);
					int texEnd = (int)f.BaseStream.Position + (int)h.Size;
					if (h.Type != ChunkType.Texture) {
						throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
					}
					h = ReadHeader(f);
					if (h.Type != ChunkType.Struct) {
						throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
					}

					m.Textures = new Texture[1];
					Texture t = new Texture();
					m.Textures[0] = t;

					t.Filter = (TextureFile.FilterMode)f.ReadByte();
					byte wrapd = f.ReadByte();
					t.AddressU = (TextureFile.AddressMode)(wrapd & 0x0F);
					t.AddressV = (TextureFile.AddressMode)(wrapd >> 4);
					f.BaseStream.Position += 2;

					h = ReadHeader(f);
					if (h.Type != ChunkType.String) {
						throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
					}
					t.Name = f.ReadVCString((int)h.Size).ToLower();

					h = ReadHeader(f);
					if (h.Type != ChunkType.String) {
						throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
					}
					t.MaskName = f.ReadVCString((int)h.Size).ToLower();

					if (t.MaskName != "") {
						m.HasAlpha = true;
					}

					h = ReadHeader(f);
					if (h.Type != ChunkType.Extension) {
						throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
					}
					f.BaseStream.Position += h.Size;
				}

				h = ReadHeader(f);
				if (h.Type != ChunkType.Extension) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}
				if (h.Size > 0) {
					int ops = (int)(f.BaseStream.Position + h.Size);
					while (true) {
						if (f.BaseStream.Position >= ops) {
							break;
						}
						h = ReadHeader(f);
						switch (h.Type) {

							default:
								f.BaseStream.Position += h.Size;
								break;
						}
					}
				}

				g.Materials[mt] = m;
			}
		}

		/// <summary>
		/// Reading surface links to frames<para/>
		/// Чтение ссылок поверхностей на фреймы
		/// </summary>
		/// <param name="f">BinaryReader</param>
		/// <param name="numAtomics">Count of atomics<para/>Количество ссылок</param>
		void ReadAtomics(BinaryReader f, int numAtomics) {
			for (int i = 0; i < numAtomics; i++) {
				ChunkHeader h = ReadHeader(f);
				if (h.Type != ChunkType.Atomic) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}
				h = ReadHeader(f);
				if (h.Type != ChunkType.Struct) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}

				// Read and skip some data
				// Чтение и пропуск некоторой информации
				int frameIndex = f.ReadInt32();
				int geomIndex = f.ReadInt32();
				f.BaseStream.Position += 8;

				// Prevent multiple assignment
				// Обход многоразового присвоения
				if (Surfaces[geomIndex].Frame != -1) {
					throw new Exception("[ModelFile] Geometry already assigned to frame: " + geomIndex);
				}
				Surfaces[geomIndex].Frame = frameIndex;

				// Extension part
				// Расширения
				h = ReadHeader(f);
				if (h.Type != ChunkType.Extension) {
					throw new Exception("[ModelFile] Unexpected chunk: " + h.Type);
				}
				f.BaseStream.Position += h.Size;
			}
		}

		/// <summary>
		/// Single hierarchy frame<para/>
		/// Одна часть иерархии
		/// </summary>
		public class Frame {
			/// <summary>
			/// Frame name<para/>
			/// Имя части
			/// </summary>
			public string Name;

			/// <summary>
			/// Rotation matrix<para/>
			/// Матрица поворота
			/// </summary>
			public float[] Rotation;

			/// <summary>
			/// Position vector<para/>
			/// Вектор расположения
			/// </summary>
			public float[] Position;

			/// <summary>
			/// Parent index<para/>
			/// Индекс родителя
			/// </summary>
			public int Parent = -1;
		}

		/// <summary>
		/// Single geometry entry<para/>
		/// Геометрические данные
		/// </summary>
		public class Geometry {
			/// <summary>
			/// Geometry flags<para/>
			/// Бинарные флаги
			/// </summary>
			public int Flags;

			/// <summary>
			/// Culling sphere position<para/>
			/// Позиция сферы отсечения
			/// </summary>
			public float[] SpherePos;

			/// <summary>
			/// Culling sphere radius<para/>
			/// Радиус сферы отсечения
			/// </summary>
			public float SphereRadius;

			/// <summary>
			/// Binary mesh data (world meshes)<para/>
			/// Данные Binary mesh (мировые меши)
			/// </summary>
			public BinaryMesh[] Binary;

			/// <summary>
			/// Model materials<para/>
			/// Материалы модели
			/// </summary>
			public Material[] Materials;

			/// <summary>
			/// Vertex colors<para/>
			/// Цвета вершин
			/// </summary>
			public byte[] Colors;
 
			/// <summary>
			/// Vertex texture coords<para/>
			/// Текстурные координаты вершины
			/// </summary>
			public float[] TextureCoords;

			/// <summary>
			/// Vertex second texture coords<para/>
			/// Вторые текстурные координаты вершины
			/// </summary>
			public float[] SecondTextureCoords;

			/// <summary>
			/// Parent frame for surface<para/>
			/// Родительский фрейм
			/// </summary>
			public int Frame = -1;

			/// <summary>
			/// Vertex indices<para/>
			/// Индексы вершин
			/// </summary>
			public ushort[] Indices;

			/// <summary>
			/// Vertex positions<para/>
			/// Координаты вершин
			/// </summary>
			public float[] Vertices;

			/// <summary>
			/// Vertex normals<para/>
			/// Нормали вершин
			/// </summary>
			public float[] Normals;

		}

		/// <summary>
		/// Binary surface
		/// </summary>
		public class BinaryMesh {
			/// <summary>
			/// Vertex indices<para/>
			/// Индексы вершин
			/// </summary>
			public ushort[] Indices;

			/// <summary>
			/// BinMesh material
			/// </summary>
			public Material BinaryMaterial;

			/// <summary>
			/// BinMesh splitting type<para/>
			/// Тип триангуляции меша
			/// </summary>
			public SplitMode Mode;

		}

		/// <summary>
		/// Surface material<para/>
		/// Материал поверхности
		/// </summary>
		public class Material {
			/// <summary>
			/// Surface flags<para/>
			/// Бинарные флаги поверхности
			/// </summary>
			public int Flags;

			/// <summary>
			/// Surface tint color<para/>
			/// Цвет поверхности
			/// </summary>
			public byte[] Color;

			/// <summary>
			/// Surface color parameters<para/>
			/// Дополнительные цвета поверхности
			/// </summary>
			public float[] Props;

			/// <summary>
			/// Surface textures<para/>
			/// Текстуры поверхности
			/// </summary>
			public Texture[] Textures;

			/// <summary>
			/// Flag that surface is alphablended<para/>
			/// Флаг что поверхность прозрачная
			/// </summary>
			public bool HasAlpha;
		}

		/// <summary>
		/// Surface texture class<para/>
		/// Класс текстуры поверхности
		/// </summary>
		public class Texture {

			/// <summary>
			/// Texture name<para/>
			/// Имя текстуры
			/// </summary>
			public string Name;

			/// <summary>
			/// Texture mask name<para/>
			/// Имя маски текстуры
			/// </summary>
			public string MaskName;

			/// <summary>
			/// Texture filtering mode<para/>
			/// Режим фильтрации текстуры
			/// </summary>
			public TextureFile.FilterMode Filter;

			/// <summary>
			/// Texture addressing in U<para/>
			/// Горизонтальная адресация текстуры
			/// </summary>
			public TextureFile.AddressMode AddressU;

			/// <summary>
			/// Texture addressing in V<para/>
			/// Вертикальная адресация текстуры
			/// </summary>
			public TextureFile.AddressMode AddressV;
			
		}

		/// <summary>
		/// Surface flags<para/>
		/// Флаги поверхности
		/// </summary>
		public enum GeometryFlags : int {
			TriangleStrip		= 0x01,
			Positions			= 0x02,
			TextureCoords		= 0x04,
			Colors				= 0x08,
			Normals				= 0x10,
			Light				= 0x20,
			ModulatedColor		= 0x40,
			SecondTexCoords		= 0x80
		}

		/// <summary>
		/// BinMesh splitting mode<para/>
		/// Тип разбиения BinMesh
		/// </summary>
		public enum SplitMode : int {
			TriangleList	= 0,
			TriangleStrip	= 1
		}
	}
}
