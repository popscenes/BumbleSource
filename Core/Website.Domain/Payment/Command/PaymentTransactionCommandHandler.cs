using System;
using System.Collections.Generic;
using System.Linq;
using Website.Domain.Claims;
using Website.Domain.Claims.Command;
using Website.Domain.Claims.Event;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Domain.Browser;
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
            var payerId = command.PayerId;
            var paymentId = command.PaymentId;
            var amount = command.PaymentAmount;
            var type = command.PaymentType;
            var status = command.PaymentTransactionStatus;

            var transaction = new PaymentTransaction()
                {
                    PayerId = payerId,
                    PaymentEntityId = paymentEntity.Id,
                    TransactionId = paymentId,
                    Type = type,
                    Amount = amount,
                    Status = status,
                    Id = Guid.NewGuid().ToString(),
                    FriendlyId = "trns" + paymentId,
                    AggregateId = payerId,
                    BrowserId = payerId,
                    Message = command.ErrorMessage
                };

            var uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)
            {
                _genericRepository.Store(transaction);
            }

            if (paymentEntity is ChargableEntityInterface && status == PaymentTransactionStatus.Success)
            {
                uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
                using (uow)
                {
                    _genericRepository.UpdateEntity(paymentEntity.GetType()
                    , paymentEntity.Id
                    , o =>
                    {
                        var chargeable = o as ChargableEntityInterface;
                        if (chargeable != null) chargeable.AccountCredit += amount;
                    });
                }
            }

           return new MsgResponse("Payment Transaction", false)
             .AddCommandId(command)
             .AddEntityId(transaction.Id);
        }

    }
}