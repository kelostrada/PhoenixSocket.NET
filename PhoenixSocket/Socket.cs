using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;

namespace PhoenixSocket
{
    /// <summary>
    /// WebSocket connection to Phoenix Framework Channels 
    /// 
    /// Based on commit: dffe05346e1b8b159dfdde418774dba5fed82a3f
    /// In https://github.com/phoenixframework/phoenix/blob/master/web/static/js/phoenix.js
    /// </summary>
    public class Socket
    {
        #region Constants

        public const string Vsn = "1.0.0";
        public const int DefaultTimeout = 10000;

        public static Dictionary<ChannelState, string> ChannelStates = new Dictionary<ChannelState, string>
        {
            {ChannelState.Closed, "closed"},
            {ChannelState.Errored, "errored" },
            {ChannelState.Joined, "joined" },
            {ChannelState.Joining, "joining" },
            {ChannelState.Leaving, "leaving" },
        };

        public static Dictionary<ChannelEvent, string> ChannelEvents = new Dictionary<ChannelEvent, string>
        {
            {ChannelEvent.Close, "phx_close" },
            {ChannelEvent.Error, "phx_error" },
            {ChannelEvent.Join, "phx_join" },
            {ChannelEvent.Reply, "phx_reply" },
            {ChannelEvent.Leave, "phx_leave" },
        };

        public static Dictionary<Transport, string> Transports = new Dictionary<Transport, string>
        {
            {Transport.Longpoll, "longpoll" },
            {Transport.Websocket, "websocket" }
        };

        #endregion

        private List<Channel> _channels = new List<Channel>();
        private List<Action> _sendBuffer = new List<Action>();
        private int _ref;
        private int _timeout = DefaultTimeout;
        private int _heartbeatIntervalMs = 30000;
        private Func<int, int> _reconnectAfterMs = tries => tries > 3 ? 10000 : new[] {1000, 2000, 5000}[tries - 1];
        private Action<string, string, object> _logger = (kind, msg, data) => { };
        private dynamic _params = new {};
        private string _endPoint;


        public Socket(string endPoint)
        {
        }

        private WebSocket _conn;

        public async Task Connect()
        {
            _conn = new WebSocket("");
            _conn.Opened += websocket_Opened;
            _conn.Error += ConnError;
            _conn.Closed += ConnClosed;
            _conn.MessageReceived += ConnMessageReceived;
            _conn.Open();
            
            while (true)
            {
                await Task.Delay(5000);
                _conn.Send("{ \"topic\":\"phoenix\",\"event\":\"heartbeat\",\"payload\":{ },\"ref\":\"" + _ref++ + "\"}");
            }
        }

        private void ConnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine("Received: " + e.Message);
        }

        private void ConnError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine("Error " + e.Exception.Message);
        }

        private void ConnClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Closed");
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            _conn.Send("{\"topic\":\"quotes:EURUSD\",\"event\":\"phx_join\",\"payload\":{},\"ref\":\"" + _ref++ + "\"}");            
        }
    }
}
