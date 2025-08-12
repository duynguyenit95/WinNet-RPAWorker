using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;

namespace OutlookReader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Console UTF-8 support
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== HRIS Mail Checker started ===");

            using HttpClient httpClient = new HttpClient();

            string baseUrl = "https://ros.reginamiracle.com:8118/wecomNotify/hubMail";

            while (true)
            {
                try
                {
                    // Đợi mail mới trong folder HRIS, keyword subject có thể để trống nếu lấy tất
                    MailItem? mail = await OutlookInterop.WaitForNewMailAsync(
                        subjectKeyword: "",
                        rootFolderName: "HRIS"
                    );

                    if (mail != null)
                    {
                        var payload = new
                        {
                            subject = mail.Subject,
                            body = mail.Body,
                        };

                        //string apiUrl = $"http://ros:8112/AbhScrapper/mailHub?mailContent={body}";
                        var response = await httpClient.PostAsJsonAsync(baseUrl, payload);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Sent to API successfully.");

                            mail.UnRead = false;  // Mark as read to skip next time
                            mail.Save();
                        }
                        else
                        {
                            Console.WriteLine($"API call failed: {response.StatusCode}");
                        }

                        // Giải phóng COM object
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(mail);
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                await Task.Delay(2000); // Nhỏ delay vì WaitForNewMailAsync đã tự chờ 5s
            }
        }
    }
}
