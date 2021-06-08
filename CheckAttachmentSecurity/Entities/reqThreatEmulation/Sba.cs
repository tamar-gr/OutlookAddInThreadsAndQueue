
namespace OutlookAddInThreadsAndQueue.CheckAttachmentSecurity
{
    class Sba
    {
        public string blade_version { set; get; }
        public string emulation_reason { set; get; }
        public string[] file_attributes { set; get; }
        public string file_path { set; get; }
        public string host_id { set; get; }
        public string process_hash { set; get; }
        public string process_path { set; get; }
        public int users_logged_on { set; get; }
    }
}
