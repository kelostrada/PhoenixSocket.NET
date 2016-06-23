using System;

namespace PhoenixSocket
{
    internal class Hook
    {
        public string Status { get; set; }
        public Action<dynamic> Callback { get; set; }
    }
}
