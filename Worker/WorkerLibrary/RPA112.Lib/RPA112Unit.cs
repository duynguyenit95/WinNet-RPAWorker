using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RPA.Core;
using RPA.Tools;
using RPA.Worker.Core;
using SharpCifs.Smb;
using System.IO.Compression;
using System.Data;
using System.Text;
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper;

namespace RPA112.Lib
{
    public class RPA112Unit
    {
        public Dictionary<string, string> colKey { get; set; }
        public RPA112Unit()
        {
            colKey = new Dictionary<string, string>()
            {
                { "SupplierID", "供应商编码"},
                { "SupplierName", "供应商名称"},
                // 2022-09-22: Request by Zelda, change order
                { "CustomerName2", "客户名称"}, // 2022-09-20 : Request by Zelda Use 客户名称 instead of 客户名称2
                { "CustomerNo", "客户代码"},
                { "DocumentNo", "采购凭证"},
                { "ShortText", "短文本"},
                { "ColorDescription", "网格值描述"},
                { "DocumentDate", "凭证日期"},
                { "RequiredDeliveryDate", "要求交货日期"},
                { "PlannedQuantity", "已计划数量"},
                { "UnitNo", "订单单位"},
                { "DeliveryDate", "交货日期"},
                { "SupplierConfirmDeliveryDate", "供应商确认交期"},
                { "AccumulatedReceivedQuantity", "实际到料总数"},
                { "LackQuantity", "实际欠料数"},
                { "MaterialProcess1", "物料进度1"},
                { "ExpectedComeInFactoryDate", "预计到厂期"},
                { "ProductionPlanDate", "生产计划期"},
                { "StyleNo", "成品物料号"},
                { "Season", "季节"},
                { "AddressID", "AddressID"},
                { "Emails", "Emails"},
            };
            SQLiteHelper.CreateDatabaseIfNotExists<RPA112Context, RPA112DBInitializer>(RPA112Context.DB_NAME);
        }
        public WorkerExecutionContext<WorkerOption> PreExecute(WorkerExecutionContext<WorkerOption> context, string LocalProcessFolder)
        {
            try
            {
                var auth = new NtlmPasswordAuthentication(context.WorkerOptions.NSFOpt.AuthDomain, context.WorkerOptions.NSFOpt.AuthUsername, context.WorkerOptions.NSFOpt.AuthPassword);
                var smbDir = new SmbFile(context.WorkerOptions.NSFOpt.InputFolderURI, auth);
                var yesterdayStr = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");
                var allFiles = smbDir.ListFiles($"采购跟踪报表_VN*.zip");
                var vnp01 = allFiles.Where(x => x.GetName().Contains("VNP01")).OrderByDescending(x => SmbExtensions.GetLocalModifiedDate(x)).FirstOrDefault();
                if (vnp01 != null)
                {
                    SmbExtensions.CopyNSFFileToLocal(auth, vnp01.GetPath(), Path.Combine(LocalProcessFolder, vnp01.GetName()));
                }

                var vnp02 = allFiles.Where(x => x.GetName().Contains("VNP02")).OrderByDescending(x => SmbExtensions.GetLocalModifiedDate(x)).FirstOrDefault();
                if (vnp02 != null)
                {
                    SmbExtensions.CopyNSFFileToLocal(auth, vnp02.GetPath(), Path.Combine(LocalProcessFolder, vnp02.GetName()));
                }

                var vnp04 = allFiles.Where(x => x.GetName().Contains("VNP04")).OrderByDescending(x => SmbExtensions.GetLocalModifiedDate(x)).FirstOrDefault();
                if (vnp04 != null)
                {
                    SmbExtensions.CopyNSFFileToLocal(auth, vnp04.GetPath(), Path.Combine(LocalProcessFolder, vnp04.GetName()));
                }

            }
            catch (Exception ex)
            {
                context.Log("Error: " + ex);
                context.IsSuccess = false;
            }
            return context;
        }

