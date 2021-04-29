using Newtonsoft.Json;

namespace MemberMailbox.Data
{
    public class UserInfo
    {
        [JsonProperty("data")]
        public UserInfoData Data { get; set; }
    }

    public class UserInfoData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("live_room")]
        public UserLiveroomData Liveroom { get; set; }
    }

    public class UserLiveroomData
    {
        [JsonProperty("roomid")]
        public string RoomId { get; set; }
    }
}
