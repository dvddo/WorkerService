using Newtonsoft.Json.Linq;
using SharedLibrary;
using SharedLibrary.Models;
using SharedLibrary.Xml;
using SharedLibrary.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ClientConsole
{

    public abstract class Client<TClientSocketChannel, TProtocol, TMessageType>
        where TClientSocketChannel : ClientSocketChannel<TProtocol, TMessageType>
        where TProtocol: Protocol<TMessageType>, new()
        where TMessageType : class, new()
    {
        public event EventHandler ChannelClosed;

        public readonly WorkerServiceDbContext _context = null;


        private DataMessage _message { get; set; }
        public readonly Connection _connection;

        private readonly CancellationToken _stoppingToken;
        private HeartBeatRequestMessage hbMessage;
        public Client(string ipAddress, int Port, WorkerServiceDbContext context, Connection connection, CancellationToken stoppingToken)
        {
            _context = context;
            _connection = connection;
            _stoppingToken = stoppingToken;

            hbMessage = new HeartBeatRequestMessage
            {
                Id = "♥♥HB♥♥",
                POSData = new POSData { Id = $"POS_{_connection.Id}" }
            };
        }

        public abstract Task<bool> Connect();

        protected async Task CheckChannel(TClientSocketChannel channel)
        {

            do
            {
                await Task.Delay(1000);
            } while (channel.IsClosed() == false);

            ChannelClosed?.Invoke(this, EventArgs.Empty);
        }

        protected async Task HBLoop(int lcount, TClientSocketChannel channel)
        {
            bool loopControl(int count) => count == -1 ? true : lcount-- > 0;
            while (loopControl(lcount))
            {
                //Console.WriteLine($"count: { lcount}");
                await SendAsync(hbMessage, channel);
                await Task.Delay(10 * 1000);
            }
        }

        private void SaveMessage<T>(T message)
        {
            if (_context != null)
            {
                lock (_context)
                {
                    //_message = new DataMessage() { InMessage = (new JsonMessageProtocol()).Serialize<T>(message), Connection = _connection };
                    _context.DataMessages.Add(new DataMessage() { InMessage = (new JsonMessageDispatcher()).SerializeToString<T>(message), Connection = _connection });

                    _context.SaveChanges();
                    //await _context.SaveChangesAsync(_stoppingToken);
                }
            }

        }

        public async Task SendAsync<T>(T message, TClientSocketChannel channel)
        {
            try
            {
                SaveMessage(message);
                await channel.SendAsync(message).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Oh NO!!! {ex}");
            }
        }

        public void Dispose()
        {


        }
    }
}
