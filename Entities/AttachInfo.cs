
namespace OutlookAddInThreadsAndQueue.Entities
{
    public class AttachInfo
    {
        public string fileName { get; set; }
        public int size { get; set; }
        public string attachmentHashCode { get; set; }
        public byte[] content { get; set; }

    }
}
