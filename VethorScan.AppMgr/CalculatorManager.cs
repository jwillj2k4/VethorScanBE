using System.Threading.Tasks;
using VethorScan.Contracts;
using VethorScan.Domain.Vet;

namespace VethorScan.AppMgr
{
    public class CalculatorManager
    {
        private readonly IVetSystem _vetSystem;

        public CalculatorManager(IVetSystem vetSystem)
        {
            _vetSystem = vetSystem;
        }

        public async Task<double> GetCurrentVetPrice()
        {
            var metadata = await _vetSystem.GetVetMetadata().ConfigureAwait(false);

            return metadata.CurrentfVetPrice;
        }

        public Task<VetInformationDto> GetVetMetadata()
        {
            return _vetSystem.GetVetMetadata();
        }

        public async Task<AccountInformationDto> CalculateSimple(VetInformationDto informationDto)
        {
            //calculate the price of vet based on the current rate * the arg amount
            var currentVetPrice = await GetCurrentVetPrice().ConfigureAwait(false);

            var result = new AccountInformationDto { TotalAmount = informationDto.TotalVetAmount * currentVetPrice };

            return result;
        }

        public async Task<AccountInformationDto> CalculateAdvanced(VetInformationDto informationDto)
        {
            //use the following formula to calculate the total price of vet and thor
            //project 3/5/10 years for each

            var currentVetPrice = await GetCurrentVetPrice().ConfigureAwait(false);



        }
    }
}
