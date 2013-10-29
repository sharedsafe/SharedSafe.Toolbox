namespace Toolbox.CrashReporting
{
	partial class CrashReporterForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.Sorry = new System.Windows.Forms.Label();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.Details = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.Sorry, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(487, 228);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// Sorry
			// 
			this.Sorry.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Sorry.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Sorry.Location = new System.Drawing.Point(4, 0);
			this.Sorry.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.Sorry.Name = "Sorry";
			this.Sorry.Size = new System.Drawing.Size(479, 178);
			this.Sorry.TabIndex = 1;
			this.Sorry.Text = "Sorry, this application crashed and can not be continued.";
			this.Sorry.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 5;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.Details, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.button2, 3, 0);
			this.tableLayoutPanel2.Controls.Add(this.button1, 2, 0);
			this.tableLayoutPanel2.Controls.Add(this.button3, 4, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 183);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(479, 40);
			this.tableLayoutPanel2.TabIndex = 2;
			// 
			// Details
			// 
			this.Details.AutoSize = true;
			this.Details.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Details.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Details.Location = new System.Drawing.Point(4, 5);
			this.Details.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Details.MinimumSize = new System.Drawing.Size(80, 0);
			this.Details.Name = "Details";
			this.Details.Size = new System.Drawing.Size(80, 30);
			this.Details.TabIndex = 3;
			this.Details.Text = "Details...";
			this.Details.UseVisualStyleBackColor = true;
			this.Details.Click += new System.EventHandler(this.Details_Click);
			// 
			// button2
			// 
			this.button2.AutoSize = true;
			this.button2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Retry;
			this.button2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button2.Location = new System.Drawing.Point(307, 5);
			this.button2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.button2.MinimumSize = new System.Drawing.Size(80, 0);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(80, 30);
			this.button2.TabIndex = 1;
			this.button2.Text = "Restart";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.AutoSize = true;
			this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button1.Location = new System.Drawing.Point(219, 5);
			this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.button1.MinimumSize = new System.Drawing.Size(80, 0);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(80, 30);
			this.button1.TabIndex = 0;
			this.button1.Text = "Report...";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// button3
			// 
			this.button3.AutoSize = true;
			this.button3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.button3.DialogResult = System.Windows.Forms.DialogResult.Abort;
			this.button3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button3.Location = new System.Drawing.Point(395, 5);
			this.button3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.button3.MinimumSize = new System.Drawing.Size(80, 0);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(80, 30);
			this.button3.TabIndex = 2;
			this.button3.Text = "Exit";
			this.button3.UseVisualStyleBackColor = true;
			// 
			// CrashReporterForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(487, 228);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "CrashReporterForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "CrashReporterForm";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label Sorry;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button Details;
	}
}