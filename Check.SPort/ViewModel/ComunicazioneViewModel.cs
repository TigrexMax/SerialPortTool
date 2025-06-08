using Check.SPort.Models;
using Check.SPort.Utilities;
using Custom.CeFCom;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Check.SPort.ViewModel
{
    class ComunicazioneViewModel : BaseViewModel
    {
        #region Property
        private const string NAK_CODE = "14";
        private const string XON_CODE = "11";
        private string _comandi;
        private string _responseComandi;
        private bool _isConnection;
        private bool _activeFlipper;
        private int _badgeCount;
        #endregion Property

        public ComunicazioneViewModel()
        {
            var settings = App.SettingsProtocol;
            NameProtocol = $"Protocol set {settings.Protocollo}";
            NameMode = $"Mode set {(settings.IsSeriale ? "Seriale" : "Ethernet")} ";

            SerialPortSettings = settings.SerialPortSettings;
            EthernetSettings = settings.EthernetSettings;
        }

        #region Command
        public ICommand OnOffCommand => new RelayCommand(sender =>
        {
            if (IsConnection)
            {
                CloseCommand.Execute(this);
            }
            else
            {
                OpenCommand.Execute(this);
            }
        });
        public ICommand OpenCommand => new RelayCommand(_ => IsConnection = OpenConnection());
        public ICommand CloseCommand => new RelayCommand(_ => CloseConnection());
        public ICommand SendCommand => new RelayCommand(async _ => await SendCommandForProtocolAsync(), _ => !string.IsNullOrEmpty(Comandi));
        public ICommand FileCommand => new RelayCommand(_ => OpenFileForComandi(), _ => IsConnection);
        public ICommand ActiveFlipperCommand => new RelayCommand(_ => ActiveFlipper = !ActiveFlipper);
        #endregion Command

        #region Metodi
        bool OpenConnection()
        {
            var settings = App.SettingsProtocol;
            try
            {
                if (settings.IsSeriale)
                {
                    return settings.Protocollo switch
                    {
                        "XonXoff" => XonXoff_Seriale(),
                        "Custom" => Custom_Seriale(),
                        _ => false
                    };
                }
                else if (settings.IsEthernet)
                {
                    return settings.Protocollo switch
                    {
                        "XonXoff" => XonXoff_Ethernet(),
                        "Custom" => Custom_Ethernet(),
                        _ => false
                    };
                }
                else
                {
                    SnackbarService.ShowMessage("No protocol selected.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                SnackbarService.ShowMessage($"Error opening connection for [{settings.Protocollo}]: {ex.Message}.");
                return false;
            }
        }

        void CloseConnection()
        {
            var settings = App.SettingsProtocol;
            try
            {
                if (settings.Protocollo == "Custom")
                {
                    int resultCommand = 0;
                    if (Connection_Custom.EthernetPortIsOpened || Connection_Custom.SerialPortIsOpened)
                    {
                        resultCommand = Connection_Custom.CEFClose();
                        IsConnection = false;
                    }
                }
                else if (settings.Protocollo == "XonXoff")
                {
                    if (settings.IsEthernet && Connection_XonXoff_Ethernet.Connected)
                    {
                        Connection_XonXoff_Ethernet.Close();
                        IsConnection = Connection_XonXoff_Ethernet.Connected;
                        Connection_XonXoff_Ethernet = null;
                    }
                    else if(settings.IsSeriale && Connection_XonXoff_Seriale.IsOpen)
                    {
                        Connection_XonXoff_Seriale.Write("Y");
                        Connection_XonXoff_Seriale.Close();
                        IsConnection = Connection_XonXoff_Seriale.IsOpen;
                        Connection_XonXoff_Seriale = null;
                    }
                }
            }
            catch (Exception ex)
            {
                SnackbarService.ShowMessage($"Error closing connection for [{settings.Protocollo}]: {ex.Message}.");
            }
        }

        async Task SendCommandForProtocolAsync()
        {
            var settings = App.SettingsProtocol;
            ResponseComandi = string.Empty;
            try
            {
                if (settings.IsSeriale)
                {
                    if (settings.Protocollo == "XonXoff")
                    {
                        await XonXoff_Write(true);
                    }
                    else if (settings.Protocollo == "Custom")
                    {
                        Custom_Write();
                    }
                }
                else if (settings.IsEthernet)
                {
                    if (settings.Protocollo == "XonXoff")
                    {
                        await XonXoff_Write(false);
                    }
                    else if (settings.Protocollo == "Custom")
                    {
                        Custom_Write();
                    }
                }
                else
                {
                    SnackbarService.ShowMessage("No protocol selected.");
                }
            }
            catch (Exception ex)
            {
                SnackbarService.ShowMessage($"Error connection for [{settings.Protocollo}]: {ex.Message}.");
            }
            finally
            {
                Comandi = string.Empty;
            }
        }

        void OpenFileForComandi()
        {
            Comandi = string.Empty;
            var settings = App.SettingsProtocol;

            OpenFileDialog ofd = new()
            {
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (settings.Protocollo == "Custom")
            {
                ofd.Filter = "Designer file (*.dsf)|*.dsf|All files (*.*)|*.*";
            }
            else if (settings.Protocollo == "XonXoff")
            {
                ofd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            }
            else
            {
                SnackbarService.ShowMessage("Miss protocol settings");
                return;
            }

            if (ofd.ShowDialog() == true)
            {
                using var reader = new StreamReader(ofd.FileName);
                Comandi = reader.ReadToEnd();
                SnackbarService.ShowMessage("Eseguo questi comandi?", "Si", async () =>
                {
                    await SendCommandForProtocolAsync();
                    Comandi = string.Empty;
                });
            }
        }

        private void ScriviResponseBox(string riga)
        {
            StringBuilder sb = new(ResponseComandi);
            sb.AppendLine(riga);
            ResponseComandi = sb.ToString();
        }
        #endregion Metodi

        #region XonXoff
        bool XonXoff_Seriale()
        {
            var settings = App.SettingsProtocol;
            Connection_XonXoff_Seriale = new SerialPort(settings.SerialPort.PortName, settings.SerialPort.BaudRate, settings.SerialPort.Parity, settings.SerialPort.DataBits, settings.SerialPort.StopBits)
            {
                ReadTimeout = 5000,
                WriteTimeout = 5000,
                NewLine = Environment.NewLine,
                Handshake = settings.SerialPort.Handshake
            };
            Connection_XonXoff_Seriale.DataReceived += SerialPort_DataReceived;
            Connection_XonXoff_Seriale.ErrorReceived += SerialPort_ErrorReceived;

            try
            {
                Connection_XonXoff_Seriale.Open();
            }
            catch (Exception ex)
            {
                SnackbarService.ShowMessage(ex.Message, "OK", () => { });
            }
            return Connection_XonXoff_Seriale.IsOpen;
        }
        bool XonXoff_Ethernet()
        {
            var settings = App.SettingsProtocol;
            Connection_XonXoff_Ethernet = new TcpClient();
            try
            {
                Connection_XonXoff_Ethernet.Connect(IPAddress.Parse(EthernetSettings.IpAddress), EthernetSettings.Port);
            }
            catch (Exception ex)
            {
                SnackbarService.ShowMessage(ex.Message, "OK", () => { });
            }
            return Connection_XonXoff_Ethernet.Connected;
        }
        private async Task XonXoff_Write(bool isSerial)
        {
            foreach (var cmd in Comandi.Split(Environment.NewLine))
            {
                if (isSerial)
                {
                    Connection_XonXoff_Seriale.WriteLine(cmd);
                }
                else
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(cmd);
                    var stream = Connection_XonXoff_Ethernet.GetStream();

                    await SendCommandAsync(buffer, stream);
                    var responseBuffer = await ReceiveResponseAsync(stream);

                    if (responseBuffer.Length > 3)
                    {
                        string controlCode = Encoding.ASCII.GetString(responseBuffer[^3..^1]);

                        if (controlCode == NAK_CODE)
                        {
                            SnackbarService.ShowMessage("Errore ricevuto (NAK), in attesa di riprovare...");
                            await Task.Delay(1000);
                            await SendCommandAsync(buffer, stream);
                        }
                        else if (controlCode == XON_CODE)
                        {
                            SnackbarService.ShowMessage("Via libera ricevuta (Xon), posso continuare.\n");
                        }
                    }
                    ScriviResponseBox(Encoding.ASCII.GetString(responseBuffer));
                }
            }

            async Task SendCommandAsync(byte[] buffer, NetworkStream stream)
            {
                await stream.WriteAsync(buffer);
                await stream.FlushAsync();
            }

            async Task<byte[]> ReceiveResponseAsync(NetworkStream stream)
            {
                byte[] buffer = new byte[256];
                int bytesRead = await stream.ReadAsync(buffer);
                return [.. buffer.Take(bytesRead)];
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if (sender is SerialPort sp && sp.IsOpen)
            {
                SnackbarService.ShowMessage($"Errore: {Enum.GetName(e.EventType)}, codice: {(int)e.EventType}");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sender is SerialPort sp && sp.IsOpen)
            {
                string indata = sp.ReadExisting();
                SnackbarService.ShowMessage($"Response: {indata}");
            }
        }
        #endregion XonXoff

        #region Custom
        bool Custom_Seriale()
        {
            int retunCommand = 0;
            var settings = App.SettingsProtocol;
            Connection_Custom = settings.CeFCom;
            try
            {
                if (Connection_Custom.SerialPortIsOpened)
                {
                    Connection_Custom.CEFClose();
                }
                retunCommand = Connection_Custom.CEFOpen(
                    SerialPortSettings.BaudRate,
                    SerialPortSettings.DataBits,
                    SerialPortSettings.StopBits,
                    SerialPortSettings.Parity,
                    SerialPortSettings.PortName,
                    SerialPortSettings.HandshakeProp,
                    SerialPortSettings.Dtr,
                    SerialPortSettings.Rts);

                if (retunCommand != 0) throw new InvalidOperationException("Error opening serial port.");
            }
            catch (Exception ex)
            {
                SnackbarService.ShowMessage(ex.Message, "OK", () => { });
            }
            return retunCommand == 0;
        }

        bool Custom_Ethernet()
        {
            int retunCommand = 0;
            var settings = App.SettingsProtocol;
            Connection_Custom = settings.CeFCom;
            try
            {
                if (Connection_Custom.EthernetPortIsOpened)
                {
                    Connection_Custom.CEFClose();
                }
                retunCommand = Connection_Custom.CEFOpenEth(EthernetSettings.IpAddress, EthernetSettings.Port);

                if (retunCommand != 0) throw new InvalidOperationException("Error opening connection lan");
            }
            catch (Exception ex)
            {
                SnackbarService.ShowMessage(ex.Message, "OK", () => { });
            }
            return retunCommand == 0;
        }

        private void Custom_Write()
        {
            foreach (var cmd in Comandi.Split(Environment.NewLine))
            {
                EseguiComando(Connection_Custom, cmd, out string cmdResponse);
                ScriviResponseBox(cmdResponse);
            }

            static int EseguiComando(CeFCom ceFCom, string command, out string cmdResponse)
            {
                int response = 0;
                cmdResponse = string.Empty;

                response = ceFCom.CEFWriteRead(command, out cmdResponse);

                if (cmdResponse.Contains("ERR99"))
                {
                    response = ceFCom.CEFWriteRead("1015", out cmdResponse);
                }
                return response;
            }
        }
        #endregion Custom

        #region Binding
        public string Comandi
        {
            get => _comandi;
            set
            {
                _comandi = value;
                OnPropertyChanged(nameof(Comandi));
            }
        }
        public string ResponseComandi
        {
            get => _responseComandi;
            set
            {
                _responseComandi = value;
                OnPropertyChanged(nameof(ResponseComandi));
                BadgeCount = _responseComandi.Length;
            }
        }
        public bool IsConnection
        {
            get => _isConnection;
            set
            {
                _isConnection = value;
                OnPropertyChanged(nameof(IsConnection));
                if (!IsConnection) Comandi = string.Empty;
            }
        }

        public bool ActiveFlipper
        {
            get => _activeFlipper;
            set
            {
                _activeFlipper = value;
                OnPropertyChanged(nameof(ActiveFlipper));
            }
        }
        public int BadgeCount
        {
            get => _badgeCount;
            set { _badgeCount = value; OnPropertyChanged(nameof(BadgeCount)); }
        }
        public string NameProtocol { get; }
        public string NameMode { get; }
        public CeFCom Connection_Custom { get; set; }
        public SerialPort Connection_XonXoff_Seriale { get; set; }
        public TcpClient Connection_XonXoff_Ethernet { get; set; }
        public SerialPortParams SerialPortSettings { get; }
        public EthernetParams EthernetSettings { get; }
        #endregion Binding
    }
}
