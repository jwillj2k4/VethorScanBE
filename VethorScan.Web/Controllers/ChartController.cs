using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VethorScan.AppMgr;
using VethorScan.Common.CacheProfiles;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    public class ChartController : BaseController
    {
        private readonly CalculatorManager _calculatorManager;

        public ChartController(CalculatorManager calculatorManager)
        {
            _calculatorManager = calculatorManager;
        }

        [HttpGet("GetTransactionHistoryChart")]
        public async Task<object> GetTransactionHistoryChart()
        {
            return new object();
        }
    }
}
