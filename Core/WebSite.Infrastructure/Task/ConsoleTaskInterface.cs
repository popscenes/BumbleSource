using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Ninject;
using Website.Infrastructure.Configuration;

namespace Website.Infrastructure.Task
{
    public interface ConsoleTaskInterface
    {
        void Execute(string[] args);
    }

    public abstract class ConsoleTask : ConsoleTaskInterface
    {
        public void Execute(string[] args)
        {
            Run(args);
        }

        protected abstract void Run(string[] args);
    }

    public class TaskRunner
    {
        public TaskRunner()
        {
        }

        protected Logger Logger = LogManager.GetCurrentClassLogger();
        protected IKernel Kernel { get; set; }
        protected List<Assembly> AllAssemblies { get; set; }
        protected IEnumerable<Type> Tasks { get; set; }

        public void RunTask(string[] args)
        {
            Logger.Trace("Loading modules for task {0} ....", args[0]);
            ReInitKernel();

            var binding = Kernel.GetBindings(typeof (ConsoleTaskInterface))
                  .Single(b => b.Metadata.Get<string>("taskname").Equals(args[0], StringComparison.OrdinalIgnoreCase));

            var type = binding.Metadata.Get<Type>("tasktype");
            AppConfig.Use(type);
            
            var task = Kernel.Get<ConsoleTaskInterface>(
                metadata => metadata.Get<string>("taskname")
                    .Equals(args[0], StringComparison.OrdinalIgnoreCase));


            Logger.Trace("starting task {0} ....", args[0]);

            task.Execute(args.Skip(1).ToArray());

            Logger.Trace("finished task {0} ....", args[0]);

        }


        private void ReInitKernel()
        {
            if(Kernel != null) Kernel.Dispose();

            Kernel = new StandardKernel();
            
            LoadModulesAndTasks();

            Kernel.Load(AllAssemblies);

            foreach (var task in Tasks)
            {
                Kernel.Bind<ConsoleTaskInterface>()
                      .To(task)
                      .WithMetadata("taskname", task.Name)
                      .WithMetadata("tasktype", task);
            }

        }

        public void LoadModulesAndTasks()
        {
            if (AllAssemblies != null) return;

            string path = Assembly.GetExecutingAssembly().Location;

            AllAssemblies = Directory.GetFiles(path, "*.dll")
                                     .Select(Assembly.LoadFile).ToList();


            Tasks = AllAssemblies.SelectMany(assembly => assembly.GetTypes())
                                 .Where(arg => arg.GetInterfaces().Any(i => i == typeof(ConsoleTaskInterface)));


        }
    }
}
