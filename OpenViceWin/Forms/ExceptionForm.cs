using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenVice.Forms {

	/// <summary>
	/// Exception information form<para/>
	/// Форма вывода ошибки
	/// </summary>
	public partial class ExceptionForm : Form {


		public ExceptionForm(Exception e) {
			InitializeComponent();

			errorData.Text = e.ToString();

		}

		private void exitButton_Click(object sender, EventArgs e) {
			Close();
		}

		private void restart_Click(object sender, EventArgs e) {
			Process.Start(Application.ExecutablePath);
			Close();
		}
	}
}
