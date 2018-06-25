using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VethorScan.AppMgr;
using VethorScan.Common.CacheProfiles;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    public class BlockController : BaseController
    {
        private readonly CalculatorManager _calculatorManager;

        public BlockController(CalculatorManager calculatorManager)
        {
            _calculatorManager = calculatorManager;
        }

        [HttpGet("GetBlocks")]
        public async Task<object> GetBlocks()
        {
            return new object();
        }
    }
}
