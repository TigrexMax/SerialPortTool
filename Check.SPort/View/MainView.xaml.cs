using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Check.SPort.View
{
    /// <summary>
    /// Logica di interazione per MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private readonly SerialPort _serialPort = new();
        private TcpClient? _tcpClient = null;

        public MainView()
        {
            InitializeComponent();
            SetComboBox();

            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 5000;
            _serialPort.NewLine = Environment.NewLine;
            _serialPort.RtsEnable = true;
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
                Dispatcher.Invoke(() => ScriviResponseBox(string.Format("Errore: {0}, codice: {1}{2}", Enum.GetName(e.EventType), (int)e.EventType, Environment.NewLine)));
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sender is SerialPort sp && sp.IsOpen)
            {
                string indata = sp.ReadExisting();
                Dispatcher.Invoke(() => ScriviResponseBox(string.Format("Response: {0}{1}", indata, Environment.NewLine)));
            }
        }

        private void SetComboBox()
        {
            cbxPortName.ItemsSource = SerialPort.GetPortNames();
            cbxParity.ItemsSource = Enum.GetValues(typeof(Parity));

            cbxPortName.SelectedIndex = 0;
            cbxParity.SelectedIndex = 0;
        }

        private void BtnOpenClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rbtnETH.IsChecked ?? false)
                {
                    OpenCloseETH();
                }
                else
                {
                    OpenCloseSerialPort();
                }
            }
            catch (Exception ex)
            {
                ScriviResponseBox(string.Format("{0}{1}", ex.Message, Environment.NewLine));
            }
        }

        private void OpenCloseSerialPort()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.PortName = cbxPortName.SelectedItem?.ToString() ?? "COM3";
                _serialPort.BaudRate = int.TryParse(cbxBaudRate.SelectedItem.ToString(), out int velocitaPorta) ? velocitaPorta : 9600;
                _serialPort.Parity = (Parity)cbxParity.SelectedItem;
                _serialPort.StopBits = Enum.Parse<StopBits>(cbxStopBits.SelectedValue?.ToString() ?? "One");
                _serialPort.DataBits = int.TryParse(cbxDatabits.SelectedItem.ToString(), out int dataBit) ? dataBit : 8;
                _serialPort.Handshake = Enum.Parse<Handshake>(cbxFlowControl.SelectedValue?.ToString() ?? "XOnXOff");

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

        private void OpenCloseETH()
        {
            if (_tcpClient == null)
            {
                _tcpClient = new();
                _tcpClient.Connect(IPAddress.Parse(txtIPAddress.Text), int.Parse(txtPortETH.Text));
                btnOpenClose.Content = "CLOSE";
            }
            else
            {
                _tcpClient.Close();
                _tcpClient = null;
                btnOpenClose.Content = "OPEN";
                txtResponse.Text = string.Empty;
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (btnSend.IsEnabled)
            {
                if (rbtnSerial.IsChecked ?? false)
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
                            ScriviResponseBox(string.Format("TIMEOUT: {0}{1}", te.Message, Environment.NewLine));
                        }
                        catch (Exception ex)
                        {
                            ScriviResponseBox(string.Format("ERROR: {0}{1}", ex.Message, Environment.NewLine));
                        }
                    }
                    else
                    {
                        MessageBox.Show("La connessione con la porta risulta chiusa.", "Connesione Chiusa", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    try
                    {
                        if (_tcpClient?.Connected == true)
                        {
                            byte[] buffer = Encoding.ASCII.GetBytes(txtCMD.Text);
                            NetworkStream stream = _tcpClient.GetStream();
                            stream.Write(buffer, 0, buffer.Length);
                            txtCMD.Text = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        ScriviResponseBox(string.Format("ERROR: {0}{1}", ex.Message, Environment.NewLine));
                    }
                }
            }
        }

        private void ScriviResponseBox(string riga)
        {
            if (txtResponse.Text.Length > 5000) txtResponse.Text = string.Empty;
            txtResponse.Text += riga;
            txtResponse.SelectionStart = txtResponse.Text.Length;
            txtResponse.ScrollToEnd();
        }

        private void BtnFileCmd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (ofd.ShowDialog() == true)
            {
                FileInfo fi = new(ofd.FileName);
                StreamReader sr = new(fi.FullName);
                while (sr.Peek() > 0)
                {
                    txtCMD.Text = sr.ReadLine();
                    BtnSend_Click(sender, e);
                }
            }
        }

        private void ControlloChangeRB_Click(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;
            if (rb is RadioButton rbSerial && rbSerial.Name == "rbtnSerial" && _tcpClient?.Connected == true)
            {
                OpenCloseETH();
            }

            if (rb is RadioButton rbETH && rbETH.Name == "rbtnETH" && _serialPort.IsOpen)
            {
                OpenCloseSerialPort();
            }
        }
    }
}
