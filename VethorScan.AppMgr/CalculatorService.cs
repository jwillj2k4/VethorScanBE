using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly VetDictionaryService _vetDictionaryService;

        public Dictionary<Func<NodeType, bool>, Func<UserVetAmountsDto, List<Task<UserVetResultDto>>>> CalculationDictionary =
            new Dictionary<Func<NodeType, bool>, Func<UserVetAmountsDto, List<Task<UserVetResultDto>>>>();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="memCache"></param>
        /// <param name="vetSystem"></param>
        /// <param name="vetDictionaryService"></param>
        public CalculatorService(IMemoryCache memCache, IVetSystem vetSystem, VetDictionaryService vetDictionaryService)
        {
            _vetSystem = vetSystem;
            _vetDictionaryService = vetDictionaryService;
            _memCache = memCache;
            InitializeCalculationDictionary();
        }

        /// <summary>
        /// Calculates vet profit only
        /// </summary>
        /// <param name="totalVetAmount"></param>
        public List<Task<UserVetResultDto>>  CalculateSimple(decimal totalVetAmount)
        {
            var results = new List<Task<UserVetResultDto>>();

            KeyValuePair<Func<decimal, bool>, NodeType> foundval = _vetDictionaryService.VeThorNodeTypeDictionary.FirstOrDefault(z => z.Key.Invoke(totalVetAmount));

            var nodeList = foundval
                .Value;

            var foundMatches = CalculationDictionary.FirstOrDefault(x => x.Key.Invoke(nodeList));

            results.AddRange(
                foundMatches.Value.Invoke(new UserVetAmountsDto {UserVetAmount = totalVetAmount}));

            return results;
        }

        /// <summary>
        /// Calculates vet and vethor profit, node and xnodes included
        /// </summary>
        /// <param name="userVetAmountsDto"></param>
        /// <returns></returns>
        public Task<IEnumerable<Task<UserVetResultDto>>> CalculateAdvanced(UserVetAmountsDto userVetAmountsDto)
        {
            IList<Task<UserVetResultDto>> result = new List<Task<UserVetResultDto>>();

            var nodeType =_vetDictionaryService.VeThorNodeTypeDictionary[x => x == userVetAmountsDto.UserVetAmount];

            CalculationDictionary.Where(x => x.Key.Invoke(nodeType)).ToList().ForEach(a =>
                a.Value.Invoke(userVetAmountsDto)
                    .ForEach(b =>
                    {
                        result.Add(b);
                    }));

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
        /// initialize dictionary
        /// </summary>
        private void InitializeCalculationDictionary()
        {
            CalculationDictionary =
                new Dictionary<Func<NodeType, bool>, Func<UserVetAmountsDto, List<Task<UserVetResultDto>>>>
                {
                    {
                        x => x.ExactMatch(NodeType.None),
                        dto => new List<Task<UserVetResultDto>> {Calculate(dto, NodeType.None)}
                    },
                    {
                        x => x.ExactMatch(NodeType.VeThorX),
                        dto => new List<Task<UserVetResultDto>> {Calculate(dto, NodeType.VeThorX, .000432, .25)}
                    },
                    {
                        x => x.ExactMatch(NodeType.Strength | NodeType.VeThorX),
                        dto => new List<Task<UserVetResultDto>>
                        {
                            Calculate(dto, NodeType.Strength, .000432, 1),
                            Calculate(dto, NodeType.VeThorX, .000432, .25)
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Strength | NodeType.StrengthX),
                        dto => new List<Task<UserVetResultDto>>
                        {
                            Calculate(dto, NodeType.Strength, .000432, 1),
                            Calculate(dto, NodeType.StrengthX, .000432, 1)
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Thunder | NodeType.StrengthX),
                        dto => new List<Task<UserVetResultDto>>
                        {
                            Calculate(dto, NodeType.Thunder, .000432, 1.5),
                            Calculate(dto, NodeType.StrengthX, .000432, 1)
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Thunder | NodeType.ThunderX),
                        dto => new List<Task<UserVetResultDto>>
                        {
                            Calculate(dto, NodeType.Thunder, .000432, 1.5),
                            Calculate(dto, NodeType.ThunderX, .000432, 1.5)
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Mjolnir | NodeType.ThunderX),
                        dto => new List<Task<UserVetResultDto>>
                        {
                            Calculate(dto, NodeType.Mjolnir, .000432, 2),
                            Calculate(dto, NodeType.ThunderX, .000432, 1.5)
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Mjolnir | NodeType.MjolnirX),
                        dto => new List<Task<UserVetResultDto>>
                        {
                            Calculate(dto, NodeType.Mjolnir, .000432, 2),
                            Calculate(dto, NodeType.MjolnirX, .000432, 2)
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Mjolnir | NodeType.MjolnirX | NodeType.Thrudheim),
                        dto => new List<Task<UserVetResultDto>>
                        {
                            Calculate(dto, NodeType.Mjolnir, .000432, 2),
                            Calculate(dto, NodeType.MjolnirX, .000432, 2),
                            Calculate(dto, NodeType.Thrudheim, .000432, 2)
                        }
                    }

                };
        }

        /// <summary>
        /// calculates thor taking node and xnode into consideration
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="nodeType"></param>
        /// <param name="baseThorGeneration">average amount of base rewards base on the number of node hodlers across all nodes, just a estimate provided by vechain</param>
        /// <param name="bonusPercentage"></param>
        /// <returns></returns>
        private async Task<UserVetResultDto> Calculate(UserVetAmountsDto dto, NodeType nodeType,
            double baseThorGeneration = .000432, double bonusPercentage = 0)
        {
            dto.CirculatingSupply =
                await CalculateCirculatingSupply(dto).ConfigureAwait(false);

            UserVetResultDto result = new UserVetResultDto();

            result.VetResultDto =
                await CalculateVetTransactionalProfits(dto).ConfigureAwait(false);

            result.VeThorResultDto = await CalculateVeThorProfits(dto,
                (decimal) baseThorGeneration, (decimal) bonusPercentage).ConfigureAwait(false);

            result.VeThorResultDto.NodeType = result.VetResultDto.NodeType = nodeType;

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
                :
                //append 2 zeros at the end for post split, until the 9th.
                DateTime.UtcNow.Month < 7 && DateTime.UtcNow.Day < 10 && DateTime.UtcNow.Year == 2018
                    ? long.Parse(metadata.Data.CirculatingSupply.ToString().PadRight(2, '0'))
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
                    //example: 0.00042 + (baseGenerationOfThor * 200%) = 0.000720 VeThor per VET a day or 0.2628 per Year;
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
            var transactionsPerDay = userVetAmountsDto.TransactionsPerSecond * 86400;
            var totalVethor = transactionsPerDay * userVetAmountsDto.CurrentThorPrice;

            return percentageOfEcoSystem * totalVethor;
        }
    }
}