using System;
using Sentry;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenVice.Engine;
using System.Diagnostics;
using OpenVice.Entities;

namespace OpenVice.Forms {
	
	/// <summary>
	/// Base game window (using OpenToolkit)<para/>
	/// Основное игровое окно
	/// </summary>
	public partial class GameForm : GameWindow {

		/// <summary>
		/// Window constructor<para/>
		/// Конструктор окна
		/// </summary>
		/// <param name="width">Window width<para/>Ширина окна</param>
		/// <param name="height">Window height<para/>Высота окна</param>
		/// <param name="fullscreen">Use fullscreen window<para/>Использовать полноэкранное окно</param>
		public GameForm(int width, int height, bool fullscreen) :
		base(width, height, new GraphicsMode(new ColorFormat(24), 24), "OpenVice", GameWindowFlags.Default, DisplayDevice.Default, 2, 4, GraphicsContextFlags.Default) {
            using (SentrySdk.Init("https://c538a4e287ff404d890420b77fe03437@o447173.ingest.sentry.io/5426812"))
            {
                Load += InitEventHook;
                UpdateFrame += UpdateEventHook;
                RenderFrame += RenderEventHook;
                Resize += ResizeEventHook;
                Icon = OpenVice.Properties.Resources.AppIcon;

                Viewport.Size = new Vector2(ClientSize.Width, ClientSize.Height);
                Camera.Zoom = 1f;
                Camera.FarClip = 2000f;
            }
		}

		/// <summary>
		/// Window initialization event<para/>
		/// Событие инициализации окна
		/// </summary>
		/// <param name="sender">Window<para/>Окно</param>
		/// <param name="e">Event params<para/>Параметры события</param>
		void InitEventHook(object sender, EventArgs e) {
			if (Debugger.IsAttached) {
				Core.Init();
			} else {
				try {
					Core.Init();
				} catch (Exception ex) {
					Close();
					Application.Run(new ExceptionForm(ex));
				}
			}
		}

		/// <summary>
		/// Window update event<para/>
		/// Событие обновления окна
		/// </summary>
		/// <param name="sender">Window<para/>Окно</param>
		/// <param name="e">Event params<para/>Параметры события</param>
		void UpdateEventHook(object sender, FrameEventArgs e) {
			float tween = (float)(e.Time / 0.016);
			if (Debugger.IsAttached) {
				Core.Update(tween);
			} else {
				try {
					Core.Update(tween);
				} catch (Exception ex) {
					Close();
					Application.Run(new ExceptionForm(ex));
				}
			}
		}

		/// <summary>
		/// Window render event<para/>
		/// Событие отрисовки окна
		/// </summary>
		/// <param name="sender">Window<para/>Окно</param>
		/// <param name="e">Event params<para/>Параметры события</param>
		void RenderEventHook(object sender, FrameEventArgs e) {
			float tween = (float)(e.Time / 0.016);
			if (Debugger.IsAttached) {
				Core.Render(tween);
			} else {
				try {
					Core.Render(tween);
				} catch (Exception ex) {
					Close();
					Application.Run(new ExceptionForm(ex));
				}
			}
			SwapBuffers();
		}

		/// <summary>
		/// Window resize event<para/>
		/// Событие изменения размера окна
		/// </summary>
		/// <param name="sender">Window<para/>Окно</param>
		/// <param name="e">Event params<para/>Параметры события</param>
		void ResizeEventHook(object sender, EventArgs e) {
			Viewport.Size = new Vector2(ClientSize.Width, ClientSize.Height);
		}

	}
}
