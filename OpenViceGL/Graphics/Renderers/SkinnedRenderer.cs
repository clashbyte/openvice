using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenVice.Graphics.Shaders;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenVice.Files;

namespace OpenVice.Graphics.Renderers {
	
	/// <summary>
	/// Renderer for skinned meshes<para/>
	/// Отрисовщик для скинненых мешей
	/// </summary>
	public class SkinnedRenderer : RendererBase {

		/// <summary>
		/// Render single object pass<para/>
		/// Отрисовка одного прохода объекта
		/// </summary>
		/// <param name="transparentPass">True if pass is transparent<para/>True если проход прозрачный</param>
		public override void Render(bool transparentPass) {

			// Prepare rendering matrix
			// Установка матрицы рендера
			PrepareMatrix();

			// Recalculating bones
			// Пересчитываем кости
			Matrix4[] bones = new Matrix4[64];
			BuildMatrices(SubMesh.ParentModel.Children, ref bones);


			// Prepare shader
			// Установка шейдера
			SkinnedShader.Shader.Bind();
			CameraManager.Bind3DUniforms(SkinnedShader.ProjectionMatrix, SkinnedShader.ModelViewMatrix);
			GL.UniformMatrix4(SkinnedShader.ObjectMatrix, false, ref completeMatrix);
			//GL.Uniform1(StaticShader.CameraRange, CameraManager.Range());
			//GL.Uniform1(StaticShader.FogRange, Renderer.SkyState.FogDistance);
			//GL.Uniform3(StaticShader.FogColor, Renderer.SkyState.SkyBottom);

			float[] matrixData = new float[bones.Length * 16];
			for (int i = 0; i < bones.Length; i++) {
				matrixData[i * 16 + 00] = bones[i][0, 0];
				matrixData[i * 16 + 01] = bones[i][0, 1];
				matrixData[i * 16 + 02] = bones[i][0, 2];
				matrixData[i * 16 + 03] = bones[i][0, 3];
				matrixData[i * 16 + 04] = bones[i][1, 0];
				matrixData[i * 16 + 05] = bones[i][1, 1];
				matrixData[i * 16 + 06] = bones[i][1, 2];
				matrixData[i * 16 + 07] = bones[i][1, 3];
				matrixData[i * 16 + 08] = bones[i][2, 0];
				matrixData[i * 16 + 09] = bones[i][2, 1];
				matrixData[i * 16 + 10] = bones[i][2, 2];
				matrixData[i * 16 + 11] = bones[i][2, 3];
				matrixData[i * 16 + 12] = bones[i][3, 0];
				matrixData[i * 16 + 13] = bones[i][3, 1];
				matrixData[i * 16 + 14] = bones[i][3, 2];
				matrixData[i * 16 + 15] = bones[i][3, 3];
			}
			GL.UniformMatrix4(SkinnedShader.BoneMatrices, 64, false, matrixData);

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
		public override void SetupSurface(Model.SubMesh.Surface surf, Model.Material mat, ModelFile.Geometry geom, Model.SubMesh subMesh) {
			//GL.Uniform1(StaticShader.AppearValue, Fading ? FadingDelta : 1f);
			//GL.Uniform3(StaticShader.AmbientColor, Renderer.SkyState.AmbientStatic);
			//GL.Uniform3(StaticShader.DiffuseColor, Renderer.SkyState.DirectLight);
			//GL.Uniform4(StaticShader.TintColor, new Color4(mat.Color[0], mat.Color[1], mat.Color[2], mat.Color[3]));

			GL.BindBuffer(BufferTarget.ArrayBuffer, subMesh.BoneIndexBuffer);
			GL.VertexAttribPointer(SkinnedShader.BoneIndexAttrib, 4, VertexAttribPointerType.Byte, false, 0, IntPtr.Zero);
			GL.EnableVertexAttribArray(SkinnedShader.BoneIndexAttrib);
			GL.BindBuffer(BufferTarget.ArrayBuffer, subMesh.BoneWeightBuffer);
			GL.VertexAttribPointer(SkinnedShader.BoneWeightAttrib, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
			GL.EnableVertexAttribArray(SkinnedShader.BoneWeightAttrib);
		}

		/// <summary>
		/// Check for transparent surfaces<para/>
		/// Проверка на прозрачные поверхности
		/// </summary>
		/// <returns>True if mesh contains them<para/>True, если они есть</returns>
		public override bool IsAlphaBlended() {
			return base.IsAlphaBlended();
		}

		/// <summary>
		/// Building matrix array
		/// </summary>
		/// <param name="branches">Bones</param>
		/// <param name="matrices">Matrices</param>
		void BuildMatrices(Model.Branch[] branches, ref Matrix4[] matrices) {
			Matrix4 rot = meshMatrix;
			foreach (Model.Branch branch in branches) {
				matrices[branch.OriginalIndex] = rot.Inverted() * branch.OriginalInvMatrix * branch.Matrix;// branch.Matrix * branch.OriginalInvMatrix;
				if (branch.Children != null) {
					BuildMatrices(branch.Children, ref matrices);
				}
			}
		}

	}
}
