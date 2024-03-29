using System;
using System.Collections.Generic;
using Ninject.Modules;
using Website.Infrastructure.Authentication;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace Website.Mocks.Domain.Defaults
{
    public class GlobalDefaultsNinjectModule : NinjectModule
    {
        public static Location DefaultLocation = new Location(144.979, -37.769);
        public static Tags DefaultTags = new Tags { "music", "food & drink" };
        public static Tags DefaultWebsiteTags = new Tags { "Popscenes"};
        public static Tags DefaultNonExistentTags = new Tags { "what", "tag" };
        public static string DefaultBrowserId = "40D8AC2A-F95C-40A8-9A75-EE87146838A2";
        public static string DefaultImageId = "8F68AE77-0F61-4BFD-92AC-BFCA1CC5B9E2";


        public override void Load()
        {
            //Default Testing Location
            //Latitude: -37.769
            //Longitude: 144.979
            Bind<Location>()
                .ToConstant(DefaultLocation)
                .WithMetadata("default", true);

            Bind<Tags>()
                .ToConstant(DefaultTags)
                .WithMetadata("default", true);

            Bind<Tags>()
                .ToConstant(new Tags(){"music","food","otherstuff"})
                .WithMetadata("defaulttags2", true);

            Bind<Tags>()
                .ToConstant(DefaultNonExistentTags)
                .WithMetadata("someothertags", true);

            Bind<string>()
                .ToConstant(DefaultBrowserId)
                .WithMetadata("defaultbrowserid", true);

            var token = new AccessToken()
                    {
                        Expires = DateTime.Now,
                        Permissions = "post",
                        Token = "123abc"
                    };

            Bind<BrowserInterface>().ToMethod(ctx => 
                new Browser(GlobalDefaultsNinjectModule.DefaultBrowserId)
                {
                    FriendlyId = "rickyaudsley",
                    EmailAddress = "rickyaudlsey@gmail.com",
                    FirstName = "Ricky",
                    Surname = "Audsley",
                    PhoneNumber = "0411111111",
                    Roles = new Roles{Role.Participant.ToString()},
                    ExternalCredentials = new HashSet<BrowserIdentityProviderCredential>()
                                                {
                                                    new BrowserIdentityProviderCredential()
                                                        {
                                                            BrowserId = DefaultBrowserId,
                                                            IdentityProvider = IdentityProviders.GOOGLE, 
                                                            UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1hoBlah",
                                                            AccessToken =  token
                                                        },

                                                        new BrowserIdentityProviderCredential()
                                                        {
                                                            BrowserId = DefaultBrowserId,
                                                            IdentityProvider = IdentityProviders.FACEBOOK, 
                                                            UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1hoBlah",
                                                            AccessToken =  token
                                                        }
                                                }

                })
                .WithMetadata("defaultbrowser", true);

            Bind<BrowserInterface>().ToMethod(ctx =>
                new Browser(GlobalDefaultsNinjectModule.DefaultBrowserId)
                {
                    FriendlyId = "anthonyborg",
                    EmailAddress = "teddymccuddles@gmail.com",
                    Roles = new Roles{ Role.Participant.ToString() },
                    ExternalCredentials = new HashSet<BrowserIdentityProviderCredential>()
                                                {
                                                    new BrowserIdentityProviderCredential()
                                                        {
                                                            BrowserId = DefaultBrowserId,
                                                            IdentityProvider = IdentityProviders.GOOGLE,
                                                            UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho"
                                                        }
                                                }

                })
                .WithMetadata("ststestbrowser", true); 
        }
    }
}