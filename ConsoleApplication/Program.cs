using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {

            //var host = "ws://localhost:4000/socket/websocket?vsn=1.0.0";
            var host = "wss://calm-peak-50914.herokuapp.com/socket";
            var socket = new PhoenixSocket.Socket(host,
                logger: (kind, msg, data) => Console.WriteLine($"{kind}: {msg}, \n" + JsonConvert.SerializeObject(data)));

            Task.Run(() => socket.Connect());

            var channel = socket.Channel("quotes:EURUSD");
            channel.On("receive", quote =>
            {
                Console.WriteLine("QUOTE: " + JsonConvert.SerializeObject(quote));
            });
            channel.Join();

            Thread.Sleep(5000);

            channel.Leave();

            Console.ReadKey();
        }
    }
}
