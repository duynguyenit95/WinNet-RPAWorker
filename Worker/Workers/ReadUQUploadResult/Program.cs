
using BIReport.Lib;
using RPA.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadUQUploadResult
{
    public class Program
    {
        public static string waitForSyncFolderPath = @"\\172.17.29.104\Archive\";

        static async Task Main()
        {
            await ReadUQUploadResult();
        }

        static async Task ReadUQUploadResult()
        {
            Console.WriteLine("START !!!");
            try
            {
                var syncFolder = new DirectoryInfo(waitForSyncFolderPath);
                var result = new List<string>();
                //List<string> allfiles = Directory.GetFiles(waitForSyncFolderPath, "*.log", SearchOption.AllDirectories).ToList();
                using (var db = new BIContext())
                {
                    var data = await db.BIReport_UQUploadResults.Where(x => x.Status == "Uploaded").ToListAsync();
                    Console.WriteLine($"Number files wait to sync: {data.Count}");
                    if (data.Count == 0)
                    {
                        return;
                    }
                    foreach (var dt in data)
                    {
                        var paramDate = dt.UploadTime;
                        Console.WriteLine($"{dt.ZipFileName}");
                        Console.WriteLine($"UploadTime: {paramDate.ToString("yyyy-MM-dd HH:mm:ss")}");
                        bool isFounded = false;
                        do
                        {
                            var path = syncFolder.GetDirectories(paramDate.Year.ToString()).FirstOrDefault().GetDirectories(paramDate.ToString("MM")).FirstOrDefault().GetDirectories(paramDate.ToString("dd")).FirstOrDefault();
                            Console.WriteLine($"Path: {path.FullName}");
                            var logFiles = path.GetFiles("*.log");
                            Console.WriteLine($"Files count: {logFiles.Count()}");
                            foreach (var file in logFiles)
                            {
                                var log = File.ReadAllText(file.FullName);
                                if (!log.Contains(dt.ZipFileName)) continue;
                                isFounded = true;
                                if (log.Split(',').FirstOrDefault() == "\"SPL_RP_SUCCESS\"") dt.Status = ProcessStatus.Success;
                                else dt.Status = ProcessStatus.Fail;
                                dt.UpdateTime = DateTime.Now;
                                Console.WriteLine($"{dt.ZipFileName}: {dt.Status}");
                                result.Add($"<br>{dt.ZipFileName}: {dt.Status}");
                                break;
                            }
                            paramDate = paramDate.AddDays(1);
                        }
                        while (paramDate > dt.UploadTime.AddDays(3));
                        if (!isFounded) Console.WriteLine($"{dt.ZipFileName}: NotSync");
                    }
                    await db.SaveChangesAsync();
                }
                if (result.Count == 0) return;
                //var detailContent = result.
                SendMailResult($"Pilot 3: Read Archive Files <br>" +
                    $"{string.Join("<br>", result)}"
                    , null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                SendMailResult($"Pilot 3: Read Archive Files <br>{ex.Message}");
            }
            Console.WriteLine("END !!!");
            var recordPath = Path.Combine(AppContext.BaseDirectory, "Log", $"log_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt");
            if (!Directory.Exists(Path.GetDirectoryName(recordPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(recordPath));
            }
            await Task.Delay(60 * 1000);
        }

        public static void SendMailResult(string content, List<string> attachment = null)
        {
            var mail = new SendMail("oa.admin@reginamiracle.com", "Mis2018it");
            var listMailFile = Path.Combine(AppContext.BaseDirectory, "Parameter", "ListMailReceiver.txt");
            var listMail = File.ReadAllLines(listMailFile).ToList();
            Console.WriteLine("Send email ...");
            mail.BasicEmail(listMail, "BIReport", "", content, null, attachment);
            //var mailHelper = HttpClientHelper.HttpSendMail(listMail, "BIReport", "BIReport", content, attachment);

        }
    }
}
