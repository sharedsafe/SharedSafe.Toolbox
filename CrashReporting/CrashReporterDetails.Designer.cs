namespace Toolbox.CrashReporting
{
	partial class CrashReporterDetails
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
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.StackTrace = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.button1 = new System.Windows.Forms.Button();
			this.Description = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.StackTrace, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.Description, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(478, 317);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.SystemColors.Highlight;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.ForeColor = System.Drawing.SystemColors.HighlightText;
			this.label2.Location = new System.Drawing.Point(0, 45);
			this.label2.Margin = new System.Windows.Forms.Padding(0);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(3);
			this.label2.Size = new System.Drawing.Size(478, 19);
			this.label2.TabIndex = 1;
			this.label2.Text = "Stack Trace";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.SystemColors.Highlight;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.SystemColors.HighlightText;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(0);
			this.label1.Name = "label1";
			this.label1.Padding = new System.Windows.Forms.Padding(3);
			this.label1.Size = new System.Drawing.Size(478, 19);
			this.label1.TabIndex = 0;
			this.label1.Text = "Description";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// StackTrace
			// 
			this.StackTrace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StackTrace.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StackTrace.Location = new System.Drawing.Point(3, 67);
			this.StackTrace.Multiline = true;
			this.StackTrace.Name = "StackTrace";
			this.StackTrace.ReadOnly = true;
			this.StackTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.StackTrace.Size = new System.Drawing.Size(472, 205);
			this.StackTrace.TabIndex = 2;
			this.StackTrace.WordWrap = false;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.button1, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 278);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(472, 36);
			this.tableLayoutPanel2.TabIndex = 3;
			// 
			// button1
			// 
			this.button1.AutoSize = true;
			this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button1.Location = new System.Drawing.Point(386, 3);
			this.button1.MinimumSize = new System.Drawing.Size(80, 0);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(83, 30);
			this.button1.TabIndex = 0;
			this.button1.Text = "Continue";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// Description
			// 
			this.Description.AutoSize = true;
			this.Description.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Description.Location = new System.Drawing.Point(3, 22);
			this.Description.Margin = new System.Windows.Forms.Padding(3);
			this.Description.Name = "Description";
			this.Description.Size = new System.Drawing.Size(472, 20);
			this.Description.TabIndex = 4;
			this.Description.Text = "label3";
			// 
			// CrashReporterDetails
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(478, 317);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "CrashReporterDetails";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Crash Details";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox StackTrace;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label Description;
	}
}