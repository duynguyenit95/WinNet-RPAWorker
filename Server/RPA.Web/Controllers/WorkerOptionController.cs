using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using RPA.Core.Data;
using Microsoft.EntityFrameworkCore;
using RPA.Core;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using RPA.Web.Hubs;

namespace RPA.Web.Controllers
{

    public class WorkerOptionController : Controller
    {
        private readonly ILogger<WorkerOptionController> _logger;
        private readonly RPAContext context;
        private readonly WorkerConnectionFactory _workerConnectionFactory;
        private readonly IHubContext<WorkerHubBase> hubContext;
        private readonly string _token;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var headerToken = context.HttpContext.Request.Headers["Token"].ToString();
            if (User.Identity.IsAuthenticated || string.Equals(_token, headerToken))
            {
                base.OnActionExecuting(context);
            }
            else
            {
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new JsonResult("Access Denied");
            }
        }

        public WorkerOptionController(ILogger<WorkerOptionController> logger
            , RPAContext context
            , IConfiguration configuration
            , WorkerConnectionFactory workerConnectionFactory
            , IHubContext<WorkerHubBase> hubContext)
        {
            _logger = logger;
            this.context = context;
            this._workerConnectionFactory = workerConnectionFactory;
            this.hubContext = hubContext;
            this._token = configuration["WorkerToken"];
        }


        [HttpGet]
        [Route("/api/workeroption/getall")]
        public async Task<IActionResult> GetAllWorkers()
        {
            var data = await context.WorkerConfigurations
                            .AsNoTracking()
                            .ToListAsync();
            return Ok(data);
        }


        /// <summary>
        /// Return current Worker Opitions saved on database
        /// </summary>
        /// <param name="group"></param>
        /// <param name="workerName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/workeroption/get/{group}/{workerName}")]
        public async Task<IActionResult> GetWorkerOptions(string group, string workerName)
        {
            var config = await context.WorkerConfigurations
                            .Where(x => x.Name == workerName && x.Group == group)
                            .SingleOrDefaultAsync();
            if (config != null)
            {
                return Ok(config.JSONOptions);
            }
            else
            {
                return Ok();
            }

        }

        [HttpGet]
        [Route("/api/workeroption/get/{workerId}")]
        public async Task<IActionResult> GetWorkerOptionsById(Guid workerId)
        {
            var config = await context.WorkerConfigurations
                            .Where(x => x.WorkerId == workerId)
                            .SingleOrDefaultAsync();
            if (config != null)
            {
                return Ok(config.JSONOptions);
            }
            else
            {
                return Ok();
            }

        }

        /// <summary>
        /// Call by user only
        /// Handle User Request to Update Worker Options
        /// </summary>
        /// <param name="group"></param>
        /// <param name="workerName"></param>
        /// <param name="jsonValues"></param>
        /// <param name="requestConnectionId"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("/api/workeroption/requestupdate")]
        public async Task<IActionResult> RequestUpdateWorkerOptions(string workerConnectionId,
            string jsonValues, string requestConnectionId)
        {
            var workerConID = _workerConnectionFactory
                .GetByConnectionId(workerConnectionId);
            if (workerConID != null)
            {
                await hubContext.Clients
                    .Client(workerConnectionId)
                    .SendAsync(WorkerActions.ValidateOptions, requestConnectionId, jsonValues);
                return Ok();
            }
            else
            {
                return BadRequest("Worker is not online");
            }
        }

        /// <summary>
        /// Call by Worker Only
        /// Save Options to database after finish validation
        /// </summary>
        /// <param name="group"></param>
        /// <param name="workerName"></param>
        /// <param name="jsonValues"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/api/workeroption/register")]
        public async Task<IActionResult> UpdateWorkerOptions(Guid workerId, string group, string workerName, string jsonValues)
        {
            var config = await context.WorkerConfigurations.SingleOrDefaultAsync(x =>
                    (x.Name == workerName && x.Group == group)
                    || x.WorkerId == workerId);
            if (config != null)
            {
                config.JSONOptions = jsonValues;
                config.LastUpdatedTime = DateTime.Now;
                await context.SaveChangesAsync();
            }
            else
            {
                var newConfig = new WorkerConfiguration()
                {
                    Name = workerName,
                    Group = group,
                    JSONOptions = jsonValues,
                    WorkerId = workerId,
                };
                context.Add(newConfig);
            }
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
