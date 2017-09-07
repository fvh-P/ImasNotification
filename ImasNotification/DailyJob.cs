using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ImasNotification
{
    [DataContract]
    class DailyJob
    {
        [DataMember(Name = "date")]
        private string DateString { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "content")]
        public List<Job> Jobs { get; set; }

        public DateTime Date => DateTime.ParseExact(DateString, "yyyy-MM-dd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None);

        public static DailyJob Deserialize(string jsontext)
        {
            DailyJob data;
            var serializer = new DataContractJsonSerializer(typeof(DailyJob));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsontext)))
            {
                data = (DailyJob)serializer.ReadObject(ms);
            }
            return data;
        }
    }
}
