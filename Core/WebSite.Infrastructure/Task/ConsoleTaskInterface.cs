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
        private readonly IKernel _kernel;

        public TaskRunner(IKernel kernel)
        {
            _kernel = kernel;
        }

        protected Logger Logger = LogManager.GetCurrentClassLogger();
        protected List<Assembly> AllAssemblies { get; set; }
        protected IEnumerable<Type> Tasks { get; set; }

        public void RunTask(string[] args)
        {
            Logger.Trace("Loading modules for task {0} ....", args[0]);

            var binding = _kernel.GetBindings(typeof(ConsoleTaskInterface))
                  .Single(b => b.Metadata.Get<string>("taskname").Equals(args[0], StringComparison.OrdinalIgnoreCase));

            Logger.Trace("starting task {0} ....", args[0]);

            var type = binding.Metadata.Get<Type>("tasktype");

            using (AppConfig.Use(type))
            {
                var task = _kernel.Get<ConsoleTaskInterface>(
                    metadata => metadata.Get<string>("taskname")
                        .Equals(args[0], StringComparison.OrdinalIgnoreCase));

                task.Execute(args.Skip(1).ToArray());
            }
            
            Logger.Trace("finished task {0} ....", args[0]);

        }

        public void LoadModulesAndTasks(string path)
        {
            if (AllAssemblies != null) return;


            AllAssemblies = Directory.GetFiles(path, "*.dll")
                                     .Select(Assembly.LoadFile).ToList();


            Tasks = AllAssemblies.SelectMany(assembly => assembly.GetTypes())
                                 .Where(arg => arg.GetInterfaces().Any(i => i == typeof(ConsoleTaskInterface)));

            foreach (var task in Tasks)
            {
                _kernel.Bind<ConsoleTaskInterface>()
                      .To(task)
                      .WithMetadata("taskname", task.Name)
                      .WithMetadata("tasktype", task);
            }

        }
    }
}
