#nullable enable

using System.Xml.Serialization;

using Newtonsoft.Json;

namespace SharedLibrary.Messages
{
    [XmlRoot("Message")]
    public class BasketPaidResponse : Message
    {

        [XmlAttribute("POSTxnNumber")]
        [JsonProperty("posTxnNumber")]
        public string? POSTransactionNumber { get; set; }

        [XmlElement("Result")]
        [JsonProperty("result")]
        public Result? Result { get; set; }

        public BasketPaidResponse()
        {
            Type = MessageType.Response;
            Action = "BasketPaid";
        }
    }
}
