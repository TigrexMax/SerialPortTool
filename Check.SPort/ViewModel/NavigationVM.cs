using Check.SPort.Models;
using Check.SPort.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        public ICommand CloseAppCommand => new RelayCommand(CloseApp);
        public ICommand MaxAppCommand => new RelayCommand(MaxApp);
        public ICommand MiniAppCommand => new RelayCommand(MiniApp);
        #endregion Command

        #region Metodi
        // Close App
        public void CloseApp(object obj)
        {
            MainWindow win = obj as MainWindow;
            win.Close();
        }

        // Maximize/Restore App
        public void MaxApp(object obj)
        {
            MainWindow win = obj as MainWindow;

            if (win.WindowState == WindowState.Normal)
            {
                win.WindowState = WindowState.Maximized;
            }
            else if (win.WindowState == WindowState.Maximized)
            {
                win.WindowState = WindowState.Normal;
            }
        }

        public void MiniApp(object obj)
        {
            MainWindow win = obj as MainWindow;
            win.WindowState = WindowState.Minimized;
        }

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
