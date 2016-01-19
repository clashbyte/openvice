using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenVice.Entities;
using OpenTK.Graphics.OpenGL;

namespace OpenVice.Graphics {

	/// <summary>
	/// Internal class for GL camera setup<para/>
	/// Внутренний класс для обработки GL-камеры
	/// </summary>
	public static class CameraManager {

		/// <summary>
		/// Camera near clipplane distance<para/>
		/// Расположение ближнего предела камеры
		/// </summary>
		const float CameraNear = 0.5f;

		/// <summary>
		/// Flag that position matrix need to be rebuilt<para/>
		/// Нуждается ли матрица позиции камеры в пересчёте
		/// </summary>
		static bool needModelRebuild = true;

		/// <summary>
		/// Flag that projection matrix need to be rebuilt<para/>
		/// Нуждается ли матрица проекции камеры в пересчёте
		/// </summary>
		static bool needViewRebuild = true;

		/// <summary>
		/// Flag that ortho matrix need to be rebuilt<para/>
		/// Нуждается ли ортогональная матрица камеры в пересчёте
		/// </summary>
		static bool needOrthoRebuild = true;

		/// <summary>
		/// Camera GL perspective projection matrix<para/>
		/// GL-матрица перспективы камеры
		/// </summary>
		static Matrix4 projMatrix;

		/// <summary>
		/// Camera GL orthographic projection matrix<para/>
		/// GL-матрица ортографии камеры
		/// </summary>
		static Matrix4 orthoMatrix;

		/// <summary>
		/// Camera GL modelview matrix (straight)<para/>
		/// GL-матрица позиции камеры (обычная)
		/// </summary>
		static Matrix4 modelMatrix;

		/// <summary>
		/// Camera GL modelview matrix (inverted)<para/>
		/// GL-матрица позиции камеры (инвертированная)
		/// </summary>
		static Matrix4 modelInvMatrix;

		/// <summary>
		/// Camera GL sky matrix (no transition)<para/>
		/// GL-матрица неба (без смещения)
		/// </summary>
		static Matrix4 skyMatrix;

		/// <summary>
		/// Cached position value in 3D space<para/>
		/// Кешированное расположение камеры в 3D
		/// </summary>
		static Vector3 position;

		/// <summary>
		/// Cached angular value in 3D space<para/>
		/// Кешированный поворот камеры в 3D
		/// </summary>
		static Vector3 angles;

		/// <summary>
		/// Cached viewport size<para/>
		/// Кешированный размер вьюпорта
		/// </summary>
		static Vector2 size;

		/// <summary>
		/// Cached camera zoom<para/>
		/// Кешированное увеличение камеры
		/// </summary>
		static float zoom;

		/// <summary>
		/// Range where the camera ends rasterizing tris<para/>
		/// Расстояние, после которого камера не рисует треугольники
		/// </summary>
		static float farClip;

		/// <summary>
		/// Synchronize camera and viewport variables<para/>
		/// Синхронизация переменных камеры и вьюпорта
		/// </summary>
		public static void Sync() {
			
			// Camera orientation
			// Расположение камеры
			if (position != Camera.Position || angles != Camera.Angles) {
				position = Camera.Position;
				angles = Camera.Angles;
				needModelRebuild = true;
			}

			// Camera viewport
			// Вьюпорт камеры
			if (Viewport.Size != size) {
				size = Viewport.Size;
				needOrthoRebuild = true;
				needViewRebuild = true;
			}

			// Camera zoom
			// Увеличение камеры
			if (Camera.Zoom != zoom || Camera.FarClip != farClip) {
				zoom = Camera.Zoom;
				farClip = Camera.FarClip;
				needViewRebuild = true;
			}

		}

		/// <summary>
		/// Set viewport size and clear it<para/>
		/// Установка размера вьюпорта
		/// </summary>
		public static void SetupViewport() {

			// Adjust size
			// Установка размера
			GL.Viewport(0, 0, (int)size.X, (int)size.Y);

			// TODO: Remove
			GL.ClearColor(0.2f, 0.2f, 0.2f, 1f);
			GL.Clear(ClearBufferMask.ColorBufferBit);

		}

		/// <summary>
		/// Setting up 3D matrices<para/>
		/// Подготовка матриц проекции
		/// </summary>
		public static void Setup3D() {

			// Rebuild modelview
			// Пересчёт матрицы расположения
			if (needModelRebuild) {
				modelMatrix =
					Matrix4.CreateRotationZ(-angles.Z * 0.0174f) * 
					Matrix4.CreateRotationX(-angles.X * 0.0174f) * 
					Matrix4.CreateRotationY(-angles.Y * 0.0174f) * 
					Matrix4.CreateTranslation(position.X, position.Y, -position.Z);
				modelInvMatrix = modelMatrix.Inverted();
				skyMatrix = modelInvMatrix.ClearTranslation();
				needModelRebuild = false;
			}

			// Rebuild projection
			// Пересчёт матрицы проекции
			if (needViewRebuild) {
				projMatrix =
					Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3f / zoom, size.X/size.Y, CameraNear, farClip);
				needViewRebuild = false;
			}

			// Clearing depth
			// Очистка глубины
			GL.Clear(ClearBufferMask.DepthBufferBit);

			// Update frustum
			// Обновление пирамиды отсечения
			Frustum.Update(projMatrix, modelInvMatrix);
		}

		/// <summary>
		/// Setting up 2D matrices<para/>
		/// Подготовка ортогональных матриц
		/// </summary>
		public static void Setup2D() {

			// Rebuild ortho
			// Пересчёт матрицы ортографии
			if (needOrthoRebuild) {
				float hsize = size.X / size.Y * 300f;
				orthoMatrix = Matrix4.CreateOrthographicOffCenter(-hsize, hsize, 300f, -300f, -1, 100);
				needOrthoRebuild = false;
			}

			// Clearing depth
			// Очистка глубины
			GL.Clear(ClearBufferMask.DepthBufferBit);

		}

		/// <summary>
		/// Send 3D camera matrices to shader<para/>
		/// Отправка 3D матриц камеры в шейдер 
		/// </summary>
		/// <param name="projectionLocation">Projection matrix uniform location<para/>Расположение юниформа матрицы проекции</param>
		/// <param name="modelLocation">Modelview matrix uniform location<para/>Расположение юниформа матрицы расположения</param>
		public static void Bind3DUniforms(int projectionLocation, int modelLocation) {
			GL.UniformMatrix4(projectionLocation, false, ref projMatrix);
			GL.UniformMatrix4(modelLocation, false, ref modelInvMatrix);
		}

		/// <summary>
		/// Set 3D camera sky matrices to shader<para/>
		/// Отправка 3D-матриц неба в шейдер
		/// </summary>
		/// <param name="projectionLocation">Projection matrix uniform location<para/>Расположение юниформа матрицы проекции</param>
		/// <param name="modelLocation">Modelview matrix uniform location<para/>Расположение юниформа матрицы расположения</param>
		public static void BindSkyUniforms(int projectionLocation, int modelLocation) {
			GL.UniformMatrix4(projectionLocation, false, ref projMatrix);
			GL.UniformMatrix4(modelLocation, false, ref skyMatrix);
		}

		/// <summary>
		/// Returns special camera range, for fog shaders<para/>
		/// Возвращает специальное значение дальности, для шейдеров с туманом
		/// </summary>
		public static float Range() {
			return farClip - CameraNear;
		}
	}
}
