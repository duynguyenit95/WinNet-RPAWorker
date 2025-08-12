using Microsoft.Playwright;
using SCCWorker.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SCC.Lib.Operation
{
    public class APIP2
    {
        private readonly IPage page;
        private readonly string _PO;
        private bool _checkRequest = false;
        private bool _getTotalLines = false;
        private bool _getDOIssue = false;
        private int _totalLinesCount = -1;
        private List<SCC_DOIssue> _DOIssues = new List<SCC_DOIssue>();
        public APIP2(IPage page, string PO)
        {
            this.page = page;
            this._PO = PO;
        }
        public async Task<List<SCC_DOIssue>> CollectDOIssues()
        {
            //Check request

            page.Request += HandleRequest;

            await page.GotoAsync("https://www.onestep-cloud.com/so/do-issue/list");

            while (!_checkRequest)
            {
                await Task.Delay(500);
            }

            page.Request -= HandleRequest;

            //Get DOIssue

            await page.Locator("span[class=\"c7n-pro-table-add-fields\"]").ClickAsync();

            await page.Locator("input[value=\"sapPo\"]").CheckAsync();

            await page.Locator("i[name=\"sapPo\"]").ClickAsync();

            var modal = page.Locator("div[class=\"c7n-pro-modal-content\"]").First;

            await modal.Locator("textarea[name=\"sapPo\"]").FillAsync(_PO);

            await modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions() { Name = "Determine" }).ClickAsync();

            page.Response += HandleCheckTotalLines;

            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Query" }).ClickAsync();

            while (!_getTotalLines)
            {
                await Task.Delay(100);
            }

            page.Response -= HandleCheckTotalLines;

            if(_totalLinesCount == 0) return null;

            page.Response += HandleDOIssue;

            var handle = new Handle(page);

            await handle.ChangeLinesPerPage(_totalLinesCount.ToString());

            while (!_getDOIssue)
            {
                await Task.Delay(100);
            }

            page.Response -= HandleDOIssue;

            return _DOIssues;
        }





        private void HandleRequest(object sender, IRequest request)
        {
            if (request.Url.Contains("https://zone-hw-api.onestep-cloud.com/hpfm/v1/150/lovs/value/batch"))
            {
                //headerRequest = request.Headers;
                _checkRequest = true;
            }
        }

        private async void HandleCheckTotalLines(object sender, IResponse response)
        {
            if (response.Url.Contains("https://zone-hw-api.onestep-cloud.com/hmde/v1/150/scripts/execute/zcus.vs.do_issue_need"))
            {
                var postData = JsonObject.Parse(response.Request.PostData).AsObject();
                if (postData == null) return;
                var postPO = postData["sapPo"].AsArray().First().ToString();
                if (postPO != _PO) return;
                var content = await response.TextAsync();

                var jsonNode = JsonNode.Parse(content);
                var result = jsonNode["result"].AsObject();
                _totalLinesCount = (int)Convert.ToInt32(result["totalElements"].ToString());
                if (!_getTotalLines)
                {
                    _getTotalLines = true;
                }
            }
        }

        private async void HandleDOIssue(object sender, IResponse response)
        {
            if (response.Url.Contains("https://zone-hw-api.onestep-cloud.com/hmde/v1/150/scripts/execute/zcus.vs.do_issue_need"))
            {
                var content = await response.TextAsync();
                try
                {
                    var jsonNode = JsonNode.Parse(content);
                    var data = jsonNode["result"]["content"].AsArray();
                    foreach (var dt in data)
                    {
                        _DOIssues.Add(new SCC_DOIssue
                        {
                            Line = dt["documentLineSeq"].ToString(),
                            MerchID = dt["customerItemCode"].ToString(),
                            SKU = dt["customerItemSkuCode"].ToString(),
                            DemandQty = (int)Convert.ToDouble(dt["customerNeedQty"].ToString()),
                            PromisedDeliveryDate = Convert.ToDateTime(dt["promiseDate"].ToString()),
                            FactoryCode = dt["factoryCode"].ToString()
                        });
                    }
                    _getDOIssue = true;
                }
                catch
                {

                }
            }

        }
    }
}
