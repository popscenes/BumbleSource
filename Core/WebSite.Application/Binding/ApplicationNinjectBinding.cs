using System.Diagnostics;
using Ninject;
using Ninject.Modules;
using Website.Application.Content;
using Website.Application.Email;
using Website.Application.Messaging;
using Website.Application.Publish;
using Website.Application.Schedule;
using Website.Application.WebsiteInformation;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Publish;

namespace Website.Application.Binding
{
    public class ApplicationNinjectBinding : NinjectModule
    {
        #region Overrides of NinjectModule

        public override void Load()
        {
            Trace.TraceInformation("Binding ApplicationNinjectBinding");

            Kernel.Bind<WebsiteInfoServiceInterface>().To<CachedWebsiteInfoService>();

            Bind<BroadcastServiceInterface>()
                .To<DefaultBroadcastService>();

            Bind<QrCodeServiceInterface>()
                .To<ZXingQrCodeService>()
                .InSingletonScope();

            Bind<TimeServiceInterface>()
                .To<DefaultTimeService>()
                .InSingletonScope();

            Bind<SchedulerInterface>().ToMethod(context =>
                {
                    var ret = context.Kernel.Get<Scheduler>();
                    return ret;
                }).InSingletonScope();

            Bind<SendEmailServiceInterface>().To<QueuedSendEmailService>().InTransientScope();
            Bind<SendMailImplementationInterface>().To<SendGridSendMailImplementation>().InTransientScope();

            Bind<EventPublishServiceInterface>().To<EventPublishService>();

            Trace.TraceInformation("Finished Binding ApplicationNinjectBinding");

        }

        #endregion
    }

}
