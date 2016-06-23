using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;

namespace PhoenixSocket
{
    public class Socket4Net
    {
        private string _host;
        private UTF8Encoding encoding = new UTF8Encoding();
        private WebSocket _transport;

        public Socket4Net(string host)
        {
            _host = host;
        }

        private int _ref = 1;

        public async Task Connect()
        {
            _transport = new WebSocket(_host);
            _transport.Opened += new EventHandler(websocket_Opened);
            _transport.Error += _transport_Error;
            _transport.Closed += _transport_Closed;
            _transport.MessageReceived += _transport_MessageReceived;
            _transport.Open();
            
            while (true)
            {
                await Task.Delay(5000);
                _transport.Send("{ \"topic\":\"phoenix\",\"event\":\"heartbeat\",\"payload\":{ },\"ref\":\"" + _ref++ + "\"}");
            }
        }

        private void _transport_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine("Received: " + e.Message);
        }

        private void _transport_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine("Error " + e.Exception.Message);
        }

        private void _transport_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Closed");
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            _transport.Send("{\"topic\":\"quotes:EURUSD\",\"event\":\"phx_join\",\"payload\":{},\"ref\":\"" + _ref++ + "\"}");            
        }
    }
}
