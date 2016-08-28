using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenVice.Graphics.Shaders;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenVice.Graphics.Renderers {
	
	/// <summary>
	/// Debugging-related renderer (draws cube)<para/>
	/// Отладочный шейдер (рисует куб)
	/// </summary>
	public class DebugRenderer : RendererBase {

		/// <summary>
		/// Position matrix<para/>
		/// Матрица расположения 
		/// </summary>
		public Matrix4 Matrix { get; set; }

		/// <summary>
		/// List of all primitives<para/>
		/// Список всех примитивов
		/// </summary>
		public List<Primitive> Primitives { get; private set; }

		/// <summary>
		/// Flag that all lines are opaque<para/>
		/// Флаг что все линии непрозрачные
		/// </summary>
		public override bool IsAlphaBlended() {
			return false;
		}

		/// <summary>
		/// Surface is always visible<para/>
		/// Поверхность всегда видна
		/// </summary>
		public override bool IsVisible() {
			return true;
		}

		/// <summary>
		/// Disable surface processing<para/>
		/// Отключение обработки поверхностей
		/// </summary>
		public override void SetupSurface(Model.SubMesh.Surface surf, Model.Material mat, Files.ModelFile.Geometry geom) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Render all the primitives<para/>
		/// Отрисовка примитивов
		/// </summary>
		public override void Render(bool transparentPass) {
			if (!transparentPass) {
				
				// Enable states
				// Включение стейтов
				GL.EnableClientState(ArrayCap.VertexArray);

				// Enable shader
				// Включение шейдера
				Matrix4 m = Matrix;
				DebugShader.Shader.Bind();
				CameraManager.Bind3DUniforms(DebugShader.ProjectionMatrix, DebugShader.ModelViewMatrix);
				GL.UniformMatrix4(DebugShader.ParentMatrix, false, ref m);

				// Render one-by-one
				// Отрисовка по одному
				foreach (Primitive p in Primitives) {
					p.Render();
				}

				// Disable states
				// Отключение стейтов
				GL.DisableClientState(ArrayCap.VertexArray);
				GL.LineWidth(0.5f);
			}
		}

		/// <summary>
		/// Initialize basic fields<para/>
		/// Инициализация внутренних полей
		/// </summary>
		public DebugRenderer() {
			Matrix = Matrix4.Identity;
			Primitives = new List<Primitive>();
		}

		/// <summary>
		/// Single wire primitive<para/>
		/// Одна примитивная фигура
		/// </summary>
		public abstract class Primitive {
			/// <summary>
			/// Position matrix<para/>
			/// Матрица расположения
			/// </summary>
			public Matrix4 Matrix = Matrix4.Identity;

			/// <summary>
			/// Color of this primitive<para/>
			/// Цвет примитива
			/// </summary>
			public Vector3 Color = Vector3.UnitY;

			/// <summary>
			/// Line size for drawing<para/>
			/// Размер линии при отрисовке
			/// </summary>
			public float LineSize = 0.5f;

			/// <summary>
			/// Render this primitive<para/>
			/// Отрисовка данного примитива
			/// </summary>
			public abstract void Render();
		}

		/// <summary>
		/// Line primitive<para/>
		/// Примитив коробки
		/// </summary>
		public class Line : Primitive {

			/// <summary>
			/// Start and end points<para/>
			/// Точки начала и окончания
			/// </summary>
			public Vector3 Start, End;

			public override void Render() {
				// Setting uniforms
				Matrix4 m = Matrix4.Identity;
				GL.LineWidth(LineSize);
				GL.UniformMatrix4(DebugShader.ChildMatrix, false, ref m);
				GL.Uniform3(DebugShader.Color, Color);

				// Rendering
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				GL.Begin(PrimitiveType.Lines);
				GL.Vertex3(Start); GL.Vertex3(End);
				GL.End();
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
		}

		/// <summary>
		/// Box primitive<para/>
		/// Коробка
		/// </summary>
		public class Box : Primitive {
			/// <summary>
			/// Box boundaries<para/>
			/// Размер коробки
			/// </summary>
			public Vector3 Size;

			// GL-side data
			// Данные для GL
			static float[] pos;
			static ushort[] idx;

			/// <summary>
			/// Render box<para/>
			/// Отрисовка коробки
			/// </summary>
			public override void Render() {
				
				// Generate mesh data
				// Создание данных меша
				if (idx == null) {
					InitBuffers();
				}

				// Prepare final matrix
				// Создание финальной матрицы
				Matrix4 m = Matrix4.CreateScale(Size) * Matrix;

				// Setting uniforms
				GL.LineWidth(LineSize);
				GL.UniformMatrix4(DebugShader.ChildMatrix, false, ref m);
				GL.Uniform3(DebugShader.Color, Color);

				// Rendering
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				GL.VertexPointer(3, VertexPointerType.Float, 0, pos);
				GL.DrawElements(PrimitiveType.Quads, idx.Length, DrawElementsType.UnsignedShort, idx);
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}

			/// <summary>
			/// Generate cube vertex data<para/>
			/// Создание данных куба
			/// </summary>
			static void InitBuffers() {

				// Vertices
				// Вершины
				pos = new float[] {
					-1f, 1f, -1f, 1f, 1f, -1f,
					-1f, -1f, -1f, 1f, -1f, -1f,
					-1f, 1f, 1f, 1f, 1f, 1f,
					-1f, -1f, 1f, 1f, -1f, 1f
				};

				// Indices
				// Индексы
				idx = new ushort[] {
					0, 1, 3, 2,
					4, 5, 7, 6,
					0, 1, 5, 4,
					2, 3, 7, 6,
					0, 4, 6, 2,
					5, 1, 3, 7
				};
			}

		}
	}
}
