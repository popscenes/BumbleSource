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
                var runner = new TaskRunner(kernel, PopscenesNinjectBindings.NinjectModules);
                runner.LoadModulesAndTasks(System.Environment.CurrentDirectory);

                runner.RunTask(args);                
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

        }
    }
}
