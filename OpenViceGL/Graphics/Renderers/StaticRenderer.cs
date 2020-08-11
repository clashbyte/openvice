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
		/// Force alphablended fading<para/>
		/// Принудительное альфа-смешивание
		/// </summary>
		public bool Fading { get; set; }

		/// <summary>
		/// Fading opacity value<para/>
		/// Значение непрозрачности для фейдинга
		/// </summary>
		public float FadingDelta { get; set; }

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
			GL.Uniform1(StaticShader.CameraRange, CameraManager.Range());
			GL.Uniform1(StaticShader.FogRange, Renderer.SkyState.FogDistance);
			GL.Uniform3(StaticShader.FogColor, Renderer.SkyState.SkyBottom);

			// Rendering surfaces
			// Отрисовка поверхностей
			if (Fading) {
				if (transparentPass) {
					SubMesh.Render(this, Textures, transparentPass, true);
				}
			}else{
				SubMesh.Render(this, Textures, transparentPass);
			}
			

		}

		/// <summary>
		/// Setup uniforms for specified surface<para/>
		/// Установка параметров шейдера для поверхности
		/// </summary>
		/// <param name="surf">Surface<para/>Поверхность</param>
		/// <param name="mat">Material<para/>Материал поверхности</param>
		public override void SetupSurface(Model.SubMesh.Surface surf, Model.Material mat, ModelFile.Geometry geom, Model.SubMesh subMesh) {
			GL.Uniform1(StaticShader.AppearValue, Fading ? FadingDelta : 1f);
			GL.Uniform3(StaticShader.AmbientColor, Renderer.SkyState.AmbientStatic);
			GL.Uniform3(StaticShader.DiffuseColor, Renderer.SkyState.DirectLight);
			GL.Uniform4(StaticShader.TintColor, new Color4(mat.Color[0], mat.Color[1], mat.Color[2], mat.Color[3]));
		}

		/// <summary>
		/// Check for transparent surfaces<para/>
		/// Проверка на прозрачные поверхности
		/// </summary>
		/// <returns>True if mesh contains them<para/>True, если они есть</returns>
		public override bool IsAlphaBlended() {
			if (Fading && FadingDelta<1f) {
				return true;
			}
			return base.IsAlphaBlended();
		}

	}
}
