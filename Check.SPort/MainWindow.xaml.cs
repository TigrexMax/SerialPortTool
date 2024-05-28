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
            SetComboBox();

            _serialPort.Handshake = Handshake.XOnXOff;
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
            _serialPort.NewLine = "\r\n";
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.ErrorReceived += SerialPort_ErrorReceived;

            this.KeyDown += MainWindow_KeyDown;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Controlla se il tasto premuto è Enter
            if (e.Key == Key.Enter)
            {
                // Chiama il metodo che vuoi eseguire
                BtnSend_Click(this, new RoutedEventArgs());
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if (sender is SerialPort sp && sp.IsOpen)
            {
                Dispatcher.Invoke(() => ScriviResponse(string.Format("Errore: {0}, codice: {1}{2}", Enum.GetName(e.EventType), (int)e.EventType, Environment.NewLine)));
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sender is SerialPort sp && sp.IsOpen)
            {
                string indata = sp.ReadExisting();
                Dispatcher.Invoke(() => ScriviResponse(string.Format("Response: {0}{1}", indata, Environment.NewLine)));
            }
        }

        public void SetComboBox()
        {
            cbxPortName.ItemsSource = SerialPort.GetPortNames();
            cbxParity.ItemsSource = Enum.GetValues(typeof(Parity));
            cbxStopBits.ItemsSource = Enum.GetValues(typeof(StopBits));

            cbxParity.SelectedIndex = 0;
            cbxStopBits.SelectedIndex = 0;
        }

        private void BtnOpenClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.PortName = cbxPortName.SelectedItem?.ToString() ?? "COM3";
                    _serialPort.BaudRate = int.TryParse(cbxBaudRate.SelectedItem.ToString(), out int velocitaPorta) ? velocitaPorta : 9600;
                    _serialPort.Parity = (Parity)cbxParity.SelectedItem;
                    _serialPort.StopBits = (StopBits)cbxStopBits.SelectedItem;

                    _serialPort.Open();
                    btnOpenClose.Content = "CLOSE";
                }
                else
                {
                    _serialPort.WriteLine("Y");
                    _serialPort.Close();
                    btnOpenClose.Content = "OPEN";
                    txtResponse.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ScriviResponse(string.Format("{0}{1}", ex.Message, Environment.NewLine));
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (btnSend.IsEnabled)
            {
                if (_serialPort.IsOpen)
                {
                    try
                    {
                        _serialPort.WriteLine(txtCMD.Text);
                        txtCMD.Text = string.Empty;
                    }
                    catch (TimeoutException te)
                    {
                        ScriviResponse(string.Format("TIMEOUT: {0}{1}", te.Message, Environment.NewLine));
                    }
                    catch (Exception ex)
                    {
                        ScriviResponse(string.Format("ERROR: {0}{1}", ex.Message, Environment.NewLine));
                    }
                }
                else
                {
                    MessageBox.Show(this, "La connessione con la porta risulta chiusa.", "Connesione Chiusa", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ScriviResponse(string riga)
        {
            if (txtResponse.Text.Length > 5000) txtResponse.Text = string.Empty;
            txtResponse.Text += riga;
            txtResponse.SelectionStart = txtResponse.Text.Length;
            txtResponse.ScrollToEnd();
        }
    }
}