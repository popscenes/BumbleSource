using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using PostaFlya.Domain.Browser;
using WebSite.Infrastructure.Authentication;
using Ninject.MockingKernel.Moq;

namespace PostaFlya.Mocks.Domain.Data
{
    public static class PrincipalData
    {
        public static void SetPrincipal(MoqMockingKernel kernel)
        {
            kernel.Unbind<WebPrincipalInterface>();
            
            //var mockHttpContext = kernel.GetMock<HttpContextBase>();
            var mockPrincipal = kernel.GetMock<WebPrincipalInterface>();

            kernel.Bind<WebPrincipalInterface>()
                .ToConstant(mockPrincipal.Object).InSingletonScope();

            kernel.Unbind<IdentityProviderServiceInterface>();

            var browser = kernel.Get<BrowserInterface>(ctx => ctx.Has("ststestbrowser"));
            var externalCreds = browser.ExternalCredentials.ElementAt(0);
            //hmmm should IdentityProviderCredentialStore email address and everything.
            mockPrincipal.Setup(p => p.NameIdentifier).Returns(externalCreds.UserIdentifier);
            mockPrincipal.Setup(p => p.Name).Returns(browser.Handle);
            mockPrincipal.Setup(p => p.EmailAddress).Returns(browser.EmailAddress);
            mockPrincipal.Setup(p => p.IdentityProvider).Returns(externalCreds.IdentityProvider);
            mockPrincipal.Setup(p => p.Identity.IsAuthenticated).Returns(true);
            mockPrincipal.Setup(p => p.ToCredential()).Returns(externalCreds);
        }
    }
}
