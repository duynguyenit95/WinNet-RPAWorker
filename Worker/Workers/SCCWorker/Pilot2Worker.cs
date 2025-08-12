using System.Linq;
using System.Threading.Tasks;
using RPA.Core;
using RPA.Worker.Core;
using SCCWorker;
using Microsoft.Playwright;
using System;
using SCCWorker.SCCServices;
using SCC.Lib.Operation;
using System.Activities.Expressions;
using SCCWorker.Model;
using System.Windows.Documents;
using System.Collections.Generic;
using SharpCifs.Smb;
using System.Runtime.Remoting.Contexts;
using OfficeOpenXml;
using RPA.Tools;
using System.IO;
using SharpCifs.Util.Sharpen;

namespace SCC
{
    public class Pilot2Worker 
    {
        private readonly SCCServices _services;
        private IPage _page;
        private NtlmPasswordAuthentication _auth;

        public Pilot2Worker(IPage page, NtlmPasswordAuthentication authentication)
        {
            _page = page;
            _services = new SCCServices();
            _auth = authentication;
        }
        public class ActionLog
        {
            public List<string> Message { get; set; } = new List<string>();
            public void Add(string msg)
            {
                Message.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}: {msg}");
            }
        }


        public async Task<Tuple<string, List<string>, List<string>>> ExecutionHandler(int orderId)
        {

            var handle = new Handle(_page);

            var captureImages = new List<string>();

            var logs = new ActionLog();

            var orderInfos = await _services.GetPilot2OrderInfo(orderId);

            var PO = orderInfos.FirstOrDefault().PO;

            //_services.SaveLog(orderId, $"PO: {PO}");

            logs.Add($"PO: {PO}");

            var api = new APIP2(_page, PO);

            var DOIssues =  await api.CollectDOIssues();

            captureImages.Add(await handle.Screenshot());

            if (DOIssues == null)
            {
                //_services.SaveLog(orderId, "No DOIssues");
                return Tuple.Create($"PO {PO}<br/> Không có thông tin trên SCC/ No data info on SCC website", captureImages, logs.Message);
            }

            logs.Add("Line/MerchID/SKU/DemandQty/PromisedDeliveryDate/FactoryCode");

            foreach (var issue in DOIssues)
            {
                logs.Add($"{issue.Line}/{issue.MerchID}/{issue.SKU}/{issue.DemandQty}/{issue.PromisedDeliveryDate.ToString("yyyy-MM-dd")}/{issue.FactoryCode}");
            }
            var pickedList = new List<SCC_DOIssue>();
            foreach (var order in orderInfos)
            {
                logs.Add($"SKU: {order.SKU}/Quantity: {order.Quantity}:");
                var items = DOIssues.Where(x => x.SKU == order.SKU && x.DemandQty == order.Quantity).ToList();
                var target = new SCC_DOIssue();
                if (items.Count == 0)
                {
                    //_services.SaveLog(orderId, $"SKU: {order.SKU} no data");
                    logs.Add("Không tồn tại mã SKU với số lượng tương ứng trên SCC. Hủy quá trình tự động / SKU & quantity not available on SCC. Cancel RPA");
                    return Tuple.Create($"PO {PO}<br/>SKU {order.SKU} / Quantity: {order.Quantity}: Không tồn tại mã SKU với số lượng tương ứng trên SCC. Hủy quá trình tự động / SKU & quantity not available on SCC. Cancel RPA", captureImages, logs.Message);
                }
                else if (items.Count > 1)
                {
                    //_services.SaveLog(orderId, $"SKU: {order.SKU} got {items.Count} line.");
                    logs.Add($"Số dòng dữ liệu tương thích/Compatible data line number : {items.Count} ");
                    for (int i = 0; i < items.Count; i++)
                    {
                        //_services.SaveLog(orderId, $"{i + 1}. Line: {items[i].Line} / Promise Delivery Date: {items[i].PromisedDeliveryDate}");
                        logs.Add($"{i + 1}. Line: {items[i].Line} / Promise Delivery Date: {items[i].PromisedDeliveryDate}");
                    }
                    target = items.Aggregate((x, y) => x.PromisedDeliveryDate > y.PromisedDeliveryDate ? x : y);
                }
                else
                {
                    target = items.FirstOrDefault();
                }
                //_services.SaveLog(orderId, $"Chosse Line No: {target.Line}");
                logs.Add($"Chọn line số/Chosse line number: {target.Line}");
                pickedList.Add(target);
                try
                {
                    var row = await handle.FilterRowP2(target.Line, target.SKU);
                    if (row == null)
                    {
                        var mess = "Lỗi: Robot không nhận dạng được dòng. Hủy quá trình tự động/ RPAFail: Robot cannot dectect rows. Cancel RPA";
                        logs.Add(mess);
                        captureImages.Add(await handle.Screenshot(true));
                        return Tuple.Create($"{order.SKU} / {order.Quantity}: {mess}", captureImages, logs.Message);
                    }
                    else
                    {
                        await row.Locator("input[type=checkbox]").CheckAsync();
                        logs.Add("Đã tích chọn/Checked");
                    }
                    
                }
                catch (Exception ex)
                {
                    order.Status = ex.Message;
                    //_services.SaveLog(orderId, $"SKU: {order.SKU}: {ex.Message}");
                    logs.Add($"Lỗi/RPAFail: {ex.Message}");
                    captureImages.Add(await handle.Screenshot(true));
                    return Tuple.Create($"Lỗi / RPAFail: {ex.Message}", captureImages, logs.Message); ;
                }
            }

            captureImages.Add(await handle.Screenshot(true));

            await _page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Ship" }).ClickAsync();

            //Check Means of Transport Type



            var transportType = GetTransportType(PO, pickedList);

            logs.Add($"Loại hình vận chuyển/Transport Type: {transportType}");

            if (transportType == null || transportType == "Fail" || transportType == "")
            {
                return Tuple.Create("Không tìm thấy thông tin loại hình vận chuyển", captureImages, logs.Message);
            }

            //_services.SaveLog(orderId, $"Transport Type:{transportType}");

            await _page.Locator("input[label=\"Means of Transport Type\"]").ClickAsync();

            await _page.GetByRole(AriaRole.Menuitem, new PageGetByRoleOptions() { Name = transportType }).ClickAsync();

            captureImages.Add(await handle.Screenshot(true));

            await _page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Packing List" }).ClickAsync();

            logs.Add("Ấn nút / Click button: 'Packing List'");

            var modal = _page.Locator("div[class=\"c7n-pro-modal-content\"]").First;

            await modal.Locator("thead").First.Locator("th").First.Locator("input[type=checkbox]").CheckAsync();

            await modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions() { Name = "Add" }).ClickAsync();

