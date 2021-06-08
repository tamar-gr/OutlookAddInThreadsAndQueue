using System.Collections.Generic;

namespace OutlookAddInThreadsAndQueue.Entities
{
    public class MailInfo
    {
        public string mailID { get; set; } 
        public string body { get; set; }
        public string subject { get; set; }
        public string sender { get; set; }
        public string senderEmailAddress { get; set; }
        public string receivedTime { get; set; }
        public string sentOn { get; set; }
        public List<AttachInfo> attachmentInfoList { get; set; }
        public MailInfo()
        {
            attachmentInfoList = new List<AttachInfo>();
        }
    }
}
