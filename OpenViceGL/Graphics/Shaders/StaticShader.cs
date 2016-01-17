using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Graphics.Shaders {
	
	/// <summary>
	/// Shader for static objects<para/>
	/// Шейдер для статичных объектов
	/// </summary>
	public class StaticShader : ShaderBase {

		/// <summary>
		/// Internal shader object<para/>
		/// Внутренний объект шейдера
		/// </summary>
		static StaticShader shader;

		/// <summary>
		/// Shader object access field<para/>
		/// Поле доступа к объекту шейдера
		/// </summary>
		public static StaticShader Shader {
			get {
				if (shader == null) {
					shader = new StaticShader();
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
			ObjectMatrix = GL.GetUniformLocation(glprog, "objectMatrix");
			TintColor = GL.GetUniformLocation(glprog, "tintColor");
			AmbientColor = GL.GetUniformLocation(glprog, "ambientColor");
			DiffuseColor = GL.GetUniformLocation(glprog, "diffuseColor");
			AmbientValue = GL.GetUniformLocation(glprog, "ambient");
			DiffuseValue = GL.GetUniformLocation(glprog, "diffuse");
			SpecularValue = GL.GetUniformLocation(glprog, "specular");
			GL.Uniform1(GL.GetUniformLocation(glprog, "texture"), 0);
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
		/// Object matrix<para/>
		/// Матрица объекта
		/// </summary>
		public static int ObjectMatrix { get; private set; }

		/// <summary>
		/// Surface tint color<para/>
		/// Цвет поверхности
		/// </summary>
		public static int TintColor { get; private set; }

		/// <summary>
		/// Diffuse color<para/>
		/// Цвет освещённости
		/// </summary>
		public static int DiffuseColor { get; private set; }

		/// <summary>
		/// Ambient color<para/>
		/// Цвет затемнённых участков
		/// </summary>
		public static int AmbientColor { get; private set; }

		/// <summary>
		/// Diffuse color multiplier<para/>
		/// Цвет освещённости
		/// </summary>
		public static int DiffuseValue { get; private set; }

		/// <summary>
		/// Ambient color multiplier<para/>
		/// Цвет затемнённых участков
		/// </summary>
		public static int AmbientValue { get; private set; }

		/// <summary>
		/// Specular color multiplier<para/>
		/// Цвет затемнённых участков
		/// </summary>
		public static int SpecularValue { get; private set; }

		/// <summary>
		/// Vertex program for this shader<para/>
		/// Вершинная программа для этого шейдера
		/// </summary>
		static string vertexProg = @"
			// Basic uniforms list
			// Список юниформов
			uniform mat4 projMatrix;
			uniform mat4 modelMatrix;
			uniform mat4 objectMatrix;
			
			// Texture coords for next pass
			// Текстурные координаты для следующего пасса
			varying vec2 texCoords;
			varying vec4 lerpColor;
			
			// Processing vertex
			// Обработка вершины
			void main() {
				mat4 completeMat = projMatrix * modelMatrix * objectMatrix;
				texCoords = gl_MultiTexCoord0.xy;
				lerpColor = gl_Color;
				gl_Position = completeMat * gl_Vertex;
			}
		";

		/// <summary>
		/// Fragment program for this shader<para/>
		/// Фрагментная программа для этого шейдера
		/// </summary>
		static string fragmentProg = @"
			// Basic uniforms list
			// Список юниформов
			uniform sampler2D texture;
			uniform vec4 tintColor;
			uniform vec3 diffuseColor;
			uniform vec3 ambientColor;
			uniform float ambient;
			uniform float diffuse;
			uniform float specular;
			
			// Texture coords
			// Текстурные координаты
			varying vec2 texCoords;
			varying vec4 lerpColor;

			// Processing fragment
			// Обработка фрагмента
			void main() {
				vec4 litColor = vec4(mix(ambientColor.rgb, vec3(1.0, 1.0, 1.0), lerpColor.rgb), 1.0);
				gl_FragColor = texture2D(texture, texCoords.xy) * litColor * tintColor;
			}
		";

	}

}
