using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;

namespace OpenVice.Files {
	
	/// <summary>
	/// Class that handles COLL files reading<para/>
	/// Класс, который читает файлы физданных
	/// </summary>
	public class CollisionFile {

		/// <summary>
		/// Number of Primitive.Material indices allowed<para/>
		/// Количество индексов материалов
		/// </summary>
		const int SurfaceMaterialsCount = 64;

		/// <summary>
		/// Dictionary that holds all loaded collision meshes<para/>
		/// Словарь, хранящий все модели столкновений
		/// </summary>
		public List<Group> Collisions;

		/// <summary>
		/// Read collision file from file<para/>
		/// Чтение файлов коллизий из файла
		/// </summary>
		/// <param name="filename">File name<para/>Имя файла</param>
		public CollisionFile(string filename) {
			if (!File.Exists(filename)) {
				throw new FileNotFoundException("[CollisionFile] File not found: "+Path.GetFileName(filename), filename);
			}

			// Opening reader
			// Открытие ридера
			Collisions = new List<Group>();
			BinaryReader f = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read), Encoding.ASCII);

			// Read all the models
			// Чтение всех моделей
			while (f.BaseStream.Position<f.BaseStream.Length-1) {

				// Reading header
				// Чтение заголовка
				string header = new string(f.ReadChars(4));
				if (header != "COLL") {
					throw new Exception("[CollisionFile] Unknown collision format: "+header);
				}
				f.BaseStream.Position += 4;

				// Creating new group
				// Создание новой группы
				Group c = new Group();
				
				// Reading collision head
				// Чтение оглавления файла
				c.Name = f.ReadVCString(22).ToLower();
				c.ID = f.ReadUInt16();

				// Reading bounds
				// Чтение габаритов
				c.BoundsRadius = f.ReadSingle();
				c.BoundsCenter = f.ReadVCVector();
				c.BoundsMin = f.ReadVCVector();
				c.BoundsMax = f.ReadVCVector();

				// Reading objects
				// Чтение объектов
				int numSpheres = f.ReadInt32();
				if (numSpheres>0) {
					// Spheres
					// Сферы
					c.Spheres = new Sphere[numSpheres];
					for (int i = 0; i < numSpheres; i++) {
						Sphere s = new Sphere();
						s.Radius = f.ReadSingle();
						s.Center = f.ReadVCVector();
						s.ReadParams(f);
						c.Spheres[i] = s;
					}
				}

				// Skip unknown index
				// Пропуск неизвестного числа
				f.BaseStream.Position += 4;

				int numBoxes = f.ReadInt32();
				if (numBoxes>0) {
					// Boxes
					// Боксы
					c.Boxes = new Box[numBoxes];
					for (int i = 0; i < numBoxes; i++) {
						Box b = new Box();
						b.Min = f.ReadVCVector();
						b.Max = f.ReadVCVector();
						b.ReadParams(f);
						c.Boxes[i] = b;
					}
				}

				int numVerts = f.ReadInt32();
				if (numVerts>0) {
					// Vertices and triangles
					// Вершины и треугольники
					c.Vertices = new Vector3[numVerts];
					for (int i = 0; i < numVerts; i++) {
						c.Vertices[i] = f.ReadVCVector();
					}

					// Reading trimeshes
					// Чтение тримешей
					int indexCount = f.ReadInt32();
					List<int>[] indices = new List<int>[SurfaceMaterialsCount];
					int meshCount = 0;
					for (int i = 0; i < indexCount; i++) {
						// Reading single tri
						// Чтение треугольника
						int v0 = f.ReadInt32();
						int v1 = f.ReadInt32();
						int v2 = f.ReadInt32();
						int mat = f.ReadByte();
						f.BaseStream.Position += 3;

						// Determining surf
						// Поиск поверхности
						if (indices[mat]==null) {
							indices[mat] = new List<int>();
							meshCount++;
						}
						indices[mat].AddRange(new int[]{ v0, v1, v2 });
					}

					// Storing trimeshes
					// Сохранение тримешей
					c.Meshes = new Trimesh[meshCount];
					int p = 0;
					for (int i = 0; i < indices.Length; i++) {
						if (indices[i]!=null) {
							c.Meshes[p] = new Trimesh() {
								Flags = 0,
								Material = (byte)i,
								Indices = indices[i].ToArray()
							};
							p++;
						}
					}
				}else{
					// Skip indices - they are empty
					// Пропуск индексов - они пустые
					f.BaseStream.Position += 4;
				}

				// Store mesh
				// Сохранение меша
				if (c.Spheres != null || c.Boxes != null || c.Meshes !=null) {
					Collisions.Add(c);
				}

				// Give prior to main thread
				// Отдаём предпочтение основному потоку
				System.Threading.Thread.Sleep(0);
			}

