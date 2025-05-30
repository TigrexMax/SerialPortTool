using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Check.SPort.Models
{
    public class SerialPortParams : INotifyPropertyChanged
    {
        private int _baudRate = 19200;
        private Parity _parity = Parity.Odd;
        private StopBits _stopBits = StopBits.One;
        private int _dataBits = 7;
        private string _portName = string.Empty;
        private Handshake _handshakeProp = Handshake.None;
        private bool _dtr = false;
        private bool _rts = true;

        public SerialPortParams()
        {
            BaudRateList = new ObservableCollection<int>(ComboBoxBaudRate());
            Parities = new ObservableCollection<Parity>(ComboBoxParity());
            StopBitsList = new ObservableCollection<StopBits>(ComboBoxStopBits());
            DataBitsList = new ObservableCollection<int>(ComboBoxDataBits());
            Handshakes = new ObservableCollection<Handshake>(ComboBoxHandshake());
            RefreshFormValues();
        }

        #region Public Property
        /// <summary>
        /// Property to hold the BaudRate
        /// of our manager class
        /// </summary>
        public int BaudRate
        {
            get => _baudRate;
            set { _baudRate = value; OnNotifyChanged(nameof(BaudRate)); }
        }

        /// <summary>
        /// property to hold the Parity
        /// of our manager class
        /// </summary>
        public Parity Parity
        {
            get => _parity;
            set { _parity = value; OnNotifyChanged(nameof(Parity)); }
        }

        /// <summary>
        /// property to hold the StopBits
        /// of our manager class
        /// </summary>
        public StopBits StopBits
        {
            get => _stopBits;
            set { _stopBits = value; OnNotifyChanged(nameof(StopBits)); }
        }

        /// <summary>
        /// property to hold the DataBits
        /// of our manager class
        /// </summary>
        public int DataBits
        {
            get => _dataBits;
            set { _dataBits = value; OnNotifyChanged(nameof(DataBits)); }
        }

        /// <summary>
        /// property to hold the PortName
        /// of our manager class
        /// </summary>
        public string PortName
        {
            get => _portName;
            set { _portName = value; OnNotifyChanged(nameof(PortName)); }
        }

        /// <summary>
        /// property to hold the Handshake
        /// of our manager class
        /// </summary>
        public Handshake HandshakeProp
        {
            get => _handshakeProp;
            set { _handshakeProp = value; OnNotifyChanged(nameof(HandshakeProp)); }
        }

        /// <summary>
        /// property to hold the DtrEnabled
        /// of our manager class
        /// </summary>
        public bool Dtr
        {
            get => _dtr;
            set { _dtr = value; OnNotifyChanged(nameof(Dtr)); }
        }

        /// <summary>
        /// property to hold the RtsEnabled
        /// of our manager class
        /// </summary>
        public bool Rts
        {
            get => _rts;
            set { _rts = value; OnNotifyChanged(nameof(Rts)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion Public Property

        #region Liste_Dati
        public ObservableCollection<int> BaudRateList { get; }
        public ObservableCollection<Parity> Parities { get; }
        public ObservableCollection<StopBits> StopBitsList { get; }
        public ObservableCollection<int> DataBitsList { get; }
        public ObservableCollection<Handshake> Handshakes { get; }
        #endregion Liste_Dati

        #region Metodi
        private static List<int> ComboBoxBaudRate() => [ 9600, 19200, 38400, 59600];
        private static List<Parity> ComboBoxParity() => [.. Enum.GetValues(typeof(Parity)).Cast<Parity>()];
        private static List<StopBits> ComboBoxStopBits() => [.. Enum.GetValues(typeof(StopBits)).Cast<StopBits>()];
        private static List<int> ComboBoxDataBits() => [7, 8, 9];
        private static List<Handshake> ComboBoxHandshake() => [.. Enum.GetValues(typeof(Handshake)).Cast<Handshake>()];
        public void RefreshFormValues()
        {
            if (this != null)
            {
                BaudRate = 19200;
                Parity = Parity.Odd;
                StopBits = StopBits.One;
                DataBits = 7;
                PortName = string.Empty;
                HandshakeProp = Handshake.None;
                Dtr = false;
                Rts = true;
            }
        }
        protected void OnNotifyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion Metodi
    }
}
