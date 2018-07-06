using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VethorScan.Contracts
{
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum NodeType
    {
        None = 1,
        VeThorX = 2,
        Strength = 4,
        StrengthX = 8,
        Thunder = 16,
        ThunderX = 32,
        MjolnirX = 64,
        Mjolnir = 128,
        Thrudheim = 256
    }
}

