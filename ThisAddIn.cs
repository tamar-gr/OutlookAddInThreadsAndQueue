using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using OutlookAddInThreadsAndQueue.Entities;
using Outlook = Microsoft.Office.Interop.Outlook;
using OutlookAddInThreadsAndQueue.CheckAttachmentSecurity;
using OutlookAddInThreadsAndQueue.CheckAttachmentSecurity.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Office = Microsoft.Office.Core;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Security.AccessControl;
using System.Net.Http;
using System.Reactive.Linq;
using System.IO;
using System.Net;
using System.Collections;
using System.Threading;
using System.Text.RegularExpressions;
using OutlookAddInThreadsAndQueue.CheckUrlSecurity;
using OutlookAddInThreadsAndQueue.CheckUrlSecurity.server;

namespace OutlookAddInThreadsAndQueue
{
    public partial class ThisAddIn
    {
        static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = _cancellationTokenSource.Token;
        BlockingCollection<MailInfo> waitingMails = new BlockingCollection<MailInfo>();// Construct a Blocking Collection of new mails
        //Task securityTask; //responsible for checkSecurity thread
        AttachInfo myAttachmentInfo;
        url_state_machine urlChecker = new url_state_machine();
        CheckPointSecureUrlClient serverCheck = new CheckPointSecureUrlClient();

        //opens an output file in current directory
        static string path = Directory.GetCurrentDirectory() + "\\logThreatEmulation.txt";
        public static StreamWriter outputFile = new StreamWriter(path, false);//override file if already exists


        #region ThisAddIn_Startup
        private void ThisAddIn_Startup(object sender, EventArgs e)
        {
            Task.Run(() => CheckMailSecurity());
            Application.NewMailEx += new Outlook.ApplicationEvents_11_NewMailExEventHandler(Application_NewMailEx);
            ((Outlook.ApplicationEvents_11_Event)Application).Quit += new Outlook.ApplicationEvents_11_QuitEventHandler(ThisAddIn_Quit);
        }
        #endregion

        #region NewMailEx event
        //an event that accures when new mail arrives
        void Application_NewMailEx(string EntryIDCollection)
        {
            MailInfo maiInfo = new MailInfo();
            try
            {
                var mailItem = (Outlook.MailItem)Application.Session.GetItemFromID(EntryIDCollection, missing);

                if (((Outlook.MailItem)Application.Session.GetItemFromID(EntryIDCollection, missing)).UnRead)
                {
                    maiInfo.mailID = EntryIDCollection;
                    maiInfo.body = mailItem.Body;
                    maiInfo.subject = mailItem.Subject;
                    maiInfo.sender = mailItem.SenderName;
                    maiInfo.senderEmailAddress = mailItem.SenderEmailAddress;
                    maiInfo.receivedTime = mailItem.ReceivedTime.ToString();
                    maiInfo.sentOn = mailItem.SentOn.ToString();
                }

                //if there is attachments in the mail, add to list
                for (int i = 1; i <= mailItem.Attachments.Count; i++)
                {
                    Outlook.Attachment attachment = mailItem.Attachments[i];
                    myAttachmentInfo = new AttachInfo
                    {
                        fileName = attachment.FileName,
                        size = attachment.Size,
                        attachmentHashCode = CalculateSHA1.GetSHA1(attachment),
                        content = CalculateSHA1.ConvertAttachmentToBytes(attachment)
                    };
                    maiInfo.attachmentInfoList.Add(myAttachmentInfo); //keep in list the info of the files and insert it to queue
                }

                //adding the mailItem into the BlockingCollection
                waitingMails.Add(maiInfo);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region CheckMailSecurity-function of securityTask
        public async void CheckMailSecurity()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)                            // cancellationToken: Object that can be used to cancel the take operation.      
                {                                                                                   //A call to Take may block until an item is available to be removed or the token is canceled
                    MailInfo mailitem = waitingMails.Take(token);
                    var checkUrlAndIp = Task.Run(() => CheckUrlAndIp(mailitem));
                    var checkAttachments = Task.Run(() => CheckAttachmentsSecurity(mailitem));
                    await Task.WhenAll(checkUrlAndIp, checkAttachments);
                   
                }
            }

