using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RPA.Worker.Core;
using Microsoft.AspNetCore.SignalR.Client;
using RPA.Core;
using AutoUpdaterDotNET;
using Newtonsoft.Json;

namespace RPA.Worker.Framework
{
    public partial class GeneralControlPanel<T> : Form where T : WorkerOption
    {
        private readonly RPAWorkerCore<T> worker;

        public GeneralControlPanel(RPAWorkerCore<T> worker)
        {
            InitializeComponent();
            var icon = Icon.ExtractAssociatedIcon(System.IO.Path.Combine(AppContext.BaseDirectory, "robot.ico"));
            this.Icon = icon;
            this.worker = worker;
            lbWorkerName.Text = worker.ServerOptions.WorkerName;
            this.Text = worker.ServerOptions.WorkerName + " " + this.Text;
            notifyIcon1.Icon = icon;
            notifyIcon1.Text = worker.ServerOptions.WorkerName;
            notifyIcon1.MouseDoubleClick += notifyIcon_MouseDoubleClick;
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            //if the form is minimized  
            //hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private Task Worker_HubLogAdded(string arg)
        {
            Action writeLog = delegate { listboxHubLog.Items.Insert(0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + ": " + arg); };
            this.Invoke(writeLog);
            return Task.CompletedTask;
        }

        private Task Worker_WorkerStateChanged(WorkerState arg)
        {
            Action updateState = delegate
            {
                lbWorkerState.Text = arg.ToString();
            };
            this.Invoke(updateState);
            return Task.CompletedTask;
        }

        private Task Worker_HubStateChanged(HubConnectionState arg)
        {
            Action updateState = delegate { lbHubState.Text = arg.ToString(); };
            this.Invoke(updateState);
            return Task.CompletedTask;
        }

        private void ControlPanel_Load(object sender, EventArgs e)
        {
            AutoUpdater.CheckForUpdateEvent += CheckForUpdateEventHandler;
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
            AutoUpdater.RunUpdateAsAdmin = false;
            _ = Task.Run(async () =>
            {
                do
                {
                    AutoUpdate();
                    await Task.Delay(60 * 60 * 24 * 1000);
                }
                while (true);
            });
            
            worker.HubStateChanged += Worker_HubStateChanged;
            worker.WorkerStateChanged += Worker_WorkerStateChanged;
            worker.HubLogAdded += Worker_HubLogAdded;
            worker.AutoUpdateHandler += AutoUpdate;
            _ = worker.Start();
            this.WindowState = FormWindowState.Minimized;
        }

        #region AutoUpdate
        private void AutoUpdate()
        {
            AutoUpdater.Start($"{worker.ServerOptions.WorkerInfoUrl}");
        }
        void CheckForUpdateEventHandler(UpdateInfoEventArgs args)
        {
            var v1 = new Version(args.CurrentVersion);
            var v2 = new Version(worker.ServerOptions.WorkerVersion);
            var result = v1.CompareTo(v2);
            if (result > 0)
            {
                AutoUpdater.DownloadUpdate(args);
                Application.Exit();
            }
        }
        private void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            var workerInfor = JsonConvert.DeserializeObject<WorkerInfor>(args.RemoteData);
            args.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = workerInfor.Version,
                DownloadURL = $"{worker.ServerOptions.ServerUrl}Worker/{workerInfor.FileName}",
            };
        }
        #endregion

        private void QueueWorkerPanel_Shown(object sender, EventArgs e)
        {
            //to minimize window
            this.WindowState = FormWindowState.Minimized;

            //to hide from taskbar
            this.Hide();
        }
    }
}
