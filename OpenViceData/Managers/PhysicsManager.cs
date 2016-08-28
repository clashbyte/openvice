using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;
using OpenTK;

namespace OpenVice.Managers {
	/// <summary>
	/// Class that handles all the wrap around BEPU<para/>
	/// Класс, осуществляющий всю обёртку над BEPU
	/// </summary>
	public static class PhysicsManager {

		public static Vector3 pos = Vector3.Zero;
		public static Quaternion rot = Quaternion.Identity;
		static BEPUphysics.Entities.Entity box;

		/// <summary>
		/// Physics working range<para/>
		/// Расстояние обработки физики
		/// </summary>
		public static float VisibleRange = 200f;

		/// <summary>
		/// Main physical space object<para/>
		/// Основной объект физического окружения
		/// </summary>
		public static Space World { get; private set; }

		/// <summary>
		/// Inititalize physics engine<para/>
		/// Инициализация физического движка
		/// </summary>
		public static void Init() {

			// Create simulation
			// Создание симуляции
			World = new Space();
			World.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);
			BEPUphysics.Settings.CollisionDetectionSettings.DefaultMargin = 0f;
			BEPUphysics.Settings.CollisionDetectionSettings.AllowedPenetration = 0f;
			//BEPUphysics.Settings.CollisionResponseSettings.PenetrationRecoveryStiffness = 0.005f;
		}

		/// <summary>
		/// Update physical entities<para/>
		/// Обновление физического окружения
		/// </summary>
		public static void Update(float delta) {

			// Update simulation
			// Обновление симуляции
			World.Update(delta * 0.016f);


			if (box != null) {
				pos = new Vector3(box.Position.X, box.Position.Y, box.Position.Z);
				rot = new Quaternion(box.Orientation.X, box.Orientation.Y, box.Orientation.Z, box.Orientation.W);
			}

		}

		public static void SetBox(Vector3 p, Vector3 n) {
			if (box == null) {
				box = new BEPUphysics.Entities.Prefabs.Box(BEPUutilities.Vector3.Zero, 1, 1, 1, 50);
				box.BecomeDynamic(50);
				World.Add(box);
			}
			box.Position = new BEPUutilities.Vector3(p.X, p.Y, p.Z);
			box.Orientation = BEPUutilities.Quaternion.Identity;
			box.LinearVelocity = new BEPUutilities.Vector3(n.X, n.Y, n.Z);
		}

	}
}
