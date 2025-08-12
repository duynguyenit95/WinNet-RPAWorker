using Microsoft.Playwright;
using SCCWorker.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SCC.Lib.Operation
{
    public class Handle
    {
        private readonly IPage page;
        public Handle(IPage page)
        {
            this.page = page;
        }
        public async Task<string> Screenshot(bool fullPage = false)
        {
            try
            {
                var imageCapturePath = Path.Combine(AppContext.BaseDirectory, "ImageCapture");
                if (!Directory.Exists(imageCapturePath))
                {
                    Directory.CreateDirectory(imageCapturePath);
                }
                var imageCaptureName = $"screenshot_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.png";
                var imageFullPath = Path.Combine(imageCapturePath, imageCaptureName);
                await page.ScreenshotAsync(new PageScreenshotOptions()
                {
                    Path = imageFullPath,
                    FullPage = fullPage,
                });
                return imageFullPath;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> Login(string uid, string pass)
        {
            await page.GotoAsync("https://zone-hw-api.onestep-cloud.com/oauth/login");

            await Task.Delay(5000);

            await page.GetByTitle("EN").ClickAsync();

            await Task.Delay(5000);

            await page.GetByPlaceholder("UserName/mail").ClickAsync();

            await page.GetByPlaceholder("UserName/mail").FillAsync(uid);

            await page.GetByPlaceholder("Password").ClickAsync();

            await page.GetByPlaceholder("Password").FillAsync(pass);

            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Login" }).ClickAsync();

            //var wrongNotifyLogin = await page.Locator("#loginFormAccount").GetByText("您输入的账号或密码错误").IsVisibleAsync();

            var wrongNotifyLogin = await page.Locator("#loginFormAccount").GetByText("Account or password is error").IsVisibleAsync();

            if (wrongNotifyLogin)
            {
                return false;
            }

            return true;
        }
        public async Task<bool> ChangeLinesPerPage(string lines)
        {
            var input = page.Locator("nav[class=\"c7n-pro-pagination-wrapper c7n-pro-table-pagination c7n-pro-table-pagination-with-selection-tips c7n-pro-pagination\"]")
                .Locator("span[class=\"c7n-pro-select-wrapper c7n-pro-pagination-size-changer c7n-pro-pagination-size-editable c7n-pro-select-sm c7n-pro-select-suffix-button c7n-pro-select-border\"]")
                .Locator("input[class=\"c7n-pro-select\"]").First;

            if(!await input.IsVisibleAsync())
            {
                return false;
            }

            await input.FillAsync(lines);

            await page.GetByText("Lines per page:").ClickAsync();

            return true;
        }
        public async Task<ILocator> FilterRow(string doline, string SKU)
        {
            ILocator locator = null;
            var elements = await page.GetByRole(AriaRole.Row, new PageGetByRoleOptions() { Name = $"{SKU}" }).AllAsync();
            foreach(var ele in elements)
            {
                var sccLineNo = await ele.Locator("td[data-index=\"doLineSeq\"]").Locator("span").TextContentAsync();
                if (sccLineNo != doline) continue;
                locator = ele;
                break;
            }
            return locator;
        }
        public async Task<ILocator> FilterRowP2(string doline, string SKU)
        {
            ILocator locator = null;
            var elements = await page.GetByRole(AriaRole.Row, new PageGetByRoleOptions() { Name = $"{SKU}" }).AllAsync();
            foreach (var ele in elements)
            {
                var sccLineNo = await ele.Locator("td[data-index=\"documentLineSeq\"]").Locator("span").TextContentAsync();
                if (sccLineNo != doline) continue;
                locator = ele;
                break;
            }
            return locator;
        }
        public async Task<DateTime> GetPlannedNDC(ILocator row)
        {
            var demandDate = await row.Locator("td[data-index=\"doLineSeq\"]").Locator("span").First.TextContentAsync();
            return DateTime.Parse(demandDate).Date;
        }
        public async void SaveProcess()
        {
           await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Save" }).ClickAsync();
        }
        public async Task<bool> UpdateNDC(ILocator row,DateTime NDC)
        {
            await row.Locator("input[name=\"promiseDate\"]").FillAsync($"{NDC.Date.ToString("yyyy-MM-dd")}");
            return true;
        }
        public async Task<bool> UpdateQty(ILocator row, int quantity)
        {
            await row.Locator("input[name=\"confirmedQty\"]").FillAsync($"{quantity}");
            return true;
        }
        public async Task<string> SplitLine(ILocator row, List<T_SCC_Pilot1_OrderInfoDetail> order)
        {
            string result = string.Empty;

            int totalQty = 0;

            await row.Locator("td[data-index=\"action\"]").Locator("a").GetByText("Split Line").ClickAsync();

            var modal = page.Locator("div[class=\"c7n-pro-modal-content\"]").First;

            await modal.Locator("input[name=\"splitNum\"]").FillAsync(order.Count.ToString());

            await modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions() { Name = "Split Line" }).ClickAsync();

            var tbody = modal.Locator("tbody").GetByRole(AriaRole.Row);

            var sortOrder = order.OrderBy(x => x.NDC).ToList();
            
            for (int i = 0; i < sortOrder.Count; i++)
            {
                var line = tbody.Nth(i);

                var lineNo = string.Empty;

                var qty = sortOrder[i].Quantity;

                var ndc = sortOrder[i].NDC.ToString("yyyy-MM-dd");

                totalQty += qty;

                if (i == 0)
                {
                    lineNo = await line.Locator("td[data-index=\"doLineSeq\"]").Locator("span").TextContentAsync();
                }
                else
                {
                    lineNo = await line.Locator("input[name=\"doLineSeq\"]").InputValueAsync();
                }

                result += $"Line({lineNo}) - Quantity({qty}) - NDC({ndc}) / ";

                await line.Locator("input[name=\"confirmedQty\"]").FillAsync(qty.ToString());

                await line.Locator("input[name=\"promiseDate\"]").FillAsync(ndc);                
            }

            //modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions() { Name = "Confirm And Save" }).ClickAsync();
            await modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions() { Name = "Cancel" }).ClickAsync();

            return result;
        }
    }
}
