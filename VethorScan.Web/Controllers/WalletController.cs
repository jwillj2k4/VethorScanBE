using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VethorScan.AppMgr;
using VethorScan.Common.CacheProfiles;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    public class WalletController : BaseController
    {
        private readonly CalculatorManager _calculatorManager;

        public WalletController(CalculatorManager calculatorManager)
        {
            _calculatorManager = calculatorManager;
        }

        [HttpGet("GetWalletInformation")]
        public async Task<object> GetWalletInformation(string address)
        {
            return new object();
        }
    }
}
