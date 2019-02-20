using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mastonet;

namespace ImasNotification
{
    class RemindList : ObservableCollection<Reminder>
    {
        public RemindList() { }
        public RemindList(IEnumerable<Reminder> collection) : base(collection) { } 
        public void Remind(PostManager postManager, List<DailyJob> dailyJobList, string from, long id, Visibility v, string[] token)
        {
            PostContent pc = null;
            switch (token[1])
            {
                case "add":
                    pc = AddRemind(dailyJobList, from, id, v, token[2]);
                    Console.WriteLine(Count);
                    break;
                case "remove":
                    pc = RemoveRemind(from, id, v, token[2]);
                    Console.WriteLine(Count);
                    break;
                case "help":
                    pc = ShowHelp(from, id, v);
                    break;
                default:
                    var content = $"@{from} コマンドが正しくありません。使い方は\"(at)info help\"または\"(at)info remind help\"を参照してください。\n";
                    pc = new PostContent(id, content, false, null, v: v);
                    break;
            }
            if (pc != null)
            {
                postManager.Col.Add(pc);
            }
        }

        private PostContent AddRemind(List<DailyJob> dailyJobList, string from, long id, Visibility v, string code)
        {
            string content = $"@{from} ";
            foreach (var job in dailyJobList.SelectMany(x => x.Jobs))
            {
                if (job.Code == code && job.HasTime == true)
                {
                    if (job.Time - new TimeSpan(0, 10, 0) < DateTime.Now && DateTime.Now < job.Time)
                    {
                        content += $"さん、そのお仕事はもうすぐです。\n{job.Time.ToString("HH:mm")}\n{job.Item}\n{job.Url}\n";
                        return new PostContent(id, content, false, null, v: v);
                    }
                    else if(job.Time < DateTime.Now)
                    {
                        content += $"さん、そのお仕事は時間を過ぎています。\n{job.Time.ToString("HH:mm")}\n{job.Item}\n{job.Url}\n";
                        return new PostContent(id, content, false, null, v: v);
                    }

                    content += $"さん、もうすぐお仕事です。\n{job.Time.ToString("HH:mm")}\n{job.Item}\n{job.Url}\n";
                    var tmp = new Reminder(job.Time - new TimeSpan(0, 10, 0), from, code, new PostContent(id, content, false, null, v: v));

                    if (this.Count(x => x.Post.Content == content) != 0)
                    {
                        content = $"@{from} お仕事コード{code}はすでに登録されています。\n";
                    }
                    else
                    {
                        Add(tmp);
                        content = $"@{from} \n{job.Time.ToString("yy/MM/dd HH:mm")}\n{job.Item}\n登録しました。約10分前にお知らせします。\n";
                    }
                    return new PostContent(id, content, false, null, v: v);
                }
            }
            content += $"お仕事コード{code}に該当するお仕事はありません。\n";
            return new PostContent(id, content, false, null, v: v);
        }
        private PostContent RemoveRemind(string from, long id, Visibility v, string code)
        {
            string content = $"@{from} ";
            if (this.Count(x => x.From == from && x.Code == code) == 0)
            {
                content += "お仕事コード{code}は登録されていません。\n";
            }
            else
            {
                Remove(this.Where(x => x.From == from && x.Code == code).First());
                content += $"お仕事コード{code}の通知登録を解除しました。\n";
            }
            return new PostContent(id, content, false, null, v: v);
        }
        private PostContent ShowHelp(string from, long id, Visibility v)
        {
            var content = $"@{from} お仕事コードを登録するとそのお仕事の開始約10分前にリプライでお知らせします。お仕事コードはお仕事のうち開始時間の設定があるものに付与されています。\n\n" +
                $"使い方  (at)はアットマーク\n\n" +
                $"(at)info remind add xxxxxx\n" +
                $"  お仕事コードxxxxxxのお仕事を通知登録します。\n\n" +
                $"(at)info remind remove xxxxxx\n" +
                $"  お仕事コードxxxxxxのお仕事を通知解除します。\n\n" +
                $"お仕事コードはlistコマンドで調べられます。詳しくは\n" +
                $"(at)info list help\n" +
                $"と投稿してください。";
            return new PostContent(id, content, true, "remindコマンドのヘルプ\n", v: v);
        }
    }
}
