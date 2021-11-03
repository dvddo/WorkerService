#nullable enable

using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SharedLibrary.Messages
{
    [XmlRoot("Message")]
    public class BasketPaidRequest : Message
    {

        [XmlAttribute("POSTxnNumber")]
        [JsonProperty("posTxnNumber")]
        public string? POSTransactionNumber { get; set; }

        [XmlElement("PaymentInformation")]
        [JsonProperty("paymentInformation")]
        public PaymentInfo? PaymentInfo { get; set; }


        public BasketPaidRequest()
        {
            Type = MessageType.Request;
            Action = "BasketPaid";
        }
    }
}
