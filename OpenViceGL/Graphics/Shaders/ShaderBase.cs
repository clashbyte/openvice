using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenVice.Graphics.Shaders {

	/// <summary>
	/// Class that handles GLSL shader programs<para/>
	/// Класс, который обрабатывает GLSL-программы
	/// </summary>
	public abstract class ShaderBase {
		/// <summary>
		/// OpenGL Program identifier<para/>
		/// Идентификатор GL-программы
		/// </summary>
		protected int glprog;

		/// <summary>
		/// Create GL program from shader sources<para/>
		/// Создание GL-программы из исходников
		/// </summary>
		protected void CompileShader() {
			// Initialize shaders
			// Инициализация шейдеров
			int frpr = GL.CreateShader(ShaderType.FragmentShader);
			int vrpr = GL.CreateShader(ShaderType.VertexShader);
			int compstatus = 0;

			// Sending shader sources
			// Отправка исходных данных шейдеров
			GL.ShaderSource(frpr, GetFragmentCode());
			GL.ShaderSource(vrpr, GetVertexCode());
			
			// Compiling fragment program
			// Компиляция фрагментного шейдера
			GL.CompileShader(frpr);
			GL.GetShader(frpr, ShaderParameter.CompileStatus, out compstatus);
			if (compstatus == 0) {
				throw new Exception("[ShaderBase] Fragment program compiling error:\n" + GL.GetShaderInfoLog(frpr));
			}

			// Compiling vertex program
			// Компиляция вершинного шейдера
			GL.CompileShader(vrpr);
			GL.GetShader(vrpr, ShaderParameter.CompileStatus, out compstatus);
			if (compstatus == 0) {
				throw new Exception("[ShaderBase] Vertex program compiling error:\n" + GL.GetShaderInfoLog(vrpr));
			}

			// Creating program and attaching shaders
			// Создание программы и присоединение шейдеров
			glprog = GL.CreateProgram();
			GL.AttachShader(glprog, frpr);
			GL.AttachShader(glprog, vrpr);

			// Linking GL program
 			// Линковка шейдерной программы
			int lnkstat = 0;
			GL.LinkProgram(glprog);
			GL.GetProgram(glprog, GetProgramParameterName.LinkStatus, out lnkstat);
			if (lnkstat == 0) {
				throw new Exception("[ShaderBase] Shader program linking error:\n" + GL.GetProgramInfoLog(glprog));
			}

			// Seeking associated uniforms
			// Поиск униформов
			SeekUniforms();
		}

		/// <summary>
		/// Bind GLSL program<para/>
		/// Установка GLSL-программы
		/// </summary>
		public void Bind() {
			GL.UseProgram(glprog);
		}

		/// <summary>
		/// Getting uniform addresses<para/>
		/// Получение адресов юниформов
		/// </summary>
		protected abstract void SeekUniforms();

		/// <summary>
		/// Get fragment shader code<para/>
		/// Получение кода фрагментного шейдера
		/// </summary>
		protected abstract string GetFragmentCode();

		/// <summary>
		/// Get vertex shader code<para/>
		/// Получение кода вершинного шейдера
		/// </summary>
		protected abstract string GetVertexCode();

	}
}
