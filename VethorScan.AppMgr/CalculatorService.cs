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

        public Dictionary<Func<NodeType, bool>, Func<UserVetAmountsDto, List<Task<UserProfitDto>>>> CalculationDictionary =
            new Dictionary<Func<NodeType, bool>, Func<UserVetAmountsDto, List<Task<UserProfitDto>>>>();

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
        public IEnumerable<Task<UserProfitDto>>  CalculateSimple(decimal totalVetAmount)
        {
            var results = CalculateAdvanced(new UserVetAmountsDto {UserVetAmount = totalVetAmount});

            return results;
        }

        /// <summary>
        /// Calculates vet and vethor profit, node and xnodes included
        /// </summary>
        /// <param name="userVetAmountsDto"></param>
        /// <returns></returns>
        public IEnumerable<Task<UserProfitDto>> CalculateAdvanced(UserVetAmountsDto userVetAmountsDto)
        {
            var results = new List<Task<UserProfitDto>>();

            NodeType nodeType = _vetDictionaryService.NodeDictionary.FirstOrDefault(z => z.Key.Invoke(userVetAmountsDto.UserVetAmount)).Value;

            KeyValuePair<Func<NodeType, bool>, Func<UserVetAmountsDto, List<Task<UserProfitDto>>>> calculations = CalculationDictionary.FirstOrDefault(x => x.Key.Invoke(nodeType));

            results.AddRange(
                calculations.Value.Invoke(userVetAmountsDto));

            return results;
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
                new Dictionary<Func<NodeType, bool>, Func<UserVetAmountsDto, List<Task<UserProfitDto>>>>
                {
                    {
                        x => x.ExactMatch(NodeType.None),
                        dto => new List<Task<UserProfitDto>> {Calculate(dto, NodeType.None, 0)}
                    },
                    {
                        x => x.ExactMatch(NodeType.VeThorX),
                        dto => new List<Task<UserProfitDto>> { XNodeCalculate(dto, NodeType.VeThorX, 0.00006, .25)}
                    },
                    {
                        x => x.ExactMatch(NodeType.Strength | NodeType.VeThorX),
                        dto => new List<Task<UserProfitDto>>
                        {
                            Calculate(dto, NodeType.Strength, 0.00015, 1),
                            XNodeCalculate(dto, NodeType.VeThorX, 0.00006, .25, Calculate(dto, NodeType.Strength, 0.00015, 1))
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Strength | NodeType.StrengthX),
                        dto => new List<Task<UserProfitDto>>
                        {
                            Calculate(dto, NodeType.Strength, 0.00015, 1),

                            //strengthX is strengthX + strength
                            XNodeCalculate(dto, NodeType.StrengthX, 0.00006, 1, Calculate(dto, NodeType.Strength, 0.00015, 1))
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Thunder | NodeType.StrengthX),
                        dto => new List<Task<UserProfitDto>>
                        {
                            Calculate(dto, NodeType.Thunder, 0.00015, 1.5),

                            //strengthX is strengthX + strength
                            XNodeCalculate(dto, NodeType.StrengthX, 0.00006, 1, Calculate(dto, NodeType.Strength, 0.00015, 1))
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Thunder | NodeType.ThunderX),
                        dto => new List<Task<UserProfitDto>>
                        {
                            Calculate(dto, NodeType.Thunder, 0.00015, 1.5),

                            //thunderX is thunderX + thunder
                            XNodeCalculate(dto, NodeType.ThunderX, 0.00006, 1.5, Calculate(dto, NodeType.Thunder, 0.00015, 1.5))
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Mjolnir | NodeType.ThunderX),
                        dto => new List<Task<UserProfitDto>>
                        {
                            Calculate(dto, NodeType.Mjolnir, 0.00015, 2),
                            
                            //thunderX is thunderX + thunder
                            XNodeCalculate(dto, NodeType.ThunderX, 0.00006, 1.5, Calculate(dto, NodeType.Thunder, 0.00015, 1.5))
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Mjolnir | NodeType.MjolnirX),
                        dto => new List<Task<UserProfitDto>>
                        {
                            Calculate(dto, NodeType.Mjolnir, 0.00015, 2),

                            //mjolnirX is mjolnirX + mjolnir
                            XNodeCalculate(dto, NodeType.MjolnirX, 0.00006, 2, Calculate(dto, NodeType.Mjolnir, 0.00015, 2))
                        }
                    },
                    {
                        x => x.ExactMatch(NodeType.Mjolnir | NodeType.MjolnirX | NodeType.Thrudheim),
                        dto => new List<Task<UserProfitDto>>
                        {
                            //mjolnirX is mjolnirX + mjolnir
                            XNodeCalculate(dto, NodeType.MjolnirX, 0.00006, 2, Calculate(dto, NodeType.Mjolnir, 0.00015, 2)),

                            Calculate(dto, NodeType.Mjolnir, 0.00015, 2),

                            Calculate(dto, NodeType.Thrudheim, 0.00015, 2, 101, .3)
                        }
                    }

                };
        }

        ///  <summary>
        ///  calculates thor taking node and xnode into consideration
        /// 
        ///  
        /// 10,000 Vet(Vet holding) / 525,770,505 = 0.0019 % of Vet Circulating Supply
        /// Company willing to pay $0.5 per transaction on blockchain, smart contract execution
        /// Let’s say ecosystem is running 100mm transactions daily = $50mm VeThor
        /// 0.0019 % multiplied by $50,000,000 = $950 daily
        ///  </summary>
        /// <param name="dto"></param>
        /// <param name="nodeType"></param>
        /// <param name="thorPower">average amount of base rewards base on the number of node hodlers across all nodes, just a estimate provided by vechain</param>
        /// <param name="bonusPercentage"></param>
        /// <param name="burnRateBonus">the amount of the burn rate per transaction, for thunderium nodes only</param>
        private async Task<UserProfitDto> Calculate(UserVetAmountsDto dto, NodeType nodeType, double thorPower = 0.00015, double bonusPercentage = 0, double topTierNodeCount = 0, double burnRateBonus = 0.0)
        {
            UserProfitDto result = new UserProfitDto();
            
            //get vet profits
            await CalculateVetProfits(dto, result).ConfigureAwait(false);

            //get Vethor profits 
            await CalculateVeThorProfits(dto, result, 
                    (decimal) thorPower, 
                    (decimal) bonusPercentage, (decimal)topTierNodeCount, (decimal)burnRateBonus)
                .ConfigureAwait(false);
            
            result.NodeType = nodeType;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="nodeType"></param>
        /// <param name="thorPower">average amount of base rewards base on the number of node hodlers across all nodes, just a estimate provided by vechain</param>
        /// <param name="bonusPercentage"></param>
        /// <param name="tier1NodeProfits"></param>
        /// <returns></returns>
        private async Task<UserProfitDto> XNodeCalculate(UserVetAmountsDto dto, NodeType nodeType,
            double thorPower = 0.00006, double bonusPercentage = 0, Task<UserProfitDto> tier1NodeProfits = null)
        {
            UserProfitDto result = new UserProfitDto();

            UserProfitDto nodeProfits = null;

            if (tier1NodeProfits != null)
                nodeProfits = await tier1NodeProfits.ConfigureAwait(false);

            //get vet profits
            await CalculateVetProfits(dto, result).ConfigureAwait(false);

            //get Vethor profits 
            await CalculateVeThorProfits(dto, result,
                    (decimal)thorPower,
                    (decimal)bonusPercentage, 0, 0, nodeProfits)
                .ConfigureAwait(false);
            


            result.NodeType = nodeType;

            return result;
        }

        private async Task CalculateVetProfits(UserVetAmountsDto dto, UserProfitDto result)
        {
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            result.VetVetProjectedProfits.VetCurrentProfit = dto.UserVetAmount * metadata.Data.Quotes.Usd.Price; 
        }

        private async Task CalculateVeThorProfits(UserVetAmountsDto dto, UserProfitDto result,
            decimal avgThorPerNode, decimal bonusPercentage, decimal topTierNodeCount = 0, decimal burnRateBonusPercentage = 0,
            UserProfitDto nodeProfits = null)
        {
            var metadata = await GetVetMetadata().ConfigureAwait(false);

            dto.CirculatingSupply =
                await CalculateCirculatingSupply(dto).ConfigureAwait(false);

            decimal percentageOfEcoSystem = dto.UserVetAmount / dto.CirculatingSupply;

            decimal transactionsPerDay = dto.TransactionsPerSecond * 86400;

            decimal totalVethor = transactionsPerDay * dto.CurrentThorPrice;

            //this is the average value of transactions ran on the actual blockchain
            result.VeThorVetProjectedProfits.VeThorAverageBlockchainValuePerDay =
                percentageOfEcoSystem * totalVethor +
                (nodeProfits?.VeThorVetProjectedProfits?.VeThorAverageBlockchainValuePerDay ?? 0);

            //calculate thor amount per day based on argueents 
            //example: 0.000432 + (.00015 * 200%) = 0.000732 VeThor per VET per day or 0.26718 per Year;
            result.VeThorVetProjectedProfits.VeThorAmountPerDay =
                metadata.Data.VetToThorRate + avgThorPerNode * bonusPercentage +
                (nodeProfits?.VeThorVetProjectedProfits?.VeThorAmountPerDay ?? 0);
            
            //thunderium burn rate bonus, 30% of total transactions across the entire blockchain divided by amount of nodes
            if (topTierNodeCount > 0)
            {
                result.VeThorVetProjectedProfits.BurnRateProfitPerDay
                    = transactionsPerDay * burnRateBonusPercentage / topTierNodeCount;
            }

            result.VeThorVetProjectedProfits.BurnRateBonusPercentage = burnRateBonusPercentage * 100;

            result.VeThorVetProjectedProfits.VeThorAmountPerDay =
                result.VeThorVetProjectedProfits.VeThorAmountPerDay + result.VeThorVetProjectedProfits.BurnRateProfitPerDay;

            result.VeThorVetProjectedProfits.VeThorAmountPerYear =
                (result.VeThorVetProjectedProfits.VeThorAmountPerDay * 365 * dto.UserVetAmount) +
                (nodeProfits?.VeThorVetProjectedProfits?.VeThorAmountPerYear ?? 0);

            result.VeThorVetProjectedProfits.ProfitPerDay =
                dto.CurrentThorPrice * result.VeThorVetProjectedProfits.VeThorAmountPerDay +
                (nodeProfits?.VeThorVetProjectedProfits?.ProfitPerDay ?? 0);

            result.VeThorVetProjectedProfits.ProfitPerWeek =
                result.VeThorVetProjectedProfits.ProfitPerDay * 7 +
                (nodeProfits?.VeThorVetProjectedProfits?.ProfitPerWeek ?? 0);

            result.VeThorVetProjectedProfits.ProfitPerMonth =
                result.VeThorVetProjectedProfits.ProfitPerWeek * 52 / 12 +
                (nodeProfits?.VeThorVetProjectedProfits?.ProfitPerWeek ?? 0);

            result.VeThorVetProjectedProfits.ProfitPerYear =
                result.VeThorVetProjectedProfits.ProfitPerMonth * 12 +
                (nodeProfits?.VeThorVetProjectedProfits?.ProfitPerMonth ?? 0);

            result.VeThorVetProjectedProfits.Profit3Year =
                result.VeThorVetProjectedProfits.ProfitPerYear * 3 +
                (nodeProfits?.VeThorVetProjectedProfits?.ProfitPerYear ?? 0);

            result.VeThorVetProjectedProfits.Profit5Year =
                result.VeThorVetProjectedProfits.ProfitPerYear * 5 +
                (nodeProfits?.VeThorVetProjectedProfits?.ProfitPerYear ?? 0);

            result.VeThorVetProjectedProfits.Profit10Year =
                result.VeThorVetProjectedProfits.ProfitPerYear * 10 +
                (nodeProfits?.VeThorVetProjectedProfits?.ProfitPerYear ?? 0);
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
    }
}