using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookAddInThreadsAndQueue.Entities
{
   public class Url
    {

        public string scheme { get; set; }

        public string host { get; set; }

        public bool validUrl { get; set; }

        public Url(string scheme, string host)
        {
            this.scheme = scheme;
            this.host = host;
            this.validUrl = validUrl;
        }

        public Url()
        {
        }
    }
}

