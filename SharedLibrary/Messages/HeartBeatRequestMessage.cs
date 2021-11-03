#nullable   enable

using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SharedLibrary
{
    [XmlRoot( "Message" )]
    public class HeartBeatRequestMessage : Message
    {

        public HeartBeatRequestMessage( )
        {
            Type = MessageType.Request;
            Action = "HeartBeat";
        }
    }
}
