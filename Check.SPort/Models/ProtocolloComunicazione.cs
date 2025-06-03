using Custom.CeFCom;
using Custom.CeFCom.Enums;
using Custom.CeFCom.PrinterCodePages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Check.SPort.Models
{
    public class ProtocolloComunicazione : INotifyPropertyChanged
    {
        private string _protocol;
        private bool _isSeriale;
        private bool _isEthernet = true;

        public SerialPortParams SerialPortSettings { get; } = new SerialPortParams();
        public EthernetParams EthernetSettings { get; } = new EthernetParams();
        // Protocollo XonXoff - Ethernet
        public TcpClient TcpClient { get; set; }
        // Protocollo XonXoff - Seriale
        public SerialPort SerialPort { get; set; }
        // Protocollo Custom - Seriale o Ethernet
        public CeFCom CeFCom { get; set; }
        public string Protocollo
        {
            get => _protocol;
            set { _protocol = value; OnNotifyChanged(nameof(Protocollo)); }
        }
        public bool IsSeriale
        {
            get => _isSeriale;
            set { _isSeriale = value; OnNotifyChanged(nameof(IsSeriale)); }
        }
        public bool IsEthernet
        {
            get => _isEthernet;
            set { _isEthernet = value; OnNotifyChanged(nameof(IsEthernet)); }
        }

        public ProtocolloComunicazione()
        {
            // Initialize Serial Port Settings
            SerialPortSettings.RefreshFormValues();
            // Initialize Ethernet Settings
            EthernetSettings.RefreshFormValues();

            // Initialize Protocollo XonXoff - Ethernet
            TcpClient = new TcpClient();

            // Initialize Protocollo XonXoff - Seriale
            SerialPort = new SerialPort
            {
                BaudRate = SerialPortSettings.BaudRate,
                Parity = SerialPortSettings.Parity,
                StopBits = SerialPortSettings.StopBits,
                DataBits = SerialPortSettings.DataBits,
                Handshake = SerialPortSettings.HandshakeProp,
                DtrEnable = SerialPortSettings.Dtr,
                RtsEnable = SerialPortSettings.Rts,
                PortName = "COM3"
            };

            // Initialize Protocollo Custom - Seriale o Ethernet
            CeFCom = new CeFCom
            {
                CurrentProtocol = ProtocolTypeEnum.Custom,
                EncodingType = EncodingTypeEnum.Standard,
                LogLevel = LogLevelEnum.None
            };

            Protocollo = "Custom";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnNotifyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
