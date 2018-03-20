using Mastonet;
using System.Runtime.Serialization;

namespace ImasNotification
{
    [DataContract]
    class PostContent
    {
        [DataMember]
        public long? Id { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public bool Sensitive { get; set; }

        [DataMember]
        public string Spoiler { get; set; }

        [DataMember]
        public Visibility Visibility { get; set; }

        public PostContent(long? id, string content, bool sensitive, string spoiler, Visibility v = Visibility.Unlisted)
        {
            Id = id;
            Content = content.Length > 500 ? content.Substring(0, 499) : content;
            Sensitive = sensitive;
            Spoiler = spoiler;
            Visibility = v > Visibility.Unlisted ? v : Visibility.Unlisted;
        }
    }
}
