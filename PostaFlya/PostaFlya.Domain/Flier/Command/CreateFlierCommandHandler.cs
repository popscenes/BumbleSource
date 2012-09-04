using System;
using System.Collections.Generic;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Flier.Command
{
    internal class CreateFlierCommandHandler : CommandHandlerInterface<CreateFlierCommand>
    {
        private readonly FlierRepositoryInterface _flierRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public CreateFlierCommandHandler(FlierRepositoryInterface flierRepository
            , UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _flierRepository = flierRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
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
                                   UseBrowserContactDetails = command.AttachContactDetails && command.UseBrowserContactDetails 
                               };

            if(newFlier.FlierBehaviour == FlierBehaviour.Default)
                newFlier.Status = FlierStatus.Active;

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