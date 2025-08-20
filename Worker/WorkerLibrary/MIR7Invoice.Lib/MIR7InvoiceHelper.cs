using RPA.Core.Data;
using RPA.Tools;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static RPA.Tools.ITextSharpPDFOCR;

namespace MIR7Invoice.Lib
{
    public class MIR7InvoiceHelper
    {
        public MIR7InvoiceHelper()
        {
        }

        #region ReadInvoice

        #region 1 - 500855 - BEST PACIFIC TEXTILE LTD.
        public Invoice ReadBestPacificInvoice(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Match(invoiceContent, @"(?'value'H[A-Z]?\d{8}-P\d{1,3})");
            var invoiceDateMatch = Regex.Match(invoiceContent, @"(?'value'\d{4}\.\d{2}\.\d{2})");
            var invoicePOMatch = Regex.Match(invoiceContent, @"\s{1}(?'value'\d{10})\s{1}");
            var invoiceTotalAmountMatch = Regex.Match(invoiceContent, @"Total.*:\s*(?'value'[\d,.]+)");
            var invoiceCurrencyMatch = Regex.Match(invoiceContent, @"Total.*?\((?'value'\w+)\):");

            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "yyyy.MM.dd" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var PO = invoicePOMatch.Success ? invoicePOMatch.Groups["value"].Value : string.Empty;
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = { PO },
                Currency = Currency,
            };
        }
        #endregion

