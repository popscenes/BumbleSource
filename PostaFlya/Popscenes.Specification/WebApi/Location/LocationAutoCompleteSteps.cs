using System;
using System.Collections.Generic;
using System.Linq;
using System.Spatial;
using System.Text;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using Popscenes.Specification.Util;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Areas.WebApi.Location.Model;
using PostaFlya.DataRepository.Search.SearchRecord;
using TechTalk.SpecFlow;
using Website.Azure.Common.Sql;
using Website.Common.ApiInfrastructure.Model;
using Website.Domain.Location;

namespace Popscenes.Specification.WebApi.Location
{
    [Binding]
    public class LocationAutoCompleteSteps : Steps
    {

        public const string ScenarioSuburbList = "ScenarioSuburbList";

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

        [Given(@"There are (.*) suburbs within (.*) kilometers of the geolocation (.*), (.*)")]
        public void GivenThereAreSuburbsWithinKilometersOfTheGeolocation(int suburbcount, int kilometres, double latitude, double longitude)
        {
            var subBuild = DataUtil.GetSomeSuburbs(suburbcount, "",  1, kilometres, latitude, longitude);
            var suburbs = subBuild.Build().ToList();
            StorageUtil.StoreAll(suburbs);
            ScenarioContext.Current[ScenarioSuburbList] = suburbs;

        }

        [Then(@"the result should contain the suburb nearest to the geolocation (.*), (.*)")]
        public void ThenTheResultShouldContainTheSuburbNearestToTheGeolocation(double latitude, double longitude)
        {
            var content = SpecUtil.GetResponseContentAs<ResponseContent<SuburbModel>>();
            Assert.That(content, Is.Not.Null);
            Assert.That( content.Data, Is.Not.Null);
            var expectedLoc = content.Data;

            var point = SqlGeography.Point(latitude, longitude, SqlExecute.Srid); 
            var subs = ScenarioContext.Current[ScenarioSuburbList] as List<Suburb>;
            var closest = (from s in subs
                           orderby s.ToGeography()
                            .STDistance(point)
                            .Value select s).First();


            Assert.That(closest.Id, Is.EqualTo(expectedLoc.Id));
        }


    }
}
