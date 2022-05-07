using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArtOfMagicCrawler
{
    public partial class ShowArtSideForm : Form
    {
        private LibraryImageSelectionDialog MainForm;

        public ShowArtSideForm(LibraryImageSelectionDialog MainForm )
        {
            this.MainForm = MainForm;
            InitializeComponent();
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            this.label1.Height = 400;
            this.pictureBox1.Height = ClientSize.Height - this.label1.Height;
        }

        private void ShowArtSideForm_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (MainForm.HoveringTile != null)
            {
                var art = MainForm.HoveringTile.Art;
                this.pictureBox1.Image = Image.FromFile(art.AbsoluteImagePath);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(art.CardName);
                sb.AppendLine(art.MagicSet);
                sb.AppendLine(art.Artist);
                sb.AppendLine(art.Width + "x" + art.Height);
                sb.AppendLine(art.Note);
                foreach (var item in art.Keys)
                {
                    sb.Append(item);
                    sb.Append(", ");
                }

                this.label1.Text = sb.ToString();
            }
        }
    }
}
