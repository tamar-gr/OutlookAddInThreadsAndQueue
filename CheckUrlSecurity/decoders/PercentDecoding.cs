using OutlookAddInThreadsAndQueue.CheckUrlSecurity.encoders;
using OutlookAddInThreadsAndQueue.CheckUrlSecurity.tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookAddInThreadsAndQueue.CheckUrlSecurity.decoders
{
  public  class PercentDecoding
    {
        EncodingUtf8 utf8 = new EncodingUtf8();
        EncodedTypeCharCheck charType = new EncodedTypeCharCheck();
        /// <summary>
        /// Given an encoded input the function convert it back to the original form by using utf-8 percent encoding
        /// </summary>
        public List<byte> percentDecoding(string input)
        {

            List<byte> output = new List<byte> { };
            for (int i = 0; i < input.Length; i++)
            {
                if ((byte)input[i] != 0x25)
                {
                    output.Add((byte)input[i]);
                    continue;
                }
                if ((byte)input[i] == 0x25 && (!charType.isASCIIHex((byte)input[i + 1]) || !charType.isASCIIDigit((byte)input[i + 2])))
                {
                    output.Add((byte)input[i]);
                    continue;
                }
                else
                {
                    char decodedChar = Uri.HexUnescape(input, ref i);
                    output.Add((byte)decodedChar);
                    i += 2;
                }
            }
            return output;


        }
        public List<byte> percentDecoder(string input)
        {

            string encodingInput = utf8.encodingUTF8(input);
            return percentDecoding(encodingInput);


        }
    }
}
