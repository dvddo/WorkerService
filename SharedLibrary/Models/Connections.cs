using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models
{
    public enum EMessageType
    {
        JSON = 1,
        XML = 2
    }

    public enum ConnectionType
    {
        MATIP = 1,
        BATIP = 2,
    }

    public class Connection
    {
        public Guid Id { get; set; }
        public bool IsServer { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public ConnectionType Type { get; set; }
        public DateTime Created { get; set; }
        public string OutFolder { get; set; }
        public string InFolder { get; set; }
        public EMessageType MessageType { get; set;}
    }
}
