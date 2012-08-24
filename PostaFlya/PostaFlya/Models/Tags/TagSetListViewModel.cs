using System.Collections.Generic;
using System.ComponentModel;

namespace PostaFlya.Models.Tags
{
    public class TagSetListViewModel
    {
        [DisplayName("TagList")]
        public IList<Website.Domain.Tag.Tags> TagList { get; set; }
    }
}