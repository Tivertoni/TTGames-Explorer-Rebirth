using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
namespace TTGamesExplorerRebirthLib.Formats
{
    using Image = SixLabors.ImageSharp.Image;
    public class TGA
    {
        public Bitmap GetBitmap(string path)
        {
            Image image = Image.Load<Rgba32>(path);

            return ToBitmap(image);
        }
        private System.Drawing.Bitmap ToBitmap(Image image)
        {
            using (var memoryStream = new MemoryStream())
            {
                var imageEncoder = image.Configuration.ImageFormatsManager.GetEncoder(PngFormat.Instance);
                image.Save(memoryStream, imageEncoder);

                memoryStream.Seek(0, SeekOrigin.Begin);

                return new System.Drawing.Bitmap(memoryStream);
            }
        }
    }
}
