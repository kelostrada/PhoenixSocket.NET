using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhoenixSocket
{
    public class Socket
    {
        private string _host;
        private ClientWebSocket _transport;
        private UTF8Encoding encoding = new UTF8Encoding();

        public Socket(string host)
        {
            _host = host;
        }

        public async Task Connect()
        {
            Thread.Sleep(1000); //wait for a sec, so server starts and ready to accept connection..
            
            try
            {
                _transport = new ClientWebSocket();
                await _transport.ConnectAsync(new Uri(_host + "?vsn=1.0.0"), CancellationToken.None);

                //await Task.WhenAll(Receive(webSocket), Send(webSocket));

                Thread.Sleep(1000);

                await Send();

                while (true)
                {
                    await Receive();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
            finally
            {
                if (_transport != null)
                    _transport.Dispose();
                Console.WriteLine();
                Console.WriteLine("WebSocket closed.");
            }
        }

        private async Task Send()
        {
            if (_transport.State == WebSocketState.Open)
            {
                byte[] buffer = encoding.GetBytes("{\"topic\":\"quotes: BTCCNY\",\"event\":\"phx_join\",\"payload\":{},\"ref\":\"1\"}");
                await _transport.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, false, CancellationToken.None);
            }
        }

        private async Task Receive()
        {
            byte[] buffer = new byte[1024];
            while (_transport.State == WebSocketState.Open)
            {
                var result = await _transport.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _transport.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    Console.WriteLine("Receive:  " + Encoding.UTF8.GetString(buffer).TrimEnd('\0'));
                }
            }
        }
    }
}
