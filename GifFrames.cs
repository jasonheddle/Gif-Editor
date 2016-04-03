using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;

namespace GIF_Editor
{
    public class GifFrames
    {
        public List<Bitmap> frames;
        List<Bitmap> originalFrames;
        public int currentFrame = 0;

        static Bitmap[] ToGifFrames(Bitmap b)
        {
            int length = b.GetFrameCount(FrameDimension.Time);
            Bitmap[] bFrames = new Bitmap[length];

            for (int i = 0; i < length; i++)
            {
                b.SelectActiveFrame(FrameDimension.Time, i);
                bFrames[i] = new Bitmap(b.Width, b.Height);
                Graphics.FromImage(bFrames[i]).DrawImage(b, new System.Drawing.Point(0, 0));
            }

            return bFrames;
        }

        /// <summary>
        /// Turn Bitmap into GifFrames item
        /// </summary>
        /// <param name="gifBitmap">The bitmap to change split into seperate frames</param>
        public GifFrames(Bitmap b)
        {
            frames = ToGifFrames(b).ToList();
            originalFrames = frames;
        }

        /// <summary>
        /// Turn filepath into GifFrames item
        /// </summary>
        /// <param name="gifFilePath"></param>
        public GifFrames(string filePath)
        {
            frames = ToGifFrames(new Bitmap(filePath)).ToList();
            originalFrames = frames;
        }

        /// <summary>
        /// Sets the next frame of the GifFrame object
        /// </summary>
        /// <returns>The next frame</returns>
        public Bitmap Next()
        {
            if (currentFrame < frames.Count - 1)
            {
                currentFrame += 1;
                return frames[currentFrame];
            }
            else
                return null;
        }

        /// <summary>
        /// Returns the current GifFrame
        /// </summary>
        /// <returns>The current frame</returns>
        public Bitmap GetFrame()
        {
            return frames[currentFrame];
        }

        /// <summary>
        /// Returns the frame specified
        /// </summary>
        /// <param name="frameNumber">The frame that is wanted</param>
        /// <returns></returns>
        public Bitmap GetFrame(int frameNumber)
        {
            if (frameNumber > frames.Count || frameNumber < 0)
                throw new IndexOutOfRangeException("The frameNumber must be within the GifFrames frame amount");

            return frames[frameNumber];
        }

        /// <summary>
        /// Sets the currrent frame to the frame number specified
        /// </summary>
        /// <param name="frameNumber">The frame to make current</param>
        public void SetCurrentFrame(int frameNumber)
        {
            if (frameNumber > frames.Count || frameNumber < 0)
                throw new IndexOutOfRangeException("The frameNumber must be within the GifFrames frame amount");

            currentFrame = frameNumber;
        }

        /// <summary>
        /// Sets the currentframe of the GifFrame object to 1 less
        /// </summary>
        /// <returns>The previous frame</returns>
        public Bitmap Previous()
        {
            if (currentFrame > 0)
            {
                currentFrame -= 1;
                return frames[currentFrame];
            }
            else
                return null;
        }

        /// <summary>
        /// Sets the current frame to the bitmap
        /// </summary>
        /// <param name="frame">The bitmap that the frame will be set to</param>
        public void SetFrame(Bitmap frame)
        {
            frames[currentFrame] = frame;
        }

        /// <summary>
        /// Gets the width of the gif in pixels
        /// </summary>
        /// <returns>Width of the gif in pixels</returns>
        public int Width()
        {
            return frames[0].Width;
        }

        /// <summary>
        /// Gets the height of the gif in pixels
        /// </summary>
        /// <returns>Height of gif in pixels</returns>
        public int Height()
        {
            return frames[0].Height;
        }

        /// <summary>
        /// Sets the specified frame to the specified bitmap
        /// </summary>
        /// <param name="frame">The bitmap that the frame will be set to</param>
        /// <param name="frameNumber">The frame that the bitmap will be set to</param>
        public void SetFrame(Bitmap frame, int frameNumber)
        {
            if (frameNumber > frames.Count || frameNumber < 0)
                throw new IndexOutOfRangeException("The frameNumber must be within the GifFrames frame amount");

            frames[frameNumber] = frame;
        }

        /// <summary>
        /// Export all the frames into a gif
        /// </summary>
        /// <param name="path">The file path of the exported gif</param>
        /// <param name="frameDelay">The target frame delay</param>
        public void Export(string path)
        {
            GifBitmapEncoder gEnc = new GifBitmapEncoder();

            foreach (Bitmap bmpImage in frames)
            {
                var src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                gEnc.Frames.Add(BitmapFrame.Create(src));
            }
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                gEnc.Save(fs);
            }
        }

        /// <summary>
        /// Gets the original frame that corresponds to the current frame
        /// </summary>
        /// <returns>Original frame of the current frame</returns>
        public Bitmap GetOriginalFrame()
        {
            return originalFrames[currentFrame];
        }

        /// <summary>
        /// Gets the original frame that is specified
        /// </summary>
        /// <param name="frameNumber"></param>
        /// <returns>The orginal frame specified</returns>
        public Bitmap GetOriginalFrame(int frameNumber)
        {
            if (frameNumber > frames.Count || frameNumber < 0)
                throw new IndexOutOfRangeException("The frameNumber must be within the GifFrames frame amount");

            return originalFrames[frameNumber];
        }

        /// <summary>
        /// Adds a frame to the end of the GifFrame
        /// </summary>
        public void AddFrame()
        {
            Bitmap b = new Bitmap(frames[0].Width, frames[0].Height);

            using (Graphics g = Graphics.FromImage(b))
            {
                g.FillRectangle(Brushes.White, 0, 0, b.Width, b.Height);
                frames.Add(new Bitmap(b.Width, b.Height, g));
                originalFrames.Add(new Bitmap(b.Width, b.Height, g));
            }
        }

        /// <summary>
        /// Adds a frame at the specified index
        /// </summary>
        /// <param name="index">The zero based index at which the item should be inserted</param>
        public void AddFrame(int ins)
        {
            Bitmap b = new Bitmap(frames[0].Width, frames[0].Height);

            using (Graphics g = Graphics.FromImage(b))
            {
                g.FillRectangle(Brushes.White, 0, 0, b.Width, b.Height);
                frames.Insert(ins, new Bitmap(b.Width, b.Height, g));
                originalFrames.Insert(ins, new Bitmap(b.Width, b.Height, g));
            }
        }
    }
}
