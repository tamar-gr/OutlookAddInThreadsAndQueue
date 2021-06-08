using System.Linq;
using System.Security.Cryptography;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OutlookAddInThreadsAndQueue.CheckAttachmentSecurity
{
    class CalculateSHA1
    {
        #region GetSHA1
        //gets an attachment and compute its SHA1
        public static string GetSHA1(Outlook.Attachment attachment)
        {
            byte[] arrBytes = ConvertAttachmentToBytes(attachment);
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                return string.Concat(sha1.ComputeHash(arrBytes).Select(item => item.ToString("X2")));
            }

        }
        #endregion

        #region ConvertAttachmentToBytes
        //gets an attachment and convert it to arr bytes
        public static byte[] ConvertAttachmentToBytes(Outlook.Attachment attachment)
        {
            const string PR_ATTACH_DATA = "http://schemas.microsoft.com/mapi/proptag/0x37010102";
            return attachment.PropertyAccessor.GetProperty(PR_ATTACH_DATA);
        }
        #endregion
    }
}
