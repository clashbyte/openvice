using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenVice.Files;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenVice.Graphics.Renderers;

namespace OpenVice.Graphics {

	/// <summary>
	/// Hierarchy-oriented mesh<para/>
	/// Меш с выстроеннной иерархией
	/// </summary>
	public class Model {
		/// <summary>
		/// Base model file<para/>
		/// Основной файл модели
		/// </summary>
		public ModelFile File;

		/// <summary>
		/// List of all root surfaces<para/>
		/// Список корневых поверхностей
		/// </summary>
		public Branch[] Children { get; private set; }

		/// <summary>
		/// Flag that engine shouldn't clear this dictionary<para/>
		/// Флаг что движок не должен освобождать этот архив для экономии памяти
		/// </summary>
		public bool Important;

		/// <summary>
		/// Model file loading state<para/>
		/// Состояние загрузки модели
		/// </summary>
		public ReadyState State;

		/// <summary>
		/// Use count per pipeline<para/>
		/// Количество использований
		/// </summary>
		public int UseCount;

		/// <summary>
		/// Create model from model file<para/>
		/// Создание модели из ModelFile
		/// </summary>
		/// <param name="f">Complete model file<para/>Прочитанный файл модели</param>
		/// <param name="sendNow">Send submeshes to GPU instantly<para/>Сразу же отправить данные меша на видеокарту</param>
		public Model(ModelFile f, bool important = false, bool sendNow = false) {

			// Checking completeness of model file
			// Проверка прочитанности модели
			if (sendNow && f.State != RenderWareFile.LoadState.Complete) {
				throw new Exception("[Model] Trying to create model from incomplete ModelFile");
			}
			State = ReadyState.Empty;

			// Initializing fields
			// Инициализация полей
			File = f;
			Important = important;

			// Send submeshes
			// Отправка сабмешей
			if (sendNow) {
				BuildSubMeshes();
				SendSubMeshes();
			}
		}

		/// <summary>
		/// Generate submesh temporary data<para/>
		/// Генерация временных GL-данных
		/// </summary>
		public void BuildSubMeshes() {
			// Analyze sub-branches
			// Выборка дочерних веток
			List<Branch> ch = new List<Branch>();
			for (int i = 0; i < File.Frames.Length; i++) {
				ModelFile.Frame fr = File.Frames[i];
				if (fr.Parent == -1) {
					ch.Add(new Branch(this, null, i));
				}
			}
			Children = ch.ToArray();
			foreach (Branch b in Children) {
				RecursiveProcessBranch(b, false);
			}
			State = ReadyState.NotSent;
		}

		/// <summary>
		/// Send submeshes to GPU<para/>
		/// Отправка данных на GPU
		/// </summary>
		public void SendSubMeshes() {
			if (State != ReadyState.NotSent) {
				return;
			}
			foreach (Branch b in Children) {
				RecursiveProcessBranch(b, true);
			}
			State = ReadyState.Complete;
		}

		/// <summary>
		/// Take all submeshes from model<para/>
		/// Получение всех сабмешей из модели
		/// </summary>
		/// <returns>Array of SubMeshes<para/>Массив сабмешей</returns>
		public SubMesh[] GetAllSubMeshes() {
			List<SubMesh> sml = new List<SubMesh>();
			foreach (Branch b in Children) {
				CollectSubMeshes(b, sml);
			}
			return sml.ToArray();
		}

		/// <summary>
		/// Remove model data<para/>
		/// Удаление данных модели
		/// </summary>
		public void Destroy() {
			if (Children!=null) {
				foreach (Branch b in Children) {
					RecursiveDestroyBranch(b);
				}
				Children = null;
			}
			State = ReadyState.Obsolette;
		}

		/// <summary>
		/// Internal recursive function to get all submeshes<para/>
		/// Внутренняя рекурсивная функция для поиска всех сабмешей
		/// </summary>
		/// <param name="b">Current branch<para/>Текущая ветвь</param>
		/// <param name="sml">List of submeshes to return<para/>Список возвращаемых сабмешей</param>
		void CollectSubMeshes(Branch b, List<SubMesh> sml) {
			if (b.SubMeshes!=null) {
				foreach (SubMesh s in b.SubMeshes) {
					sml.Add(s);
				}
			}
			if (b.Children!=null) {
				foreach (Branch br in b.Children) {
					CollectSubMeshes(br, sml);
				}
			}
		}

