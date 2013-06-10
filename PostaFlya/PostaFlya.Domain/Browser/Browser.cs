using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Browser
{
    public class Browser : Website.Domain.Browser.Browser, BrowserInterface
    {
        public List<string> AdminBoards { get; set; }
    }

    public interface BrowserInterface : Website.Domain.Browser.BrowserInterface
    {
         List<string> AdminBoards { get; set; }
    }

    public static class BrowserInterfaceExtensions
    {
        public static void CopyFieldsFrom(this BrowserInterface target, BrowserInterface source)
        {
            Website.Domain.Browser.BrowserInterfaceExtensions.CopyFieldsFrom(target, source);
            target.AdminBoards = source.AdminBoards != null ? new List<string>(source.AdminBoards) : null;
        }

    }
}
