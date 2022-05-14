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
        private ArtObject CurrentArt;

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
                if (CurrentArt != MainForm.HoveringTile.Art)
                {
                    CurrentArt = MainForm.HoveringTile.Art;
                    this.pictureBox1.Image = Image.FromFile(CurrentArt.AbsoluteImagePath);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(CurrentArt.CardName);
                    sb.AppendLine(CurrentArt.MagicSet);
                    sb.AppendLine(CurrentArt.Artist);
                    sb.AppendLine(CurrentArt.Width + "x" + CurrentArt.Height);
                    sb.AppendLine(CurrentArt.Note);
                    foreach (var item in CurrentArt.Keys)
                    {
                        sb.Append(item);
                        sb.Append(", ");
                    }

                    this.label1.Text = sb.ToString();
                }
            }
        }
    }
}
