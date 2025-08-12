using RPA.Core.Data;
using RPA.Tools;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MIR7InvoiceWorker.Lib
{
    public class MIR7InvoiceWorkerHelper
    {
        public MIR7InvoiceWorkerHelper()
        {
        }
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
                PO = PO,
                Currency = Currency,
                Delivery = InvoiceNo
            };
        }

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
                PO = PO,
                Currency = Currency,
                Delivery = InvoiceNo
            };
        }
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
                PO = PO,
                Currency = "USD",
                Delivery = Delivery,
            };
        }
    }
}
