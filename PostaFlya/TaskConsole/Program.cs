using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoNamesImporter;
using Ninject;
using PostaFlya.App_Start;
using PostaFlya.Binding;
using Website.Infrastructure.Task;

namespace TaskConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var kernel = new StandardKernel())
            {
                kernel.Load(PopscenesNinjectBindings.NinjectModules);

                var runner = new TaskRunner(kernel);
                runner.LoadModulesAndTasks(System.Environment.CurrentDirectory);
                runner.RunTask(args);                
            }

        }
    }
}
