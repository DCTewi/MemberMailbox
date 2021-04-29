using MemberMailbox.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MemberMailbox.Services
{
    public class MailboxProxy
    {
        private static long GetTimeStamp() => (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;

        private ClientProxy ClientProxy { get; init; }

        private string DevId { get; init; } = Guid.NewGuid().ToString().ToUpper();

        public MailboxProxy(string rawCookie)
        {
            ClientProxy = new(rawCookie);
        }

        public async Task<SendResponseInfo> SendMessageAsync(string senderid, string uid, string msg)
        {
            var apiurl = "https://api.vc.bilibili.com/web_im/v1/web_im/send_msg";

            Dictionary<string, string> para = new()
            {
                { "msg[sender_uid]", senderid },
                { "msg[receiver_id]", uid },
                { "msg[receiver_type]", "1" },
                { "msg[msg_type]", "1" },
                { "msg[msg_status]", "0" },
                { "msg[content]", $"{{\"content\":\"{msg}\"}}" },
                { "msg[timestamp]", GetTimeStamp().ToString() },
                { "msg[new_face_version]", "0" },
                { "msg[dev_id]", DevId },
            };


            var response = await ClientProxy.PostAsync(para, apiurl);

            if (response.IsSuccessStatusCode)
            {
                var rawjson = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<SendResponseInfo>(rawjson);
            }

            return null;
        }
        public async Task<UserInfo> GetUserInfoAsync(string mid)
        {
            var apiurl = "http://api.bilibili.com/x/space/acc/info" +
                $"?mid={mid}";

            var response = await ClientProxy.GetAsync(apiurl);

            if (response.IsSuccessStatusCode)
            {
                var rawjson = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<UserInfo>(rawjson);
            }

            return null;
        }

        public async Task<List<MemberInfo>> GetAllMembersAsync(string uid)
        {
            var userinfo = await GetUserInfoAsync(uid);

            if (userinfo != null)
            {
                var roomid = userinfo.Data.Liveroom.RoomId;

                var apiurl = "https://api.live.bilibili.com/xlive/app-room/v2/guardTab/topList" +
                    $"?roomid={roomid}" +
                    $"&page=1" +
                    $"&ruid={uid}" +
                    $"&page_size=29";

                var response = await ClientProxy.GetAsync(apiurl);

                if (response.IsSuccessStatusCode)
                {
                    var rawjson = await response.Content.ReadAsStringAsync();

                    var info = JsonConvert.DeserializeObject<MemberListInfo>(rawjson);

                    var totalPageCount = info.Data.Info.TotalPage;

                    List<MemberInfo> result = new(info.Data.CurrentPage);

                    for (int i = 2; i <= totalPageCount; i++)
                    {
                        result.AddRange(await GetMemberOfPageAsync(uid, roomid, i));
                    }

                    return result;
                }
            }
            return null;
        }

        private async Task<List<MemberInfo>> GetMemberOfPageAsync(string uid, string roomid, int page)
        {
            var apiurl = "https://api.live.bilibili.com/xlive/app-room/v2/guardTab/topList" +
                    $"?roomid={roomid}" +
                    $"&page={page}" +
                    $"&ruid={uid}" +
                    $"&page_size=29";

            var response = await ClientProxy.GetAsync(apiurl);

            if (response.IsSuccessStatusCode)
            {
                var rawjson = await response.Content.ReadAsStringAsync();

                var info = JsonConvert.DeserializeObject<MemberListInfo>(rawjson);

                return info.Data.CurrentPage;
            }

            return null;
        }
    }
}
