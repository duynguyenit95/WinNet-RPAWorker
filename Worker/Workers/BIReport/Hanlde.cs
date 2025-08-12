using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom;
using System.Windows.Input;
using BIReportContext;
using static BIReportContext.SAPContext;
using SharpCifs.Util.Sharpen;
using BIReport_ORPContext;
using System.Data.Entity;
using static BIReport_ORPContext.ORPContext;
using RPA.Tools;
using System.Diagnostics;

namespace BIReport
{
    public class Handle
    {
        public class PODetail
        {
            //[Index(2)]
            [Name("PO Code")]
            public string POCode { get; set; }
            [Name("Sewing Actual")]
            public string SewingActual { get; set; }
            [Name("Packing Actual")]
            public string PackingActual { get; set; }
            [Name("Sewing Completion Rate%")]
            public string SewingCompletionRate { get; set; }
        }
        public class ReadPOResult
        {
            public string PO { get; set; }
            public int SewingActual { get; set; }
            public int PackingActual { get; set; }
            public int SewingRate { get; set; }
            public int DifferentPackingSewing { get; set; }
        }
        public class SAPData
        {
            public string PO { get; set; }
            public string ZDCode { get; set; }
            public string COLOR_NO { get; set; }
        }
        public class BIReportDataUpload
        {
            public string DataType { get; set; } = "Sewing Actual";
            public string ManagementFactoryCode { get; set; } = "00KO";
            public string BranchFactoryCode { get; set; } = "00GW";
            public string Workline { get; set; }
            public string PO { get; set; }
            public string MO { get; set; }
            public string ColorCode { get; set; }
            public string ActualDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
            public int DifferentPackingSewing { get; set; }
        }
        public async Task<string> AutoDownloadFile(IPage page, string fromDate, string toDate) 
        {
            await page.GotoAsync("https://spl.fastretailing.com/app/spl/login");

            Console.WriteLine("Redriect to fastretailing website...");

            var requestLogin = page.Locator("app-login");

            if (await requestLogin.IsVisibleAsync())
            {
                Console.WriteLine("Logging...");

                await page.GetByPlaceholder("Account").ClickAsync();

                await page.GetByPlaceholder("Account").FillAsync("");

                await page.GetByPlaceholder("Password").ClickAsync();

                await page.GetByPlaceholder("Password").FillAsync("");

                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Login" }).ClickAsync();
            }

            await page.Locator(".portal-article").WaitForAsync();

            await page.GotoAsync("https://spl.fastretailing.com/app/spl/tableaureport/actualanalysisdashboard");

            var frameLocator = page.FrameLocator("#tableauReportIframe");

            await frameLocator.Locator("#tab-dashboard-region").WaitForAsync();

            await frameLocator.GetByRole(AriaRole.Tab).Locator("span[value=\"Production Detail \"]").WaitForAsync();

            await frameLocator.GetByRole(AriaRole.Tab).Locator("span[value=\"Production Detail \"]").ClickAsync();

            await frameLocator.Locator("#tab-dashboard-region").GetByText("Production Detail").WaitForAsync();

            await frameLocator.GetByRole(AriaRole.Radio, new FrameLocatorGetByRoleOptions() { Name = "Select range for ETD below:" }).GetByRole(AriaRole.Radio).CheckAsync();

            await Task.Delay(15000);

            await frameLocator.GetByLabel("FROM", new FrameLocatorGetByLabelOptions() { Exact = true }).ClickAsync();

            await frameLocator.GetByLabel("FROM", new FrameLocatorGetByLabelOptions() { Exact = true }).FillAsync(fromDate);

            await frameLocator.GetByLabel("TO", new FrameLocatorGetByLabelOptions() { Exact = true }).ClickAsync();

            await Task.Delay(15000);

            await frameLocator.GetByLabel("TO", new FrameLocatorGetByLabelOptions() { Exact = true }).FillAsync(toDate);

            await frameLocator.GetByLabel("TO", new FrameLocatorGetByLabelOptions() { Exact = true }).PressAsync("Enter");


            await Task.Delay(15000);

            await frameLocator.GetByRole(AriaRole.Button, new FrameLocatorGetByRoleOptions() { Name = "Download" }).ClickAsync();

            await frameLocator.GetByRole(AriaRole.Menuitem, new FrameLocatorGetByRoleOptions() { Name = "Crosstab" }).Locator("div").First.ClickAsync();

            await frameLocator.Locator("label").Filter(new LocatorFilterOptions() { HasText = "CSV" }).ClickAsync();

            await frameLocator.GetByText("Download-PO Detail Crosstable").ClickAsync();

            Console.WriteLine("Downloading...");

            var download = await page.RunAndWaitForDownloadAsync(async () =>
            {
                await frameLocator.GetByLabel("Download Crosstab").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions() { Name = "Download" }).ClickAsync();

            });

