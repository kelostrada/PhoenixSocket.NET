using Newtonsoft.Json;

namespace PhoenixSocket
{
    public class PushData
    {
        public string Topic { get; set; } = "";
        public string Event { get; set; } = "";
        public IPayload Payload { get; set; } = EmptyPayload.Instance;
        public int? Ref { get; set; } = null;

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
        {
            return Serialize();
        }
    }
}
