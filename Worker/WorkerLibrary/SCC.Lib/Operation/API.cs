using Microsoft.Playwright;
using SCCWorker.Model;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SCC.Lib.Operation
{
    public class API
    {
        private readonly IPage page;
        private readonly string _PO;
        private SCC_DOInfo _DOInfo = new SCC_DOInfo();
        private List<SCC_DODetail> _DODetails = new List<SCC_DODetail>();
        private bool _checkRequest = false;
        private bool _getDOCode = false;
        private bool _getTotalLines = false;
        private bool _getDOLines = false;
        public API(IPage page, string PO)
        {
            this.page = page;
            this._PO = PO;
        }
        public async Task<SCC_DOInfo> CollectDOInfo()
        {
            //Check request

            page.Request += HandleRequest;

            await page.GotoAsync("https://www.onestep-cloud.com/so/demand-order/list");

            while (!_checkRequest)
            {
                await Task.Delay(500);
            }

            page.Request -= HandleRequest;

            //Get DOInfo

            await page.Locator("div").Filter(new LocatorFilterOptions { HasTextRegex = new Regex("^SAP-PO$") }).First.ClickAsync();

            await page.Locator("input[name=\"attributeString2\"]").First.FillAsync(_PO);

            page.Response += HandleDOInfo;

            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Query" }).ClickAsync();

            while (!_getDOCode)
            {
                await Task.Delay(100);
            }

            page.Response -= HandleDOInfo;

            return _DOInfo; 
        }

        public async Task<List<SCC_DODetail>> CollectDODetails()
        {
            page.Response += HandleDODetail;

            while (!_getTotalLines)
            {
                await Task.Delay(100);
            }

            for (int i = 1; i <= 10; i++)
            {
                await page.Mouse.WheelAsync(0, i * 100);
            }

            var handle = new Handle(page);

            await handle.ChangeLinesPerPage(_DOInfo.TotalElements.ToString());

            //await page.GetByRole(AriaRole.Navigation).Locator("label i").ClickAsync();

            //await page.GetByRole(AriaRole.Menuitem, new PageGetByRoleOptions() { Name = "100" }).ClickAsync();

            while (!_getDOLines)
            {
                await Task.Delay(100);
            }

            page.Response -= HandleDODetail;

            return _DODetails;
        }



        private void HandleRequest(object sender, Microsoft.Playwright.IRequest request)
        {
            if (request.Url.Contains("https://zone-hw-api.onestep-cloud.com/zoso/v1/150/dos"))
            {
                //headerRequest = request.Headers;
                _checkRequest = true;
            }
        }

        private async void HandleDOInfo(object sender, IResponse response)
        {
            if (response.Url.Contains("https://zone-hw-api.onestep-cloud.com/zoso/v1/150/dos")
                && response.Url.Contains($"attributeString2={_PO}"))
            {
                var responseData = await response.TextAsync();
                var jsonNode = JsonNode.Parse(responseData);
                var content = jsonNode["content"].AsArray();
                try
                {
                    if (content.Count != 1)
                    {

                        //context.Log($"{_inputPO} has {content.Count} DO ===>>> Cancel process!!!");
                    }
                    else
                    {
                        _DOInfo = new SCC_DOInfo
                        {
                            DOId = content[0]["doId"].ToString(),
                            DOCode = content[0]["doCode"].ToString(),
                            DOApprovalStatus = content[0]["doApprovalStatus"].ToString(),
                            DOCloseStatus = content[0]["doCloseStatus"].ToString(),
                            DOCooperateStatus= content[0]["doCooperateStatus"].ToString(),
                        };
                        _getDOCode = true;
                    }
                }
                catch
                {

                }
            }
        }

        private async void HandleDODetail(object sender, IResponse response)
        {
            if (response.Url.Contains("https://zone-hw-api.onestep-cloud.com/zoso/v1/150/do-lines") && response.Url.Contains($"doId={_DOInfo.DOId}") && !_getDOLines)
            {
                var content = await response.TextAsync();
                try
                {
                    var jsonNode = JsonNode.Parse(content);
                    var numberOfElement = (int)Convert.ToInt32(jsonNode["numberOfElements"].ToString());
                    var totalElement = (int)Convert.ToInt32(jsonNode["totalElements"].ToString());
                    var totalPages = (int)Convert.ToInt32(jsonNode["totalPages"].ToString());
                    if (!_getTotalLines)
                    {
                        _DOInfo.TotalElements = totalElement;
                        //Log($"Number of lines: {totalElement}");
                        _getTotalLines = true;
                    }
                    if (numberOfElement < totalElement || totalPages != 1) return;
                    var data = jsonNode["content"].AsArray();
                    //_DODetails.Clear();
                    foreach (var dt in data)
                    {
                        _DODetails.Add(new SCC_DODetail
                        {
                            DOId = dt["doId"].ToString(),
                            DOLine = dt["doLineSeq"].ToString(),
                            MerchID = dt["customerItemCode"].ToString(),
                            SKU = dt["customerItemSkuCode"].ToString(),
                            CustomerOrderQty = (int)Convert.ToDouble(dt["customerOrderQty"].ToString()),
                            ConfirmedQty = (int)Convert.ToDouble(dt["confirmedQty"].ToString()),
                            CustomerRequirementDate = Convert.ToDateTime(dt["customerRequirementDate"].ToString()),
                            NDC = Convert.ToDateTime(dt["promiseDate"].ToString())
                        });
                    }
                    _getDOLines = true;
                }
                catch
                {

                }
            }
        }
    }
}
