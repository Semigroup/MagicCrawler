namespace ArtOfMagicCrawler
{
    partial class LibraryImageSelectionDialog
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
            this.components = new System.ComponentModel.Container();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button_confirm = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button_cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar1.Location = new System.Drawing.Point(1365, 0);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(34, 731);
            this.vScrollBar1.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 16;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button_confirm
            // 
            this.button_confirm.Enabled = false;
            this.button_confirm.Location = new System.Drawing.Point(901, 538);
            this.button_confirm.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_confirm.Name = "button_confirm";
            this.button_confirm.Size = new System.Drawing.Size(153, 28);
            this.button_confirm.TabIndex = 2;
            this.button_confirm.Text = "Confirm Choice";
            this.button_confirm.UseVisualStyleBackColor = true;
            this.button_confirm.Click += new System.EventHandler(this.button_confirm_Click);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Calibri", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(443, 455);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(258, 30);
            this.textBox1.TabIndex = 3;
            // 
            // button_cancel
            // 
            this.button_cancel.Location = new System.Drawing.Point(1074, 445);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(153, 28);
            this.button_cancel.TabIndex = 4;
            this.button_cancel.Text = "Cancel";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // LibraryImageSelectionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1399, 731);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.button_confirm);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "LibraryImageSelectionDialog";
            this.Text = "Library Image Gallery";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LibraryImageSelectionDialog_FormClosed);
            this.Load += new System.EventHandler(this.LibraryImageSelectionDialog_Load);
            this.ClientSizeChanged += new System.EventHandler(this.LibraryImageSelectionDialog_ClientSizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.LibraryImageSelectionDialog_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LibraryImageSelectionDialog_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LibraryImageSelectionDialog_MouseMove);
            this.Resize += new System.EventHandler(this.LibraryImageSelectionDialog_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button_confirm;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button_cancel;
    }
}