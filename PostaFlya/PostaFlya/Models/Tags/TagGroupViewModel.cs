using System;
using Website.Domain.Tag;

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
    public Tags TagsList { get; set; }

}