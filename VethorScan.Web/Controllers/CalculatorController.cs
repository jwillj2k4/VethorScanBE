using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VethorScan.AppMgr;
using VethorScan.Common.CacheProfiles;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    public class CalculatorController : BaseController
    {
        private readonly CalculatorManager _calculatorManager;

        public CalculatorController(CalculatorManager calculatorManager)
        {
            _calculatorManager = calculatorManager;
        }

        [HttpGet("GetVetPrice")]
        public async Task<decimal> GetCurrentVetPrice()
        {
            var result = await _calculatorManager.GetCurrentVetPrice().ConfigureAwait(false);
            return result;
        }

        [HttpGet("GetVetMetadata")]
        public async Task<VetMetaDataDto> GetVetMetadata()
        {
            var result = await _calculatorManager.GetVetMetadata().ConfigureAwait(false);
            return result;
        }

        [NeverCache]
        [HttpPost("CalculateSimple")]
        public async Task<UserVetResultDto> CalculateSimple(decimal totalVetAmount)
        {
            var result = await _calculatorManager.CalculateSimple(totalVetAmount).ConfigureAwait(false);
            return result;
        }

        [NeverCache]
        [HttpPost("CalculateAdvanced")]
        public async Task<IEnumerable<UserVetResultDto>> CalculateAdvanced(UserVetAmountsDto informationDto)
        {
            var result = await _calculatorManager.CalculateAdvanced(informationDto).ConfigureAwait(false);
            return result;
        }
    }
}
