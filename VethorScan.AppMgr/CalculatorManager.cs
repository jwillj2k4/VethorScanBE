using System.Collections.Generic;
using System.Threading.Tasks;
using VethorScan.Contracts;

namespace VethorScan.AppMgr
{
    public class CalculatorManager
    {
        private readonly CalculatorService _calculatorService;

        public CalculatorManager(CalculatorService calculatorService)
        {
            _calculatorService = calculatorService;
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

        public List<Task<UserVetResultDto>> CalculateSimple(decimal totalVetAmount)
        {
            var result = _calculatorService.CalculateSimple(totalVetAmount);

            return result;
        }

        public Task<IEnumerable<Task<UserVetResultDto>>> CalculateAdvanced(UserVetAmountsDto userVetAmountsDto)
        {
            var result = _calculatorService.CalculateAdvanced(userVetAmountsDto);

            return result;
        }
    }
}