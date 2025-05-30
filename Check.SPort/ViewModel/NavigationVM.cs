using Check.SPort.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Check.SPort.ViewModel
{
    class NavigationVM : BaseViewModel
    {
        #region Property
        private object? _currentView;
        #endregion Property

        public NavigationVM()
        {
            CurrentView = new MainViewModel();
        }

        #region Command
        public ICommand HomeCommand => new RelayCommand(v => CurrentView = new MainViewModel());
        public ICommand ProtocolCustomCommand => new RelayCommand(v => CurrentView = new ProtocolCustomVM());
        public ICommand ProtocolXonXOffCommand => new RelayCommand(v => CurrentView = new ProtocolXonXoffVM());
        #endregion Command

        #region Metodi
        #endregion Metodi

        #region Binding
        public object? CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged(nameof(CurrentView));
                }
            }
        }
        #endregion Binding
    }
}