		/// <summary>
		/// Processing all the GL stuff from all branches<para/>
		/// Обработка всех GL-данных из веток
		/// </summary>
		/// <param name="branch">Current branch<para/>Текущая ветка</param>
		void RecursiveProcessBranch(Branch branch, bool send) {

			// Sending associated submeshes
			// Отправка ассоциированных мешей
			if (branch.SubMeshes != null) {
				foreach (SubMesh sm in branch.SubMeshes) {
					if (send) {
						sm.Send();
					}else{
						sm.Build();
					}
				}
			}

			// Recursive send children
			// Рекурсивная отправка детей
			if (branch.Children != null) {
				foreach (Branch br in branch.Children) {
					RecursiveProcessBranch(br, send);
				}
			}
		}

		/// <summary>
		/// Destroy branch data and free it's memory<para/>
		/// Уничтожение ветви
		/// </summary>
		/// <param name="b">Specified branch<para/>Ветвь для удаления</param>
		void RecursiveDestroyBranch(Branch b) {
			if (b.Children!=null) {
				foreach (Branch sb in b.Children) {
					RecursiveDestroyBranch(sb);
				}
			}
			b.Release();
		}

		/// <summary>
		/// Single hierarchy entry<para/>
		/// Одна часть иерархии
		/// </summary>
		public class Branch {

			/// <summary>
			/// List of all children branches<para/>
			/// Список дочерних бранчей
			/// </summary>
			public Branch[] Children { get; private set; }

			/// <summary>
			/// Parental branch<para/>
			/// Родительский бранч
			/// </summary>
			public Branch Parent { get; private set; }

			/// <summary>
			/// Parent model<para/>
			/// Модель, к которой отсносится бранч
			/// </summary>
			public Model ParentModel { get; private set; }

			/// <summary>
			/// List of associated submeshes<para/>
			/// Список связанных сабмешей
			/// </summary>
			public SubMesh[] SubMeshes { get; private set; }

			/// <summary>
			/// Name of this branch<para/>
			/// Имя этой ветви
			/// </summary>
			public string Name { get; private set; }

			/// <summary>
			/// Original bone position<para/>
			/// Исходное расположение кости
			/// </summary>
			public Vector3 OriginalPosition {
				get; private set;
			}

			/// <summary>
			/// Original bone rotation<para/>
			/// Исходный поворот кости
			/// </summary>
			public Quaternion OriginalAngles {
				get; private set;
			}

			/// <summary>
			/// Original bone scale<para/>
			/// Исходный скейл кости
			/// </summary>
			public float OriginalScale {
				get; private set;
			}

			/// <summary>
			/// Original inverted matrix for skinning
			/// </summary>
			public Matrix4 OriginalInvMatrix {
				get; private set;
			}

			/// <summary>
			/// Original bone index in file
			/// </summary>
			public int OriginalIndex {
				get; private set;
			}

			/// <summary>
			/// Model position in 3D space<para/>
			/// Расположение модели в пространстве
			/// </summary>
			public Vector3 Position {
				get { 
					return new Vector3(position.X, position.Y, -position.Z); 
				}
				set {
					position = new Vector3(value.X, value.Y, -value.Z);
					UpdateChildrenMatrix();

				}
			}

			/// <summary>
			/// Model rotation in 3D space<para/>
			/// Поворот модели в 3D-пространстве
			/// </summary>
			public Quaternion Angles { 
				get {
					return new Quaternion(angles.X, angles.Y, -angles.Z, -angles.W);
				}
				set {
					angles = new Quaternion(value.X, value.Y, -value.Z, -value.W);
					UpdateChildrenMatrix();
				}
			}

