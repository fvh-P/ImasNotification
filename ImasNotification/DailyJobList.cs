using Mastonet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImasNotification
{
    class DailyJobList : List<DailyJob>
    {
        public DailyJobList() : base() { }
        public DailyJobList(int capacity): base(capacity) { }

        public void ShowJobList(PostManager pm, string from, long id, Visibility v, string[] token, RemindList rl = null)
        {
            var content = $"@{from} ";
            DateTime d;
            if(token.Length == 1)
            {
                d = DateTime.Today;
                DailyJob jobList = Find(x => x.ShortDate == d.ToString("yyMMdd"));
                if (jobList.Jobs.Count == 0)
                {
                    content += $"{DateTime.Now.ToShortTimeString()}現在、本日のお仕事情報はありません。\n";
                }
                else
                {
                    content += $"本日のお仕事\n" + string.Join("\n", jobList.Jobs.Select(x => $"[{ x.Team }] { x.ReallyTime }\n{ x.Item }\n{ x.Url }" + (x.HasTime == true ? $"\nお仕事コード:{ x.Code }" : "")));
                }
            }
            else if(token.Length >= 2 && DateTime.TryParseExact(token[1], "yyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None, out d))
            {
                if (Find(x => x.ShortDate == token[1]).Jobs.Count == 0)
                {
                    content += $"{DateTime.Now.ToShortTimeString()}現在、{d.ToString("yyyy/MM/dd")}のお仕事情報はありません。\n";
                }
                else
                {
                    content += $"{d.ToString("yyyy/MM/dd")}のお仕事\n" + string.Join("\n", Find(x => x.ShortDate == token[1]).Jobs.Select(x => $"[{ x.Team }] { x.ReallyTime }\n{ x.Item }\n{ x.Url }" + (x.HasTime == true ? $"\nお仕事コード:{ x.Code }" : "")));
                }
            }
            else if(token.Length >= 2 && token[1] == "help")
            {
                content += $"listコマンド 指定した日付のお仕事一覧を返します。\n" +
                    $"使い方  (at)はアットマーク\n\n" +
                    $"(at)info list\n" +
                    $"  日付を指定しないと今日のお仕事一覧を返します。\n\n" +
                    $"(at)info list MMdd\n" +
                    $"  yyMMddの形式で指定された日付のお仕事一覧を返します。\n" +
                    $"  2018年4月1日なら180401です。\n" +
                    $"  指定できる日付は翌月末までです。";
                pm.Col.Add(new PostContent(id, content, true, "listコマンドのヘルプ\n", v: v));
                return;
            }
            else
            {
                content += $"コマンドが正しくないようです。日付が正しくないなどの理由が考えられます。\n" +
                    $"指定できる日付はyyMMddの形式で、翌月末までです。\n" +
                    $" 2018年4月1日なら180401です。\n";
            }
            pm.Col.Add(new PostContent(id, content, false, null, v: v));
        }

        public void ShowRegisteredJobList(PostManager pm, RemindList rl, string from, long id, Visibility v)
        {
            var content = $"@{from} ";
            var str = string.Join("\n", rl.Where(x => x.From == from).Select(x => $"{x.Code}"));
            content += $"現在登録中のお仕事{((str == "") ? "はありません。" : $"\n{string.Join("\n", rl.Where(x => x.From == from).Select(x => $"{x.Code}"))}")}";
            pm.Col.Add(new PostContent(id, content, false, null, v: v));
        }
    }
}
