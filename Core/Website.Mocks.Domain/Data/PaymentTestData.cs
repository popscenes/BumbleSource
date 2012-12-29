using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using Website.Application.Domain.Payment;

namespace Website.Mocks.Domain.Data
{
    class PaymentTestData : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as MoqMockingKernel;
            kernel.Unbind<PaymentServiceProviderInterface>();
            var paymentServiceProviderMock = kernel.GetMock<PaymentServiceProviderInterface>();

            kernel.Bind<PaymentServiceProviderInterface>()
                .ToConstant(paymentServiceProviderMock.Object).InSingletonScope();

            var paymnetServiceList = new List<PaymentServiceInterface>
                {
                    new PaymentServiceTest() {PaymentServiceName = "Test 1"},
                    new PaymentServiceTest() {PaymentServiceName = "Test 2"}
                };


            paymentServiceProviderMock.Setup(p => p.GetAllPaymentServices()).Returns(paymnetServiceList);
            paymentServiceProviderMock.Setup(p => p.GetPaymentServiceByName(It.IsAny<String>())).Returns(
                paymnetServiceList.First());

        }
    }
}