        public async Task<List<RPA112Data>> ProcessFile(string file, WorkerExecutionContext<WorkerOption> context)
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
            List<RPA112Data> list;
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
                    list = dataTable.ToClass<RPA112Data>(colKey, cultrure, dateFormat);
                }
            }

            context.Log($" Read XLS File Complete !!!!");

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

            var exludePOs = new List<string>() { "516", "559", "575", "599", "739" };

            var filterList = list.Where(x => // Production Date filter, not null, not default, between fileDate + 22 day
                                            (x.ProductionPlanDate != null && x.ProductionPlanDate != DateTime.MinValue
                                            //x.ProductionPlanDate >= fileDate &&
                                            && x.ProductionPlanDate <= fileDate.AddDays(22))
                                            // PO Filter Condition
                                            && !exludePOs.Contains(x.DocumentNo.Substring(0, 3))
                                            // Lack Quantity
                                            && x.LackQuantity > 0
                                            );
            context.Log($" Production Date Filter, PO Filter, Lack Quantity Filter . {filterList.Count()} Records!!!!");

            var result = filterList.GroupBy(x => new
            {
                x.SupplierID,
                x.SupplierName,
                x.DocumentNo,
                x.ShortText,
                x.ColorDescription,
                x.RequiredDeliveryDate,
                x.SupplierConfirmDeliveryDate,
                x.ProductionPlanDate,
                //CustomerNo = x.StyleNo.Substring(0, 5),
                x.Season,
            }).Select(x => new RPA112Data()
            {
                SupplierID = x.Key.SupplierID,
                SupplierName = x.Key.SupplierName,
                DocumentNo = x.Key.DocumentNo,
                ShortText = x.Key.ShortText,
                CustomerNo = x.FirstOrDefault()?.StyleNo.Substring(0,5) ?? string.Empty,
                Season = x.Key.Season,
                ColorDescription = x.Key.ColorDescription,
                DocumentDate = x.Min(y => y.DocumentDate),
                RequiredDeliveryDate = x.Key.RequiredDeliveryDate,
                PlannedQuantity = x.Sum(y => y.PlannedQuantity),
                UnitNo = string.Join(",", x.Select(y => y.UnitNo).Distinct()),
                DeliveryDate = null,
                SupplierConfirmDeliveryDate = x.Key.SupplierConfirmDeliveryDate,
                AccumulatedReceivedQuantity = x.Sum(y => y.AccumulatedReceivedQuantity),
                LackQuantity = x.Sum(y => y.LackQuantity),
                MaterialProcess1 = string.Join(",", x.Select(y => y.MaterialProcess1).Distinct()),
                ExpectedComeInFactoryDate = x.Min(y => y.ExpectedComeInFactoryDate),
                ProductionPlanDate = x.Key.ProductionPlanDate,
                CustomerName2 = string.Join(",", x.Select(y => y.CustomerName2).Distinct()),
            }).OrderBy(x => x.SupplierID).ToList();

            context.Log($" Result . {result.Count()} Records!!!!");

            return result;
        }

        public Task CreateExcelFile(List<RPA112Data> result, string file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var extractPath = Path.Combine(Path.GetDirectoryName(file), fileName);
            Helper.ClassToExcelSheet(result, Path.Combine(extractPath, fileName + "_内部.xlsx"), colKey, new List<string>() { "StyleNo" });
            Helper.ClassToExcelSheet(result, Path.Combine(extractPath, fileName + "_外部.xlsx"), colKey
                                    , new List<string>() { "ExpectedComeInFactoryDate", "ProductionPlanDate", "CustomerName2", "AddressID", "Emails", "VendorName","StyleNo" });
            //context.Log($" Create Result Excel File Completed!!!!");
            var groups = result.Select(x => x.AddressID).Distinct();
            foreach (var group in groups)
            {
                var groupData = result.Where(x => x.AddressID == group);
                // Create Group Folder 
                var groupDirName = groupData.First().AddressID + "-" + groupData.First().VendorName;
                groupDirName = groupDirName.RemoveInvalidPathChars();
                var groupDir = Directory.CreateDirectory(Path.Combine(extractPath, groupDirName));
                Helper.ClassToExcelSheet(groupData, Path.Combine(groupDir.FullName, "内部.xlsx"), colKey, new List<string>() { "StyleNo" });
                Helper.ClassToExcelSheet(groupData, Path.Combine(groupDir.FullName, "外部.xlsx"), colKey
                                        , new List<string>() { "ExpectedComeInFactoryDate", "ProductionPlanDate", "CustomerName2", "AddressID", "Emails", "VendorName", "StyleNo" });
            }
            return Task.CompletedTask;
        }


        public WorkerExecutionContext<WorkerOption> AfterExecute(WorkerExecutionContext<WorkerOption> context, string LocalProcessFolder)
        {
            var auth = new NtlmPasswordAuthentication(context.WorkerOptions.NSFOpt.AuthDomain, context.WorkerOptions.NSFOpt.AuthUsername, context.WorkerOptions.NSFOpt.AuthPassword);
            SmbExtensions.CopyLocalFolderToNSF(auth, LocalProcessFolder, context.WorkerOptions.NSFOpt.SuccessFolderURI);
            SmbExtensions.DeleteAllFilesLocal(LocalProcessFolder);
            return context;
        }
    }
}
