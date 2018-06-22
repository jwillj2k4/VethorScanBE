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
        private readonly HttpClient _client;
        private readonly ILogger<VetSystem> _logger;
        private readonly string _apiKey;

        public VetSystem(HttpClient client, ILogger<VetSystem> logger, Uri baseAddress, IConfiguration config)
        {
            _client = client;
            _client.BaseAddress = baseAddress;
            _logger = logger;
            _apiKey = config["CoinMarketCapAPIKey"];
        }

        public async Task<VetInformationDto> GetVetMetadata()
        {
            //make an http call to retrieve the current vet price from coin market cap
            try
            {
                var episodesUrl = new Uri($"/v1/podcasts/shownum/episodes.json?api_key={_apiKey}", UriKind.Relative);

                _logger.LogWarning($"HttpClient: Loading {episodesUrl}");

                var res = await _client.GetAsync(episodesUrl);

                res.EnsureSuccessStatusCode();

                return await res.Content.ReadAsJsonAsync<VetInformationDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occurred connecting to CoinMarketCap API {ex}");
                throw;
            }
        }
    }
}
