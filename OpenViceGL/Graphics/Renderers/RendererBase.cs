using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenVice.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Graphics.Renderers {

	/// <summary>
	/// Single rendering event per frame<para/>
	/// Одно событие рендера сурфейса для кадра
	/// </summary>
	public abstract class RendererBase {
		
		/// <summary>
		/// Base parent matrix<para/>
		/// Основная матрица родителя
		/// </summary>
		public Matrix4 BaseMatrix {
			get {
				return baseMatrix;
			}
			set {
				baseMatrix = value;
				needNewMatrix = true;
			}
		}

		/// <summary>
		/// Submesh matrix<para/>
		/// Матрица подмеша
		/// </summary>
		public Matrix4 SubmeshMatrix {
			get {
				return meshMatrix;
			}
			set {
				meshMatrix = value;
				needNewMatrix = true;
			}
		}

		/// <summary>
		/// Single submesh to render<para/>
		/// Подмеш для отрисовки
		/// </summary>
		public Model.SubMesh SubMesh { get; set; }

		/// <summary>
		/// Texture list to render mesh with<para/>
		/// Текстурный архив для меша
		/// </summary>
		public TextureDictionary Textures { get; set; }

		/// <summary>
		/// Culling sphere<para/>
		/// Сфера отсечения
		/// </summary>
		public CullSphere Sphere {
			get {
				return cullSphere;
			}
		}

		// Hidden values
		// Скрытые значения
		protected Matrix4 baseMatrix = Matrix4.Identity;
		protected Matrix4 meshMatrix = Matrix4.Identity;
		protected Matrix4 completeMatrix = Matrix4.Identity;
		protected CullSphere cullSphere = null;
		protected bool needNewMatrix = false;
		protected bool needNewSphere = true;

		/// <summary>
		/// Rendering this submesh<para/>
		/// Отрисовка этого сабмеша
		/// </summary>
		/// <param name="transparentPass">Is current pass alpha-blended<para/>Текущий проход в режиме прозрачности</param>
		public abstract void Render(bool transparentPass);

		/// <summary>
		/// Setup uniforms for specified surface<para/>
		/// Установка параметров шейдера для поверхности
		/// </summary>
		/// <param name="surf">Surface<para/>Поверхность</param>
		/// <param name="mat">Material<para/>Материал поверхности</param>
		public abstract void SetupSurface(Model.SubMesh.Surface surf, Model.Material mat, ModelFile.Geometry geom);

		/// <summary>
		/// Mesh must be queued for alphasorting<para/>
		/// Меш должен подвергнуться альфасортировке
		/// </summary>
		/// <returns>Mesh is not opaque<para/>Меш имеет прозрачные части</returns>
		public virtual bool IsAlphaBlended() {
			if (SubMesh!=null) {
				foreach (Model.SubMesh.Surface srf in SubMesh.Surfaces) {
					if (srf.Material.HasAlpha) {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Check for submesh visibility<para/>
		/// Проверка на видимость меша
		/// </summary>
		/// <returns>True if mesh is visible<para/>True если меш попадает в камеру</returns>
		public virtual bool IsVisible() {
			PrepareSphere();
			if (Sphere!=null) {
				return Sphere.Visible();
			}
			return true;
		}

		/// <summary>
		/// Generate new matrix if needed<para/>
		/// Генерация новой матрицы, если требуется
		/// </summary>
		protected void PrepareMatrix() {
			if (needNewMatrix) {
				completeMatrix = meshMatrix * baseMatrix;
				needNewMatrix = false;
			}
		}

		/// <summary>
		/// Generate new culling sphere if needed<para/>
		/// Генерация новой сферы отсечения, если требуется
		/// </summary>
		protected void PrepareSphere() {
			if (needNewSphere && SubMesh!=null) {
				PrepareMatrix();
				cullSphere = SubMesh.GetSphere(completeMatrix);
				needNewSphere = false;
			}
		}

	}
}
