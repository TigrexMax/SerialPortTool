using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Check.SPort.View
{
    /// <summary>
    /// Logica di interazione per ProtocolXonXoffView.xaml
    /// </summary>
    public partial class ProtocolXonXoffView : UserControl
    {
        private readonly SerialPort _serialPort = new();
        private TcpClient? _tcpClient = null;
        private const string NAK_CODE = "14";
        private const string XON_CODE = "11";

        public ProtocolXonXoffView()
        {
            InitializeComponent();
            SetComboBox();
            InitializeSerialPort();
            this.KeyDown += MainWindow_KeyDown;
            Application.Current.Exit += OnAppExit;
        }

        private void InitializeSerialPort()
        {
            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 5000;
            _serialPort.NewLine = Environment.NewLine;
            _serialPort.RtsEnable = true;
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.ErrorReceived += SerialPort_ErrorReceived;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Controlla se il tasto premuto è Enter
            if (e.Key == Key.Enter)
            {
                // Chiama il metodo che vuoi eseguire
                _ = BtnSendAsync();
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if (sender is SerialPort sp && sp.IsOpen)
            {
                Dispatcher.Invoke(() => ScriviResponseBox($"Errore: {Enum.GetName(e.EventType)}, codice: {(int)e.EventType}"));
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sender is SerialPort sp && sp.IsOpen)
            {
                string indata = sp.ReadExisting();
                Dispatcher.Invoke(() => ScriviResponseBox($"Response: {indata}"));
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
                if (rbtnETH.IsChecked == true)
                    OpenCloseETH();
                else
                    OpenCloseSerialPort();
            }
            catch (Exception ex)
            {
                ScriviResponseBox($"{ex.Message}");
            }
        }

        private void OpenCloseSerialPort()
        {
            if (!_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.PortName = cbxPortName.SelectedItem?.ToString() ?? "COM3";
                    _serialPort.BaudRate = int.TryParse(cbxBaudRate.SelectedItem?.ToString(), out int baud) ? baud : 9600;
                    _serialPort.Parity = (Parity)cbxParity.SelectedItem;
                    _serialPort.StopBits = Enum.Parse<StopBits>(cbxStopBits.SelectedValue?.ToString() ?? "One");
                    _serialPort.DataBits = int.TryParse(cbxDatabits.SelectedItem?.ToString(), out int dataBits) ? dataBits : 8;
                    _serialPort.Handshake = Enum.Parse<Handshake>(cbxFlowControl.SelectedValue?.ToString() ?? "XOnXOff");

                    _serialPort.Open();
                    btnOpenClose.Content = "CLOSE";
                    txtResponse.Clear();
                }
                catch (Exception ex)
                {
                    ScriviResponseBox($"Impossibile aprire la porta seriale: {ex.Message}");
                }
            }
            else
            {
                _serialPort.WriteLine("Y");
                _serialPort.Close();
                btnOpenClose.Content = "OPEN";
            }
        }

        private void OpenCloseETH()
        {
            if (_tcpClient == null)
            {
                if (!IPAddress.TryParse(txtIPAddress.Text, out IPAddress? ip) || !int.TryParse(txtPortETH.Text, out int port))
                {
                    MessageBox.Show("IP o porta non validi.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _tcpClient = new TcpClient();
                _tcpClient.Connect(ip, port);
                btnOpenClose.Content = "CLOSE";
                txtResponse.Clear();
                btnFileCmd.IsEnabled = true;
            }
            else
            {
                _tcpClient.Close();
                _tcpClient = null;
                btnOpenClose.Content = "OPEN";
                btnFileCmd.IsEnabled = false;
            }
        }

        private async Task BtnSendAsync()
        {
            if (!btnSend.IsEnabled) return;
            if (rbtnSerial.IsChecked == true)
            {
                if (_serialPort.IsOpen)
                {
                    try
                    {
                        _serialPort.WriteLine(txtCMD.Text);
                        txtCMD.Clear();
                    }
                    catch (Exception ex)
                    {
                        ScriviResponseBox($"ERROR: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("La connessione con la porta risulta chiusa.", "Connessione Chiusa", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    if (_tcpClient?.Connected == true)
                    {
                        byte[] buffer = Encoding.ASCII.GetBytes(txtCMD.Text);
                        var stream = _tcpClient.GetStream();

                        await SendCommandAsync(buffer, stream);
                        var responseBuffer = await ReceiveResponseAsync(stream);

                        if (responseBuffer.Length > 1)
                        {
                            string controlCode = Encoding.ASCII.GetString(responseBuffer[^3..^1]);

                            if (controlCode == NAK_CODE)
                            {
                                ScriviResponseBox("Errore ricevuto (NAK), in attesa di riprovare...\n");
                                await Task.Delay(1000);
                                await SendCommandAsync(buffer, stream);
                            }
                            else if (controlCode == XON_CODE)
                            {
                                ScriviResponseBox("Via libera ricevuta (Xon), posso continuare.\n");
                            }

                            ScriviResponseBox(Encoding.ASCII.GetString(responseBuffer));
                        }
                        txtCMD.Clear();
                    }
                }
                catch (Exception ex)
                {
                    ScriviResponseBox($"ERROR: {ex.Message}");
                }
            }
        }

        private static async Task SendCommandAsync(byte[] buffer, NetworkStream stream)
        {
            await stream.WriteAsync(buffer);
            await stream.FlushAsync();
        }

        private static async Task<byte[]> ReceiveResponseAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[256];
            int bytesRead = await stream.ReadAsync(buffer);
            return [.. buffer.Take(bytesRead)];
        }

        private void ScriviResponseBox(string riga)
        {
            if (txtResponse.Text.Length > 5000) txtResponse.Clear();
            txtResponse.AppendText($"{riga}{Environment.NewLine}");
            txtResponse.ScrollToEnd();
        }

        private async void BtnFileCmd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (ofd.ShowDialog() == true)
            {
                txtResponse.Clear();
                using var reader = new StreamReader(ofd.FileName);
                while (reader.Peek() > 0)
                {
                    txtCMD.Text = reader.ReadLine();
                    await BtnSendAsync();
                }
                txtCMD.Clear();
            }
        }

        private void ControlloChangeRB_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                if (rb.Name == "rbtnSerial" && _tcpClient?.Connected == true)
                {
                    OpenCloseETH();
                }
                else if (rb.Name == "rbtnETH" && _serialPort.IsOpen)
                {
                    OpenCloseSerialPort();
                }
            }
        }

        private void OnAppExit(object? sender, ExitEventArgs e)
        {
            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Dispose();

            _tcpClient?.Close();
            _tcpClient?.Dispose();
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            await BtnSendAsync();
        }
    }
}
