using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace PhoenixSocket.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            // wss://calm-peak-50914.herokuapp.com/socket/websocket?token=&vsn=1.0.0
            var host = "wss://calm-peak-50914.herokuapp.com/socket/websocket";

            var socket = new Socket(host);
            await socket.Connect();


        }
    }
}
