using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Threading;
using EBookCrawler;

namespace ArtOfMagicCrawler
{
    public partial class LibraryImageSelectionDialog : Form
    {
        public const int ThumbnailHeight = 300;

        public class ImageTile : IComparable<ImageTile>
        {
            public enum SelectionMode
            {
                None,
                Pinned,
                Selected
            }

            public ArtObject Art;
            public int Width;
            public int Height;
            private Image Thumbnail;
            public int Index;
            public int X;
            public int Y;
            public SelectionMode Mode;

            public void SetSize(int height)
            {
                this.Height = height;
                if (Art.Height > 0)
                    this.Width = (int)(Art.Width * height / Art.Height);
                else
                    this.Width = 0;
            }

            public Rectangle GetArea(int x, int y)
              => new Rectangle()
              {
                  X = x,
                  Y = y,
                  Width = Width,
                  Height = Height
              };
            public Image GetThumbnail()
            {
                if (Thumbnail == null)
                    Thumbnail = Image.FromFile(Art.AbsoluteThumbnailPath);
                return Thumbnail;
            }
            public bool Check(string searchword)
            {
                //Console.WriteLine("checking " + Art.CardName);
                foreach (var keyword in Art.Keys)
                    if (keyword.Contains(searchword))
                        return true;
                return false;
            }
            public void Draw(Graphics graphics, Pen selectedPen, Pen pinnedPen, int x, int y)
            {
                this.X = x;
                this.Y = y;
                var area = GetArea(x, y);
                graphics.DrawImage(GetThumbnail(), area);
                switch (Mode)
                {
                    case SelectionMode.Pinned:
                        graphics.DrawRectangle(pinnedPen, area);
                        break;
                    case SelectionMode.Selected:
                        graphics.DrawRectangle(selectedPen, area);
                        break;
                    default:
                        break;
                }
            }
            public bool Contains(int x, int y)
            {
                if (x < X)
                    return false;
                if (x > X + Width)
                    return false;
                if (y < Y)
                    return false;
                if (y > Y + Height)
                    return false;
                return true;
            }

            public int CompareTo(ImageTile other)
            {
                return Index - other.Index;
            }
            public override string ToString()
            {
                return Index + ": " + Art.CardName + ", " + Mode;
            }
        }
        public class Row
        {
            public List<ImageTile> Tiles = new List<ImageTile>();
            public int TotalWidth;
            public int MaxWidth;
            public bool IsClosed;
            public int Y;

            public void Clear(int newMaxWidth)
            {
                Tiles.Clear();
                TotalWidth = 0;
                IsClosed = false;
                this.MaxWidth = newMaxWidth;
                this.Y = 0;
            }

            public bool Add(ImageTile tile)
            {
                if (IsClosed)
                    return false;

                tile.SetSize(ThumbnailHeight);
                if (this.TotalWidth + tile.Width > this.MaxWidth)
                    return false;

                this.TotalWidth += tile.Width;
                this.Tiles.Add(tile);
                return true;
            }
            public bool Contains(int y)
            {
                if (y < Y)
                    return false;
                if (y > Y + ThumbnailHeight)
                    return false;
                return true;
            }
        }
        private class CheckingThread
        {
            internal int CurrentTile;
            internal bool[] ShowTile;
            internal int TimeStamp;
            private LibraryImageSelectionDialog MainThread;

            public CheckingThread(LibraryImageSelectionDialog MainThread)
            {
                this.MainThread = MainThread;
            }

            internal void Reset()
            {
                this.CurrentTile = 0;
                this.TimeStamp++;
            }

            internal void StartThread()
            {
                var task = new Task(Run);
                task.Start();
            }

            private void Run()
            {
                string searchWord = "";
                ShowTile = new bool[MainThread.Tiles.Count];

                while (!MainThread.IsClosed)
                {
                    try
                    {
                        if (searchWord != MainThread.textBox1.Text)
                        {
                            searchWord = MainThread.textBox1.Text.ToLower();
                            this.Reset();
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }

                    for (int i = 0; i < 100; i++)
                        if (CurrentTile < ShowTile.Length)
                        {
                            ShowTile[CurrentTile] = MainThread.Tiles[CurrentTile].Check(searchWord);
                            CurrentTile++;
                        }
                    if (CurrentTile == ShowTile.Length)
                        Thread.Sleep(16);
                }
            }
        }

        public ArtLibrary Library { get; private set; }
        public float ImageHeight { get; set; } = 300;
        public List<ImageTile> Tiles { get; set; } = new List<ImageTile>();
        public List<Row> Rows { get; set; } = new List<Row>();
        public ImageTile SelectedTile { get; set; } = null;
        public List<ImageTile> PinnedTiles { get; set; } = new List<ImageTile>();
        public List<Row> ShownRows { get; set; } = new List<Row>();
        public int CurrentRow { get; set; } = 0;
        public ImageTile HoveringTile { get; private set; }

