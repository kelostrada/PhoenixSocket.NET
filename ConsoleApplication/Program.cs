using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleApplication
{
    internal class Program
    {
        static void Main()
        {
            var host = "ws://localhost:4000/socket";
            var socket = new PhoenixSocket.Socket(host,
                logger: (kind, msg, data) => Console.WriteLine($"{kind}: {msg}, \n" + JsonConvert.SerializeObject(data)));

            Task.Run(() => socket.Connect());

            var channel = socket.Channel("rooms:lobby");
            channel.On("new_msg", msg =>
            {
                Console.WriteLine("New message: " + JsonConvert.SerializeObject(msg));
            });
            channel.Join();

            Thread.Sleep(1000);

            channel.Leave();
            
            Thread.Sleep(1000);
            
            channel = socket.Channel("rooms:lobby");
            channel.On("new_msg", msg =>
            {
                Console.WriteLine("New message: " +  msg.body);
            });
            channel.Join();

            var message = "";

            while (message?.Trim() != "q")
            {
                message = Console.ReadLine();
                channel.Push("new_msg", new { body = message });
            }

            channel.Leave();
            socket.Disconnect(null);
        }
    }
}
