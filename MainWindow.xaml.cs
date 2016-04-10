using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Path = System.IO.Path;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GIF_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // TODO:
        // Fix the colour shower so it doesn't overlap the GIF being edited : DONE
        // Add image editing support such as: resizing, rotating, moving, and cropping
        // Fix eraser tool so it doesn't do spotty erasing : DONE
        // Fix paintbrush tool so it doesn't do spotty drawing like the eraser erases : DONE
        // Add paintbrush tool so users can draw with something other than 1px pencil tool : DONE
        // Add option for user to change paintbrush thickness : DONE
        // fix pencil tool so each line segment is attached, not seperated causing weird white spaces : DONE
        // Add ability to add a frame
        // Add ability to add a layer : DONE
        // Add ability to add images : DONE

        int undoAmounts = 0;
        System.Windows.Media.Color mainColor = Colors.Red;
        static string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
        Cursor pencil = new Cursor($@"{folder}\pencil.cur");
        Cursor eraser = new Cursor($@"{folder}\eraser.cur");
        Cursor dropper = new Cursor($@"{folder}\dropper.cur");
        Cursor paintbrush = new Cursor($@"{folder}\paintbrush.cur");
        Cursor resizeLeft = new Cursor($@"{folder}\resize left.cur");
        Cursor resizeRight = new Cursor($@"{folder}\resize right.cur");
        bool pencilSelected = false;
        bool eraserSelected = false;
        bool dropperSelected = false;
        bool lineSelected = false;
        bool paintbrushSelected = false;
        bool resizeSelected = false;
        bool rotateSelected = false;
        WriteableBitmap colorBitmap = new Bitmap(75, 25).ToWritableBitmap();
        RoutedEventArgs routedEventArgs = new RoutedEventArgs();
        bool ctrlPress = false;
        WriteableBitmap b;
        Bitmap b2;
        System.Windows.Point endPoint;
        bool click = false;
        System.Windows.Point startPoint;
        static GifFrames gifFrames = new GifFrames(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\giphy.gif");
        List<LinkedList<Bitmap>> undoList = new List<LinkedList<Bitmap>>();
        List<LinkedList<Bitmap>> redoList = new List<LinkedList<Bitmap>>();
        List<int> editAmount = new List<int>();
        bool[][] pixelsEdited;
        string saveFilePath = null;
        System.Windows.Point pointOne;
        System.Windows.Point pointTwo;
        bool line = false;
        int thickness = 10;
        LayerWindow layerWindow = new LayerWindow();
        internal static int selectedLayer = 0;
        Bitmap[] currentFrameLayers;
        public static MainWindow Current;
        bool mainClick = false;
        System.Windows.Point mainStartPoint;
        System.Windows.Point mainEndPoint;
        List<List<System.Windows.Controls.Image>> imageList = new List<List<System.Windows.Controls.Image>>();

        public MainWindow()
        {
            InitializeComponent();

            AddLayersToList();

            frameTextBlock.Text = $"Frame: 1 / {gifFrames.frames.Count}";

            for (int i = 0; i < gifFrames.frames.Count; i++)
            {
                undoList.Add(new LinkedList<Bitmap>());
                redoList.Add(new LinkedList<Bitmap>());
                editAmount.Add(0);
                imageList.Add(new List<System.Windows.Controls.Image>());
            }

            for (int x = 0; x < colorBitmap.Width; x++)
                for (int y = 0; y < colorBitmap.Height; y++)
                    colorBitmap.SetPixel(x, y, mainColor);

            colorShower.Source = colorBitmap;

            KeyDown += new KeyEventHandler(KeyPress);
            KeyUp += new KeyEventHandler(KeyRelease);

            Main.WindowState = WindowState.Maximized;
            currentFrameLayers = gifFrames.GetCurrentFrameLayers();
            imageBox.Source = gifFrames.GetFrame().ToWritableBitmap();
            previousFrame.IsEnabled = false;
            imageBox.Width = gifFrames.Width();
            imageBox.Height = gifFrames.Height();
            scrollViewerGrid.Width = gifFrames.Width();
            scrollViewerGrid.Height = gifFrames.Height();
            scrollViewerGrid.Width = gifFrames.Width();
            scrollViewerGrid.Height = gifFrames.Height();
            Current = this;
        }

        private void previousFrame_Click(object sender, RoutedEventArgs e)
        {
            pixelsEdited = new bool[gifFrames.Height()][];
            for (int i = 0; i < gifFrames.Height(); i++)
                pixelsEdited[i] = new bool[gifFrames.Width()];

            click = false;
            gifFrames.SetFrame(imageBox.Source.ToBitmap(), 0);

            for (int i = 1; i <= imageList[gifFrames.currentFrame].Count; i++)
                gifFrames.SetFrame(imageList[gifFrames.currentFrame][i - 1].Source.ToBitmap(), i);

            for (int i = 0; i < scrollViewerGrid.Children.Count; i++)
                if (i != 0)
                    scrollViewerGrid.Children.RemoveAt(i);

            gifFrames.Previous();
            imageBox.Source = gifFrames.GetFrame(0).ToWritableBitmap();

            for (int i = 0; i < imageList[gifFrames.currentFrame].Count; i++)
                    scrollViewerGrid.Children.Add(imageList[gifFrames.currentFrame][i]);

            if (gifFrames.currentFrame == 0)
                previousFrame.IsEnabled = false;

            if (gifFrames.currentFrame < gifFrames.frames.Count - 1)
                nextFrame.IsEnabled = true;

            frameTextBlock.Text = $"Frame: {gifFrames.currentFrame + 1} / {gifFrames.frames.Count}";

            layerWindow.RemoveAll();
            for (int i = 0; i < gifFrames.LayerCount(); i++)
                layerWindow.AddItem($"Layer {i}");
        }

        private void nextFrame_Click(object sender, RoutedEventArgs e)
        {
            pixelsEdited = new bool[gifFrames.Height()][];
            for (int i = 0; i < gifFrames.Height(); i++)
                pixelsEdited[i] = new bool[gifFrames.Width()];

            click = false;
            gifFrames.SetFrame(imageBox.Source.ToBitmap(), 0);

            for (int i = 1; i <= imageList[gifFrames.currentFrame].Count; i++)
                gifFrames.SetFrame(imageList[gifFrames.currentFrame][i - 1].Source.ToBitmap(), i);

            for (int i = 0; i < scrollViewerGrid.Children.Count; i++)
                if (i != 0)
                    scrollViewerGrid.Children.RemoveAt(i);

            gifFrames.Next();
            imageBox.Source = gifFrames.GetFrame(0).ToWritableBitmap();

            for (int i = 0; i < imageList[gifFrames.currentFrame].Count; i++)
                scrollViewerGrid.Children.Add(imageList[gifFrames.currentFrame][i]);

            if (!(gifFrames.currentFrame < gifFrames.frames.Count - 1))
                nextFrame.IsEnabled = false;

            if (gifFrames.currentFrame > 0)
                previousFrame.IsEnabled = true;

            frameTextBlock.Text = $"Frame: {gifFrames.currentFrame + 1} / {gifFrames.frames.Count}";

            layerWindow.RemoveAll();
            for (int i = 0; i < gifFrames.LayerCount(); i++)
                layerWindow.AddItem($"Layer {i}");
        }

        private void saveAsButton_Click(object sender, RoutedEventArgs e)
        {
            ctrlPress = false;
            click = false;
            gifFrames.SetAllLayers(currentFrameLayers);
            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "Gif File (*.gif)|*.gif", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) };
            if (saveFileDialog.ShowDialog() == true)
            {
                string path = saveFileDialog.FileName;
                saveFilePath = path;
                gifFrames.Export(path);
            }
        }

        private void scrollViewerGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            click = true;
            startPoint = Mouse.GetPosition(selectedLayer != 0 ? imageList[gifFrames.currentFrame][selectedLayer - 1] : imageBox);

            if (eraserSelected || pencilSelected || lineSelected || paintbrushSelected)
            {
                undoList[gifFrames.currentFrame].AddFirst(imageBox.Source.ToBitmap());
                redoList[gifFrames.currentFrame].Clear();
                editAmount[gifFrames.currentFrame]++;
                undoAmounts = 0;
            }

            if (undoList[gifFrames.currentFrame].Count > 30)
                undoList[gifFrames.currentFrame].RemoveLast();

            if (lineSelected)
            {
                if (!line)
                {
                    pointOne = startPoint;
                    line = true;
                    b = selectedLayer != 0 ? imageList[gifFrames.currentFrame][selectedLayer - 1].Source as WriteableBitmap : imageBox.Source as WriteableBitmap;
                    b2 = b.ToBitmap();
                }
            }

            scrollViewerGrid_MouseMove(sender, new MouseEventArgs(e.MouseDevice, e.Timestamp));
        }

        private void scrollViewerGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (click && pencilSelected)
            {
                try
                {
                    b = selectedLayer != 0 ? imageList[gifFrames.currentFrame][selectedLayer - 1].Source as WriteableBitmap : imageBox.Source as WriteableBitmap;
                    endPoint = Mouse.GetPosition(selectedLayer != 0 ? imageList[gifFrames.currentFrame][selectedLayer - 1] : imageBox);
                    b.DrawLine((int)startPoint.X, (int)startPoint.Y, (int)endPoint.X, (int)endPoint.Y, mainColor);
                    startPoint = endPoint;
                }
                catch { }
            }

            if (click && eraserSelected)
            {
                new Thread(() =>
                {
                    Application.Current.Dispatcher.Invoke((Action)(() => b = selectedLayer != 0 ? imageList[gifFrames.currentFrame][selectedLayer - 1].Source as WriteableBitmap : imageBox.Source as WriteableBitmap));
                    Application.Current.Dispatcher.Invoke((Action)(() => endPoint = Mouse.GetPosition(selectedLayer != 0 ? imageList[gifFrames.currentFrame][selectedLayer - 1] : imageBox)));

                    System.Windows.Media.Color color;
                    System.Windows.Point[] linePixels = FindPixels.Line(startPoint, endPoint).ToArray();
                    int[][][] pixels = new int[linePixels.Length][][];

                    for (int i = 0; i < linePixels.Length; i++)
                        pixels[i] = FindPixels.Circle(thickness, (int)linePixels[i].X, (int)linePixels[i].Y);

                    Bitmap originalFrame = gifFrames.GetOriginalFrameLayers()[selectedLayer];

                    Application.Current.Dispatcher.Invoke((() =>
                    {
                        for (int j = 0; j < pixels.Length; j++)
                            for (int i = 0; i < pixels[j][0].Length; i++)
                                if (pixels[j][0][i] < pixelsEdited[0].Count() && pixels[j][0][i] >= 0 && pixels[j][1][i] < pixelsEdited.Count() && pixels[j][1][i] >= 0)
                                    if (!pixelsEdited[pixels[j][1][i]][pixels[j][0][i]])
                                    {
                                        color = System.Windows.Media.Color.FromArgb(originalFrame.GetPixel(pixels[j][0][i], pixels[j][1][i]).A, originalFrame.GetPixel(pixels[j][0][i], pixels[j][1][i]).R, originalFrame.GetPixel(pixels[j][0][i], pixels[j][1][i]).G, originalFrame.GetPixel(pixels[j][0][i], pixels[j][1][i]).B);

                                        try
                                        {
                                            b.SetPixel(pixels[j][0][i], pixels[j][1][i], color);
                                            pixelsEdited[pixels[j][1][i]][pixels[j][0][i]] = true;
                                        }
                                        catch { }
                                    }
                    }));

                    Application.Current.Dispatcher.Invoke((Action)(() => startPoint = endPoint));
                }).Start();
            }

            if (click && dropperSelected)
            {
                Bitmap currentFrame = selectedLayer != 0 ? imageList[gifFrames.currentFrame][selectedLayer - 1].Source.ToBitmap() : imageBox.Source.ToBitmap();
                mainColor = System.Windows.Media.Color.FromArgb(currentFrame.GetPixel((int)startPoint.X, (int)startPoint.Y).A, currentFrame.GetPixel((int)startPoint.X, (int)startPoint.Y).R, currentFrame.GetPixel((int)startPoint.X, (int)startPoint.Y).G, currentFrame.GetPixel((int)startPoint.X, (int)startPoint.Y).B);

                for (int x = 0; x < colorBitmap.Width; x++)
                    for (int y = 0; y < colorBitmap.Height; y++)
                        colorBitmap.SetPixel(x, y, System.Windows.Media.Color.FromRgb(mainColor.R, mainColor.G, mainColor.B));

                colorShower.Source = colorBitmap;
                startPoint = Mouse.GetPosition(imageBox);
            }

            if (lineSelected && line)
            {
                if (selectedLayer != 0)
                    imageList[gifFrames.currentFrame][selectedLayer - 1].Source = b2.ToWritableBitmap();
                else
                    imageBox.Source = b2.ToWritableBitmap();

                b = selectedLayer != 0 ? imageList[gifFrames.currentFrame][selectedLayer - 1].Source as WriteableBitmap : imageBox.Source as WriteableBitmap;
                b.DrawLineAa((int)pointOne.X, (int)pointOne.Y, (int)Mouse.GetPosition(scrollViewerGrid).X, (int)Mouse.GetPosition(scrollViewerGrid).Y, mainColor, thickness);
            }

            if (click && paintbrushSelected)
            {
                new Thread(() =>
                {
                    Application.Current.Dispatcher.Invoke((Action)(() => b = selectedLayer != 0 ? imageList[gifFrames.currentFrame][selectedLayer - 1].Source as WriteableBitmap : imageBox.Source as WriteableBitmap));
                    Application.Current.Dispatcher.Invoke((Action)(() => endPoint = Mouse.GetPosition(selectedLayer != 0 ? imageList[gifFrames.currentFrame][selectedLayer - 1] : imageBox)));

                    System.Windows.Point[] linePixels = FindPixels.Line(startPoint, endPoint).ToArray();
                    int[][][] pixels = new int[linePixels.Length][][];

                    for (int i = 0; i < linePixels.Length; i++)
                        pixels[i] = FindPixels.Circle(thickness, (int)linePixels[i].X, (int)linePixels[i].Y);

                    Application.Current.Dispatcher.Invoke((() =>
                    {
                    for (int j = 0; j < pixels.Length; j++)
                        for (int i = 0; i < pixels[j][0].Length; i++)
                            if (pixels[j][0][i] < pixelsEdited[0].Count() && pixels[j][0][i] >= 0 && pixels[j][1][i] < pixelsEdited.Count() && pixels[j][1][i] >= 0)
                                if (!pixelsEdited[pixels[j][1][i]][pixels[j][0][i]])
                                {
                                    try
                                    {
                                        b.SetPixel(pixels[j][0][i], pixels[j][1][i], mainColor);
                                        pixelsEdited[pixels[j][1][i]][pixels[j][0][i]] = true;
                                    }
                                    catch { }
                                }
                    }));

                    Application.Current.Dispatcher.Invoke((Action)(() => startPoint = endPoint));
                }).Start();
            }
        }

        private void Main_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mainClick = false;
            click = false;

            if (line)
            {
                pointTwo = startPoint;
                b.DrawLineAa((int)pointOne.X, (int)pointOne.Y, (int)pointTwo.X, (int)pointTwo.Y, mainColor, thickness);
                line = false;
            }
        }

        private void Main_MouseLeave(object sender, MouseEventArgs e)
        {
            click = false;
        }

        private void Main_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                click = true;
        }

        private void KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.N && nextFrame.IsEnabled)
                nextFrame_Click(sender, routedEventArgs);

            if (e.Key == Key.P && previousFrame.IsEnabled)
                previousFrame_Click(sender, routedEventArgs);
            
            if (e.Key == Key.LeftCtrl)
                ctrlPress = true;

            if (e.Key == Key.S && ctrlPress)
                saveButton_Click(sender, routedEventArgs);

            if (e.Key == Key.Z && ctrlPress)
                undoButton_Click(sender, routedEventArgs);

            if (e.Key == Key.R && ctrlPress)
                redoButton_Click(sender, routedEventArgs);

            if (e.Key == Key.OemPlus)
                thicknessSlider.Value++;

            if (e.Key == Key.OemMinus)
                thicknessSlider.Value--;

            if (e.Key == Key.I)
                imageList[gifFrames.currentFrame][selectedLayer - 1].Source.ToBitmap().Save("E:\\Documents\\Test.png");

            if (e.Key == Key.O)
                imageBox.Source.ToBitmap().Save("E:\\Documents\\Test.jpg");
        }

        private void KeyRelease(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                ctrlPress = false;
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (editAmount[gifFrames.currentFrame] > 0)
                {
                    pixelsEdited = new bool[gifFrames.Height()][];
                    for (int i = 0; i < gifFrames.Height(); i++)
                        pixelsEdited[i] = new bool[gifFrames.Width()];

                    undoAmounts++;
                    editAmount[gifFrames.currentFrame]--;
                    redoList[gifFrames.currentFrame].AddFirst(imageBox.Source.ToBitmap());
                    imageBox.Source = undoList[gifFrames.currentFrame].First().ToWritableBitmap();
                    undoList[gifFrames.currentFrame].RemoveFirst();
                }
            }
            catch { }
        }

        private void redoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (redoList[gifFrames.currentFrame].Count != 0)
                {
                    pixelsEdited = new bool[gifFrames.Height()][];
                    for (int i = 0; i < gifFrames.Height(); i++)
                        pixelsEdited[i] = new bool[gifFrames.Width()];

                    editAmount[gifFrames.currentFrame]++;
                    undoList[gifFrames.currentFrame].AddFirst(imageBox.Source.ToBitmap());
                    imageBox.Source = redoList[gifFrames.currentFrame].First().ToWritableBitmap();
                    redoList[gifFrames.currentFrame].RemoveFirst();
                }
            }
            catch { }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void eraseButton_Click(object sender, RoutedEventArgs e)
        {
            eraserSelected = !eraserSelected;
            if (eraserSelected)
            {
                pixelsEdited = new bool[gifFrames.Height()][];
                for (int i = 0; i < gifFrames.Height(); i++)
                    pixelsEdited[i] = new bool[gifFrames.Width()];
            }

            pencilSelected = false;
            dropperSelected = false;
            lineSelected = false;
            paintbrushSelected = false;
            resizeSelected = false;
            rotateSelected = false;

            pencilButton.IsChecked = false;
            paintbrushButton.IsChecked = false;
            dropperButton.IsChecked = false;
            lineButton.IsChecked = false;
            resizeButton.IsChecked = false;
            rotateButton.IsChecked = false;
        }

        private void pencilButton_Click(object sender, RoutedEventArgs e)
        {
            pencilSelected = !pencilSelected;
            eraserSelected = false;
            dropperSelected = false;
            lineSelected = false;
            paintbrushSelected = false;
            resizeSelected = false;
            rotateSelected = false;

            paintbrushButton.IsChecked = false;
            eraserButton.IsChecked = false;
            dropperButton.IsChecked = false;
            lineButton.IsChecked = false;
            resizeButton.IsChecked = false;
            rotateButton.IsChecked = false;
        }

        private void dropperButton_Click(object sender, RoutedEventArgs e)
        {
            dropperSelected = !dropperSelected;
            eraserSelected = false;
            pencilSelected = false;
            lineSelected = false;
            paintbrushSelected = false;
            resizeSelected = false;
            rotateSelected = false;

            pencilButton.IsChecked = false;
            eraserButton.IsChecked = false;
            paintbrushButton.IsChecked = false;
            lineButton.IsChecked = false;
            resizeButton.IsChecked = false;
            rotateButton.IsChecked = false;
        }

        private void colorSelection_Click(object sender, RoutedEventArgs e)
        {
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pixelsEdited = new bool[gifFrames.Height()][];
                for (int i = 0; i < gifFrames.Height(); i++)
                    pixelsEdited[i] = new bool[gifFrames.Width()];

                mainColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);

                for (int x = 0; x < colorBitmap.Width; x++)
                    for (int y = 0; y < colorBitmap.Height; y++)
                        colorBitmap.SetPixel(x, y, System.Windows.Media.Color.FromRgb(mainColor.R, mainColor.G, mainColor.B));

                colorShower.Source = colorBitmap;
            }
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Gif File (*.gif)|*.gif", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) };
            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                gifFrames = new GifFrames(path);
                undoList = new List<LinkedList<Bitmap>>();
                redoList = new List<LinkedList<Bitmap>>();
                editAmount = new List<int>();

                for (int i = 0; i < gifFrames.frames.Count; i++)
                {
                    undoList.Add(new LinkedList<Bitmap>());
                    redoList.Add(new LinkedList<Bitmap>());
                    editAmount.Add(0);
                }

                previousFrame.IsEnabled = false;
                imageBox.Width = gifFrames.Width();
                imageBox.Height = gifFrames.Height();
                scrollViewerGrid.Width = gifFrames.Width();
                scrollViewerGrid.Height = gifFrames.Height();

                saveFilePath = path;

                currentFrameLayers = gifFrames.GetCurrentFrameLayers();
                imageBox.Source = gifFrames.GetFrame().ToWritableBitmap();
                nextFrame.IsEnabled = gifFrames.frames.Count > 1;
                frameTextBlock.Text = $"Frame: {gifFrames.currentFrame + 1} / {gifFrames.frames.Count}";

                pixelsEdited = new bool[gifFrames.Height()][];
                for (int i = 0; i < gifFrames.Height(); i++)
                    pixelsEdited[i] = new bool[gifFrames.Width()];
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (saveFilePath != null)
            {
                ctrlPress = false;
                click = false;
                gifFrames.SetAllLayers(currentFrameLayers);
                gifFrames.Export(saveFilePath);
            }

            else
                saveAsButton_Click(sender, e);
        }

        private void lineButton_Click(object sender, RoutedEventArgs e)
        {
            paintbrushSelected = false;
            pencilSelected = false;
            eraserSelected = false;
            dropperSelected = false;
            resizeSelected = false;
            rotateSelected = false;

            pencilButton.IsChecked = false;
            eraserButton.IsChecked = false;
            dropperButton.IsChecked = false;
            paintbrushButton.IsChecked = false;
            resizeButton.IsChecked = false;
            rotateButton.IsChecked = false;

            lineSelected = !lineSelected;
            Mouse.OverrideCursor = null;
        }

        private void paintbrushButton_Click(object sender, RoutedEventArgs e)
        {
            pencilSelected = false;
            eraserSelected = false;
            dropperSelected = false;
            lineSelected = false;
            resizeSelected = false;
            rotateSelected = false;

            pencilButton.IsChecked = false;
            eraserButton.IsChecked = false;
            dropperButton.IsChecked = false;
            lineButton.IsChecked = false;
            resizeButton.IsChecked = false;
            rotateButton.IsChecked = false;

            paintbrushSelected = !paintbrushSelected;
            if (paintbrushSelected)
            {
                pixelsEdited = new bool[gifFrames.Height()][];
                for (int i = 0; i < gifFrames.Height(); i++)
                    pixelsEdited[i] = new bool[gifFrames.Width()];
            }
        }

        private void thicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            thickness = (int)thicknessSlider.Value;
        }

        private void addimageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "JPG File (*.jpg)|*.jpg|PNG File (*.png)|*.png", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), Multiselect = false };
            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                Bitmap addImage = new Bitmap(path);

                if (addImage.Width > imageBox.Width || addImage.Height > imageBox.Height)
                    addImage = addImage.Resize((int)(addImage.Width / (addImage.Height / imageBox.Height)), (int)(addImage.Height / (addImage.Height / imageBox.Height)));

                gifFrames.AddLayer(addImage);

                nextFrame_Click(sender, e);
                previousFrame_Click(sender, e);
            }
        }

        private void addlayerButton_Click(object sender, RoutedEventArgs e)
        {
            Bitmap foo = gifFrames.AddAndReturnLayer();
            imageList[gifFrames.currentFrame].Add(new System.Windows.Controls.Image()
            {
                Source = foo.ToWritableBitmap(),
                Width = imageBox.Width,
                Height = imageBox.Height,
                Opacity = 1
            });

            nextFrame_Click(sender, e);
            previousFrame_Click(sender, e);
        }

        private void addframeButton_Click(object sender, RoutedEventArgs e)
        {
            nextFrame.Content = selectedLayer;
        }

        private void scrollViewerGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!(resizeSelected || rotateSelected))
                Mouse.OverrideCursor = null;
        }

        private void scrollViewerGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!(resizeSelected || rotateSelected))
                Mouse.OverrideCursor = paintbrushSelected ? paintbrush : pencilSelected ? pencil : eraserSelected ? eraser : dropperSelected ? dropper : null;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            layerWindow.Topmost = true;
            layerWindow.Show();
        }

        private void Main_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        public static void AddLayer(object sender, RoutedEventArgs e)
        {
            Current.addlayerButton_Click(sender, e);
        }

        void AddLayersToList()
        {
            layerWindow.RemoveAll();
            for (int i = 0; i < gifFrames.LayerCount(); i++)
                layerWindow.AddItem($"Layer {i}");
        }

        private void resizeButton_Click(object sender, RoutedEventArgs e)
        {
            pencilSelected = false;
            eraserSelected = false;
            dropperSelected = false;
            lineSelected = false;
            paintbrushSelected = false;
            rotateSelected = false;

            pencilButton.IsChecked = false;
            eraserButton.IsChecked = false;
            dropperButton.IsChecked = false;
            lineButton.IsChecked = false;
            paintbrushButton.IsChecked = false;
            rotateButton.IsChecked = false;

            resizeSelected = !resizeSelected;
        }

        private void rotateButton_Click(object sender, RoutedEventArgs e)
        {
            pencilSelected = false;
            eraserSelected = false;
            dropperSelected = false;
            lineSelected = false;
            paintbrushSelected = false;
            resizeSelected = false;

            pencilButton.IsChecked = false;
            eraserButton.IsChecked = false;
            dropperButton.IsChecked = false;
            lineButton.IsChecked = false;
            paintbrushButton.IsChecked = false;
            resizeButton.IsChecked = false;

            rotateSelected = !rotateSelected;
        }

        private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {

        }

        public static void layerChanged()
        {
            
        }

        private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mainClick = true;
            mainStartPoint = e.GetPosition(selectedLayer != 0 ? imageList[gifFrames.currentFrame][selectedLayer - 1] : imageBox);
        }
    }
}
