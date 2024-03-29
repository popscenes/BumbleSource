using System.Web.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Website.Application.WebsiteInformation;
using Website.Common.Obsolete;
using Website.Domain.Tag;

namespace PostaFlya.Controllers
{
    public class TagsApiController : OldWebApiControllerBase   
    {
        private readonly WebsiteInfoServiceInterface _websiteInfoService;

        public TagsApiController(WebsiteInfoServiceInterface websiteInfoService)
        {
            _websiteInfoService = websiteInfoService;
        }

        public Tags Get()
        {
            return new Tags(_websiteInfoService.GetTags(Request.RequestUri.Host));
        }
    }
}