using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;
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
            Func<UserVetAmountsDto, Task<UserVetResultDto>>>> _vethorDictionary;
        
        private long _pledgeNodeVethorAmount;
        private long _pledgeXNodeVethorAmount;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="memCache"></param>
        /// <param name="vetSystem"></param>
        public CalculatorService(IMemoryCache memCache, IVetSystem vetSystem)
        {
            _vetSystem = vetSystem;
            _memCache = memCache;
            _secondsPerDay = 86400;
            InitializeDictionary();
        }

        /// <summary>
        /// initialize dictionary
        /// </summary>
        private void InitializeDictionary()
        {
            _vethorDictionary =
                new Dictionary<SplitType, Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
                    Func<UserVetAmountsDto, Task<UserVetResultDto>>>>
                {
                    {SplitType.PreSplit, IntializePreSplitCalculationDictionary()},
                    {SplitType.PreSplit, IntializePreSplitXnodeBonuDictionary()},
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
                {x => !DetermineIfNode(x, 10000),  y => CalculateThor(y, NodeType.None, 0)},

                //strength
                {x => DetermineIfNode(x, 10000, 50000), y => CalculateThor(y, NodeType.Strength, 0.00015, 1)},

                //thunder
                {x => DetermineIfNode(x, 50000, 150000), y => CalculateThor(y, NodeType.Thunder, 0.00015, 1.5)},

                //Mjolnir
                {x => DetermineIfNode(x, 150000), y => CalculateThor(y, NodeType.Mjolnir, 0.00015, 2)},

                //Thrudheim
                {x => DetermineIfNode(x, 250000), y => CalculateThor(y, NodeType.Thrudheim, 0.00015, 2)}
            };
        }

        /// <summary>
        /// pre split xnode
        /// </summary>
        private Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>, 
            Func<UserVetAmountsDto, Task<UserVetResultDto>>> IntializePreSplitXnodeBonuDictionary()
        {
            return new Dictionary<Func<KeyValuePair<SplitType, decimal>, bool>,
                Func<UserVetAmountsDto, Task<UserVetResultDto>>>
            {
                //non node
                {x => !DetermineIfNode(x, 6000), y => CalculateThor(y, NodeType.None, 0)},

                //vethor x
                {x => DetermineIfNode(x, 6000, 16000), y => CalculateThor(y, NodeType.VeThorX, 0.00015, .25)},

                //strenght x
                {x => DetermineIfNode(x, 16000, 56000), y => CalculateThor(y, NodeType.StrengthX, 0.00015, 1)},

                //thunder x
                {x => DetermineIfNode(x, 56000, 156000), y => CalculateThor(y, NodeType.ThunderX, 0.00015, 1.5)},

                //mjolnir x
                {x => DetermineIfNode(x, 156000), y => CalculateThor(y, NodeType.MjolnirX, 0.00015, 2)},
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
                {x => !DetermineIfNode(x, 1000000),  y => CalculateThor(y, NodeType.None, 0)},

                //strength
                {x => DetermineIfNode(x, 1000000, 5000000), y => CalculateThor(y, NodeType.Strength, 0.0003, 1)},

                //thunder
                {x => DetermineIfNode(x, 5000000, 15000000), y => CalculateThor(y, NodeType.Thunder, 0.0003, 1.5)},

                //Mjolnir
                {x => DetermineIfNode(x, 15000000), y => CalculateThor(y, NodeType.Mjolnir, 0.0003, 2)},
                
                //thrudheim
                {x => DetermineIfNode(x, 25000000), y => CalculateThor(y, NodeType.Thrudheim, 0.0003, 2)}
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
                {x => !DetermineIfNode(x, 600000),  y => CalculateThor(y, NodeType.None, 0)},

                //vethor x
                {x => DetermineIfNode(x, 600000, 1600000), y => CalculateThor(y, NodeType.VeThorX, 0.0003, .25)},

                //strength x
                {x => DetermineIfNode(x, 1600000, 5600000), y => CalculateThor(y, NodeType.StrengthX, 0.0003, 1)},

                //thunder x
                {x => DetermineIfNode(x, 5600000, 15600000), y => CalculateThor(y, NodeType.Thunder, 0.0003, 1.5)},

                //mjolnir x
                {x => DetermineIfNode(x, 15600000), y => CalculateThor(y, NodeType.MjolnirX, 0.0003, 2)}
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
                VetResultDto = { CurrentProfit = totalVetAmount * metadata.Data.Quotes.Usd.Price }
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
            _vethorDictionary[userVetInformation.Split]
                .Values.ToList()
                .ForEach(async x =>
                {
                    //create a result dto for each group
                    var dto = await x.Invoke(userVetInformation).ConfigureAwait(false);

                    result.Add(dto);
                });

            //since we only display 'none' vethor node types once,
            //and the result has the potential to contain 2 'none' node types due to each split type having a 'none' calculation,
            //always remove 1 'none' node type from the list
            UserVetResultDto resultToRemove = result.FirstOrDefault(x => x.VeThorResultDto.NodeType == NodeType.None);

            if(resultToRemove != null)
                result.Remove(resultToRemove);

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
        /// <param name="averageAmountOfNodeBaseRewards">average amount of base rewards base on the number of node hodlers across all nodes, just a estimate provided by vechain</param>
        /// <param name="percentageOfVet"></param>
        /// <returns></returns>
        private async Task<UserVetResultDto> CalculateThor(UserVetAmountsDto userVetAmountsDto, NodeType nodeType,
            double averageAmountOfNodeBaseRewards, double percentageOfVet = 0)
        {
            userVetAmountsDto.CirculatingSupply =
                await CalculateCirculatingSupply(userVetAmountsDto).ConfigureAwait(false);

            UserVetResultDto result = new UserVetResultDto();
            if (nodeType == NodeType.None)
            {
                result = await CalculateSimple(userVetAmountsDto.UserVetAmount).ConfigureAwait(false);

                result.VeThorResultDto =
                    await CalculateVeThorProfits(userVetAmountsDto).ConfigureAwait(false);
            }
            else
            {
                result.VetResultDto =
                    await CalculateVetTransactionalProfits(userVetAmountsDto).ConfigureAwait(false);

                result.VeThorResultDto = await CalculateVeThorProfits(userVetAmountsDto,
                    (decimal) averageAmountOfNodeBaseRewards, (decimal) percentageOfVet).ConfigureAwait(false);
            }

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
                        ? int.Parse(metadata.Data.CirculatingSupply.ToString().PadRight(2, '0'))
                        : metadata.Data.CirculatingSupply

                    : metadata.Data.CirculatingSupply;
        }

        /// <summary>
        /// Calculate projective vethor amounts
        /// </summary>
        /// <param name="userVetAmountsDto"></param>
        /// <param name="averageAmountOfNodeBaseRewards"></param>
        /// <param name="bonusPercentage"></param>
        /// <returns></returns>
        private async Task<VeThorResultDto> CalculateVeThorProfits(UserVetAmountsDto userVetAmountsDto,
            decimal averageAmountOfNodeBaseRewards = 0, decimal bonusPercentage = 0)
        {
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            VeThorResultDto result =
                new VeThorResultDto
                {
                    //calculate thor amount per day based on argueents 
                    //example: 0.00042 + (averageAmountOfNodeBaseRewards * 200%) = 0.000720 VeThor per VET a day or 0.2628 per Year;
                    AmountPerDay =
                        userVetAmountsDto.UserVetAmount *
                        (metadata.Data.VetToThorRate + averageAmountOfNodeBaseRewards * bonusPercentage)
                };

            //vethor profit per day is the amount you hold x price per vethor
            result.ProfitPerDay = metadata.Data.VetToThorRate * result.AmountPerDay;

            CalculateProjectedProfits(result);

            return result;
        }

        /// <summary>
        /// Calculate projective vethor amounts
        /// </summary>
        /// <param name="userVetAmountsDto"></param>
        /// <returns></returns>
        private async Task<VetResultDto> CalculateVetTransactionalProfits(UserVetAmountsDto userVetAmountsDto)
        {
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            VetResultDto result =
                new VetResultDto
                {
                    CurrentProfit = userVetAmountsDto.UserVetAmount * metadata.Data.Quotes.Usd.Price,

                    //vet profit per day is the (amount you hold / circulating supply) * transactionsPerDay * currentThorPrice
                    ProfitPerDay = await CalculateVetTransactionalProfitPerDay(userVetAmountsDto).ConfigureAwait(false)
                };

            CalculateProjectedProfits(result);

            return result;
        }

        /// <summary>
        /// Calculate projected amounts
        /// </summary>
        /// <param name="result"></param>
        private static void CalculateProjectedProfits(UserProfitDto result)
        {
            result.ProfitPerWeek = result.ProfitPerDay * 7;
            result.ProfitPerMonth = result.ProfitPerWeek * 52 / 12;
            result.ProfitPerYear = result.ProfitPerMonth * 12;
            result.Profit3Year = result.ProfitPerYear * 3;
            result.Profit5Year = result.ProfitPerYear * 5;
            result.Profit10Year = result.ProfitPerYear * 10;
        }

        /// <summary>
        /// Calculate Vet profit per day
        /// </summary>
        /// <param name="userVetAmountsDto"></param>
        /// <returns></returns>
        private async Task<decimal> CalculateVetTransactionalProfitPerDay(UserVetAmountsDto userVetAmountsDto)
        {
            //10,000 Vet(Vet holding) / 525,770,505 = 0.0019 % of Vet Circulating Supply
            //Company willing to pay $0.5 per transaction on blockchain, smart contract execution
            //Let’s say ecosystem is running 100mm transactions daily
            //=$50mm VeThor
            //0.0019 % multiplied by $50,000,000 = $950 daily

            var metadata = await GetVetMetadata().ConfigureAwait(false);

            var percentageOfEcoSystem = userVetAmountsDto.UserVetAmount / metadata.Data.CirculatingSupply;
            var transactionsPerDay = userVetAmountsDto.TransactionsPerSecond * _secondsPerDay;
            var totalVethor = transactionsPerDay * userVetAmountsDto.CurrentThorPrice;

            return percentageOfEcoSystem * totalVethor;
        }
    }
}