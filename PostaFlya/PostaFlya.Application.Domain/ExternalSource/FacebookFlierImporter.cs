using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Venue;
using Website.Application.Domain.Browser.Query;
using Website.Application.Intergrations;
using Website.Domain.Browser.Query;
using Website.Domain.Contact;
using Website.Domain.Content;
using Website.Domain.Location;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using Website.Application.Domain.Content;
using Website.Domain.Browser;
using Website.Domain.Content.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.ExternalSource
{
    public class FacebookFlierImporter: FlierImporterInterface
    {
        private readonly QueryChannelInterface _queryChannel;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly UrlContentRetrieverFactoryInterface _contentRetrieverFactory;
        private readonly CommandBusInterface _commandBus;
        private FacebookGraph _graphApi;
        public FacebookFlierImporter(QueryChannelInterface queryChannel,GenericQueryServiceInterface queryService, 
            UrlContentRetrieverFactoryInterface contentRetrieverFactory, 
            CommandBusInterface commandBus)
        {
            _queryChannel = queryChannel;
            _queryService = queryService;
            _contentRetrieverFactory = contentRetrieverFactory;
            _commandBus = commandBus;
        }

        public bool CanImport(Website.Domain.Browser.BrowserInterface browser)
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

        public IQueryable<PostaFlya.Domain.Flier.FlierInterface> ImportFliers(Website.Domain.Browser.BrowserInterface browser)
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


            var contactDetails = new VenueInformation()
                {
                    Address = new Location()
                        {
                            Latitude = fbEvent.venue.location == null ? 0.0: fbEvent.venue.location.latitude, 
                            Longitude =fbEvent.venue.location == null ?0.0: fbEvent.venue.location.longitude,
                            PostCode = fbEvent.venue.location == null ? "":fbEvent.venue.location.zip,
                            Street = fbEvent.venue.location == null ?"":fbEvent.venue.location.street

                        },
                    PlaceName = fbEvent.venue.name,

                };

            return new PostaFlya.Domain.Flier.Flier()
            {
                ExternalId = fbEvent.id,
                ExternalSource = IdentityProviders.FACEBOOK,
                Status = FlierStatus.Pending,
                EffectiveDate = fbEvent.start_time,
                BrowserId = browser.Id,
                Title = fbEvent.name,
                Description = fbEvent.description,
                Image = imageId,
                Id = "", 
                Venue =  contactDetails
            };
        }

        protected Guid? SaveImageFromfacebookEvent(FaceBookEvent fbEvent, BrowserInterface browser)
        {
            var retriever = _contentRetrieverFactory.GetRetriever(Website.Domain.Content.Content.ContentType.Image);
            var content = retriever.GetContent(fbEvent.picture);
            if (content == null || content.Data == null || content.Data.Length == 0)
            {
                return null;
            }

            String externalId = "facebook" + fbEvent.id;
            var imagesList = _queryChannel.Query(new GetByBrowserIdQuery() {BrowserId = browser.Id}, new List<Image>());
            var externalImage = imagesList.Where(_ => _.ExternalId == externalId);

            var imgId = Guid.NewGuid().ToString();

            if (externalImage.Any())
            {
                imgId = externalImage.First().Id;
            }

            var res = _commandBus.Send(new CreateImageCommand()
            {
                CommandId = imgId,
                Content = content,
                BrowserId = browser.Id,
                Title = fbEvent.name,
                ExternalId = externalId
            });

            return new Guid(imgId);
        }


        protected List<FaceBookEvent> GetEventsNotImported(BrowserInterface browser)
        {
            var faceBookEvents = _graphApi.UserEventsGet();

            var existingFliers = _queryChannel.Query(new GetByBrowserIdQuery() {BrowserId = browser.Id}, new List<PostaFlya.Domain.Flier.Flier>());
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
