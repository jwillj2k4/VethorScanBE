using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VethorScan.AppMgr;
using VethorScan.Common.CacheProfiles;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly CalculatorManager _calculatorManager;

        public TransactionController(CalculatorManager calculatorManager)
        {
            _calculatorManager = calculatorManager;
        }

        [HttpGet("GetTransactions")]
        public async Task<object> GetTransactions()
        {
            return new object();
        }
    }
}
