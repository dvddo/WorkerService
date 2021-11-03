#nullable   enable

using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SharedLibrary
{
    [XmlRoot( "Message" )]
    public class SubmitBasketResponse : Message
    {
        [XmlElement("Result")]
        [JsonProperty("result")]
        public Result? Result { get; set; }

        public SubmitBasketResponse()
        {
            Type = MessageType.Response;
            Action = "SubmitBasket";
        }
    }
}
