using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;
using OpenTK;

namespace OpenVice.Controls {

	/// <summary>
	/// Main input class<para/>
	/// Основной класс ввода
	/// </summary>
	public static class Input {

		/// <summary>
		/// Mouse movement speed<para/>
		/// Скорость передвижения мыши
		/// </summary>
		public static Vector2 MouseSpeed {
			get { return mouseSpeed; }
		}


		/// <summary>
		/// Internal key state list<para/>
		/// Внутренний список состояний клавиш
		/// </summary>
		static KeyState[] keyStates;

		/// <summary>
		/// Internal mouse speed<para/>
		/// Скорость мыши
		/// </summary>
		static Vector2 mouseSpeed;

		/// <summary>
		/// Old mouse position<para/>
		/// Предыдущая позиция мыши
		/// </summary>
		static Vector2 mouseOldPos;

		/// <summary>
		/// Update mouse and keyboard state<para/>
		/// Обработка состояния клавиатуры и мыши
		/// </summary>
		public static void Update() {
			
			// Creating key array
			// Создание массива клавиш
			if (keyStates==null) {
				keyStates = new KeyState[256];
				for (int i = 0; i < keyStates.Length; i++) {
					keyStates[i] = KeyState.Off;
				}
			}

			// Process keyboard
			// Обработка клавиатуры
			KeyboardState keys = Keyboard.GetState();
			for (int i = 0; i < keyStates.Length; i++) {
				bool on = keys.IsKeyDown((short)i);
				switch (keyStates[i]) {
					// Key offline
					// Клавиша отжата
					case KeyState.Off:
						if (on) {
							keyStates[i] = KeyState.Pressed;
						}
						break;

					// Key just pressed
					// Клавишу только что нажали
					case KeyState.Pressed:
						if (on) {
							keyStates[i] = KeyState.Down;
						} else {
							keyStates[i] = KeyState.Released;
						}
						break;

					// Key is down
					// Клавиша нажата
					case KeyState.Down:
						if (!on) {
							keyStates[i] = KeyState.Released;
						}
						break;

					// Key just released
					// Клавишу только что отпустили
					case KeyState.Released:
						if (on) {
							keyStates[i] = KeyState.Pressed;
						} else {
							keyStates[i] = KeyState.Off;
						}
						break;
				}
			}

			// Process mouse
			// Обработка мыши
			MouseState mouse = Mouse.GetState();
			Vector2 mpos = new Vector2(mouse.X, mouse.Y);
			mouseSpeed = mpos - mouseOldPos;
			mouseOldPos = mpos;

		}

		/// <summary>
		/// Check if key just pressed<para/>
		/// Клавиша только что нажата
		/// </summary>
		/// <param name="key">Key code<para/>Код клавиши</param>
		/// <returns>Key press<para/>Клавиша только что нажата</returns>
		public static bool KeyPress(Key key) {
			if (keyStates==null) {
				return false;
			}
			return keyStates[(short)key] == KeyState.Pressed;
		}

		/// <summary>
		/// Check if key just released<para/>
		/// Клавиша только что отпущена
		/// </summary>
		/// <param name="key">Key code<para/>Код клавиши</param>
		/// <returns>Key release<para/>Клавиша только что отпущена</returns>
		public static bool KeyRelease(Key key) {
			if (keyStates == null) {
				return false;
			}
			return keyStates[(short)key] == KeyState.Released;
		}

		/// <summary>
		/// Check if key down<para/>
		/// Клавиша нажата
		/// </summary>
		/// <param name="key">Key code<para/>Код клавиши</param>
		/// <returns>Key donw<para/>Клавиша нажата</returns>
		public static bool KeyDown(Key key) {
			if (keyStates == null) {
				return false;
			}
			return keyStates[(short)key] == KeyState.Pressed || keyStates[(short)key] == KeyState.Down;
		}

		/// <summary>
		/// Internal single key state flag<para/>
		/// Внутренний список значений состояния клавиши
		/// </summary>
		enum KeyState {
			Off,
			Pressed,
			Down,
			Released
		}
	}
}