            var savePath = Path.Combine(AppContext.BaseDirectory, "downloadFile", $"DownloadPO_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");

            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }

            await download.SaveAsAsync(savePath);

            Console.WriteLine("Download Successfully");

            Console.WriteLine($"Download File Path: {savePath}");

            return savePath;
        }

        public List<ReadPOResult> ReadPOData(string filePath)
        {
            var result = new List<ReadPOResult>();
            using (var reader = new StreamReader(filePath))
            {
                Console.WriteLine("Reading Download File");
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "\t",
                    HasHeaderRecord = true
                };

                using (var csv = new CsvReader(reader, csvConfig))
                {
                    var record = csv.GetRecords<PODetail>().ToList();
                    result = record.Select(x => new ReadPOResult
                    {
                        PO = x.POCode,
                        SewingActual = Int32.Parse(x.SewingActual, NumberStyles.AllowThousands),
                        PackingActual = Int32.Parse(x.PackingActual, NumberStyles.AllowThousands),
                        SewingRate = Int32.Parse(x.SewingCompletionRate.Replace("%", "")),
                        DifferentPackingSewing = Int32.Parse(x.PackingActual, NumberStyles.AllowThousands) - Int32.Parse(x.SewingActual, NumberStyles.AllowThousands)
                    }).ToList();
                }
            }
            var res = result.Where(x => /*x.SewingRate > 105 &&*/ x.DifferentPackingSewing > 0).ToList();
            Console.WriteLine($"Filtered data: {res.Count} rows");
            return res;

        }
        public async Task<List<BIReportDataUpload>> CollectData(List<ReadPOResult> POResults)
        {
            Console.WriteLine("Collecting data from SAP & ETS");
            var MOLines = new List<T_C_MODailyOutput_Workline>();
            using (var db = new ORPContext())
            {
                MOLines = db.T_C_MODailyOutput_Worklines
                    .Where(x => x.GxNo == 698)
                    .ToList();
            }

            using (var sapdb = new SAPContext())
            {
                var listPO = POResults.Select(x => x.PO).Distinct().ToList();
                var infoTable = sapdb.T_R_SOInfors
                    .ToList()
                    .Where(x => listPO.Contains(x.POCode))
                    .ToList();
                var collectData = (from ta in infoTable
                                   join tb in sapdb.T_R_SOLinks on new { SO = ta.SO, SOItemNo = ta.SOItemNo } equals new { SO = tb.SO, SOItemNo = tb.SOItemNo }
                                   join tc in sapdb.T_R_SOInfors on new { BigSO = tb.BigSO, BigSOItemNo = tb.BigSOItemNo } equals new { BigSO = tc.SO, BigSOItemNo = tc.SOItemNo }
                                   join td in sapdb.T_R_SOMODatas on new { SO = tb.BigSO, SOItemNo = tb.BigSOItemNo, StyleNo = ta.StyleNo } equals new { SO = td.SO, SOItemNo = td.SOItemNo, StyleNo = td.StyleNo }
                                   join mo in MOLines on new { ZDCode = td.MO, COLOR_NO = ta.Color } equals new { ZDCode = mo.ZDCode, COLOR_NO = mo.COLOR_NO } into joindt
                                   from j in joindt.DefaultIfEmpty()
                                   select new
                                   {
                                       PO = ta.POCode,
                                       ZDCode = td.MO,
                                       COLOR_NO = ta.Color,
                                       CustomerColor = ta.FLEXCOLOR,
                                       Workline = j is null ? null : j.Workline,
                                   }).ToList();
                var result = new List<BIReportDataUpload>();
                foreach (var items in POResults)
                {
                    var res = new BIReportDataUpload();
                    res.PO = items.PO;
                    res.DifferentPackingSewing = items.DifferentPackingSewing;
                    var data = collectData.Where(x => x.PO == res.PO).FirstOrDefault();
                    if (data == null) continue;
                    res.Workline = data.Workline;
                    res.MO = data.ZDCode;
                    res.ColorCode = data.CustomerColor.Substring(0,2);
                    result.Add(res);
                }
                Console.WriteLine($"Data collected: {result.Count} rows");
                return result;
            }
        }
    }
}
