using OpenVice.Controls;
using OpenVice.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace OpenVice.Dev {

	/// <summary>
	/// Some debugging utilities<para/>
	/// Некоторые дебаг-элементы
	/// </summary>
	public static class DebugStuff {

		static Graphics.Renderers.DebugRenderer rend;
		static Graphics.Renderers.DebugRenderer.Box box;


		/// <summary>
		/// Initialize debug data<para/>
		/// Инициализация отладочной иноформации
		/// </summary>
		public static void Init() {

			Entities.Camera.Position = new OpenTK.Vector3(-1647.534f, 26.54692f, -667.5128f);

			rend = new Graphics.Renderers.DebugRenderer();
			box = new Graphics.Renderers.DebugRenderer.Box();
			box.LineSize = 2f;
			box.Size = Vector3.One * 0.5f;
			rend.Primitives.Add(box);
		}

		/// <summary>
		/// Update debugging stuff<para/>
		/// Обновление дебаг-данных
		/// </summary>
		public static void Update() {
			if (Controls.Input.KeyPress(Key.KeypadPlus)) {
				CityManager.Interior = (Data.ItemPlacement.Interior)(((int)CityManager.Interior + 1) % 19);
			}
			if (Controls.Input.KeyPress(Key.Keypad0)) {
				CityManager.Interior = Data.ItemPlacement.Interior.World;
			}

			if (Controls.Input.KeyPress(Key.Q)) {
				PhysicsManager.SetBox(Entities.Camera.Position, Entities.Camera.TransformDirection(Vector3.UnitZ * 50f));
			}

			Graphics.Transform t = new Graphics.Transform() {
				Scale = Vector3.One,
				Position = PhysicsManager.pos,
				Angles = PhysicsManager.rot
			};

			box.Matrix = t.Matrix;
		}

		/// <summary>
		/// Render debugging stuff<para/>
		/// Отрисовка дебаг-данных
		/// </summary>
		public static void Render() {
			Graphics.Renderer.RenderQueue.Enqueue(rend);
		}

	}
}
