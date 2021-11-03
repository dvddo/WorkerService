#nullable enable

using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SharedLibrary.Messages
{
    public class PaymentInfo
    {
        [XmlAttribute("AuthCode")]
        [JsonProperty("authCode")]
        public string? AuthorizationCode { get; set; }

        [XmlAttribute("Amount")]
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [XmlAttribute("LastFour")]
        [JsonProperty("lastFour")]
        public string? LastFour { get; set; }

    }
}
