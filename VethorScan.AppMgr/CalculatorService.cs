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
        private readonly int _secondsPerDay;

        private List<KeyValuePair<SplitType, List<Func<UserVetAmountsDto, Task<UserVetResultDto>>>>> _vethorDictionary;

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
                new List<KeyValuePair<SplitType, List<Func<UserVetAmountsDto, Task<UserVetResultDto>>>>>
                {
                    KeyValuePair.Create(SplitType.PreSplit, InitializePreSplitList()),
                    KeyValuePair.Create(SplitType.PreSplit, InitializePreSplitXnodeList()),
                    KeyValuePair.Create(SplitType.PostSplit, InitializePostSplitList()),
                    KeyValuePair.Create(SplitType.PostSplit, InitializePostSplitXnodeList())
                };
        }

        /// <summary>
        /// Pre split regular 
        /// </summary>
        /// <returns></returns>
        private List<Func<UserVetAmountsDto, Task<UserVetResultDto>>> InitializePreSplitList()
        {
            return new List<Func<UserVetAmountsDto, Task<UserVetResultDto>>>
            {
                dto => Is(dto, NodeType.None, 0, 10000),
                dto => Is(dto, NodeType.Strength, 10000, 50000, .000432, 1),
                dto => Is(dto, NodeType.Thunder, 50000, 150000, .000432, 1.5),
                dto => Is(dto, NodeType.Mjolnir, 150000, long.MaxValue, .000432, 2),
                dto => Is(dto, NodeType.Thrudheim, 1500000, long.MaxValue, .000432, 2),
            };
        }

        /// <summary>
        /// pre split xnode
        /// </summary>
        /// <returns></returns>
        private List<Func<UserVetAmountsDto, Task<UserVetResultDto>>> InitializePreSplitXnodeList()
        {
            return new List<Func<UserVetAmountsDto, Task<UserVetResultDto>>>
            {
                dto => Is(dto, NodeType.None, 0, 6000),
                dto => Is(dto, NodeType.VeThorX, 6000, 16000, .000432, .25),
                dto => Is(dto, NodeType.StrengthX, 16000, 56000, .000432, 1),
                dto => Is(dto, NodeType.ThunderX, 56000, 156000, .000432, 1.5),
                dto => Is(dto, NodeType.MjolnirX, 156000, long.MaxValue, .000432, 2),
            };
        }

        /// <summary>
        /// post split regular
        /// </summary>
        /// <returns></returns>
        private List<Func<UserVetAmountsDto, Task<UserVetResultDto>>> InitializePostSplitList()
        {
            return new List<Func<UserVetAmountsDto, Task<UserVetResultDto>>>
            {
                dto => Is(dto, NodeType.None, 0, 1000000),
                dto => Is(dto, NodeType.Strength, 1000000, 5000000, .000432, 1),
                dto => Is(dto, NodeType.Thunder, 5000000, 15000000, .000432, 1.5),
                dto => Is(dto, NodeType.Mjolnir, 15000000, long.MaxValue, .000432, 2),
                dto => Is(dto, NodeType.Thrudheim, 15000000, long.MaxValue, .000432, 2),
            };
        }

        /// <summary>
        /// post split xnode
        /// </summary>
        /// <returns></returns>
        private List<Func<UserVetAmountsDto, Task<UserVetResultDto>>> InitializePostSplitXnodeList()
        {
            return new List<Func<UserVetAmountsDto, Task<UserVetResultDto>>>
            {
                dto => Is(dto, NodeType.None, 0, 600000),
                dto => Is(dto, NodeType.VeThorX, 600000, 1600000, .000432, .25),
                dto => Is(dto, NodeType.StrengthX, 1600000, 5600000, .000432, 1),
                dto => Is(dto, NodeType.ThunderX, 5600000, 15600000, .000432, 1.5),
                dto => Is(dto, NodeType.MjolnirX, 15600000, long.MaxValue, .000432, 2),
            };
        }

        private Task<UserVetResultDto> Is(UserVetAmountsDto dto, NodeType nodeType, long vetMinimum = 0, long vetMaximum = long.MaxValue, 
            double baseThorGeneration = .000432, double bonusPercentage = 0)
        {
            Task<UserVetResultDto> task = new Task<UserVetResultDto>(() =>
            {
                UserVetResultDto x = null;

                if (DetermineIfNode(dto, vetMinimum, vetMaximum))
                {
                    x = CalculateThor(dto, nodeType, baseThorGeneration, bonusPercentage).GetAwaiter().GetResult();
                }

                return x;
            });

            return task;
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
            _vethorDictionary.Where(x => x.Key == userVetInformation.Split).SelectMany(z => z.Value).ToList()
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
        /// <param name="userVetAmountsDto"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        private bool DetermineIfNode(UserVetAmountsDto userVetAmountsDto, long minimum, long maximum = long.MaxValue)
        {
            var result =
                //if value is greater or equal to vetMinimum and less than vetMaximum presplit
                userVetAmountsDto.UserVetAmount >= minimum && userVetAmountsDto.UserVetAmount < maximum && maximum != long.MaxValue 
                ||
                userVetAmountsDto.UserVetAmount >= minimum && maximum == long.MaxValue;

            return result;
        }

        /// <summary>
        /// calculates thor taking node and xnode into consideration
        /// </summary>
        /// <param name="userVetAmountsDto"></param>
        /// <param name="nodeType"></param>
        /// <param name="baseGenerationOfThor">average amount of base rewards base on the number of node hodlers across all nodes, just a estimate provided by vechain</param>
        /// <param name="bonusPercentage"></param>
        /// <returns></returns>
        private async Task<UserVetResultDto> CalculateThor(UserVetAmountsDto userVetAmountsDto, NodeType nodeType,
            double baseGenerationOfThor, double bonusPercentage = 0)
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
                    (decimal) baseGenerationOfThor, (decimal) bonusPercentage).ConfigureAwait(false);
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
                        ? long.Parse(metadata.Data.CirculatingSupply.ToString().PadRight(2, '0'))
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
            var transactionsPerDay = userVetAmountsDto.TransactionsPerSecond * _secondsPerDay;
            var totalVethor = transactionsPerDay * userVetAmountsDto.CurrentThorPrice;

            return percentageOfEcoSystem * totalVethor;
        }
    }
}