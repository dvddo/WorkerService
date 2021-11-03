
#nullable enable

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using SharedLibrary;
using SharedLibrary.Models;

namespace ServerConsole
{
    //1. Map POS Identifer to IChannel :: via the Heartbeat
    //2. Provide SendTo<T> abstraction to send message to POS by POS Id
    public class POSController
    {

        readonly ConcurrentDictionary<string, WeakReference<ISocketChannel>> _posChannelMap = new ConcurrentDictionary<string, WeakReference<ISocketChannel>>();

        public void ProcessHeartBeat(string posId, ISocketChannel channel)
        {
            var wr = new WeakReference<ISocketChannel>(channel);
            _posChannelMap.AddOrUpdate(posId, wr, (k, v) => wr);
        }

        public async Task SendTo<T>(T message, WorkerServiceDbContext context, Connection connection) where T : Message
        {
            var posId = message.POSData?.Id;
            if (string.IsNullOrWhiteSpace(posId))
                throw new Exception("POS ID Must be included in Message");


            if (_posChannelMap.TryGetValue(posId, out var wr))
            {
                //Console.WriteLine($"POSController Send Message");
                if (wr.TryGetTarget(out var channel))
                {
                    await channel.SendAsync(message).ConfigureAwait(false);
                    if (context != null)
                    {
                        string omess = await channel.Serialize<T>(message);
                        lock (context)
                        {
                            context.DataMessages.Add(new DataMessage() { OutMessage = omess, Connection = connection });
                            context.SaveChanges();
                            //await _context.SaveChangesAsync(_stoppingToken);
                        }
                    }
                }
                else
                {
                    //channel is dead, what to do?
                    _posChannelMap.TryRemove(posId, out var _);
                    var msg = $"Server {message.Id} PayBasket SubmitBasketRequest: POS ID [ {message.POSData.Id} ], Action[ {message.Action} ], Id[ {message.Id} ]"; ;
                    Console.WriteLine(msg);
                }
            }

        }

    }
}
