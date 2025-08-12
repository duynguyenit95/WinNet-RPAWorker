using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using RPA.Web.Common;
using RPA.Tools;
namespace RPA.Web.Controllers
{
    public class CommonController : Controller
    {
        private readonly ILogger<CommonController> _logger;
        private readonly IWebHostEnvironment webHostEnvirontment;


        public CommonController(ILogger<CommonController> logger
            , IWebHostEnvironment webHostEnvirontment)
        {
            _logger = logger;
            this.webHostEnvirontment = webHostEnvirontment;
        }

        [Route("/api/common/pdf/extractText/{fileName?}")]
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
    }
}
