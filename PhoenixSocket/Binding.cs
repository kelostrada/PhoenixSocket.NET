using System;

namespace PhoenixSocket
{
    internal class Binding
    {
        public string Event { get; set; }
        public Action<dynamic, string> Callback { get; set; }
    }
}
