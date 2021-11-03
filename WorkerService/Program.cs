using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedLibrary;

namespace WorkerService
{
    //OSI Layer
    //https://www.imperva.com/learn/application-security/osi-model/

    //https://docs.microsoft.com/en-us/dotnet/core/extensions/windows-service
    //https://codinginfinite.com/multi-threaded-tcp-server-core-example-csharp/
    //https://github.com/Job79/EasyTcp

    //Client Server code from tutorial Richard Weeks
    //https://www.youtube.com/channel/UCUg_M6wvaS-DhHpFpW7aC6w

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<WorkerServiceDbContext>();
                    optionsBuilder.UseSqlServer("Server=(local);Database=WorkerService;Trusted_Connection=True;");//,
                    services.AddScoped<WorkerServiceDbContext>(s => new WorkerServiceDbContext(optionsBuilder.Options));

                    services.AddHostedService<Worker>();
                });
    }
}
