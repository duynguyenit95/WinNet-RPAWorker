using System.Linq;
using System.Threading.Tasks;
using RPA.Core;
using RPA.Worker.Core;
using SCCWorker;
using Microsoft.Playwright;
using System;
using SCCWorker.SCCServices;
using SCC.Lib.Operation;

namespace SCC
{
    public class Pilot1Worker
    {
        private readonly SCCServices _services;

        private IPage _page;

        public Pilot1Worker(IPage page) 
        {
            _page = page;
            _services = new SCCServices();
        }     

        public async Task<bool> ExecutionHandler(int orderId)
        {

            var handle = new Handle(_page);

            _services.SaveLog(orderId, "Collect order info ");

            var orderInfos = await _services.GetPilot1OrderInfo(orderId);

            var PO = orderInfos.FirstOrDefault().PO;

            _services.SaveLog(orderId, $"PO: {PO}");

            var api = new API(_page, PO);

            var sccInfo = await api.CollectDOInfo();

            _services.SaveLog(orderId, $"DO: {sccInfo.DOCode} / {sccInfo.DOApprovalStatus} / {sccInfo.DOCooperateStatus} / {sccInfo.DOCloseStatus}");

            //if (sccInfo.DOApprovalStatus == "APPROVED" )
            //{
            //    _services.SaveLog(orderId, $"DO: {sccInfo.DOCode} is APPROVED, cannot change info");

            //    _services.SaveLog(orderId, "Exit SCC");

            //    return true;
            //}
            if (sccInfo.DOCloseStatus != "OPEN" || sccInfo.DOCooperateStatus != "CONFIRMED")
            {
                _services.SaveLog(orderId, "Exit SCC");

                return true;
            }

            await _page.GetByText(sccInfo.DOCode).ClickAsync();

            var sccDetails = await api.CollectDODetails();

            _services.SaveLog(orderId, $"{sccInfo.DOCode} has {sccDetails.Count} data rows");

            foreach (var info in orderInfos)
            {
                info.Status = string.Empty;
                var orderDetail = await _services.GetPilot1OrderInfoDetail(info.Guid);
                if (orderDetail.Count == 0)
                {
                    info.Status = "No Order Info Detail";
                }
                //Case SKU null ==> check all SKU with PO and MerchID condition
                else if (info.SKU == string.Empty)
                {
                    var details = sccDetails.Where(x => x.MerchID == info.MerchID && x.CustomerRequirementDate == info.PlannedNDC).ToList();
                    info.SKU = string.Join(",", details.Select(x => x.SKU).Distinct());
                    var updateNDC = orderDetail.FirstOrDefault().NDC;
                    if (details.Count == 0)
                    {
                        info.Status = "No MerchID info on SCC";
                    }
                    else if (info.PlannedQty != sccDetails.Sum(x => x.CustomerOrderQty))
                    {
                        info.Status = "Incorrect Customer Order Qty / ";
                    }
                    else
                    {
                        foreach (var item in details)
                        {
                            info.Status += $"Line({item.DOLine}) - SKU({item.SKU}): ";
                            try
                            {
                                var rowLine = await handle.FilterRow(item.DOLine, item.SKU);
                                await rowLine.Locator("input[name=\"promiseDate\"]").FillAsync($"{updateNDC.Date.ToString("yyyy-MM-dd")}");
                                info.Status += $"UpdatedNDC {updateNDC.ToString("yyyy-MM-dd")} / ";
                            }
                            catch (Exception ex)
                            {
                                info.Status += $"{ex.Message} / ";
                            }
                        }
                    }
                }
                else //Case SKU info is available
                {
                    var details = sccDetails.Where(x => x.MerchID == info.MerchID && x.SKU == info.SKU && x.DOLine == info.Line).FirstOrDefault();
                    if (details == null)
                    {
                        info.Status = "No Data on SCC";
                    }
                    else if (info.PlannedQty != details.CustomerOrderQty)
                    {
                        info.Status = "Incorrect Customer Order Qty";
                    }
                    else if (info.PlannedNDC != details.CustomerRequirementDate)
                    {
                        info.Status = "Incorrect Customer Requirement Date";
                    }
                    else
                    {
                        try
                        {
                            //Detected row
                            var row = await handle.FilterRow(info.Line, info.SKU);
                            //If count is 1 => just update NDC and quantity
                            if (orderDetail.Count == 1)
                            {
                                var updateNDC = orderDetail.FirstOrDefault().NDC;
                                await row.Locator("input[name=\"promiseDate\"]").FillAsync($"{updateNDC.Date.ToString("yyyy-MM-dd")}");
                                info.Status += $"UpdatedNDC: {updateNDC.ToString("yyyy-MM-dd")} / ";

                                var updateQty = orderDetail.FirstOrDefault().Quantity;
                                if (updateQty != details.ConfirmedQty)
                                {
                                    await row.Locator("input[name=\"confirmedQty\"]").FillAsync($"{updateQty}");
                                    info.Status += $"UpdatedQty: {updateQty} / ";
                                }
                            }
                            else //If count > 1: split line
                            {
                                //Save change before split line
                                await _page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Save" }).ClickAsync();

                                await row.Locator("td[data-index=\"action\"]").Locator("a").GetByText("Split Line").ClickAsync();

                                var modal = _page.Locator("div[class=\"c7n-pro-modal-content\"]").First;

                                await modal.Locator("input[name=\"splitNum\"]").FillAsync(orderDetail.Count.ToString());

                                await modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions() { Name = "Split Line" }).ClickAsync();

                                var tbody = modal.Locator("tbody").GetByRole(AriaRole.Row);

                                var sortOrder = orderDetail.OrderBy(x => x.NDC).ToList();

                                for (int i = 0; i < sortOrder.Count; i++)
                                {
                                    var line = tbody.Nth(i);

                                    var lineNo = string.Empty;

                                    var qty = sortOrder[i].Quantity;

                                    var ndc = sortOrder[i].NDC.ToString("yyyy-MM-dd");

                                    if (i == 0)
                                    {
                                        lineNo = await line.Locator("td[data-index=\"doLineSeq\"]").Locator("span").TextContentAsync();
                                    }
                                    else
                                    {
                                        lineNo = await line.Locator("input[name=\"doLineSeq\"]").InputValueAsync();
                                    }

                                    info.Status += $"Line({lineNo}) - Quantity({qty}) - NDC({ndc}) / ";

                                    await line.Locator("input[name=\"promiseDate\"]").FillAsync(ndc);

                                    await line.Locator("input[name=\"confirmedQty\"]").FillAsync(qty.ToString());
                                }

                                await modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions() { Name = "Confirm And Save" }).ClickAsync();
                                //await modal.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions() { Name = "Cancel" }).ClickAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            info.Status = ex.Message;
                        }
                    }
                }
                _services.UpdatePilot1OrderInfor(info.Id, info.Status);

                //Save change info process
                await _page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Save" }).ClickAsync();
            }

            return true;
        }


    }
}
