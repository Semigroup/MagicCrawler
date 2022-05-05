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

namespace ArtOfMagicCrawler
{
    public partial class LibraryImageSelectionDialog : Form
    {
        public const int ThumbnailHeight = 300;

        public class ImageTile : IComparable<ImageTile>
        {
            public ArtObject Art;
            public int Width;
            public int Height;
            private Image Thumbnail;
            public int Index;
            public bool IsPinned;
            public bool IsSelected;
            public int X;
            public int Y;

            public void SetSize(int height)
            {
                this.Height = height;
                this.Width = (int)(Art.Width * height / Art.Height);
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
                if (IsSelected)
                    graphics.DrawRectangle(selectedPen, area);
                if (IsPinned)
                    graphics.DrawRectangle(pinnedPen, area);
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

            private void Reset()
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
                    if (searchWord != MainThread.textBox1.Text)
                    {
                        searchWord = MainThread.textBox1.Text.ToLower();
                        this.Reset();
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
        //public List<ImageTile> FilteredTiles { get; set; } = new List<ImageTile>();
        public List<ImageTile> SelectedTiles { get; set; } = new List<ImageTile>();
        public List<ImageTile> PinnedTiles { get; set; } = new List<ImageTile>();
        public List<Row> ShownRows { get; set; } = new List<Row>();
        public int CurrentRow { get; set; } = 0;

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

        public LibraryImageSelectionDialog()
        {
            InitializeComponent();
            this.MouseWheel += LibraryImageSelectionDialog_MouseWheel;

            this.DoubleBuffered = true;

            this.Thread2 = new CheckingThread(this);
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

            foreach (var tile in SelectedTiles)
                AddTile(tile);
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
                    //this.FilteredTiles.Add(tile);

                    if (tile.IsPinned || tile.IsSelected)
                        continue;
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
            if (tile.IsSelected)
            {
                if (tile.IsPinned)
                    PinnedTiles.Remove(tile);
                else
                {
                    tile.IsSelected = false;
                    SelectedTiles.Remove(tile);
                    PinnedTiles.Add(tile);
                    PinnedTiles.Sort();
                }
            }
            else
            {
                if (tile.IsPinned)
                    PinnedTiles.Remove(tile);
                else
                {
                    PinnedTiles.Add(tile);
                    PinnedTiles.Sort();
                }
            }
            tile.IsPinned = !tile.IsPinned;
            ClearRows();
        }
        private void ToggleSelected(ImageTile tile)
        {
            if (tile.IsPinned)
            {
                tile.IsPinned = false;
                PinnedTiles.Remove(tile);
            }

            if (tile.IsSelected)
                SelectedTiles.Remove(tile);
            else
            {
                foreach (var item in SelectedTiles)
                {
                    item.IsSelected = false;
                    if (!item.IsPinned)
                    {
                        item.IsPinned = true;
                        PinnedTiles.Add(item);
                    }
                }
                PinnedTiles.Sort();
                SelectedTiles.Clear();
                SelectedTiles.Add(tile);
            }
            tile.IsSelected = !tile.IsSelected;

            ClearRows();
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

            Rectangle lowerRight = new Rectangle(0, 0, 750, 80);
            lowerRight.Offset(ClientSize.Width - lowerRight.Width, ClientSize.Height - lowerRight.Height);
            e.Graphics.FillRectangle(Brushes.White, lowerRight);
            e.Graphics.DrawString(
               SelectedTiles.Count + " selected, "
               + PinnedTiles.Count + " pinned, "
               + shownTiles + " shown, " +
               "Progress " + CurrentTile + " / " + Tiles.Count,
                Calibri, Brushes.Black, lowerRight.Location);
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
            this.IsClosed = false;
            this.Thread2.StartThread();
        }
        private void LibraryImageSelectionDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.IsClosed = true;
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

        private void LibraryImageSelectionDialog_MouseMove(object sender, MouseEventArgs e)
        {

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
    }
}
