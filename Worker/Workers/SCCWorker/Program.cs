using RPA.Core;
using RPA.Tools;
using RPA.Worker.Framework;
using SCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCCWorker
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var worker = new BaseWorker(new ServerOption()
            {
#if !DEBUG
                ServerUrl = ConfigurationHelper.GetAppSettingsValue("ServerUrl"),
#endif
                WorkerName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                WorkerVersion = Application.ProductVersion
            });
            Application.Run(new ControlPanel<SCCWorkerOption>(worker));
        }
    }
}
