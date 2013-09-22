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
        private readonly GenericRepositoryInterface _genericRepository;

        public SetBrowserCreditCommandHandler(GenericRepositoryInterface genericRepository)
        {
            _genericRepository = genericRepository;
        }

        public void Handle(SetBrowserCreditCommand command)
        {

            _genericRepository.UpdateEntity<Browser>(command.BrowserId
                    , browserUpdate =>
                        {
                            browserUpdate.AccountCredit = command.Credit;
                        });

        }
    }
}
