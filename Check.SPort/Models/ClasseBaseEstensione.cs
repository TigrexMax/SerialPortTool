using Custom.CeFCom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Check.SPort.Models
{
    public static class ClasseBaseEstensione
    {
        public static class Costanti
        {
            public const string XonXoff = "XonXoff";
            public const string XonXoff_NoEcho = "XonXoff NoEcho";
            public const string Custom = "Custom";
            public const string Custom_DLL = "Custom DLL";
        }

        public static int EseguiComando(this CeFCom ceFCom, string command, out string cmdResponse)
        {
            int response = ceFCom.CEFWriteRead(command, out cmdResponse);
            if (cmdResponse.Contains("ERR99"))
            {
                response = ceFCom.CEFWriteRead("1015", out cmdResponse);
            }
            return response;
        }
    }
}
