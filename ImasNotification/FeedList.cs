using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mastonet;

namespace ImasNotification
{
    class FeedList : List<long>
    {
        public FeedList() : base() { }
        public FeedList(int capacity) : base(capacity) { }

        public void Subscribe(PostManager pm, string from, long id, long accountId, Visibility v)
        {
            var content = $"@{from} ";
            if (Contains(accountId))
            {
                content += $"すでに購読済みです。\n";
            }
            else if (Count > 50)
            {
                content += $"購読できません。\n" +
                    $"現在、購読人数が上限に達しています。\n" +
                    $"詳しくは(at)fまでお問い合わせください。\n\n" +
                    $"※(at)はアットマーク";
            }
            else
            {
                Add(accountId);
                if (File.Exists("feedList.json"))
                {
                    File.Delete("feedList.json");
                }
                File.WriteAllLines("feedList.json", this.Select(x => x.ToString()));
                content += $"購読登録しました。\n" +
                    $"アイマスニュースおよびアイマス公式ブログ更新情報をDMで配信します。\n" +
                    $"解除する場合はinfo宛にfeed removeとリプライしてください。";
            }
            pm.Col.Add(new PostContent(id, content, false, null, v: v));
        }

        public void UnSubscribe(PostManager pm, string from, long id, long accountId, Visibility v)
        {
            var content = $"@{from} ";
            if (Contains(accountId))
            {
                Remove(accountId);
                if (File.Exists("feedList.json"))
                {
                    File.Delete("feedList.json");
                }
                File.WriteAllLines("feedList.json", this.Select(x => x.ToString()));
                content += $"購読解除しました。\n";
            }
            else
            {
                content += $"購読していません。\n";
            }
            pm.Col.Add(new PostContent(id, content, false, null, v: v));
        }

        public void ShowHelp(PostManager pm, string from, long id, Visibility v)
        {
            var content = $"feedコマンド アイマスニュースとアイマス公式ブログの更新情報をDMでお知らせします。\n" +
                    $"使い方  (at)はアットマーク\n\n" +
                    $"(at)info feed add\n" +
                    $"  購読します。以降更新情報があるとDMで送られてきます。\n\n" +
                    $"(at)info feed remove\n" +
                    $"  購読を解除します\n";
            pm.Col.Add(new PostContent(id, content, true, "feedコマンドのヘルプ\n", v: v));
            return;
        }
    }
}
