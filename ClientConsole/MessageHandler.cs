using System;
using System.Threading.Tasks;

using SharedLibrary;
using SharedLibrary.Json;
using SharedLibrary.Messages;
using SharedLibrary.Models;
using SharedLibrary.Xml;

namespace ClientConsole
{
    public class MessageHandler
    {
        //Handler on the 'Client' side of the system
        [XPathRoute("/Message[@type='Response' and @action='HeartBeat']")]
        [JsonRoute("$.action", "HeartBeat")]
        public Task HandleMessage(WorkerServiceDbContext context, Connection connection, HeartBeatResponseMessage response)
        {
            Received(response, context, connection);
            return Task.CompletedTask;
        }

        [XPathRoute("/Message[@type='Response' and @action='SubmitBasket']")]
        [JsonRoute("$.action", "SubmitBasket")]
        public Task HandleMessage(WorkerServiceDbContext context, Connection connection, SubmitBasketResponse response)
        {
            Received(response, context, connection);
            return Task.CompletedTask;
        }

        [XPathRoute("/Message[@type='Request' and @action='BasketPaid']")]
        [JsonRoute("$.action", "BasketPaid")]
        public Task HandleMessage(WorkerServiceDbContext context, Connection connection, BasketPaidRequest request)
        {
            Received(request, context, connection);

            //Console.WriteLine("==========================[ Basket Paid ]==================================");
            //Console.WriteLine($"Transaction: {request.POSTransactionNumber}");
            //Console.WriteLine($"     Amount: {request.PaymentInfo.Amount:C}");
            //Console.WriteLine($"       Card: ************{request.PaymentInfo.LastFour}");

            LogMessage(context, connection, $"Client {connection.Id} Transaction: {request.POSTransactionNumber}      Amount: {request.PaymentInfo.Amount:C}        Card: ************{request.PaymentInfo.LastFour}");

            return Task.CompletedTask;
        }

        private void LogMessage(WorkerServiceDbContext context, Connection connection, string message)
        {
            if (context != null)
            {
                lock (context)
                {
                    context.DebugLogs.Add(new DebugLog() { Connection = connection, Detail = message });
                    context.SaveChanges();
                }
            }
        }

        private void Received<T>(T msg, WorkerServiceDbContext context, Connection connection) where T : Message
        {
            var smsg = $"Client {connection.Id} Received {typeof(T).Name}: POS ID [ {msg.POSData.Id} ], Action[ {msg.Action} ], Id[ {msg.Id} ]";
            LogMessage(context, connection, smsg);
            //Console.WriteLine(smsg);
        }


    }
}
