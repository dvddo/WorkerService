#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using SharedLibrary;
using SharedLibrary.Messages;
using SharedLibrary.Models;

namespace ServerConsole
{
    public class TransactionManager
    {

        readonly List<SubmitBasketRequest> _transactions = new List<SubmitBasketRequest>();


        public POSController? POSController { get; set; }


        public void ProcessBasket(SubmitBasketRequest r, Connection connection)
        {
            //var msg = $"Server {connection.Id} ProcessBasket SubmitBasketRequest: POS ID [ {r.POSData.Id} ], Action[ {r.Action} ], Id[ {r.Id} ]"; ;
            //Console.WriteLine(msg);
            _transactions.Add(r); 
        }

        public async Task PayBasket(WorkerServiceDbContext context, Connection connection)
        {

            if (_transactions.Count > 0)
            {

                var basket = _transactions[0];
                _transactions.RemoveAt(0);


                var payBasketRequest = new BasketPaidRequest
                {
                    Id = basket.Id.ToString(),
                    POSTransactionNumber = basket.POSTransactionNumber,
                    POSData = basket.POSData,
                    PaymentInfo = new PaymentInfo
                    {
                        Amount = 50.00m,
                        AuthorizationCode = "AUTH1234",
                        LastFour = "9876"
                    }
                };
                try
                {


#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    await POSController.SendTo(payBasketRequest, context, connection).ConfigureAwait(false);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    //var msg = $"Server {connection.Id} PayBasket SubmitBasketRequest: POS ID [ {payBasketRequest.POSData.Id} ], Action[ {payBasketRequest.Action} ], Id[ {payBasketRequest.Id} ]"; ;
                    //Console.WriteLine(msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }


    }
}
