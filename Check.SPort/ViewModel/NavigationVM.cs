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
        private IPageViewModel? _currentView;
        #endregion Property

        public NavigationVM()
        {
            CurrentView = new MainViewModel();
        }

        #region Command
        public ICommand HomeCommand => new RelayCommand(_ => CurrentView = new MainViewModel());
        public ICommand ProtocolCustomCommand => new RelayCommand(_ => CurrentView = new ProtocolCustomVM());
        public ICommand ProtocolXonXOffCommand => new RelayCommand(_ => CurrentView = new ProtocolXonXoffVM());
        #endregion Command

        #region Binding
        public IPageViewModel? CurrentView
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
