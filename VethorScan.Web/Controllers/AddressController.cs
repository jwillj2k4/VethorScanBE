using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VethorScan.AppMgr;
using VethorScan.Common.CacheProfiles;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    public class AddressController : BaseController
    {
        private readonly CalculatorManager _calculatorManager;

        public AddressController(CalculatorManager calculatorManager)
        {
            _calculatorManager = calculatorManager;
        }

        [HttpGet("GetAddresses")]
        public async Task<object> GetAddresses()
        {
            return new object();
        }

    }
}
