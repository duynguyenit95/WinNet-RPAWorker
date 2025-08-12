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
    public class InvoiceController : Controller
    {
        private readonly ILogger<InvoiceController> _logger;
        private readonly IRegexParserServices _regexParserServices;
        private readonly IWebHostEnvironment _webHostEnvirontment;
        private readonly IFormRecognizerServices _formRecognizerServices;
        private readonly RPAContext _context;


        public InvoiceController(ILogger<InvoiceController> logger
            , IWebHostEnvironment webHostEnvirontment
            , IRegexParserServices regexParserServices
            , IFormRecognizerServices formRecognizerServices
            , RPAContext context)
        {
            _logger = logger;
            _webHostEnvirontment = webHostEnvirontment;
            _regexParserServices = regexParserServices;
            _formRecognizerServices = formRecognizerServices;
            _context = context;
        }

        [Route("/api/invoice/readHeader/{customerID?}/{fileName?}")]
        public async Task<IActionResult> ParseInvoiceHeader(int customerID = 0, string fileName = "")
        {
            using (var memoryStream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(memoryStream);
                var bytess = memoryStream.GetBuffer();
                var filePath = System.IO.Path.Combine(_webHostEnvirontment.WebRootPath, fileName);
                System.IO.File.WriteAllBytes(filePath, bytess);
                var ocrContent = ITextSharpPDFOCR.ReadFile(filePath);
                var regexs = await _context.RegexInfors.Where(x => x.CustomerID == customerID).ToListAsync();
                var parseResult = _regexParserServices.Parse(ocrContent, regexs);
                System.IO.File.Delete(filePath);
                return Ok(parseResult);
            }
        }

        [Route("/api/invoice/parseInvoice/{customerID?}/{modelID?}/{fileName?}")]
        public async Task<IActionResult> ParseInvoiceDocument(int customerID = 0, string modelID = ""
                                                        , string fileName = "")
        {
            using (var memoryStream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(memoryStream);
                var bytess = memoryStream.GetBuffer();
                var filePath = System.IO.Path.Combine(_webHostEnvirontment.WebRootPath, fileName);
                System.IO.File.WriteAllBytes(filePath, bytess);
                var data = await _formRecognizerServices.ParseInvoiceDocument(customerID, modelID, filePath);
                System.IO.File.Delete(filePath);
                return Ok(data);
            }
        }

    }
}
