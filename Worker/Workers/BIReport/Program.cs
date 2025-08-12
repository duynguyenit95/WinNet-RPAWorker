using System.Threading.Tasks;
using System;
using System.IO;
using RPA.Tools;
using FlaUI.UIA3;
using FlaUI.Core.AutomationElements;
using System.Linq;
using System.Drawing;
using FlaUI.Core.Definitions;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Drawing.Imaging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using RPA.Core;
using SharpCifs.Smb;
using Microsoft.Playwright;
using PlayWrightCommonLib;
using SharpCifs.Util.Sharpen;
using System.ComponentModel;
using System.Management.Instrumentation;
using static BIReport.Handle;

namespace BIReport
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public class ActionLog
        {
            public List<string> Message { get; set; } = new List<string>();
            public void Add(string msg)
            {
                Message.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}: {msg}");
            }
        }
        public static async Task SendMailResult(string content, List<string> attachment, bool isError = false)
        {
            var listMail = new string[] { "" };
            var listMailReceiver = Path.Combine(AppContext.BaseDirectory, "Parameter", "ListMailReceiver.txt");
            var listMailError = Path.Combine(AppContext.BaseDirectory, "Parameter", "ListMailError.txt");
            var listMailFile = isError ? listMailError : listMailReceiver;
            if (File.Exists(listMailFile)) listMail = File.ReadAllLines(listMailFile);
            Console.WriteLine("Sending email...");
            var mailProcessResult = await HttpClientHelper.HttpSendMail(listMail, "BIReport", "BIReport", content, attachment);
            log.Add(mailProcessResult.StatusCode.ToString());
        }
        

        public static ActionLog log = new ActionLog();

        
        static async Task Main()
        {
            Console.WriteLine("Start");
            var listFileAttachment = new List<string>();
            var listImageCapture = new List<string>();
            var cert = new BIReport.Handle();
            log.Add("Start !!!");

            var today = DateTime.Now.Date;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var fromDate = firstDayOfMonth.ToString("yyyy-MM-dd");//today.AddDays(-7).ToString("yyyy-MM-dd");
            var toDate = firstDayOfMonth.AddMonths(2).AddDays(-1).ToString("yyyy-MM-dd");//today.ToString("yyyy-MM-dd");

            log.Add($"Searching...");
            log.Add($"FromDate: {fromDate}");
            log.Add($"ToDate: {toDate}");
            var ipAddress = Helper.GetIPAddress();

            var destinationPath = "smb://172.19.18.43/BIReportAutomation/Pending/";
            var fileName = $"BIReport_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx";
            var destinationFile = destinationPath + fileName;
            int count = 0; var path = string.Empty;

            var userChormePath = Path.Combine(System.Environment.GetEnvironmentVariable("USERPROFILE"), "AppData\\Local\\Google\\Chrome\\User Data\\Default");

            var userFireFoxPath = Path.Combine(System.Environment.GetEnvironmentVariable("USERPROFILE"), "AppData\\Local\\Mozilla\\Firefox");

            var playwright = await Playwright.CreateAsync();

            var browser = await playwright.Firefox.LaunchPersistentContextAsync(userFireFoxPath, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = false,
                ScreenSize = new ScreenSize
                {
                    Width = 1920,
                    Height = 1080
                },
                Timeout = 60 * 1000
            });

            var page = await browser.NewPageAsync();

            var extension = new PageExtension(page);

            while (true)
            {
                count++;
                try
                {
                    Console.WriteLine($"Download file: {count} try");
                    log.Add($"Download file: {count} try");
                    path = await cert.AutoDownloadFile(page, fromDate, toDate);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    log.Add(ex.Message);
                    var image = await extension.Screenshot("BIReport_CaptureError");
                    if (image != null)
                    {
                        listImageCapture.Add(image);
                    }
                    if (count < 3) continue;
                    listFileAttachment.AddRange(listImageCapture);
                    await SendMailResult(ex.Message, listFileAttachment, true);
                    return;
                }
            }

            await browser.DisposeAsync();

            playwright.Dispose();

            try
            {
                log.Add($"WorkerIP: {ipAddress}");
                
                
                //var path = "D:\\RPA\\RPA\\Worker\\Workers\\BIReport\\bin\\Debug\\downloadFile\\DownloadPO_20240326_184157.csv";
                log.Add($"Download file successfully: {path}");
                listFileAttachment.Add(path);
                var readData = cert.ReadPOData(path);

                if (readData.Count == 0)
                {
                    log.Add("No data available after filtering conditions");
                    await SendMailResult("No data available after filtering conditions \n Không có dữ liệu sau khi lọc điều kiện", listFileAttachment);
                    return;
                }
                var result = await cert.CollectData(readData);
                if (result.Count == 0)
                {
                    log.Add("No data found on ETS and SAP");
                    await SendMailResult("No data found on ETS and SAP \n Không tìm thấy dữ liệu trên ETS, SAP", listFileAttachment);
                    return;
                }

                var template = Path.Combine(AppContext.BaseDirectory, "Template", "BIReportTemplate.xlsx");


                var savepath = Path.Combine(AppContext.BaseDirectory, "result", $"{fileName}");

                log.Add($"File upload: {savepath}");

                if (!Directory.Exists(Path.GetDirectoryName(savepath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(savepath));
                }
                Console.WriteLine("Preparing BI Report via template");
                Helper.CreateExcelSheetFromTemplate(result, savepath, template, 3);
                listFileAttachment.Add(savepath);
                Console.WriteLine($"Result file {savepath}");

                //--------------Copy file to FR3 ---------------//
                if (false)
                {
                    Console.WriteLine($"Copy result file to FR3 machine");
                    var auth = new NtlmPasswordAuthentication("REGINAMIRACLE", "darius.nguyen", "Duycnt@1234#");
                    Console.WriteLine($"Copying to {destinationFile} ...");
                    SmbExtensions.CopyLocalFileToNSF(auth, savepath, destinationFile);
                    //File.Copy(savepath, destinationFile, true);
                    Console.WriteLine($"Copy success !!");
                }


                //--------------Send Email ---------------//
                await SendMailResult($"Pilot 1: Collect Data Successfully " +
                        $"<br> Worker Machine: {ipAddress}" +
                        //$"<br> File upload store in {destinationFile}" +
                        //$"<br> Wait for pilot 2: Upload file",
                        $"<br> After confirm, please store file on: " +
                        $"<br> 附件为BI系统导出的异常数据，请帮手查阅，若有差异请补充并将准确数据文件放在以下路径，谢谢 " +
                        $"<br> {destinationPath}",
                        listFileAttachment);
            }
            catch (Exception ex)
            {
                log.Add(ex.Message);
                await SendMailResult(ex.Message, new List<string> {"darius.nguyen@reginamiracle.com"});
                Console.WriteLine(ex.Message);
            }
            log.Add("ROBOT STOPPED");
            Console.WriteLine("End of robot session !");
            Console.WriteLine("Record log... !");
            var recordPath = Path.Combine(AppContext.BaseDirectory, "Log", $"log_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt");
            if (!Directory.Exists(Path.GetDirectoryName(recordPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(recordPath));
            }
            File.WriteAllLines(recordPath, log.Message);
            Console.WriteLine($"File log: {recordPath}");
            Console.WriteLine("END !!!");
        }


    }
}
