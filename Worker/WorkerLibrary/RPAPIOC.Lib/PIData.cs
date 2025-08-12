using System;
using System.Collections.Generic;
using System.Linq;

namespace RPAPIOC.Lib
{
    public class PIData
    {

        public PIData()
        {

        }
        public string PINumber { get; set; } = string.Empty;
        public string PO { get; set; } = string.Empty;
        public List<PIOCItem> Items { get; set; } = new List<PIOCItem>();
        public decimal TotalAmount => Items.Sum(x => x.Colors.Sum(y => y.Amount));
        public string Address { get; set; } = string.Empty;
        public string RawContent { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string RootFileName { get; set; } = string.Empty;
    }

    public class PIOCItem
    {
        public string SAPSKUName { get; set; }= string.Empty;
        public string SKUName { get; set; } = string.Empty;
        public List<PIOCColor> Colors { get; set; } = new List<PIOCColor>();
        public decimal TotalQuantity => Colors.Sum(x => x.Quantity);
        public int StartLineIndex { get; set; }
        public int EndLineIndex { get; set; } = -1;
        public int LineCount => EndLineIndex - StartLineIndex + 1;
    }

    public class PIOCColor
    {
        public string SKUColor { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 0m;
        public string UnitNo { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; } = 0m;
        public decimal Amount => UnitPrice * Quantity;
    }

    public class PIOCResult
    {
        public static List<PIOCResult> CompareData(SAPData sapData, PIData piData)
        {
            var result = new List<PIOCResult>();
            foreach (var item in piData.Items)
            {
                var sapItem = sapData.Items.FirstOrDefault(x => x.SKUName.Contains(item.SKUName) || x.TotalQuantity == item.TotalQuantity);
                foreach (var color in item.Colors)
                {
                    var sapColor = sapItem?.Colors.FirstOrDefault(x => x.Quantity == color.Quantity);
                    var record = new PIOCResult()
                    {
                        InputFileName = piData.FileName,
                        PIAddress = piData.Address,
                        SAPFabricNo = sapData.FabricNo,
                        PIFabricNo = piData.RawContent.Contains(sapData.FabricNo),
                        PINumber = piData.PINumber,

                        PITotalAmount = piData.TotalAmount,
                        SAPTotalAmount = sapData.TotalAmount,

                        PIPO = piData.PO,
                        SAPPO = sapData.PO,

                        PISKU = item.SKUName,
                        SAPSKU = sapItem?.SKUName ?? string.Empty,

                        PIColor = color.SKUColor,
                        SAPColor = sapColor?.SKUColor ?? string.Empty,

                        PICurrency = color.Currency.ToUpper(),
                        SAPCurrency = sapColor?.Currency ?? string.Empty,

                        PIQuantity = color.Quantity,
                        SAPQuantity = sapColor?.Quantity ?? 0,

                        PIUnit = color.UnitNo.ToUpper(),
                        SAPUnit = sapColor?.UnitNo ?? string.Empty,

                        PIUnitPrice = color.UnitPrice,
                        SAPUnitPrice = sapColor?.UnitPrice ?? 0,
                        PIAmount = color.Amount,
                        SAPAmount = sapColor?.Amount ?? 0,

                        SAPSKUName = sapItem?.SAPSKUName ?? string.Empty,
                    };
                    result.Add(record);
                }
            }

            foreach (var sapItem in sapData.Items)
            {
                var existedItem = result.Where(x => x.SAPSKU == sapItem.SKUName).ToList();
                if (existedItem.Any())
                {
                    foreach(var sapColor in sapItem.Colors)
                    {
                        var existColor = existedItem.FirstOrDefault(x=>x.PIQuantity == sapColor.Quantity);
                        if(existColor == null)
                        {
                            var record = new PIOCResult()
                            {
                                InputFileName = piData.FileName,
                                PIAddress = piData.Address,
                                SAPFabricNo = sapData.FabricNo,
                                PIFabricNo = piData.RawContent.Contains(sapData.FabricNo),
                                PINumber = piData.PINumber,

                                PITotalAmount = piData.TotalAmount,
                                SAPTotalAmount = sapData.TotalAmount,

                                PIPO = piData.PO,
                                SAPPO = sapData.PO,

                                PISKU = string.Empty,
                                SAPSKU = sapItem.SKUName,

                                PIColor = string.Empty,
                                SAPColor = sapColor.SKUColor,

                                PICurrency = string.Empty,
                                SAPCurrency = sapColor.Currency,

                                PIQuantity = 0,
                                SAPQuantity = sapColor.Quantity,

                                PIUnit = string.Empty,
                                SAPUnit = sapColor.UnitNo,

                                PIUnitPrice = 0,
                                SAPUnitPrice = sapColor.UnitPrice,

                                PIAmount = 0,
                                SAPAmount = sapColor.Amount,

                                SAPSKUName = sapItem?.SAPSKUName ?? string.Empty,
                            };
                            result.Add(record);
                        }
                    }
                }
                else
                {
                    foreach (var sapColor in sapItem.Colors)
                    {
                        var record = new PIOCResult()
                        {
                            InputFileName = piData.FileName,
                            PIAddress = piData.Address,
                            SAPFabricNo = sapData.FabricNo,
                            PIFabricNo = piData.RawContent.Contains(sapData.FabricNo),
                            PINumber = piData.PINumber,

                            PITotalAmount = piData.TotalAmount,
                            SAPTotalAmount = sapData.TotalAmount,

                            PIPO = piData.PO,
                            SAPPO = sapData.PO,

                            PISKU = string.Empty,
                            SAPSKU = sapItem.SKUName,

                            PIColor = string.Empty,
                            SAPColor = sapColor.SKUColor,

                            PICurrency = string.Empty,
                            SAPCurrency = sapColor.Currency,

                            PIQuantity = 0,
                            SAPQuantity = sapColor.Quantity,

                            PIUnit = string.Empty,
                            SAPUnit = sapColor.UnitNo,

                            PIUnitPrice = 0,
                            SAPUnitPrice = sapColor.UnitPrice,

                            PIAmount = 0,
                            SAPAmount = sapColor.Amount,

                            SAPSKUName = sapItem?.SAPSKUName ?? string.Empty,
                        };
                        result.Add(record);
                    }
                }
            }
            return result;
        }

