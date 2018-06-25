using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VethorScan.AppMgr;
using VethorScan.Common.CacheProfiles;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    public class ContractController : BaseController
    {
        private readonly CalculatorManager _calculatorManager;

        public ContractController(CalculatorManager calculatorManager)
        {
            _calculatorManager = calculatorManager;
        }

        [HttpGet("GetContracts")]
        public async Task<object> GetContracts()
        {
            return new object();
        }
    }
}
