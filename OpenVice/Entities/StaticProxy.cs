using OpenVice.Data;
using OpenVice.Files;
using OpenVice.Graphics;
using OpenVice.Graphics.Renderers;
using OpenVice.Managers;
using OpenVice.World;

namespace OpenVice.Entities {

	/// <summary>
	/// City static mesh proxy object<para/>
	/// Статический меш городского объекта
	/// </summary>
	public class StaticProxy {

		/// <summary>
		/// Huge Beach LOD mesh<para/>
		/// Огромный меш первого острова
		/// </summary>
		public static StaticProxy BeachLod;

		/// <summary>
		/// Huge MainLand LOD mesh<para/>
		/// Огромный меш второго острова
		/// </summary>
		public static StaticProxy MainlandLod;

		/// <summary>
		/// Main hipoly mesh group<para/>
		/// Группа высокополигонального меша
		/// </summary>
		public Group MainMesh;

		/// <summary>
		/// Lowpoly LOD mesh group<para/>
		/// Группа низкополигонального меша
		/// </summary>
		public Group LODMesh;

		/// <summary>
		/// Current rendering state<para/>
		/// Текущее состояние видимости
		/// </summary>
		public VisState State;
		
		/// <summary>
		/// Flag that lod is found<para/>
		/// Флаг что LOD найден
		/// </summary>
		public bool LodAssigned;

		/// <summary>
		/// Sending models to rendering queue<para/>
		/// Отправка моделей на отрисовку
		/// </summary>
		public void Render(float delta) {

			bool needMesh = false;
			bool needLod = false;

			switch (State) {
				case VisState.Hidden:
					break;
				case VisState.LodVisible:
					if (MainMesh.Ready && !LODMesh.Ready) {
						needMesh = true;
					}
					needLod = true;
					break;
				case VisState.MeshVisible:
					if (LODMesh!=null && !MainMesh.Ready) {
						needLod = true;
					}
					needMesh = true;
					break;
			}


			// Mesh is needed
			MainMesh.Process(needMesh);
			if (needMesh && MainMesh.IsTimedVisible()) {
				MainMesh.Render();
			}
			

			// Lod is needed
			if (LODMesh != null) {
				LODMesh.Process(needLod);
				if (needLod && LODMesh.IsTimedVisible()) {
					LODMesh.Render();
				}
			}

		}

		/// <summary>
		/// Clean mesh unused data<para/>
		/// Очистка неиспользуемых данных
		/// </summary>
		public void Cleanup() {

		}

		/// <summary>
		/// Single model group<para/>
		/// Одна группа моделей
		/// </summary>
		public class Group {

			/// <summary>
			/// ItemPlacement link for this group<para/>
			/// Ссылка на ItemPlacement
			/// </summary>
			public ItemPlacement Definition;

			/// <summary>
			/// Group position in scene<para/>
			/// Расположение группы в сцене
			/// </summary>
			public Transform Coords;

			/// <summary>
			/// Model for this group<para/>
			/// Модель для этой группы
			/// </summary>
			public Model GroupModel;

			/// <summary>
			/// Mesh rendering distance<para/>
			/// Дистанция для рендера меша
			/// </summary>
			public float Range;

			/// <summary>
			/// Mesh is time-oriented<para/>
			/// Меш ориентирован на время
			/// </summary>
			public bool Timed;

			/// <summary>
			/// Hour to start object rendering<para/>
			/// Час когда начать рисовать объект
			/// </summary>
			public int HourOn;

			/// <summary>
			/// Hour to stop object rendering<para/>
			/// Час когда перестать рисовать объект
			/// </summary>
			public int HourOff;

			/// <summary>
			/// Mesh opacity<para/>
			/// Непрозрачность меша
			/// </summary>
			public float Opacity = 1f;

			/// <summary>
			/// TextureDictionary for this group<para/>
			/// Текстурный архив дня этой группы
			/// </summary>
			public TextureDictionary GroupTextures;

			/// <summary>
			/// Array of cached SubmeshRenderers<para/>
			/// Массив кешированных отрисовщиков
			/// </summary>
			public StaticRenderer[] Renderers;

			/// <summary>
			/// Flag that surface is ready to draw<para/>
			/// Флаг что поверхность готова к отрисовке
			/// </summary>
			public bool Ready;