            catch (OperationCanceledException)
            {
                Console.WriteLine("Taking canceled.");
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Check Attachments Security
        public void CheckAttachmentsSecurity(MailInfo mailItem)
        {
            try
            {
                if (mailItem.attachmentInfoList.Count > 0)
                {
                    foreach (var attachment in mailItem.attachmentInfoList)
                    {
                        var responseReputationService = CheckPointService.CheckFileReputationService(attachment);
                        var reputation = responseReputationService["response"][0]["reputation"];
                        string classification = reputation["classification"].ToString();
                        string severity = reputation["severity"].ToString();
                        string confidence = reputation["confidence"].ToString();
                        string risk = responseReputationService["response"][0]["risk"].ToString();
                        int riskNumber = int.Parse(risk);
                        if (riskNumber > 50)
                        {
                            var mail = (Outlook.MailItem)Application.Session.GetItemFromID(mailItem.mailID, missing);
                            string alertMessage = "The attachment: " + attachment.fileName + " is not secured,\r\n Classification: " + classification;
                            alertMessage += " Severity: " + severity + " Confidence: " + confidence; 
                            mail.HTMLBody = mail.HTMLBody.Insert(0, alertMessage);
                        }
                    }

                    //if the file is malicious add message alert to the mail's body
                    var tupleResponseList = CheckPointService.CheckFileThreatEmulation(mailItem);

                    foreach (var tuple in tupleResponseList)
                    {
                        string fileName = tuple.Item1;
                        string severity = tuple.Item2;
                        string confidence = tuple.Item3;

                        //alert to user in mail's body about the attachment security
                        if (severity == "benign" && confidence == "benign")//the attachment is benign
                        {
                            var mail = (Outlook.MailItem)Application.Session.GetItemFromID(mailItem.mailID, missing);
                            string alertMessage = "\r\n\n\n" + "The attachment: " + fileName + " is secure. " + "\r\n\n\n";
                            mail.HTMLBody = mail.HTMLBody.Insert(0, alertMessage);
                        }

                        else if (severity != "" && confidence != "")//the attachment is malicious
                        {
                            var mail = (Outlook.MailItem)Application.Session.GetItemFromID(mailItem.mailID, missing);
                            string alertMessage = "\r\n\n\n" + "Warning Security!" + "\r\n\n\n";
                            alertMessage += "The attachment: " + fileName + " is not secured\r\n Severity: " + severity + " Confidence: " + confidence;
                            mail.HTMLBody = mail.HTMLBody.Insert(0, alertMessage);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Check Url And Ip Security
        
        public void CheckUrlAndIp(MailInfo mailItem)
        {
            string[] seperatingTags = { "<", ">" };
            StringBuilder mailWithoutMaliciousUrl = new StringBuilder();
            List<string> splitMessage = mailItem.body.Split(seperatingTags, StringSplitOptions.RemoveEmptyEntries).ToList();
            var mail = (Outlook.MailItem)this.Application.Session.GetItemFromID(mailItem.mailID);
            for (int i = 0; i < splitMessage.Count; i++)
            {
                int lengthBody = mailItem.body.Length;
                if (!(splitMessage[i].StartsWith("http") || splitMessage[i].StartsWith("www") || splitMessage[i].StartsWith("https"))) { mailWithoutMaliciousUrl.Append(splitMessage[i]); continue; }
                //check if valid url
                if (!urlChecker.CheckIfValidUrl(splitMessage[i])) { mailWithoutMaliciousUrl.Append(splitMessage[i]); continue; }
                // valid url, lets check if safe
               if (serverCheck.PostAsyncfunc(splitMessage[i].Substring(0, splitMessage[i].Length - 2)) == "url is not malicious")
                {
                  
                    mailWithoutMaliciousUrl.Append(splitMessage[i]); 
                }
            }
            //append only not malicious url
            if ((mail.EntryID == mailItem.mailID))
            {
                mail.Body = mailWithoutMaliciousUrl.ToString();
                mail.Save();

            }
        }
        #endregion

        #region ThisAddIn_Quit event
        public void ThisAddIn_Quit()
        {
            outputFile.Close();
            _cancellationTokenSource.Cancel();
        }
        #endregion

        #region VSTO generated code
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // Note: Outlook no longer raises this event. If you have code that 
            //    must run when Outlook shuts down, see https://go.microsoft.com/fwlink/?LinkId=506785
        }

        /// <summary>z
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
