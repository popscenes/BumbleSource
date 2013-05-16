using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
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
using Website.Domain.Contact;
using Website.Domain.Location;
using Website.Infrastructure.Command;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Application.Domain.Email.Claims
{
    public class BrowserClaimEmailSubscription : BrowserSubscriptionBase<ClaimEvent>
    {
        private readonly GenericQueryServiceInterface _entityQueryService;
        private readonly SendEmailServiceInterface _emailService;
        private readonly ConfigurationServiceInterface _config;

        public BrowserClaimEmailSubscription(CommandBusInterface commandBus
                                                   , GenericQueryServiceInterface entityQueryService
                                                   , SendEmailServiceInterface emailService
                                                    ,ConfigurationServiceInterface config) : base(commandBus)
        {
            _entityQueryService = entityQueryService;
            _emailService = emailService;
            _config = config;
        }

        public override string SubscriptionName
        {
            get { return "Tear Off Claim Email"; }
        }

        //just returning browser who claims atm, future use may be charging for contact details the other way.
        public override BrowserInterface[] GetBrowsersForPublish(ClaimEvent publish)
        {
            var claim = publish.NewState;
            BrowserInterface browser = _entityQueryService.FindById<Website.Domain.Browser.Browser>(claim.BrowserId);
            

            var browserPublishList = new List<BrowserInterface>();

            if(browser != null)
                browserPublishList.Add(browser);

            if (claim.ClaimContext.Equals("senduserdetails", StringComparison.CurrentCultureIgnoreCase))
            {
                FlierInterface flier = _entityQueryService.FindById<PostaFlya.Domain.Flier.Flier>(claim.AggregateId);
                BrowserInterface ownerBrowser = _entityQueryService.FindById<Website.Domain.Browser.Browser>(flier.BrowserId);
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
            //not sending through contact details for now...considering 
//            var email = new MailMessage();
//
//            if (flier.HasLeadGeneration)
//            {
//                var vcard = GetVCardForBrowser(publish.NewState.BrowserId);
//                if (vcard != null)
//                    email.AddVCardAsAttachment(vcard);
//            }
//
//            email.Subject = "PostaFlya User Contact Details";
//            email.Body = "Posta flya tearoff details " + publish.NewState.ClaimMessage;
//            email.IsBodyHtml = false;
//            email.To.Add(new MailAddress(browser.EmailAddress));
//            _emailService.Send(email);

            return true;
        }

        private bool SendToClaimer(BrowserInterface browser, ClaimEvent publish, PostaFlya.Domain.Flier.Flier flier)
        {
            var email = new MailMessage();

            email.From = new MailAddress("details@popscenes.com");
            email.Subject = "Popscenes details for: " + flier.Title.ToLetterOrDigitAndSpaceOnly();

            var poster = _entityQueryService.FindById<Website.Domain.Browser.Browser>(flier.BrowserId);
            var dets = flier.GetContactDetailsForFlier(poster);

            email.Body = GetBodyFor(flier, dets);

            email.IsBodyHtml = false;
            email.AddSimpleHtmlAlternate(email.Body);
            if (string.IsNullOrWhiteSpace(browser.EmailAddress))
                return false;
            email.To.Add(new MailAddress(browser.EmailAddress));

//            if (flier.HasEventDates())
//            {
//                var calEvent = GetEventForFlier(flier);
//                if (calEvent != null)
//                {
//                    email.AddEvent(calEvent);
//                    email.AddEventAsAttachment(calEvent);
//                }
//                    
//            }

//            if (flier.GetContactDetailsForFlier(browser).HasEnoughForContact())
//            {
//                var vcard = GetVCardForFlier(flier);
//                if (vcard != null)
//                {
//                    email.AddVCardAsAttachment(vcard);
//                }
//                                
//            }

            _emailService.Send(email);

            return true;
        }

        private string GetBodyFor(PostaFlya.Domain.Flier.Flier flier, ContactDetailsInterface posterDetails)
        {
           
            var builder = new StringBuilder();
            builder.Append("Please find below the details for the flyer ");
            builder.Append(flier.Title);
            builder.Append("\n\n");

//            if (posterDetails.HasEnoughForContact())
//                builder.Append("The contact details are also attached as a vcard for import into phone or web contacts \n");
//            
//            if(flier.EffectiveDate > DateTime.UtcNow)
//                builder.Append("The event date and details are also attached as a ical for import into your calendar \n");

           
            builder.Append("\n\n");
            builder.Append("Popscenes Url: " + _config.GetSetting("SiteUrl") + "/" + flier.FriendlyId + "\n\n");

            var contactDets = GetContactDetails(posterDetails);
            if (!string.IsNullOrWhiteSpace(contactDets))
            {
                builder.AppendLine(contactDets);
                builder.Append("\n");
            }


            builder.Append("Event Dates: \n");
            foreach (var @event in flier.EventDates )
            {
                builder.Append(@event.DateTime.ToLongDateString());
            }
                
            builder.Append("\n\n");
            

            builder.Append("Description from flier:\n");
            builder.Append(flier.Description);

            return builder.ToString();

        }

        private string GetContactDetails(ContactDetailsInterface dets)
        {
            var builder = new StringBuilder();
            builder.Append("Contact Details:\n");
            var name = dets.ToNameString();
            if (!string.IsNullOrWhiteSpace(name))
            {
                builder.Append("Name: ");
                builder.Append(name);
                builder.Append("\n");
            }

            if (!string.IsNullOrWhiteSpace(dets.PhoneNumber))
            {
                builder.Append("Phone: ");
                builder.Append(dets.PhoneNumber);
                builder.Append("\n");
            }

            if (!string.IsNullOrWhiteSpace(dets.EmailAddress))
            {
                builder.Append("Email: ");
                builder.Append(dets.EmailAddress);
                builder.Append("\n");
            }

            var address = dets.Address.GetAddressDescription();
            if (!string.IsNullOrWhiteSpace(address))
            {
                builder.Append("Address: ");
                builder.Append(address);
                builder.Append("\n");
            }

            if (!string.IsNullOrWhiteSpace(dets.WebSite))
            {
                builder.Append("WebSite: ");
                builder.Append(dets.WebSite);
                builder.Append("\n");
            }

            return builder.ToString();
        }

        private Event GetEventForFlier(PostaFlya.Domain.Flier.Flier flier)
        {
            var browser = _entityQueryService.FindById<Website.Domain.Browser.Browser>(flier.BrowserId);
            return flier.ToICalEvent(browser);
        }

        private VCard GetVCardForBrowser(string browserId)
        {
            var browser = _entityQueryService.FindById<Website.Domain.Browser.Browser>(browserId);
            return browser.ToVCard();
        }

        private VCard GetVCardForFlier(PostaFlya.Domain.Flier.Flier flier)
        {
            var browser = _entityQueryService.FindById<Website.Domain.Browser.Browser>(flier.BrowserId);
            var dets = flier.GetContactDetailsForFlier(browser);
            return dets == null ? null : dets.ToVCard();
        }
    }
}