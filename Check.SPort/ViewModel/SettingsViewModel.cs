using Check.SPort.Models;
using Check.SPort.Utilities;
using Custom.CeFCom.Enums;
using Custom.CeFCom.PrinterCodePages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
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
        private string selectedProtocol;
        private string selectedLogLevel;
        private string selectedEncoding;
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
            SettingReg.SerialPort.PortName = SerialPorts[0];

            RefreshSerialSettingsCommand = new RelayCommand(RefreshSettingsCassa);
            SaveCommand = new RelayCommand(SaveSetting);
        }

        #region Command
        public ICommand SaveCommand { get; }
        public ICommand RefreshSerialSettingsCommand { get; }
        #endregion Command

        #region Metodi
        private void SaveSetting(object sender)
        {
            if (SettingReg.IsSeriale)
            {

            }
            else
            {
                
            }
        }

        private void RefreshSettingsCassa(object sender)
        {
            SettingReg.SerialPortSettings.RefreshFormValues();
            SettingReg.EthernetSettings.RefreshFormValues();
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
        public bool IsSerial { get; set; } = true;
        public bool IsEthernet { get; set; }
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
        #endregion Binding
    }
}
