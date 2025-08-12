using System.Linq;
using System.Threading.Tasks;
using RPA.Core;
using RPA.Worker.Core;
using SCCWorker;
using Microsoft.Playwright;
using System;
using SCCWorker.SCCServices;
using SCC.Lib.Operation;
using SharpCifs.Smb;
using System.Drawing.Imaging;
using System.IO;
using RPA.Tools;
using SCCWorker.Model;
using System.Collections.Generic;
using OfficeOpenXml;
using System.Threading;

namespace SCC
{
    public class BaseWorker : TimerWorker<SCCWorkerOption>
    {
        private readonly SCCServices _services;

        private IPlaywright playwright;

        private IBrowser browser;

        private IPage page;

        public BaseWorker(ServerOption options) : base(options)
        {
            _services = new SCCServices();
        }
        public static void SendMailResult(string content, List<string> capture = null)
        {
            var mail = new SendMail("oa.admin@reginamiracle.com", "Mis2018it");
            var listMailFile = Path.Combine(AppContext.BaseDirectory, "Parameter", "ListMailReceiver.txt");
            var listMail = File.ReadAllLines(listMailFile).ToList();
            mail.BasicEmail(listMail, "SCC System", "", content, null, capture);
        }
        public override SCCWorkerOption LoadWorkerDefaultOption()
        {
            return new SCCWorkerOption()
            {
                NSFOpt = new NSFOptions()
                {
                    InputFolderURI = "",
                },
                ORPConnetion = "",
                UserName = "",
                Password = "",
            };
        }
        public async override Task<WorkerExecutionContext<SCCWorkerOption>> PreExecute(WorkerExecutionContext<SCCWorkerOption> context)
        {
            playwright = await Playwright.CreateAsync();

            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
            });
            var PWcontext = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize() { Width = 1800, Height = 900 }
            });

            page = await PWcontext.NewPageAsync();

            return await base.PreExecute(context);
        }

      

        public override async Task<WorkerExecutionContext<SCCWorkerOption>> ExecutionHandler(WorkerExecutionContext<SCCWorkerOption> context)
        {
            List<string> captureScreenList = new List<string>();

            int orderId = Int32.Parse(context.JsonData);

            var handle = new Handle(page);

            //Login
            if (!await handle.Login(WorkerOption.UserName, WorkerOption.Password))
            {
                //_services.SaveLog(orderId, "Login failed, wrong account or password !!!");

                //_services.SaveLog(orderId, "Exit SCC");

                captureScreenList.Add(await handle.Screenshot());

                SendMailResult("Đăng nhập thất bại, sai tài khoản hoặc mật khẩu<br/>Login failed, wrong account or password ", captureScreenList);

                return await base.ExecutionHandler(context);
            }

            //_services.SaveLog(orderId, "Login successfully !!!");

            await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions() { Name = "HZERO Dev Platform" }).WaitForAsync(new LocatorWaitForOptions{State = WaitForSelectorState.Visible, Timeout = 5 * 60 *1000});

            var auth = new NtlmPasswordAuthentication(context.WorkerOptions.NSFOpt.AuthDomain, context.WorkerOptions.NSFOpt.AuthUsername, context.WorkerOptions.NSFOpt.AuthPassword);
            //var pilot2 = new Pilot2Worker(page, auth);
            //context.IsSuccess = await pilot2.ExecutionHandler(74);

            var order = await _services.GetOrder(orderId);

            var sccProcessLogPath = Path.Combine(AppContext.BaseDirectory, "SCCLog");
            if (!Directory.Exists(sccProcessLogPath))
            {
                Directory.CreateDirectory(sccProcessLogPath);
            }
            switch (order?.Pilot)
            {
                case 1:
                    var pilot1 = new Pilot1Worker(page);
                    context.IsSuccess = await pilot1.ExecutionHandler(orderId);
                    break;
                case 2:
                    var pilot2 = new Pilot2Worker(page, auth);
                    var result = await pilot2.ExecutionHandler(orderId);
                    var logFile = Path.Combine(sccProcessLogPath, $"SCCLog_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt");
                    File.WriteAllLines(logFile, result.Item3);
                    captureScreenList.Insert(0, logFile);
                    captureScreenList.AddRange(result.Item2);
                    SendMailResult(result.Item1, captureScreenList);
                    break;
                default:
                    break;
            }
            return await base.ExecutionHandler(context);
        }

        public override async Task<WorkerExecutionContext<SCCWorkerOption>> AfterExecute(WorkerExecutionContext<SCCWorkerOption> context)
        {
            context.Log("Close Brower");

            await browser.DisposeAsync();

            playwright.Dispose();

            SmbExtensions.DeleteAllFilesLocal(LocalProcessFolder);

            return await base.AfterExecute(context);
        }

    }
}
