using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Json
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class JsonRouteAttribute : RouteAttribute
    {
        public string Value { get; }
        public JsonRouteAttribute(string path, string value)
            : base(path) => Value = value;
    }
}
