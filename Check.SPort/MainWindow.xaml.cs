using Check.SPort.View;
using Microsoft.Win32;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WF_Control = System.Windows.Forms.Control;
using WF_Screen = System.Windows.Forms.Screen;

namespace Check.SPort
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize; // Nasconde il grip
            // Avvia animazione all'apertura
            Loaded += (s, e) =>
            {
                var sb = (Storyboard)this.Resources["WindowEnterStoryboard"];
                sb.Begin(this);
            };
        }

        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            // Set tooltip visibility
            if (Tg_Btn.IsChecked == true)
            {
                tt_home.Visibility = Visibility.Collapsed;
                tt_protocol.Visibility = Visibility.Collapsed;
                tt_settings.Visibility = Visibility.Collapsed;
            }
            else
            {
                tt_home.Visibility = Visibility.Visible;
                tt_protocol.Visibility = Visibility.Visible;
                tt_settings.Visibility = Visibility.Visible;
            }
        }

        private void BorderMove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ResponsiveWindow.WindowState = ResponsiveWindow.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Ottieni posizione assoluta del cursore
                var mousePosition = WF_Control.MousePosition;

                if (ResponsiveWindow.WindowState == WindowState.Maximized)
                {
                    // Rileva lo schermo dove si trova il cursore
                    var currentScreen = WF_Screen.FromPoint(mousePosition);

                    // Calcola offset del mouse rispetto allo schermo corrente
                    double percentX = (double)(mousePosition.X - currentScreen.Bounds.X) / currentScreen.WorkingArea.Width;

                    // Ripristina finestra
                    ResponsiveWindow.WindowState = WindowState.Normal;

                    // Calcola nuova posizione
                    double targetLeft = mousePosition.X - (ResponsiveWindow.ActualWidth * percentX);
                    double targetTop = mousePosition.Y - 10;

                    // Applica nuova posizione
                    ResponsiveWindow.Left = targetLeft;
                    ResponsiveWindow.Top = targetTop;
                }
                ResponsiveWindow.DragMove();
            }
        }

        private void Storyboard_Completed(object sender, System.EventArgs e)
        {
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
            this.MinWidth = 900;
        }
    }
}