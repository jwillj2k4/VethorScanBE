using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VethorScan.AppMgr;
using VethorScan.Common.CacheProfiles;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    public class AssetController : BaseController
    {
        private readonly CalculatorManager _calculatorManager;

        public AssetController(CalculatorManager calculatorManager)
        {
            _calculatorManager = calculatorManager;
        }

        [HttpGet("GetAssets")]
        public async Task<object> GetAssets()
        {
            return new object();
        }
    }
}
