using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenVice.Graphics.Shaders {
	/// <summary>
	/// Debugging shader<para/>
	/// Шейдер для дебага
	/// </summary>
	public class DebugShader : ShaderBase {
		/// <summary>
		/// Internal shader object<para/>
		/// Внутренний объект шейдера
		/// </summary>
		static DebugShader shader;

		/// <summary>
		/// Shader object access field<para/>
		/// Поле доступа к объекту шейдера
		/// </summary>
		public static DebugShader Shader {
			get {
				if (shader == null) {
					shader = new DebugShader();
					shader.CompileShader();
				}
				return shader;
			}
		}

		/// <summary>
		/// Seek associated uniforms<para/>
		/// Поиск ассоциированных униформов
		/// </summary>
		protected override void SeekUniforms() {
			GL.UseProgram(glprog);
			ProjectionMatrix = GL.GetUniformLocation(glprog, "projMatrix");
			ModelViewMatrix = GL.GetUniformLocation(glprog, "modelMatrix");
			ParentMatrix = GL.GetUniformLocation(glprog, "parentMatrix");
			ChildMatrix = GL.GetUniformLocation(glprog, "childMatrix");
			Color = GL.GetUniformLocation(glprog, "color");
			GL.UseProgram(0);
		}

		/// <summary>
		/// Get fragment shader code<para/>
		/// Получение кода фрагментного шейдера
		/// </summary>
		protected override string GetFragmentCode() {
			return fragmentProg;
		}

		/// <summary>
		/// Get vertex shader code<para/>
		/// Получение кода вершинного шейдера
		/// </summary>
		protected override string GetVertexCode() {
			return vertexProg;
		}

		/// <summary>
		/// Camera projection matrix<para/>
		/// Матрица проекции камеры
		/// </summary>
		public static int ProjectionMatrix { get; private set; }

		/// <summary>
		/// Camera position matrix<para/>
		/// Матрица расположения камеры
		/// </summary>
		public static int ModelViewMatrix { get; private set; }

		/// <summary>
		/// Parent position matrix<para/>
		/// Матрица расположения родителя
		/// </summary>
		public static int ParentMatrix { get; private set; }

		/// <summary>
		/// Child position matrix<para/>
		/// Матрица расположения
		/// </summary>
		public static int ChildMatrix { get; private set; }

		/// <summary>
		/// Line color<para/>
		/// Цвет линии
		/// </summary>
		public static int Color { get; set; }

		/// <summary>
		/// Vertex program for this shader<para/>
		/// Вершинная программа для этого шейдера
		/// </summary>
		static string vertexProg = @"
			// Basic uniforms list
			// Список юниформов
			uniform mat4 projMatrix;
			uniform mat4 modelMatrix;
			uniform mat4 parentMatrix;
			uniform mat4 childMatrix;
			
			// Processing vertex
			// Обработка вершины
			void main() {
				mat4 completeMat = projMatrix * modelMatrix * parentMatrix * childMatrix;
				gl_Position = completeMat * gl_Vertex;
			}
		";

		/// <summary>
		/// Fragment program for this shader<para/>
		/// Фрагментная программа для этого шейдера
		/// </summary>
		static string fragmentProg = @"
			// Fragment color
			// Цвет фрагмента
			uniform vec3 color;

			// Processing fragment
			// Обработка фрагмента
			void main() {
				gl_FragColor = vec4(color, 1.0);
			}
		";

	}
}