        public int MaxWidth => this.ClientSize.Width - vScrollBar1.Width;

        private bool IsClosed;
        private int CurrentTile;
        private int TimeStamp;

        private Font Calibri = new Font("Calibri", 11);
        public Pen SelectedPen = new Pen(Color.Red)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
            DashPattern = new float[] { 4, 4, 4, 4 },
            Width = 5,
            Alignment = System.Drawing.Drawing2D.PenAlignment.Inset
        };
        public Pen PinnedPen = new Pen(Color.Blue)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
            DashPattern = new float[] { 4, 4, 4, 4 },
            Width = 5,
            Alignment = System.Drawing.Drawing2D.PenAlignment.Inset
        };

        private CheckingThread Thread2;

        private Rectangle LowerRightInfoBox = new Rectangle(0, 0, 750, 43);

        private ShowArtSideForm SideForm;

        public LibraryImageSelectionDialog()
        {
            this.SideForm = new ShowArtSideForm(this);
            this.Thread2 = new CheckingThread(this);

            InitializeComponent();

            this.DoubleBuffered = true;
            this.MouseWheel += LibraryImageSelectionDialog_MouseWheel;
        }

        public void SetLibrary(ArtLibrary library)
        {
            this.Library = library;
            Tiles.Clear();
            int ind = 0;
            foreach (var art in library.ArtObjects)
                Tiles.Add(new ImageTile() { Art = art, Index = ind++ });
        }

        private void AddTile(ImageTile tile)
        {
            while (CurrentRow < Rows.Count)
            {
                Row current = Rows[CurrentRow];
                if (current.Add(tile))
                    return;
                else
                    CurrentRow++;
            }

            Row newRow = new Row() { MaxWidth = MaxWidth };
            newRow.Add(tile);
            this.Rows.Add(newRow);
        }
        private void ClearRows()
        {
            foreach (var row in Rows)
                row.Clear(MaxWidth);
            this.CurrentRow = 0;
            this.CurrentTile = 0;

            if (SelectedTile != null)
                AddTile(SelectedTile);
            foreach (var tile in PinnedTiles)
                AddTile(tile);
            this.TimeStamp = Thread2.TimeStamp;
            UpdateRows();
        }
        private void UpdateRows()
        {
            for (int i = CurrentTile; i < Thread2.CurrentTile; i++)
                if (Thread2.ShowTile[i])
                {
                    var tile = Tiles[i];
                    if (tile.Mode == ImageTile.SelectionMode.None)
                        this.AddTile(tile);
                }
            this.CurrentTile = Thread2.CurrentTile;

            int totalHeight = (CurrentRow + 1) * ThumbnailHeight;
            totalHeight = Math.Max(totalHeight, this.ClientSize.Height);
            this.vScrollBar1.Maximum = totalHeight;
        }
        private ImageTile GetTile(int x, int y)
        {
            foreach (var row in ShownRows)
                if (row.Contains(y))
                    foreach (var tile in row.Tiles)
                        if (tile.Contains(x, y))
                            return tile;
            return null;
        }
        private void TogglePinned(ImageTile tile)
        {
            switch (tile.Mode)
            {
                case ImageTile.SelectionMode.None:
                    Pin(tile);
                    break;
                case ImageTile.SelectionMode.Pinned:
                    DePin(tile);
                    break;
                case ImageTile.SelectionMode.Selected:
                    DeSelect(tile);
                    Pin(tile);
                    break;
                default:
                    throw new NotImplementedException();
            }
            ClearRows();
        }
        private void ToggleSelected(ImageTile tile)
        {
            var oldTile = SelectedTile;
            switch (tile.Mode)
            {
                case ImageTile.SelectionMode.None:
                    if (oldTile != null)
                    {
                        DeSelect(oldTile);
                        Pin(oldTile);
                    }
                    Select(tile);
                    break;
                case ImageTile.SelectionMode.Pinned:
                    if (oldTile != null)
                    {
                        DeSelect(oldTile);
                        Pin(oldTile);
                    }
                    DePin(tile);
                    Select(tile);
                    break;
                case ImageTile.SelectionMode.Selected:
                    DeSelect(oldTile);
                    break;
                default:
                    throw new NotImplementedException();
            }
            ClearRows();
        }
        private void Select(ImageTile tile)
        {
            this.SelectedTile = tile;
            tile.Mode = ImageTile.SelectionMode.Selected;

            this.Text = tile.Art.CardName + " ist ausgewählt";
            this.button_confirm.Enabled = true;
        }
        private void DeSelect(ImageTile tile)
        {
            if (SelectedTile != tile)
                Logger.LogWarning("DeSelect(" + tile.Art.CardName + ")" +
                    " called while SelectedTile = " + SelectedTile.Art.CardName);

            this.button_confirm.Enabled = false;
            SelectedTile.Mode = ImageTile.SelectionMode.None;
            SelectedTile = null;
            this.Text = "Kein Bild ausgewählt";
        }
        private void Pin(ImageTile tile)
        {
            tile.Mode = ImageTile.SelectionMode.Pinned;
            PinnedTiles.Add(tile);
            PinnedTiles.Sort();
        }
        private void DePin(ImageTile tile)
        {
            tile.Mode = ImageTile.SelectionMode.None;
            PinnedTiles.Remove(tile);
        }

