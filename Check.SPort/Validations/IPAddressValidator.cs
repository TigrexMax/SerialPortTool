using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Check.SPort.Validations
{
    public class IPAddressValidator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? userInput = value as string;
            if (string.IsNullOrEmpty(userInput))
                return null;

            if (IPAddress.TryParse(userInput, out IPAddress? address) && address.ToString() == userInput)
                return address;
            else
                return null;
        }
    }
}
