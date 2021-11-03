#nullable enable

using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace SharedLibrary.Json {

    public class JsonMessageProtocol : Protocol<JObject>
    {
        public override Task<string> Serialize<T>(T message) => Task.FromResult(JsonSerialization.Serialize(message).ToString());

        protected override JObject Decode(byte[] message)
            => JsonSerialization.Deserialize(Encoding.UTF8.GetString(message));

        protected override byte[] EncodeBody<T>(T message)
            => Encoding.UTF8.GetBytes(JsonSerialization.Serialize(message).ToString());

        
    }
}
