using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using TechTalk.SpecFlow;
using PostaFlya.Mocks.Domain.Data;
using Website.Mocks.Domain.Data;
using Website.Test.Common;
using Website.Application.Domain.Browser;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Mocks.Domain.Defaults;

namespace PostaFlya.Specification.Util
{
    internal static class SpecUtil
    {
        public static BrowserInformationInterface GetCurrBrowser()
        {
            return CurrIocKernel.Get<BrowserInformationInterface>();
        }

        public static T GetMockStore<T>(string name) where T : class, new()
        {
            T store = null;
            if(!ScenarioContext.Current.TryGetValue(name, out store))
            {
                store = new T();
                ScenarioContext.Current.Add(name, store);
            }

            return store;
        }

        public static T GetController<T>() where T: class 
        {
            object currentController = null;
            if(!ScenarioContext.Current.TryGetValue("controller", out currentController)
                || !(currentController is T))            
            {
                if (currentController != null)
                    ScenarioContext.Current.Remove("controller");
                
                currentController = CurrIocKernel.Get<T>();
                //ControllerContextMock.FakeControllerContext(CurrIocKernel, (Controller)currentController);
                ScenarioContext.Current.Add("controller", currentController);
            }
            return currentController as T;
        }

        public static T GetApiController<T>() where T : ApiController
        {
            var controller = GetController<T>();
            if (controller.ControllerContext == null)
                ApiControllerContextMock.FakeHttpControllerContext(CurrIocKernel, controller);
            return controller;
        }

        public static RequestContext RequestContext<T>() where T : class, IPrincipal
        {
            var principal = CurrIocKernel.GetMock<T>();

            var httpContext = new Moq.Mock<HttpContextBase>();
            httpContext.Setup(x => x.User).Returns(principal.Object);
            // ... mock other httpContext's properties, methods, as needed 

            var reqContext = new RequestContext(httpContext.Object, new RouteData());
            return reqContext;
        }

        public static object ControllerResult
        {
            get
            {
                object result = null;
                if(!ScenarioContext.Current.TryGetValue("actionresult", out result))
                        Assert.Fail("no current ActionResult set");
                return result;
            }
            set
            {
                if (ScenarioContext.Current.ContainsKey("actionresult"))
                    ScenarioContext.Current.Remove("actionresult");
                ScenarioContext.Current.Add("actionresult", value);
            }
        }

//        example of using the mock kernel, to mock a command handler
//        var addTagHandler = SpecUtil.CurrIocKernel.GetMock<CommandHandlerInterface<AddTagCommand>>();
//        addTagHandler.Setup(s => s.Handle(It.IsAny<AddTagCommand>())).Returns(true);
//        See also MockRepositoriesNinjectModule
        public static MoqMockingKernel CurrIocKernel
        {
            get
            {
                MoqMockingKernel currIocKernel = null;
                if(!ScenarioContext.Current.TryGetValue("mockingkernel", out currIocKernel))
                {
                    var testKernel = new TestKernel(NinjectModules);
                    currIocKernel = testKernel.Kernel;
                    ScenarioContext.Current.Add("mockingkernel", currIocKernel);
                }
                return currIocKernel;
            }
        }

        public static BrowserInterface AssertGetParticipantBrowser(string id)
        {
            var browserQuery = SpecUtil.CurrIocKernel.Get<BrowserQueryServiceInterface>();
            var browser = browserQuery.FindById<Browser>(id);
            Assert.IsTrue(browser != null);
            Assert.IsTrue(browser.HasRole(Role.Participant));
            return browser;
        }

        private static readonly List<INinjectModule> NinjectModules 
            = new List<INinjectModule>()
                  {
                      new GlobalDefaultsNinjectModule(),
                      new DefaultsNinjectModule(),
                      new Website.Infrastructure.Binding.InfrastructureNinjectBinding(),
                      new PostaFlya.Domain.Binding.DefaultServicesNinjectBinding(),
                      new Website.Domain.Binding.DefaultServicesNinjectBinding(),
                      new PostaFlya.Domain.Binding.CommandNinjectBinding(),
                      new Website.Domain.Binding.CommandNinjectBinding(),
                      new PostaFlya.Domain.TaskJob.Binding.TaskJobNinjectBinding(),
                      new PostaFlya.Binding.WebNinjectBindings(),  
                      new PostaFlya.Areas.Default.Binding.DefaultBehaviourWebNinjectBinding(),
                      new PostaFlya.Areas.TaskJob.Binding.TaskJobBehaviourWebNinjectBinding(),
                      new MockRepositoriesNinjectModule(),
                      new Website.Mocks.Domain.Data.TestIdentityProvidersNinjectModult(),
                      new ExternalSourceNinjectModule(),
                      new PaymentTestData()
                  };
    }
}
