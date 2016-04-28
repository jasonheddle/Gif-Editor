using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GIF_Editor
{
    /// <summary>
    /// Interaction logic for LayerWindow.xaml
    /// </summary>
    public partial class LayerWindow : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);



        const uint MF_BYCOMMAND = 0x00000000;
        const uint MF_GRAYED = 0x00000001;
        const uint MF_ENABLED = 0x00000000;

        const uint SC_CLOSE = 0xF060;

        const int WM_SHOWWINDOW = 0x00000018;
        const int WM_CLOSE = 0x10;

        public LayerWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(this.hwndSourceHook));
            }
        }

        private void layerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.layerChanged();
            MainWindow.selectedLayer = Math.Abs(layerList.SelectedIndex - layerList.Items.Count + 1);
            MainWindow.setMainB();
        }

        public void SetSelectedLayer(int layerIndex)
        {
            layerList.SelectedIndex = layerIndex;
        }

        public void AddItem(object obj)
        {
            layerList.Items.Insert(0, obj);
        }

        public void RemoveAll()
        {
            layerList.Items.Clear();
        }

        IntPtr hwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SHOWWINDOW)
            {
                IntPtr hMenu = GetSystemMenu(hwnd, false);
                if (hMenu != IntPtr.Zero)
                {
                    EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
                }
            }
            else if (msg == WM_CLOSE)
            {
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.removeLayer();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AddLayer(sender, e);
        }
    }
}
