using OpenTK;
using OpenVice.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.Managers {
	
	/// <summary>
	/// Class that contains all the data from Timecyc.dat<para/>
	/// Класс, который содержит данные из файла Timecyc.dat
	/// </summary>
	public static class TimeCycleManager {

		/// <summary>
		/// Parameters for each weather and each hour<para/>
		/// Параметры для каждой погоды и каждого часа
		/// </summary>
		static Entry[,] weatherParams;

		/// <summary>
		/// Initialize Timecyc.dat file<para/>
		/// Чтение файла Timecyc
		/// </summary>
		public static void Init() {
			if (weatherParams == null) {
				Entry[,] ea = new Entry[7, 24];
				TextFile f = new TextFile(PathManager.GetAbsolute("data/Timecyc.dat"), false, false);
				int w = 0, h = 0;
				foreach (TextFile.Line l in f.Lines) {
					Entry e = new Entry();

					e.AmbientStatic = new Vector3(
						l.Text[0].ToFloat() / 255f,
						l.Text[1].ToFloat() / 255f,
						l.Text[2].ToFloat() / 255f
					);
					e.AmbientDynamic = new Vector3(
						l.Text[3].ToFloat() / 255f,
						l.Text[4].ToFloat() / 255f,
						l.Text[5].ToFloat() / 255f
					);
					e.DiffuseStatic = new Vector3(
						l.Text[6].ToFloat() / 255f,
						l.Text[7].ToFloat() / 255f,
						l.Text[8].ToFloat() / 255f
					);
					e.DiffuseDynamic = new Vector3(
						l.Text[9].ToFloat() / 255f,
						l.Text[10].ToFloat() / 255f,
						l.Text[11].ToFloat() / 255f
					);
					e.DirectLight = new Vector3(
						l.Text[12].ToFloat() / 255f,
						l.Text[13].ToFloat() / 255f,
						l.Text[14].ToFloat() / 255f
					);
					e.SkyTop = new Vector3(
						l.Text[15].ToFloat() / 255f,
						l.Text[16].ToFloat() / 255f,
						l.Text[17].ToFloat() / 255f
					);
					e.SkyBottom = new Vector3(
						l.Text[18].ToFloat() / 255f,
						l.Text[19].ToFloat() / 255f,
						l.Text[20].ToFloat() / 255f
					);

					e.CameraClip = l.Text[33].ToFloat();
					e.FogDistance = l.Text[34].ToFloat();

					// Storing data
					// Сохранение данных
					ea[w, h] = e;
					h++;
					if (h>23) {
						h = 0;
						w++;
					}
				}
				weatherParams = ea;
			}
		}

		/// <summary>
		/// Get current weather state for specified weather group<para/>
		/// Получение текущего значения погоды для одной из групп
		/// </summary>
		/// <param name="weatherGroup">Group index<para/>Индекс группы</param>
		/// <param name="hour">Current game hour<para/>Текущий час игрового времени</param>
		/// <param name="minute">Current game minute<para/>Текущая минута игрового времени</param>
		/// <returns>Interpolated weather entry<para/>Интерполированную запись</returns>
		public static Entry GetCurrent(int weatherGroup, int hour, int minute) {

			// Check if Timecyc is actually loaded<para/>
			// Проверка, загружен ли Timecyc
			if (weatherParams!=null) {
				// Calculate next hour and interpolation value
				// Вычисление следующего часа и значения интерполяции
				int nextHour = (hour + 1) % 24;
				float delta = (float)minute / 59f;

				// Creating new value based on two
				// Создаём новые значения, базирующиеся на двух
				return weatherParams[weatherGroup, hour].Mix(weatherParams[weatherGroup, nextHour], delta);
			}else{
				// Give away empty weather data
				// Отдача пустых данных погоды
				return new Entry() {
					AmbientStatic = new Vector3(0.1f, 0.1f, 0.1f),
					AmbientDynamic = new Vector3(0.1f, 0.1f, 0.1f),
					DiffuseStatic = new Vector3(0.8f, 0.8f, 0.8f),
					DiffuseDynamic = new Vector3(0.8f, 0.8f, 0.8f),
					DirectLight = new Vector3(1f, 1f, 1f),
					SkyTop = new Vector3(0.8f, 0.8f, 0.8f),
					SkyBottom = new Vector3(0.1f, 0.1f, 0.1f),
					FogDistance = 100,
					CameraClip = 2000,
				};
			}
		}

		/// <summary>
		/// Single weather entry<para/>
		/// Один элемент из файла Timecyc
		/// </summary>
		public struct Entry {
			/// <summary>
			/// Ambient color for static objects<para/>
			/// Цвет тени для статичных объектов
			/// </summary>
			public Vector3 AmbientStatic;

			/// <summary>
			/// Ambient color for dynamic objects<para/>
			/// Цвет тени для динамичных объектов
			/// </summary>
			public Vector3 AmbientDynamic;

			/// <summary>
			/// Diffuse color for static objects<para/>
			/// Цвет освещённости для статичных объектов
			/// </summary>
			public Vector3 DiffuseStatic;

			/// <summary>
			/// Diffuse color for dynamic objects<para/>
			/// Цвет освещённости для динамичных объектов
			/// </summary>
			public Vector3 DiffuseDynamic;

			/// <summary>
			/// Direct light color for dynamic objects<para/>
			/// Прямой свет для динамичных объектов
			/// </summary>
			public Vector3 DirectLight;

			/// <summary>
			/// Sky colors, top and bottom<para/>
			/// Цвет неба, верхний и нижний
			/// </summary>
			public Vector3 SkyTop, SkyBottom;

			/// <summary>
			/// Fog range<para/>
			/// Расстояние тумана
			/// </summary>
			public float FogDistance;

			/// <summary>
			/// Camera range<para/>
			/// Дальность прорисовки
			/// </summary>
			public float CameraClip;

			/*
			public Vector3 sunColor, sunCoronaColor;
			public float sunCoreSize, sunCoronaSize;
			public float sunBright;
			public float farClip, fogDist;
			public Vector4 water;
			 */

			/// <summary>
			/// Interpolate current values to another Timecyc entry<para/>
			/// Интерполяция текущих данных до другого значения
			/// </summary>
			/// <param name="et">Interpolation target<para/>Целевое значение интерполяции</param>
			/// <param name="delta">Mixing amount<para/>Сила смешивания</param>
			/// <returns></returns>
			public Entry Mix(Entry et, float delta) {
				Entry e = this;
				return new Entry() {
					AmbientStatic	= Vector3.Lerp(e.AmbientStatic, et.AmbientStatic, delta),
					AmbientDynamic	= Vector3.Lerp(e.AmbientDynamic, et.AmbientDynamic, delta),
					DiffuseStatic	= Vector3.Lerp(e.DiffuseStatic, et.DiffuseStatic, delta),
					DiffuseDynamic	= Vector3.Lerp(e.DiffuseDynamic, et.DiffuseDynamic, delta),
					DirectLight		= Vector3.Lerp(e.DirectLight, et.DirectLight, delta),
					SkyTop			= Vector3.Lerp(e.SkyTop, et.SkyTop, delta),
					SkyBottom		= Vector3.Lerp(e.SkyBottom, et.SkyBottom, delta),
					FogDistance		= e.FogDistance + (et.FogDistance-e.FogDistance) * delta,
					CameraClip		= e.CameraClip + (et.CameraClip - e.CameraClip) * delta,
				};
			}


		}
	}
}
