using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace GIF_Editor
{
    static class BitmapExtensions
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Converts a Bitmap to a BitmapImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>A BitmapImage of the Bitmap</returns>
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapImage retval;

            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()).ToBitmapImage();
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }

        /// <summary>
        /// Converts BitmapSource to BitmapImage
        /// </summary>
        /// <param name="bitmapSource"></param>
        /// <returns>A BitmapImage of the BitmapSource</returns>
        public static BitmapImage ToBitmapImage(this BitmapSource bitmapSource)
        {
            BitmapImage bImg = new BitmapImage();
            var encoder = new PngBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = memoryStream;
            bImg.EndInit();

            return bImg;
        }

        /// <summary>
        /// Converts a WriteableBitmap to a Bitmap
        /// </summary>
        /// <param name="writeableBitmap"></param>
        /// <returns>A Bitmap of the WriteableBitmap</returns>
        public static Bitmap ToBitmap(this WriteableBitmap writeBmp)
        {
            Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new Bitmap(outStream);
            }
            return bmp;
        }

        /// <summary>
        /// Converts a BitmapSource to a Bitmap
        /// </summary>
        /// <param name="bitmapSource"></param>
        /// <returns>A Bitmap of the BitmapSource</returns>
        public static Bitmap ToBitmap(this BitmapSource srs)
        {
            int width = srs.PixelWidth;
            int height = srs.PixelHeight;
            int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(height * stride);
                srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
                using (var btm = new Bitmap(width, height, stride, PixelFormat.Format1bppIndexed, ptr))
                {
                    return new Bitmap(btm);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Converts a Bitmap to a WriteableBitmap
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>A WriteableBitmap of the Bitmap</returns>
        public static WriteableBitmap ToWritableBitmap(this Bitmap b)
        {
            return new WriteableBitmap(b.ToBitmapImage());
        }

        /// <summary>
        /// Converts an ImageSource to Image
        /// </summary>
        /// <param name="imageSource"></param>
        /// <returns>An Image of the ImageSource</returns>
        public static Image ToImage(this System.Windows.Media.ImageSource image)
        {
            MemoryStream ms = new MemoryStream();
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image as BitmapSource));
            encoder.Save(ms);
            ms.Flush();
            return Image.FromStream(ms);
        }

        /// <summary>
        /// Converts an ImageSource to a Bitmap
        /// </summary>
        /// <param name="imageSource"></param>
        /// <returns>A Bitmap of the ImageSource</returns>
        public static Bitmap ToBitmap(this System.Windows.Media.ImageSource image)
        {
            return new Bitmap(image.ToImage());
        }
    }
}
