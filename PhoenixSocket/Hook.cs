using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixSocket
{
    internal class Hook
    {
        public string Status { get; set; }
        public Action<dynamic> Callback { get; set; }
    }
}
