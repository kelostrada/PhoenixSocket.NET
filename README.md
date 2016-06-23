# PhoenixSocket.NET
Phoenix Framework WebSocket Channels implemented as .NET Client

Ported from https://github.com/phoenixframework/phoenix/blob/master/web/static/js/phoenix.js
(by making some necessary changes to adapt the differences between `C#` and `ES6`)

## Examples

You can check `ConsoleApplication` project, although in the future it will be replaced with UnitTests.

To test it out you need some simple `Phoenix Framework` project with configured `Channels`. I used the one explained in the tutorial: http://www.phoenixframework.org/docs/channels

### Example Usage:

``` C#
    var host = "ws://localhost:4000/socket";
    var socket = new PhoenixSocket.Socket(host,
        logger: (kind, msg, data) => Console.WriteLine($"{kind}: {msg}, \n" + JsonConvert.SerializeObject(data)));

    socket.Connect();
    
    var channel = socket.Channel("rooms:lobby");
    channel.On("new_msg", msg =>
    {
        Console.WriteLine("New message: " + JsonConvert.SerializeObject(msg));
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
