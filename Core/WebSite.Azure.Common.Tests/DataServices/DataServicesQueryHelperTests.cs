using System;
using System.Data.Services.Client;
using System.Net;
using MbUnit.Framework;
using WebSite.Azure.Common.DataServices;
using WebSite.Azure.Common.Environment;
using WebSite.Infrastructure.Domain;

namespace WebSite.Azure.Common.Tests.DataServices
{
    [TestFixture]
    public class DataServicesQueryHelperTests
    {
         
        [Test]
        public void AzureRepositoryHelperQueryRetryTestBackOffSucceedsWithin3Tries()
        {
            var ret = TestRetry(3);
            Assert.IsNotNull(ret);
        }

        [Test]
        public void AzureRepositoryHelperQueryRetryTestBackOffGivesUpAfter3Tries()
        {
            var ret = TestRetry(4);
            Assert.IsNull(ret);
        }

        private SomeEntityInterface TestRetry(int failNumber)
        {
            var nCount = 0;
            Func<SomeEntityInterface> testFunc = () =>
            {
                if (++nCount < failNumber)
                    throw new DataServiceQueryException("Test",
                                                        new DataServiceClientException(
                                                            "Busy",
                                                            (int)
                                                            HttpStatusCode.
                                                                GatewayTimeout));
                return new SomeEntity();//expect exception here if debugging ^
            };

            return DataServicesQueryHelper.QueryRetry(testFunc);
        }
    }

    internal interface SomeEntityInterface : EntityInterface
    {
    }

    internal class SomeEntity : SomeEntityInterface
    {
        public string Id { get; set; }
        public int Version { get; set; }
        public Type PrimaryInterface { get; private set; }
 
    }
}
