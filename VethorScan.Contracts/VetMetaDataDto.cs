using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VethorScan.Contracts
{
    public partial class VetMetaDataDto
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

    }

    public partial class Data : UserVetAmountsDto
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("website_slug")]
        public string WebsiteSlug { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("circulating_supply")]
        public override long CirculatingSupply { get; set; }

        [JsonProperty("total_supply")]
        public long TotalSupply { get; set; }

        [JsonProperty("max_supply")]
        public object MaxSupply { get; set; }

        [JsonProperty("quotes")]
        public Quotes Quotes { get; set; }

        [JsonProperty("last_updated")]
        public long LastUpdated { get; set; }

        public decimal VetToThorRate { get; set; } = .000432m;
    }

    public partial class Quotes
    {
        [JsonProperty("USD")]
        public Usd Usd { get; set; }
    }

    public partial class Usd
    {
        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("volume_24h")]
        public long Volume24H { get; set; }

        [JsonProperty("market_cap")]
        public long MarketCap { get; set; }

        [JsonProperty("percent_change_1h")]
        public double PercentChange1H { get; set; }

        [JsonProperty("percent_change_24h")]
        public double PercentChange24H { get; set; }

        [JsonProperty("percent_change_7d")]
        public double PercentChange7D { get; set; }
    }

    public partial class Metadata
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("error")]
        public object Error { get; set; }
    }
}
