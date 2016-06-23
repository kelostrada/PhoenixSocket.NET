using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixSocket
{
    public class Channel
    {
        private dynamic chanParams;
        private Socket socket;
        private string topic;

        public Channel(string topic, dynamic chanParams, Socket socket)
        {
            this.topic = topic;
            this.chanParams = chanParams;
            this.socket = socket;
        }

        public bool IsMember(string topic)
        {
            return true;
        }

        public void Trigger(string @event)
        {
            Trace.WriteLine($"Trigger channel event {@event}");
        }
        
        internal void Trigger(string @event, dynamic payload, string @ref)
        {
            Trigger(@event);
        }
    }
}
