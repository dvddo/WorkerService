using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models
{
    public class DataMessage
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public Connection Connection { get; set; }
        public string OutMessage { get; set; }
        public string InMessage { get; set; }
    }

    public class DebugLog
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public Connection Connection { get; set; }
        public string Detail { get; set; }
    }
}
