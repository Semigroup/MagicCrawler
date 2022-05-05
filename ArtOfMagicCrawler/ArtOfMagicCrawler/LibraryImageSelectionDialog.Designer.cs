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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBox1.Font = new System.Drawing.Font("Calibri", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(0, 1099);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(2099, 43);
            this.textBox1.TabIndex = 0;
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar1.Location = new System.Drawing.Point(2065, 0);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(34, 1099);
            this.vScrollBar1.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 16;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // LibraryImageSelectionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2099, 1142);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.textBox1);
            this.Name = "LibraryImageSelectionDialog";
            this.Text = "LibraryImageSelectionDialog";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LibraryImageSelectionDialog_FormClosed);
            this.Load += new System.EventHandler(this.LibraryImageSelectionDialog_Load);
            this.ClientSizeChanged += new System.EventHandler(this.LibraryImageSelectionDialog_ClientSizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.LibraryImageSelectionDialog_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LibraryImageSelectionDialog_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LibraryImageSelectionDialog_MouseMove);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.Timer timer1;
    }
}