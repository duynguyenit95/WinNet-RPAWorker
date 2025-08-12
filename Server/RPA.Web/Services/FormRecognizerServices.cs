using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RPA.Core.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RPA.Core;
using Azure.FormRecognizer;

namespace RPA.Web.Services
{
    public interface IFormRecognizerServices
    {
        Task<List<Invoice>> ParseInvoiceDocument(int customerID, string modelID, string filePath);
        //Task<PIData> ParsePIOCDocument(int customerID, string modelID, string filePath);
    }

    public class FormRecognizerServices : IFormRecognizerServices
    {
        private readonly string _FormRecognizerAPIKey;
        private readonly string _FormRecognizerEndPoint;
        private readonly RPAContext context;

        public FormRecognizerServices(IConfiguration configuration, RPAContext context)
        {
            this._FormRecognizerAPIKey = configuration["FormRecognizerAPIKey"];
            this._FormRecognizerEndPoint = configuration["FormRecognizerEndPoint"];
            this.context = context;
        }

        #region General
        public async Task<FormRecognizerLog> GeneralParser(int customerID, string modelID, string filePath, string type = "")
        {
            //var log = await GetLog(customerID, modelID, filePath,type);
            //if (log != null)
            //{
            //    return log;
            //}
            var formRecognizer = new FormRecognizerUtilities(_FormRecognizerEndPoint, _FormRecognizerAPIKey);
            var result = await formRecognizer.UseRestAPI(modelID, filePath);
            var newLog = new FormRecognizerLog()
            {
                CustomerID = customerID,
                FileName = Path.GetFileName(filePath),
                IsSuccess = result.IsSuccess,
                ModelID = modelID,
                Type = type,
                Result = JsonConvert.SerializeObject(result.Content)
            };
            
            return await SaveLog(newLog);
        }

        private async Task<FormRecognizerLog> GetLog(int customerID, string modelID, string filePath, string type = "")
        {
            var fileName = Path.GetFileName(filePath);
            return await context.FormRecognizerLogs.AsNoTracking()
                .Where(x => x.CustomerID == customerID && x.ModelID == modelID
                        && x.FileName == fileName && x.IsSuccess
                        && x.Type == type).FirstOrDefaultAsync();
        }

        private async Task<FormRecognizerLog> SaveLog(FormRecognizerLog inp)
        {
            context.FormRecognizerLogs.Add(inp);
            await context.SaveChangesAsync();
            return inp;
        }
        private async Task SaveLog(int customerID, string modelID, string filePath, string result)
        {
            var newLog = new FormRecognizerLog()
            {
                FileName = Path.GetFileName(filePath),
                CustomerID = customerID,
                ModelID = modelID,
                Result = result,
            };
            context.FormRecognizerLogs.Add(newLog);
            await context.SaveChangesAsync();
        }

        #endregion

        #region Invoice
        public async Task<List<Invoice>> ParseInvoiceDocument(int customerID, string modelID, string filePath)
        {
            var log = await GetLog(customerID, modelID, filePath);
            if (log != null)
            {
                return JsonConvert.DeserializeObject<List<Invoice>>(log.Result);
            }
            var invoice = new List<Invoice>();
            var formRecognizer = new FormRecognizerUtilities(_FormRecognizerEndPoint, _FormRecognizerAPIKey);
            var result = await formRecognizer.UseRestAPI(modelID, filePath);
            if (result.IsSuccess)
            {
                var regexInfors = context.RegexInfors.Where(x => x.CustomerID == customerID).ToList();
                invoice = ParseInvoice(result.Content, regexInfors);

                await SaveLog(customerID, modelID, filePath, JsonConvert.SerializeObject(invoice));
            }
            else
            {
                await SaveLog(customerID, modelID, filePath, string.Join("||", result.Content));
            }


            return invoice;
        }

        private List<Invoice> ParseInvoice(List<string> jsonContents, List<RegexInfor> regexInfors)
        {
            var result = new List<Invoice>();
            foreach (var jsonContent in jsonContents)
            {
                var invoice = new Invoice();
                var jObject = JObject.Parse(jsonContent);
                if (jObject["status"].ToString() == "succeeded")
                {
                    JArray docResults = JArray.FromObject(jObject["analyzeResult"]["documentResults"]);
                    foreach (JObject doc in docResults)
                    {
                        invoice.DocType = doc["docType"].ToString();
                        invoice.DocTypeConfidence = decimal.Parse(doc["docTypeConfidence"].ToString());
                        var fields = JObject.FromObject(doc["fields"]);

                        var invoiceNo = fields["InvoiceNo"];
                        if (invoiceNo != null)
                        {
                            var invoiceNoRegex = regexInfors.First(x => x.Name == "InvoiceNo");
                            var checker = Regex.Match(invoiceNo["valueString"].ToString()
                                , invoiceNoRegex.Pattern, invoiceNoRegex.Options);
                            if (checker.Success)
                            {
                                invoice.InvoiceNo = checker.Value;
                            }

                        }
                        var invoiceDate = fields["InvoiceDate"];
                        if (invoiceDate != null)
                        {
                            var invoiceDateRegex = regexInfors.First(x => x.Name == "InvoiceDate");
                            if (DateTime.TryParseExact(invoiceDate["valueString"].ToString(), invoiceDateRegex.DateFormat
                                , new CultureInfo("en-US"), DateTimeStyles.None, out DateTime date))
                            {
                                invoice.InvoiceDate = date;
                            }
                        }
                        var invoiceTotalAmount = fields["TotalAmount"];
                        if (invoiceTotalAmount != null)
                        {
                            invoice.TotalAmount = decimal.Parse(invoiceTotalAmount["text"].ToString());
                        }

                        var invoiceTable = fields["InvoiceTable"];
                        var properties = typeof(InvoiceDetails).GetProperties();
                        if (invoiceTable != null)
                        {
                            var itemArray = JArray.FromObject(invoiceTable["valueArray"]);
                            foreach (var item in itemArray)
                            {
                                var invoiceDetails = new InvoiceDetails();
                                foreach (var prop in properties)
                                {
                                    var propResult = item["valueObject"][prop.Name];
                                    if (propResult != null)
                                    {
                                        if (propResult["type"].ToString() == "number")
                                        {
                                            if (propResult["valueNumber"] != null)
                                            {
                                                prop.SetValue(invoiceDetails, decimal.Parse(propResult["valueNumber"].ToString()), null);
                                            }
                                        }
                                        else if (propResult["type"].ToString() == "string")
                                        {
                                            if (propResult["valueString"] != null)
                                            {
                                                prop.SetValue(invoiceDetails, propResult["valueString"].ToString(), null);
                                            }
                                        }
                                        else if (propResult["type"].ToString() == "integer")
                                        {
                                            if (propResult["valueInteger"] != null)
                                            {
                                                prop.SetValue(invoiceDetails, int.Parse(propResult["valueInteger"].ToString()), null);
                                            }

                                        }
                                    }
                                }
                                invoice.InvoiceTable.Add(invoiceDetails);
                            }

                        }
                    }
                    result.Add(invoice);
                }
            }

            return result;
        }

        #endregion

        #region PIOC
        //public async Task<PIData> ParsePIOCDocument(int customerID, string modelID, string filePath)
        //{
        //    var parseResult = await GeneralParser(customerID, modelID, filePath, FormRecognizerParserType.PIOC);
        //    if (parseResult.IsSuccess)
        //    {
        //        var contents = JsonConvert.DeserializeObject<List<string>>(parseResult.Result);
        //        return new PIData(contents[0]);
        //    }
        //    return null;
        //}
        #endregion

    }
}

