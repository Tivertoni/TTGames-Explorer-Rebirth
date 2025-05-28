using DarkUI.Controls;
using DarkUI.Forms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using TTGamesExplorerRebirthLib.Formats;
using TTGamesExplorerRebirthLib.Formats.DDS;
using Image = System.Drawing.Image;

namespace TTGamesExplorerRebirthUI.Forms
{
    public enum ImageFormType
    {
        DDS,
        NXGTextures,
        TGA,
        PNG,
    }

    public partial class ImageForm : DarkForm
    {
        private readonly List<string> _ddsNames = [];
        private readonly List<byte[]> _ddsFilesRaw = [];

        private readonly string _filePath;
        private readonly bool _isDDS;

        private System.Drawing.Image _previewImage;

        private bool _transparentBackground = true;
        private int _previewWidth;
        private int _previewHeight;
        private int _zoomVal = 100;

        [GeneratedRegex(@"\d+")]
        private static partial Regex Regex_FirstDigit();

        public ImageForm(string filePath, byte[] fileBuffer, ImageFormType type)
        {
            InitializeComponent();

            _filePath = filePath;

            if (type == ImageFormType.DDS)
            {
                _isDDS = true;

                _ddsFilesRaw = [fileBuffer];

                LoadImages();
            }
            else if (type == ImageFormType.NXGTextures)
            {
                _isDDS = true;

                NXGTextures nxgTextures = new(fileBuffer);

                foreach (var nxgFile in nxgTextures.Files)
                {
                    _ddsNames.Add(nxgFile.Key);
                    _ddsFilesRaw.Add(nxgFile.Value);
                }

                LoadImages();

                darkButton2.Visible = true;
            }
            else if (type == ImageFormType.TGA)
            {
                TGA tga = new TGA();

                Bitmap map = tga.GetBitmap(filePath);

                LoadStandaloneImage(map, filePath);
            }
            else if (type == ImageFormType.PNG)
            {
                using (MemoryStream ms = new MemoryStream(fileBuffer))
                {
                    Bitmap bitmap = new Bitmap(ms);
                    LoadStandaloneImage(bitmap, filePath);
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            Helper.EnableDarkModeTitle(Handle);
        }
        private void LoadStandaloneImage(Bitmap map, string path)
        {
            string name = Path.GetFileName(path);

            DarkListItem item = new($"Image #0")
            {
                Icon = Properties.Resources.picture
            };

            darkListView1.Items.Add(item);

            _previewImage = map;
            _previewHeight = map.Height;
            _previewWidth = map.Width;
            pictureBox1.Image = map;
        }
        private void LoadImages()
        {
            if (_isDDS)
            {
                int i = 0;
                foreach (var file in _ddsFilesRaw)
                {
                    if (_ddsNames.Count != 0)
                    {
                        string name = Path.GetFileName(_ddsNames[i]);
                        if (name != null)
                        {
                            DarkListItem item2 = new($"{i + 1}: {name}")
                            {
                                Icon = Properties.Resources.picture,
                            };

                            darkListView1.Items.Add(item2);

                            i++;
                        }
                    }
                    else
                    {
                        DarkListItem item = new($"Image #{i + 1}")
                        {
                            Icon = Properties.Resources.picture
                        };

                        darkListView1.Items.Add(item);

                        i++;
                    }
                }

                if (_ddsFilesRaw.Count == 1)
                {
                    darkListView1.Enabled = false;
                }

                toolStripStatusLabel1.Text = $"{Path.GetFileName(_filePath)} - {_ddsFilesRaw.Count} Image(s)";
            }

            if (darkListView1.Items.Count > 0)
            {
                darkListView1.SelectItem(0);
            }
        }

        private void LoadDDSImage(uint index)
        {
            darkListView2.Items.Clear();

            DDSImage dds = new(_ddsFilesRaw[(int)index]);

            int i = 1;
            foreach (SixLabors.ImageSharp.Image image in dds.Images)
            {
                DarkListItem item = new($"MipMap #{i} ({image.Width}x{image.Height})")
                {
                    Icon = Properties.Resources.shape_shadow
                };

                darkListView2.Items.Add(item);

                i++;
            }

            if (dds.Images.Length > 0)
            {
                darkListView2.SelectItem(0);
            }

            if (dds.Images.Length == 1)
            {
                darkListView2.Enabled = false;
            }
        }

        private static uint GetIndexFromName(string input)
        {
            string str = Regex_FirstDigit().Match(input).Value ?? throw new("Could not find number");

            return uint.Parse(str) - 1;
        }

        private void DarkListView1_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (_isDDS)
            {
                uint index = GetIndexFromName(darkListView1.Items[darkListView1.SelectedIndices[0]].Text);

                LoadDDSImage(index);
            }
        }

        private void DarkListView2_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (darkListView2.SelectedIndices.Count > 0)
            {
                if (_isDDS)
                {
                    uint indexMipMap = GetIndexFromName(darkListView2.Items[darkListView2.SelectedIndices[0]].Text);
                    uint indexImage = GetIndexFromName(darkListView1.Items[darkListView1.SelectedIndices[0]].Text);

                    using MemoryStream stream = new();

                    DDSImage _ddsFile = new(_ddsFilesRaw[(int)indexImage]);

                    _ddsFile.Images[indexMipMap].Save(stream, PngFormat.Instance);

                    _zoomVal = trackBar1.Value = 100;

                    darkLabel1.Text = $"{_zoomVal}%";

                    _previewImage = new Bitmap(stream);
                    _previewWidth = _ddsFile.Images[indexMipMap].Width;
                    _previewHeight = _ddsFile.Images[indexMipMap].Height;

                    pictureBox1.Image = new Bitmap(stream);
                }
            }
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value <= 0)
            {
                return;
            }

