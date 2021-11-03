using Newtonsoft.Json.Linq;
using SharedLibrary.Models;
using System.Threading;

namespace SharedLibrary.Json
{
    public class JsonSocketServer
        : SocketServer<JsonSocketChannel, JsonMessageProtocol, JObject, JsonMessageDispatcher>
    {
        public JsonSocketServer(WorkerServiceDbContext context, Connection connection, CancellationToken cancellationToken, int maxConnections = 100000000)
        : base(context, connection, cancellationToken, maxConnections) { }
    }
}
