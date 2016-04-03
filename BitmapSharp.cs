//Copyright (C) 2016 Jason Heddle

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//   http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

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
    }
}
