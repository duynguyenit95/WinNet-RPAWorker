using System.Threading.Tasks;
using System;
using System.IO;
using RPA.Tools;
using FlaUI.Core.AutomationElements;
using System.Linq;
using System.Drawing;
using FlaUI.Core.Definitions;
using System.Collections.Generic;
using FlaUI.UIA3;
using System.Diagnostics;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using BIReport.Lib;
using static BIReport.Lib.BIContext;
using FlaUI.Core.Capturing;

namespace UQUploadTool
{
    public class Program
    {

        public static string pendingFolderPath = @"E:\\BIReportAutomation\\Pending\";
        public static string uploadedFolderPath = @"E:\\BIReportAutomation\\Uploaded\";
        public static string waitForSyncFolderPath = @"\\172.17.29.104\Archive\";
        //public static class ProcessStatus
        //{
        //    public const string Uploaded = "Uploaded";
        //    public const string Success = "Success";
        //    public const string Fail = "Fail";
        //}
        public class ProcessResult
        {
            public string Result { get; set; } = string.Empty;
            public ActionLog Log { get; set; } = new ActionLog();
            public List<string> Files { get; set; } = new List<string>();
            public string SewingPath { get; set; } = string.Empty;
            public string ZipFilePath { get; set; } = string.Empty;
            public string ZipFileName { get; set; } = string.Empty;
            public string UploadedFile { get; set; } = string.Empty;
        }
        public class ActionLog
        {
            public List<string> Message { get; set; } = new List<string>();
            public void Add(string msg)
            {
                Message.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}: {msg}");
            }
        }
        static async Task Main()
        {
#if DEBUG
            await Test();
#else
            Console.WriteLine("Pilot 2: Upload file via UQ Upload tool");
            await UQUploadTool();
#endif
        }
        static async Task Test()
        {
            try
            {
                Process process = Process.GetProcessesByName("UQ_Output_Upload").FirstOrDefault();
                if (process != null)
                {
                    DateTime timeSpan = DateTime.Now;
                    var app = FlaUI.Core.Application.Attach(process);
                    using (var automation = new UIA3Automation())
                    {
                        var rootElement = automation.GetDesktop();
                        Window window = rootElement.FindFirstChild(x => x.ByProcessId(app.ProcessId)).AsWindow();
                        //window.FindFirstChild(x => x.ByAutomationId("TitleBar")).Click(true);
                        var mainPanel = window.FindFirstChild(x => x.ByClassName("TPanel"));

                        var topPanel = mainPanel.FindFirstChild(x => x.ByClassName("TPanel"));
                        var uploadBtn = topPanel.FindFirstChild(x => x.ByClassName("TBitBtn"));
                        //uploadBtn.Click();
                        var historyPanel = mainPanel.FindFirstChild(x => x.ByClassName("TRichEdit")).AsTextBox();
                        

                        //var scrollVertical = historyPanel.FindFirstChild(x => x.ByAutomationId("NonClientVerticalScrollBar"));
                        //var scrollBarThumb = scrollVertical.FindFirstChild(x => x.ByAutomationId("ScrollbarThumb"));
                        //var lineDownBtn = scrollVertical.FindFirstChild(x => x.ByAutomationId("DownButton"));
                        ////Scroll down
                        //while (lineDownBtn.BoundingRectangle.Y > scrollBarThumb.BoundingRectangle.Y + scrollBarThumb.BoundingRectangle.Height)
                        //{
                        //    lineDownBtn.AsButton().Click();
                        //}

                        var listHistory = new List<string>();
                        //Check result
                        int elapsed = 0;
                        Console.WriteLine($"Wait and read log history");
                        do
                        {
                            await Task.Delay(3000);
                            listHistory = ReadPanelHistories(historyPanel.Name, timeSpan);
                            elapsed += 1;
                            if (listHistory.Any(x => x.Contains("Fail")))
                            {
                                break;
                            }
                            if (listHistory.Any(x => x.StartsWith("All Files Uploaded Success")))
                            {
                                var sewingActualPathKey = "SewingActual Path: ";
                                var sewingActualPathLog = listHistory.Where(x => x.StartsWith(sewingActualPathKey)).FirstOrDefault();
                                if (sewingActualPathLog == null)
                                {
                                    break;
                                }
                                var sewingActualPath = sewingActualPathLog.Replace(sewingActualPathKey, "");
                                Console.WriteLine(sewingActualPath);

                                var zipFilePathKey = "Zip file success ";
                                var zipFilePathLog = listHistory.Where(x => x.StartsWith(zipFilePathKey)).FirstOrDefault();
                                if (zipFilePathLog == null)
                                {
                                    break;
                                }
                                var zipFilePath = zipFilePathLog.Replace(zipFilePathKey, "");
                                Console.WriteLine($"ZipFile Path: {zipFilePath}");

                                var zipFileName = zipFilePath.Substring(zipFilePath.Length - 32);
                                //Path.GetFileName(zipFilePath); 
                                // zipFilePath.Split("\\").LastOrDefault();
                                Console.WriteLine($"ZipFileName: {zipFileName}");
                                break;
                            }
                            if (elapsed > 10)
                            {
                                break;
                            }

                        } while (1 == 1);
                    }
                        
                }
                else
                {
                    Console.WriteLine("Not Found UQ Upload tool");
                    await Task.Delay(5000);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void CaptureError()
        {
            try
            {
                Process process = Process.GetProcessesByName("UQ_Output_Upload_20230414").FirstOrDefault();
                if (process == null)
                {
                    Console.WriteLine("Not Found UQ Upload tool");
                    return;
                }
                var app = FlaUI.Core.Application.Attach(process);
                using (var automation = new UIA3Automation())
                {
                    var rootElement = automation.GetDesktop();
                    FlaUI.Core.AutomationElements.Window window = rootElement.FindFirstChild(x => x.ByProcessId(app.ProcessId)).AsWindow();

                    // Ensure the window is not minimized
                    window.Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Normal);
                    // Capture the window
                    var bitmap = window.Capture();
                    
                    var screenshotPath = Path.Combine(AppContext.BaseDirectory, "Screenshot", $"Screenshot_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.png");
                    if (!Directory.Exists(Path.GetDirectoryName(screenshotPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath));
                    }
                    bitmap.Save(screenshotPath);
                    //Open image
                    Launch(screenshotPath);
                }
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static void Launch(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo(path));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static async Task UQUploadTool()
        {
            Console.WriteLine("START !!!");
            var processResult = new ProcessResult();
            processResult.Log.Add("START PROCESS");
            //Check new file
            try
            {
                var files = new DirectoryInfo(pendingFolderPath).GetFiles("*.xlsx").ToList();
                Console.WriteLine($"New files: {files.Count}");
                processResult.Log.Add($"New files: {files.Count}");
                if (files.Count == 0 )
                {
                    Console.WriteLine("Not found any new files. END !!!");
                    return;
                }
                var file = files.OrderByDescending(x => x.CreationTime).FirstOrDefault();
                processResult.Log.Add($"Process file {file.FullName}");
                Console.WriteLine($"Process file {file.FullName}");
                //Attach
                Process process = Process.GetProcessesByName("UQ_Output_Upload_20230414").FirstOrDefault();
                if (process == null)
                {
                    Console.WriteLine("Not Found UQ Upload tool");
                    processResult.Log.Add("Not Found UQ Upload tool");
                    var ipAddress = Helper.GetIPAddress();
                    SendMailResult($"Pilot2: Not Found UQ Upload tool, please access {ipAddress} and start application manually");
                    return;
                }
                var app = FlaUI.Core.Application.Attach(process);
                //Launch tool
                //var UQToolInfoPath = Path.Combine(AppContext.BaseDirectory, "Parameter", "UQpath.txt");
                //var UQToolPath = File.ReadAllText(UQToolInfoPath);
                //Console.WriteLine($"Launch tool: {UQToolPath}");
                //processResult.Log.Add($"Launch tool: {UQToolPath}");
                //var app = FlaUI.Core.Application.Launch(new ProcessStartInfo()
                //{
                //    FileName = UQToolPath,
                //});

                await Task.Delay(3000);
                using (var automation = new UIA3Automation())
                {
                    var rootElement = automation.GetDesktop();
                    //var children = rootElement.FindAllChildren();
                    //Console.WriteLine("Minimizing all windows");
                    //processResult.Log.Add("Minimizing all windows");
                    //foreach (var child in children)
                    //{
                    //    try
                    //    {
                    //        if (child.ControlType == ControlType.Window && child.AsWindow().Patterns.Window.Pattern.WindowVisualState != WindowVisualState.Minimized)
                    //        {
                    //            child.AsWindow().Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Minimized);
                    //            Console.WriteLine($"{child.Name}");
                    //            processResult.Log.Add($"{child.Name}: Minimized");
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Console.WriteLine($"{child.Name}: {ex.Message}");
                    //        processResult.Log.Add($"{child.Name}: {ex.Message}");
                    //    }

                    //}

                    FlaUI.Core.AutomationElements.Window window = rootElement.FindFirstChild(x => x.ByProcessId(app.ProcessId)).AsWindow();
                    window.Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Maximized);

                    Console.WriteLine($"Click on TitleBar");
                    processResult.Log.Add($"Click on TitleBar");
                    window.FindFirstChild(x => x.ByAutomationId("TitleBar")).Click(true);
                    
                    //window.SetForeground();

                    Console.WriteLine($"ProcessId: {app.ProcessId}");
                    processResult.Log.Add($"ProcessId: {app.ProcessId}");
                    //Find file uploader component
                    var mainPanel = window.FindFirstChild(x => x.ByClassName("TPanel"));


                    var topPanel = mainPanel.FindFirstChild(x => x.ByClassName("TPanel"));
                    var fileSelecting = topPanel.FindFirstChild(x => x.ByClassName("TDBEditEh"));

                    //Click small button on the right
                    var bounding = fileSelecting.BoundingRectangle;
                    Point point = new Point(bounding.X + bounding.Width - 10, bounding.Y + bounding.Height / 2);
                    FlaUI.Core.Input.Mouse.LeftClick(point);
                    
                    
                    Console.WriteLine($"Click small button to open explore {point}");
                    processResult.Log.Add($"Click small button to open explore {point}");

                    await Task.Delay(2000);
                    //Reload root
                    window = automation.GetDesktop().FindFirstChild(x => x.ByProcessId(app.ProcessId)).AsWindow();

                    var dialogUpload = window.FindFirstChild(x => x.ByName("Open"));
                    var inputFileName = dialogUpload
                        .FindAllChildren(x => x.ByControlType(ControlType.ComboBox))
                        .FirstOrDefault(x => x.Name == "File name:")
                        .FindFirstChild(x => x.ByControlType(ControlType.Edit)).AsTextBox();
                    await Task.Delay(2000);
                    inputFileName.Text = file.FullName;

                    var openBtn = dialogUpload
                        .FindAllChildren(x => x.ByControlType(ControlType.Button))
                        .FirstOrDefault(x => x.Name == "Open")
                        .AsButton();
                    await Task.Delay(2000);
                    openBtn.Click();
                    await Task.Delay(1000);
                    var uploadBtn = topPanel.FindFirstChild(x => x.ByClassName("TBitBtn"));

                    //return;
                    Console.WriteLine($"Click upload button");
                    processResult.Log.Add($"Click upload button");
                    await Task.Delay(2000);
                    DateTime timeSpan = DateTime.Now;
                    uploadBtn.Click();
                    await Task.Delay(2000);
                    var historyPanel = mainPanel.FindFirstChild(x => x.ByClassName("TRichEdit")).AsTextBox();
                    //---------Scroll down----------------//
                    //var scrollVertical = historyPanel.FindFirstChild(x => x.ByAutomationId("NonClientVerticalScrollBar"));
                    //var lineDownBtn = scrollVertical.FindFirstChild(x => x.ByAutomationId("DownButton"));
                    //var scrollBarThumb = scrollVertical.FindFirstChild(x => x.ByAutomationId("ScrollbarThumb"));
                    //while (lineDownBtn.BoundingRectangle.Y > scrollBarThumb.BoundingRectangle.Y + scrollBarThumb.BoundingRectangle.Height)
                    //{
                    //    lineDownBtn.AsButton().Click();
                    //    await Task.Delay(500);
                    //}


                    var listHistory = new List<string>();
                    //Check result
                    int elapsed = 0;
                    Console.WriteLine($"Wait and read log history");
                    processResult.Log.Add($"Wait and read log history");
                    do
                    {
                        await Task.Delay(3000);
                        listHistory = ReadPanelHistories(historyPanel.Text, timeSpan);
                        elapsed += 1;
                        if (listHistory.Count == 0)
                        {
                            Console.WriteLine("Cannot read notification");
                            continue;
                        }
                        if (listHistory.Any(x => x.Contains("Fail")))
                        {
                            processResult.Result = ProcessStatus.Fail;
                            processResult.Log.Add(ProcessStatus.Fail);
                            break;
                        }
                        if (listHistory.Any(x => x.StartsWith("All Files Uploaded Success")))
                        {
                            processResult.Log.Add("All Files Uploaded Success");
                            var sewingActualPathKey = "SewingActual Path: ";
                            var sewingActualPathLog = listHistory.Where(x => x.StartsWith(sewingActualPathKey)).FirstOrDefault();
                            if (sewingActualPathLog == null)
                            {
                                processResult.Result = "Cannot find SewingActual path";
                                processResult.Log.Add("Cannot find SewingActual path");
                                break;
                            }
                            var sewingActualPath = sewingActualPathLog.Replace(sewingActualPathKey, "");
                            Console.WriteLine(sewingActualPath);
                            processResult.SewingPath = sewingActualPath;//.Split('\\').LastOrDefault();
                            Console.WriteLine($"SewingActual Path: {processResult.SewingPath}");

                            var zipFilePathKey = "Zip file success ";
                            var zipFilePathLog = listHistory.Where(x => x.StartsWith(zipFilePathKey)).FirstOrDefault();
                            if (zipFilePathLog == null)
                            {
                                processResult.Result = "Cannot find zip file path";
                                processResult.Log.Add("Cannot find zip file path");
                                break;
                            }
                            var zipFilePath = zipFilePathLog.Replace(zipFilePathKey, "");
                            Console.WriteLine($"ZipFile Path: {zipFilePath}");

                            var zipFileName = zipFilePath.Substring(zipFilePath.Length - 32);
                                //Path.GetFileName(zipFilePath); 
                                // zipFilePath.Split("\\").LastOrDefault();
                            Console.WriteLine($"ZipFileName: {zipFileName}");

                            processResult.ZipFilePath = zipFilePath;
                            processResult.ZipFileName = zipFileName;
                            processResult.Result = ProcessStatus.Success;
                            break;
                        }
                        if (elapsed > 10)
                        {
                            processResult.Result = "Timeout";
                            break;
                        }

                    } while (1 == 1);
                    Console.WriteLine($"Result: {processResult.Result}");
                    
                    var logHistoryFile = Path.Combine(AppContext.BaseDirectory, "uploadUQLog", $"UploadLog_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt");
                    if (!Directory.Exists(Path.GetDirectoryName(logHistoryFile)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(logHistoryFile));
                    }

                    File.WriteAllLines(logHistoryFile, ReadPanelHistories(historyPanel.Text, timeSpan, true));
                    processResult.Files.Add(logHistoryFile);
                    if (processResult.Result == ProcessStatus.Success)
                    {
                        await SaveToDB(processResult);
                    }
                    //app.Close();
                    processResult.UploadedFile = Path.Combine(uploadedFolderPath, file.Name);
                    if (File.Exists(processResult.UploadedFile))
                    {
                        File.Delete(processResult.UploadedFile);
                    }
                    file.MoveTo(processResult.UploadedFile);
                    processResult.Files.Add(processResult.UploadedFile);
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"{ex.Message}");
                processResult.Log.Add($"{ex.Message}");
                processResult.Result = ProcessStatus.Fail;
                SendMailResult($"{ex.Message}");
            }
            Console.WriteLine("End of robot session !");
            Console.WriteLine("Record log... !");
            var recordPath = Path.Combine(AppContext.BaseDirectory, "Log", $"log_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt");
            if (!Directory.Exists(Path.GetDirectoryName(recordPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(recordPath));
            }
            File.WriteAllLines(recordPath, processResult.Log.Message);
            processResult.Files.Add(recordPath);
            Console.WriteLine($"File log: {recordPath}");

            var emailMessage = $"Pilot2: Upload Report" +
                $"<br>File Uploaded: {processResult.UploadedFile}" +
                $"<br>Result: <b>{processResult.Result}</b>";
            if (processResult.Result == ProcessStatus.Success)
            {
                emailMessage += $"<br>Sewing Actual Path: {processResult.SewingPath}" +
                $"<br>Zip File Success Path: {processResult.ZipFilePath}" +
                $"<br>Zip File Name: {processResult.ZipFileName}" +
                $"<br>Wait for pilot 3: Read file archive !!!";
            }

            emailMessage +=  $"<br>Please check the UploadLog & ProcessLog in attachment to see detail";

            SendMailResult(emailMessage, processResult.Files);
            Console.WriteLine("END !!!");

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
        public static List<string> ReadPanelHistories(string historyPanel, DateTime timeStamp, bool keepTimeSpan = false)
        {
            var result = new List<string>();
            var list = historyPanel.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var item in list)
            {
                var data = item.Split(new[] { "      " }, StringSplitOptions.None).ToList();
                var timeString = data.FirstOrDefault();
                DateTime correctTime = DateTime.TryParseExact(timeString, new[] { "yyyy-MM-dd HH:mm ss" }, null, System.Globalization.DateTimeStyles.None, out DateTime timeParse) ? timeParse : DateTime.MinValue;
                Console.WriteLine(correctTime);
                Console.WriteLine(timeStamp);
                if (correctTime < timeStamp) continue;
                var log = data.LastOrDefault();
                if (string.IsNullOrEmpty(log)) continue;
                if (keepTimeSpan) result.Add(item);
                else result.Add(log);
            }
            return result;
        }

        public static async Task<bool> SaveToDB(ProcessResult processResult)
        {
            
            var input = new BIReport_UQUploadResult();
            using (var db = new BIContext())
            {
                input.SewingActualPath = processResult.SewingPath;
                input.ZipFileName = processResult.ZipFileName;
                input.Status = ProcessStatus.Uploaded;
                input.UpdateTime = DateTime.Now;
                input.UploadTime = DateTime.Now;
                db.BIReport_UQUploadResults.Add(input);
                await db.SaveChangesAsync();
            }
            return true;
        }

    }
}
