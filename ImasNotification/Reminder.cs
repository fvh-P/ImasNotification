using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ImasNotification
{
    [DataContract]
    class Reminder
    {
        [DataMember]
        public DateTime? PostTime { get; set; }

        [DataMember]
        public string From { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public PostContent Post { get; set; }

        public bool Posted { get; set; }

        public Reminder(DateTime? t, string f, string c, PostContent pc)
        {
            PostTime = t;
            From = f;
            Code = c;
            Post = pc;
            Posted = false;
        }
        public static Reminder[] Deserialize(string jsontext)
        {
            Reminder[] data;
            var serializer = new DataContractJsonSerializer(typeof(Reminder[]));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsontext)))
            {
                data = (Reminder[])serializer.ReadObject(ms);
            }
            return data;
        }
        public string Serialize()
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(GetType());
                serializer.WriteObject(stream, this);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
