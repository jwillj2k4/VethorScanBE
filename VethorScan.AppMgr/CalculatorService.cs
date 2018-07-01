using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using VethorScan.Contracts;
using VethorScan.Domain.Vet;

namespace VethorScan.AppMgr
{
    public class CalculatorService
    {

        private readonly IVetSystem _vetSystem;
        private readonly IMemoryCache _memCache;
        private readonly int _secondsPerDay;

        public readonly Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>, Func<UserVetAmountsDto, Task<UserVetResultDto>>> CalculationHash;
            
            
        public CalculatorService(IMemoryCache memCache, IVetSystem vetSystem)
        {
            _vetSystem = vetSystem;
            _memCache = memCache;
            _secondsPerDay = 86400;
            CalculationHash = IntializeCalculationDictionary();
        }

        private Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>, 
            Func<UserVetAmountsDto, Task<UserVetResultDto>>> IntializeCalculationDictionary()
        {
            return new Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
                Func<UserVetAmountsDto, Task<UserVetResultDto>>>
            {
                //non node
                {x => !DetermineIfNode(x, 10000, 1000000, 50000, 5000000), CalculateThor},

                //strength
                {x => DetermineIfNode(x, 10000, 1000000, 50000, 5000000), y => CalculateThor(y, 5000, 100)},

                //thunder
                {x => DetermineIfNode(x, 50000, 5000000, 150000, 15000000), y => CalculateThor(y, 1600, 150)},

                //Mjolnir
                {x => DetermineIfNode(x, 150000, 15000000, 250000, 25000000), y => CalculateThor(y, 667, 200)},

                //vethor x
                {x => DetermineIfNode(x, 6000, 600000, 16000, 1600000), y => CalculateThor(y, 667, 200)},

                //strenght x
                {x => DetermineIfNode(x, 16000, 1600000, 56000, 5600000), y => CalculateThor(y, 667, 200)},

                //thunder x
                {x => DetermineIfNode(x, 56000, 5600000, 156000, 15600000), y => CalculateThor(y, 667, 200)},

                //mjolnir x
                {x => DetermineIfNode(x, 156000, 15600000, 256000, 25600000), y => CalculateThor(y, 667, 200)},

                //thrudheim
                {x => DetermineIfNode(x, 256000, 25600000, 999999999, 999999999), y => CalculateThor(y, 667, 200)},
            };
        }
        
        public async Task<UserVetResultDto> CalculateSimple(decimal totalVetAmount)
        {
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            //set the vetdto total amount
            var result = new UserVetResultDto
            {
                VetResultDto = { TotalProfit = totalVetAmount * metadata.Data.Quotes.Usd.Price }
            };

            return result;
        }

        public async Task<UserVetResultDto> CalculateAdvanced(UserVetAmountsDto userVetInformation)
        {
            //Vethor generation is calculated by the following
            //total vet amount * the rate of vethor to vet generated per day
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            userVetInformation.CirculatingSupply =
                userVetInformation.CirculatingSupply == 0 ? metadata.Data.CirculatingSupply : userVetInformation.CirculatingSupply;

            var result = await CalculateSimple(userVetInformation.UserVetAmount).ConfigureAwait(false);

            result.VeThorBaseResultDto = await CalculateBaseVeThorAmounts(userVetInformation).ConfigureAwait(false);

            return result;
        }

        public async Task<decimal> GetCurrentVetPrice()
        {
            var vetInformationDto = await GetVetMetadata().ConfigureAwait(false);

            return vetInformationDto.Data.Quotes.Usd.Price;
        }

        public async Task<VetMetaDataDto> GetVetMetadata()
        {
            return await _memCache.GetOrCreateAsync("metadata", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                return _vetSystem.GetVetMetadata();
            });

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
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            return userVetInformation.UserVetAmount * metadata.Data.VetToThorRate;
        }

        private async Task<VeThorResultDto> CalculateBaseVeThorAmounts(UserVetAmountsDto userVetInformation)
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

        private static bool DetermineIfNode(KeyValuePair<SplitType, decimal> keyVal, int preSplitMin, int postSplitMin, int preSplitMax, int postSplitMax)
        {
            var result = keyVal.Key.Equals(SplitType.BeforeSplit)
                ?
                //if value is greater or equal to minimum and less than maximum presplit
                keyVal.Value >= preSplitMin && keyVal.Value < preSplitMax

                //if value is greater or equal to minimum and less than maximum post
                : keyVal.Value >= postSplitMin && keyVal.Value < postSplitMax;

            return result;
        }

        private async Task<UserVetResultDto> CalculateThor(UserVetAmountsDto userVetAmountsDto)
        {
            UserVetResultDto result = new UserVetResultDto();

            var metadata = await GetVetMetadata().ConfigureAwait(false);

            result.VeThorBaseResultDto.AmountPerDay = userVetAmountsDto.UserVetAmount * metadata.Data.VetToThorRate;

            return result;
        }
        
        private static async Task<UserVetResultDto> CalculateThor(UserVetAmountsDto userVetAmountsDto, int amountofHodlers, int percentageOfVet)
        {
            UserVetResultDto result = null;
            return result;
        }
    }
}