        private void LibraryImageSelectionDialog_MouseWheel(object sender, MouseEventArgs e)
        {
            int newValue = this.vScrollBar1.Value - vScrollBar1.SmallChange * e.Delta;
            newValue = Math.Max(this.vScrollBar1.Minimum, newValue);
            newValue = Math.Min(this.vScrollBar1.Maximum, newValue);

            this.vScrollBar1.Value = newValue;
        }
        private void LibraryImageSelectionDialog_Paint(object sender, PaintEventArgs e)
        {
            ShownRows.Clear();
            e.Graphics.Clear(Color.Black);
            int y = -vScrollBar1.Value;
            int shownTiles = 0;
            foreach (var row in Rows)
            {
                shownTiles += row.Tiles.Count;
                row.Y = y;
                y += ThumbnailHeight;
                if (row.Y + ThumbnailHeight < 0 || row.Y > this.ClientSize.Height)
                    continue;

                ShownRows.Add(row);
                int x = 0;
                foreach (var tile in row.Tiles)
                {
                    tile.Draw(e.Graphics, SelectedPen, PinnedPen, x, row.Y);
                    //e.Graphics.FillRectangle(Brushes.White, x, y, 500, 100);
                    //e.Graphics.DrawString(tile.Index + ": " + tile.Art.CardName,
                    //    Calibri, Brushes.Black, x, y);
                    x += tile.Width;
                }
            }

            LowerRightInfoBox.Location = new Point(ClientSize.Width - LowerRightInfoBox.Width,
                ClientSize.Height - LowerRightInfoBox.Height);
            e.Graphics.FillRectangle(Brushes.White, LowerRightInfoBox);
            e.Graphics.DrawString(
               (SelectedTile == null ? "0" : "1") + " selected, "
               + PinnedTiles.Count + " pinned, "
               + shownTiles + " shown, " +
               "Progress " + CurrentTile + " / " + Tiles.Count,
                Calibri, Brushes.Black, LowerRightInfoBox.Location);
        }
        private void LibraryImageSelectionDialog_ClientSizeChanged(object sender, EventArgs e)
        {
            ClearRows();
            //foreach (var tile in FilteredTiles)
            //    AddTile(tile);
            this.vScrollBar1.Maximum = Math.Max(this.vScrollBar1.Maximum, this.ClientSize.Height);
            this.vScrollBar1.LargeChange = (int)this.ClientSize.Height;
        }
        private void LibraryImageSelectionDialog_Load(object sender, EventArgs e)
        {
            this.OnResize(e);
            this.IsClosed = false;
            this.SideForm.Show();

            this.Thread2.Reset();
            this.Thread2.StartThread();
        }
        private void LibraryImageSelectionDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.SideForm.Hide();
            this.IsClosed = true;
        }
        private void LibraryImageSelectionDialog_MouseMove(object sender, MouseEventArgs e)
        {
            this.HoveringTile = GetTile(e.X, e.Y);
        }
        private void LibraryImageSelectionDialog_MouseDown(object sender, MouseEventArgs e)
        {
            var tile = GetTile(e.X, e.Y);
            if (tile == null)
                return;
            if (e.Button == MouseButtons.Right)
                TogglePinned(tile);
            else if (e.Button == MouseButtons.Left)
                ToggleSelected(tile);
        }
        private void LibraryImageSelectionDialog_Resize(object sender, EventArgs e)
        {
            this.button_confirm.Location =
                new Point(0, ClientSize.Height - button_confirm.Height);
            this.button_cancel.Location =
                new Point(button_confirm.Width, ClientSize.Height - button_confirm.Height);
            this.textBox1.Location =
                new Point(button_cancel.Right, ClientSize.Height - button_confirm.Height);
            this.textBox1.Size =
                new Size(ClientSize.Width - button_cancel.Right - LowerRightInfoBox.Width, textBox1.Height);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Refresh();
            this.SelectedPen.DashOffset += 0.25f;

            if (this.TimeStamp < Thread2.TimeStamp)
                this.ClearRows();
            else
                UpdateRows();
        }
        private void button_confirm_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