			/// <summary>
			/// Model scale in 3D space<para/>
			/// Размер модели в 3D-мире
			/// </summary>
			public float Scale {
				get {
					return scale;
				}
				set {
					scale = value;
					UpdateChildrenMatrix();
				}
			}

			/// <summary>
			/// Calculated position matrix<para/>
			/// Вычисленная матрица расположения
			/// </summary>
			public Matrix4 Matrix {
				get {
					if (needNewMatrix) {
						mat =
							Matrix4.CreateScale(scale) *
							Matrix4.CreateFromQuaternion(angles) *
							Matrix4.CreateTranslation(position);
						if (Parent!=null) {
							mat = mat * Parent.Matrix;
						}
						needNewMatrix = false;
					}
					return mat;
				}
			}

			// Hidden GL-oriented values
			// Скрытые, GL-дружелюбные значения
			Vector3 position	= Vector3.Zero;
			Quaternion angles	= Quaternion.Identity;
			float scale			= 1f;
			Matrix4 mat			= Matrix4.Identity;
			bool needNewMatrix	= false;

			/// <summary>
			/// Creating branch for existing model<para/>
			/// Создание ветки для существующей модели
			/// </summary>
			/// <param name="model">Parent model<para/>Модель-родитель</param>
			/// <param name="parent">Parent branch<para/>Родительский бранч</param>
			/// <param name="branchIndex">Branch index in ModelFile<para/>Индекс в файле модели</param>
			public Branch(Model model, Branch parent, int branchIndex) {
				
				// Setting up special data
				// Установка спецданных
				ModelFile.Frame f = model.File.Frames[branchIndex];
				Position = new Vector3(f.Position[0], f.Position[2], f.Position[1]);
				Quaternion q = Quaternion.FromMatrix(
					new Matrix3(
						f.Rotation[0], f.Rotation[1], f.Rotation[2],
						f.Rotation[3], f.Rotation[4], f.Rotation[5],
						f.Rotation[6], f.Rotation[7], f.Rotation[8]
					)
				);
				Angles = new Quaternion(-q.X, -q.Z, -q.Y, -q.W);
				Scale = 1f;
				ParentModel = model;
				Parent = parent;
				Name = f.Name;
				
				OriginalIndex = branchIndex;
				OriginalPosition = Position;
				OriginalScale = Scale;
				OriginalAngles = Angles;
				
				// Hack to rebuild matrix
				// Хак для перестройки матрицы
				UpdateChildrenMatrix();
				mat = Matrix;
				OriginalInvMatrix = mat.Inverted();

				// Analyze model's geometries for data
				// Анализируем геометрию модели для создание сурфейсов
				List<SubMesh> lm = new List<SubMesh>();
				foreach (ModelFile.Geometry g in model.File.Surfaces) {
					if (g.Frame == branchIndex) {
						lm.Add(new SubMesh(this, model, g) { 
							

						});
					}
				}
				SubMeshes = lm.ToArray();

				// Analyze sub-branches
				// Выборка дочерних веток
				List<Branch> ch = new List<Branch>();
				for (int i = 0; i < model.File.Frames.Length; i++) {
					ModelFile.Frame fr = model.File.Frames[i];
					if (fr.Parent == branchIndex) {
						ch.Add(new Branch(model, this, i) {
						});
					}
				}
				Children = ch.ToArray();
			}


			/// <summary>
			/// Clearing branch stuff<para/>
			/// Очистка данных ветки
			/// </summary>
			public void Release() {
				foreach (SubMesh s in SubMeshes) {
					s.Release();
				}
				Parent = null;
				ParentModel = null;
				Children = null;
				SubMeshes = null;
			}

			/// <summary>
			/// Rebuilding all children<para/>
			/// Перестраиваем дочерние матрицы
			/// </summary>
			void UpdateChildrenMatrix() {
				needNewMatrix = true;
				if (Children != null) {
					foreach (Branch branch in Children) {
						branch.UpdateChildrenMatrix();
					}
				}
			}

		}

		/// <summary>
		/// Данные о поверхности
		/// </summary>
		public class SubMesh {

			/// <summary>
			/// Parental branch<para/>
			/// Родительский бранч
			/// </summary>
			public Branch Parent { get; private set; }

