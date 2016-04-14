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
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using System.Threading;

namespace GIF_Editor
{
    public class GifFrames
    {
        public List<List<Bitmap>> frames = new List<List<Bitmap>>();
        List<List<Bitmap>> originalFrames = new List<List<Bitmap>>();
        public int currentFrame = 0;
        public int currentLayer = 0;
        public List<List<Point>> layerPlacement = new List<List<Point>>();
        Size mainSize;


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
            Point p = new Point(0, 0);
            Bitmap[] tempFrames = ToGifFrames(b);
            mainSize = new Size(tempFrames[0].Width, tempFrames[0].Height);

            for (int i = 0; i < tempFrames.Length; i++)
            {
                frames.Add(new List<Bitmap>());
                frames[i].Add(tempFrames[i]);
                layerPlacement.Add(new List<Point>());
                layerPlacement[i].Add(p);
            }

            Bitmap[] tempFramesTwo = ToGifFrames(b);

            for (int i = 0; i < tempFrames.Length; i++)
            {
                originalFrames.Add(new List<Bitmap>());
                originalFrames[i].Add(tempFramesTwo[i]);
            }
        }

        /// <summary>
        /// Turn filepath into GifFrames item
        /// </summary>
        /// <param name="gifFilePath"></param>
        public GifFrames(string filePath)
        {
            Point p = new Point(0, 0);
            Bitmap[] tempFrames = ToGifFrames(new Bitmap(filePath));
            mainSize = new Size(tempFrames[0].Width, tempFrames[0].Height);

            for (int i = 0; i < tempFrames.Length; i++)
            {
                frames.Add(new List<Bitmap>());
                frames[i].Add(tempFrames[i]);
                layerPlacement.Add(new List<Point>());
                layerPlacement[i].Add(p);
            }

            Bitmap[] tempFramesTwo = ToGifFrames(new Bitmap(filePath));

            for (int i = 0; i < tempFrames.Length; i++)
            {
                originalFrames.Add(new List<Bitmap>());
                originalFrames[i].Add(tempFramesTwo[i]);
            }
        }

        /// <summary>
        /// Sets the next frame of the GifFrame object
        /// </summary>
        /// <returns>The next frame</returns>
        public Bitmap NextBackground()
        {
            if (currentFrame < frames.Count - 1)
            {
                currentLayer = 0;
                currentFrame += 1;
                return frames[currentFrame][currentLayer];
            }
            else
                return null;
        }

        public void Next()
        {
            currentLayer = 0;
            currentFrame++;
        }

        /// <summary>
        /// Returns the current GifFrame
        /// </summary>
        /// <returns>The current frame</returns>
        public Bitmap GetFrame()
        {
            return BitmapSharp.MergeBitmaps(frames[currentFrame].ToArray(), frames[0][0].Width, frames[0][0].Height, layerPlacement[currentFrame].ToArray());
        }

        public Bitmap GetCurrentLayer()
        {
            return frames[currentFrame][currentLayer];
        }

        /// <summary>
        /// Returns the frame specified
        /// </summary>
        /// <param name="frameNumber">The frame that is wanted</param>
        /// <returns></returns>
        public Bitmap GetFrame(int frameNumber, int layerNumber)
        {
            if (frameNumber > frames.Count || frameNumber < 0)
                throw new IndexOutOfRangeException("The frameNumber must be within the GifFrames frame amount");

            return frames[frameNumber][layerNumber];
        }

