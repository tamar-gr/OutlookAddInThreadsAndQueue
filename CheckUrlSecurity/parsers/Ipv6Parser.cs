using OutlookAddInThreadsAndQueue.CheckUrlSecurity.tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookAddInThreadsAndQueue.CheckUrlSecurity.parsers
{
    class Ipv6Parser
    {
        public string Ipv6ParserFunc(string input)
        {
            string failure = "";
            EncodedTypeCharCheck enc = new EncodedTypeCharCheck();
            int[] address = { 0, 0, 0, 0, 0, 0, 0, 0 };
            int pieceIndex = 0;
            int compress = -1;
            int i = 0;
            if ((byte)input[0] == 0x3A)
            {
                if ((byte)input[1] != 0x3A)
                {
                    return failure;
                }
                i += 2;
                pieceIndex++;
                compress = pieceIndex;
            }
            while (i < input.Length)
            {
                if (pieceIndex == 8)
                {
                    return failure;
                }
                if ((byte)input[i] == 0x3A)
                {
                    if (compress != -1)
                    {
                        return failure;
                    }
                    i++;
                    pieceIndex++;
                    compress = pieceIndex;
                    continue;
                }
                int value = 0;
                int length = 0;
                while ((length < 4) && enc.isASCIIHex((byte)input[i]))
                {
                    string hex = ((int)input[i]).ToString("x2");
                    value = value * 0x10 + Convert.ToInt32(hex);
                    i++;
                    length++;
                }
                char c = i != input.Length ? input[i] : ' ';
                if ((byte)c == 0x2E)
                {
                    if (length == 0)
                    {
                        return failure;
                    }
                    i -= length;
                    if (pieceIndex > 6)
                    {
                        return failure;
                    }
                    int numbersSeen = 0;
                    while (i != input.Length)
                    {
                        int ipv4Piece = -1;
                        if (numbersSeen > 0)
                        {
                            if ((byte)input[i] == 0x2E && numbersSeen < 4)
                            {
                                i++;
                            }
                            else
                            {
                                return failure;
                            }
                        }
                        if (!enc.isASCIIDigit((byte)input[i]))
                        {
                            return failure;
                        }
                        while (enc.isASCIIDigit((byte)c))
                        {
                            int number = Convert.ToInt32(input[i]);
                            if (ipv4Piece == -1)
                            {
                                ipv4Piece = number;

                            }
                            else if (ipv4Piece == 0)
                            {
                                return failure;
                            }
                            else
                            {
                                ipv4Piece = ipv4Piece * 10 + number;
                            }
                            if (ipv4Piece > 255)
                            {
                                return failure;
                            }
                            i++;
                        }
                        address[pieceIndex] = address[pieceIndex] * 0x100 + ipv4Piece;
                        numbersSeen++;
                        if (numbersSeen == 2 || numbersSeen == 4)
                        {
                            pieceIndex++;
                        }
                    }
                    if (numbersSeen != 4)
                    {
                        return failure;
                    }
                    break;
                }
                else if ((byte)c == 0x3A)
                {
                    i++;
                    if (i == input.Length)
                    {
                        return failure;
                    }
                }
                else if (i != input.Length)
                {
                    return failure;
                }
                address[pieceIndex] = value;
                pieceIndex++;
            }
            if (compress != -1)
            {
                int swaps = pieceIndex - compress;
                pieceIndex = 7;
                while (pieceIndex != 0 && swaps > 0)
                {
                    int val = address[pieceIndex];
                    address[pieceIndex] = address[compress + swaps - 1];
                    address[compress + swaps - 1] = val;
                    pieceIndex--;
                    swaps--;
                }
            }
            else if (compress == -1 && pieceIndex != 8)
            {
                return failure;
            }
            StringBuilder ipv6Adress = new StringBuilder();
            foreach (int num in address)
            {
                ipv6Adress.Append(num);
                ipv6Adress.Append(":");
            }
            ipv6Adress.Length -= 1;
            return ipv6Adress.ToString();
        }
    }
}
