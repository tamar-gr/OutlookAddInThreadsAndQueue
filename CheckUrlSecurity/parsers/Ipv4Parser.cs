using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookAddInThreadsAndQueue.CheckUrlSecurity.parsers
{
   public class Ipv4Parser
    {
        Ipv4NumberParser Ipv4Number = new Ipv4NumberParser();

        public string IPv4Parser(string input)
        {
            string failure = "";
            string[] parts = input.Split('.');
            if (parts[parts.Length - 1] == "")
            {
                if (parts.Length > 1)
                {
                    Array.Resize(ref parts, parts.Length - 1);
                }
            }
            if (parts.Length > 4)
            {
                return input;
            }
            List<int> numbers = new List<int>();
            foreach (string part in parts)
            {
                if (part == "")
                {
                    return input;
                }
                int result = Ipv4Number.IPv4NumberParser(part);
                if (result == 0)
                {
                    return input;
                }
                numbers.Add(result);
            }
            for (int i = 0; i < numbers.Count; i++)
            {

                if ((i < numbers.Count - 1) && numbers[i] > 255)
                {
                    return failure;
                }
                else if (numbers[i] > 255)
                {
                    return failure;
                }
                else if ((i == numbers.Count - 1) && numbers[i] >= Math.Pow(256, 5 - numbers.Count))
                {
                    return failure;
                }
            }
            int ipv4 = numbers[numbers.Count - 1];
            numbers.RemoveAt(numbers.Count - 1);
            int counter = 0;
            foreach (int n in numbers)
            {
                ipv4 += n * (int)Math.Pow(256, 3 - counter);
                counter++;
            }
            return ipv4.ToString();
        }
    }
}
