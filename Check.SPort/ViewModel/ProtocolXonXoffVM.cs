using Check.SPort.Models;
using Check.SPort.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Check.SPort.ViewModel
{
    class ProtocolXonXoffVM : BaseViewModel
    {
        #region Property
        private SerialPortParams _settingCassa;
        #endregion Property

        public ProtocolXonXoffVM()
        {
            SettingCassa = new();
        }

        #region Command
        #endregion Command

        #region Metodi
        #endregion Metodi

        #region Binding
        public SerialPortParams SettingCassa
        {
            get => _settingCassa;
            set { _settingCassa = value; OnPropertyChanged(nameof(SettingCassa)); }
        }
        #endregion Binding
    }
}
