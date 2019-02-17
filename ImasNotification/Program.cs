using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ImasNotification
{
    class Program
    {
        static DailyJobList dailyJobList;
        static RemindList remindList = new RemindList();
        static FeedList feedList = new FeedList();
        static string[] configText = File.ReadAllLines("ImasNotification.config").Select(x => x.TrimEnd(new char[] {'\n', '\r'})).ToArray();
        static DateTime today = DateTime.Today;
        static int triggerCount = 0;

        static readonly string CId = configText[1];
        static readonly string CSec = configText[2];
        static readonly string Token = configText[3];
        static PostManager postManager = new PostManager("imastodon.net", CId, CSec, Token);

        static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (dailyJobList == null)
            {
                today = DateTime.Today;
                dailyJobList = new DailyJobList();
                var files = new DirectoryInfo(configText[0]).GetFiles();
                foreach (var f in files)
                {
                    var tmp = File.ReadAllText(f.FullName);
                    dailyJobList.Add(DailyJob.Deserialize(tmp));
                }
            }
            if (File.Exists("remindList.json"))
            {
                remindList = new RemindList(Reminder.Deserialize(File.ReadAllText("remindList.json")));
            }
            if (File.Exists("feedList.json"))
            {
                feedList.AddRange(File.ReadLines("feedList.json").Select(x => long.Parse(x)).ToList());
            }
            var timer = new Timer(new TimerCallback(ThreadingTimerCallback));
            timer.Change(0, 1000 * 10);

            postManager.Col.CollectionChanged += (sender, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    var c = e.NewItems.Count;
                    var item = postManager.Col.Select(x => x).ToArray();
                    foreach (var x in item)
                    {
                        postManager.Col.Remove(x);
                        Console.WriteLine(x.Content);
                        postManager.Client.PostStatus(x.Content, x.Visibility, replyStatusId: x.Id, sensitive: x.Sensitive, spoilerText: x.Spoiler);
                    }
                    Debug.WriteLine(item.First().Content);
                }
            };

            remindList.CollectionChanged += (sender, e) =>
            {
                var jsonString = string.Join(",\n", remindList.Select(x => x.Serialize()));
                File.WriteAllText("remindList.json", $"[{jsonString}]");
            };

            var task = RunAsync();
            task.Wait();
        }

        private static void ThreadingTimerCallback(object state)
        {
            postManager.Client.UpdateCredentials();
            if ((++triggerCount) == 360)
            {
                triggerCount = 0;
                today = DateTime.Today;
                var newJobList = new DailyJobList();
                //var files = new DirectoryInfo(configText[0]).GetFiles();
                foreach (var f in Directory.EnumerateFiles(configText[0]))
                {
                    newJobList.Add(DailyJob.Deserialize(File.ReadAllText(f)));
                }
                dailyJobList = newJobList;
            }
            var now = DateTime.Now;
            var removeList = new List<Reminder>();
            foreach (var remind in remindList)
            {
                if (remind.PostTime < now)
                {
                    postManager.Col.Add(remind.Post);
                    remind.Posted = true;
                    removeList.Add(remind);
                }
            }
            foreach (var rm in removeList)
            {
                remindList.Remove(rm);
                Console.WriteLine(remindList.Count);
            }
        }

        static async Task RunAsync()
        {
            var stream = new Manager("imastodon.net", CId, CSec, Token).Client.GetUserStreaming();
            stream.OnNotification += (sender, e) =>
            {
                if (e.Notification.Type == "mention" && e.Notification.Status.Mentions.Count() == 1)
                {
                    var v = e.Notification.Status.Visibility;
                    var my = e.Notification.Status.Mentions.First().AccountName;
                    var from = e.Notification.Account.AccountName;
                    var id = e.Notification.Status.Id;
                    var account = e.Notification.Status.Account.Id;
                    Debug.WriteLine($"{from}: {e.Notification.Status.Content}");
                    var toot = HtmlParse(e.Notification.Status.Content).Replace($"@{my}", "").TrimStart();

                    if (from != my & toot != "")
                    {
                        var token = Regex.Split(toot, @"\s");
                        switch (token[0])
                        {
                            case "remind":
                                remindList.Remind(postManager, dailyJobList, from, id, v, token);
                                break;
                            case "list":
                                if (token.Length == 2 && token[1] == "my")
                                {
                                    dailyJobList.ShowRegisteredJobList(postManager, remindList, from, id, v);
                                }
                                else
                                {
                                    dailyJobList.ShowJobList(postManager, from, id, v, token);
                                }
                                break;
                            case "feed":
                                var accountId = e.Notification.Account.Id;
                                if (token.Length == 2 && token[1] == "add")
                                {
                                    feedList.Subscribe(postManager, from, id, accountId, v);
                                }
                                else if (token.Length == 2 && token[1] == "remove")
                                {
                                    feedList.UnSubscribe(postManager, from, id, accountId, v);
                                }
                                else
                                {
                                    feedList.ShowHelp(postManager, from, id, v);
                                }
                                break;
                            case "help":
                                var content = $"@{from} 現在、infoでは以下のコマンドを実行できます。\n" +
                                $"各コマンドの詳しい使用法は、\"(at)info コマンド名 help\"と投稿してください。\n\n" +
                                $"feed : アイマスニュースとアイマス公式ブログの更新情報をDMでお知らせします。\n\n" +
                                $"remind : お仕事のリマインダーです。登録すると10分前にリプライでお知らせします。\n\n" +
                                $"list : 指定した日付のお仕事一覧を表示します。\n\n" +
                                $"help : ここ。";
                                postManager.Col.Add(new PostContent(id, content, true, "infoのコマンドヘルプ\n"));
                                break;
                            case "admin":
                                
                                if (account == 35)
                                {
                                    AdminCommand.AdminCommander(postManager, from, id, v, token);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            };
            while (true)
            {
                try
                {
                    await stream.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(DateTime.Now);
                    Console.WriteLine(remindList.Count);
                    Debug.WriteLine(e.Message);
                }
            }
        }

        static string HtmlParse(string str)
        {
            var html = new HtmlDocument()
            {
                OptionWriteEmptyNodes = true
            };
            html.LoadHtml(str);
            var toot = string.Join("\n", html.DocumentNode.SelectNodes("p").Select(x => x.InnerHtml).ToArray());
            html.LoadHtml(toot.Replace("<br />", "\n"));
            toot = html.DocumentNode.InnerText;
            return WebUtility.HtmlDecode(toot);
        }
    }
}
