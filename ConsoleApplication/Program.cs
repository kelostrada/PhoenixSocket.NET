using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {

            //var host = "ws://localhost:4000/socket/websocket?vsn=1.0.0";
            var host = "wss://calm-peak-50914.herokuapp.com/socket/websocket?token=&vsn=1.0.0";
            var socket = new PhoenixSocket.Socket4Net(host);
            Task.Run(() => socket.Connect());

            Console.ReadKey();
        }
    }
}
