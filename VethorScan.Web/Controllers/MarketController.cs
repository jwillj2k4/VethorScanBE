using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VethorScan.AppMgr;
using VethorScan.Common.CacheProfiles;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    public class MarketController : BaseController
    {
        private readonly CalculatorManager _calculatorManager;

        public MarketController(CalculatorManager calculatorManager)
        {
            _calculatorManager = calculatorManager;
        }

        [HttpGet("GetData")]
        public async Task<object> GetMarketInformation()
        {
            return new object();
        }
    }
}
