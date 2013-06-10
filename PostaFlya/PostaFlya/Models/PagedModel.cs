using System.Runtime.Serialization;

namespace PostaFlya.Models
{
    [DataContract]
    public class PagedModel
    {
        [DataMember(IsRequired = false)]
        public int Count { get; set; }

        [DataMember(IsRequired = false)]
        public int Skip { get; set; }
    }
}