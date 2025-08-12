using RPA.Core;
using RPA.Worker.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCifs.Smb;
using RPA.Tools;
using System.IO.Compression;
using CsvHelper.Configuration;
using CsvHelper;
using System.Xml.Linq;
using System.Runtime.Remoting.Contexts;
//using Sha
namespace RPA12.Lib
{
    public class RPA12Unit
    {
        private readonly XNamespace XML_EXCEL_NAMESPACE;
        private readonly XName XML_WORKSHEET;
        private readonly XName XML_TABLE;
        private readonly XName XML_ROW;
        private readonly XName XML_CELL;
        private readonly XName XML_DATA;
        private readonly WorkerExecutionContext<WorkerOption> context;

        public Dictionary<string, string> colKey { get; set; }
        public Dictionary<string, string> ztfColKey { get; set; }
        public RPA12Unit(WorkerExecutionContext<WorkerOption> context)
        {
            XML_EXCEL_NAMESPACE = "urn:schemas-microsoft-com:office:spreadsheet";
            XML_WORKSHEET = XML_EXCEL_NAMESPACE + "Worksheet";
            XML_TABLE = XML_EXCEL_NAMESPACE + "Table";
            XML_ROW = XML_EXCEL_NAMESPACE + "Row";
            XML_CELL = XML_EXCEL_NAMESPACE + "Cell";
            XML_DATA = XML_EXCEL_NAMESPACE + "Data";

            colKey = new Dictionary<string, string>()
            {
                { "SupplierID", "供应商编码"},
                { "PO", "采购凭证"},
                { "DeliveryDate", "要求交货日期"},
                { "LackQuantity", "实际欠料数"},
                { "MaterialGroup", "采购组"},
                { "Factory", "采购工厂"},
            };
            ztfColKey = new Dictionary<string, string>()
            {
                { "WERKS", "WERKS"},
                { "PO", "EBELN"},
                { "MaterialNo", "MATNR"},
                { "Color", "J_3ASIZE"},
                { "Quantity", "MENGE_P"},
                { "ProductionPlanDate", "FDATE"},
                { "AmountHKD", "QLJE_HK"},
            };
            this.context = context;
            //SQLiteHelper.CreateDatabaseIfNotExists<RPA112Context, RPA112DBInitializer>(RPA112Context.DB_NAME);
        }

        public WorkerExecutionContext<WorkerOption> PreExecute(string LocalProcessFolder)
        {
            try
            {
                var auth = new NtlmPasswordAuthentication(context.WorkerOptions.NSFOpt.AuthDomain, context.WorkerOptions.NSFOpt.AuthUsername, context.WorkerOptions.NSFOpt.AuthPassword);
                var smbDir = new SmbFile(context.WorkerOptions.NSFOpt.InputFolderURI, auth);
                var allFiles = smbDir.ListFiles($"采购跟踪报表_VN*.zip");
                var vnp01 = allFiles.Where(x => x.GetName().Contains("VNP01")).OrderByDescending(x => x.GetLocalLastModified()).FirstOrDefault();
                if (vnp01 != null)
                {
                    SmbExtensions.CopyNSFFileToLocal(auth, vnp01.GetPath(), Path.Combine(LocalProcessFolder, vnp01.GetName()));
                }
                var vnp02 = allFiles.Where(x => x.GetName().Contains("VNP02")).OrderByDescending(x => x.GetLocalLastModified()).FirstOrDefault();
                if (vnp02 != null)
                {
                    SmbExtensions.CopyNSFFileToLocal(auth, vnp02.GetPath(), Path.Combine(LocalProcessFolder, vnp02.GetName()));
                }
            }
            catch (Exception ex)
            {
                context.Log("Error: " + ex);
                context.IsSuccess = false;
            }
            return context;
        }

        public async Task<IEnumerable<RPA12Input>> ReadInputFile(string file)
        {
            var cultrure = new CultureInfo("en-US");
            string[] dateFormat = { "yyyy/MM/dd" };
            var fileFullName = Path.GetFileName(file);
            context.Log($" Process file {fileFullName} !!!!");

            var fileName = Path.GetFileNameWithoutExtension(file);
            var fileDate = DateTime.ParseExact(fileName.Substring(fileName.Length - 8, 8), "yyyyMMdd", cultrure);
            var extractPath = Path.Combine(Path.GetDirectoryName(file), fileName);
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }
            ZipFile.ExtractToDirectory(file, extractPath, Encoding.GetEncoding("GB2312"));
            context.Log($" UpZip Complete !!!!");

            var xlsFile = new DirectoryInfo(extractPath).GetFiles().First();
            var dataTable = new DataTable();
            var content = File.ReadAllText(xlsFile.FullName, Encoding.GetEncoding("GB2312"));
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                BadDataFound = null,
                NewLine = Environment.NewLine,
                DetectDelimiter = true,
                MissingFieldFound = null,
            };

            using (var csv = new CsvReader(new StringReader(content), config))
            {
                using (var dr = new CsvDataReader(csv))
                {
                    dataTable.Load(dr);
                }
            }
            context.Log($"Read XLS File Complete !!!!");

            var moveZipFile = extractPath + "/" + fileFullName;
            if (File.Exists(moveZipFile))
            {
                File.Delete(moveZipFile);
            }

            File.Move(file, moveZipFile);
            File.Delete(xlsFile.FullName);

            while (File.Exists(file))
            {
                File.Delete(file);
                await Task.Delay(1000);
            }
            context.Log($" Moving Source File Complete!!!!");


