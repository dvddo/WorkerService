using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public abstract class SocketServer<TSocketChannelType, TProtocol, TMessageType, TMessageDispatcher>
        where TSocketChannelType : SocketChannel<TProtocol, TMessageType>, new()
        where TProtocol : Protocol<TMessageType>, new()
        where TMessageType : class, new()
        where TMessageDispatcher : MessageDispatcher<TMessageType>, new()
    {
        Func<Socket> _serverSocketFactory;

        private SocketChannelManager _channelManager; //need to dispose this???
        readonly TMessageDispatcher _messageDispatcher = new TMessageDispatcher();


        readonly SemaphoreSlim _connectionLimiter;
        private readonly WorkerServiceDbContext _context;
        private readonly Connection _connection;
        private readonly CancellationToken _cancellationToken;

        public Guid Id { get; set; }

        public SocketServer(WorkerServiceDbContext context, Connection connection, CancellationToken cancellationToken, int maxConnections = 100_000_000)
        {
            _context = context;
            _connection = connection;
            _cancellationToken = cancellationToken;

            _connectionLimiter = new SemaphoreSlim(maxConnections, maxConnections);

            _messageDispatcher.DBContext = _context;
            _messageDispatcher.ConnModel = _connection;

            _channelManager = new SocketChannelManager(() => {
                var channel = CreateChannel();
                _messageDispatcher.Bind(channel,true);
                return channel;
            });


            _channelManager.ChannelClosed += (s, e) => _connectionLimiter.Release();
        }

        public void Bind<TController>(TController controller)
        {
            _messageDispatcher.Bind<TController>(controller);
        }

        public Task StartAsync()
        {

            _serverSocketFactory = () => {
                var endPoint = new IPEndPoint(IPAddress.Parse(_connection.IPAddress), _connection.Port);
                var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(endPoint);
                socket.Listen(128);
                return socket;
            };

            return Task.Factory.StartNew(() => RunAsync(_cancellationToken), _cancellationToken);
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                Socket serverSocket = null;

                do
                {

                    if (!await _connectionLimiter.WaitAsync(1000, cancellationToken))
                    {
                        //Console.WriteLine("SocketServer :: Max Connections Reached");
                        //max connections reached, so nuke the server socket
                        try
                        {
                            serverSocket?.Close();
                            serverSocket?.Dispose();
                            serverSocket = null;
                        }
                        catch { }

                        await _connectionLimiter.WaitAsync(cancellationToken);
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        serverSocket ??= _serverSocketFactory();
                        await AcceptConnection(serverSocket);
                    }

                } while (!cancellationToken.IsCancellationRequested);

            }
            catch (OperationCanceledException)
            {
                //Expected exception for task cancellation - swallow it
            }
            catch (Exception _e)
            {
                //TODO: Log it somewhere good :)
                Console.WriteLine($"Exception in SocketServer::RunAsync => {_e}");
            }
        }

        async Task AcceptConnection(Socket socket)
        {
            var clientSocket = await Task.Factory.FromAsync(new Func<AsyncCallback, object, IAsyncResult>(socket.BeginAccept),
                                                             new Func<IAsyncResult, Socket>(socket.EndAccept),
                                                             null).ConfigureAwait(false);

            //Console.WriteLine("SERVER :: CLIENT CONNECTION REQUEST");
            _channelManager.Accept(clientSocket);
            //Console.WriteLine($"SERVER :: CLIENT CONNECTED :: {_connectionLimiter.CurrentCount}");
        }

        protected virtual TSocketChannelType CreateChannel() => new TSocketChannelType();

    }
}
