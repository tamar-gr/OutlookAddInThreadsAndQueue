
using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using OutlookAddInThreadsAndQueue.Entities;
using System.Windows.Forms;

namespace OutlookAddInThreadsAndQueue.CheckAttachmentSecurity.Services
{
    class HttpRequests
    {
        static readonly Encoding encoding = Encoding.UTF8;

        #region Post
        //headers is an optional field
        public static JObject Post(string postUrl, string body, string contentType, Dictionary<string, string> headers = null)
        {
            try
            {
                // Create POST data and convert it to a byte array.
                byte[] postData = encoding.GetBytes(body);

                WebRequest myWebRequest = WebRequest.Create(postUrl);

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        myWebRequest.Headers.Add(header.Key, header.Value);
                    }
                }

                myWebRequest.ContentType = contentType;
                myWebRequest.Method = "Post";
                myWebRequest.ContentLength = postData.Length;

                var newStream = myWebRequest.GetRequestStream(); // Get the request stream.
                newStream.Write(postData, 0, postData.Length); // Write the data to the request stream.
                WebResponse response = myWebRequest.GetResponse();
                string t_string = "";
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    t_string += reader.ReadToEnd();
                }
                JObject jsonResponse = JObject.Parse(t_string);
                ThisAddIn.outputFile.WriteLine("\r\n\n\n" + DateTime.Now + " Got response:\n" + jsonResponse.ToString());//write to file
                return jsonResponse;
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        #endregion

        #region PostMultipartData - for upload request
        public static JObject PostMultipartData(string uploadUrl, string body, AttachInfo fileToUpload)
        {
            int secondsSinceEpoch = (int)DateTimeOffset.Now.ToUnixTimeSeconds();

            string dataBoundary = secondsSinceEpoch.ToString();

            //create the request upload data
            var data = CreateMultipartData(body, dataBoundary, fileToUpload);

            string contentType = "multipart/form-data; boundary=" + dataBoundary;

            var uploadHeader = new Dictionary<string, string>()
            {
                   {"Authorization", "CK-4EE13BB6EC6C"}
            };
            return Post(uploadUrl, data, contentType, uploadHeader);
        }
        #endregion

        #region CreateMultipartData - for upload request
        private static string CreateMultipartData(string body, string boundary, AttachInfo fileToUpload)
        {
            Stream formDataStream = new MemoryStream();

            string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\nContent-Type: {2}\r\n\r\n",
                boundary,
                "request",
                "application/json");

            //concat body to postData
            postData += body;
            formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));

            formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

            string extention = Path.GetExtension(fileToUpload.fileName);
            string contentType;

            switch (extention)
            {
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".doc":
                    contentType = "application/msword";
                    break;
                case ".jpeg":
                case ".jpg":
                    contentType = "image/jpeg";
                    break;
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                default:
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
            }

            string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                boundary,
                "file",
                fileToUpload.fileName,
                contentType);

            //write header
            formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));// Write the header to the Stream 

            // Write the file data directly to the Stream, rather than serializing it to a string.
            formDataStream.Write(fileToUpload.content, 0, fileToUpload.content.Length);

            // Add the end of the request
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            //Stream into a string
            formDataStream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(formDataStream);
            string data = reader.ReadToEnd();
            formDataStream.Close();
            return data;
        }
        #endregion

    }
}


