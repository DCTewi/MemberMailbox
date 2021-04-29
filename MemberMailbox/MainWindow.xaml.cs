using MemberMailbox.Data;
using MemberMailbox.Services;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MemberMailbox
{
    public partial class MainWindow : Window
    {
        private readonly Regex UidRegex = new("^[0-9]{1,}$");

        private bool Checked = false;

        public MainWindow()
        {
            InitializeComponent();
            CheckInputCache();
        }

        private void SetChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Checked = false;
            SetProgress(0, "");
        }

        private void SetProgress(double value, string message)
        {
            ProgressMessage.Value = value;
            ProgressText.Text = message;
        }

        private void CheckInputCache()
        {

            if (File.Exists("cache.json"))
            {
                var rawjson = File.ReadAllText("cache.json");
                var info = JsonConvert.DeserializeObject<InputCacheInfo>(rawjson);

                if (info == null)
                {
                    File.Delete("cache.json");
                }
                else
                {
                    TextUid.Text = info.Uid;
                    TextCookie.Text = info.Cookie;
                }
            }
        }

        private void SaveInputCache()
        {
            try
            {
                var info = new InputCacheInfo { Uid = TextUid.Text, Cookie = TextCookie.Text };
                var rawjson = JsonConvert.SerializeObject(info);
                File.WriteAllText("cache.json", rawjson);
            }
            finally { }
        }

        private async void OnSendClickAsync(object sender, RoutedEventArgs e)
        {
            if (!Checked)
            {
                MessageBox.Show("请先检查连通性!");
                return;
            }
            if (string.IsNullOrWhiteSpace(TextUid.Text))
            {
                MessageBox.Show("请输入要发送的消息内容");
                return;
            }

            MailboxProxy proxy = new(TextCookie.Text);

            try
            {
                var info = await proxy.GetUserInfoAsync(TextUid.Text);

                var confirmResult = MessageBox.Show(
                    $"将要把消息发送给 [{info.Data.Name}] (直播间号为:{info.Data.Liveroom.RoomId}) 的舰长, 确认吗?",
                    "确认",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    await SendMessageAsync(TextUid.Text, TextMessage.Text, proxy);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Uid无效!\n异常消息:{exception.Message}");
            }
        }

        internal async Task SendMessageAsync(string uid, string msg, MailboxProxy proxy)
        {
            try
            {
                SetProgress(0, "正在处理消息");
                msg = msg.Replace(Environment.NewLine, "\\n");

                SetProgress(1, "正在获取舰长名单");
                var memberList = await proxy.GetAllMembersAsync(TextUid.Text);

                for (int i = 0; i < memberList.Count; i++)
                {
                    SetProgress(1 + 99 * i / (double)memberList.Count, $"({i}/{memberList.Count}) 正在发送给[{memberList[i].Username}]");

                    // DEBUG
                    //await proxy.SendMessageAsync(uid, "1427846", $"正在向 [{memberList[i].Uid}]({memberList[i].Username}) 模拟发送:\\n{msg}");

                    await proxy.SendMessageAsync(uid, memberList[i].Uid, msg);

                    await Task.Delay(400);
                }

                SetProgress(100, "任务已完成");
            }
            catch (Exception exception)
            {
                SetProgress(0, $"出现错误:{exception.Message}");
            }
        }

        private async void OnTestClickAsync(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextUid.Text) || !UidRegex.IsMatch(TextUid.Text))
            {
                MessageBox.Show("请输入正确的Uid");
                return;
            }
            if (string.IsNullOrWhiteSpace(TextCookie.Text))
            {
                MessageBox.Show("请输入对应账号的Cookie");
                return;
            }
            ProgressText.Text = "正在测试连通性";

            MailboxProxy proxy = new(TextCookie.Text);

            try
            {
                var result = await proxy.SendMessageAsync(TextUid.Text, "1427846", "[测试消息]该账号正在使用MemberMailbox测试连通性");

                if (result.Code == 0)
                {
                    SaveInputCache();
                    ProgressText.Text = "测试通过";
                    Checked = true;
                }
                else
                {
                    ProgressText.Text = $"测试未通过, 请检查Uid和Cookie: [{result.Message}]";
                    Checked = false;
                }
            }
            catch (Exception exception)
            {
                ProgressText.Text = $"测试未通过, 请检查Uid和Cookie: [{exception.Message}]";
                Checked = false;
            }
        }
    }
}