        public Bitmap GetFrame(int layerNumber)
        {
            return frames[currentFrame][layerNumber];
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
        public Bitmap PreviousBackground()
        {
            if (currentFrame > 0)
            {
                currentLayer = 0;
                currentFrame -= 1;
                return frames[currentFrame][currentLayer];
            }
            else
                return null;
        }

        public void Previous()
        {
            currentLayer = 0;
            currentFrame--;
        }

        /// <summary>
        /// Sets the current frame to the bitmap
        /// </summary>
        /// <param name="frame">The bitmap that the frame will be set to</param>
        public void SetFrame(Bitmap frame, int layer)
        {
            frames[currentFrame][layer] = frame;
        }

        /// <summary>
        /// Gets the width of the gif in pixels
        /// </summary>
        /// <returns>Width of the gif in pixels</returns>
        public int Width()
        {
            return frames[0][0].Width;
        }

        /// <summary>
        /// Gets the height of the gif in pixels
        /// </summary>
        /// <returns>Height of gif in pixels</returns>
        public int Height()
        {
            return frames[0][0].Height;
        }

        /// <summary>
        /// Sets the specified frame to the specified bitmap
        /// </summary>
        /// <param name="frame">The bitmap that the frame will be set to</param>
        /// <param name="frameNumber">The frame that the bitmap will be set to</param>
        /// <param name="layer">The layer to set the bitmap to</param>
        public void SetFrame(Bitmap frame, int frameNumber, int layer)
        {
            if (frameNumber > frames.Count || frameNumber < 0)
                throw new IndexOutOfRangeException("The frameNumber must be within the GifFrames frame amount");

            frames[frameNumber][layer] = frame;
        }

        /// <summary>
        /// Export all the frames into a gif
        /// </summary>
        /// <param name="path">The file path of the exported gif</param>
        /// <param name="frameDelay">The target frame delay</param>
        public void Export(string path)
        {
            GifBitmapEncoder gEnc = new GifBitmapEncoder();

            foreach (Bitmap bmpImage in frames[currentLayer])
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
            return originalFrames[currentFrame][currentLayer];
        }

        public Bitmap[] GetOriginalFrameLayers()
        {
            return originalFrames[currentFrame].ToArray();
        }

        /// <summary>
        /// Gets the original frame that is specified
        /// </summary>
        /// <param name="frameNumber"></param>
        /// <returns>The orginal frame specified</returns>
        public Bitmap GetOriginalFrame(int frameNumber, int layer)
        {
            if (frameNumber > frames.Count || frameNumber < 0)
                throw new IndexOutOfRangeException("The frameNumber must be within the GifFrames frame amount");

            return originalFrames[frameNumber][layer];
        }

        /// <summary>
        /// Adds a frame to the end of the GifFrame
        /// </summary>
        public void AddFrame()
        {
            Bitmap b = new Bitmap(frames[0][0].Width, frames[0][0].Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(b))
            {
                g.FillRectangle(Brushes.White, 0, 0, b.Width, b.Height);
                frames.Add(new List<Bitmap>());
                frames[frames.Count].Add(new Bitmap(b.Width, b.Height, g));
                originalFrames.Add(new List<Bitmap>());
                originalFrames[originalFrames.Count].Add(new Bitmap(b.Width, b.Height, g));
            }
        }

        /// <summary>
        /// Adds a layer to the current frame
        /// </summary>
        public void AddLayer()
        {
            Bitmap b = new Bitmap(frames[0][0].Width, frames[0][0].Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(b))
            {
                g.FillRectangle(Brushes.Transparent, 0, 0, b.Width, b.Height);
                frames[currentFrame].Add(new Bitmap(b.Width, b.Height, g));
                originalFrames[currentFrame].Add(new Bitmap(b.Width, b.Height, g));
            }

            layerPlacement[currentFrame].Add(new Point(0, 0));
        }

        public Bitmap AddAndReturnLayer()
        {
            Bitmap b = new Bitmap(frames[0][0].Width, frames[0][0].Height, PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(b);

            g.FillRectangle(Brushes.Transparent, 0, 0, b.Width, b.Height);
            frames[currentFrame].Add(new Bitmap(b.Width, b.Height, g));
            originalFrames[currentFrame].Add(new Bitmap(b.Width, b.Height, g));


            layerPlacement[currentFrame].Add(new Point(0, 0));
            return new Bitmap(b.Width, b.Height, g);
        }

        /// <summary>
        /// Sets the current layer up one
        /// </summary>
        public void LayerUp()
        {
            currentLayer++;
        }

        /// <summary>
        /// Sets the current layer down one
        /// </summary>
        public void LayerDown()
        {
            currentLayer--;
        }

        /// <summary>
        /// Sets the current layer of the current frame to the specified Bitmap
        /// </summary>
        /// <param name="frame">Frame to set</param>
        public void SetFrame(Bitmap frame)
        {
            frames[currentFrame][currentLayer] = frame;
        }

        /// <summary>
        /// Gets all layers on current frame
        /// </summary>
        /// <returns>All layers on current frame</returns>
        public Bitmap[] GetCurrentFrameLayers()
        {
            Bitmap[] layers = new Bitmap[frames[currentFrame].Count];

            for (int i = 0; i < layers.Length; i++)
                layers[i] = frames[currentFrame][i];

            return layers;
        }

        /// <summary>
        /// Sets all the layers as long as the specified bitmap is the same length as the current frames layers
        /// </summary>
        /// <param name="layers">All the bitmaps to set the layers to</param>
        public void SetAllLayers(Bitmap[] layers)
        {
            for (int i = 0; i < layers.Length; i++)
                frames[currentFrame][i] = layers[i];
        }

        public int LayerCount()
        {
            return frames[currentFrame].Count;
        }

        public void RemoveLayer(int index)
        {
            frames[currentFrame].RemoveAt(index);
        }

        public void AddLayer(Bitmap layer)
        {
            frames[currentFrame].Add(layer);
            originalFrames[currentFrame].Add(layer);
            layerPlacement[currentFrame].Add(new Point((Math.Abs(layer.Width - mainSize.Width)) / 2, (Math.Abs(layer.Height - mainSize.Height)) / 2));
        }

        public Point GetPoint()
        {
            return layerPlacement[currentFrame][currentLayer];
        }

        public void SetPoint(Point p)
        {
            layerPlacement[currentFrame][currentLayer] = p;
        }

        public Bitmap CreateAndDisposeLayer()
        {
            Bitmap b = new Bitmap(frames[0][0].Width, frames[0][0].Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(b))
            {
                g.FillRectangle(Brushes.Transparent, 0, 0, b.Width, b.Height);
            }

            return b;
        }
    }
}
