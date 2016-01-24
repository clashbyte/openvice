using OpenVice.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenVice.World {
	
	/// <summary>
	/// Class that handles basic environment<para/>
	/// Класс, содержащий базовые значения игрового окружения
	/// </summary>
	public static class Environment {

		/// <summary>
		/// Current game world hour (0-23)<para/>
		/// Текущий час игрового мира
		/// </summary>
		public static int Hour { get; set; }

		/// <summary>
		/// Current game world minute (0-59)<para/>
		/// Текущая минута игрового мира
		/// </summary>
		public static int Minute { get; set; }

		static float d = 0;

		/// <summary>
		/// Update environment<para/>
		/// Обновление окружения
		/// </summary>
		/// <param name="tween"></param>
		public static void Update(float tween) {
			d += 1f*tween;
			if (d>1) {
				d = 0;
				Minute++;
				if (Minute>59) {
					Minute = 0;
					Hour = (Hour + 1) % 24;
				}
			}

			Graphics.Renderer.SkyState = TimeCycleManager.GetCurrent(4, Hour, Minute);
			Entities.Camera.FarClip = Graphics.Renderer.SkyState.CameraClip;

		}
	
	}
}
