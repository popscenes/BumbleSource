using System.Linq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Infrastructure.Authentication;
using Website.Domain.Browser;

namespace Website.Mocks.Domain.Data
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
            mockPrincipal.Setup(p => p.Name).Returns(browser.FriendlyId);
            mockPrincipal.Setup(p => p.EmailAddress).Returns(browser.EmailAddress);
            mockPrincipal.Setup(p => p.IdentityProvider).Returns(externalCreds.IdentityProvider);
            mockPrincipal.Setup(p => p.Identity.IsAuthenticated).Returns(true);
            mockPrincipal.Setup(p => p.ToCredential()).Returns(externalCreds);
        }
    }
}
