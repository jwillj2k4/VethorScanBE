using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using VethorScan.Contracts;
using VethorScan.Domain.Vet;

namespace VethorScan.AppMgr
{
    public class CalculatorManager
    {
        private readonly IVetSystem _vetSystem;
        private readonly IMemoryCache _memCache;
        private readonly int _secondsPerDay;

        public CalculatorManager(IVetSystem vetSystem, IMemoryCache memCache)
        {
            _vetSystem = vetSystem;
            _memCache = memCache;
            _secondsPerDay = 86400;
        }
        
        public async Task<VetMetaDataDto> GetVetMetadata()
        {
            return await _memCache.GetOrCreateAsync("metadata", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                return _vetSystem.GetVetMetadata();
            });
        }

        public async Task<decimal> GetCurrentVetPrice()
        {
            var vetInformationDto = await GetVetMetadata().ConfigureAwait(false);

            return vetInformationDto.Data.Quotes.Usd.Price;
        }

        public async Task<UserVetResultDto> CalculateSimple(decimal totalVetAmount)
        {
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            //set the vetdto total amount
            var result = new UserVetResultDto
            {
                VetResultDto = {TotalProfit = totalVetAmount * metadata.Data.Quotes.Usd.Price}
            };

            return result;
        }

        public async Task<UserVetResultDto> CalculateAdvanced(UserVetAmountsDto userVetInformation)
        {
            var result = await CalculateSimple(userVetInformation.UserVetAmount).ConfigureAwait(false);

            result.VeThorResultDto = await CalculateVeThorAmounts(userVetInformation).ConfigureAwait(false);

            return result;
        }

        private async Task<VeThorResultDto> CalculateVeThorAmounts(UserVetAmountsDto userVetInformation)
        {
            VeThorResultDto result =
                new VeThorResultDto
                {
                    AmountPerDay = await CalculateVeThorDayAmount(userVetInformation).ConfigureAwait(false),
                    ProfitPerDay = await CalculateVeThorProfitPerDay(userVetInformation).ConfigureAwait(false)
                };

            result.ProfitPerWeek = result.ProfitPerDay * 7;
            result.ProfitPerMonth = result.ProfitPerWeek * 52 / 12;
            result.ProfitPerYear = result.ProfitPerMonth * 12;
            result.Profit3Year = result.ProfitPerYear * 3;
            result.Profit5Year = result.ProfitPerYear * 5;
            result.Profit10Year = result.ProfitPerYear * 10;

            return result;
        }

        /// <summary>
        /// Calculate Vet profit per day
        /// </summary>
        /// <param name="userVetInformation"></param>
        /// <returns></returns>
        private async Task<decimal> CalculateVeThorProfitPerDay(UserVetAmountsDto userVetInformation)
        {
            //10,000 Vet(Vet holding) / 525,770,505 = 0.0019 % of Vet Circulating Supply
            //Company willing to pay $0.5 per transaction on blockchain, smart contract execution
            //Let’s say ecosystem is running 100mm transactions daily
            //=$50mm VeThor
            //0.0019 % multiplied by $50,000,000 = $950 daily

            var metadata = await GetVetMetadata().ConfigureAwait(false);

            var percentageOfEcoSystem = userVetInformation.UserVetAmount / metadata.Data.CirculatingSupply;
            var transactionsPerDay = userVetInformation.TransactionsPerSecond * _secondsPerDay;
            var totalVethor = transactionsPerDay * userVetInformation.CurrentThorPrice;

            return percentageOfEcoSystem * totalVethor;
        }

        /// <summary>
        /// Calclate vethor day amount
        /// </summary>
        /// <param name="userVetInformation"></param>
        /// <returns></returns>
        private async Task<decimal> CalculateVeThorDayAmount(UserVetAmountsDto userVetInformation)
        {
            //Vethor generation is calculated by the following
            //total vet amount * the rate of vethor to vet generated per day
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            return userVetInformation.UserVetAmount * metadata.Data.VetToThorRate;
        }
    }
}
