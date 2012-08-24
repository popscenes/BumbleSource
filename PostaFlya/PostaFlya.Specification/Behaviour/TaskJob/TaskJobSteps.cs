using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using MbUnit.Framework;
using Ninject;
using TechTalk.SpecFlow;
using PostaFlya.Areas.TaskJob.Controllers;
using PostaFlya.Areas.TaskJob.Models;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using PostaFlya.Domain.TaskJob.Query;
using WebSite.Infrastructure.Command;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using PostaFlya.Specification.Fliers;
using PostaFlya.Specification.Util;
using Website.Domain.Location;

namespace PostaFlya.Specification.Behaviour.TaskJob
{
    [Binding]
    public class TaskJobSteps
    {
        [When(@"I submit the following data for the TASKJOB:")]
        public void WhenISubmitRequiredDataForTheTaskjob(Table createParams)
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;

            var createModel = new TaskJobBehaviourCreateModel
                                  {
                                      FlierId = flier.Id,
                                      CostOverhead = Double.Parse(createParams.Rows[0]["CostOverhead"]),
                                      MaxAmount = Double.Parse(createParams.Rows[0]["MaxAmount"]),
                                      ExtraLocations = createParams.Rows
                                      .Select(r => new LocationModel(new Location(r["ExtraLocations"])))
                                      .Where(l => l.IsValid()).ToList()
                                  };

            TaskJobBehcaiourCreate(createModel);
        }

        public void TaskJobBehcaiourCreate(TaskJobBehaviourCreateModel createModel)
        {
            var taskJobApiController = SpecUtil.GetApiController<MyTaskJobsController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            var res = taskJobApiController.Post(browserInformation.Browser.Id, createModel);
            Assert.IsNotNull(res);
            Assert.AreEqual(HttpStatusCode.Created, res.StatusCode);

            ScenarioContext.Current["taskjobcreatemodel"] = createModel;
        }


        [Then(@"the TASKJOB will be stored for the FLIER")]
        public void ThenTheTaskjobWillBeStoredForTheFlier()
        {
            var createModel = ScenarioContext.Current["taskjobcreatemodel"] as TaskJobBehaviourCreateModel;
            Assert.IsNotNull(createModel);

            var flier = ScenarioContext.Current["flier"] as FlierInterface;

            var queryService = SpecUtil.CurrIocKernel.Get<TaskJobQueryServiceInterface>();
            var behaviour = queryService.FindById<TaskJobFlierBehaviour>(flier.Id);
            Assert.IsNotNull(behaviour);

            Assert.AreEqual(createModel.MaxAmount, behaviour.MaxAmount);
            Assert.AreEqual(createModel.CostOverhead, behaviour.CostOverhead);
            Assert.AreEqual(behaviour.ExtraLocations, new Locations(createModel.ExtraLocations.Select(l => l.ToDomainModel())));

            ScenarioContext.Current["taskbeaviour"] = behaviour;
        }

        [Given(@"there is a flier with TASKJOB behaviour")]
        public void GivenThereIsAFlierWithTaskjobBehaviour()
        {
            new FlierSteps().GivenIHaveCreatedAflierofBehaviour("TaskJob");
            var flier = ScenarioContext.Current["flier"] as FlierInterface;

            var createModel = new TaskJobBehaviourCreateModel
            {
                FlierId = flier.Id,
                CostOverhead = 100,
                MaxAmount = 150,
                ExtraLocations = new List<LocationModel>() { SpecUtil.CurrIocKernel.Get<Location>(ib => ib.Get<bool>("default")).ToViewModel()}
            };

            TaskJobBehcaiourCreate(createModel);
            ThenTheTaskjobWillBeStoredForTheFlier();
        }

        [Then(@"I should be able to navigate to the BID page for that TASKJOB")]
        public void ThenIShouldBeAbleToNavigateToTheBidPageForThatTaskjob()
        {
            var taskJobBidController = SpecUtil.GetController<TaskJobBidController>();
            var taskJob = ScenarioContext.Current["taskbeaviour"] as TaskJobFlierBehaviourInterface;
            var vr = taskJobBidController.Get(taskJob.Id) as ViewResult;
            var model = vr.Model as TaskJobBidModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(model.TaskJobId, taskJob.Id);
            ScenarioContext.Current["taskjobbidmodel"] = model;
        }


        [Given(@"I have navigated to the BID page for a TASKJOB")]
        public void GivenIHaveNavigatedToTheBidPageForAtaskjob()
        {
            GivenThereIsAFlierWithTaskjobBehaviour();
            ThenIShouldBeAbleToNavigateToTheBidPageForThatTaskjob();
        }

        [When(@"I place a TASKBID on the TASKJOB")]
        public void WhenIPlaceATaskbidOnTheTaskjob()
        {
            var model = ScenarioContext.Current["taskjobbidmodel"] as TaskJobBidModel;
            model.BidAmount = 101;

            var browserInformation = SpecUtil.GetCurrBrowser();

            var myTaskBidsController = SpecUtil.GetApiController<MyTaskBidsController>();
            var ret = myTaskBidsController.Post(browserInformation.Browser.Id, model);
            ret.AssertStatusCode();
            var behaviourid = ret.EntityId();
            Assert.AreEqual(model.TaskJobId, behaviourid);
        }

        [Then(@"the TASKBID will be registered against the TASKJOB")]
        public void ThenTheTaskbidWillBeRegisteredAgainstTheTaskjob()
        {
            var model = ScenarioContext.Current["taskjobbidmodel"] as TaskJobBidModel;
            var browserInformation = SpecUtil.GetCurrBrowser();

            var taskJobQueryService = SpecUtil.CurrIocKernel.Get<TaskJobQueryServiceInterface>();
            var bid = taskJobQueryService.GetBids(model.TaskJobId).SingleOrDefault(b => b.BrowserId == browserInformation.Browser.Id);
            Assert.IsNotNull(bid);
            Assert.AreEqual(model.BidAmount, bid.BidAmount);
        }
    }
}
