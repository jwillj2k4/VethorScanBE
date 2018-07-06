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
        public Task<decimal> GetCurrentVetPrice()
        {
            var result = _calculatorManager.GetCurrentVetPrice();
            return result;
        }

        [HttpGet("GetVetMetadata")]
        public Task<VetMetaDataDto> GetVetMetadata()
        {
            var result = _calculatorManager.GetVetMetadata();
            return result;
        }

        [NeverCache]
        [HttpPost("CalculateSimple")]
        public List<Task<UserVetResultDto>> CalculateSimple(decimal totalVetAmount)
        {
            var result = _calculatorManager.CalculateSimple(totalVetAmount);
            return result;
        }

        [NeverCache]
        [HttpPost("CalculateAdvanced")]
        public Task<IEnumerable<Task<UserVetResultDto>>> CalculateAdvanced(UserVetAmountsDto informationDto)
        {
            var result = _calculatorManager.CalculateAdvanced(informationDto);
            return result;
        }
    }
}
