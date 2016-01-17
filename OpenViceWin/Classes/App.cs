using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenVice.Forms;

namespace OpenVice.Classes {
	
	/// <summary>
	/// Internal app class<para/>
	/// Внутренний класс приложения
	/// </summary>
	public class App {

		/// <summary>
		/// Start application<para/>
		/// Запуск приложения
		/// </summary>
		public static void Start() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			GameForm w = new GameForm(1024, 600, false);
			w.Run(60.0, 60.0);
			
		}

	}
}
