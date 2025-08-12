using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RPA.Core.Data;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using RPA.Tools;
using MIR7InvoiceWorker.Model;
using SharpCifs.Smb;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
//using iText.Kernel.Geom;

namespace RPA.Web.Controllers
{
    public class PID135Controller : Controller
    {
        private readonly RPAInvoiceContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public PID135Controller(RPAInvoiceContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        public async void MIR7RecordProcess(PID135_InvoiceResult input)
        {
            input.SAPServer = "98";
            input.SAPAccount = "RPA_01";
            _context.PID135_InvoiceResults.Add(input);
            await _context.SaveChangesAsync();
        }
        public async void ZMMR0005RecordResult(List<PID135_ZMMR0005Result> results)
        {
            _context.PID135_ZMMR0005Results.AddRange(results);
            await _context.SaveChangesAsync();
        }
        [AllowAnonymous]
        public async Task<IActionResult> MIR7SendResult(DateTime? paramDate = null)
        {
            if(paramDate == null)
            {
                var n = DateTime.Now;
                paramDate = new DateTime(n.Year, n.Month, n.Day, 0, 0, 0);
            }
            try
            {
                //Collect SAP server name
                var sapServerPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "SAPServer.txt");
                var sapServerName = System.IO.File.ReadAllText(sapServerPath);

                //Db
                var processResult = await _context.PID135_InvoiceResults.Where(x => x.TimeBegin.Date == paramDate.Value.Date && x.TimeBegin >= paramDate).ToListAsync();
                var zmmmr0005Result = await _context.PID135_ZMMR0005Results.Where(x => x.UpdateTime.Date == paramDate.Value.Date && x.UpdateTime >= paramDate).ToListAsync();
                if (processResult.Count == 0) return Ok("No result");
                //List Mail
                var listMailPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "ListMailReceiver.txt");
                var listMail = System.IO.File.ReadAllLines(listMailPath);
                //Result Folder
                var resultFolderInfo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "ResultFolder.txt");
                var resultFolderPath = System.IO.File.ReadAllText(resultFolderInfo);
                //Auth
                var auth = new NtlmPasswordAuthentication("REGINAMIRACLE", "darius.nguyen", "Duycnt@1234#");
                //Template
                var mainResultTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "MIR7InvoiceResult.xlsx");
                var zmmr0005TemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "ZMMR0005Result.xlsx");
                //Email
                var sendMail = new SendMail("rpa05@reginamiracle.com", "Daj16028");

                //Each supplier need an email
                var customers = await _context.Customers.ToListAsync();
                var listSupplier = processResult.Select(x => new 
                {
                    SupplierId = x.SupplierId, 
                    SupplierName = x.SupplierName, 
                    //FolderPath = x.FolderName.Split("\\").Where(z => z.EndsWith(x.SupplierName)).FirstOrDefault().TrimEnd('.')
                }).Distinct().ToList();
                foreach (var supplier in listSupplier)
                {
                    var outputFile = new List<string>();
                    var mainResultSavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temporary", $"SummaryData-{paramDate.Value.ToString("yyyyMMdd")}-{supplier.SupplierName}-{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx");
                    if (!Directory.Exists(Path.GetDirectoryName(mainResultSavePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(mainResultSavePath));
                    }

                    var supplierProcessResult = processResult.Where(x => x.SupplierId == supplier.SupplierId).ToList();
                    Helper.CreateExcelSheetFromTemplate(supplierProcessResult, mainResultSavePath, mainResultTemplatePath, 3);
                    outputFile.Add(mainResultSavePath);

                    //ZMMR0005
                    var listReference = supplierProcessResult.Where(x => x.ResultType == "Balance").Select(x => x.Reference).Distinct().ToList();
                    if (listReference.Count > 0)
                    {
                        var zmmr0005SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temporary", $"ZMMR0005-{paramDate.Value.ToString("yyyyMMdd")}-{supplier.SupplierName}-{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx");
                        if (!Directory.Exists(Path.GetDirectoryName(zmmr0005SavePath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(zmmr0005SavePath));
                        }
                        Dictionary<string, List<PID135_ZMMR0005Result>> zmmmr0005 = zmmmr0005Result.GroupBy(x => x.Reference).ToDictionary(x => x.Key, x => x.ToList());
                        if (zmmmr0005.Count > 0)
                        {
                            Helper.CreateExcelMultiSheetFromTemplate(zmmmr0005, zmmr0005SavePath, zmmr0005TemplatePath, 3);
                            outputFile.Add(zmmr0005SavePath);
                        }
                    }

                    var subjectMail = $"[{sapServerName}]<br>[预制发票] 日常报告{paramDate.Value.ToString("yyyyMMdd")}_{supplier.SupplierId} {supplier.SupplierName}";
                    var contentMail = $"此电邮为【预制发票】日常运作报告<br>";
                    await HttpClientHelper.HttpSendMail(listMail, "PID135Invoice", subjectMail, contentMail, outputFile, "rpa05@reginamiracle.com", "Daj16028");

                    //Export file
                    try
                    {
                        var folderPath = customers.Where(x => x.SAPID == supplier.SupplierId).Select(x => x.ID + "." + x.Name).FirstOrDefault();
                        if (folderPath == null) continue;
                        var savePath = $"{resultFolderPath}/{folderPath}/06.Result/";
                        foreach (var item in outputFile)
                        {
                            var file = new FileInfo(item);
                            //file.MoveTo(savePath);
                            SmbExtensions.CopyLocalFileToNSF(auth, item, savePath + file.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        await HttpClientHelper.HttpSendMail(new[] { "darius.nguyen@reginamiracle.com" }, "PID135Invoice", "ERROR", ex.Message, new List<string>());
                        continue;
                    }

                }
                return Ok();
            }
            catch (Exception ex)
            {
                await HttpClientHelper.HttpSendMail(new[] { "darius.nguyen@reginamiracle.com" }, "PID135Invoice", "ERROR", ex.Message, null);
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Route("/pid135/uploadimage")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(file.FileName, out string contentType);
            if (file.Length > 0 && contentType != null && contentType.StartsWith("image/"))
            {
                var saveFolder = Path.Combine(webHostEnvironment.WebRootPath, "img");
                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }
                var webStaticPath = Path.Combine("img", file.FileName);
                var filePath = Path.Combine(saveFolder, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return Ok(webStaticPath);
            }
            return BadRequest();

        }

    }
}
