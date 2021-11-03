using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Linq;

namespace ServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
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
                    myService.Run();

                    Console.WriteLine("Success");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Occured");
                }
            }

            Console.WriteLine("Echo Server running");
            Console.ReadLine();


        }
    }
    public class MyApplication
    {
        private readonly WorkerServiceDbContext _context;

        public MyApplication(WorkerServiceDbContext context)
        {
            _context = context;
        }

        public void Run()
        {
            var conn = _context.Connections.Where(cnn => cnn.IsServer == true).FirstOrDefault();
            var server = new JsonServer(_context, conn, new System.Threading.CancellationToken());
            server.Start();
        }
    }
}
