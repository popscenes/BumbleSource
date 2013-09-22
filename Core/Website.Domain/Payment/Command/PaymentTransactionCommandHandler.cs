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
        private readonly GenericRepositoryInterface _genericRepository;

        public PaymentTransactionCommandHandler(GenericRepositoryInterface genericRepository)
        {
            _genericRepository = genericRepository;
        }

        public void Handle(PaymentTransactionCommand command)
        {

            var paymentEntity = command.Entity;
            var transaction = command.Transaction;

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

    }
}