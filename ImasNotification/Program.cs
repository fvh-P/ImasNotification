using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mastonet;
using HtmlAgilityPack;

namespace ImasNotification
{
    class Program
    {
        static List<DailyJob> dailyJobList;
        static RemindList remindList = new RemindList();
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
                dailyJobList = new List<DailyJob>();
                var files = new DirectoryInfo(configText[0]).GetFiles();
                foreach (var f in files)
                {
                    var tmp = File.ReadAllText(f.FullName);
                    dailyJobList.Add(DailyJob.Deserialize(tmp));
                }
            }
            if (File.Exists("remindList.json"))
            {
                var tmp = Reminder.Deserialize(File.ReadAllText("remindList.json"));
                foreach(var r in tmp)
                {
                    remindList.Add(r);
                }
            }

            Debug.WriteLine(dailyJobList);
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
                        postManager.Client.PostStatus(x.Content, x.Visibility, replyStatusId: x.Id, sensitive: x.Sensitive, spoilerText: x.Spoiler);
                    }
                    Debug.WriteLine(item[0].Content);
                }
            };

            remindList.CollectionChanged += (sender, e) =>
            {
                var jsonString = string.Join(",\n", remindList.Select(x => x.Serialize()).ToArray());
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
                var newJobList = new List<DailyJob>();
                var files = new DirectoryInfo(configText[0]).GetFiles();
                foreach (var f in files)
                {
                    var tmp = File.ReadAllText(f.FullName);
                    newJobList.Add(DailyJob.Deserialize(tmp));
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
            }
            Console.WriteLine(remindList.Count);
        }

        static async Task RunAsync()
        {
            var streamingClient = new Manager("imastodon.net", CId, CSec, Token);
            var stream = streamingClient.Client.GetUserStreaming();
            stream.OnNotification += (sender, e) =>
            {
                if (e.Notification.Type == "mention" && e.Notification.Status.Mentions.Count() == 1)
                {
                    var my = e.Notification.Status.Mentions.First().AccountName;
                    var from = e.Notification.Account.AccountName;
                    var id = e.Notification.Status.Id;
                    Debug.WriteLine($"{from}: {e.Notification.Status.Content}");
                    var toot = HtmlParse(e.Notification.Status.Content).Replace($"@{my}", "").TrimStart();

                    if (from != my & toot != "")
                    {
                        var token = Regex.Split(toot, @"\s");
                        switch (token[0])
                        {
                            case "remind":
                                remindList.Remind(postManager, dailyJobList, from, id, token);
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
