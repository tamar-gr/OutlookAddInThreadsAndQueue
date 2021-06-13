using System;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using OutlookAddInThreadsAndQueue.Entities;

namespace OutlookAddInThreadsAndQueue.CheckAttachmentSecurity.Services
{
    class CheckPointService
    {
        //Data For Reputation Service 
        static string CLIENT_KEY_RS = "2fddabf7-ac89-4ba5-9c1d-69af31f86408";
        static string TOKEN = "exp=1623855427~acl=/*~hmac=356c729ca4eef44ee7bad67b36501c9a552328bc9502a43d21d1deb2b370cf93";
        static string RS_URL = "https://rep.checkpoint.com/file-rep/service/v2.0/query?";

        //Data For Threat Emulation
        static string CLIENT_KEY_TE = "CK-B83D83DA69DD";
        static string QUERY_URL = "https://te.checkpoint.com/tecloud/api/v1/file/query";
        static string UPLOAD_URL = "https://te.checkpoint.com/tecloud/api/v1/file/upload";

        #region check file reputation service
        public static JObject CheckFileReputationService(AttachInfo myAttachment)
        {
            // Create the body request 
            string bodyRequest = "{\"request\":[{\"resource\":\""+myAttachment.attachmentHashCode+"\"}]}";

            string url = $"{RS_URL}resource={myAttachment.attachmentHashCode}";

            var contentType = "application/json";

            var headers = new Dictionary<string, string>()
            {
                {"Client-Key" , CLIENT_KEY_RS},
                {"token" , TOKEN }
            };

            //write request to output file
            ThisAddIn.outputFile.WriteLine("\r\n\n\n" + DateTime.Now + " Writing request. Url:\n" + url + "\n" + bodyRequest);
            JObject jsonResponse = HttpRequests.Post(url, bodyRequest, contentType, headers);
            return jsonResponse;
        }
        #endregion

        #region check file Threat Emulation
        public static List<Tuple<string, string, string>> CheckFileThreatEmulation(MailInfo mailItem)
        {
            int i = 0;
            string messageResponse, severity, confidence;
            messageResponse = severity = confidence= "";

            var resultTable = new List<Tuple<string, string, string>>();

            // Create the body of all attachments together 
            var jsonBody = buildBodyBatchingTE(mailItem);
            string body = jsonBody.ToString();

            //write request to output file
            ThisAddIn.outputFile.WriteLine("\r\n\n\n" + DateTime.Now + " Writing request. Url:\n" + QUERY_URL + "\n" + body);

            string contentType = "application/json";

            var headers = new Dictionary<string, string>()
            {
                {"Authorization", CLIENT_KEY_TE},
                {"te_cookie", "remember" }
            };

            //http post query for all attachments at once
            JObject jsonResponse = HttpRequests.Post(QUERY_URL, body, contentType, headers);

            foreach (var attachment in mailItem.attachmentInfoList)
            {
                messageResponse = jsonResponse["response"][i]["status"]["message"].ToString();
                if (messageResponse == "Could not find the requested file. Please upload it.")
                {
                    string uploadBody = buildBodyUploadTE(attachment.fileName, attachment.attachmentHashCode);

                    //write request to output file
                    ThisAddIn.outputFile.WriteLine("\r\n\n\n" + DateTime.Now + " Writing request. Url:\n" + UPLOAD_URL + "\n" + uploadBody);

                    //http post upload file
                    JObject uploadJsonResponse = HttpRequests.PostMultipartData(UPLOAD_URL, uploadBody, attachment);

                    messageResponse = uploadJsonResponse["response"]["status"]["message"].ToString();

                    if (messageResponse == "The file was uploaded successfully.")
                    {
                        //repeat post upload every 10 seconds until file isn't pending 
                        do
                        {
                            Thread.Sleep(10000);

                            //write request to output file
                            ThisAddIn.outputFile.WriteLine("\r\n\n\n" + DateTime.Now + " Writing request. Url:\n" + UPLOAD_URL + "\n" + uploadBody);

                            //http upload again, to check the status of the file emulation
                            uploadJsonResponse = HttpRequests.PostMultipartData(UPLOAD_URL, uploadBody, attachment);
                            messageResponse = uploadJsonResponse["response"]["status"]["message"].ToString();
                        }
                        while (messageResponse == "The request is pending.");
                    }

                    if (messageResponse == "The request has been fully answered.")
                    {
                        var te_eb = uploadJsonResponse["response"]["te_eb"];
                        string verdict = te_eb["combined_verdict"].ToString();
                        if (verdict == "malicious")
                        {
                            //check the security of the attachment and do alert to user
                            severity = te_eb["severity"].ToString();
                            confidence = te_eb["confidence"].ToString();
                            resultTable.Add(Tuple.Create(attachment.fileName, severity, confidence));
                        }
                        else //the attachment is benign
                        {
                            severity = "benign";
                            confidence = "benign";
                            resultTable.Add(Tuple.Create(attachment.fileName, severity, confidence));
                        }
                    }
                }
                i++;
            }
            return resultTable;
        }
        #endregion

        #region Build Body Batching Query Request Threat Emulation
        public static JObject buildBodyBatchingTE(MailInfo mailItem)
        {
            // Create the body request of all attachments in current mail together -Batching Request
            string[] myFeatures = new string[]
            {
              "te",
              "te_eb"
            };

            Te myTe = new Te()
            {
                reports = new string[]
                {
                    "xml"
                }
            };

            var myRequest = new CollectionRequest()
            {
                request = new Request[mailItem.attachmentInfoList.Count]
            };

            for (int i = 0; i < mailItem.attachmentInfoList.Count; i++)
            {
                Request myInnerRequest = new Request()
                {
                    features = myFeatures,
                    file_name = mailItem.attachmentInfoList[i].fileName,
                    sha1 = mailItem.attachmentInfoList[i].attachmentHashCode,
                    te = myTe
                };

                myRequest.request[i] = myInnerRequest;
            }
            return JObject.Parse(JsonConvert.SerializeObject(myRequest));
        }
        #endregion

        #region Build Body Of One Upload Request Threat Emulation
        public static string buildBodyUploadTE(string fileName, string hash)
        {
            string[] myFeatures = new string[]
            {
              "te",
              "te_eb"
            };

            Te myTe = new Te()
            {
                reports = new string[]
                {
                    "xml"
                }
            };

            var myRequest = new CollectionRequest()
            {
                request = new Request[1]
            };

            Request myInnerRequest = new Request()
            {
                features = myFeatures,
                file_name = fileName,
                sha1 = hash,
                te = myTe
            };

            myRequest.request[0] = myInnerRequest;

            return JObject.Parse(JsonConvert.SerializeObject(myRequest)).ToString();
        }
        #endregion
    }
}




