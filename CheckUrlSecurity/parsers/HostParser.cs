using OutlookAddInThreadsAndQueue.CheckUrlSecurity.decoders;
using OutlookAddInThreadsAndQueue.CheckUrlSecurity.tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookAddInThreadsAndQueue.CheckUrlSecurity.parsers
{
    public class HostParser
    {
        Ipv6Parser ipv6Parser = new Ipv6Parser();
        Ipv4Parser ipv4Parser = new Ipv4Parser();
        PercentDecoding decodedChar = new PercentDecoding();
        EncodedTypeCharCheck checkTypeOfChar = new EncodedTypeCharCheck();

        public string hostParser(string input, bool isNotSpecial)
        {
            string asciDomain = "";
            string faliure = "";
            char c = input[0];
            if (c == 0x5B)
            {
                if (input[input.Length - 1] != 0x5D)
                {
                    return faliure;
                }
                input = input.Remove(0, 1);
                input = input.Remove(input.Length - 1, 1);
                return ipv6Parser.Ipv6ParserFunc(input);
            }

            if (input != "")
            {
                Decoder utf8Decoder = Encoding.UTF8.GetDecoder();
                byte[] decode = decodedChar.percentDecoder(input).ToArray();
                char[] domain = new char[utf8Decoder.GetCharCount(decode, 0, decode.Length)];
                utf8Decoder.GetChars(decode, 0, decode.Length, domain, 0);
                asciDomain = Gnu.Inet.Encoding.IDNA.ToASCII(new string(domain));
                if (asciDomain == "" || asciDomain == null)
                {
                    return faliure;
                }
                for (int i = 0; i < asciDomain.Length; i++)
                {
                    if (checkTypeOfChar.forbiddenHostCodePoint(c))
                    {
                        return faliure;
                    }

                }
                string ipv4Host = ipv4Parser.IPv4Parser(asciDomain);
                if (ipv4Host == "" || isIpv4Digit(ipv4Host))
                {
                    return ipv4Host;
                }
                return asciDomain;
            }
            return faliure;
        }
        private bool isIpv4Digit(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
