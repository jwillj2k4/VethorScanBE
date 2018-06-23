using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VethorScan.Common.Extensions.ReadAsAsyncCore;
using VethorScan.Contracts;
using VethorScan.Domain.Vet;

namespace VethorScan.AC
{
    public class VetSystem : IVetSystem
    {
        private const string VenCoinMarketCapUri = "VenCoinMarketCapUri";
        private readonly HttpClient _client;
        private readonly ILogger<VetSystem> _logger;
        private readonly IConfiguration _config;

        public VetSystem(HttpClient client, ILogger<VetSystem> logger, IConfiguration config)
        {
            _client = client;
            _logger = logger;
            _config = config;
        }

        public async Task<VetMetaDataDto> GetVetMetadata()
        {
            //make an http call to retrieve the current vet price from coin market cap
            try
            {
                var url = new Uri(_config[VenCoinMarketCapUri], UriKind.Absolute);

                var res = await _client.GetAsync(url);

                res.EnsureSuccessStatusCode();

                return await res.Content.ReadAsJsonAsync<VetMetaDataDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occurred connecting to CoinMarketCap API {ex}");
                throw;
            }
        }
    }
}
