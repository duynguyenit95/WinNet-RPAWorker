using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using RPA.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using RPA.Web.Hubs;
using RPA.Core;
using Microsoft.AspNetCore.Authorization;
using System;
using SharpCifs.Netbios;

namespace RPA.Web.Controllers
{
    [Authorize]
    public class HubController : Controller
    {
        private readonly ILogger<HubController> _logger;
        private readonly WorkerConnectionFactory _workerConnectionFactory;
        private readonly IHubContext<WorkerHubBase> _hubContext;
        private readonly RPAContext _context;


        public HubController(ILogger<HubController> logger
            , WorkerConnectionFactory workerConnectionFactory
            , IHubContext<WorkerHubBase> hubContext
            , RPAContext context)
        {
            _logger = logger;
            _workerConnectionFactory = workerConnectionFactory;
            _hubContext = hubContext;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Report()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Report2()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult GetWorkers()
        {
            return Ok(_workerConnectionFactory.GetConnections());
        }
        [AllowAnonymous]
        public async Task<IActionResult> ExecuteSCC(string orderId)
        {   
            var workerConnection = _workerConnectionFactory.GetConnections().Where(x => x.Name == "SCCWorker").FirstOrDefault();
            if (workerConnection == null) return BadRequest();
            await _hubContext.Clients.Client(workerConnection.ConnectionId).SendAsync(WorkerActions.Execute, orderId);
            return Ok();
        }
        [AllowAnonymous]
        [Route("/zppe031/{irpeid}")]
        public async Task<IActionResult> ExecuteZPPE031(int irpeId)
        {   
            var workerConnection = _workerConnectionFactory.GetConnections().Where(x => x.Name == "Zppe031Calculation").FirstOrDefault();
            if (workerConnection == null) return BadRequest();
            await _hubContext.Clients.Client(workerConnection.ConnectionId).SendAsync(WorkerActions.Execute, irpeId);
            return Ok();
        }
        public async Task<IActionResult> ExecuteWorker(string workerConnectionId, string jsonData)
        {
            await _hubContext.Clients.Client(workerConnectionId).SendAsync(WorkerActions.Execute, jsonData);
            return Ok();
        }

        public async Task<IActionResult> GerberExport(string workerConnectionId)
        {
            await _hubContext.Clients.Client(workerConnectionId).SendAsync(WorkerActions.GerberExport);
            return Ok();
        }

        public async Task<IActionResult> ShowAbnormalForm(string workerConnectionId)
        {
            await _hubContext.Clients.Client(workerConnectionId).SendAsync(WorkerActions.CutMachineShowAbnormalForm);
            return Ok();
        }


        public async Task<IActionResult> UpdateWorkerVersion(string workerConnectionId, string workerVersion, string workerName)
        {
            var workerInfors = await _context.WorkerInfors
                .Where(x => x.Name == workerName)
                .AsNoTracking().FirstOrDefaultAsync();
            var v1 = new Version(workerInfors.Version);
            var v2 = new Version(workerVersion);
            var result = v1.CompareTo(v2);
            if (result > 0)
            {
                await _hubContext.Clients.Client(workerConnectionId).SendAsync(WorkerActions.WorkerVersionAutoUpdate);
                return Ok("Send request to Update");
            }
            else
            {
                return Ok("Worker is up to date");
            }
        }


        public async Task<IActionResult> UpdateDefaultParam([FromBody] string connectionId, [FromBody] string jsonData)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync(WorkerActions.UpdateParamater, jsonData);
            return Ok();
        }
    }
}
