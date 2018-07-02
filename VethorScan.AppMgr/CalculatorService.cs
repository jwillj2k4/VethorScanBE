using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
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

        private Dictionary<SplitType, Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
            Func<UserVetAmountsDto, Task<UserVetResultDto>>>> _vethorDictionary = null;

        public CalculatorService(IMemoryCache memCache, IVetSystem vetSystem)
        {
            _vetSystem = vetSystem;
            _memCache = memCache;
            _secondsPerDay = 86400;
            InitializeDictionary();
        }

        private void InitializeDictionary()
        {
            _vethorDictionary =
                new Dictionary<SplitType, Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
                    Func<UserVetAmountsDto, Task<UserVetResultDto>>>>
                {
                    {SplitType.PreSplit, IntializePreSplitCalculationDictionary()},
                    {SplitType.PreSplit, IntializePreSplitXnodeCalculationDictionary()},
                    {SplitType.PostSplit, IntializePostSplitCalculationDictionary()},
                    {SplitType.PostSplit, IntializePostSplitXnodeCalculationDictionary()}
                };
        }

        /// <summary>
        /// Pre split regular 
        /// </summary>
        /// <returns></returns>
        private Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
            Func<UserVetAmountsDto, Task<UserVetResultDto>>> IntializePreSplitCalculationDictionary()
        {
            return new Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
                Func<UserVetAmountsDto, Task<UserVetResultDto>>>
            {
                //non node
                {x => !DetermineIfNode(x, 10000, 50000),  y => CalculateThor(y, NodeType.None)},

                //strength
                {x => DetermineIfNode(x, 10000, 50000), y => CalculateThor(y, NodeType.Strength, 5000, 100)},

                //thunder
                {x => DetermineIfNode(x, 50000, 150000), y => CalculateThor(y, NodeType.Thunder, 1600, 150)},

                //Mjolnir
                {x => DetermineIfNode(x, 150000, 250000), y => CalculateThor(y, NodeType.Mjolnir, 667, 200)},

                //Thrudheim
                {x => DetermineIfNode(x, 250000), y => CalculateThor(y, NodeType.Thrudheim, 667, 200)},
            };
        }

        /// <summary>
        /// pre split xnode
        /// </summary>
        private Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>, 
            Func<UserVetAmountsDto, Task<UserVetResultDto>>> IntializePreSplitXnodeCalculationDictionary()
        {
            return new Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
                Func<UserVetAmountsDto, Task<UserVetResultDto>>>
            {
                //non node
                {x => !DetermineIfNode(x, 6000, 6000), y => CalculateThor(y, NodeType.None)},

                //vethor x
                {x => DetermineIfNode(x, 6000, 16000), y => CalculateThor(y, NodeType.VeThorX, 667, 200)},

                //strenght x
                {x => DetermineIfNode(x, 16000, 56000), y => CalculateThor(y, NodeType.StrengthX, 667, 200)},

                //thunder x
                {x => DetermineIfNode(x, 56000, 156000), y => CalculateThor(y, NodeType.ThunderX, 667, 200)},

                //mjolnir x
                {x => DetermineIfNode(x, 156000, 256000), y => CalculateThor(y, NodeType.MjolnirX, 667, 200)},

                //thrudheim
                {x => DetermineIfNode(x, 256000), y => CalculateThor(y, NodeType.Thrudheim, 667, 200)},
            };
        }

        /// <summary>
        /// post split regular
        /// </summary>
        private Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
            Func<UserVetAmountsDto, Task<UserVetResultDto>>> IntializePostSplitCalculationDictionary()
        {
            return new Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
                Func<UserVetAmountsDto, Task<UserVetResultDto>>>
            {
                //non node
                {x => !DetermineIfNode(x, 1000000, 0),  y => CalculateThor(y, NodeType.None)},

                //strength
                {x => DetermineIfNode(x, 1000000, 5000000), y => CalculateThor(y, NodeType.Strength, 5000, 100)},

                //thunder
                {x => DetermineIfNode(x, 5000000, 15000000), y => CalculateThor(y, NodeType.Thunder, 1600, 150)},

                //Mjolnir
                {x => DetermineIfNode(x, 15000000, 25000000), y => CalculateThor(y, NodeType.Mjolnir, 667, 200)},
                
                //thrudheim
                {x => DetermineIfNode(x, 25000000), y => CalculateThor(y, NodeType.Thrudheim, 667, 200)},
            };
        }

        /// <summary>
        /// post split xnode
        /// </summary>
        /// <returns></returns>
        private Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
            Func<UserVetAmountsDto, Task<UserVetResultDto>>> IntializePostSplitXnodeCalculationDictionary()
        {
            return new Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
                Func<UserVetAmountsDto, Task<UserVetResultDto>>>
            {
                //non node
                {x => !DetermineIfNode(x, 6000, 6000),  y => CalculateThor(y, NodeType.None)},

                //vethor x
                {x => DetermineIfNode(x, 600000, 1600000), y => CalculateThor(y, NodeType.VeThorX, 667, 200)},

                //strength x
                {x => DetermineIfNode(x, 1600000, 5600000), y => CalculateThor(y, NodeType.StrengthX, 667, 200)},

                //thunder x
                {x => DetermineIfNode(x, 5600000, 15600000), y => CalculateThor(y, NodeType.Thunder, 667, 200)},

                //mjolnir x
                {x => DetermineIfNode(x, 15600000, 25600000), y => CalculateThor(y, NodeType.MjolnirX, 667, 200)},

                //thrudheim
                {x => DetermineIfNode(x, 25600000), y => CalculateThor(y, NodeType.Thrudheim, 667, 200)},
            };
        }

        /// <summary>
        /// Calculates vet profit only
        /// </summary>
        /// <param name="totalVetAmount"></param>
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
            
        /// <summary>
        /// Calculates vet and vethor profit, node and xnodes included
        /// </summary>
        /// <param name="userVetInformation"></param>
        /// <returns></returns>
        public Task<IEnumerable<UserVetResultDto>> CalculateAdvanced(UserVetAmountsDto userVetInformation)
        {
            //depending on the split type, call the appropriate dictionary calculator and return
            IList<UserVetResultDto> result = new List<UserVetResultDto>();

            //for each matching dictionary, determine where the current user stands for their hodlings.
            _vethorDictionary[userVetInformation.Split].Values.ToList().ForEach(async x =>
            {
                //create a result dto for each group
                var dto = await x.Invoke(userVetInformation).ConfigureAwait(false);

                result.Add(dto);
            });

            return Task.FromResult(result.AsEnumerable());
        }

        /// <summary>
        /// returns the current price of vet
        /// </summary>
        /// <returns></returns>
        public async Task<decimal> GetCurrentVetPrice()
        {
            var vetInformationDto = await GetVetMetadata().ConfigureAwait(false);

            return vetInformationDto.Data.Quotes.Usd.Price;
        }

        /// <summary>
        /// returns vet metadata
        /// </summary>
        /// <returns></returns>
        public async Task<VetMetaDataDto> GetVetMetadata()
        {
            return await _memCache.GetOrCreateAsync("metadata", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                return _vetSystem.GetVetMetadata();
            });

        }
        
        /// <summary>
        /// determines if the passed arguments satisfy node criteria
        /// </summary>
        /// <param name="keyVal"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        private bool DetermineIfNode(KeyValuePair<SplitType, decimal> keyVal, int minimum = int.MinValue, int maximum = int.MaxValue)
        {
            var result =
                //if value is greater or equal to minimum and less than maximum presplit
                keyVal.Value >= minimum && maximum != int.MaxValue && keyVal.Value < maximum ||
                keyVal.Value >= minimum && maximum == int.MaxValue;

            return result;
        }

        /// <summary>
        /// calculates thor taking node and xnode into consideration
        /// </summary>
        /// <param name="userVetAmountsDto"></param>
        /// <param name="nodeType"></param>
        /// <param name="amountofHodlers"></param>
        /// <param name="percentageOfVet"></param>
        /// <returns></returns>
        private async Task<UserVetResultDto> CalculateThor(UserVetAmountsDto userVetAmountsDto, NodeType nodeType,
            int amountofHodlers = 0, int percentageOfVet = 0)
        {
            //Vethor generation is calculated by the following
            //total vet amount * the rate of vethor to vet generated per day
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            userVetAmountsDto.CirculatingSupply = await CalculateCirculatingSupply(userVetAmountsDto).ConfigureAwait(false);

            UserVetResultDto result = await CalculateSimple(userVetAmountsDto.UserVetAmount).ConfigureAwait(false);

            if (nodeType == NodeType.None)
            {
                result.VeThorResultDto = await CalculateProjectiveVeThorAmounts(userVetAmountsDto).ConfigureAwait(false);
                return result;
            }

            //calculate thor amount perday based on argueents 


            result.VeThorResultDto.NodeType = nodeType;

            return result;
        }

        /// <summary>
        /// determines the circulating supply, takes pre/post split into consideration
        /// </summary>
        /// <param name="userVetAmountsDto"></param>
        /// <returns></returns>
        private async Task<long> CalculateCirculatingSupply(UserVetAmountsDto userVetAmountsDto)
        {
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            return userVetAmountsDto.CirculatingSupply != 0
                ? userVetAmountsDto.CirculatingSupply
                : userVetAmountsDto.Split == SplitType.PostSplit
                    ?
                    //append 2 zeros at the end for post split, until the 9th.
                    DateTime.UtcNow.Month < 7 && DateTime.UtcNow.Day < 10 && DateTime.UtcNow.Year == 2018
                        ?
                        int.Parse(metadata.Data.CirculatingSupply.ToString().PadRight(2, '0'))
                        :
                        metadata.Data.CirculatingSupply

                    : metadata.Data.CirculatingSupply;
        }
        
        /// <summary>
        /// Calclate daily vethor amount
        /// </summary>
        /// <param name="userVetInformation"></param>
        /// <returns></returns>
        private async Task<decimal> CalculateDailyVeThorAmount(UserVetAmountsDto userVetInformation)
        {
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            return userVetInformation.UserVetAmount * metadata.Data.VetToThorRate;
        }

        /// <summary>
        /// Calculate projective vethor amounts
        /// </summary>
        /// <param name="userVetInformation"></param>
        /// <returns></returns>
        private async Task<VeThorResultDto> CalculateProjectiveVeThorAmounts(UserVetAmountsDto userVetInformation)
        {
            VeThorResultDto result =
                new VeThorResultDto
                {
                    AmountPerDay = await CalculateDailyVeThorAmount(userVetInformation).ConfigureAwait(false),
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
    }
}