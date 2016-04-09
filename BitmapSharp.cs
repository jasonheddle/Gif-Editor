using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIF_Editor
{
    public class BitmapSharp
    {
        /// <summary>
        /// Compares 2 bitmaps too see if they are identical
        /// </summary>
        /// <param name="bitmap1"></param>
        /// <param name="bitmap2"></param>
        /// <returns>A bool of whether or not the bitmaps are identical</returns>
        public static bool CompareBitmap(Bitmap bmp1, Bitmap bmp2)
        {
            bool equals = true;
            Rectangle rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);
            BitmapData bmpData1 = bmp1.LockBits(rect, ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bmpData2 = bmp2.LockBits(rect, ImageLockMode.ReadOnly, bmp2.PixelFormat);
            unsafe
            {
                byte* ptr1 = (byte*)bmpData1.Scan0.ToPointer();
                byte* ptr2 = (byte*)bmpData2.Scan0.ToPointer();
                int width = rect.Width * 3; // for 24bpp pixel data
                for (int y = 0; equals && y < rect.Height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (*ptr1 != *ptr2)
                        {
                            equals = false;
                            break;
                        }
                        ptr1++;
                        ptr2++;
                    }
                    ptr1 += bmpData1.Stride - width;
                    ptr2 += bmpData2.Stride - width;
                }
            }
            bmp1.UnlockBits(bmpData1);
            bmp2.UnlockBits(bmpData2);
            return equals;
        }

        /// <summary>
        /// Merges an array of bitmaps into one bitmap
        /// </summary>
        /// <param name="bitmaps"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Bitmap MergeBitmaps(Bitmap[] bitmaps, Size size)
        {
            Bitmap finalBitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            finalBitmap.MakeTransparent();
            using (Graphics g = Graphics.FromImage(finalBitmap))
            {

                foreach (var bitmap in bitmaps)
                {
                    g.DrawImage(bitmap, (size.Width - bitmap.Width) / 2, (size.Height - bitmap.Height) / 2);
                }

                return finalBitmap;
            }
        }

        public static Bitmap MergeBitmaps(Bitmap[] bitmaps, int width, int height)
        {
            return MergeBitmaps(bitmaps, new Size(width, height));
        }
    }
}
