using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostaFlya.Application.Domain.Content
{
    public interface UrlContentRetrieverInterface
    {
        PostaFlya.Domain.Content.Content GetContent(String url);
    }
}
