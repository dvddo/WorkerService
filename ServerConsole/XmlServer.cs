using SharedLibrary;
using SharedLibrary.Models;
using SharedLibrary.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ServerConsole
{
    public class XmlServer : Server<XmlSocketServer, XmlSocketChannel, XmlMessageProtocol, XDocument, XDocumentMessageDispatcher>, IDisposable
    {
        private XmlSocketServer server;

        public XmlServer(WorkerServiceDbContext context, Connection connection, CancellationToken stoppingToken)
        {
            var posController = new POSController();
            var txController = new TransactionManager { POSController = posController };

            var msgHndl = new MessageHandler();

            msgHndl.POSController = posController;
            msgHndl.TransactionManager = txController;

            _ = Task.Run(() => base.CheckBasket(txController, context, connection));

            server = new XmlSocketServer(context, connection, stoppingToken, 4);
            server.Id = connection.Id;
            server.Bind<MessageHandler>(msgHndl);

        }

        public override void Start()
        {
            _ = server.StartAsync();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
