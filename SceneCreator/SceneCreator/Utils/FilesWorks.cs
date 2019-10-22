using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SceneCreator.Utils
{
    public static class FilesWorks
    {
        private static string initialDirectory=string.Empty;

        public static Utils.ImageUI GetFilesByteArray(string FileExtension,int name)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = FileExtension; //"Изображения | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.gif; *.bmp; *.tif; *.tiff; |Все файлы (*.*)|*.*";
                ofd.FilterIndex = 0;
                ofd.Multiselect = true;
                ofd.InitialDirectory = initialDirectory;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (System.IO.File.Exists(ofd.FileName))
                    {
                        initialDirectory = ofd.InitialDirectory;
                        using (Image img = Image.FromFile(ofd.FileName))
                        {
                            Utils.ImageUI imageUI = new ImageUI();
                            imageUI.Name = name;
                            imageUI.Image = ImageToByte(img);
                            imageUI.PreviewBig = ImageToByte(ResizeImage(img, 255,255));
                            imageUI.PreviewSmall = ImageToByte(ResizeImage(img, 32, 32));
                            return imageUI;
                        }
                    }
                }
            }
            return null;
        }


        public static Image Resize(Image image, int newWidth, int maxHeight, bool onlyResizeIfWider)
        {
            

            if (onlyResizeIfWider && image.Width <= newWidth) newWidth = image.Width;

            var newHeight = image.Height * newWidth / image.Width;
            if (newHeight > maxHeight)
            {
                newWidth = image.Width * maxHeight / image.Height;
                newHeight = maxHeight;
            }

            var res = new Bitmap(newWidth, newHeight);
            using (var graphic = Graphics.FromImage(res))
            {
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphic.CompositingQuality = CompositingQuality.HighQuality;
                graphic.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return res;
        }

        internal static byte[] ImageToByte(Image preview)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    preview.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка [ImageToByte]!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GC.Collect();
                return null;
            }
        }

        internal static Image ByteToImage(byte[] value)
        {
            if (value != null)
            {
                using (MemoryStream mStream = new MemoryStream(value))
                {
                    return Image.FromStream(mStream);
                }
            }
            else { return null; }
        }

        public static byte[] GetBytesFromImage(string imageFile, int name)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                if (File.Exists(imageFile))
                {
                    try
                    {
                        using (Image img = Image.FromFile(imageFile))
                        {
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            return ms.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка изображения!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        GC.Collect();
                        return null;
                    }
                }
                else { return null; }
            }
        }







        public static Image ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }



    }
}
