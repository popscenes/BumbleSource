using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using Ninject;
using TechTalk.SpecFlow;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Binding;
using PostaFlya.Controllers;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Comments;
using PostaFlya.Specification.Util;

namespace PostaFlya.Specification.DynamicBulletinBoard
{
    [Binding]
    public class CommentSteps
    {
        #region Comment Feature

        [When(@"I submit a comment on the FLIER")]
        [Given(@"There is a comment on the FLIER")]
        public void WhenISubmitACommentOnAflier()
        {
            var viewMod = ScenarioContext.Current["fliermodel"] as DefaultDetailsViewModel;
            var bulletinCommentController = SpecUtil.GetApiController<CommentController>();
            var browserInformation = SpecUtil.GetCurrBrowser();

            var commentModel = new CreateCommentModel()
            {
                BrowserId = browserInformation.Browser.Id,
                Comment = "This is a comment",
                EntityId = viewMod.Flier.Id,
                CommentEntity = EntityTypeEnum.Flier
            };
            var res = bulletinCommentController.Post(commentModel);
            res.AssertStatusCode();
            ScenarioContext.Current["fliercommentid"] = res.EntityId();
            ScenarioContext.Current["fliercommentmodel"] = commentModel;
        }

        [When(@"I submit a comment on the FLIER that fails")]
        public void WhenISubmitACommentOnAflierThatFails()
        {
            var viewMod = ScenarioContext.Current["fliermodel"] as DefaultDetailsViewModel;
            var bulletinCommentController = SpecUtil.GetApiController<CommentController>();
            var browserInformation = SpecUtil.GetCurrBrowser();

            var commentModel = new CreateCommentModel()
            {
                BrowserId = browserInformation.Browser.Id,
                Comment = null,
                EntityId = viewMod.Flier.Id,
                CommentEntity = EntityTypeEnum.Flier
            };
            var res = bulletinCommentController.Post(commentModel);
            res.AssertStatusCodeFailed();
            ScenarioContext.Current["responsemessage"] = res;
            ScenarioContext.Current["fliercommentmodel"] = commentModel;
        }

        [Then(@"the comment should be stored against the FLIER")]
        public void ThenTheCommentShouldBeStoredAgainstTheFlier()
        {
            var commentId = ScenarioContext.Current["fliercommentid"] as string;
            var viewMod = ScenarioContext.Current["fliermodel"] as DefaultDetailsViewModel;
            var commentModel = ScenarioContext.Current["fliercommentmodel"] as CreateCommentModel;
            var queryService = SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>();
            var comments = queryService.GetComments(viewMod.Flier.Id);
            Assert.IsTrue(comments.Any(c =>
                c.BrowserId == commentModel.BrowserId &&
                c.CommentContent == commentModel.Comment));

        }

        [Then(@"I should see the comments for the flier")]
        public void ThenIShouldSeeTheCommentsForTheFlier()
        {
            var viewMod = ScenarioContext.Current["fliermodel"] as DefaultDetailsViewModel;
            var commentModel = ScenarioContext.Current["fliercommentmodel"] as CreateCommentModel;
            var bulletinCommentController = SpecUtil.GetApiController<CommentController>();
            var ret = bulletinCommentController.Get(EntityTypeEnum.Flier, viewMod.Flier.Id);
            Assert.IsNotNull(ret);
            Assert.IsNotEmpty(ret);
            Assert.IsTrue(ret.Any(c =>
                c.Browser.Id == commentModel.BrowserId &&
                c.Comment == commentModel.Comment));

        }

        #endregion
    }
}
