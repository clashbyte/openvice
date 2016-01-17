using OpenTK.Graphics.OpenGL;
using OpenVice.Graphics.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Graphics.Renderers {
	
	/// <summary>
	/// Skysphere renderer<para/>
	/// Отрисовщик сферы неба
	/// </summary>
	public static class SkyRenderer {

		/// <summary>
		/// GL vertex buffer handle<para/>
		/// Вершинный буффер для OpenGL
		/// </summary>
		static int vertexBuffer;

		/// <summary>
		/// GL index buffer handle<para/>
		/// Индексный буффер для OpenGL
		/// </summary>
		static int indexBuffer;

		/// <summary>
		/// Number of indices in index array<para/>
		/// Количество индексов в индексном буффере
		/// </summary>
		static int indexCount;

		/// <summary>
		/// Render sky sphere<para/>
		/// Отрисовка небесной сферы
		/// </summary>
		public static void Render() {

			// Generate vertex data
			// Генерация вершинных данных
			if (indexCount==0) {
				InitMesh();
			}

			// Bind shader
			// Установка шейдера
			SkyShader.Shader.Bind();
			CameraManager.BindSkyUniforms(SkyShader.ProjectionMatrix, SkyShader.ModelViewMatrix);
			GL.Uniform3(SkyShader.TopColor, Renderer.SkyState.SkyTop);
			GL.Uniform3(SkyShader.BottomColor, Renderer.SkyState.SkyBottom);

			// Drawing quads
			// Отрисовка квадов
			GL.EnableClientState(ArrayCap.VertexArray);
			
			// Vertices
			// Вертексы
			GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
			GL.VertexPointer(3, VertexPointerType.Float, 0, IntPtr.Zero);

			// Indices
			// Индексы
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
			GL.DrawElements(PrimitiveType.Quads, indexCount, DrawElementsType.UnsignedShort, IntPtr.Zero);

			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.DisableClientState(ArrayCap.VertexArray);
		}

		/// <summary>
		/// Initialize GL buffers if they not exist<para/>
		/// Инициализация буфферов GL если они не существуют
		/// </summary>
		static void InitMesh() {
			int numRings = 12;
			int numSplit = 24;

			float stepRing = 1f / (float)(numRings - 1);
			float stepSector = 1f / (float)(numSplit - 1);
			float[] pos = new float[numRings * numSplit * 3];
			ushort[] ind = new ushort[numRings * numSplit * 4];
			indexCount = ind.Length;
			for (int r = 0; r < numRings; r++) {
				for (int s = 0; s < numSplit; s++) {
					int idx = (r * numSplit + s) * 3;
					pos[idx + 0] = (float)(Math.Cos(2 * Math.PI * stepSector * s) * Math.Sin(Math.PI * stepRing * r));
					pos[idx + 1] = (float)(Math.Sin(-Math.PI / 2f + Math.PI * stepRing * r));
					pos[idx + 2] = (float)(Math.Sin(2 * Math.PI * stepSector * s) * Math.Sin(Math.PI * stepRing * r));
				}
			}
			for (int r = 0; r < numRings - 1; r++) {
				for (int s = 0; s < numSplit - 1; s++) {
					int idx = (r * numSplit + s) * 4;
					ind[idx + 0] = (ushort)(r * numSplit + s);
					ind[idx + 1] = (ushort)(r * numSplit + (s + 1));
					ind[idx + 2] = (ushort)((r + 1) * numSplit + (s + 1));
					ind[idx + 3] = (ushort)((r + 1) * numSplit + s);
				}
			}

			// Initialize buffers
			vertexBuffer = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
			GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(pos.Length * 4), pos, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

			indexBuffer = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
			GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(ind.Length * 2), ind, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}
	}
}
