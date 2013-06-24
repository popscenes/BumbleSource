﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Popscenes.Specification.Util;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Areas.MobileApi.Infrastructure.Model;
using PostaFlya.Domain.Flier;
using TechTalk.SpecFlow;

namespace Popscenes.Specification.MobileApi.Bulletin
{
    [Binding]
    public class FlyerDetailSteps : Steps
    {
        // For additional details on SpecFlow step definitions see http://go.specflow.org/doc-stepdef

        [Given(@"There is an (.*) flyer with the id (.*)")]
        public void GivenThereIsAnActiveFlyerWithTheId(FlierStatus status, Guid id)
        {
            var flyerBuild = DataUtil.GetAFlyer(id, status);
            var flyer = flyerBuild.Build();

            StorageUtil.Store(flyer);
        }

        [Then(@"The content should contain the detail for a flyer with the id (.*)")]
        public void ThenTheContentShouldContainTheDetailForAFlyerWithTheId(Guid id)
        {
            var content = SpecUtil.GetResponseContentAs<ResponseContent<FlyerDetailModel>>();
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Data, Is.Not.Null);
            Assert.That(content.Data.Id, Is.EqualTo(id.ToString()));
            Assert.That(content.Data.Description, Is.Not.Null);
            Assert.That(content.Data.Description, Is.Not.Empty);

        }

    }
}