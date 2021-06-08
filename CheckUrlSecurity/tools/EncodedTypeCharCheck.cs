using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookAddInThreadsAndQueue.CheckUrlSecurity.tools
{
    public class EncodedTypeCharCheck
    {
        public bool isASCIIDigit(byte c)
        {
            return c >= 0x30 && c <= 0x39;
        }

        public bool isASCIIAlpha(byte c)
        {
            return (c >= 0x41 && c <= 0x5A) || (c >= 0x61 && c <= 0x7A);
        }

        public bool isASCIIAlphanumeric(byte c)
        {
            return isASCIIAlpha(c) || isASCIIDigit(c);
        }

        public bool isASCIIHex(byte c)
        {
            return isASCIIDigit(c) || (c >= 0x41 && c <= 0x46) || (c >= 0x61 && c <= 0x66);
        }
        public bool isC0ControlPercentEncode(byte c)
        {
            return c <= 0x1F || c > 0x7E;
        }

        public bool IsC0ControlOrWhiteSpace(byte c)
        {
            return isC0ControlPercentEncode(c) || c == 0x20 || c == 0x3c || c == 0x3d;
        }

        public bool isPlusMinusDot(byte c)
        {
            return c == 0x2B || c == 0x2E || c == 0x2D;
        }

        public bool forbiddenHostCodePoint(char c)
        {
            List<byte> list = new List<byte> { 0x00, 0x09, 0x0A, 0x0D, 0x20, 0x23, 0x25, 0x2F, 0x3A, 0x3C, 0x3E, 0x3F, 0x40, 0x5B, 0x5C, 0x5D, 0x5E };
            return list.Contains((byte)c);
        }
    }
}
