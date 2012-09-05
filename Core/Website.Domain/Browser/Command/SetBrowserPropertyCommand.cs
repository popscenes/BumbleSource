using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    public class SetBrowserPropertyCommand : DefaultCommandBase
    {
        public BrowserInterface Browser { get; set; }
        public object PropertyValue { get; set; }
        public string PropertyName { get; set; }
    }
}