using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Graphics {
	
	/// <summary>
	/// Class that handles culling sphere<para/>
	/// Класс, обрабатывающий сферу отсечения
	/// </summary>
	public class CullSphere {

		/// <summary>
		/// Position in world<para/>
		/// Расположение на карте
		/// </summary>
		Vector3 pos;

		/// <summary>
		/// Sphere radius<para/>
		/// Радиус сферы
		/// </summary>
		float radius;

		/// <summary>
		/// Create CullSphere from data and matrix<para/>
		/// Создание сферы из данных и матрицы
		/// </summary>
		/// <param name="position">Sphere position in local space<para/>Расположение в локальных координатах</param>
		/// <param name="rad">Sphere radius<para/>Радиус сферы</param>
		/// <param name="transform">Matrix<para/>Матрица</param>
		public CullSphere(Vector3 position, float rad, Matrix4 transform) {
			radius = 1f * rad;
			pos = Vector3.TransformPosition(new Vector3(position.X, position.Y, -position.Z), transform);
			//pos.Z = -pos.Z;
		}

		/// <summary>
		/// Check if sphere is visible<para/>
		/// Видна ли сфера камере
		/// </summary>
		/// <returns>True if sphere is visible<para/>True если сфера видна камере</returns>
		public bool Visible() {
			return Frustum.ContainsSphere(pos, radius);
		}
	}
}
