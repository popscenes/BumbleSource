using System;
using Website.Domain.Tag;

namespace PostaFlya.Models.Tags
{
    public static class TagGroupExtension
    {
        public static TagGroupViewModel ToViewModel(this TagGroup tagGroup)
        {
            //dynamic behav = flier.Behaviour;
            return new TagGroupViewModel(){
                ParentTag = tagGroup.ParentTag,
                TagsList = tagGroup.TagsList
            };
        }
    }

    public class TagGroupViewModel
    {
        public String ParentTag { get; set; }
        public Website.Domain.Tag.Tags TagsList { get; set; }

    }
}