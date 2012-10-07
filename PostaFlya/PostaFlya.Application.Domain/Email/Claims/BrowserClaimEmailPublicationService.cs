using System;
using System.Net.Mail;
using PostaFlya.Application.Domain.Email.ICalendar;
using PostaFlya.Domain.Flier;
using Website.Application.Domain.Email.VCard;
using Website.Application.Email;
using Website.Application.Email.ICalendar;
using Website.Application.Email.VCard;
using Website.Domain.Browser;
using Website.Domain.Browser.Publish;
using Website.Domain.Claims;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.Email.Claims
{
    public class BrowserClaimEmailPublicationService : PublishServiceBrowserSubscriptionBase<Claim>
    {
        private readonly GenericQueryServiceInterface _entityQueryService;
        private readonly SendEmailServiceInterface _emailService;

        public BrowserClaimEmailPublicationService(CommandBusInterface commandBus
                                                   , GenericQueryServiceInterface entityQueryService
                                                   , SendEmailServiceInterface emailService  ) : base(commandBus)
        {
            _entityQueryService = entityQueryService;
            _emailService = emailService;
        }

        public override string Name
        {
            get { return "Tear Off Claim Email"; }
        }

        public override string Description
        {
            get { return "Send me an Email when I claim a tear off on a flier"; }
        }

        //just returning browser who claims atm, future use may be charging for contact details the other way.
        public override BrowserInterface[] GetBrowsersForPublish(Claim publish)
        {
            BrowserInterface browser = _entityQueryService.FindById<Browser>(publish.BrowserId);
            return browser == null ? null : new[]{browser};
        }

        public override bool PublishToBrowser(BrowserInterface browser, Claim publish)
        {
            var flier = _entityQueryService.FindById<PostaFlya.Domain.Flier.Flier>(publish.AggregateId);

            var email = new MailMessage();
            
            if(flier.HasContactDetails())
            {
                var vcard = GetVCardForFlier(flier);
                if(vcard != null)
                    email.AddVCardAsAttachment(vcard);
            }

            if(flier.EffectiveDate > DateTime.UtcNow)
            {
                var calEvent = GetEventForFlier(flier);
                if(calEvent != null)
                    email.AddEventAsAttachment(calEvent);
            }

            email.Subject = "PostaFlya Tear Off";
            email.Body = "Posta flya tearoff details";
            email.IsBodyHtml = false;
            email.To.Add(new MailAddress(browser.EmailAddress));
            _emailService.Send(email);
            
            return true;
        }

        private Event GetEventForFlier(PostaFlya.Domain.Flier.Flier flier)
        {
            var browser = _entityQueryService.FindById<Browser>(flier.BrowserId);
            return flier.ToICalEvent(browser);
        }

        private VCard GetVCardForFlier(PostaFlya.Domain.Flier.Flier flier)
        {
            var browser = _entityQueryService.FindById<Browser>(flier.BrowserId);
            var dets = flier.GetContactDetailsForFlier(browser);
            return dets == null ? null : dets.ToVCard();
        }
    }
}