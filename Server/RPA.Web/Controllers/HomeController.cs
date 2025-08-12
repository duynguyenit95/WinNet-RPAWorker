using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RPA.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RPA.Core.Data;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using RPA.Web.Services;
using Microsoft.EntityFrameworkCore;
using RPA.Web.Common;
using RPA.Tools;
namespace RPA.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRegexParserServices _documentHeaderServices;
        private readonly IWebHostEnvironment webHostEnvirontment;
        private readonly IFormRecognizerServices _formRecognizerServices;
        private readonly IConfiguration configuration;
        private readonly RPAContext context;


        public HomeController(ILogger<HomeController> logger, IRegexParserServices regexServices
            , IWebHostEnvironment webHostEnvirontment
            , IFormRecognizerServices formRecognizerServices
            , IConfiguration configuration
            , RPAContext context)
        {
            _logger = logger;
            this._documentHeaderServices = regexServices;
            this.webHostEnvirontment = webHostEnvirontment;
            this._formRecognizerServices = formRecognizerServices;
            this.configuration = configuration;

            this.context = context;
        }
        [Route("/403")]
        public IActionResult Page403()
        {
            return View();
        }


        public ActionResult ReadDocHeader(IFormFile file)
        {
            var a = Request;
            using (var stream = file.OpenReadStream())
            {
                var ocrContent = ITextSharpPDFOCR.ReadFile(stream);
                var regexs = context.RegexInfors.ToList();
                var parseResult = _documentHeaderServices.Parse(ocrContent, regexs);
                return Ok(parseResult);
            }
        }
        [Route("/api/pdf/readHeader/")]
        public async Task<IActionResult> ReadDocHeaderRPASF()
        {
            using (var memoryStream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(memoryStream);
                var bytess = memoryStream.GetBuffer();
                var fileName = Guid.NewGuid().ToString();
                var filePath = System.IO.Path.Combine(webHostEnvirontment.WebRootPath, fileName);
                System.IO.File.WriteAllBytes(filePath, bytess);
                var ocrContent = ITextSharpPDFOCR.ReadFile(filePath);
                var regexs = context.RegexInfors.ToList();
                var parseResult = _documentHeaderServices.Parse(ocrContent, regexs);
                System.IO.File.Delete(filePath);
                return Ok(parseResult);
            }
        }

        [Route("/api/pdf/readHeader/{customerID?}/{fileName?}")]
        public async Task<IActionResult> ReadDocHeaderRPASFV2(int customerID = 0, string fileName = "")
        {
            using (var memoryStream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(memoryStream);
                var bytess = memoryStream.GetBuffer();
                var filePath = System.IO.Path.Combine(webHostEnvirontment.WebRootPath, fileName);
                System.IO.File.WriteAllBytes(filePath, bytess);
                var ocrContent = ITextSharpPDFOCR.ReadFile(filePath);
                var regexs = await context.RegexInfors.Where(x => x.CustomerID == customerID).ToListAsync();
                var parseResult = _documentHeaderServices.Parse(ocrContent, regexs);
                System.IO.File.Delete(filePath);
                return Ok(parseResult);
            }
        }

        [Route("/api/pdf/extractText/{fileName?}")]
        public async Task<IActionResult> ExtractText(string fileName = "")
        {
            using (var memoryStream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(memoryStream);
                var bytess = memoryStream.GetBuffer();
                var filePath = System.IO.Path.Combine(webHostEnvirontment.WebRootPath, fileName);
                System.IO.File.WriteAllBytes(filePath, bytess);
                var ocrContent = ITextSharpPDFOCR.ReadFile(filePath);
                System.IO.File.Delete(filePath);
                return Ok(ocrContent);
            }
        }

        [Route("/api/pdf/formrecognizer/{customerID?}/{modelID?}/{fileName?}")]
        public async Task<IActionResult> FormRecognizerDocumentParser(int customerID = 0, string modelID = ""
                                                                , string fileName = "")
        {
            using (var memoryStream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(memoryStream);
                var bytess = memoryStream.GetBuffer();
                var filePath = System.IO.Path.Combine(webHostEnvirontment.WebRootPath, fileName);
                System.IO.File.WriteAllBytes(filePath, bytess);
                var data = await _formRecognizerServices.ParseInvoiceDocument(customerID, modelID, filePath);
                System.IO.File.Delete(filePath);
                return Ok(data);
            }
        }

        [Route("/api/customer/{customerID?}")]
        public async Task<IActionResult> GetCustomer(int customerID = 0)
        {
            var data = await context.Suppliers.Where(x => x.ID == customerID).FirstOrDefaultAsync();
            return Ok(data);
        }

        public IActionResult Index()
        {
            var req = HttpContext.Request;
            return View();
        }

        public async Task<IActionResult> Test()
        {
            var dir = new DirectoryInfo(Path.Combine(webHostEnvirontment.ContentRootPath, "Invoice"));
            foreach (var file in dir.GetFiles().OrderBy(x => x.CreationTime))
            {
                await _formRecognizerServices.ParseInvoiceDocument(1, "2f3ba4c9-f22a-4047-ad5b-c55e254b245a", file.FullName);
            }
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

    }
}
