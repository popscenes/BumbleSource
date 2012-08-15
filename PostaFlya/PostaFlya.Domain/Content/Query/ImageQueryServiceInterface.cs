using System;
using System.Collections;
using System.Linq;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Domain.Content.Query
{
    public interface ImageQueryServiceInterface : GenericQueryServiceInterface<ImageInterface>
        , QueryByBrowserInterface<ImageInterface>
    {
    }
}
