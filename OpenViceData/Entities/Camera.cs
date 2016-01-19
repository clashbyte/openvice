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

	}
}
