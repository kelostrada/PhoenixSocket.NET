using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private int _timeout;
        private int _heartbeatIntervalMs;
        private readonly Func<int, int> _reconnectAfterMs = tries => tries > 3 ? 10000 : new[] {1000, 2000, 5000}[tries - 1];
        private readonly Action<string, string, object> _logger = (kind, msg, data) => { };
        private readonly dynamic _params = new {};
        private string _endPoint;
        private Timer _reconnectTimer;

        /// <summary>
        /// Initializes the Socket
        /// </summary>
        /// <param name="endPoint">The string WebSocket endpoint, ie,
        /// "ws://example.com/ws", "wss://example.com", "/ws" (inherited host & protocol)</param>
        /// <param name="timeout">The default timeout in milliseconds to trigger push timeouts. Defaults `DefaultTimeout`</param>
        /// <param name="heartbeatIntervalMs">The millisec interval to send a heartbeat message</param>
        /// <param name="reconnectAfterMs">The optional function that returns the millsec reconnect interval.
        /// Defaults to stepped backoff of:
        /// tries =&gt; tries &gt; 3 ? 10000 : new[] {1000, 2000, 5000}[tries - 1];
        /// </param>
        /// <param name="logger">The optional function for specialized logging, ie:
        /// (kind, msg, data) =&gt; { Trace.WriteLine($"{kind}: {msg}, {data}"); }</param>
        /// <param name="urlparams">The optional params to pass when connecting</param>
        public Socket(string endPoint, int timeout = DefaultTimeout, int heartbeatIntervalMs = 30000, 
            Func<int, int> reconnectAfterMs = null, Action<string, string, object> logger = null,
            dynamic urlparams = null)
        {
            _timeout = timeout;
            _heartbeatIntervalMs = heartbeatIntervalMs;
            _reconnectAfterMs = reconnectAfterMs ?? _reconnectAfterMs;
            _logger = logger ?? _logger;
            _params = urlparams ?? _params;
            _endPoint = $"{endPoint}/{Transports[Transport.Websocket]}";
            _reconnectTimer = new Timer(() =>
            {
                Disconnect(Connect);
            }, _reconnectAfterMs);
        }

        private WebSocket _conn;

        public void Connect()
        {
            if (_conn != null) return;

            _conn = new WebSocket("");
            _conn.Opened += websocket_Opened;
            _conn.Error += ConnError;
            _conn.Closed += ConnClosed;
            _conn.MessageReceived += ConnMessageReceived;
            _conn.Open();
            
            while (true)
            {
                Thread.Sleep(5000);
                _conn.Send("{ \"topic\":\"phoenix\",\"event\":\"heartbeat\",\"payload\":{ },\"ref\":\"" + _ref++ + "\"}");
            }
        }

        public void Disconnect(Action callback)
        {
            
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
