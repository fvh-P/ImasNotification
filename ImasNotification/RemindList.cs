using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ImasNotification
{
    class RemindList : ObservableCollection<Reminder>
    {
        public RemindList() { }
        public void Remind(PostManager postManager, List<DailyJob> dailyJobList, string from, int id, string[] token)
        {
            if (token.Length == 3)
            {
                switch (token[1])
                {
                    case "add":
                        AddRemind(postManager, dailyJobList, from, id, token[2]);
                        return;
                    case "remove":
                        RemoveRemind(postManager, from, id, token[2]);
                        return;
                    case "list":
                        if (token[2].Length == 4)
                        {
                            ShowJobList(postManager, dailyJobList, from, id, token[2], "all");
                            return;
                        }
                        else if(token[2] == "-m" || token[2] == "--my")
                        {
                            ShowJobList(postManager, dailyJobList, from, id, null, "my");
                            return;
                        }
                        break;
                }

            }
            else if (token.Length == 2)
            {
                switch (token[1])
                {
                    case "list":
                        ShowJobList(postManager, dailyJobList, from, id, DateTime.Today.ToString("MMdd"), "all");
                        return;
                    case "help":
                        ShowHelp(postManager, from, id);
                        return;
                }
            }
            var content = $"@{from} コマンドが正しくありません。使い方は\"remind help\"を参照してください。\n";
            postManager.Col.Add(new PostContent(id, content, false, null));
        }

        private void AddRemind(PostManager postManager, List<DailyJob> dailyJobList, string from, int id, string code)
        {
            string content;
            foreach (var job in dailyJobList.SelectMany(x => x.Jobs).ToList())
            {
                if (job.Code == code && job.HasTime == true)
                {
                    if (job.Time - new TimeSpan(0, 10, 0) < DateTime.Now && DateTime.Now < job.Time)
                    {
                        content = $"@{from} さん、そのお仕事はもうすぐです。\n{job.Time.ToShortTimeString()}\n{job.Item}\n{job.Url}\n";
                        postManager.Col.Add(new PostContent(id, content, false, null));
                        return;
                    }
                    else if(job.Time < DateTime.Now)
                    {
                        content = $"@{from} さん、そのお仕事は時間を過ぎています。\n{job.Time.ToShortTimeString()}\n{job.Item}\n{job.Url}\n";
                        postManager.Col.Add(new PostContent(id, content, false, null));
                        return;
                    }

                    content = $"@{from} さん、もうすぐお仕事です。\n{job.Time.ToShortTimeString()}\n{job.Item}\n{job.Url}\n";
                    var tmp = new Reminder(job.Time - new TimeSpan(0, 10, 0), from, code, new PostContent(id, content, false, null));

                    if (this.Count(x => x.Post.Content == content) != 0)
                    {
                        content = $"@{from} お仕事コード{code}はすでに登録されています。\n";
                        postManager.Col.Add(new PostContent(id, content, false, null));
                    }
                    else
                    {
                        Add(tmp);
                        content = $"@{from} \n{job.Time.ToShortDateString()} {job.Time.ToShortTimeString()}\n{job.Item}\n登録しました。約10分前にお知らせします。\n";
                        postManager.Col.Add(new PostContent(id, content, false, null));
                    }
                    return;
                }
            }
            content = $"@{from} お仕事コード{code}に該当するお仕事はありません。\n";
            postManager.Col.Add(new PostContent(id, content, false, null));
        }
        private void RemoveRemind(PostManager postManager, string from, int id, string code)
        {
            string content;
            if (this.Count(x => x.From == from && x.Code == code) == 0)
            {
                content = $"@{from} お仕事コード{code}は登録されていません。\n";
                postManager.Col.Add(new PostContent(id, content, false, null));
            }
            else
            {
                Remove(this.Where(x => x.From != from || x.Code != code).First());
                content = $"@{from} お仕事コード{code}の通知登録を解除しました。\n";
                postManager.Col.Add(new PostContent(id, content, false, null));
            }
        }
        private void ShowJobList(PostManager postManager, List<DailyJob> dailyJobList, string from, int id, string date, string option)
        {
            var content = $"@{from} ";
            DateTime d;
            try
            {
                d = DateTime.ParseExact(date, "MMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None);
            }
            catch
            {
                content += "日付指定の書式が間違っています。\n例:4月1日は0401";
                postManager.Col.Add(new PostContent(id, content, false, null));
                return;
            }
            DailyJob jobList = dailyJobList.Find(x => x.ShortDate == date);
            if(jobList.Jobs.Count == 0)
            {
                content += $"{DateTime.Now.ToShortTimeString()}現在、{d.ToShortDateString()}のお仕事情報はありません。\n";
            }
            else if (option == null)
            {
                var tmp = jobList.Jobs.Where(x => x.HasTime == true).ToList();
                content += $"{d.ToShortDateString()}のお仕事(登録可能のみ)\n" + string.Join("\n", tmp.Select(x => $"[{ x.Team }] { x.ReallyTime }\n{ x.Item }\n{ x.Url }\nお仕事コード:{ x.Code }"));
            }
            else if(option == "all")
            {
                content += $"{d.ToShortDateString()}のお仕事\n" + string.Join("\n", jobList.Jobs.Select(x => $"[{ x.Team }] { x.ReallyTime }\n{ x.Item }\n{ x.Url }" + (x.HasTime == true ? $"\nお仕事コード:{ x.Code }" : "")));
            }
            else if(option == "my")
            {
                content += "現在登録中のお仕事\n" + string.Join("\n", this.Where(x => x.From == from).Select(x => $"{x.Code}"));
            }
            postManager.Col.Add(new PostContent(id, content, false, null));
        }
        private void ShowHelp(PostManager postManager, string from, int id)
        {
            var content = $"@{from} お仕事コードを登録するとそのお仕事の開始約10分前にリプライでお知らせします。お仕事コードは本日のお仕事のうち開始時間の設定があるものに付与されています。\n\n" +
                $"オプションリスト\n" +
                $"remind add <jobcode>\n" +
                $"  お仕事コード<jobcode>のお仕事を通知登録します。\n" +
                $"remind remove <jobcode>\n" +
                $"  お仕事コード<jobcode>のお仕事を通知解除します。\n" +
                $"remind list <date>\n" +
                $"  <date>に日付を指定するとその日のお仕事を返します。\n" +
                $"  日付の書式は4月1日の場合0401です。" +
                $"  <date>に何も指定しないと本日のお仕事を返します。\n";
            postManager.Col.Add(new PostContent(id, content, true, "remindコマンドのヘルプ\n"));
        }
    }
}