			/// <summary>
			/// Parent model<para/>
			/// Модель, к которой отсносится бранч
			/// </summary>
			public Model ParentModel { get; private set; }

			/// <summary>
			/// Bone index buffer
			/// </summary>
			public int BoneIndexBuffer {
				get {
					return boneBuffer;
				}
			}

			/// <summary>
			/// Buffer with bone weight data
			/// </summary>
			public int BoneWeightBuffer {
				get {
					return boneWeightBuffer;
				}
			}

			/// <summary>
			/// Renderable surfaces<para/>
			/// Группы индексов вершин с материалами
			/// </summary>
			public Surface[] Surfaces;

			/// <summary>
			/// Current submesh state<para/>
			/// Текущее состояние сабмеша
			/// </summary>
			public ReadyState State;

			// Internal GL data
			// Внутренние GL-данные
			float[] vertexData;
			float[] normalData;
			byte[] colorData;
			float[] texCoord1Data;
			float[] texCoord2Data;
			byte[] boneData;
			float[] boneWeightData;
			int vertexBuffer, normalBuffer, colorBuffer, tex1Buffer, tex2Buffer;
			int boneBuffer, boneWeightBuffer;
			ModelFile.Geometry geometry;

			/// <summary>
			/// Create submesh from ModelFile's Geometry entry<para/>
			/// Создание сабмеша из данных Geometry
			/// </summary>
			/// <param name="g">Loaded Geometry<para/>Загруженая Geometry</param>
			public SubMesh(Branch parent, Model model, ModelFile.Geometry g) {
				
				// Setting up the fields
				// Настройка полей
				State = ReadyState.Empty;
				Parent = parent;
				ParentModel = model;
				geometry = g;

			}

			/// <summary>
			/// Get static cull sphere for this submesh<para/>
			/// Получение статической сферы для меша
			/// </summary>
			/// <param name="parent">Parent matrix<para/>Родительская матрица</param>
			/// <returns>CullSphere</returns>
			public CullSphere GetSphere(Matrix4 parent) {
				return new CullSphere(new Vector3(
						geometry.SpherePos[0],
						geometry.SpherePos[2],
						geometry.SpherePos[1]
					), geometry.SphereRadius, Parent.Matrix * parent);
			}

			/// <summary>
			/// Render this submesh<para/>
			/// Отрисовка этого меша
			/// </summary>
			/// <param name="td">Textures to use<para/>Текстуры для использования</param>
			/// <param name="trans">Transparent mode<para/>Полупрозрачный режим</param>
			public void Render(RendererBase renderer, TextureDictionary td, bool trans, bool force = false) {
				if (State != ReadyState.Obsolette) {
					if (State == ReadyState.Empty) {
						Build();
						State = ReadyState.NotSent;
					} else if (State == ReadyState.Complete) {
						DrawGL(renderer, td, trans, force);
					}
				}
				ParentModel.UseCount++;
			}

			

