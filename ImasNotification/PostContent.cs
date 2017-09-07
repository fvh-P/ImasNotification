using System.Runtime.Serialization;

namespace ImasNotification
{
    [DataContract]
    class PostContent
    {
        [DataMember]
        public int? Id { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public bool Sensitive { get; set; }

        [DataMember]
        public string Spoiler { get; set; }

        public PostContent(int? id, string content, bool sensitive, string spoiler)
        {
            
            Id = id;
            Content = content.Length > 500 ? content.Substring(0, 499) : content;
            Sensitive = sensitive;
            Spoiler = spoiler;
        }
    }
}
