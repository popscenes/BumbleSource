using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Popscenes.Specification.Util;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Areas.WebApi.Location.Model;
using TechTalk.SpecFlow;
using Website.Common.ApiInfrastructure.Model;

namespace Popscenes.Specification.WebApi.Location
{
    [Binding]
    public class LocationAutoCompleteSteps : Steps  
    {
        [Given(@"There are (.*) suburbs with (.*) containing a word starting with the term (.*)")]
        public void GivenThereAreSuburbsWithContainingAWordStartingWithTheTerm(int suburbTotal, int prefeixTotal, string termPrefix)
        {
            var subBuild = DataUtil.GetSomeSuburbs(suburbTotal, termPrefix, prefeixTotal);
            var suburbs = subBuild.Build().ToList();
            StorageUtil.StoreAll(suburbs);
        }


        [Then(@"the result list should contain (.*) suburbs containing a word starting with one of:")]
        public void ThenTheResultListShouldContainSuburbsContainingAWordsStartingWith(int resultCount, Table table)
        {
            var content = SpecUtil.GetResponseContentAs<ResponseContent<List<AutoCompleteModel>>>();
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Data.Count, Is.EqualTo(resultCount));

            var prefixes = table.Rows.Select(row => row["Prefix"]).ToList();
            
            foreach (var res in content.Data)
            {
                Assert.True(res.Description.Split(' ').Any(s => prefixes.Any(s.StartsWith)));
            }
        }

    }
}