			/// <summary>
			/// Build vertex data from specified geometry<para/>
			/// Построение геометрических данных из вершин
			/// </summary>
			public void Build() {
				if (State!=ReadyState.Empty)
				{
					return;
				}
				State = ReadyState.Reading;

				// Decoding vertices
				// Разбираем вершины
				if (geometry.Vertices!=null) {
					vertexData = new float[geometry.Vertices.Length];
					for (int i = 0; i < vertexData.Length; i+=3) {
						vertexData[i] = geometry.Vertices[i];
						vertexData[i+1] = geometry.Vertices[i+2];
						vertexData[i+2] = -geometry.Vertices[i+1];
					}
				}

				// Decoding normals
				// Разбираем нормали
				if (geometry.Normals != null) {
					normalData = new float[geometry.Normals.Length];
					for (int i = 0; i < normalData.Length; i += 3) {
						normalData[i] = geometry.Normals[i];
						normalData[i + 1] = geometry.Normals[i + 2];
						normalData[i + 2] = -geometry.Normals[i + 1];
					}
				}

				// Decoding colors
				// Разбираем цвета
				if (geometry.Colors != null) {
					colorData = new byte[geometry.Colors.Length];
					Array.Copy(geometry.Colors, colorData, geometry.Colors.Length);
				}
				

				// Decoding first texture coords
				// Разбираем первые текстурные координаты
				if (geometry.TextureCoords != null) {
					texCoord1Data = new float[geometry.TextureCoords.Length];
					for (int i = 0; i < texCoord1Data.Length; i+=2) {
						texCoord1Data[i] = geometry.TextureCoords[i];
						texCoord1Data[i+1] = geometry.TextureCoords[i+1];
					}
				}

				// Decoding second texture coords
				// Разбираем вторые текстурные координаты
				if (geometry.SecondTextureCoords != null) {
					texCoord2Data = new float[geometry.SecondTextureCoords.Length];
					for (int i = 0; i < texCoord2Data.Length; i += 2) {
						texCoord2Data[i] = geometry.SecondTextureCoords[i];
						texCoord2Data[i + 1] = geometry.SecondTextureCoords[i + 1];
					}
				}

				// Decoding bones
				// Декодим кости
				if (geometry.Bones != null) {
					boneData = new byte[geometry.Bones.Length];
					for (int i = 0; i < boneData.Length; i ++) {
						boneData[i] = geometry.Bones[i];
					}
				}
				if (geometry.Weights != null) {
					boneWeightData = new float[geometry.Weights.Length];
					for (int i = 0; i < boneWeightData.Length; i++) {
						boneWeightData[i] = geometry.Weights[i];
					}
				}

				// Decoding indices
				// Разбор индексов
				List<Surface> surfs = new List<Surface>();
				if (geometry.Binary!=null) {
					// Detect binary meshes
					// Разбор бинарных мешей
					foreach (ModelFile.BinaryMesh m in geometry.Binary) {
						surfs.Add(new Surface() {
							IndexData = m.Indices.ToArray(),
							IsTriangleStrip = m.Mode == ModelFile.SplitMode.TriangleStrip,
							Material = new Material(m.BinaryMaterial)
						});
					}
				}else{
					// Detect normal meshes
					// Разбор обычных мешей
					List<ushort>[] indices = new List<ushort>[geometry.Materials.Length];
					for (int i = 0; i < geometry.Materials.Length; i++) {
						indices[i] = new List<ushort>();
					}
					for (int idx = 0; idx < geometry.Indices.Length; idx += 4) {
						indices[geometry.Indices[idx + 2]].AddRange(new ushort[]{
							geometry.Indices[idx+0],
							geometry.Indices[idx+1],
							geometry.Indices[idx+3]
						});
					}
					// Creating surfaces
					// Создание сурфейсов
					for (int i = 0; i < geometry.Materials.Length; i++) {
						surfs.Add(new Surface() {
							IndexData = indices[i].ToArray(),
							IsTriangleStrip = false,
							Material = new Material(geometry.Materials[i])
						});
					}
				}
				Surfaces = surfs.ToArray();
				State = ReadyState.NotSent;
			}

			/// <summary>
			/// Send vertex data to GL<para/>
			/// Отправка вершинных данных в GL
			/// </summary>
			public void Send() {
				if (State!=ReadyState.NotSent) {
					if (State == ReadyState.Empty) {
						Build();
					}else{
						return;
					}
				}

				// Sending positions
				// Отправка позиций
				if (vertexData!=null) {
					vertexBuffer = GL.GenBuffer();
					GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
					GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertexData.Length * sizeof(float)), vertexData, BufferUsageHint.StaticDraw);
					vertexData = null;
				}

				// Sending normals
				// Отправка нормалей
				if (normalData != null) {
					normalBuffer = GL.GenBuffer();
					GL.BindBuffer(BufferTarget.ArrayBuffer, normalBuffer);
					GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(normalData.Length * sizeof(float)), normalData, BufferUsageHint.StaticDraw);
					normalData = null;
				}

