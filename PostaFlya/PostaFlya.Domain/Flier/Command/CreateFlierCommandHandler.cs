using System;
using System.Collections.Generic;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Flier.Command
{
    internal class CreateFlierCommandHandler : CommandHandlerInterface<CreateFlierCommand>
    {
        private readonly FlierRepositoryInterface _flierRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly FlierQueryServiceInterface _flierQueryService;

        public CreateFlierCommandHandler(FlierRepositoryInterface flierRepository
            , UnitOfWorkFactoryInterface unitOfWorkFactory, FlierQueryServiceInterface flierQueryService)
        {
            _flierRepository = flierRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _flierQueryService = flierQueryService;
        }

        public object Handle(CreateFlierCommand command)
        {
            var newFlier = new Flier(command.Location)
                               {
                                   BrowserId = command.BrowserId,
                                   Title = command.Title,
                                   Description = command.Description,
                                   Tags = command.Tags,
                                   Image = command.Image,
                                   CreateDate = DateTime.UtcNow,
                                   FlierBehaviour = command.FlierBehaviour,
                                   EffectiveDate = command.EffectiveDate == default(System.DateTime) ? DateTime.UtcNow : command.EffectiveDate,
                                   ImageList = command.ImageList,
                                   UseBrowserContactDetails = command.AttachContactDetails && command.UseBrowserContactDetails,
                                   ExternalSource = command.ExternalSource,
                                   ExternalId = command.ExternalId
                               };

            if(newFlier.FlierBehaviour == FlierBehaviour.Default)
                newFlier.Status = FlierStatus.Active;

            newFlier.FriendlyId = _flierQueryService.FindFreeFriendlyId(newFlier);
  
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                
                _flierRepository.Store(newFlier);
            }

            if(!unitOfWork.Successful)
                return new MsgResponse("Flier Creation Failed", true)
                        .AddCommandId(command);

            return new MsgResponse("Flier Create", false)
                .AddEntityId(newFlier.Id)
                .AddCommandId(command);
        }

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _flierRepository };
        }
    }
}