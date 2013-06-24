using System.Collections.Generic;
using PostaFlya.Domain.Boards.Event;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    internal class EditBoardFlierCommandHandler : CommandHandlerInterface<EditBoardFlierCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;


        public EditBoardFlierCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory, GenericQueryServiceInterface queryService, GenericRepositoryInterface repository, DomainEventPublishServiceInterface domainEventPublishService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _queryService = queryService;
            _repository = repository;
            _domainEventPublishService = domainEventPublishService;
        }

        public object Handle(EditBoardFlierCommand command)
        {
            UnitOfWorkInterface unitOfWork = null;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _repository }))
            {

                var boardId = command.BoardId;
                    _repository.UpdateEntity<Domain.Flier.Flier>(command.FlierId,
                    flier =>
                    {
                        if (flier.Boards == null)
                            flier.Boards = new HashSet<string>();

                        flier.Boards.Add(boardId);

                    });
                


            }
            if (!unitOfWork.Successful)
                return new MsgResponse("Edit Flyer to board failed", true)
                    .AddCommandId(command);

            return new MsgResponse("Edit Flyer to board", false)
                .AddCommandId(command);
        }
    }
}