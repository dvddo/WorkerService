
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SharedLibrary.Models;
using SharedLibrary;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private ConcurrentDictionary<Guid, object> _connections;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _connections = new ConcurrentDictionary<Guid, object>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WorkerServiceDbContext>();

            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    if (!dbContext.Connections.Any())
                    {
                        dbContext.Connections.Add(new Connection() { IPAddress = "127.0.0.1", Port = 8080, IsServer = true, MessageType = EMessageType.JSON });
                        dbContext.Connections.Add(new Connection() { IPAddress = "127.0.0.1", Port = 8080, IsServer = false, MessageType = EMessageType.JSON });
                        dbContext.Connections.Add(new Connection() { IPAddress = "127.0.0.1", Port = 8080, IsServer = false, MessageType = EMessageType.JSON });
                        dbContext.Connections.Add(new Connection() { IPAddress = "127.0.0.1", Port = 8081, IsServer = true, MessageType = EMessageType.XML });
                        dbContext.Connections.Add(new Connection() { IPAddress = "127.0.0.1", Port = 8081, IsServer = false, MessageType = EMessageType.XML });
                        dbContext.Connections.Add(new Connection() { IPAddress = "127.0.0.1", Port = 8081, IsServer = false, MessageType = EMessageType.XML });
                        dbContext.SaveChanges();
                        break;
                    }
                    else
                    {
                        foreach (var conn in dbContext.Connections.Where(cnn => cnn.IsServer == true).ToList())
                        {
                            if (!_connections.ContainsKey(conn.Id))
                            {
                                //Thread t = new Thread(delegate ()
                                {
                                    var port = (ushort)conn.Port;
                                    //Server server = new Server(conn.IPAddress, port);

                                    /**/
                                    switch (conn.MessageType)
                                    {
                                        case EMessageType.JSON:
                                            var jsonServer = new ServerConsole.JsonServer(dbContext, conn, stoppingToken);
                                            jsonServer.Start();
                                            _connections[conn.Id] = jsonServer;
                                            break;
                                        case EMessageType.XML:
                                            var xmlServer = new ServerConsole.XmlServer(dbContext, conn, stoppingToken);
                                            xmlServer.Start();
                                            _connections[conn.Id] = xmlServer;
                                            break;
                                    }

                                }
                                //);
                                //t.Start();

                            }
                        }
                        //if(false)
                        foreach (var conn in dbContext.Connections.Where(cnn => cnn.IsServer == false).ToList())
                        {
                            if (!_connections.ContainsKey(conn.Id))
                            {
                                await Task.Delay(100, stoppingToken);

                                var basket = new SubmitBasketRequest
                                {
                                    Id = $"{conn.Id}",
                                    POSData = new POSData { Id = $"POS_{conn.Id}" }
                                };

                                switch (conn.MessageType)
                                {
                                    case EMessageType.JSON:
                                
                                        var clientJson = new ClientConsole.JsonClient(conn.IPAddress, conn.Port, dbContext, conn, stoppingToken);
                                        clientJson.ChannelClosed += (s, e) => _connections.TryRemove(clientJson._connection.Id, out var _);
                                        if (await clientJson.Connect())
                                        {

                                            await clientJson.SendAsync(basket).ConfigureAwait(false);

                                            _connections[conn.Id] = clientJson;
                                        }
                                        break;
                                    case EMessageType.XML:

                                        var clientXml = new ClientConsole.XmlClient(conn.IPAddress, conn.Port, dbContext, conn, stoppingToken);
                                        clientXml.ChannelClosed += (s, e) => _connections.TryRemove(clientXml._connection.Id, out var _);
                                        if (await clientXml.Connect())
                                        {

                                            await clientXml.SendAsync(basket).ConfigureAwait(false);

                                            _connections[conn.Id] = clientXml;
                                        }
                                        break;

                                }
                            }
                        }
                    }
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(100 * 1000, stoppingToken);
                    //Console.WriteLine($"Number of connection {_connections.Count()}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Worker ExecuteAsync Exception {ex}");
                }
            }
        }
    }
}
