using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Configuration;
using RPA.Tools;
using RPA.Core;
using System.IO.Compression;
using Newtonsoft.Json;

namespace AutoDeployWorker
{
    public partial class Main : Form
    {
        private string _TempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            lbVs2019Path.Text = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe";
            lbNSF.Text = @"\\172.19.18.68\Departments2\QASRPA\QASRPA\9999.Workers";
            lbWebServerUrl.Text = "http://qasrpaauto/";
        }

        void AddLog(string msg)
        {
            Action writeLog = delegate { lbLog.Items.Insert(0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + ": " + msg); };
            this.Invoke(writeLog);
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            lbLog.Items.Clear();
            btnRun.Enabled = false;
            if (Directory.Exists(_TempFolder))
            {
                // Clear old data
                Directory.Delete(_TempFolder, true);
                // Create New
                Directory.CreateDirectory(_TempFolder);
            }
            Task.Run(async () =>
           {
               try
               {
                   var startDirectory = AppDomain.CurrentDomain.BaseDirectory;
                   var found = false;
                   var slnPath = string.Empty;
                   while (!found)
                   {
                       var fold = System.IO.Directory.GetParent(startDirectory);
                       if (fold != null)
                       {
                           if (fold.GetFiles("RPA.sln").Length > 0)
                           {
                               slnPath = fold.FullName;
                               found = true;
                               AddLog($"Solution Path: {slnPath}");
                           }
                           else
                           {
                               startDirectory = fold.FullName;
                           }
                       }
                       else
                       {
                           AddLog("Could not find solution file");
                           break;
                       }

                   }
                   if (found && await InputCheck())
                   {
                       await DeployWorkerToNSF(slnPath);
                   }
               }
               catch (Exception ex)
               {
                   AddLog("Error occured !!!!!!!!!!!!!!!");
                   AddLog(ex.ToString());
               }
               finally
               {
                   Action enableBtnRun = delegate { btnRun.Enabled = true; };
                   this.Invoke(enableBtnRun);
               }
           });

        }

        private async Task<bool> InputCheck()
        {
            if (!File.Exists(lbVs2019Path.Text))
            {
                AddLog("VS 2019 not found");
                return false;
            }
            if (!(await WebServerAlive()))
            {
                AddLog("Web Server is not online");
                return false;
            }
            //try
            //{
            //    if (!Directory.Exists(lbNSF.Text))
            //    {
            //        Directory.CreateDirectory(lbNSF.Text);
            //        return true;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    AddLog(ex.Message);
            //    AddLog("Could not access NSF Folder");
            //    return false;
            //}
            return true;
        }

        private async Task<bool> WebServerAlive()
        {
            HttpClient client = new HttpClient();
            try
            {
                var checkingResponse = await client.GetAsync(lbWebServerUrl.Text);
                if (!checkingResponse.IsSuccessStatusCode)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                AddLog(ex.Message);
                return false;
            }

            return true;
        }

        private async Task DeployWorkerToNSF(string slnFolder)
        {
            var slnPath = Path.Combine(slnFolder, "RPA.sln");
            if (BuildSolution(slnPath))
           {
                await PublishFile(slnFolder);
                AddLog("Deploy Successfully");
            }
        }

        private async Task PublishFile(string slnFolder)
        {
            var workerFolder = Path.Combine(slnFolder, "Worker", "Workers");
            var workerDir = new DirectoryInfo(workerFolder);
            foreach (var dir in workerDir.GetDirectories())
            {
                AddLog($"========================================================");
                AddLog($"Process folder {dir.Name}");
                var sourceFolder = Path.Combine(dir.FullName, "bin", "Release");
                var baseExePath = Path.Combine(sourceFolder, $"{dir.Name}.exe");
                if (!File.Exists(baseExePath))
                {
                    continue;
                }
                var version = FileVersionInfo.GetVersionInfo(baseExePath);
                if (IsNewVersion(dir.Name, version.ProductVersion))
                {
                    var tempFolder = Path.Combine(_TempFolder, dir.Name);
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                    }
                    // Copy file 
                    CopyFilesRecursively(sourceFolder, tempFolder);
                    // Update config
                    UpdateConfigFile(Path.Combine(tempFolder, $"{dir.Name}.exe"));
                    var targetZipFileName = $"{dir.Name}_{version.ProductVersion}.zip";
                    var targetZipFilePath = Path.Combine(tempFolder
                                    , Path.Combine(Directory.GetParent(tempFolder).FullName, targetZipFileName));
                    // Zip file
                    ZipFile.CreateFromDirectory(tempFolder, targetZipFilePath);
                    // Delete Temp Folder 
                    //Directory.Delete(tempFolder, true);

                    //// Moving ZipFile
                    //var NSFZIPFilePath = Path.Combine(lbNSF.Text, targetZipFileName);
                    //File.Copy(targetZipFilePath, NSFZIPFilePath, true);

                    var newInfor = new WorkerInfor()
                    {
                        Name = dir.Name,
                        //DownloadPath = NSFZIPFilePath,
                        Version = version.ProductVersion,
                    };
                    await AddOrUpdateWorkerInfor(newInfor,new FileInfo(targetZipFilePath));
                    AddLog($"Process folder {dir.Name} Successfully");
                }
                else
                {
                    AddLog($"No new version detected. Skip {dir.Name}");
                }

            }
        }

        private async Task AddOrUpdateWorkerInfor(WorkerInfor newInfor, FileInfo file)
        {
            var content = HttpClientHelper.CreateFormAndFile(new List<FileInfo>() { file },
                new { info = newInfor });
            var res = await ServerRequest.Post(lbWebServerUrl.Text, "api/worker/addorupdate", content, string.Empty);
            if (res)
            {
                AddLog($"Add or update {newInfor.Name} info to server Successfully");
            }
            else
            {
                AddLog($"Add or update {newInfor.Name} info to server Failure");
            }
        }

        private void UpdateConfigFile(string exeFilePath)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(exeFilePath);
            var serverUrlSetting = config.AppSettings.Settings["ServerUrl"];
            if (serverUrlSetting != null)
            {
                serverUrlSetting.Value = lbWebServerUrl.Text;
            }
            else
            {
                config.AppSettings.Settings.Add("ServerUrl", lbWebServerUrl.Text);
            }
            config.Save(ConfigurationSaveMode.Modified);
        }
        private bool IsNewVersion(string name, string version)
        {
            AddLog("Version:" + version);
            var workerInfo = ServerRequest.Get<WorkerInfor>(lbWebServerUrl.Text, $"api/worker/{name}", "").GetAwaiter().GetResult();
            if (workerInfo != null)
            {
                AddLog("Server Version:" + workerInfo.Version);
                var v1 = new Version(workerInfo.Version);
                var v2 = new Version(version);
                var result = v1.CompareTo(v2);
                return result < 0;
            }
            return true;
        }

        private bool BuildSolution(string slnPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(lbVs2019Path.Text);
            startInfo.Arguments = $"{slnPath} -Build Release";
            startInfo.UseShellExecute = true;
            Process process = new Process();
            process.StartInfo = startInfo;

            process.Start();
            process.WaitForExit();
            bool isCompilationSuccesful = process.ExitCode == 0;
            if (!isCompilationSuccesful)
            {
                AddLog("Fail to Rebuild Solution Stop !!!!!");
            }
            else
            {
                AddLog("Build Solution Successfully");
            }

            //if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log")))
            //{
            //    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log"));
            //}
            //File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")), stringOutput);

            return isCompilationSuccesful;
        }
        private void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }
    }
}
