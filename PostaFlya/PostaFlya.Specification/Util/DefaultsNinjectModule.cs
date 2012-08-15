using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using WebSite.Application.Content;
using PostaFlya.Areas.TaskJob.Models;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Service;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Command;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using PostaFlya.Mocks.Domain.Data;
using System;
using PostaFlya.Models.Content;

namespace PostaFlya.Specification.Util
{
    public class DefaultsNinjectModule : NinjectModule
    {
        public override void Load()
        {
            var fc = new FormCollection
                         {
                             {"wa", "wsignin1.0"},
                             {
                                 "wresult",
                                 "<t:RequestSecurityTokenResponse Context=\"http://localhost:64000/\" xmlns:t=\"http://schemas.xmlsoap.org/ws/2005/02/trust\"><t:Lifetime><wsu:Created xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">2012-03-12T08:26:50.556Z</wsu:Created><wsu:Expires xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">2012-03-12T09:26:50.556Z</wsu:Expires></t:Lifetime><wsp:AppliesTo xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\"><EndpointReference xmlns=\"http://www.w3.org/2005/08/addressing\"><Address>http://localhost:64000/</Address></EndpointReference></wsp:AppliesTo><t:RequestedSecurityToken><Assertion ID=\"_e1eadb76-ffe5-43e7-a0c4-948364352cf6\" IssueInstant=\"2012-03-12T08:26:53.790Z\" Version=\"2.0\" xmlns=\"urn:oasis:names:tc:SAML:2.0:assertion\"><Issuer>https://bumbltest.accesscontrol.windows.net/</Issuer><ds:Signature xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"><ds:SignedInfo><ds:CanonicalizationMethod Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\" /><ds:SignatureMethod Algorithm=\"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256\" /><ds:Reference URI=\"#_e1eadb76-ffe5-43e7-a0c4-948364352cf6\"><ds:Transforms><ds:Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" /><ds:Transform Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\" /></ds:Transforms><ds:DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\" /><ds:DigestValue>yHTY/bw+XAMzHW+qjf8jEp3g5wohbkLJy2NJed45P68=</ds:DigestValue></ds:Reference></ds:SignedInfo><ds:SignatureValue>vvr84BZQX1jfFtJ1vkRcWdksZOTuUVfN+WodhUuTkyXLBfgLbWQSJCcwQbBtEpRovd1QJNqp329Wh+I1m+B0LaLmzu6RKBDuqzT6XnzwMJRpakJFGaRFcZ3CSm/iBPWcNuKU8fTu8gDXvVRVapZ/62ze317IuHChb0r2Cy9JGW7AJxqSUQ78VvheS+x5w2nIn6KoWH8FosWtKM9MU1I66EprU+zX3Y4zPrdJIBiRh5nx9E/ucMqrNbi1zJdCszrjtKLM127S+8Sm8w1UkhH++t82sFOi5sDtLb+t+pg3XN4krwLfR6t0h3v8Tc4991vsFOG00za6wkWIhyjQlU9knA==</ds:SignatureValue><KeyInfo xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><X509Data><X509Certificate>MIIDFDCCAfygAwIBAgIQFedU8g1GQ6hIkJeHHC6vMzANBgkqhkiG9w0BAQUFADAuMSwwKgYDVQQDEyNidW1ibHRlc3QuYWNjZXNzY29udHJvbC53aW5kb3dzLm5ldDAeFw0xMjAzMDkxMTA1NDVaFw0xMzAzMDkxNzA1NDVaMC4xLDAqBgNVBAMTI2J1bWJsdGVzdC5hY2Nlc3Njb250cm9sLndpbmRvd3MubmV0MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyA5Yffho6eeXSg9oLvLHhQVWp6k3eL1D5ULQiqTiHtdiZ7jgDJ0+xKZeLKApsodJDJrHApV6U+B2ppdQbmxySioMoLT8U6/kJ5P3GvT9Ac79Q1itSLV3yolSVr4BtOy7ijpF37q/fDnxf8n+HHDwGA5PvVW90b4Zd5hBM5OYqdbe3VxdTu/8nS17Xx4iGH1I+Pt/Q1IMNf7Uye8ITDPsmy810qWAqfDWCI0bjKmIG1mSNCVzefrNv3iufFUmA23R+xUQdlVO7efolDJz9WjCnVf7po4oLwWQqw3HpLB+MU9ttVLLpbgIP67vg2/RhxSfHmKX0dze7Tur1VR/yawr4wIDAQABoy4wLDALBgNVHQ8EBAMCBPAwHQYDVR0OBBYEFM2fIOp4EtAlue67FBNiHknxdoNOMA0GCSqGSIb3DQEBBQUAA4IBAQAhk2adAUvlYrFp8zqo8ws1RGVtXN6I0OAy6IG1b2CrpkgESiiMgDnRQQJvIx00VCXkv9NHClFWihkDymkSd5dJwiQov5XeSuSyckQrd67ARD3vrDDabyqcS1S104LgHREpzrgXlopnnPT8aE5vvmPRhKv/6IMOzMEaKuyyu53hKhU5KNFVO7oBDvsG/IxZ5skq/eUGn9MK1rPz9sSQIdeBaXNgwe+XBur+0zoh5MxEJBuND1VDb4TRVnBNVkSYx29P8jarMTj9VZZQ21YcQGKvKPjXg35OL4M/urBewV0yp5kazdTCiDu2lo45gJAccMWZgXJDXsqUi/GHvG3kJUwf</X509Certificate></X509Data></KeyInfo></ds:Signature><Subject><NameID>https://www.google.com/accounts/o8/id?id=AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho</NameID><SubjectConfirmation Method=\"urn:oasis:names:tc:SAML:2.0:cm:bearer\" /></Subject><Conditions NotBefore=\"2012-03-12T08:26:50.556Z\" NotOnOrAfter=\"2012-03-12T09:26:50.556Z\"><AudienceRestriction><Audience>http://localhost:64000/</Audience></AudienceRestriction></Conditions><AttributeStatement><Attribute Name=\"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress\"><AttributeValue>teddymccuddles@gmail.com</AttributeValue></Attribute><Attribute Name=\"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name\"><AttributeValue>Anthony Borg</AttributeValue></Attribute><Attribute Name=\"http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider\"><AttributeValue>Google</AttributeValue></Attribute></AttributeStatement></Assertion></t:RequestedSecurityToken><t:RequestedAttachedReference><SecurityTokenReference d3p1:TokenType=\"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0\" xmlns:d3p1=\"http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd\" xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><KeyIdentifier ValueType=\"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID\">_e1eadb76-ffe5-43e7-a0c4-948364352cf6</KeyIdentifier></SecurityTokenReference></t:RequestedAttachedReference><t:RequestedUnattachedReference><SecurityTokenReference d3p1:TokenType=\"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0\" xmlns:d3p1=\"http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd\" xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><KeyIdentifier ValueType=\"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID\">_e1eadb76-ffe5-43e7-a0c4-948364352cf6</KeyIdentifier></SecurityTokenReference></t:RequestedUnattachedReference><t:TokenType>urn:oasis:names:tc:SAML:2.0:assertion</t:TokenType><t:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</t:RequestType><t:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</t:KeyType></t:RequestSecurityTokenResponse>"
                                 },
                             {"wctx", "http://localhost:64000/"}
                         };

            Bind<FormCollection>().ToConstant(fc).WithMetadata("ACSGoogleFormCollection", true);

            //flier creation
            Bind<FlierCreateModel>()
            .ToConstant(new FlierCreateModel()
                            {
                                Title = "This is a Title",
                                Description = "This is a Description",
                                TagsString = Kernel.Get<Tags>(ib => ib.Get<bool>("default")).ToString(),
                                Location = Kernel.Get<Location>(ib => ib.Get<bool>("default")).ToViewModel(),
                                ImageList = new List<ImageViewModel>() 
                                { 
                                    new ImageViewModel() { ImageId = Guid.NewGuid().ToString() }, 
                                    new ImageViewModel() { ImageId = Guid.NewGuid().ToString() }, 
                                    new ImageViewModel() { ImageId = Guid.NewGuid().ToString() } 
                                }
                            })
            .WithMetadata("fliercreate", true);

            //set a default command bus if it is needed
            Kernel.Bind<CommandBusInterface>().To<DefaultCommandBus>();
        }
    }
}