			// Closing reader
			// Закрытие потока
			f.Close();
		}

		/// <summary>
		/// Base container for single collision mesh<para/>
		/// Базовый объект одного меша столкновений
		/// </summary>
		public class Group {
			/// <summary>
			/// Name of this mesh<para/>
			/// Имя этого меша
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// IDE index of this mesh<para/>
			/// IDE-идентификатор этого меша
			/// </summary>
			public int ID { get; set; }

			/// <summary>
			/// Bound sphere radius<para/>
			/// Радиус сферы
			/// </summary>
			public float BoundsRadius { get; set; }

			/// <summary>
			/// Bounding sphere center<para/>
			/// Центр сферы коллизий
			/// </summary>
			public Vector3 BoundsCenter { get; set; }

			/// <summary>
			/// Bounding box start<para/>
			/// Начальные координаты бокса
			/// </summary>
			public Vector3 BoundsMin { get; set; }

			/// <summary>
			/// Bounding box end<para/>
			/// Конечные координаты бокса
			/// </summary>
			public Vector3 BoundsMax { get; set; }

			/// <summary>
			/// Collision spheres<para/>
			/// Сферы столкновений
			/// </summary>
			public Sphere[] Spheres;

			/// <summary>
			/// Collision boxes<para/>
			/// Ящики столкновений
			/// </summary>
			public Box[] Boxes;

			/// <summary>
			/// Vertices for meshes<para/>
			/// Вершины для тримешей
			/// </summary>
			public Vector3[] Vertices;

			/// <summary>
			/// Collision meshes<para/>
			/// Меши столкновений
			/// </summary>
			public Trimesh[] Meshes;
		}

		/// <summary>
		/// Base class for all primitives<para/>
		/// Базовый класс для всех примитивов
		/// </summary>
		public class Primitive {
			/// <summary>
			/// Surface material<para/>
			/// Материал поверхности
			/// </summary>
			public byte Material;

			/// <summary>
			/// Surface flags<para/>
			/// Флаги поверхности
			/// </summary>
			public byte Flags;

			/// <summary>
			/// Read associated parameters<para/>
			/// Чтение параметров
			/// </summary>
			/// <param name="f">File reader<para/>Открытый файл</param>
			public void ReadParams(BinaryReader f) {
				Material = f.ReadByte();
				Flags = f.ReadByte();
				f.BaseStream.Position += 2;
			}
		}

		/// <summary>
		/// Collision sphere<para/>
		/// Сфера столкновений
		/// </summary>
		public class Sphere : Primitive {
			/// <summary>
			/// Sphere radius<para/>
			/// Радиус сферы
			/// </summary>
			public float Radius;

			/// <summary>
			/// Sphere center<para/>
			/// Центр сферы
			/// </summary>
			public Vector3 Center;
		}

		/// <summary>
		/// Collision box<para/>
		/// Бокс столкновений
		/// </summary>
		public class Box : Primitive {
			/// <summary>
			/// Bounding box<para/>
			/// Ограничивающий бокс
			/// </summary>
			public Vector3 Min, Max;
		}

		/// <summary>
		/// Model index container<para/>
		/// Контейнер для индексов модели
		/// </summary>
		public class Trimesh : Primitive {
			/// <summary>
			/// Indices triplets<para/>
			/// Триплеты индексов
			/// </summary>
			public int[] Indices;
		}

		
	}
}
