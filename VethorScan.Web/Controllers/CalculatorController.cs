using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VethorScan.AppMgr;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    [Route("api/[controller]")]
    public class CalculatorController : BaseController
    {
        private readonly CalculatorManager _calculatorManager;

        public CalculatorController(CalculatorManager calculatorManager)
        {
            _calculatorManager = calculatorManager;
        }

        [HttpGet]
        public Task<double> GetCurrentVetPrice()
        {
            return _calculatorManager.GetCurrentVetPrice();
        }

        [HttpGet("GetVetMetadata")]
        public Task<VetInformationDto> GetVetMetadata()
        {
            return _calculatorManager.GetVetMetadata();
        }

        [HttpPost("CalculateSimple")]
        public Task<AccountInformationDto> CalculateSimple(VetInformationDto informationDto)
        {
            return _calculatorManager.CalculateSimple(informationDto);
        }

        [HttpPost("CalculateAdvanced")]
        public Task<AccountInformationDto> CalculateAdvanced(VetInformationDto informationDto)
        {
            return _calculatorManager.CalculateAdvanced(informationDto);
        }

    }
}
