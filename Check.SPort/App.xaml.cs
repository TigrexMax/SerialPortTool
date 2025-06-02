using Check.SPort.Models;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Check.SPort
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ProtocolloComunicazione SettingsProtocol { get; } = new ProtocolloComunicazione();
    }
}
