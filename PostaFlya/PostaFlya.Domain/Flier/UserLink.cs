using System;

namespace PostaFlya.Domain.Flier
{
    [Serializable]
    public class UserLink
    {
        public LinkType Type { get; set; }
        public String Text { get; set; }
        public String Link { get; set; }
    }
}