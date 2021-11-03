using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ClientConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press Enter to Connect");
            Console.ReadLine();

            IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();

            var builder = new HostBuilder()
              .ConfigureServices((hostContext, services) =>
              {
                  services.AddTransient<MyApplication>().AddDbContext<WorkerServiceDbContext>(options =>
                  {
                      options.UseSqlServer("Server=(local);Database=WorkerService;Trusted_Connection=True;");
                  });

              }).UseConsoleLifetime();
            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    var myService = services.GetRequiredService<MyApplication>();
                    await myService.Run();

                    Console.WriteLine("Success");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Occured");
                }
            }

            Console.ReadLine();
        }
        public class MyApplication
        {
            private readonly WorkerServiceDbContext _context;

            public MyApplication(WorkerServiceDbContext context)
            {
                _context = context;
            }

            internal async Task Run()
            {
                var conn = _context.Connections.Where(cnn => cnn.IsServer == false).FirstOrDefault();
                var client = new JsonClient(conn.IPAddress, conn.Port, _context, conn, new System.Threading.CancellationToken());
                if (await client.Connect())
                {

                    var basket = new SubmitBasketRequest
                    {
                        Id = "TXN0007",
                        POSData = new POSData { Id = "POS001 " }
                    };

                    await client.SendAsync(basket).ConfigureAwait(false);
                }
            } 
        }
    }
}
