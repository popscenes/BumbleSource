using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PostaFlya.Models.Tags
{
    public class TagSetListViewModel
    {
        [Display(Name = "TagList", ResourceType = typeof(Properties.Resources))] 
        public IList<Website.Domain.Tag.Tags> TagList { get; set; }
    }
}