using Newtonsoft.Json;

namespace MemberMailbox.Data
{
    public class SendResponseInfo
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
