using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SharedLibrary.Xml
{

    public class XmlMessageProtocol : Protocol<XDocument>
    {
        public override Task<string> Serialize<T>(T message) => Task.FromResult(XmlSerialization.Serialize(message).ToString());

        protected override XDocument Decode( byte[ ] message )
        {
            var xmlData = Encoding.UTF8.GetString(message);
            var xmlReader = XmlReader.Create(new StringReader(xmlData), new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore});
            return XDocument.Load( xmlReader );
        }

        protected override byte[ ] EncodeBody<T>( T message )
        {
            if ( message is XDocument )
                return Encoding.UTF8.GetBytes( message.ToString( ) );
            else
                return Encoding.UTF8.GetBytes( XmlSerialization.Serialize( message ).ToString( ) );
        }
    }
}
