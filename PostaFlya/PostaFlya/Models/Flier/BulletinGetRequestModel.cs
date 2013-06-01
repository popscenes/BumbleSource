using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Http;
using PostaFlya.Models.Location;

namespace PostaFlya.Models.Flier
{
    /// <summary>
    /// test 123456
    /// </summary>
    [DataContract]
    public class BulletinGetRequestModel : RequestModelInterface
    {
        [DataMember]
        public LocationModel Loc { get; set; }
        [DataMember(IsRequired = false)]
        public int Count { get; set; }
        [DataMember]
        public string Board { get; set; }
        [DataMember]
        public string SkipPast { get; set; }
        [DataMember(IsRequired = false)]
        public int Distance { get; set; }
        [DataMember]
        public string Tags { get; set; }
        [DataMember]
        public DateTime? Date { get; set; }
    }

    public interface RequestModelInterface
    {
    }
}