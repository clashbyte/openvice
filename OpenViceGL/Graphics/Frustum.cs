using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Graphics {

	/// <summary>
	/// Class that handles culling frustum<para/>
	/// Класс, который обрабатывает пирамиду отсечения
	/// </summary>
	public static class Frustum {
		
		/// <summary>
		/// Internal frustum values<para/>
		/// Внутренния значения фрустума
		/// </summary>
		static float[,] frustum = new float[6, 4];

		/// <summary>
		/// Update frustum<para/>
		/// Обновление значений
		/// TODO: Переделать под Matrix4
		/// </summary>
		public static void Update(Matrix4 projMatrix, Matrix4 modelMatrix) {
			
			// Defining clipping matrix<para/>
			// Задаём матрицу отсечения
			float t = 0f;
			Matrix4 clipMatrix = modelMatrix * projMatrix;

			// Extract the numbers for the right plane
			// Вычисление значений для правой поверхности
			frustum[0, 0] = clipMatrix.Row0.W - clipMatrix.Row0.X;
			frustum[0, 1] = clipMatrix.Row1.W - clipMatrix.Row1.X;
			frustum[0, 2] = clipMatrix.Row2.W - clipMatrix.Row2.X;
			frustum[0, 3] = clipMatrix.Row3.W - clipMatrix.Row3.X;

			t = (float)Math.Sqrt(frustum[0, 0] * frustum[0, 0] + frustum[0, 1] * frustum[0, 1] + frustum[0, 2] * frustum[0, 2]);
			frustum[0, 0] /= t;
			frustum[0, 1] /= t;
			frustum[0, 2] /= t;
			frustum[0, 3] /= t;

			// Extract the numbers for the left plane
			// Вычисление значений для левой поверхности
			frustum[1, 0] = clipMatrix.Row0.W + clipMatrix.Row0.X;
			frustum[1, 1] = clipMatrix.Row1.W + clipMatrix.Row1.X;
			frustum[1, 2] = clipMatrix.Row2.W + clipMatrix.Row2.X;
			frustum[1, 3] = clipMatrix.Row3.W + clipMatrix.Row3.X;

			t = (float)Math.Sqrt(frustum[1, 0] * frustum[1, 0] + frustum[1, 1] * frustum[1, 1] + frustum[1, 2] * frustum[1, 2]);
			frustum[1, 0] /= t;
			frustum[1, 1] /= t;
			frustum[1, 2] /= t;
			frustum[1, 3] /= t;

			// Extract the numbers for the bottom plane
			// Вычисление значений для нижней поверхности
			frustum[2, 0] = clipMatrix.Row0.W + clipMatrix.Row0.Y;
			frustum[2, 1] = clipMatrix.Row1.W + clipMatrix.Row1.Y;
			frustum[2, 2] = clipMatrix.Row2.W + clipMatrix.Row2.Y;
			frustum[2, 3] = clipMatrix.Row3.W + clipMatrix.Row3.Y;

			// Normalize the result
			t = (float)Math.Sqrt(frustum[2, 0] * frustum[2, 0] + frustum[2, 1] * frustum[2, 1] + frustum[2, 2] * frustum[2, 2]);
			frustum[2, 0] /= t;
			frustum[2, 1] /= t;
			frustum[2, 2] /= t;
			frustum[2, 3] /= t;

			// Extract the TOP plane
			// Вычисление верхней поверхности
			frustum[3, 0] = clipMatrix.Row0.W - clipMatrix.Row0.Y;
			frustum[3, 1] = clipMatrix.Row1.W - clipMatrix.Row1.Y;
			frustum[3, 2] = clipMatrix.Row2.W - clipMatrix.Row2.Y;
			frustum[3, 3] = clipMatrix.Row3.W - clipMatrix.Row3.Y;

			// Normalize the result
			t = (float)Math.Sqrt(frustum[3, 0] * frustum[3, 0] + frustum[3, 1] * frustum[3, 1] + frustum[3, 2] * frustum[3, 2]);
			frustum[3, 0] /= t;
			frustum[3, 1] /= t;
			frustum[3, 2] /= t;
			frustum[3, 3] /= t;


			//      0   1   2  3

			// #X - 0   4   8  12 
			// #Y - 1   5   9  13
			// #Z - 2   6  10  14
			// #W - 3   7  11  15


			// Extract the FAR plane
			frustum[4, 0] = clipMatrix.Row0.W - clipMatrix.Row0.Z;
			frustum[4, 1] = clipMatrix.Row1.W - clipMatrix.Row1.Z;
			frustum[4, 2] = clipMatrix.Row2.W - clipMatrix.Row2.Z;
			frustum[4, 3] = clipMatrix.Row3.W - clipMatrix.Row3.Z;

			// Normalize the result
			t = (float)Math.Sqrt(frustum[4, 0] * frustum[4, 0] + frustum[4, 1] * frustum[4, 1] + frustum[4, 2] * frustum[4, 2]);
			frustum[4, 0] /= t;
			frustum[4, 1] /= t;
			frustum[4, 2] /= t;
			frustum[4, 3] /= t;

			// Extract the NEAR plane
			frustum[5, 0] = clipMatrix.Row0.W + clipMatrix.Row0.Z;
			frustum[5, 1] = clipMatrix.Row1.W + clipMatrix.Row1.Z;
			frustum[5, 2] = clipMatrix.Row2.W + clipMatrix.Row2.Z;
			frustum[5, 3] = clipMatrix.Row3.W + clipMatrix.Row3.Z;

			// Normalize the result 
			t = (float)Math.Sqrt(frustum[5, 0] * frustum[5, 0] + frustum[5, 1] * frustum[5, 1] + frustum[5, 2] * frustum[5, 2]);
			frustum[5, 0] /= t;
			frustum[5, 1] /= t;
			frustum[5, 2] /= t;
			frustum[5, 3] /= t;
		}

		/// <summary>
		/// Check if sphere is in the frustum<para/>
		/// Проверка находится ли сфера в поле зрения
		/// </summary>
		/// <param name="center">Sphere center<para/>Центр сферы</param>
		/// <param name="radius">Sphere radius<para/>Радиус сферы</param>
		/// <returns>True if sphere is visible<para/>True если сфера видна</returns>
		public static bool ContainsSphere(Vector3 center, float radius) {
			for (int p = 0; p < 6; p++) {
				if ((frustum[p, 0] * center.X + frustum[p, 1] * center.Y + frustum[p, 2] * center.Z + frustum[p, 3]) <= -radius) {
					return false;
				}
			}
			return true;
		}

	}
}
