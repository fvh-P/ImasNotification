﻿using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Linq;
using System.Collections.Generic;

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
        public static IEnumerable<Reminder> Deserialize(string jsontext)
        {
            var serializer = new DataContractJsonSerializer(typeof(IEnumerable<Reminder>));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsontext)))
            {
                return (IEnumerable<Reminder>)serializer.ReadObject(ms);
            }
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
