using System;
using System.Threading.Tasks;

using SharedLibrary;
using SharedLibrary.Json;
using SharedLibrary.Models;
using SharedLibrary.Xml;

namespace ServerConsole
{
    public class MessageHandler
    {

        //should be IoC/DI 
        public  POSController POSController;
        public  TransactionManager TransactionManager { get; set; }

        //Handler on the 'Server' side of the system
        [XPathRoute("/Message[@type='Request' and @action='HeartBeat']")]
        [JsonRoute("$.action", "HeartBeat")]
        public Task<HeartBeatResponseMessage> HandleMessage(WorkerServiceDbContext context, Connection connection, ISocketChannel channel, HeartBeatRequestMessage request)
        {

            Received(channel, request, context, connection);

            POSController.ProcessHeartBeat(request.POSData.Id, channel);


            var response = new HeartBeatResponseMessage
            {
                Id = request.Id,
                POSData = request.POSData,
                Result = new Result { Status = Status.Success }
            };
            Sending(response, context, connection);
            return Task.FromResult(response);
        }

        //Handler on the 'Server' side of the system
        [XPathRoute("/Message[@type='Request' and @action='SubmitBasket']")]
        [JsonRoute("$.action", "SubmitBasket")]
        public Task<SubmitBasketResponse> HandleMessage(WorkerServiceDbContext context, Connection connection, SubmitBasketRequest request)
        {
            Received(request, context, connection);

            TransactionManager.ProcessBasket(request, connection);

            var response = new SubmitBasketResponse
            {
                Id = request.Id,
                POSData = request.POSData,
                Result = new Result { Status = Status.Success }
            };
            Sending(response, context, connection);
            return Task.FromResult(response);
        }

        private void LogMessage(WorkerServiceDbContext context, Connection connection, string message)
        {
            if(context!=null)
            {
                lock(context)
                {
                    context.DebugLogs.Add(new DebugLog() { Connection = connection, Detail = message });
                    context.SaveChanges();
                }
            }
        }

        private void Received<T>(ISocketChannel channel, T msg, WorkerServiceDbContext context, Connection connection) where T : Message
        {
            var smsg = $"Server {connection.Id} Received {typeof(T).Name} From Channel {channel.Id}: POS ID [ {msg.POSData.Id} ], Action[ {msg.Action} ], Id[ {msg.Id} ]";
            LogMessage(context, connection, smsg);
            //Console.WriteLine(smsg); 
        }

        private void Received<T>(T msg, WorkerServiceDbContext context, Connection connection) where T : Message
        {
            var smsg = $"Server {connection.Id} Received {typeof(T).Name}: POS ID [ {msg.POSData.Id} ], Action[ {msg.Action} ], Id[ {msg.Id} ]";
            LogMessage(context, connection, smsg);
            //Console.WriteLine(smsg); 
        }

        private void Sending<T>(T msg, WorkerServiceDbContext context, Connection connection) where T : Message
        {
            var smsg = $"Server {connection.Id} Sending {typeof(T).Name}: POS ID [ {msg.POSData.Id} ], Action[ {msg.Action} ], Id[ {msg.Id} ]";
            LogMessage(context, connection, smsg);
            //Console.WriteLine(smsg); 
        }
    }
}
