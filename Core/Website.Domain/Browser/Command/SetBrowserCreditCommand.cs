using System;
using System.Collections.Generic;
using Website.Domain.Payment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;

namespace Website.Domain.Browser.Command
{
    [Serializable]
    public class SetBrowserCreditCommand : DefaultCommandBase, BrowserIdInterface
    {
        public string BrowserId { get; set; }
        public double Credit { get; set; }
    }

    public class SetBrowserCreditCommandHandler : MessageHandlerInterface<SetBrowserCreditCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _genericRepository;

        public SetBrowserCreditCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory, GenericRepositoryInterface genericRepository)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericRepository = genericRepository;
        }

        public object Handle(SetBrowserCreditCommand command)
        {
            var uow = _unitOfWorkFactory.GetUnitOfWork(new List<object>() { _genericRepository });
            using (uow)//one unit of work for this
            {

                _genericRepository.UpdateEntity<Browser>(command.BrowserId
                                                  , browserUpdate =>
                                                      {
                                                          browserUpdate.AccountCredit = command.Credit;
                                                      });

            };

            var response = (!uow.Successful)
                           ? new MsgResponse("Error updating browser credit", true)
                           : new MsgResponse("browser credit updated", false);
            return response;
        }
    }
}
