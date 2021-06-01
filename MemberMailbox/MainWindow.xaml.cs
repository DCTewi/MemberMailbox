using MemberMailbox.Data;
using MemberMailbox.Services;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

namespace MemberMailbox
{
    public partial class MainWindow : Window
    {
        private readonly Regex UidRegex = new("^[0-9]{1,}$");

        private bool Checked = false;

        public MainWindow()
        {
            InitializeComponent();

            TextUid.Text = "698438232";

            ButtonGet.Click += async (sender, e) =>
            {
                try
                {
                    ListMember.Items.Clear();
                    ListMember.Items.Add("加载中...");

                    MailboxProxy proxy = new(string.Empty);

                    var info = await proxy.GetUserInfoAsync(TextUid.Text);

                    var memberList = await proxy.GetAllMembersAsync(TextUid.Text);

                    var selectedList = memberList.Select(m => $"{m.Username}");

                    ListMember.Items.Clear();
                    foreach (var i in selectedList)
                    {
                        ListMember.Items.Add(i);
                    }

                    MessageBox.Show($"获取成功, 共{selectedList.Count()}个");
                }
                catch
                {
                    MessageBox.Show("Uid有误!");
                }
            };

            ButtonExport.Click += async (sender, e) =>
            {
                try
                {
                    ListMember.Items.Clear();
                    ListMember.Items.Add("加载中...");

                    MailboxProxy proxy = new(string.Empty);

                    var info = await proxy.GetUserInfoAsync(TextUid.Text);

                    var memberList = await proxy.GetAllMembersAsync(TextUid.Text);

                    var selectedList = memberList.Select(m => $"{m.Username}");

                    var dialog = new SaveFileDialog
                    {
                        DefaultExt = "txt",
                        FileName = $"{TextUid.Text} 舰长名单"
                    };

                    if (dialog.ShowDialog() ?? false)
                    {
                        var path = dialog.FileName;
                        File.AppendAllText(path, string.Join(Environment.NewLine, selectedList));

                    }

                    ListMember.Items.Clear();
                    foreach (var i in selectedList)
                    {
                        ListMember.Items.Add(i);
                    }

                    MessageBox.Show($"获取成功, 共{selectedList.Count()}个");
                }
                catch
                {
                    MessageBox.Show("Uid有误!");
                }
            };

            ButtonExportAll.Click += async (sender, e) =>
            {
                try
                {
                    ListMember.Items.Clear();
                    ListMember.Items.Add("加载中...");

                    MailboxProxy proxy = new(string.Empty);

                    var info = await proxy.GetUserInfoAsync(TextUid.Text);

                    var memberList = await proxy.GetAllMembersAsync(TextUid.Text);

                    var selectedList = memberList.Select(m => $"{m.Username},{m.Uid},{m.MemberLevel}");

                    var dialog = new SaveFileDialog
                    {
                        DefaultExt = "txt",
                        FileName = $"{TextUid.Text} 舰长名单"
                    };

                    if (dialog.ShowDialog() ?? false)
                    {
                        var path = dialog.FileName;

                        File.WriteAllText(path, $"用户名,Uid,舰长等级{Environment.NewLine}");
                        File.AppendAllText(path, string.Join(Environment.NewLine, selectedList));

                    }

                    ListMember.Items.Clear();
                    foreach (var i in selectedList)
                    {
                        ListMember.Items.Add(i);
                    }

                    MessageBox.Show($"获取成功, 共{selectedList.Count()}个");
                }
                catch
                {
                    MessageBox.Show("Uid有误!");
                }
            };
        }
    }
}
