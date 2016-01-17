using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenVice.Graphics.Shaders;
using OpenTK.Graphics;
using OpenVice.Files;

namespace OpenVice.Graphics.Renderers {
	
	/// <summary>
	/// Static mesh renderer<para/>
	/// Рендерер для статичных мешей
	/// </summary>
	public class StaticRenderer : RendererBase {

		/// <summary>
		/// Diffusion color<para/>
		/// Диффузный цвет
		/// </summary>
		public static Vector4 DiffuseColor { get; set; }

		/// <summary>
		/// Ambient color<para/>
		/// Амбиентный цвет
		/// </summary>
		public static Vector4 AmbientColor { get; set; }

		/// <summary>
		/// Render single object pass<para/>
		/// Отрисовка одного прохода объекта
		/// </summary>
		/// <param name="transparentPass">True if pass is transparent<para/>True если проход прозрачный</param>
		public override void Render(bool transparentPass) {
			
			// Prepare rendering matrix
			// Установка матрицы рендера
			PrepareMatrix();

			// Prepare shader
			// Установка шейдера
			StaticShader.Shader.Bind();
			CameraManager.Bind3DUniforms(StaticShader.ProjectionMatrix, StaticShader.ModelViewMatrix);
			GL.UniformMatrix4(StaticShader.ObjectMatrix, false, ref completeMatrix);
			


			// Rendering surfaces
			// Отрисовка поверхностей
			SubMesh.Render(this, Textures, transparentPass);

		}

		/// <summary>
		/// Setup uniforms for specified surface<para/>
		/// Установка параметров шейдера для поверхности
		/// </summary>
		/// <param name="surf">Surface<para/>Поверхность</param>
		/// <param name="mat">Material<para/>Материал поверхности</param>
		public override void SetupSurface(Model.SubMesh.Surface surf, Model.Material mat, ModelFile.Geometry geom) {
			GL.Uniform1(StaticShader.AmbientValue, mat.Props[0]);
			GL.Uniform1(StaticShader.DiffuseValue, mat.Props[1]);
			GL.Uniform1(StaticShader.SpecularValue, mat.Props[2]);
			GL.Uniform3(StaticShader.AmbientColor, Renderer.SkyState.AmbientStatic);
			GL.Uniform3(StaticShader.DiffuseColor, Renderer.SkyState.DiffuseStatic);
			GL.Uniform4(StaticShader.TintColor, new Color4(mat.Color[0], mat.Color[1], mat.Color[2], mat.Color[3]));
		}

	}
}
