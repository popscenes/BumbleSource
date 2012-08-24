using System;
using System.Web;
using MbUnit.Framework;
using Moq;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using WebSite.Infrastructure.Authentication;
using WebSite.Test.Common;

namespace Website.Mocks.Domain.Data
{
    class TestIdentityProvidersNinjectModult : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as MoqMockingKernel;
            Assert.IsNotNull(kernel, "should be using mock kernel for tests");

            SetUpTestIdentityProviderService(kernel);

        }



        public void SetUpTestIdentityProviderService(MoqMockingKernel kernel)
        {
            var identityProviderService = kernel.GetMock<IdentityProviderServiceInterface>();
            var idenityProvider = kernel.GetMock<IdentityProviderInterface>();
            HttpContextMock.FakeHttpContext(kernel);
            var httpContext = kernel.GetMock<HttpContextBase>();

            idenityProvider.Object.Name = "hey";

            var identityProviderCredentials = new IdentityProviderCredential
                                                  {
                                                      Email = "teddymccuddles@gmail.com",
                                                      Name = "Anthony Borg",
                                                      UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho",
                                                      IdentityProvider = IdentityProviders.GOOGLE
                                                  };


            idenityProvider.Setup(_ => _.RequestAuthorisation()).Callback(() => httpContext.Object.Response.Redirect("www.google.com"));
            idenityProvider.Setup(_ => _.GetCredentials()).Returns(identityProviderCredentials);

            identityProviderService.Setup(_ => _.GetProviderByIdentifier(It.Is<string>(id => id == "TestProvider"))).Returns(idenityProvider.Object);
            
            ////////////////////////////////////////////////////////facebook///////////////////////////////////////////////////////////////

            var idenityProviderFB = new Mock<IdentityProviderInterface>();
            var accessToken = new AccessToken()
            {
                Expires = DateTime.Now.AddDays(1),
                Permissions = "user_events",
                Token = "abc123"
            };

            var identityProviderCredentialsFB = new IdentityProviderCredential
            {
                Email = "teddymccuddles@gmail.com",
                Name = "Anthony Borg",
                UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1hoFB",
                IdentityProvider = IdentityProviders.FACEBOOK,
                AccessToken = accessToken
            };

            idenityProviderFB.Setup(_ => _.RequestAuthorisation()).Callback(() => httpContext.Object.Response.Redirect("https://graph.facebook.com/oauth/access_token"));
            idenityProviderFB.Setup(_ => _.GetCredentials()).Returns(identityProviderCredentialsFB);

            identityProviderService.Setup(_ => _.GetProviderByIdentifier(It.Is<string>(id => id == IdentityProviders.FACEBOOK))).Returns(idenityProviderFB.Object);


            //kernel.Bind<IdentityProviderServiceInterface>().ToConstant(identityProviderService.Object).InSingletonScope();


        }
    }
}

