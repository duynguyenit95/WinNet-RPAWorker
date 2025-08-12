using RPA.Core;
using RPA.Worker.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using RPA.Tools;
namespace RPA.Worker.Framework
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var worker = new QueueWorker<WorkerOption>(new ServerOption() { 
               // ServerUrl = ConfigurationHelper.GetAppSettingsValue("ServerUrl")
            });
            Application.Run(new GeneralControlPanel<WorkerOption>(worker));
        }
    }
}
