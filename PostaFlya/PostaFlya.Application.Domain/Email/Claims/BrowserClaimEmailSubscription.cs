using System;
using System.Collections.Generic;
using System.Linq;
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
using Website.Domain.Claims.Event;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.Email.Claims
{
    public class BrowserClaimEmailSubscription : BrowserSubscriptionBase<ClaimEvent>
    {
        private readonly GenericQueryServiceInterface _entityQueryService;
        private readonly SendEmailServiceInterface _emailService;

        public BrowserClaimEmailSubscription(CommandBusInterface commandBus
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
        public override BrowserInterface[] GetBrowsersForPublish(ClaimEvent publish)
        {
            var claim = publish.NewState;
            BrowserInterface browser = _entityQueryService.FindById<Browser>(claim.BrowserId);
            

            var browserPublishList = new List<BrowserInterface>();

            if(browser != null)
                browserPublishList.Add(browser);

            if (claim.ClaimContext.Equals("senduserdetails", StringComparison.CurrentCultureIgnoreCase))
            {
                FlierInterface flier = _entityQueryService.FindById<PostaFlya.Domain.Flier.Flier>(claim.AggregateId);
                BrowserInterface ownerBrowser = _entityQueryService.FindById<Browser>(flier.BrowserId);
                if (ownerBrowser != null)
                    browserPublishList.Add(ownerBrowser);
            }

            return browserPublishList.Any() ? browserPublishList.ToArray() : null;
        }

        public override bool PublishToBrowser(BrowserInterface browser, ClaimEvent publish)
        {
            var flier = _entityQueryService.FindById<PostaFlya.Domain.Flier.Flier>(publish.NewState.AggregateId);

            if (browser.Id.Equals(publish.NewState.BrowserId))
            {
                return SendToClaimer(browser, publish, flier);
            }
            if (browser.Id.Equals(flier.BrowserId))
            {
                SendToOwner(browser, publish, flier);
            }

            return false;


        }

        private bool SendToOwner(BrowserInterface browser, ClaimEvent publish, PostaFlya.Domain.Flier.Flier flier)
        {
            var email = new MailMessage();

            if (flier.HasLeadGeneration)
            {
                var vcard = GetVCardForBrowser(publish.NewState.BrowserId);
                if (vcard != null)
                    email.AddVCardAsAttachment(vcard);
            }

            email.Subject = "PostaFlya User Contact Details";
            email.Body = "Posta flya tearoff details " + publish.NewState.ClaimMessage;
            email.IsBodyHtml = false;
            email.To.Add(new MailAddress(browser.EmailAddress));
            _emailService.Send(email);

            return true;
        }

        private bool SendToClaimer(BrowserInterface browser, ClaimEvent publish, PostaFlya.Domain.Flier.Flier flier)
        {
            var email = new MailMessage();

            if (flier.HasContactDetails())
            {
                var vcard = GetVCardForFlier(flier);
                if (vcard != null)
                    email.AddVCardAsAttachment(vcard);
            }

            if (flier.EffectiveDate > DateTime.UtcNow)
            {
                var calEvent = GetEventForFlier(flier);
                if (calEvent != null)
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

        private VCard GetVCardForBrowser(string browserId)
        {
            var browser = _entityQueryService.FindById<Browser>(browserId);
            return browser.ToVCard();
        }

        private VCard GetVCardForFlier(PostaFlya.Domain.Flier.Flier flier)
        {
            var browser = _entityQueryService.FindById<Browser>(flier.BrowserId);
            var dets = flier.GetContactDetailsForFlier(browser);
            return dets == null ? null : dets.ToVCard();
        }
    }
}