            context.Log($" Start Processing Data!!!!");
            var data = dataTable.ToClass<RPA12RawInput>(colKey, cultrure, dateFormat);

            var inputs = data
                // Supplier Filter
                .Where(x => x.SupplierID != "0000500855" && x.SupplierID != "0000500641"
                // Lack Quantity
                && x.LackQuantity > 0
                ).GroupBy(x => (x.PO, x.DeliveryDate))
                .Select(x => new RPA12Input()
                {
                    PO = x.Key.PO,
                    DeliveryDate = x.Key.DeliveryDate,
                    FileDate = fileDate
                });
            Helper.ClassToExcelSheet(inputs, Path.Combine(extractPath, fileName + "_Inputs.xlsx"));
            return inputs;
        }

        public List<ZTF003Output> ReadXML(string filePath)
        {
            //var filename = Path.Combine(AppContext.BaseDirectory, "2022.01.12.xml");
            var result = new List<ZTF003Output>();
            XElement data = XElement.Load(filePath);
            var sheet = data.Elements(XML_WORKSHEET).First();
            var table = sheet.Element(XML_TABLE);
            var rows = table.Elements(XML_ROW);
            var rowIndex = 2;
            foreach (var row in rows.Skip(1))
            {
                var cells = row.Elements(XML_CELL).ToArray();
                var item = new ZTF003Output()
                {
                    AmountHKD = decimal.TryParse(cells[39].Element(XML_DATA)?.Value ?? string.Empty, out decimal amountHKD) ? amountHKD : 0m,
                    Color = cells[20].Element(XML_DATA)?.Value ?? string.Empty,
                    WERKS = cells[1].Element(XML_DATA)?.Value ?? string.Empty,
                    MaterialNo = cells[18].Element(XML_DATA)?.Value ?? string.Empty,
                    PO = cells[14].Element(XML_DATA)?.Value ?? string.Empty,
                    Quantity = decimal.TryParse(cells[22].Element(XML_DATA)?.Value ?? string.Empty, out decimal qty) ? qty : 0m,
                    ProductionPlanDate = DateTime.TryParse(cells[28].Element(XML_DATA)?.Value ?? string.Empty, out DateTime date) ? date : new DateTime()
                };
                result.Add(item);
                rowIndex += 1;
            }
            return result;
        }



        public Task ProcessZTFOutput(string ZTF003DataFolder, List<(int year, int month)> months, string saveFolderPath)
        {
            var cultrure = new CultureInfo("en-US");
            string[] dateFormat = { "dd.MM.yyyy" };
            var minDate = new DateTime(months.First().year, months.First().month, 1);
            var data = new List<ZTF003Output>();
            var files = new DirectoryInfo(ZTF003DataFolder).GetFiles();
            foreach (var file in files)
            {
                context.Log($"Process XML File: {file.Name}");
                data.AddRange(ReadXML(file.FullName));
            }
            foreach (var item in data)
            {
                item.SetFactory();
            }
            Helper.ClassToExcelSheet(data, Path.Combine(saveFolderPath, "ZTF003Data.xlsx"));
            context.Log($"Complete Update Factory");
            var group = data
                .Where(x => !string.IsNullOrEmpty(x.Factory) && x.ProductionPlanDate >= minDate)
                .GroupBy(x => (x.Factory, x.PO, x.MaterialNo, x.Color, ProductPlanYear: x.ProductionPlanDate.Year, ProductPlanMonth: x.ProductionPlanDate.Month))
                .Select(x => new RPA12Summary
                {
                    Factory = x.Key.Factory,
                    PO = x.Key.PO,
                    MaterialNo = x.Key.MaterialNo,
                    Color = x.Key.Color,
                    ProductPlanYear = x.Key.ProductPlanYear,
                    ProductPlanMonth = x.Key.ProductPlanMonth,
                    TotalAmount = x.Sum(y => y.AmountHKD)
                });
            context.Log($"Complete Calculate Total Amount Each Month");
            Helper.ClassToExcelSheet(group, Path.Combine(saveFolderPath, "Total Amount Each Month.xlsx"));

            var count = 0;
            var total = group.Count();
            foreach (var item in group)
            {
                item.TotalFutureAmount = group.Where(z => z.Factory == item.Factory
                                        && z.PO == item.PO
                                        && z.MaterialNo == item.MaterialNo
                                        && z.Color == item.Color
                                        && (z.ProductPlanYear > item.ProductPlanYear 
                                        || (z.ProductPlanYear == item.ProductPlanYear
                                            && z.ProductPlanMonth > item.ProductPlanMonth)))
                            .Sum(z => z.TotalAmount);
                count += 1;
                if(count % 1000 == 0)
                {
                    context.Log($"Processed {count}/{total}");
                }
            }
            Helper.ClassToExcelSheet(group, Path.Combine(saveFolderPath, "Calculate Future Amount.xlsx"));
            context.Log($"Complete Calculate Future Amount");
            var grouplevel3 = group
                .Where(x => months.Any(y => y.month == x.ProductPlanMonth
                                    && y.year == x.ProductPlanYear))
                .GroupBy(x => (x.Factory, x.ProductPlanYear, x.ProductPlanMonth))
                .Select(x => new
                {
                    x.Key.Factory,
                    x.Key.ProductPlanYear,
                    x.Key.ProductPlanMonth,
                    TotalFutureAmount = x.Sum(y => y.TotalFutureAmount)
                });
            Helper.ClassToExcelSheet(grouplevel3, Path.Combine(saveFolderPath, "GroupLevel3.xlsx"));
            return Task.CompletedTask;
        }
    }
}
