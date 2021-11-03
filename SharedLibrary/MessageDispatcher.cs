#nullable enable

using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public abstract class MessageDispatcher<TMessageType> where TMessageType : class, new()
    {

        readonly List<(RouteAttribute route, Func<ISocketChannel, TMessageType, Task<TMessageType?>> targetMethod)> _handlers = new List<(RouteAttribute route, Func<ISocketChannel, TMessageType, Task<TMessageType?>> targetMethod)>();

        //public Guid Id { get; set; }
        public WorkerServiceDbContext DBContext { get; set; }
        public Connection ConnModel { get; set; }

        public void Bind<TProtocol>(SocketChannel<TProtocol, TMessageType> channel, bool status)
            where TProtocol : Protocol<TMessageType>, new()
        {

            channel.OnMessage(async message =>
            {
                var response = await DispatchAsync(channel, message).ConfigureAwait(false);
                if (response != null)
                {
                    try
                    {
                        if (DBContext != null)
                        {
                            lock (DBContext)
                            {
                                DBContext.DataMessages.Add(new DataMessage() { InMessage = message.ToString(), OutMessage = response.ToString(), Connection = ConnModel });
                                DBContext.SaveChanges();
                                //await _context.SaveChangesAsync(_stoppingToken);
                            }
                        }
                        await channel.SendAsync(response).ConfigureAwait(false);
                    }
                    catch (Exception _e)
                    {
                        Console.WriteLine($"Oh NO!!! {_e}");
                    }
                }
            });
        }

        public void Bind<TController>(TController controller)
        {
            static bool returnTypeIsTask(MethodInfo mi)
             => mi.ReturnType.IsAssignableFrom(typeof(Task));

            static bool returnTypeIsTaskT(MethodInfo mi)
                => mi.ReturnType.BaseType?.IsAssignableFrom(typeof(Task)) ?? false;



#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var methods = controller.GetType()
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                            .GetMethods(BindingFlags.Instance | BindingFlags.Public) //| BindingFlags.Static
                                                             //must have a route
                            .Where(HasRouteAttribute)
                            //only support a single parameter
                            .Where(x => x.GetParameters().Count() >= 3 && x.GetParameters().Count() <= 4)
                            //only support methods that return a Task or Task<T>
                            .Where(x => returnTypeIsTask(x) || returnTypeIsTaskT(x));

            foreach (var mi in methods)
            {

                var wrapper = new Func<ISocketChannel, TMessageType, Task<TMessageType?>>(async (channel, msg) => {

                    var @param =
                    mi.GetParameters().Count() == 3
                         ? Deserialize(mi.GetParameters()[2].ParameterType, msg)
                         : Deserialize(mi.GetParameters()[3].ParameterType, msg);

                    try
                    {
                        if (returnTypeIsTask(mi))
                        {
                            var t = mi.GetParameters().Count() == 3
                                    ? (mi.Invoke(controller, new object[] { DBContext, ConnModel, @param }) as Task)
                                    : (mi.Invoke(controller, new object[] { DBContext, ConnModel, channel, @param }) as Task);

                            if (t != null)
                                await t;
                            return null;
                        }
                        else
                        {
                            var result = mi.GetParameters().Count() == 3
                            ? await (mi.Invoke(controller, new object[] { DBContext, ConnModel, @param }) as dynamic)
                            : await (mi.Invoke(controller, new object[] { DBContext, ConnModel, channel, @param }) as dynamic);

                            if (result != null)
                            {
                                return Serialize(result as dynamic);
                            }
                            else
                                return null;
                        }
                    }
                    catch (Exception _e)
                    {
                        //logging would go here & exception decisions happen here
                        Console.WriteLine(_e);
                        return null;
                    }
                });

#pragma warning disable CS8604 // Possible null reference argument.
                //routeAttribute is not null here - hence the suppression
                AddHandler(GetRouteAttribute(mi), wrapper);
#pragma warning restore CS8604 // Possible null reference argument.
            }
        }

        private bool HasRouteAttribute(MethodInfo mi) => GetRouteAttribute(mi) != null;

        public async Task<TMessageType?> DispatchAsync(ISocketChannel channel, TMessageType message)
        {
            foreach (var (route, target) in _handlers)
            {
                if (IsMatch(route, message))
                {
                    return await target(channel, message);
                }
            }
            //No handler?? what to do??
            return null;
        }

        protected void AddHandler(RouteAttribute route, Func<ISocketChannel, TMessageType, Task<TMessageType?>> targetMethod)
           => _handlers.Add((route, targetMethod));

        protected abstract bool IsMatch(RouteAttribute route, TMessageType message);

        public virtual void Register<TParam>(Func<TParam, Task> target)
            => Register(new Func<ISocketChannel, TParam, Task>((c, m) => target(m)));

        public virtual void Register<TParam>(Func<ISocketChannel, TParam, Task> target)
        {
            if (!HasAttribute(target.Method))
                throw new Exception("Missing Required Route Attribute");

            var wrapper = new Func<ISocketChannel, TMessageType, Task<TMessageType?>>(async (channel, xml) => {
                var @param = Deserialize<TParam>(xml);
                await target(channel, @param);
                return null;
            });

#pragma warning disable CS8604 // Possible null reference argument.
            AddHandler(GetRouteAttribute(target.Method), wrapper);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        public virtual void Register<TParam, TResult>(Func<TParam, Task<TResult>> target)
            => Register(new Func<ISocketChannel, TParam, Task<TResult>>((c, x) => target(x)));

        public virtual void Register<TParam, TResult>(Func<ISocketChannel, TParam, Task<TResult>> target)
        {
            if (!HasAttribute(target.Method))
                throw new Exception("Missing Required Route Attribute");

            var wrapper = new Func<ISocketChannel, TMessageType, Task<TMessageType?>>(async (channel, xml) => {
                var @param = Deserialize<TParam>(xml);
                var result = await target(channel, @param);
                return result != null ? Serialize(result) : null;
            });

#pragma warning disable CS8604 // Possible null reference argument.
            AddHandler(GetRouteAttribute(target.Method), wrapper);
#pragma warning restore CS8604 // Possible null reference argument.
        }


        protected abstract TParam Deserialize<TParam>(TMessageType message);
        protected abstract object Deserialize(Type paramType, TMessageType message);

        protected abstract TMessageType? Serialize<T>(T instance);



        protected bool HasAttribute(MethodInfo mi) => GetRouteAttribute(mi) != null;
        protected abstract RouteAttribute? GetRouteAttribute(MethodInfo mi);


    }
}
