using Microsoft.AspNetCore.Mvc;
using VethorScan.Contracts;

namespace VethorScan.Web.Controllers
{
    [Route("api/[controller]")]
    public class CalculatorController : BaseController
    {

        [HttpGet("GetCirculatingSupply")]
        public CirculatingSupplyDto GetTotalCirculatingSupply()
        {
            return new CirculatingSupplyDto();
        }

        [HttpGet("Calculate")]
        public AccountInformationDto GetTotalCalculation(VetInformationDto informationDto)
        {
            return new AccountInformationDto();
        }

    }
}
