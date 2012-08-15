using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using TechTalk.SpecFlow;
using PostaFlya.Controllers;
using PostaFlya.Domain.Tag;
using PostaFlya.Specification.Util;

namespace PostaFlya.Specification.TagsAndTagGroups
{
    [Binding]
    public class TagsAndTagGroupsSteps
    {
        [Given(@"i have navigated to a page the requires Tag Selection")]
        public void GivenIHaveNavigatedToAPageTheRequiresTagSelection()
        {
            var tagsController = SpecUtil.GetApiController<TagsApiController>();
            SpecUtil.ControllerResult = tagsController.Get();
        }

        [Then(@"then i should be able to choose the correct TAGS for the website")]
        public void ThenThenIShouldBeAbleToChooseTheCorrectTAGSForTheWebsite()
        {
            var TagGroups = SpecUtil.ControllerResult as Tags;
            var tagsList = TagGroups;
            Assert.Count(30, tagsList);
        }
    }
}
