using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixSocket
{
    public class Channel
    {
        private IPayload chanParams;
        private Socket socket;
        private string topic;

        public Channel(string topic, IPayload chanParams, Socket socket)
        {
            this.topic = topic;
            this.chanParams = chanParams;
            this.socket = socket;
        }

        public void Trigger(ChannelEvent error)
        {
            throw new NotImplementedException();
        }
    }
}
