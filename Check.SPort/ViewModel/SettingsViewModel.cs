using Check.SPort.Models;
using Check.SPort.Utilities;
using Custom.CeFCom.Enums;
using Custom.CeFCom.PrinterCodePages;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Check.SPort.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Property
        private ProtocolloComunicazione _settingReg;
        private string _selectedProtocolConnection;
        private string selectedProtocol;
        private string selectedLogLevel;
        private string selectedEncoding;
        private bool _isSeriale;
        private bool _isEthernet;
        #endregion Property

        public SettingsViewModel()
        {
            SettingReg = App.SettingsProtocol;

            Protocols = new ObservableCollection<string>(Enum.GetNames(typeof(ProtocolTypeEnum)));
            LogLevels = new ObservableCollection<string>(Enum.GetNames(typeof(LogLevelEnum)));
            Encodings = new ObservableCollection<string>(Enum.GetNames(typeof(EncodingTypeEnum)));
            SerialPorts = new ObservableCollection<string>(SerialPort.GetPortNames());

            SelectedProtocol = Protocols[0];
            SelectedLogLevel = LogLevels[0];
            SelectedEncoding = Encodings[0];

            IsEthernet = SettingReg.IsEthernet;
            IsSerial = SettingReg.IsSeriale;
            SelectedProtocolConnection = SettingReg.Protocollo;

            SearchPortCOMCommand = new RelayCommand(SearchPortCOM);
            SaveCommand = new RelayCommand(SaveSetting);
            ResetSettingsCommand = new RelayCommand(RefreshSettingsCassa);

            ProtocolConnections = new() { "XonXoff", "Custom" };
        }

        #region Command
        public ICommand SaveCommand { get; }
        public ICommand ResetSettingsCommand { get; }
        public ICommand SearchPortCOMCommand { get; }
        #endregion Command

        #region Metodi
        private void SearchPortCOM(object sender)
        {
            var porteAttuali = SerialPorts.ToList();
            SerialPorts = new ObservableCollection<string>(SerialPort.GetPortNames());
            SnackbarService.ShowMessage("Ricerca porte COM completata!", "Annulla", () => SerialPorts = new ObservableCollection<string>(porteAttuali));
        }
        private void SaveSetting(object sender)
        {
            SettingReg.IsSeriale = IsSerial;
            SettingReg.IsEthernet = IsEthernet;
            SettingReg.Protocollo = SelectedProtocolConnection;
            SnackbarService.ShowMessage("Salvataggio impostazioni effettuato!");
        }
        private void RefreshSettingsCassa(object sender)
        {
            SettingReg.SerialPortSettings.RefreshFormValues();
            SettingReg.EthernetSettings.RefreshFormValues();

            SnackbarService.ShowMessage("Impostazioni ripristinate!");
        }
        #endregion Metodi

        #region Binding
        public ProtocolloComunicazione SettingReg
        {
            get => _settingReg;
            set { _settingReg = value; OnPropertyChanged(nameof(SettingReg)); }
        }
        public ObservableCollection<string> Protocols { get; set; }
        public ObservableCollection<string> LogLevels { get; set; }
        public ObservableCollection<string> Encodings { get; set; }
        public ObservableCollection<string> SerialPorts { get; set; }
        public ObservableCollection<string> ProtocolConnections { get; }
        public bool IsSerial
        {
            get => _isSeriale;
            set { _isSeriale = value; OnPropertyChanged(nameof(IsSerial)); }
        }
        public bool IsEthernet
        {
            get => _isEthernet;
            set { _isEthernet = value; OnPropertyChanged(nameof(IsEthernet)); }
        }
        public string SelectedProtocol
        {
            get => selectedProtocol;
            set { selectedProtocol = value; OnPropertyChanged(nameof(SelectedProtocol)); }
        }

        public string SelectedLogLevel
        {
            get => selectedLogLevel;
            set { selectedLogLevel = value; OnPropertyChanged(nameof(SelectedLogLevel)); }
        }

        public string SelectedEncoding
        {
            get => selectedEncoding;
            set { selectedEncoding = value; OnPropertyChanged(nameof(SelectedEncoding)); }
        }
        public string SelectedProtocolConnection
        {
            get => _selectedProtocolConnection;
            set { _selectedProtocolConnection = value; OnPropertyChanged(nameof(SelectedProtocolConnection)); }
        }
        #endregion Binding
    }
}
