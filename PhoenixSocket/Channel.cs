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
        public Socket Socket { get; private set; }
        public string Topic { get; private set; }

        private int _timeout;

        public Channel(string topic, dynamic chanParams, Socket socket)
        {
            this.Topic = topic;
            this.chanParams = chanParams;
            this.Socket = socket;
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

        public void Join(int timeout = 0)
        {
            if (timeout == 0) timeout = _timeout;

        }

        public void Off(string refEvent)
        {
            throw new NotImplementedException();
        }

        public string ReplyEventName(string @ref)
        {
            throw new NotImplementedException();
        }

        public void On(string refEvent, Action<dynamic> action)
        {
            throw new NotImplementedException();
        }
    }
}
