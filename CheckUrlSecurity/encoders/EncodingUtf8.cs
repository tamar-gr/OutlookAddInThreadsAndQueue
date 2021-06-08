using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookAddInThreadsAndQueue.CheckUrlSecurity.encoders
{
    public class EncodingUtf8
    {
        public string encodingUTF8(string input)
        {
            UTF8Encoding UTF8 = new UTF8Encoding();
            byte[] encoderbytes = UTF8.GetBytes(input);
            string encodingInput = Encoding.UTF8.GetString(encoderbytes);
            return encodingInput;
        }
    }
}
