using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public interface ISocketChannel
    {
        Guid Id { get; }

        DateTime LastSent { get; }
        DateTime LastReceived { get; }

        event EventHandler Closed;
        bool IsClosed();
        void Attach(Socket socket);
        void Close();
        void Dispose();
        Task SendAsync<T>(T message);
        Task<string> Serialize<T>(T message);
    }
}