			/// <summary>
			/// Process mesh internals<para/>
			/// Обработка данных меша
			/// </summary>
			public void Process(bool needed) {
				if (needed) {
					if (GroupModel != null) {
						if (GroupTextures != null) {
							if (GroupModel.State == Model.ReadyState.Complete && GroupTextures.State == TextureDictionary.ReadyState.Complete) {
								// Surface is ready to render
								// Поверхность готова к отрисовке
								Ready = true;
								Model.SubMesh[] subs = GroupModel.GetAllSubMeshes();
								Renderers = new StaticRenderer[subs.Length];
								Coords.Position = Coords.Position;
								for (int i = 0; i < Renderers.Length; i++) {
									Renderers[i] = new StaticRenderer() {
										BaseMatrix = Coords.Matrix,
										SubmeshMatrix = subs[i].Parent.Matrix,
										SubMesh = subs[i],
										Textures = GroupTextures,
										Fading = false,
										FadingDelta = 1
									};
								}
							} else {
								// Check for model state
								// Проверка состояния модели
								if (GroupModel.State != Model.ReadyState.Complete) {
									if (GroupModel.File.State == Files.RenderWareFile.LoadState.Complete) {
										if (!ModelManager.IsProcessing(GroupModel)) {
											ModelManager.ModelProcessQueue.Enqueue(GroupModel);
										}
									}
								}
								// Check for texture dictionary state
								// Проверка состояния архива текстур
								if (GroupTextures.State != TextureDictionary.ReadyState.Complete) {
									if (GroupTextures.File.State == Files.RenderWareFile.LoadState.Complete) {
										if (!TextureManager.IsProcessing(GroupTextures)) {
											TextureManager.TextureProcessQueue.Enqueue(GroupTextures);
										}
									}
								}
							}
						} else {
							// Texture not found - get it
							// Текстура не найдена - получаем её
							string tname = ObjectManager.Definitions[Definition.ID].TexDictionary;
							if (TextureManager.Cached.ContainsKey(tname)) {
								GroupTextures = TextureManager.Cached[tname];
							} else {
								TextureFile tf = null;
								if (TextureManager.CachedFiles.ContainsKey(tname)) {
									tf = TextureManager.CachedFiles[tname];
								} else {
									tf = new TextureFile(ArchiveManager.Get(tname + ".txd"), false);
									TextureManager.CachedFiles.TryAdd(tname, tf);
									TextureManager.TextureFileProcessQueue.Enqueue(tf);
								}
								GroupTextures = new TextureDictionary(tf);
								TextureManager.Cached.TryAdd(tname, GroupTextures);
							}
							GroupTextures.UseCount++;
						}
					} else {
						// Model not found - get it
						// Модель не найдена - получаем её
						string mname = ObjectManager.Definitions[Definition.ID].ModelName;
						if (ModelManager.Cached.ContainsKey(mname)) {
							GroupModel = ModelManager.Cached[mname];
						} else {
							ModelFile mf = null;
							if (ModelManager.CachedFiles.ContainsKey(mname)) {
								mf = ModelManager.CachedFiles[mname];
							} else {
								mf = new ModelFile(ArchiveManager.Get(mname + ".dff"), false);
								ModelManager.CachedFiles.TryAdd(mname, mf);
								ModelManager.ModelFileProcessQueue.Enqueue(mf);
							}
							GroupModel = new Model(mf);
							ModelManager.Cached.TryAdd(mname, GroupModel);
						}
						GroupModel.UseCount++;
					}
				} else {
					// Cleaning all the usings
					// Очистка использований
					if (GroupModel != null) {
						if (!GroupModel.Important) {
							GroupModel.UseCount--;
						}
						GroupModel = null;
					}
					if (GroupTextures != null) {
						if (!GroupTextures.Important) {
							GroupTextures.UseCount--;
						}
						GroupTextures = null;
					}
					if (Renderers!=null) {
						Renderers = null;
					}
					Ready = false;
				}
			}


			/// <summary>
			/// Render this group<para/>
			/// Отрисовка этой группы
			/// </summary>
			public void Render() {
				if (Renderers!=null) {
					if (Opacity > 0) {
						foreach (StaticRenderer r in Renderers) {
							if (Opacity>=1f) {
								r.Fading = false;
							} else {
								r.Fading = true;
							}
							r.FadingDelta = 1f;
							Renderer.RenderQueue.Enqueue(r);
						}
					}
				}
			}

			/// <summary>
			/// Check if this object is time-visible<para/>
			/// Виден ли данный объект в данный момент времени
			/// </summary>
			public bool IsTimedVisible() {
				if (Timed) {
					if (HourOff>HourOn) {
						if (Environment.Hour>=HourOff || Environment.Hour<HourOn ) {
							return false;
						}
					} else {
						if (Environment.Hour>=HourOff && Environment.Hour<HourOn) {
							return false;
						}
					}
				}
				return true;
			}
		}

		/// <summary>
		/// Mesh visibility flags<para/>
		/// Флаги видимости меша
		/// </summary>
		public enum VisState {
			Hidden, 
			LodVisible,
			MeshVisible
		}


	}
}