        #region 2 - 500333 - SML (HONG KONG) Limited.
        public Invoice ReadSMLInvoice(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Match(invoiceContent, @"Invoice ID:(?'value'\w{11})");
            var invoiceDateMatch = Regex.Match(invoiceContent, @"Invoice date:(?'value'\d{1,2}/\d{1,2}/\d{4})");
            var invoicePOMatch = Regex.Match(invoiceContent, @"Po&So\s*:(?'value'\s*(\d+))");
            var invoiceTotalAmountMatch = Regex.Match(invoiceContent, @"Total amount:\s*(?'value'[\d,.]+)");
            var invoiceCurrencyMatch = Regex.Match(invoiceContent, @"Amount\((?'value'\w+)\)");

            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "M/d/yyyy" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var PO = invoicePOMatch.Success ? invoicePOMatch.Groups["value"].Value : string.Empty;
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = { PO },
                Currency = Currency,
            };
        }
        #endregion

        #region 3 - 502350 - Nilorn East Asia Limited
        public Invoice ReadNilornInvoice(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Match(invoiceContent, @"Invoice No (?'value'\w{11})");
            var invoiceDateMatch = Regex.Match(invoiceContent, @"Invoice Date (?'value'\d{1,2}/\d{1,2}/\d{4})");
            var invoicePOMatch = Regex.Match(invoiceContent, @"Your Order (?'value'(\d+))");
            var invoiceTotalAmountMatch = Regex.Match(invoiceContent, @"Total USD (?'value'\d{1,3}(?: \d{3})*(?:,\d+)?(?:\.\d+)?)");
            var invoiceOrderNoMatch = Regex.Match(invoiceContent, @"Order No\. (?'value'\w+)");

            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(Regex.Replace(invoiceTotalAmountMatch.Groups["value"].Value, @"\s+", "")) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "dd/MM/yyyy" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var PO = invoicePOMatch.Success ? invoicePOMatch.Groups["value"].Value : string.Empty;
            var Delivery = invoiceOrderNoMatch.Success ? invoiceOrderNoMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = { PO },
                Currency = "USD",
                Delivery = { Delivery },
            };
        }
        #endregion

        #region 4 - 500190 - INTERNATIONAL TRIMMINGS & LABELS
        public Invoice ReadInternationalTrimmingsLabels(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Match(invoiceContent, @"Invoice No: (?'value'\w{8})");
            var invoiceDateMatch = Regex.Match(invoiceContent, @"Invoice Date: (?'value'\d{4}/\d{2}/\d{2})");
            var invoiceTotalAmountMatch = Regex.Match(invoiceContent, @"INVOICE TOTAL: (?'quantity'\d+) (?'totalAmount'[\d,.]+)");
            var invoicePOMatch = Regex.Match(invoiceContent, @"Customer Ref: (?'value'\w{10,11})");
            var invoiceCurrencyMatch = Regex.Match(invoiceContent, @"TOTAL.*?\((?'value'\w+)\)");

            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(Regex.Replace(invoiceTotalAmountMatch.Groups["totalAmount"].Value, @"\s+", "")) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "yyyy/MM/dd" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var PO = invoicePOMatch.Success ? invoicePOMatch.Groups["value"].Value : string.Empty;
            var Quantity = invoiceTotalAmountMatch.Success ? decimal.Parse(Regex.Replace(invoiceTotalAmountMatch.Groups["quantity"].Value, @"\s+", "")) : 0;
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = { PO },
                Currency = Currency,
                Quantity = Quantity,
            };
        }
        #endregion

        #region 5 - 500119 - FineLine Technologies LLC LTD
        public Invoice ReadFineLineTechnologies(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"^Invoice:\s*(?'value'\d+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"(?'value'[A-Za-z]+, [A-Za-z]+ \d{1,2}, \d{4})$\nShip Date:$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"USD TOTAL\n.{0,}?\$(?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "dddd, MMMM d, yyyy" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = { },
                Currency = "USD",
            };
        }
        #endregion

        #region 6 - 500449 - YVONNE INDUSTRIAL COMPANY LTD
        public Invoice ReadYVONNEINDUSTRIAL(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"^INVOICE NO\. : (?'value'\d{2}-\d{6})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"^DATE : (?'value'\d{2}\/\d{2}\/\d{4})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"(?'value'[\d,.]+)\nTotal:", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "dd/MM/yyyy" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = { },
                Currency = "HKD",
                
            };
        }
        #endregion

        #region 7 - 500341 - STRETCHLINE (HONG KONG) LTD
        public Invoice ReadSTRETCHLINE(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            Match invoiceIDMatch, invoiceDateMatch, invoiceTotalAmountMatch, invoiceCurrencyMatch;
            var formatDate = string.Empty;
            if (Regex.Match(invoiceContent, @"\(Authorized Signature\)").Success)
            {
                invoiceIDMatch = Regex.Matches(invoiceContent, @"Invoice Number : (?'value'\d{8})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoiceDateMatch = Regex.Matches(invoiceContent, @"Invoice Date :\n(?'value'\d{2}/\d{2}/\d{4})", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"(?'value'[\d,.]+)\nSample Types", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"Currency.+?(?'value'\w{2,3}.+?) Tel:", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                formatDate = "MM/dd/yyyy";
            }
            else
            {
                invoiceIDMatch = Regex.Matches(invoiceContent, @"^Invoice Number(?:\n| )?.*?(?'value'\d{8})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoiceDateMatch = Regex.Matches(invoiceContent, @"^Date(?:\n| )?.*?(?'value'\d{4}/\d{2}/\d{2})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"(?'value'[\d,.]+)\nSur Charges$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"^(?'value'.+?)\nTotal", RegexOptions.Multiline).Cast<Match>().LastOrDefault();
                formatDate = "yyyy/MM/dd";
            }
            

            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { formatDate }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                Currency = Currency,
            };
        }
        #endregion

        #region 8 - 500295 - PRYM INTIMATES HONG KONG LIMITED
        public Invoice ReadPRYMINTIMATES(string pdfFilePath)
        {
            var pageDimension = GetPageDimension(PageType.A4, PageOrientation.Landscape);
            var custPOContent = ITextSharpPDFOCR.ExtractVerticalColumnText(pdfFilePath, pageDimension, "Cust.PO.No", "Sub-total Quantity", 20, 0);
            var PO = Regex.Matches(custPOContent, @"\d+", RegexOptions.Multiline).Cast<Match>().Select(x => x.Value).Distinct().ToList();

            var invoiceTotalContent = ITextSharpPDFOCR.ExtractHorizontalColumnText(pdfFilePath, pageDimension, "Invoice Total", 2);
            var invoiceTotalAmountMatch = Regex.Matches(invoiceTotalContent, @"(?'value'[\d,.]+)", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();


            var commerialInvoiceContent = ITextSharpPDFOCR.ExtractVerticalColumnText(pdfFilePath, pageDimension, "Commercial Invoice", "Account No", 5, 100);
            var invoiceIDMatch = Regex.Matches(commerialInvoiceContent, @"Number(?:\n|\r\n| )?.*?(?'value'\d+)", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(commerialInvoiceContent, @"Date(?:\n|\r\n| )?.*?(?'value'\d{4}.\d{2}.\d{2})", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceCurrencyMatch = Regex.Matches(commerialInvoiceContent, @"Currency(?:\n|\r\n| )?.*?(?'value'\w+)", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

                        

            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "yyyy.MM.dd" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = PO,
                Currency = Currency,
                
            };
        }
        #endregion

        #region 9 - 502054 - New Horizon Investment (Hong Kong）
        public Invoice ReadNewHorizonInvestment(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"^Invoice No\.:\n(?'value'\w{2,3}\d{7,8}(?:-\w+)?)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"Invoice Date:\n(?'value'\d{4}.\d{2}.\d{2})", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"^Total 總金額.+?(?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"^Total 總金額\((?'value'\w+)\)", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "yyyy.MM.dd" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = { },
                Currency = Currency,
                
            };
        }
        #endregion

        #region 10 - 500283 - PIONEER ELASTIC (HONG KONG) Ltd
        public Invoice ReadPIONEERELASTIC(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"(?'value'MF\d+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"(?'value'\d{2}-\w{3}-\d{4})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"GRAND-TOTAL : (?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"^TOTAL:SAY (?'value'\w+)", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "dd-MMM-yyyy" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = { },
                Currency = Currency,
                
            };
        }
        #endregion

        #region 11 - 500158 - HING YIP PRODUCTS 1971 LIMITED
        public Invoice ReadHINGYIPPRODUCTS1971LIMITED(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"INV NO.+?(?'value'\w{10})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"DATE.+?(?'value'\d{4}/\d{1,2}/\d{1,2})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"Total amount .+?(?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"CURR.+?(?'value'\w+)", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDeliveryMatch = Regex.Matches(invoiceContent, @"(?<=备注\(Remarks\):\s*(?:[\s\S]*?))(?'value'SO\w{8})", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "yyyy/M/d" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;
            var Delivery = invoiceDeliveryMatch.Success ? invoiceDeliveryMatch.Groups["value"].Value : null;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                Currency = Currency,
                Delivery = { Delivery },
                PO = { },
            };
        }
        #endregion

        #region 12 - 500587 - LIJUN (HK) INDUSTRIAL CO.LTD
        public Invoice ReadLIJUNINDUSTRIAL(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"^(?'value'\d{9})$\n電話", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"Date/日期: (?'value'\d{4}/\d{2}/\d{2})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"^應收總金額: (?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "yyyy/MM/dd" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = { },
                Currency = "HKD",
                
            };
        }
        #endregion

        #region 13 - 500380 - TIANHAI LACE CO., LTD
        public Invoice ReadTIANHAILACE(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"INV NO:$\n.+\n(?'value'\w+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"^(?'value'\d{4}-\d{2}-\d{2})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"(?'value'[\d,.]+)$\n总计.+\nTOTAL", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "yyyy-MM-dd" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = { },
                Currency = "USD",
                
            };
        }
        #endregion

        #region 14. 500538 SILVER PRINTING COMPANY LTD
        public Invoice ReadSilverPrintingCompany(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"Invoice No\.\n.+: (?'value'\w{10})\nDate$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDeliveryMatchList = Regex.Matches(invoiceContent, @"^Sales Order:(?'value'\w{12})$", RegexOptions.Multiline).Cast<Match>().ToList();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"^Date\n.+(?'value'\w{9})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"^Net Amount (?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoicePOMatch = Regex.Matches(invoiceContent, @"^Cus. P.O. No.:(?'value'\d{10})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"^Net Amount.+\n.+?(?'value'\w{2,3})", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var Delivery = invoiceDeliveryMatchList.Where(match => match.Success).Select(match => match.Groups["value"].Value).Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToList();
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "ddMMMyyyy" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var PO = invoicePOMatch.Success ? invoicePOMatch.Groups["value"].Value : string.Empty;
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;

            return new Invoice()
            {
                InvoiceNo = InvoiceNo,
                Delivery = Delivery,
                InvoiceDate = InvoiceDate,
                TotalAmount = TotalAmount,
                PO = { PO },
                Currency = Currency,
            };
        }
        #endregion

        #region 15. 502485 INSPIRE PLASTICS LIMITED
        public Invoice ReadInspirePlasticsLimited(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"^INVOICE NO : (?'value'\w{10})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDeliveryMatchList = Regex.Matches(invoiceContent, @"^OUR SALES ORDER NO\. : (?'value'\w{10})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"^Date : (?'value'\d{1,2}/\d{1,2}/\d{4})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"^TOTAL.+?(?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoicePOMatch = Regex.Matches(invoiceContent, @"^CUSTOMER REF\. NO\. : (?'value'\w{10})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"^TOTAL : (?'value'\w{2,3}) [\d[,.]+$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var Delivery = invoiceDeliveryMatchList.Success ? invoiceDeliveryMatchList.Groups["value"].Value : null;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "dd/MM/yyyy" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var PO = invoicePOMatch.Success ? invoicePOMatch.Groups["value"].Value : string.Empty;
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;

            return new Invoice()
            {
                InvoiceNo = InvoiceNo,
                Delivery = { Delivery },
                InvoiceDate = InvoiceDate,
                TotalAmount = TotalAmount,
                PO = { PO },
                Currency = Currency,
            };
        }
        #endregion

        #region 16. 501091 Hung Hon (4K) Limited
        public Invoice ReadHungHon4KLimited(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"Invno: (?'value'\w{6,8})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"^Date: (?'value'\d{4}-\d{2}-\d{2})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"^Total amount: .+?(?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoicePOMatch = Regex.Matches(invoiceContent, @"(?'value'^\d{10})", RegexOptions.Multiline).Cast<Match>().ToList();
            var invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"^Cur: (?'value'\w{2,3})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "yyyy-MM-dd" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var PO = invoicePOMatch.Where(match => match.Success).Select(match => match.Groups["value"].Value).Distinct().ToList();
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;


            var invoiceDeliveryPatternCase1 = @"Delivery.*under D\/O NO\s*(?'value'[A-Za-z0-9-]+)\s+on";
            var invoiceDeliveryPatternCase2 = @"Delivery:\s*(?'value'[A-Z0-9-]+)\b";
            var invoiceDeliveryMatch = Regex.Match(invoiceContent, invoiceDeliveryPatternCase1, RegexOptions.Multiline);
            if (!invoiceDeliveryMatch.Success)
            {
                invoiceDeliveryMatch = Regex.Match(invoiceContent, invoiceDeliveryPatternCase2, RegexOptions.Multiline);
            }

            var Delivery = (invoiceDeliveryMatch != null && invoiceDeliveryMatch.Success) ? new List<string> { invoiceDeliveryMatch.Groups["value"].Value } : new List<string>();

            return new Invoice()
            {
                InvoiceNo = InvoiceNo,
                Delivery =  Delivery ,
                InvoiceDate = InvoiceDate,
                TotalAmount = TotalAmount,
                PO = PO,
                Currency = Currency,
            };
        }
        #endregion

        #region 17. 500353 SUNRISE TEXTILE ACCESSORIES (TRADING)
        public Invoice ReadSunriseTextileAccessories(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"^INV NO\.: (?'value'\w{1}\d{6}-\d{3})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDeliveryMatch = Regex.Matches(invoiceContent, @"PO. \d{10}\((?'value'DN\/\d{6}-\w+)\)", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"Date : (?'value'\d{2} \w{3}\.\, \d{4})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"^Net Amount : (?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoicePOMatch = Regex.Matches(invoiceContent, @"^INVOICE P\.O : (?'value'\d{10})$", RegexOptions.Multiline).Cast<Match>().ToList();
            var invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"^DESCRIPTION Quantity Unit Price Amount\n\w+ (?'value'\w{2,3})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var Delivery = (invoiceDeliveryMatch != null && invoiceDeliveryMatch.Success) ? new List<string> { invoiceDeliveryMatch.Groups["value"].Value } : new List<string>();
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "dd MMM., yyyy" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var PO = String.Join("/", invoicePOMatch.Where(match => match.Success).Select(match => match.Groups["value"].Value).Distinct().ToList());
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;

            return new Invoice()
            {
                InvoiceNo = InvoiceNo,
                Delivery = Delivery,
                InvoiceDate = InvoiceDate,
                TotalAmount = TotalAmount,
                PO = { PO },
                Currency = Currency,
            };
        }
        #endregion


        #region 18. 500266 PACIFIC TEXTILES LTD
        public Invoice ReadPacificTextilesLTD(string pdfFilePath)
        {
            var pageDimension = GetPageDimension(PageType.A4, PageOrientation.Portrait);

            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"^INVOICE \# : (?'value'\d{9})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDeliveryMatchList = Regex.Matches(invoiceContent, @"^(?'value'80\d{7})", RegexOptions.Multiline).Cast<Match>().ToList();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"^Date: (?'value'\d{2}-\w{3}-\d{2})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"^TOTAL [\d,.]+YD (?'value'[\d,.]+) $\nTOTAL POUNDS", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoicePOMatch = Regex.Matches(invoiceContent, @"^PO: (?'value'\d{10})\(SO:\d{7}\)", RegexOptions.Multiline).Cast<Match>().ToList();
            var invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"(?'value'\w{2,3})\nUnit /Unit$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var Delivery = invoiceDeliveryMatchList.Where(match => match.Success).Select(match => match.Groups["value"].Value).Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToList();
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "dd-MMM-y" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var PO = String.Join("/", invoicePOMatch.Where(match => match.Success).Select(match => match.Groups["value"].Value).Distinct().ToList());
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;

            var deliveryColumn = ITextSharpPDFOCR.ExtractVerticalColumnText(pdfFilePath, pageDimension, "Mark & No", "TOTAL", 5, 30);
            var invoiceDeliveryMatchListV2 = Regex.Matches(deliveryColumn, @"^(?'value'\d{9})", RegexOptions.Multiline).Cast<Match>().ToList();
            var DeliveryV2 = invoiceDeliveryMatchListV2.Where(match => match.Success).Select(match => match.Groups["value"].Value).Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToList();

            return new Invoice()
            {
                InvoiceNo = InvoiceNo,
                Delivery = DeliveryV2,
                InvoiceDate = InvoiceDate,
                TotalAmount = TotalAmount,
                PO = { PO },
                Currency = Currency,
            };
        }
        #endregion

        #region 19. 500029 BILLION RISE KNITTING (HK) Limited
        public Invoice ReadBillionRiseKnittingLimited(string pdfFilePath)
        {
            var pageDimension = GetPageDimension(PageType.A4, PageOrientation.Portrait);
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"Invoice No:( +)?(?'value'\d{2}\w{3}-\w{2}\d{4})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"(?'value'\d{4}/\d{1,2}/\d{1,2})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"TOTAL:? ?[\d,.]+ (?'value'[\d,.]+)( ?)+$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoicePOMatch = Regex.Matches(invoiceContent, @"PO#( +)?(\n+)?(?'value'\d{10})$", RegexOptions.Multiline).Cast<Match>().ToList();
            var invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"送货单号\n.+\((?'value'\w{2,3})\)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "yyyy/M/d" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var PO = String.Join("/", invoicePOMatch.Where(match => match.Success).Select(match => match.Groups["value"].Value).Distinct().ToList());
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;

            var invoiceDeliveryMatch = Regex.Matches(invoiceContent, @"(?<=送货单号\r?\n(?:\S+\s+){2,})(?'value'[Z]?\d{6,9}( ?|/[Z]?\d{3}?))$", RegexOptions.Multiline).Cast<Match>().ToList();
            //Join all delivery matches
            var joinedDelivery = string.Join(" ", invoiceDeliveryMatch.Select(x => x.Groups["value"].Value).ToList());
            var year2digit = InvoiceDate.ToString("yy");
            //Split by / or space or 9 characters
            var Delivery = Regex.Split(joinedDelivery, @"( |\/|\r?\n)")
                            .Where(x => !String.IsNullOrWhiteSpace(x) && x.Length == 9)
                            .Where(x => x.StartsWith(year2digit) || x.StartsWith("Z"+year2digit))
                            .Distinct()
                            .ToList();


            var deliveryColumn = ITextSharpPDFOCR.ExtractVerticalColumnText(pdfFilePath, pageDimension, "送货单号", "TOTAL", 10, 10);
            var cleanDeliveryText = Regex.Replace(deliveryColumn, @"\s+", "");
            var DeliveryV2 = Regex.Matches(cleanDeliveryText, "([Z0-9]{9})").Cast<Match>().Select(x => x.Value).Distinct().ToList();

            return new Invoice()
            {
                InvoiceNo = InvoiceNo,
                Delivery = DeliveryV2,
                InvoiceDate = InvoiceDate,
                TotalAmount = TotalAmount,
                PO = { PO },
                Currency = Currency,
            };
        }
        #endregion

        #region 20. 501245 FASTECH ASIA WORLDWIDE LIMITED
        public Invoice ReadFastechAsiaWorldwideLimited(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"^(?'value'#\d{10})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDeliveryMatchList = Regex.Matches(invoiceContent, @"^#(?'value'\d{10})$", RegexOptions.Multiline).Cast<Match>().ToList();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"^(?'value'\d{1,2}/\d{1,2}/\d{4})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"^ Total.+?(?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoicePOMatch = Regex.Matches(invoiceContent, @"PO #.+\n.+?(?'value'\d{10}) ", RegexOptions.Multiline).Cast<Match>().ToList();
            var invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"^ Total (?!Quantity)(?'value'.+?)[\d,.]+$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var Delivery = invoiceDeliveryMatchList.Where(match => match.Success).Select(match => match.Groups["value"].Value).Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToList();
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "M/d/yyyy" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var PO = String.Join("/", invoicePOMatch.Where(match => match.Success).Select(match => match.Groups["value"].Value).Distinct().ToList());
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;

            return new Invoice()
            {
                InvoiceNo = InvoiceNo,
                Delivery = Delivery,
                InvoiceDate = InvoiceDate,
                TotalAmount = TotalAmount,
                PO = { PO },
                Currency = Currency,
            };
        }
        #endregion

        #region 21. 500523 JIANGMEN XINHUI CHARMING Industry
        public Invoice ReadJiangmenXinhuiCharmingIndustry(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            var invoiceIDMatch = Regex.Matches(invoiceContent, @"^INVOICE NO: (?'value'SEOUT\d{6})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDeliveryMatchList = Regex.Matches(invoiceContent, @"PACKING LIST NO: (?'value'\w{10})$", RegexOptions.Multiline).Cast<Match>().ToList();
            var invoiceDateMatch = Regex.Matches(invoiceContent, @"DATE: (?'value'[A-Za-z]+ \d{1,2}, \d{4})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @" (?'value'[\d,.]+)\nTOTAL SAY:", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoicePOMatch = Regex.Matches(invoiceContent, @"P/O NO.+\n.+\n[\w/,.-]+ (?'value'\d{10})", RegexOptions.Multiline).Cast<Match>().ToList();
            var invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"AMOUNT\n.+\n.+?(?'value'\w{3})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var Delivery = invoiceDeliveryMatchList.Where(match => match.Success).Select(match => match.Groups["value"].Value).Where(x => !String.IsNullOrWhiteSpace(x)).Distinct().ToList();
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "MMMM d, yyyy" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var PO = String.Join("/", invoicePOMatch.Where(match => match.Success).Select(match => match.Groups["value"].Value).Distinct().ToList());
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;

            return new Invoice()
            {
                InvoiceNo = InvoiceNo,
                Delivery = Delivery,
                InvoiceDate = InvoiceDate,
                TotalAmount = TotalAmount,
                PO = { PO },
                Currency = Currency,
            };
        }
        #endregion

        #region 22 - 500364 - TAI HING PLASTIC METAL LTD
        public Invoice ReadTaiHingPlasticMetal(string pdfFilePath)
        {
            var invoiceContent = ITextSharpPDFOCR.ReadFile(pdfFilePath);
            Match invoiceNoMatch, invoiceDateMatch, invoiceTotalAmountMatch, invoiceCurrencyMatch;
            List<Match> invoicePOMatch;
            if (Regex.Match(invoiceContent, @"INVOICE").Success)
            {
                invoiceNoMatch = Regex.Matches(invoiceContent, @"Invoice No. : (?'value'\d{6})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoiceDateMatch = Regex.Matches(invoiceContent, @"Date : (?'value'\d{2}/\d{2}/\d{4})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"NET AMOUNT : (?'currency'\w{2,3}) (?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoicePOMatch = Regex.Matches(invoiceContent, @"Reference No. : (?'value'\d{10})$", RegexOptions.Multiline).Cast<Match>().ToList();
                invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"NET AMOUNT : (?'value'\w{2,3}) (?'total'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            }
            else
            {
                invoiceNoMatch = Regex.Matches(invoiceContent, @"發票號碼 : (?'value'\d{6})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoiceDateMatch = Regex.Matches(invoiceContent, @"日期 : (?'value'\d{2}/\d{2}/\d{4})$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoiceTotalAmountMatch = Regex.Matches(invoiceContent, @"總淨金額 : (?'currency'\w{2,3}) (?'value'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
                invoicePOMatch = Regex.Matches(invoiceContent, @"參考號碼 : (?'value'\d{10})$", RegexOptions.Multiline).Cast<Match>().ToList();
                invoiceCurrencyMatch = Regex.Matches(invoiceContent, @"總淨金額 : (?'value'\w{2,3}) (?'total'[\d,.]+)$", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            }
            var invoiceDeliveryMatch = Regex.Matches(invoiceContent, @"OUR REF\s*#\s*:\s*?(?'value'\w+)\/\+", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();

            var InvoiceNo = invoiceNoMatch.Success ? invoiceNoMatch.Groups["value"].Value : string.Empty;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "dd/MM/yyyy" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var PO = String.Join("/", invoicePOMatch.Where(match => match.Success).Select(match => match.Groups["value"].Value).Distinct().ToList());
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;
            var Delivery = (invoiceDeliveryMatch != null && invoiceDeliveryMatch.Success) ? new List<string> { invoiceDeliveryMatch.Groups["value"].Value } : new List<string>();

            return new Invoice()
            {
                InvoiceNo = InvoiceNo,
                InvoiceDate = InvoiceDate,
                TotalAmount = TotalAmount,
                PO = { PO },
                Currency = Currency,
                Delivery = Delivery,
            };
        }
        #endregion

        #region 23. 500255 NEXGEN PACKAGING LTD
        public Invoice ReadNEXGENPACKAGINGLTD(string pdfFilePath)
        {
            var pageDimension = GetPageDimension(PageType.A4, PageOrientation.Landscape);
            var custPOContent = ITextSharpPDFOCR.ExtractVerticalColumnText(pdfFilePath, pageDimension, "Cust.PO.No", "Sub-total Quantity", 20, 0);
            var PO = Regex.Matches(custPOContent, @"\d+", RegexOptions.Multiline).Cast<Match>().Select(x => x.Value).Distinct().ToList();

            var invoiceTotalContent = ITextSharpPDFOCR.ExtractHorizontalColumnText(pdfFilePath, pageDimension, "Invoice Total", 2);
            var invoiceTotalAmountMatch = Regex.Matches(invoiceTotalContent, @"(?'value'[\d,.]+)", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();


            var commerialInvoiceContent = ITextSharpPDFOCR.ExtractVerticalColumnText(pdfFilePath, pageDimension, "Commercial Invoice", "Account No", 5, 100);
            var invoiceIDMatch = Regex.Matches(commerialInvoiceContent, @"Number(?:\n|\r\n| )?.*?(?'value'\d+)", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceDateMatch = Regex.Matches(commerialInvoiceContent, @"Date(?:\n|\r\n| )?.*?(?'value'\d{4}.\d{2}.\d{2})", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();
            var invoiceCurrencyMatch = Regex.Matches(commerialInvoiceContent, @"Currency(?:\n|\r\n| )?.*?(?'value'\w+)", RegexOptions.Multiline).Cast<Match>().FirstOrDefault();



            var InvoiceNo = invoiceIDMatch.Success ? invoiceIDMatch.Groups["value"].Value : string.Empty;
            var InvoiceDate = invoiceDateMatch.Success ? DateTime.TryParseExact(invoiceDateMatch.Groups["value"].Value, new[] { "yyyy.MM.dd" }, null
                 , System.Globalization.DateTimeStyles.None, out DateTime result) ? result : DateTime.MinValue : DateTime.MinValue;
            var TotalAmount = invoiceTotalAmountMatch.Success ? decimal.Parse(invoiceTotalAmountMatch.Groups["value"].Value) : 0;
            var Currency = invoiceCurrencyMatch.Success ? invoiceCurrencyMatch.Groups["value"].Value : string.Empty;
            return new Invoice()
            {
                TotalAmount = TotalAmount,
                InvoiceDate = InvoiceDate,
                InvoiceNo = InvoiceNo,
                PO = PO,
                Currency = Currency,

            };
        }
        #endregion

        #endregion
    }
}
