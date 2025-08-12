using RPA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQ09.Handle
{
    public class Mail
    {
        public async Task SendMailResult(string content, string step = "UQ09", string attachfiles = "", bool isException = false)
        {
            string[] listMail;
            if (isException)
            {
                listMail = new string[] {"darius.nguyen@reginamiracle.com"};
            }
            else
            {
                var listMailFile = Path.Combine(AppContext.BaseDirectory, "Parameter", "ListMailReceiver.txt");
                listMail = File.ReadAllLines(listMailFile);
            }
            List<string> attach = new List<string>();
            if (!string.IsNullOrEmpty(attachfiles))
            {
                attach = attachfiles.Split(",").ToList();
            }

            Console.WriteLine("Sending email...");
            var response = await HttpClientHelper.HttpSendMail(listMail, "UQ09", step, content, attach);
            Console.WriteLine($"Response: {response.StatusCode}"); 
        }
    }
}
