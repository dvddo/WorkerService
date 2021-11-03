using System.Xml.Linq;

namespace SharedLibrary.Xml
{

    public class XmlSocketChannel : SocketChannel<XmlMessageProtocol, XDocument> { }

    public class XmlClientSocketChannel : ClientSocketChannel<XmlMessageProtocol, XDocument> { }

}
