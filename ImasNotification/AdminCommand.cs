using System.Linq;
using System.Management.Automation;
using Mastonet;

namespace ImasNotification
{
    static class AdminCommand
    {
        public static void AdminCommander(PostManager pm, string from, long id, Visibility v, string[] tokens)
        {
            switch (tokens[1])
            {
                case "fetch":
                    var content = $"@{from} ";
                    if (tokens.Length < 3)
                    {
                        content += $"-bオプションか-nオプションを指定してください。\n";
                    }
                    else if (tokens[2] == "-b")
                    {
                        content += FetchNewBlogItem() ? "成功" : "失敗";
                    }
                    else if (tokens[2] == "-n")
                    {
                        content += FetchNewNewsItem() ? "成功" : "失敗";
                    }

                    pm.Col.Add(new PostContent(id, content, false, null, v: v));
                    break;
            }
        }
        private static bool FetchNewBlogItem()
        {
            var source = $"ruby ../ImasInfo/ImasInfo.rb\n$LASTEXITCODE\n";
            PSObject result = null;
            using (var invoker = new RunspaceInvoke())
            {
                result = invoker.Invoke(source).Last();
            }
            var exitcode = result.BaseObject as int?;
            return exitcode == 0 ? true : false;
        }
        private static bool FetchNewNewsItem()
        {
            var source = $"ruby ../ImasNews/ImasNews.rb\n$LASTEXITCODE\n";
            PSObject result = null;
            using (var invoker = new RunspaceInvoke())
            {
                result = invoker.Invoke(source).Last();
            }
            var exitcode = result.BaseObject as int?;
            return exitcode == 0 ? true : false;
        }
    }
}
