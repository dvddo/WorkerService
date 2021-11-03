using Newtonsoft.Json.Linq;
using SharedLibrary;
using SharedLibrary.Json;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerConsole
{
    public class JsonServer : Server<JsonSocketServer, JsonSocketChannel, JsonMessageProtocol, JObject, JsonMessageDispatcher>, IDisposable
    {
        private JsonSocketServer server;

        public JsonServer(WorkerServiceDbContext context, Connection connection, CancellationToken stoppingToken)
        {
            // these need to be pull from db
            var posController = new POSController();
            var txController = new TransactionManager { POSController = posController };

            var msgHndl = new MessageHandler();

            msgHndl.POSController = posController;
            msgHndl.TransactionManager = txController;

            _ = Task.Run(() => base.CheckBasket(txController, context, connection));

            server = new JsonSocketServer(context, connection, stoppingToken, 4);
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
