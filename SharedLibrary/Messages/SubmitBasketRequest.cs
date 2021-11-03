#nullable   enable

using System.Xml.Serialization;

using Newtonsoft.Json;

namespace SharedLibrary
{
    [XmlRoot( "Message" )]
    public class SubmitBasketRequest : Message
    {
        [XmlAttribute("POSTxnNumber")]
        [JsonProperty("posTxnNumber")]
        public string? POSTransactionNumber { get; set; }

        public SubmitBasketRequest()
        {
            Type = MessageType.Request;
            Action = "SubmitBasket";
        }
    }
}
