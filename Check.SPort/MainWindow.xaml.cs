using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;

namespace Check.SPort
{
    public partial class MainWindow : Window
    {
        private readonly SerialPort _serialPort = new();

        public MainWindow()
        {
            InitializeComponent();
        }

    }
}