            captureImages.Add(await handle.Screenshot(true));

            await modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions() { Name = "确认" }).ClickAsync();

            await _page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Submit" }).ClickAsync();

            await Task.Delay(10000);

            //_services.SaveLog(orderId, $"Submitted");

            logs.Add("Đã gửi thông tin / Submitted");

            //await _page.Locator("i").Filter(new LocatorFilterOptions() { HasText = "Return" }).ClickAsync();

            //await _page.Locator("i[class=\"anticon anticon-arrow-left back-btn\"]").ClickAsync();

            //await _page.Locator("div[class=\"c7n-pro-modal-footer\"]").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions() { Name = "Save And Exit" }).ClickAsync();

            return Tuple.Create($"PO {PO}<br/>Đã gửi thông tin / Submitted <br/>PO: {PO}<br/>Số dòng đã chọn / Line checked: {orderInfos.Count}<br/>Tổng số lượng/Total amount: {orderInfos.Select(x=>x.Quantity).Sum()}", captureImages, logs.Message);
        }

        public string GetTransportType(string PO, List<SCC_DOIssue> data)
        {
            //Get file in share Folder
            try
            {
                //var smbDir = new SmbFile("smb://172.19.18.54/robot/", _auth);
                //var shipmentPlanFile = smbDir.ListFiles($"*VS L-brand*.xlsx").First();
                //var LocalProcessFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkingFolder", "Process");
                //var shipmentPlanPath = Path.Combine(LocalProcessFolder, shipmentPlanFile.GetName());
                //SmbExtensions.CopyNSFFileToLocal(_auth, shipmentPlanFile.GetPath(), shipmentPlanPath);
                //var folder = new DirectoryInfo(@"E:\\robot\");
                //var file = folder.GetFiles().Select(x => x.FullName).First();
                //
                var folder = new DirectoryInfo(@"C:\\robot\");
                var file = folder.GetFiles("*VS L-brand*.xlsx").FirstOrDefault().FullName;
                var package = new ExcelPackage(new FileInfo(file));
                var worksheets = package.Workbook.Worksheets;
                //Check sheet name
                var factoryCode = data.FirstOrDefault().FactoryCode;
                var merchID = data.Select(x => x.MerchID).FirstOrDefault();
                var color = merchID.Substring(merchID.Length - 4);
                var worksheet = worksheets.Where(x => factoryCode == "46000003" ? x.Name.StartsWith("D厂") : x.Name == DateTime.Now.Month + "月").FirstOrDefault();
                if (worksheet == null)
                {
                    return null;
                }
                //Get Data and collect transport type
                int rowCount = worksheet.Dimension.End.Row;
                int startRowData = 4;
                var transportResult = String.Empty;
                for (int i = startRowData; i <= rowCount; i++)
                {
                    var dtPO = worksheet.Cells[i, 19].Text.Split('\n').LastOrDefault();
                    if (dtPO != PO) continue;
                    var transportText = worksheet.Cells[i, 31].Text.Split('\n').LastOrDefault();
                    var transport = "Ocean";
                    if (transportText.ToUpper().Contains("AIR"))
                    {
                        transport = "Air";
                    }
                    else if (transportText.ToUpper().Contains("OCEAN"))
                    {
                        transport = "Ocean";
                    }
                    else
                    {
                        return null;
                    }
                    if (transportText == transportResult) continue;
                    //Check color
                    var colorText = worksheet.Cells[i, 20].Text.Split(' ').Where(x => x.Length == 4).ToList();
                    if (colorText.Any(x => x == color))
                    {
                        transportResult = transport;
                    }
                }

                return transportResult;
            }
            catch
            {
                return "Fail";
            }
            
        }
    }
}
