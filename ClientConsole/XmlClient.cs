using SharedLibrary;
using SharedLibrary.Models;
using SharedLibrary.Xml;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ClientConsole
{
    public class XmlClient : Client<XmlClientSocketChannel, XmlMessageProtocol, XDocument> , IDisposable
    {
        private XmlClientSocketChannel channel;

        private XDocumentMessageDispatcher messageDispatcher { get; set; }
        private IPEndPoint endpoint { get; set; }

        public XmlClient(string ipAddress, int Port, WorkerServiceDbContext context, Connection connection, CancellationToken stoppingToken) : base(ipAddress, Port, context, connection, stoppingToken)
        {
            var msgHndl = new MessageHandler();
            endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), Port);

            messageDispatcher = new XDocumentMessageDispatcher();
            messageDispatcher.DBContext = _context;
            messageDispatcher.ConnModel = _connection;
            messageDispatcher.Bind<MessageHandler>(msgHndl);
        }

        public override async Task<bool> Connect()
        {

            bool status = true;
            try
            {
                channel = new XmlClientSocketChannel();
                messageDispatcher.Bind(channel, true);
                await channel.ConnectAsync(endpoint).ConfigureAwait(false);
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                status = false;
                Console.WriteLine($"Client Failed to Connect: {ex}");
            }
            finally
            {
                if (status)
                {
                    _ = Task.Run(() => base.HBLoop(2, channel));
                    _ = Task.Run(() => base.CheckChannel(channel));
                }
            }
            return status;
        }



        public async Task SendAsync<T>(T message)
        {
            await base.SendAsync(message, channel);
        }




        public void Dispose()
        {
            //ClientClosed = null;
            endpoint = null;
            channel = null;

            messageDispatcher = null;

        }
    }
}