        public DateTime ProcessDate { get; set; } = DateTime.Today;
        public string InputFileName { get; set; } = string.Empty;
        public string PINumber { get; set; } = string.Empty;
        public string PIPO { get; set; } = string.Empty;
        public string SAPPO { get; set; } = string.Empty;
        public string PISKU { get; set; } = string.Empty;
        public string SAPSKU { get; set; } = string.Empty;
        public string PIColor { get; set; } = string.Empty;
        public string SAPColor { get; set; } = string.Empty;
        public decimal PIQuantity { get; set; }
        public decimal SAPQuantity { get; set; }
        public string PIUnit { get; set; } = string.Empty;
        public string SAPUnit { get; set; } = string.Empty;
        public decimal PIUnitPrice { get; set; } = 0m;
        public decimal SAPUnitPrice { get; set; } = 0m;
        public string PICurrency { get; set; } = string.Empty;
        public string SAPCurrency { get; set; } = string.Empty;
        public decimal PIAmount { get; set; } = 0m;
        public decimal SAPAmount { get; set; } = 0m;
        public decimal PITotalAmount { get; set; } = 0m;
        public decimal SAPTotalAmount { get; set; } = 0m;
        
        public string PIAddress { get; set; } = string.Empty;
        public bool PIFabricNo { get; set; } = false;
        public string SAPFabricNo { get; set; } = string.Empty;
        public bool ColorMatch => PIColor == SAPColor;
        public bool QuantityMatch => PIQuantity == SAPQuantity;
        public bool SKUMatch => PISKU == SAPSKU;
        public bool UnitMatch => PIUnit == SAPUnit;
        public bool UnitPriceMatch => PIUnitPrice == SAPUnitPrice;
        public bool CurrencyMatch => PICurrency == SAPCurrency;
        public bool AmountMatch => PIAmount == SAPAmount;
        public bool TotalAmountMatch => PITotalAmount == SAPTotalAmount;
        public bool FabricMatch => PIFabricNo;
        public string SAPSKUName { get; set; }
    }

}
