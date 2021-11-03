using SharedLibrary.Models;
using System.Threading;
using System.Xml.Linq;

namespace SharedLibrary.Xml
{
    public class XmlSocketServer
        : SocketServer<XmlSocketChannel, XmlMessageProtocol, XDocument, XDocumentMessageDispatcher>
    {
        public XmlSocketServer(WorkerServiceDbContext context, Connection connection, CancellationToken cancellationToken, int maxConnections = 100000000)
        : base(context, connection, cancellationToken, maxConnections) { }
    }
}
