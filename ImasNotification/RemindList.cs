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
                }

            }
            else if (token.Length == 2 && token[1] == "help")
            {
                ShowHelp(postManager, from, id);
                return;
            }
            var content = $"@{from} コマンドが正しくありません。使い方は\"(at)info help\"または\"(at)info remind help\"を参照してください。\n";
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
        private void ShowHelp(PostManager postManager, string from, int id)
        {
            var content = $"@{from} お仕事コードを登録するとそのお仕事の開始約10分前にリプライでお知らせします。お仕事コードは本日のお仕事のうち開始時間の設定があるものに付与されています。\n\n" +
                $"使い方  (at)はアットマーク\n\n" +
                $"(at)info remind add xxxxxx\n" +
                $"  お仕事コードxxxxxxのお仕事を通知登録します。\n\n" +
                $"(at)info remind remove <jobcode>\n" +
                $"  お仕事コードxxxxxxのお仕事を通知解除します。\n\n" +
                $"お仕事コードはlistコマンドで調べられます。詳しくは\n" +
                $"(at)info list help\n" +
                $"と投稿してください。";
            postManager.Col.Add(new PostContent(id, content, true, "remindコマンドのヘルプ\n"));
        }
    }
}
