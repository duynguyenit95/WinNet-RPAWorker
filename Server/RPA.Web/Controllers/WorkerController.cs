using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using RPA.Core.Data;
using Microsoft.EntityFrameworkCore;
using RPA.Core;
using Newtonsoft.Json;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace RPA.Web.Controllers
{
    public class WorkerController : Controller
    {
        private readonly ILogger<WorkerController> _logger;
        private readonly string _workersFilePath;
        private readonly RPAContext context;


        public WorkerController(ILogger<WorkerController> logger
            , RPAContext context,IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _workersFilePath = System.IO.Path.Combine(webHostEnvironment.WebRootPath,"Worker");
            if (!Directory.Exists(_workersFilePath))
            {
                Directory.CreateDirectory(_workersFilePath);
            }
            this.context = context;
        }

        [HttpGet]
        [Route("/api/worker/getall")]
        public async Task<IActionResult> GetAllWorker()
        {
            var data = await context.WorkerInfors.AsNoTracking().ToListAsync();
            return Ok(data);
        }

        [HttpGet]
        [Route("/api/worker/{workerName}")]
        public async Task<IActionResult> GetWorker(string workerName)
        {
            var data = await context.WorkerInfors.AsNoTracking().Where(x => x.Name == workerName).SingleOrDefaultAsync();
            return Ok(data);
        }

        [HttpPost]
        [Route("/api/worker/addorupdate")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> AddOrUpdateWorker(WorkerInfor info, IFormFileCollection files)
        {
            try
            {
                if (files.Count < 1)
                {
                    return BadRequest("No File");
                };
                var file = files.First();
                string filePath = Path.Combine(_workersFilePath, file.FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                var worker = await context.WorkerInfors.Where(x => x.Name == info.Name).SingleOrDefaultAsync();
                if (worker != null)
                {
                    var currentFile = Path.Combine(_workersFilePath, worker.FileName);
                    if (System.IO.File.Exists(currentFile))
                    {
                        System.IO.File.Delete(currentFile);
                    }
                    worker.Version = info.Version;
                    worker.FileName = file.FileName;
                    worker.LastUpdatedTime = DateTime.Now;

                    await context.SaveChangesAsync();
                    return Ok(worker);
                }
                else
                {
                    info.FileName = file.FileName;
                    context.WorkerInfors.Add(info);
                    await context.SaveChangesAsync();
                    return Ok(info);
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.ToString());
            }
            
        }
    }
}
 