using System.Collections.Generic;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Flier.Command
{
    internal class EditFlierCommandHandler : CommandHandlerInterface<EditFlierCommand>
    {
        private readonly FlierRepositoryInterface _flierRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly FlierQueryServiceInterface _flierQueryService;

        public EditFlierCommandHandler(FlierRepositoryInterface flierRepository
            ,UnitOfWorkFactoryInterface unitOfWorkFactory
            , FlierQueryServiceInterface flierQueryService)
        {
            _flierRepository = flierRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _flierQueryService = flierQueryService;
        }

        public object Handle(EditFlierCommand command)
        {
            UnitOfWorkInterface unitOfWork;
            using(unitOfWork = _unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                var flierQuery = _flierQueryService.FindById<Flier>(command.Id);
                if (flierQuery == null || flierQuery.BrowserId == null || !flierQuery.BrowserId.Equals(command.BrowserId))
                    return false;

                var friendlyId = _flierQueryService.FindFreeFriendlyId(flierQuery);

                _flierRepository.UpdateEntity<Flier>(command.Id, 
                    flier =>
                        {
                            flier.FriendlyId = friendlyId;
                            flier.Title = command.Title;
                            flier.Description = command.Description;
                            flier.Tags = command.Tags;
                            flier.Location = command.Location;
                            flier.Image = command.Image;
                            flier.EffectiveDate = command.EffectiveDate;
                            flier.ImageList = command.ImageList;
                        });

                                    
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Flier Edit Failed", true)
                    .AddCommandId(command);

            return new MsgResponse("Flier Edit", false)
                .AddEntityId(command.Id)
                .AddCommandId(command);
        }

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _flierRepository };
        }
    }
}