            _zoomVal = trackBar1.Value;

            darkLabel1.Text = $"{_zoomVal}%";

            int newWidth = (int)(_previewWidth * _zoomVal / 100.0);
            int newHeight = (int)(_previewHeight * _zoomVal / 100.0);

            pictureBox1.Image = PictureBoxZoom(_previewImage, new System.Drawing.Size(newWidth, newHeight));
        }

        public System.Drawing.Image PictureBoxZoom(System.Drawing.Image img, System.Drawing.Size size)
        {
            int width = size.Width <= 0 ? 1 : size.Width;
            int height = size.Height <= 0 ? 1 : size.Height;

            Bitmap bitmap = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, width, height);
            }

            return bitmap;
        }

        private void DarkButton1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Portable Network Graphic files (*.png)|*.png";
            saveFileDialog1.DefaultExt = "png";
            saveFileDialog1.Title = "Save as PNG...";

            int i = darkListView1.SelectedIndices.Count > 0 ? darkListView1.SelectedIndices[0] : 0;
            uint indexImage = GetIndexFromName(darkListView1.Items[i].Text);

            saveFileDialog1.FileName = _ddsNames.Count != 0 ? $"{Path.GetFileName(_ddsNames[(int)indexImage])}.png" : $"{Path.GetFileNameWithoutExtension(_filePath)}.png";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _previewImage.Save(saveFileDialog1.FileName);

                MessageBox.Show("File saved!", "Save as PNG...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DarkButton2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "DirectDraw Surface files (*.dds)|*.dds";
            saveFileDialog1.DefaultExt = "dds";
            saveFileDialog1.Title = "Save as DDS...";

            uint indexImage = GetIndexFromName(darkListView1.Items[darkListView1.SelectedIndices[0]].Text);

            saveFileDialog1.FileName = $"{Path.GetFileName(_ddsNames[(int)indexImage])}.dds";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(saveFileDialog1.FileName, _ddsFilesRaw[(int)indexImage]);

                MessageBox.Show("File saved!", "Save as DDS...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DarkButton3_Click(object sender, EventArgs e)
        {
            _transparentBackground = !_transparentBackground;

            if (_transparentBackground)
            {
                pictureBox1.BackColor = System.Drawing.Color.Transparent;

                return;
            }

            pictureBox1.BackColor = System.Drawing.Color.Black;
        }
    }
}
