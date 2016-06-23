namespace PhoenixSocket
{
    public enum ChannelState
    {
        Closed,
        Errored,
        Joined,
        Joining,
        Leaving
    }

    public enum ChannelEvent
    {
        Close,
        Error,
        Join,
        Reply,
        Leave
    }

    public enum Transport
    {
        Longpoll,
        Websocket
    }
}
