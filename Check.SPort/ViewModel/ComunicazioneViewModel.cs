using Check.SPort.Models;
using Check.SPort.Utilities;
using Custom.CeFCom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Check.SPort.ViewModel
{
    class ComunicazioneViewModel : BaseViewModel
    {
        #region Property
        private const string NAK_CODE = "14";
        private const string XON_CODE = "11";
        private string _comandi;
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
        public ICommand OpenCommand => new RelayCommand(_ => OpenConnection());
        public ICommand CloseCommand => new RelayCommand(_ => CloseConnection());
        public ICommand SendCommand => new RelayCommand(x =>
        {
        }, _ => !string.IsNullOrEmpty(Comandi));
        #endregion Command

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
                    }
                }
                else if (settings.Protocollo == "XonXoff")
                {
                    if (settings.IsEthernet && Connection_XonXoff_Ethernet.Connected)
                    {
                        Connection_XonXoff_Ethernet.Close();
                    }
                    else if(settings.IsSeriale && Connection_XonXoff_Seriale.IsOpen)
                    {
                        Connection_XonXoff_Seriale.Write("Y");
                        Connection_XonXoff_Seriale.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                SnackbarService.ShowMessage($"Error closing connection for [{settings.Protocollo}]: {ex.Message}.");
            }
        }

        #region XonXoff
        bool XonXoff_Seriale()
        {
            var settings = App.SettingsProtocol;
            Connection_XonXoff_Seriale = settings.SerialPort;
            Connection_XonXoff_Seriale.ReadTimeout = 5000;
            Connection_XonXoff_Seriale.WriteTimeout = 5000;
            Connection_XonXoff_Seriale.NewLine = Environment.NewLine;

            try
            {
                if (Connection_XonXoff_Seriale.IsOpen)
                {
                    Connection_XonXoff_Seriale.Close();
                }
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
            Connection_XonXoff_Ethernet = settings.TcpClient;
            try
            {
                if (Connection_XonXoff_Ethernet.Connected)
                {
                    Connection_XonXoff_Ethernet.Close();
                }
                Connection_XonXoff_Ethernet.Connect(EthernetSettings.IpAddress, EthernetSettings.Port);
            }
            catch (Exception ex)
            {
                SnackbarService.ShowMessage(ex.Message, "OK", () => { });
            }
            return Connection_XonXoff_Ethernet.Connected;
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
