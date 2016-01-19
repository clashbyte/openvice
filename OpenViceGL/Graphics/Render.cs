using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenVice.Managers;
using OpenVice.Files;

namespace OpenVice.Graphics {

	/// <summary>
	/// Main rendering class<para/>
	/// Основной класс рендера
	/// </summary>
	public static class Renderer {

		/// <summary>
		/// Current sky values<para/>
		/// Текущие значения параметров неба
		/// </summary>
		public static TimeCycleManager.Entry SkyState { get; set; }

		/// <summary>
		/// Flag to render 3D stuff<para/>
		/// Флаг отрисовки 3D
		/// </summary>
		public static bool Enable3D = true;

		/// <summary>
		/// Defines algorithm for alphablended surfaces<para/>
		/// Задаёт алгоритм отрисовки полупрозрачных поверхностей
		/// </summary>
		public static TransparencyRendering TransparencyRenderingMode = TransparencyRendering.Classic;

		/// <summary>
		/// Flag to render 2D stuff<para/>
		/// Флаг отрисовки 2D
		/// </summary>
		public static bool Enable2D = true;

		/// <summary>
		/// Number of rendered triangles<para/>
		/// Количество нарисованных треугольников
		/// </summary>
		public static int TrisRendered = 0;

		/// <summary>
		/// Number of draw calls<para/>
		/// Количество вызовов отрисовки
		/// </summary>
		public static int DrawCalls = 0;

		/// <summary>
		/// Checkerboard empty texture<para/>
		/// Текстура для отображения ненайденных текстур
		/// </summary>
		public static int EmptyTexture = 0;

		/// <summary>
		/// 3D rendering queue<para/>
		/// Список 3D объектов для отрисовки
		/// </summary>
		public static Queue<Renderers.RendererBase> RenderQueue = new Queue<Renderers.RendererBase>();

		/// <summary>
		/// Draw single game frame<para/>
		/// Отрисовка одного кадра
		/// </summary>
		public static void RenderFrame() {

			// Clearing variables
			// Очистка переменных
			TrisRendered = 0;
			DrawCalls = 0;

			// Creating empty texture
			// Создание текстуры ошибки
			if (EmptyTexture == 0) {
				GenerateEmptyTex();
			}

			// Processing models and textures
			// Обработка моделей и текстур
			ModelManager.SendComplete();
			TextureManager.SendComplete();

			// Verify camera and set up viewport
			// Подгонка камеры и установка вьюпорта
			CameraManager.Sync();
			CameraManager.SetupViewport();

			// Draw single 3D frame
			// Отрисовка 3D данных
			if (Enable3D) {
				Render3D();
			}

			// Draw single 2D frame
			// Отрисовка 2D данных
			if (Enable2D) {
				Render2D();
			}

			// Processing models and textures
			// Обработка моделей и текстур
			ModelManager.CheckUnused();
			TextureManager.CheckUnused();
		}

		/// <summary>
		/// Render 3D scene<para/>
		/// Отрисовка 3D сцены
		/// </summary>
		static void Render3D() {
			// Setting up camera
			// Подготовка камеры
			CameraManager.Setup3D();

			// Render opaque objects, saving transparent for later use
			// Отрисовка непрозрачных объектов с сохранением прозрачных
			List<Renderers.RendererBase> transMeshes = new List<Renderers.RendererBase>();
			
			// Enabling states
			// Активация стейтов
			GL.Enable(EnableCap.Texture2D);

			// Rendering sky
			// Отрисовка неба
			GL.DepthMask(false);
			Renderers.SkyRenderer.Render();
			GL.DepthMask(true);
			GL.Enable(EnableCap.DepthTest);

			// Rendering or storing surfaces
			// Отрисовка или сохранение сурфейсов
			if (RenderQueue.Count > 0) {
				Renderers.RendererBase r = RenderQueue.Dequeue();
				while (true) {
					if (r.IsVisible()) {
						r.Render(false);
					}
					if (r.IsAlphaBlended()) {
						transMeshes.Add(r);
					}
					if (RenderQueue.Count>0) {
						r = RenderQueue.Dequeue();
					}else{
						break;
					}
				}
			}

			// Rendering transparent meshes
			// Отрисовка прозрачных мешей
			if (transMeshes.Count > 0) {
				switch (TransparencyRenderingMode) {
					// Default VC mode
					// Обычный режим, как в оригинале
					case TransparencyRendering.Classic:
						ClassicTransparencyRender(transMeshes);
						break;
				}
			}
			
			// Disabling states
			// Отключение стейтов
			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Texture2D);
		}

		/// <summary>
		/// Render 2D stuff<para/>
		/// Отрисовка 2D
		/// </summary>
		static void Render2D() {

			// Setting up camera
			// Подготовка камеры
			CameraManager.Setup2D();

		}

		/// <summary>
		/// Drawing transparency using alphatest<para/>
		/// Отрисовка прозрачности через AlphaTest
		/// </summary>
		static void ClassicTransparencyRender(List<Renderers.RendererBase> meshes) {
			GL.Enable(EnableCap.AlphaTest);
			GL.Enable(EnableCap.Blend);

			GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			foreach (Renderers.RendererBase tr in meshes) {
				if (tr.IsVisible()) tr.Render(true);
			}

			GL.Disable(EnableCap.Blend);
			GL.Disable(EnableCap.AlphaTest);
		}





		/// <summary>
		/// Creating red checkerboard tex<para/>
		/// Создание пустой текстуры
		/// </summary>
		static void GenerateEmptyTex() {
			// Generate colormap
			// Создание массива цветов
			/*
			byte[] data = new byte[128*128*4];
			for (int y = 0; y < 128; y++) {
				bool col = ((y/16) % 2) == 1;
				for (int x = 0; x < 128; x++) {
					if ((x%16) == 0) {
						col = !col;
					}

					int idx = (128 * y + x) * 4;
					if (col) {
						data[idx + 0] = 255;
						data[idx + 1] = 128;
						data[idx + 2] = 0;
						data[idx + 3] = 255;
					}else{
						data[idx + 0] = 255;
						data[idx + 1] = 200;
						data[idx + 2] = 50;
						data[idx + 3] = 128;
					}

				}
			}*/
			byte[] data = new byte[] { 255, 255, 255, 255 };

			// Sending to GL
			// Отправка в GL
			GL.Enable(EnableCap.Texture2D);
			EmptyTexture = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, EmptyTexture);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, 1, 1, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			GL.Disable(EnableCap.Texture2D);
		}

		/// <summary>
		/// Transparency rendering modes<para/>
		/// Режимы отрисовки прозрачности
		/// </summary>
		public enum TransparencyRendering {
			
			/// <summary>
			/// Classic VC mode, using AlphaTesting<para/>
			/// Классический режим, используя AlphaTesting
			/// </summary>
			Classic,

		}

	}
}
