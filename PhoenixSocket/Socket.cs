using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SuperSocket.ClientEngine;
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
        private readonly Dictionary<string, string> _params = new Dictionary<string, string>();
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

        private string EndPointUrl()
        {
            var uriBuilder = new UriBuilder(_endPoint);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var p in _params) query.Set(p.Key, p.Value);
            query.Set("vsn", Vsn);
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
            // Note: Removed checking protocol, because this will never get called 
            // from browser so we don't have javascript's "location" object.
        }

        private WebSocket _conn;

        public void Disconnect(Action callback, int? code = null, string reason = "")
        {
            if (_conn != null)
            {
                _conn.Closed -= OnConnClose; // noop
                if (code != null)
                {
                    _conn.Close(code.Value, reason);
                }
                else
                {
                    _conn.Close();
                }
                _conn = null;
            }

            callback?.Invoke();
        }

        public void Connect()
        {
            // Note: Didn't implement deprecated params
            if (_conn != null) return;

            _conn = new WebSocket(EndPointUrl());
            _conn.Opened += OnConnOpen;
            _conn.Error += OnConnError;
            _conn.MessageReceived += OnConnMessage;
            _conn.Closed += OnConnClose;
            _conn.Open();
            
            /*
            while (true)
            {
                Thread.Sleep(5000);
                _conn.Send("{ \"topic\":\"phoenix\",\"event\":\"heartbeat\",\"payload\":{ },\"ref\":\"" + _ref++ + "\"}");
            }
            */
        }

        private void Log(string kind, string msg, object data)
        {
            _logger(kind, msg, data);
        }
        
        #region Event Handlers

        public event EventHandler Opened;
        public event EventHandler Closed;
        public event EventHandler<ErrorEventArgs> Error;
        public event EventHandler<MessageReceivedEventArgs> Message;

        #endregion

        private void OnConnOpen(object sender, EventArgs eventArgs)
        {
            Log("transport", $"connected to {EndPointUrl()}", Transports[Transport.Websocket]);
            //FlushSendBuffer();
        }

        private void OnConnClose(object sender, EventArgs eventArgs)
        {
            
        }

        private void OnConnError(object sender, ErrorEventArgs errorEventArgs)
        {
            
        }

        private void TriggerChanError()
        {
            _channels.ForEach(channel => channel.Trigger(ChannelEvent.Error));
        }

        public string ConnectionState()
        {
            if (_conn == null) return "closed";
            return _conn.State.ToString().ToLower();
        }

        public bool IsConnected()
        {
            return ConnectionState() == "open";
        }

        public void Remove(Channel channel)
        {
            _channels.Remove(channel); 
            // TODO: easier way to do this by reference, although not exactly the same as JS Client
        }

        public Channel Channel(string topic, IPayload chanParams)
        {
            chanParams = chanParams ?? EmptyPayload.Instance;
            var chan = new Channel(topic, chanParams, this);
            _channels.Add(chan);
            return chan;
        }

        public void Push(PushData data)
        {
            Action callback = () => _conn.Send(data.Serialize());
            Log("push", $"{data.Topic} {data.Event} ({data.Ref})", data.Payload);
            if (IsConnected())
            {
                callback();
            }
            else
            {
                _sendBuffer.Add(callback);
            }
        }
        
        public string MakeRef()
        {
            var newRef = _ref + 1;
            _ref = newRef == _ref ? 0 : newRef;
            return _ref.ToString();
        }

        public void SendHeartbeat()
        {
            if (!IsConnected()) return;
            Push(new PushData
            {
                Topic = "phoenix",
                Event = "heartbeat",
                Ref = MakeRef()
            });
        }

        private void FlushSendBuffer()
        {
            if (IsConnected() && _sendBuffer.Count > 0)
            {
                _sendBuffer.ForEach(callback => callback());
                _sendBuffer.Clear();
            }
        }
        
        private void OnConnMessage(object sender, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            
        }
        
    }
}
