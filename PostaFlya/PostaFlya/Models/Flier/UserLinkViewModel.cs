using System.Collections.Generic;
using PostaFlya.Domain.Flier;

namespace PostaFlya.Models.Flier
{
    public static class UserLinkViewModelExtensions
    {
        public static UserLinkViewModel ToCreateModel(this UserLink userLink)
        {
            //dynamic behav = flier.Behaviour;
            return new UserLinkViewModel()
                {
                    Link = userLink.Link,
                    Text = userLink.Text,
                    Type = userLink.Type
                };
        }

    }

    public class UserLinkViewModel
    {
        public LinkType Type { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }

        
    }
}