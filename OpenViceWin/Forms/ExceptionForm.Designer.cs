namespace OpenVice.Forms {
	partial class ExceptionForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionForm));
			this.errorData = new System.Windows.Forms.RichTextBox();
			this.exitButton = new System.Windows.Forms.Button();
			this.restart = new System.Windows.Forms.Button();
			this.sendData = new System.Windows.Forms.Button();
			this.imageBar = new System.Windows.Forms.PictureBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.infoPanel = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.imageBar)).BeginInit();
			this.panel1.SuspendLayout();
			this.infoPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// errorData
			// 
			this.errorData.BackColor = System.Drawing.SystemColors.Window;
			this.errorData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.errorData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.errorData.Location = new System.Drawing.Point(0, 139);
			this.errorData.Name = "errorData";
			this.errorData.Size = new System.Drawing.Size(584, 276);
			this.errorData.TabIndex = 2;
			this.errorData.Text = "";
			// 
			// exitButton
			// 
			this.exitButton.Image = ((System.Drawing.Image)(resources.GetObject("exitButton.Image")));
			this.exitButton.Location = new System.Drawing.Point(472, 8);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = new System.Drawing.Size(100, 30);
			this.exitButton.TabIndex = 3;
			this.exitButton.Text = "Exit";
			this.exitButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.exitButton.UseVisualStyleBackColor = true;
			this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
			// 
			// restart
			// 
			this.restart.Image = ((System.Drawing.Image)(resources.GetObject("restart.Image")));
			this.restart.Location = new System.Drawing.Point(366, 8);
			this.restart.Name = "restart";
			this.restart.Size = new System.Drawing.Size(100, 30);
			this.restart.TabIndex = 4;
			this.restart.Text = "Restart";
			this.restart.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.restart.UseVisualStyleBackColor = true;
			this.restart.Click += new System.EventHandler(this.restart_Click);
			// 
			// sendData
			// 
			this.sendData.Image = ((System.Drawing.Image)(resources.GetObject("sendData.Image")));
			this.sendData.Location = new System.Drawing.Point(210, 8);
			this.sendData.Name = "sendData";
			this.sendData.Size = new System.Drawing.Size(150, 30);
			this.sendData.TabIndex = 5;
			this.sendData.Text = "Send exception data";
			this.sendData.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.sendData.UseVisualStyleBackColor = true;
			// 
			// imageBar
			// 
			this.imageBar.Dock = System.Windows.Forms.DockStyle.Top;
			this.imageBar.Image = ((System.Drawing.Image)(resources.GetObject("imageBar.Image")));
			this.imageBar.Location = new System.Drawing.Point(0, 0);
			this.imageBar.Name = "imageBar";
			this.imageBar.Size = new System.Drawing.Size(584, 100);
			this.imageBar.TabIndex = 6;
			this.imageBar.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.exitButton);
			this.panel1.Controls.Add(this.restart);
			this.panel1.Controls.Add(this.sendData);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 415);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(584, 46);
			this.panel1.TabIndex = 7;
			// 
			// infoPanel
			// 
			this.infoPanel.Controls.Add(this.label1);
			this.infoPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.infoPanel.Location = new System.Drawing.Point(0, 100);
			this.infoPanel.Name = "infoPanel";
			this.infoPanel.Size = new System.Drawing.Size(584, 39);
			this.infoPanel.TabIndex = 8;
			// 
			// label1
			// 
			this.label1.AutoEllipsis = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.label1.Size = new System.Drawing.Size(584, 39);
			this.label1.TabIndex = 0;
			this.label1.Text = "OpenVice engine just caught an unhandled exception and must be restarted or close" +
    "d. You can help us fix this error by sending crash debug information, or just re" +
    "start the game and keep playing!";
			// 
			// ExceptionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 461);
			this.Controls.Add(this.errorData);
			this.Controls.Add(this.infoPanel);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.imageBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExceptionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Engine crash";
			((System.ComponentModel.ISupportInitialize)(this.imageBar)).EndInit();
			this.panel1.ResumeLayout(false);
			this.infoPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox errorData;
		private System.Windows.Forms.Button exitButton;
		private System.Windows.Forms.Button restart;
		private System.Windows.Forms.Button sendData;
		private System.Windows.Forms.PictureBox imageBar;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel infoPanel;
		private System.Windows.Forms.Label label1;

	}
}