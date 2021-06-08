using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookAddInThreadsAndQueue.CheckUrlSecurity.tools
{
   public class InputIntegrityCheck
    {

        EncodedTypeCharCheck checkType = new EncodedTypeCharCheck();
        public string trimControl(string s)
        {
            StringBuilder newString = new StringBuilder();
            foreach (char c in s)
            {
                if (!checkType.IsC0ControlOrWhiteSpace((byte)c))
                    newString.Append(c);
            }
            return newString.ToString();
        }
    }
}
