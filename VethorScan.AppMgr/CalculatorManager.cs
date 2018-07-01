using System.Threading.Tasks;
using VethorScan.Contracts;

namespace VethorScan.AppMgr
{
    public class CalculatorManager
    {
        private readonly CalculatorService _calculatorService;

        public CalculatorManager(CalculatorService calculatorService)
        {
            this._calculatorService = calculatorService;
        }


        public async Task<VetMetaDataDto> GetVetMetadata()
        {
            var result = await _calculatorService.GetVetMetadata().ConfigureAwait(false);

            return result;
        }

        public async Task<decimal> GetCurrentVetPrice()
        {
            decimal price = await _calculatorService.GetCurrentVetPrice().ConfigureAwait(false);

            return price;
        }

        public async Task<UserVetResultDto> CalculateSimple(decimal totalVetAmount)
        {
            var result = await _calculatorService.CalculateSimple(totalVetAmount).ConfigureAwait(false);

            return result;
        }

        public async Task<UserVetResultDto> CalculateAdvanced(UserVetAmountsDto userVetAmountsDto)
        {
            var result = await _calculatorService.CalculateAdvanced(userVetAmountsDto).ConfigureAwait(false);

            return result;
        }
    }
}