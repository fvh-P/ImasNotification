using System;
using System.Runtime.Serialization;

namespace ImasNotification
{
    [DataContract]
    public class Job
    {
        [DataMember(Name ="has_time")]
        public bool HasTime { get; set; }

        [DataMember(Name = "time")]
        private string Timetext { get; set; }

        [DataMember(Name = "really_time")]
        public string ReallyTime { get; set; }

        [DataMember(Name = "team")]
        public string Team { get; set; }

        [DataMember(Name = "item")]
        public string Item { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "post")]
        public string Post { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }

        public DateTime Time => DateTime.ParseExact(Timetext, "yyyy-MM-dd HH':'mm':'ss zzz", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None);
    }
}
