using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VethorScan.AppMgr;
using VethorScan.Common.CacheProfiles;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    public class TokenController : BaseController
    {
        private readonly CalculatorManager _calculatorManager;

        public TokenController(CalculatorManager calculatorManager)
        {
            _calculatorManager = calculatorManager;
        }

        [HttpGet("GetTokens")]
        public async Task<object> GetTokens()
        {
            return new object();
        }
    }
}
