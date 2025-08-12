using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using RPA.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace RPA.Web.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ILogger<SupplierController> _logger;
        private readonly RPAContext context;


        public SupplierController(ILogger<SupplierController> logger
            , RPAContext context)
        {
            _logger = logger;
            this.context = context;
        }

        [Route("/api/supplier/{supplierID?}")]
        public async Task<IActionResult> GetSupplier(int supplierID = 0)
        {
            var data = await context.Suppliers.Where(x => x.ID == supplierID).FirstOrDefaultAsync();
            return Ok(data);
        }
    }
}
