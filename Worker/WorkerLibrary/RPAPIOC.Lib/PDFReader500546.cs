using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RPAPIOC.Lib
{
    public class PDFReader500546 : IPDFReader
    {
        //private readonly Regex _itemRegex = new Regex("(?'stt'\\d{1}\\)) (?'item'.{0,}) (?'quantity'\\d{0,}) (?'unit'pcs) (?'currency'HKD) (?'unitPrice'\\d{0,3},{0,1}\\d{0,3}\\.\\d+)");
        private readonly Regex _itemRegex = new Regex("(?'item'.{0,}) (?'quantity'\\d{0,}) (?'unit'pcs) (?'currencyPriceAll'(?'currencyPriceA'(?'currency'HKD|HK) (?'unitPrice'\\d{0,3},{0,1}\\d{0,3}\\.\\d+))|(?'currencyPriceB'(?'currency'HKD|HK)\\$(?'unitPrice'\\d{0,3},{0,1}\\d{0,3}\\.\\d+)))");
        private readonly Regex _poRegex = new Regex("(Your Ref : \\d{10})");
        private readonly Regex _itemStartRegex = new Regex("^\\d{1}\\)");
        private readonly Regex _piNumberRegex = new Regex("(DT\\d{4}-\\d{0,3})");
        private readonly Regex _colorStartRegex = new Regex("(\\b[\\w\\d]{0,4}\\b:)");
        private readonly Regex _colorInfoRegex = new Regex("(?'quantity'\\d{0,}) (?'unit'pcs) (?'currencyPriceAll'(?'currencyPriceA'(?'currency'HKD|HK) (?'unitPrice'\\d{0,3},{0,1}\\d{0,3}\\.\\d+))|(?'currencyPriceB'(?'currency'HKD|HK)\\$(?'unitPrice'\\d{0,3},{0,1}\\d{0,3}\\.\\d+)))");

        class SKUIndex
        {
            public string SKUName { get; set; }

            public int RowIndex { get; set; }
        }

        public PIData ReadDetails(PIData PIData, SAPData sapData)
        {
            var lines = PIData.RawContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var sapSKUs = sapData.Items.Select(x => new SKUIndex()
            {
                SKUName = x.SKUName,
                RowIndex = 0
            }
            ).ToList();
            var itemFound = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                // Detect Start lines
                if (_itemStartRegex.Match(line).Success)
                {
                    if (itemFound != -1)
                    {
                        PIData.Items[itemFound].EndLineIndex = i - 1;
                    }
                    itemFound += 1;
                    PIData.Items.Add(new PIOCItem()
                    {
                        StartLineIndex = i,
                    });
                }

                // detect sku 
                var matchSku = sapSKUs.FirstOrDefault(x => line.Contains(x.SKUName));
                if (matchSku != null)
                {
                    matchSku.RowIndex = i;
                }
            }

            // Try Detect SKU Again if not found
            // Use first word block of each line and find them in SKU
            var notFoundSKUs = sapSKUs.Where(x => x.RowIndex == 0).ToList();
            if (notFoundSKUs.Any())
            {
                var start = PIData.Items.Min(x => x.StartLineIndex);
                var matchs = new List<(string SKUName, int index, string item)>();
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    if(i < start)
                    {
                        continue;
                    }
                    if (line.StartsWith("Total"))
                    {
                        break;
                    }
                    var lineItem = line.Split(' ')[0];
                    if (string.IsNullOrEmpty(lineItem))
                    {
                        continue;
                    }
                    var skuMatch = notFoundSKUs.FirstOrDefault(x => x.SKUName.Contains(lineItem));
                    if (skuMatch != null)
                    {
                        matchs.Add((skuMatch.SKUName, i, lineItem));
                    }
                }
                if (matchs.Count > 0)
                {
                    foreach(var sku in notFoundSKUs)
                    {
                        var skuMatch = matchs.Where(x=>x.SKUName == sku.SKUName).ToList();
                        if(skuMatch.Count > 0)
                        {
                            sku.SKUName = string.Join("|", skuMatch.Select(x => x.item));
                            sku.RowIndex = skuMatch.Min(x => x.index);
                        }
                    }
                }
            }
            for (int i = 0; i < PIData.Items.Count; i++)
            {
                var item = PIData.Items[i];
                var matchSku = sapSKUs
                            .Where(x => x.RowIndex >= item.StartLineIndex).OrderBy(x => x.RowIndex)
                            .FirstOrDefault();
                if (matchSku != null)
                {
                    item.SKUName = matchSku.SKUName;
                }
                item = ReadItem(lines, item);
            }
            return PIData;
        }

        public PIData ReadMeta(string pdfText)
        {
            var PIData = new PIData()
            {
                RawContent = pdfText,
            };
            var poMatch = _poRegex.Match(pdfText);
            if(!poMatch.Success) {
                PIData.PO = "NotFound";
                return PIData;
            }
            var address = new string[] { "Regina Miracle International (Group)Limited", "Units 1012, 10/F., Tower A, Regent Centre,", "63 Wo Yi Hop Road, Kwai Chung, Hong Kong" };
            if (address.All(x => pdfText.Contains(x)))
            {
                PIData.Address = string.Join(" ", address);
            }

            PIData.PO = poMatch.Value.Replace("Your Ref : ", string.Empty).Trim();
            PIData.PINumber = _piNumberRegex.Match(pdfText).Value;

            return PIData;
        }

        PIOCItem ReadItem(List<string> lines, PIOCItem item)
        {
            var isEnd = false;
            var colorFound = -1;
            var i = 0;
            while (!isEnd && (item.EndLineIndex == -1 || item.StartLineIndex + i <= item.EndLineIndex))
            {
                var line = lines[item.StartLineIndex + i];
                i++;

                // Standard matching 
                var match = _itemRegex.Match(line);
                if (match.Success)
                {
                    colorFound += 1;
                    item.Colors.Add(new PIOCColor()
                    {
                        Quantity = int.Parse(match.Groups["quantity"].Value),
                        UnitNo = match.Groups["unit"].Value,
                        Currency = match.Groups["currency"].Value,
                        UnitPrice = decimal.Parse(match.Groups["unitPrice"].Value),
                    });
                    var matchItem = match.Groups["item"].Value;
                    var colorMatch = _colorStartRegex.Match(matchItem);
                    if (colorMatch.Success)
                    {
                        item.Colors[colorFound].SKUColor = matchItem.Substring(colorMatch.Index).Trim();
                    }
                    else
                    {
                        item.Colors[colorFound].SKUColor = lines[item.StartLineIndex + 1].Trim();
                    }
                    continue;
                }

                // Extra color name only 
                // PI01-22.9.PDF
                if (line.StartsWith("(") && line.EndsWith(")") && i != 0)
                {
                    item.Colors[colorFound].SKUColor += " " + line;
                    continue;
                }

                // Item is separated into 2 lines
                // Color first line - color infor next line
                var colorStartFirstMatch = _colorStartRegex.Match(line);
                if (colorStartFirstMatch.Success)
                {
                    // No extra add due to i is added by one at start
                    var nextLine = lines[item.StartLineIndex + i];
                    var colorInfoMatch = _colorInfoRegex.Match(nextLine);
                    if (colorInfoMatch.Success)
                    {
                        // mark nextLine as read
                        i++;
                        item.Colors.Add(new PIOCColor()
                        {
                            Quantity = int.Parse(colorInfoMatch.Groups["quantity"].Value),
                            UnitNo = colorInfoMatch.Groups["unit"].Value,
                            Currency = colorInfoMatch.Groups["currency"].Value,
                            UnitPrice = decimal.Parse(colorInfoMatch.Groups["unitPrice"].Value),
                            SKUColor = line
                        });
                    }
                    continue;
                }

                // Item is separated into 2 lines
                // Color first line - color infor next line
                // DT2308-084
                var colorInfoFirstMatch = _colorInfoRegex.Match(line);
                if (colorInfoFirstMatch.Success)
                {
                    // No extra add due to i is added by one at start
                    var nextLine = lines[item.StartLineIndex + i];
                    var colorNextMatch = _colorStartRegex.Match(nextLine);
                    if (colorNextMatch.Success)
                    {
                        // mark nextLine as read
                        i++;
                        item.Colors.Add(new PIOCColor()
                        {
                            Quantity = int.Parse(colorInfoFirstMatch.Groups["quantity"].Value),
                            UnitNo = colorInfoFirstMatch.Groups["unit"].Value,
                            Currency = colorInfoFirstMatch.Groups["currency"].Value,
                            UnitPrice = decimal.Parse(colorInfoFirstMatch.Groups["unitPrice"].Value),
                            SKUColor = nextLine
                        });
                    }
                    continue;
                }

                if (line.StartsWith("Total"))
                {
                    isEnd = true;
                }
            }
            return item;
        }
    }
}