				// Sending colors
				// Отправка цвета
				if (colorData != null) {
					colorBuffer = GL.GenBuffer();
					GL.BindBuffer(BufferTarget.ArrayBuffer, colorBuffer);
					GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(colorData.Length * sizeof(byte)), colorData, BufferUsageHint.StaticDraw);
					colorData = null;
				}

				// Sending tex coords
				// Отправка текстурных координат
				if (texCoord1Data != null) {
					tex1Buffer = GL.GenBuffer();
					GL.BindBuffer(BufferTarget.ArrayBuffer, tex1Buffer);
					GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(texCoord1Data.Length * sizeof(float)), texCoord1Data, BufferUsageHint.StaticDraw);
					texCoord1Data = null;
				}
				if (texCoord2Data != null) {
					tex2Buffer = GL.GenBuffer();
					GL.BindBuffer(BufferTarget.ArrayBuffer, tex2Buffer);
					GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(texCoord2Data.Length * sizeof(float)), texCoord2Data, BufferUsageHint.StaticDraw);
					texCoord2Data = null;
				}

				// Sending bone info
				// Отправка данных о костях
				if (boneData != null) {
					boneBuffer = GL.GenBuffer();
					GL.BindBuffer(BufferTarget.ArrayBuffer, boneBuffer);
					GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(boneData.Length * sizeof(byte)), boneData, BufferUsageHint.StaticDraw);
				}
				if (boneWeightData != null) {
					boneWeightBuffer = GL.GenBuffer();
					GL.BindBuffer(BufferTarget.ArrayBuffer, boneWeightBuffer);
					GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(boneWeightData.Length * sizeof(float)), boneWeightData, BufferUsageHint.StaticDraw);
				}

				// Sending indices
				// Отправка индексов вершин
				foreach (Surface s in Surfaces) {
					s.IndexBuffer = GL.GenBuffer();
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, s.IndexBuffer);
					GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(s.IndexData.Length * sizeof(ushort)), s.IndexData, BufferUsageHint.StaticDraw);
					s.IndexCount = s.IndexData.Length;
					s.IndexData = null;
				}


				// Releasing buffers
				// Освобождаем буфферы
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

				State = ReadyState.Complete;
			}

			/// <summary>
			/// Drawing GL data<para/>
			/// Отрисовка GL-данных
			/// </summary>
			void DrawGL(RendererBase renderer, TextureDictionary td, bool trans, bool force) {
				
				// Sending vertex data
				// Отправка вершинных данных
				if (vertexBuffer!=0) {
					GL.EnableClientState(ArrayCap.VertexArray);
					GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
					GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);
				}

				// Sending normals
				// Отправка нормалей
				if (normalBuffer!=0) {
					GL.EnableClientState(ArrayCap.NormalArray);
					GL.BindBuffer(BufferTarget.ArrayBuffer, normalBuffer);
					GL.NormalPointer(NormalPointerType.Float, 0, IntPtr.Zero);
				}

				// Sending colors
				// Отправка цветов
				if (colorBuffer != 0) {
					GL.EnableClientState(ArrayCap.ColorArray);
					GL.BindBuffer(BufferTarget.ArrayBuffer, colorBuffer);
					GL.ColorPointer(4, ColorPointerType.UnsignedByte, 0, IntPtr.Zero);
				}


				// Drawing surfaces
				// Отрисовка поверхностей
				foreach (Surface s in Surfaces) {
					if (s.IndexBuffer != 0 && (s.Material.HasAlpha == trans || force)) {
						// Binding material
						// Присвоение материала
						BindMaterial(renderer, s, td);

						// Drawing tris
						// Отрисовка треугольников
						PrimitiveType pt = PrimitiveType.Triangles;
						int triCount = s.IndexCount/3;
						if (s.IsTriangleStrip) {
							pt = PrimitiveType.TriangleStrip;
							triCount = s.IndexCount - 2;
						}
						GL.BindBuffer(BufferTarget.ElementArrayBuffer, s.IndexBuffer);
						GL.DrawElements(pt, s.IndexCount, DrawElementsType.UnsignedShort, IntPtr.Zero);
						GL.DisableClientState(ArrayCap.TextureCoordArray);

						for (int i = 0; i < 8; i++) {
							GL.DisableVertexAttribArray(i);
						}

						// Incrementing counters
						// Увеличение счётчиков
						Renderer.TrisRendered += triCount;
						Renderer.DrawCalls++;
					}
				}

				// Unbinding the buffers
				// Отключаем буфферы
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

				// Disabling states
				// Отключение стейтов
				if (vertexBuffer != 0) {
					GL.DisableClientState(ArrayCap.VertexArray);
				}
				if (normalBuffer != 0) {
					GL.DisableClientState(ArrayCap.NormalArray);
				}
				if (colorBuffer != 0) {
					GL.DisableClientState(ArrayCap.ColorArray);
				}
			}

			/// <summary>
			/// Bind material data<para/>
			/// Присвоение материала
			/// </summary>
			void BindMaterial(RendererBase renderer, Surface surf, TextureDictionary td) {
				Material mat = surf.Material;
				if (mat.Textures!=null) {
					Texture t = (Texture)mat.Textures[0];
					TextureDictionary.Texture tex = t.CachedTexture;

					// Caching texture
					// Кешируем текстуру
					if (tex != null) {
						if (tex.State == TextureDictionary.ReadyState.Obsolette) {
							tex = null;
						}	
					}
					if (tex == null) {
						string lname = t.Name.ToLower();
						if (td.Textures.ContainsKey(lname)) {
							tex = td.Textures[lname];
							t.CachedTexture = tex;
						}
					}

					// Binding texture coord array
					// Установка массива текстурных координат
					GL.EnableClientState(ArrayCap.TextureCoordArray);
					GL.BindBuffer(BufferTarget.ArrayBuffer, tex1Buffer);
					GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, IntPtr.Zero);

					// Binding it
					// Установка текстуры
					if (tex!=null) {
						if (tex.State == TextureDictionary.ReadyState.Complete) {

							// Binding tex
							// Установка текстуры
							tex.Bind();

							// Setting up addressing and filtering
							// Установка адресации и фильтрации
							TextureFile.FilterMode fm = t.Filter;
							if (true) {//!tex.Mipmapped) {
								switch (t.Filter) {
									case TextureFile.FilterMode.MipNearest:
										fm = TextureFile.FilterMode.Nearest;
										break;
									case TextureFile.FilterMode.MipLinear:
										fm = TextureFile.FilterMode.Nearest;
										break;
									case TextureFile.FilterMode.LinearMipNearest:
										fm = TextureFile.FilterMode.Linear;
										break;
									case TextureFile.FilterMode.LinearMipLinear:
										fm = TextureFile.FilterMode.Linear;
										break;
								}
							}
							switch (fm) {
								case TextureFile.FilterMode.Nearest:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
									break;
								case TextureFile.FilterMode.Linear:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
									break;
								case TextureFile.FilterMode.MipNearest:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.NearestMipmapNearest);
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
									break;
								case TextureFile.FilterMode.MipLinear:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.NearestMipmapLinear);
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
									break;
								case TextureFile.FilterMode.LinearMipNearest:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapNearest);
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
									break;
								case TextureFile.FilterMode.LinearMipLinear:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapLinear);
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
									break;
								default:
									throw new Exception("[Model] Invalid value for FilterMode");
							}

							// U addressing
							// Адресация текстуры по горизонтали
							switch (t.AddressU) {
								case TextureFile.AddressMode.Repeat:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
									break;
								case TextureFile.AddressMode.Mirror:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.MirroredRepeat);
									break;
								case TextureFile.AddressMode.Clamp:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Clamp);
									break;
								default:
									throw new Exception("[Model] Invalid value for AddressMode");
							}

							// V addressing
							// Адресация текстуры по вертикали
							switch (t.AddressV) {
								case TextureFile.AddressMode.Repeat:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
									break;
								case TextureFile.AddressMode.Mirror:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.MirroredRepeat);
									break;
								case TextureFile.AddressMode.Clamp:
									GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Clamp);
									break;
								default:
									throw new Exception("[Model] Invalid value for AddressMode");
							}
						}
					} else {
						GL.BindTexture(TextureTarget.Texture2D, Renderer.EmptyTexture);
					}
				}else{
					GL.BindTexture(TextureTarget.Texture2D, Renderer.EmptyTexture);
				}
				renderer.SetupSurface(surf, mat, geometry, this);
			}
			
			/// <summary>
			/// Release memory used by this mesh<para/>
			/// Очистка памяти, используемой этим мешем
			/// </summary>
			public void Release() {

				// Removing surfaces
				// Удаляем поверхности
				foreach (Surface s in Surfaces) {
					s.IndexData = null;
					s.Material = null;
					if (s.IndexBuffer != 0) GL.DeleteBuffer(s.IndexBuffer);
				}
				geometry = null;
				Parent = null;
				ParentModel = null;

				// Removing buffers
				// Удаляем буфферы
				vertexData		= null;
				normalData		= null;
				colorData		= null;
				texCoord1Data	= null;
				texCoord2Data	= null;
				boneData		= null;
				boneWeightData	= null;
				if (vertexBuffer != 0)	GL.DeleteBuffer(vertexBuffer);
				if (normalBuffer != 0)	GL.DeleteBuffer(normalBuffer);
				if (colorBuffer != 0)	GL.DeleteBuffer(colorBuffer);
				if (tex1Buffer != 0)	GL.DeleteBuffer(tex1Buffer);
				if (tex2Buffer != 0)	GL.DeleteBuffer(tex2Buffer);
				if (boneBuffer != 0)	GL.DeleteBuffer(boneBuffer);
				if (boneWeightBuffer != 0) GL.DeleteBuffer(boneWeightBuffer);

			}

			/// <summary>
			/// Single material surface<para/>
			/// Одна поверхность материала
			/// </summary>
			public class Surface {

				/// <summary>
				/// Vertex indices data<para/>
				/// Индексы вершин
				/// </summary>
				public ushort[] IndexData;

				/// <summary>
				/// GL indices buffer<para/>
				/// Буффер с GL-индексами
				/// </summary>
				public int IndexBuffer;

				/// <summary>
				/// Material for this surface<para/>
				/// Материал для данной поверхности
				/// </summary>
				public Material Material;

				/// <summary>
				/// Number of indexing elements<para/>
				/// Количество индексов
				/// </summary>
				public int IndexCount;

				/// <summary>
				/// Use Triangle strip instead of triangle list<para/>
				/// Использовать при рендере TriangleStrip
				/// </summary>
				public bool IsTriangleStrip;

			}
		}

		/// <summary>
		/// Surface material<para/>
		/// Материал поверхности
		/// </summary>
		public class Material : ModelFile.Material {

			/// <summary>
			/// Copy existing material<para/>
			/// Копирование существующего материала
			/// </summary>
			/// <param name="mt">Existing material<para/>Существующий материал</param>
			public Material(ModelFile.Material mt) {
				Color = mt.Color;
				Flags = mt.Flags;
				HasAlpha = mt.HasAlpha;
				Props = mt.Props;
				if (mt.Textures!=null) {
					Textures = new Texture[mt.Textures.Length];
					for (int i = 0; i < Textures.Length; i++) {
						Textures[i] = new Texture(mt.Textures[i]);
					}
				}
			}

		}

		/// <summary>
		/// Material<para/>
		/// Текстура материала
		/// </summary>
		public class Texture : ModelFile.Texture {

			/// <summary>
			/// Cached texture<para/>
			/// Кешированная текстура
			/// </summary>
			public TextureDictionary.Texture CachedTexture;

			/// <summary>
			/// Create copy of texture<para/>
			/// Создание копии текстуры
			/// </summary>
			/// <param name="tx">Existing texture<para/>Существующая текстура</param>
			public Texture(ModelFile.Texture tx) {
				AddressU = tx.AddressU;
				AddressV = tx.AddressV;
				Filter = tx.Filter;
				Name = tx.Name;
				MaskName = tx.MaskName;
			}
		}

		/// <summary>
		/// Model state<para/>
		/// Состояние модели
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
