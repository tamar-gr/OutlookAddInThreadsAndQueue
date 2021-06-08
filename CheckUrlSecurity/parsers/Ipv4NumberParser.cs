using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookAddInThreadsAndQueue.CheckUrlSecurity.parsers
{
   public  class Ipv4NumberParser
    {
        public int IPv4NumberParser(string input)
        {
            int returnList;
            int R = 10;
            if (input.Length >= 2 && input[0] == '0' && (input[1] == 'x' || input[1] == 'X'))
            {
                input = input.Substring(2);
                R = 16;

            }
            else if (input.Length >= 2 && (byte)input[0] == 0x30)
            {
                input = input.Substring(1);
                R = 8;
            }
            if (input == "")
            {
                returnList = 0;
                return returnList;
            }
            if (!isInGivenBase(input, R))
            {

                return 0;
            }
            int output = Convert.ToInt32(input, R);
            returnList = output;
            return returnList;
        }


        private bool isInGivenBase(String str,
                                int bas)
        {

            if (bas > 16)
                return false;

            else if (bas <= 10)
            {
                for (int i = 0; i < str.Length; i++)
                    if (!(str[i] >= '0' &&
                        str[i] < ('0' + bas)))
                        return false;
            }

            else
            {
                for (int i = 0; i < str.Length; i++)
                    if (!((str[i] >= '0' &&
                            str[i] < ('0' + bas)) ||
                        (str[i] >= 'A' &&
                            str[i] < ('A' + bas - 10))
                        ))
                        return false;
            }
            return true;
        }

    }
}
