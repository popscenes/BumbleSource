using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Intergrations;
using Website.Application.Intergrations.Payment;
using Website.Test.Common.Facebook;

namespace Website.Application.Tests.Intergrations
{
    [TestFixture]    
    class GoogleWalletDigitalGoodsTests
    {

        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        GoogleWalletDigitalGoods digitalGoodsApi = new GoogleWalletDigitalGoods();

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            digitalGoodsApi.SellerId = "08584053315349997151";
            digitalGoodsApi.Secret = "FSTqPHezgu_-8Hbjpf5OGw";

        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
        }


        [Test]
        public void GenerateJWTandDecode()
        {

            var digitalGoodsOrder = new DigitalGoodsOrder();
            digitalGoodsOrder.Name = "$10 Account Credit";
            digitalGoodsOrder.Description = "Postaflya credit";
            digitalGoodsOrder.Price = 10.00;
            digitalGoodsOrder.Currency = GoogleWalletDigitalGoods.CurrencyCode.AUD;
            digitalGoodsOrder.UserData["browserID"] = "browserId";
            digitalGoodsOrder.UserData["entityId"] = "entityId";

            String jwt = digitalGoodsApi.GenerateJWT(digitalGoodsOrder);
            Assert.IsNotEmpty(jwt);
            digitalGoodsOrder = digitalGoodsApi.OrderFromJWT(jwt);

            Assert.NotNull(digitalGoodsOrder.OrderId);
            Assert.AreEqual(digitalGoodsOrder.Name, "$10 Account Credit");
            Assert.AreEqual(digitalGoodsOrder.Description, "Postaflya credit");
            Assert.AreEqual(digitalGoodsOrder.Price, 10.00);
            Assert.AreEqual(digitalGoodsOrder.Currency, GoogleWalletDigitalGoods.CurrencyCode.AUD);
            Assert.AreEqual(digitalGoodsOrder.UserData["browserID"], "browserId");
            Assert.AreEqual(digitalGoodsOrder.UserData["entityId"], "entityId");
        }
    }
}
