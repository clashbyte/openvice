using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenVice.Files {

	/// <summary>
	/// Some filesystem reading extensions<para/>
	/// Некоторые расширения для чтения
	/// </summary>
	public static class Extensions {

		/// <summary>
		/// Read string with specified length<para/>
		/// Чтение строки указанной длины
		/// </summary>
		/// <param name="Length">Length of string<para/>Длина строки</param>
		/// <returns>String<para/>Строка</returns>
		public static string ReadVCString(this BinaryReader f, int length) {
			char[] c = f.ReadChars(length);
			for (int i = 0; i < c.Length; i++) {
				if (c[i]=='\0') {
					return new string(c.Take(i).ToArray());
				}
			}
			return new string(c);
		}

		/// <summary>
		/// Read octet-aligned string<para/>
		/// Чтение строки, кратной четырём байтам
		/// </summary>
		/// <returns>String<para/>Строка</returns>
		public static string ReadOctetString(this BinaryReader f) {
			string o = "";
			while (true) {
				char c = f.ReadChar();
				if (c=='\0') {
					break;
				}
				o += c;
			}
			int skip = (o.Length+1) % 4;
			if (skip>0) {
				f.BaseStream.Position += (4 - skip);
			}
			return o;
		}

		/// <summary>
		/// Read ZY-YZ reversed vector<para/>
		/// Чтение перевернутого ZY-YZ вектора
		/// </summary>
		/// <returns>Correct vector<para/>Исправленный вектор</returns>
		public static OpenTK.Vector3 ReadVCVector(this BinaryReader f) {
			OpenTK.Vector3 v = new OpenTK.Vector3();
			v.X = f.ReadSingle();
			v.Z = f.ReadSingle();
			v.Y = f.ReadSingle();
			return v;
		}

		/// <summary>
		/// Read ZY-YZ reversed quaternion<para/>
		/// Чтение перевернутого ZY-YZ кватерниона
		/// </summary>
		/// <returns>Correct quaternion<para/>Исправленный кватернион</returns>
		public static OpenTK.Quaternion ReadVCQuaternion(this BinaryReader f) {
			OpenTK.Quaternion q = new OpenTK.Quaternion();
			q.X = -f.ReadSingle();
			q.Z = -f.ReadSingle();
			q.Y = -f.ReadSingle();
			q.W = -f.ReadSingle();
			return q;
		}

		/// <summary>
		/// Reorder matrix to VC quaternion<para/>
		/// Пересчёт матрицы в VC-кватернион
		/// </summary>
		/// <returns>Correct quaternion<para/>Исправленный кватернион</returns>
		public static OpenTK.Quaternion ToVCQuaternion(this OpenTK.Matrix3 rotation) {
			OpenTK.Quaternion q = OpenTK.Quaternion.FromMatrix(rotation);
			return new OpenTK.Quaternion(-q.X, -q.Z, -q.Y, -q.W);
		}

		/// <summary>
		/// Parse int from string<para/>
		/// Разбор строки в int
		/// </summary>
		/// <param name="s">Number string<para/>Строка с числом</param>
		/// <returns>Number<para/>Число</returns>
		public static int ToInt(this string s) {
			return int.Parse(s.Trim(), System.Globalization.NumberStyles.Integer);
		}

		/// <summary>
		/// Parse uint from string<para/>
		/// Разбор строки в uint
		/// </summary>
		/// <param name="s">Number string<para/>Строка с числом</param>
		/// <returns>Number<para/>Число</returns>
		public static uint ToUInt(this string s) {
			return uint.Parse(s.Trim(), System.Globalization.NumberStyles.Integer);
		}

		/// <summary>
		/// Parse float from string<para/>
		/// Разбор строки во float
		/// </summary>
		/// <param name="s">Number string<para/>Строка с числом</param>
		/// <returns>Number<para/>Число</returns>
		public static float ToFloat(this string s) {
			return float.Parse(s.Trim().Replace('.', ','), System.Globalization.NumberStyles.Float);
		}

		/// <summary>
		/// Scale up int to %4<para/>
		/// Подгонка числа под кратное четырем
		/// </summary>
		/// <returns>Octet int<para/>Число кратное 4</returns>
		public static int AlignToOctet(this int num) {
			int i = num % 4;
			if (i>0) {
				num = num + (4 - i);
			}
			return num; 
		}
	}
}
