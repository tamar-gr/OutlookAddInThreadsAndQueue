using OutlookAddInThreadsAndQueue.CheckUrlSecurity.parsers;
using OutlookAddInThreadsAndQueue.CheckUrlSecurity.tools;
using OutlookAddInThreadsAndQueue.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookAddInThreadsAndQueue.CheckUrlSecurity
{
    public class url_state_machine
    {
        EncodedTypeCharCheck checkType = new EncodedTypeCharCheck();
        InputIntegrityCheck intedrityCheck = new InputIntegrityCheck();
        HostParser hostParser = new HostParser();
        // PercentEncoding percentEnc = new PercentEncoding();
        List<string> listOfSpecialScheme = new List<string> { "http", "https", "ws", "wss" };

        public string stateOverid { get; set; }
        public string input { get; set; }
        public StringBuilder buffer { get; set; }
        public Url url { get; set; }
        public string state { get; set; }
        public bool insideBrackets { get; set; }

        public url_state_machine()
        {
        }

        public bool CheckIfValidUrl(string input)
        {
            this.stateOverid = stateOverid;
            this.input = input;
            this.url = url;
            this.state = (stateOverid != null) ? stateOverid : "schemeState";
            this.buffer = new StringBuilder();
            this.insideBrackets = false;
            bool v = UrlStateMachine();
            this.url.validUrl = v;
            return this.url.validUrl;
        }
        public bool UrlStateMachine()
        {
            bool valFromFunc = false;

            if (url == null)
            {
                url = new Url();
                input = intedrityCheck.trimControl(input);
            }
            for (int i = 0; i <= input.Length; i++)
            {
                switch (this.state)
                {

                    case "schemeState":

                        valFromFunc = schemeState(ref i);
                        break;
                    case "noSchemeState":
                        valFromFunc = NoSchemeState(ref i);
                        break;

                    case "hostState":
                        valFromFunc = hostState(ref i);
                        break;

                    case "finalState":
                        valFromFunc = finalState(ref i);
                        break;
                }
                if (valFromFunc == false)
                {
                    return url.validUrl = false; ;
                }
                else
                {

                }
            }


            return url.validUrl = true;
        }

        private bool schemeState(ref int i)
        {

            char c = i != input.Length ? input[i] : ' ';
            if (checkType.isASCIIAlphanumeric((byte)c) || checkType.isPlusMinusDot((byte)c))
            {
                buffer.Append(c.ToString().ToLower());
            }
            else if ((byte)c == 0x3A)
            {
                if (stateOverid != null)
                {
                    string sche = url.scheme;

                    if (listOfSpecialScheme.Contains(sche) && !listOfSpecialScheme.Contains(buffer.ToString()))
                    {
                        return false;
                    }
                    else if (!listOfSpecialScheme.Contains(sche) && listOfSpecialScheme.Contains(buffer.ToString()))
                    {
                        return false;
                    }

                }

                url.scheme = buffer.ToString();

                buffer.Clear();
                if (listOfSpecialScheme.Contains(url.scheme))
                {
                    if (input[i + 1] == 0x2F && input[i + 2] == 0x2F)
                    {
                        i += 2;
                        state = "hostState";
                        return true;
                    }
                    return false;
                }

                else
                {

                    return false;

                }
            }

            else
            {
                buffer.Clear();
                buffer.Append("");
                state = "noSchemeState";
                i = -1;
                return true;

            }

            return true;
        }
        private bool NoSchemeState(ref int i)
        {

            char c = i != input.Length ? input[i] : ' ';
            if (input.StartsWith("www"))
            {
                input = listOfSpecialScheme[0] + "://" + input;
                state = "schemeState";
                i = -1;
                return true;

            }
            return false;
        }
        #region cases

        #endregion
        private bool hostnameHelperFunc(int index, string input, Url url)
        {
            char c = index != input.Length ? input[index] : ' ';
            if (c == 0x2F || c == 0x3F || c == 0x23 || index == input.Length)
            {
                return true;
            }
            else if (listOfSpecialScheme.Contains(url.scheme) && c == 0x5C)
            {
                return true;
            }
            return false;
        }
        private bool hostState(ref int i)
        {
            char c = i != input.Length ? input[i] : ' ';


            if (c == 0x3A && !insideBrackets)
            {
                if (buffer.Length == 0)
                {
                    return false;
                }
                string host = hostParser.hostParser(buffer.ToString(), false);

                if (host == "")
                {
                    return false;
                }
                url.host = host;
                buffer.Length = 0;
                state = "finalState";
                if (stateOverid != null && stateOverid == "hostState")
                {
                    return false;
                }
                return true;
            }
            else if (hostnameHelperFunc(i, input, url))
            {
                i -= buffer.Length + 1;
                if (listOfSpecialScheme.Contains(url.scheme) && buffer.Length == 0)
                {
                    return false;
                }

                string host = hostParser.hostParser(buffer.ToString(), false);

                if (host == "")
                {
                    return false;
                }
                url.host = host;
                buffer.Length = 0;
                state = "finalState";

                if (stateOverid != null)
                {
                    return false;
                }
                return true;
            }
            else
            {
                if (c == 0x5B)
                {
                    insideBrackets = true;
                }
                if (c == 0x5D)
                {
                    insideBrackets = false;
                }

                buffer.Append(c);
                return true;

            }
        }

        public bool finalState(ref int i)
        {
            char c = i != input.Length ? input[i] : ' '; ;
            if (i != input.Length)
            {
                i = input.Length;
                return true;
            }
            return true;
        }
    }
}
