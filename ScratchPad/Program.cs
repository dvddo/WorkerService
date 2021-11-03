#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ScratchPad
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RouteAttribute : Attribute
    {
        public string Path { get; }
        public RouteAttribute(string path) => Path = path;
    }

    public abstract class MessageDispatcher<TMessageType> where TMessageType : class, new()
    {
        public abstract void Register<TParam, TResult>(Func<TParam, Task<TResult>> target);
        public abstract void Register<TParam>(Func<TParam, Task> target);
        public abstract Task<TMessageType?> DispatchAsync(TMessageType message);
    }

    public class XDocumentDispatcher : MessageDispatcher<XDocument>
    {
        readonly List<(string xpathExpression, Func<XDocument, Task<XDocument?>> targetMethod)> _handlers = new List<(string xpathExpression, Func<XDocument, Task<XDocument?>> targetMethod)>();
        public override async Task<XDocument?> DispatchAsync(XDocument message)
        {
            foreach(var (xpath, target) in _handlers)
            {
                if((message.XPathEvaluate(xpath) as bool?)==true)
                {
                    return await target(message);
                }
            }
            return null;
        }

        public override void Register<TParam, TResult>(Func<TParam, Task<TResult>> target)
        {
            var xpathRouteExpression = GetXPathRoute(target.Method);

            var wrapper = new Func<XDocument, Task<XDocument?>>(
                async xml => {
                    var param = XmlSerialization.Deserialize<TParam>(xml);
                    var result = await target(param);

                    if (result != null)
                        return XmlSerialization.Serialize(result);
                    else
                        return null;
                });

            _handlers.Add((xpathRouteExpression, wrapper));
        }


        public override void Register<TParam>(Func<TParam, Task> target)
        {
            var xpathRouteExpression = GetXPathRoute(target.Method);

            var wrapper = new Func<XDocument, Task<XDocument?>>(async xml => {
                var param = XmlSerialization.Deserialize<TParam>(xml);
                await target(param);
                return null;
            });

            _handlers.Add((xpathRouteExpression, wrapper));
        }

        string GetXPathRoute(MethodInfo methodInfo)
        {
            var routeAttribute = methodInfo.GetCustomAttribute<RouteAttribute>();
            if (routeAttribute == null)
                throw new ArgumentException($"Method {methodInfo.Name} missing required RouteAttribute");
            return $"boolean({routeAttribute.Path})";
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var messageHandler = new XDocumentDispatcher();

            messageHandler.Register<HeartBeatRequestMessage, HeartBeatResponseMessage>(HeartBeatRequestHandler);
            messageHandler.Register<HeartBeatResponseMessage>(HeartBeatResponsetHandler);

            var hbRequest = new HeartBeatRequestMessage { Id = "HB_001", POSData = new POSData { Id = "POS_001" } };
            var hbRequestDoc = XmlSerialization.Serialize(hbRequest);

            var responseXDoc = await messageHandler.DispatchAsync(hbRequestDoc);
            if (responseXDoc != null)
                await messageHandler.DispatchAsync(responseXDoc);
        }

        [Route("/Message[@type='Request' and @action='HeartBeat']")]
        public static Task<HeartBeatResponseMessage> HeartBeatRequestHandler(HeartBeatRequestMessage request)
        {
            var response = new HeartBeatResponseMessage {
                Id = request.Id,
                POSData = request.POSData,
                Result = new Result { Status = Status.Success}
            };

            return Task.FromResult(response);
        }

        [Route("/Message[@type='Response' and @action='HeartBeat']")]
        public static Task HeartBeatResponsetHandler(HeartBeatResponseMessage response)
        {
            Console.WriteLine($"Received Response: {response?.Result?.Status}, {response?.Id}");
            return Task.CompletedTask;
        }
    }
}
