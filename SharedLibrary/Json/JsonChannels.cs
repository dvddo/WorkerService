using Newtonsoft.Json.Linq;

namespace SharedLibrary.Json
{
    public class JsonSocketChannel : SocketChannel<JsonMessageProtocol, JObject> { }

    public class JsonClientSocketChannel : ClientSocketChannel<JsonMessageProtocol, JObject> { }

}
