using Check.SPort.Models;
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
        private object _currentView;
        #endregion Property

        public NavigationVM()
        {
            HomeCommand = new RelayCommand(Home);
            ProtocolCustomCommand = new RelayCommand(Custom);
            ProtocolXonXOffCommand = new RelayCommand(XonXoff);

            // Startup Page
            CurrentViewModel = new MainViewModel();
        }

        #region Command
        public ICommand HomeCommand { get; set; }
        public ICommand ProtocolCustomCommand { get; }
        public ICommand ProtocolXonXOffCommand { get; }
        #endregion Command

        #region Metodi
        private void Home(object obj) => CurrentViewModel = new MainViewModel();
        private void XonXoff(object obj) => CurrentViewModel = new ProtocolXonXoffVM();
        private void Custom(object obj) => CurrentViewModel = new ProtocolCustomVM();
        #endregion Metodi

        #region Binding
        public object CurrentViewModel
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentViewModel)); }
        }
        #endregion Binding
    }
}
