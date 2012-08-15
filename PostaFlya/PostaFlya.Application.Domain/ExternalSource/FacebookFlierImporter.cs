using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using PostaFlya.Application.Domain.Content;
using PostaFlya.Domain.Content.Command;
using WebSite.Infrastructure.Authentication;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Flier.Query;
using WebSite.Application.Intergrations;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Application.Domain.ExternalSource
{
    public class FacebookFlierImporter: FlierImporterInterface
    {

        private readonly FlierQueryServiceInterface _queryService;
        private readonly UrlContentRetrieverFactoryInterface _contentRetrieverFactory;
        private readonly CommandBusInterface _commandBus;
        private FacebookGraph _graphApi;
        public FacebookFlierImporter(FlierQueryServiceInterface queryService, UrlContentRetrieverFactoryInterface contentRetrieverFactory, CommandBusInterface commandBus)
        {
            _queryService = queryService;
            _contentRetrieverFactory = contentRetrieverFactory;
            _commandBus = commandBus;
        }

        public bool CanImport(PostaFlya.Domain.Browser.BrowserInterface browser)
        {
            var accessToken = GetAccessTokenFromExternalCredantials(browser);
            if (accessToken == null)
                return false;

            if (!accessToken.Permissions.ToLower().Contains("user_events"))
                return false;

            if (accessToken.Expires <= DateTime.Now)
                return false;

            return true;
        }

        public IQueryable<PostaFlya.Domain.Flier.FlierInterface> ImportFliers(PostaFlya.Domain.Browser.BrowserInterface browser)
        {
            List<FlierInterface> fliers = new List<FlierInterface>();
            var accessToken = GetAccessTokenFromExternalCredantials(browser);
            if (accessToken == null)
                return fliers.AsQueryable();

            _graphApi = new FacebookGraph(accessToken.Token);
            var newEvents = GetEventsNotImported(browser);

            fliers = newEvents.Select(_ => ConvertFaceBookEventToFlier(_, browser)).ToList();

            return fliers.AsQueryable();
        }

        protected FlierInterface ConvertFaceBookEventToFlier(FaceBookEvent fbEvent, BrowserInterface browser)
        {
            Guid? imageId = SaveImageFromfacebookEvent(fbEvent, browser);

            return new PostaFlya.Domain.Flier.Flier()
            {
                ExternalId = fbEvent.id,
                ExternalSource = IdentityProviders.FACEBOOK,
                Status = FlierStatus.Pending,
                EffectiveDate = fbEvent.start_time,
                BrowserId = browser.Id,
                Location = new PostaFlya.Domain.Location.Location()
                {
                    Description = fbEvent.location,
                    Latitude = fbEvent.venue.latitude,
                    Longitude = fbEvent.venue.longitude,
                },
                Title = fbEvent.name,
                Description = fbEvent.description,
                Image = imageId,
                Id = ""
            };
        }

        protected Guid? SaveImageFromfacebookEvent(FaceBookEvent fbEvent, BrowserInterface browser)
        {
            var retriever = _contentRetrieverFactory.GetRetriever(PostaFlya.Domain.Content.Content.ContentType.Image);
            var content = retriever.GetContent(fbEvent.picture);
            if (content == null || content.Data == null || content.Data.Length == 0)
            {
                return null;
            }

            var imgId = Guid.NewGuid();
            var res = _commandBus.Send(new CreateImageCommand()
            {
                CommandId = imgId.ToString(),
                Content = content,
                BrowserId = browser.Id,
                Title = fbEvent.name
            });

            return imgId;
        }


        protected List<FaceBookEvent> GetEventsNotImported(BrowserInterface browser)
        {
            var faceBookEvents = _graphApi.UserEventsGet();

            var existingFliers = _queryService.GetByBrowserId(browser.Id);
            var existingEventsIds = existingFliers.Where(_ => _.ExternalSource == IdentityProviders.FACEBOOK).Select(_ => _.ExternalId).ToList();

            return faceBookEvents.Where(_ => !existingEventsIds.Contains(_.id)).ToList();
        }


        protected AccessToken GetAccessTokenFromExternalCredantials(BrowserInterface browser)
        {
            var facebookCredentials = browser.ExternalCredentials.FirstOrDefault(_ => _.IdentityProvider == IdentityProviders.FACEBOOK);

            if (facebookCredentials == null)
                return null;

            return facebookCredentials.AccessToken;
        }
    }
}
