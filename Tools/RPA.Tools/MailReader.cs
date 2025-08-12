using System.Text.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Tools
{
    public class MailReader
    {
        public void ReadEmail()
        {
            using (var msg = new MsgReader.Outlook.Storage.Message("d:\\testfile.msg"))
            {
                var from = msg.Sender;
                var sentOn = msg.SentOn;
                var recipientsTo = msg.GetEmailRecipients(MsgReader.Outlook.RecipientType.To, false, false);
                var recipientsCc = msg.GetEmailRecipients(MsgReader.Outlook.RecipientType.Cc, false, false);
                var subject = msg.Subject;
                var htmlBody = msg.BodyHtml;
                // etc...
            }
        }
    }

}
