using System;
using System.Collections.Generic;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;

//using Website.Infrastructure.Service;

namespace Website.Domain.Payment.Command
{
    internal class PaymentTransactionCommandHandler : MessageHandlerInterface<PaymentTransactionCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _genericRepository;

        public PaymentTransactionCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
            , GenericRepositoryInterface genericRepository)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericRepository = genericRepository;
        }

        public object Handle(PaymentTransactionCommand command)
        {

            var paymentEntity = command.Entity;
            var transaction = command.Transaction;

            var uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)//one unit of work for this
            {
                _genericRepository.Store(transaction);
                if (paymentEntity is ChargableEntityInterface 
                    && transaction.Status == PaymentTransactionStatus.Success)
                {
                    _genericRepository.UpdateEntity(paymentEntity.GetType()
                    , paymentEntity.Id
                    , o =>
                    {
                        var chargeable = o as ChargableEntityInterface;
                        var package = command.Package as CreditPaymentPackage;
                        if (chargeable != null && package != null) chargeable.AccountCredit += package.Credits;
                    });
                }
            }

            if (!uow.Successful)
                return new MsgResponse("Payment Transaction Failed", true)
                  .AddCommandId(command)
                  .AddEntityId(transaction.Id);


           return new MsgResponse("Payment Transaction", false)
             .AddCommandId(command)
             .AddEntityId(transaction.Id);
        }

    }
}