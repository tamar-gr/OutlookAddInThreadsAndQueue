
namespace OutlookAddInThreadsAndQueue.CheckAttachmentSecurity
{
    class Request
    {
        public string[] features { set; get; }
        public string file_name { set; get; }
        public string file_type { set; get; }
        public string md5 { set; get; }
        public Sba sba_metadata { set; get; }
        public string sha1 { set; get; }
        public Te te { set; get; }
    }
}
