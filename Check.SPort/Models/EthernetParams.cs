using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Check.SPort.Models
{
    public class EthernetParams : INotifyPropertyChanged
    {
        private string _ipAddress;
        private int _port;

        public EthernetParams()
        {
            RefreshFormValues();
        }

        public void RefreshFormValues()
        {
            if (this != null)
            {
                IpAddress = "192.168.1.15";
                Port = 9100;
            }
        }

        public string IpAddress
        {
            get => _ipAddress;
            set { _ipAddress = value; OnNotifyChanged(nameof(IpAddress)); }
        }

        public int Port
        {
            get => _port;
            set { _port = value; OnNotifyChanged(nameof(Port)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnNotifyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
