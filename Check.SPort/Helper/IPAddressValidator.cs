using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Check.SPort.Helper
{
    public class IPAddressValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string ipString = value as string;
            bool isValid = IPAddress.TryParse(ipString, out _);

            return isValid ? new ValidationResult(true, null) : new ValidationResult(false, "Indirizzo IP non valido");
        }
    }
}
