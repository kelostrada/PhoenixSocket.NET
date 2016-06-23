using System;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;

namespace PhoenixSocket
{
    internal class Push
    {
        private readonly Channel _channel;
        private readonly string _event;
        private readonly dynamic _payload;
        private dynamic _receivedResp;
        private int _timeout;
        private Timer _timeoutTimer;
        private readonly List<Hook> _recHooks;
        public bool Sent { get; private set; }
        private string _ref;
        private string _refEvent;

        /// <summary>
        /// Initializes the Push
        /// </summary>
        /// <param name="channel">The Channel</param>
        /// <param name="event">The event, for example `"phx_join"`</param>
        /// <param name="payload">The payload, for example `{user_id: 123}`</param>
        /// <param name="timeout">The push timeout in milliseconds</param>
        public Push(Channel channel, string @event, dynamic payload, int timeout)
        {
            _channel = channel;
            _event = @event;
            _payload = payload;
            _receivedResp = null;
            _timeout = timeout;
            _timeoutTimer = null;
            _recHooks = new List<Hook>();
            Sent = false;
        }

        public void Resend(int timeout)
        {
            _timeout = timeout;
            CancelRefEvent();
            _ref = null;
            _refEvent = null;
            _receivedResp = null;
            Sent = false;
            Send();
        }
        
        public void Send()
        {
            StartTimeout();
            Sent = true;
            _channel.Socket.Push(new PushData
            {
                Topic = _channel.Topic,
                Event = _event,
                Payload = _payload,
                Ref = _ref
            });
        }

        public Push Receive(string status, Action<dynamic> callback)
        {
            if (HasReceived(status))
            {
                dynamic response = null;
                try
                {
                    response = _receivedResp.response;
                }
                catch (RuntimeBinderException)
                {
                    // property doesn't exist
                }
                callback(response);
            }

            _recHooks.Add(new Hook {Status = status, Callback = callback});
            return this;
        }

        // private

        private void MatchReceive(dynamic payload)
        {
            dynamic status;
            dynamic response;

            try
            {
                status = payload.status;
                response = payload.response;
            }
            catch (RuntimeBinderException)
            {
                // properties don't exist
                return;
            }

            _recHooks.FindAll(hook => hook.Status == status).ForEach(hook => hook.Callback(response));
        }

        private void CancelRefEvent()
        {
            if (_refEvent == null) return;
            _channel.Off(_refEvent);
        }

        private void CancelTimeout()
        {
            _timeoutTimer?.Reset();
            _timeoutTimer?.Dispose();
            _timeoutTimer = null;
        }

        private void StartTimeout()
        {
            if (_timeoutTimer != null) return;
            _ref = _channel.Socket.MakeRef();
            _refEvent = _channel.ReplyEventName(_ref);

            _channel.On(_refEvent, payload =>
            {
                CancelRefEvent();
                CancelTimeout();
                _receivedResp = payload;
                MatchReceive(payload);
            });

            _timeoutTimer = new Timer(() => Trigger("timeout", new {}), _ => _timeout);
            _timeoutTimer.ScheduleTimeout();
        }

        private bool HasReceived(string status)
        {
            try
            {
                return _receivedResp != null && _receivedResp.status == status;
            }
            catch (RuntimeBinderException)
            {
                return false;
            }
        }

        private void Trigger(string status, dynamic response)
        {
            _channel.Trigger(_refEvent, status, response);
        }

        
        
    }
}
