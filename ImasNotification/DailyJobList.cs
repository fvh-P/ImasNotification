using System;
using System.Collections.Generic;
using System.Linq;

namespace ImasNotification
{
    class DailyJobList : List<DailyJob>
    {
        public DailyJobList() : base() { }
        public DailyJobList(int capacity): base(capacity) { }

        public void ShowJobList(PostManager pm, string from, int id, string[] token, RemindList rl = null)
        {
            var content = $"@{from} ";
            DateTime d;
            if(token.Length == 1)
            {
                d = DateTime.Today;
                DailyJob jobList = Find(x => x.ShortDate == d.ToString("MMdd"));
                content += $"{d.ToShortDateString()}のお仕事\n" + string.Join("\n", jobList.Jobs.Select(x => $"[{ x.Team }] { x.ReallyTime }\n{ x.Item }\n{ x.Url }" + (x.HasTime == true ? $"\nお仕事コード:{ x.Code }" : "")));
            }
            else if(token.Length >= 2 && DateTime.TryParseExact(token[1], "MMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None, out var _))
            {
                d = DateTime.ParseExact(token[1], "MMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None);
                DailyJob jobList = Find(x => x.ShortDate == token[1]);
                if (jobList.Jobs.Count == 0)
                {
                    content += $"{DateTime.Now.ToShortTimeString()}現在、{d.ToShortDateString()}のお仕事情報はありません。\n";
                }
                else
                {
                    content += $"{d.ToShortDateString()}のお仕事\n" + string.Join("\n", jobList.Jobs.Select(x => $"[{ x.Team }] { x.ReallyTime }\n{ x.Item }\n{ x.Url }" + (x.HasTime == true ? $"\nお仕事コード:{ x.Code }" : "")));
                }
            }
            else if(token.Length >= 2 && token[1] == "help")
            {
                content += $"listコマンド 指定した日付のお仕事一覧を返します。\n" +
                    $"使い方  (at)はアットマーク\n\n" +
                    $"(at)info list\n" +
                    $"  日付を指定しないと今日のお仕事一覧を返します。\n\n" +
                    $"(at)info list MMdd\n" +
                    $"  MMddの形式で指定された日付のお仕事一覧を返します。\n" +
                    $"  4月1日は0401です。\n" +
                    $"  指定できる日付は翌月末までです。";
                pm.Col.Add(new PostContent(id, content, true, "listコマンドのヘルプ\n"));
                return;
            }
            pm.Col.Add(new PostContent(id, content, false, null));
        }

        public void ShowRegisteredJobList(PostManager pm, RemindList rl, string from, int id)
        {
            var content = $"@{from} ";
            var str = string.Join("\n", rl.Where(x => x.From == from).Select(x => $"{x.Code}"));
            content += "現在登録中のお仕事" + ((str == "") ? "はありません。" : $"\n{string.Join("\n", rl.Where(x => x.From == from).Select(x => $"{x.Code}"))}");
            pm.Col.Add(new PostContent(id, content, false, null));
        }
    }
}
