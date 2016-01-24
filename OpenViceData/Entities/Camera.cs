using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenVice.Entities {

	/// <summary>
	/// Main camera class<para/>
	/// Основной класс камеры
	/// </summary>
	public static class Camera {

		/// <summary>
		/// Camera position in 3D space<para/>
		/// Расположение камеры в сцене
		/// </summary>
		public static Vector3 Position { get; set; }

		/// <summary>
		/// Camera angles (X - pitch, Y - yaw, Z - roll)<para/>
		/// Углы камеры (X - pitch, Y - yaw, Z - roll)
		/// </summary>
		public static Vector3 Angles { get; set; }

		/// <summary>
		/// Camera zoom<para/>
		/// Увеличение камеры
		/// </summary>
		public static float Zoom { get; set; }

		/// <summary>
		/// Range where the camera ends rasterizing tris<para/>
		/// Расстояние, после которого камера не рисует треугольники
		/// </summary>
		public static float FarClip { get; set; }

		/// <summary>
		/// Transform vector from camera local space to global<para/>
		/// Перевод вектора из системы координат камеры в глобальные
		/// </summary>
		/// <param name="vec">Vector to transform<para/>Переводимый вектор</param>
		public static Vector3 TransformDirection(Vector3 vec) {
			Matrix4 rot = Matrix4.CreateRotationZ(Camera.Angles.Z * 0.0174f) *
					Matrix4.CreateRotationX(Camera.Angles.X * 0.0174f) *
					Matrix4.CreateRotationY(Camera.Angles.Y * 0.0174f);
			return Vector3.TransformVector(vec, rot);
		}

	}
}
