using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Graphics {

	/// <summary>
	/// Single transformation wraparound<para/>
	/// Обертка для трансформаций
	/// </summary>
	public class Transform {

		/// <summary>
		/// Model position in 3D space<para/>
		/// Расположение модели в пространстве
		/// </summary>
		public Vector3 Position {
			get {
				return new Vector3(position.X, position.Y, -position.Z);
			}
			set {
				position = new Vector3(value.X, value.Y, -value.Z);
				needNewMatrix = true;
			}
		}

		/// <summary>
		/// Model rotation in 3D space<para/>
		/// Поворот модели в 3D-пространстве
		/// </summary>
		public Quaternion Angles {
			get {
				return new Quaternion(angles.X, angles.Y, -angles.Z, -angles.W);
			}
			set {
				angles = new Quaternion(value.X, value.Y, -value.Z, -value.W);
				needNewMatrix = true;
			}
		}

		/// <summary>
		/// Model scale in 3D space<para/>
		/// Размер модели в 3D-мире
		/// </summary>
		public Vector3 Scale {
			get {
				return scale;
			}
			set {
				scale = value;
				needNewMatrix = true;
			}
		}

		/// <summary>
		/// Calculated position matrix<para/>
		/// Вычисленная матрица расположения
		/// </summary>
		public Matrix4 Matrix {
			get {
				if (needNewMatrix) {
					mat =
						Matrix4.CreateScale(scale) *
						Matrix4.CreateFromQuaternion(angles) *
						Matrix4.CreateTranslation(position);
					needNewMatrix = false;
				}
				return mat;
			}
		}

		// Hidden GL-oriented values
		// Скрытые, GL-дружелюбные значения
		Vector3 position = Vector3.Zero;
		Quaternion angles = Quaternion.Identity;
		Vector3 scale = Vector3.One;
		Matrix4 mat = Matrix4.Identity;
		bool needNewMatrix = true;

	}
}
