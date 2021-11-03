using System;
using System.Threading;
using System.Threading.Tasks;
using SharedLibrary;
using SharedLibrary.Models;

namespace ServerConsole
{
    //SocketServer<TChannelType, TProtocol, TMessageType, TMessageDispatcher>
    public abstract class Server<TSocketServer, TSocketChannelType, TProtocol, TMessageType, TMessageDispatcher> 
        where TSocketServer : SocketServer<TSocketChannelType, TProtocol, TMessageType, TMessageDispatcher>
        where TSocketChannelType : SocketChannel<TProtocol, TMessageType>, new()
        where TProtocol : Protocol<TMessageType>, new()
        where TMessageType : class, new()
        where TMessageDispatcher : MessageDispatcher<TMessageType>, new()
    {

        public Server()
        {
            // these need to be pull from db

        }

        public async Task CheckBasket(TransactionManager txController, WorkerServiceDbContext context, Connection connection)
        {
            do
            {
                await Task.Delay(1000);
                await txController.PayBasket(context,connection).ConfigureAwait(false);
            } while (true);
        }

        public abstract void Start();

        public void Dispose()
        {
            throw new NotImplementedException();
        }


    }
}
