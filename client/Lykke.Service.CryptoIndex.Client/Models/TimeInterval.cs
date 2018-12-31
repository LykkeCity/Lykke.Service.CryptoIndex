using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.CryptoIndex.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimeInterval
    {
        [EnumMember(Value = "Unspecified")] Unspecified,
        [EnumMember(Value = "Hour24")] Hour24,
        [EnumMember(Value = "Day5")] Day5,
        [EnumMember(Value = "Day30")] Day30
    }
}
