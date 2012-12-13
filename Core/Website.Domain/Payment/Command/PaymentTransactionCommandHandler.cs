using System;
using System.Collections.Generic;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Domain.Service;

//using Website.Infrastructure.Service;

namespace Website.Domain.Payment.Command
{
    internal class PaymentTransactionCommandHandler : CommandHandlerInterface<PaymentTransactionCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _genericRepository;
        private readonly GenericQueryServiceInterface _genericQueryService;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;

        public PaymentTransactionCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
            , GenericRepositoryInterface genericRepository
            , GenericQueryServiceInterface genericQueryService
            , DomainEventPublishServiceInterface domainEventPublishService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericRepository = genericRepository;
            _genericQueryService = genericQueryService;
            _domainEventPublishService = domainEventPublishService;
        }

        public object Handle(PaymentTransactionCommand command)
        {

            var paymentEntity = command.Entity;
            var transaction = command.Transaction;

            var uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)
            {
                _genericRepository.Store(transaction);
            }

            if (paymentEntity is ChargableEntityInterface && transaction.Status == PaymentTransactionStatus.Success)
            {
                uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
                using (uow)
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

           return new MsgResponse("Payment Transaction", false)
             .AddCommandId(command)
             .AddEntityId(transaction.Id);
        }

    }
}