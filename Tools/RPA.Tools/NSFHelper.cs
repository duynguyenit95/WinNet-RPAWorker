using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using SharpCifs.Smb;
using System.IO;
using RPA.Core;
using System.Threading.Tasks;

namespace RPA.Tools
{
    public class NSFHelper
    {
        public readonly NSFOptions nsfOptions;

        public string LocalProcessFolder { get; set; }
        public string LocalSuccessFolder { get; set; }
        public string LocalFailureFolder { get; set; }

        public event Func<string, Task> LogEvent;

        private NtlmPasswordAuthentication _Auth;
        public NSFHelper(NSFOptions nsfOptions,string LocalProcessFolder,string LocalSuccessFolder,string LocalFailureFolder)
        {
            this.nsfOptions = nsfOptions;
            this.LocalProcessFolder = LocalProcessFolder;
            this.LocalSuccessFolder = LocalSuccessFolder;
            this.LocalFailureFolder = LocalFailureFolder;
            _Auth = new NtlmPasswordAuthentication(nsfOptions.AuthDomain, nsfOptions.AuthUsername, nsfOptions.AuthPassword);
        }

        public Task PreExecute()
        {
            Log("Reading Input from NSF To Local");
            SmbExtensions.CopyNSFFolderToLocal(_Auth, nsfOptions.InputFolderURI, LocalProcessFolder);
            Log("Moving Input to Process Folder");
            SmbExtensions.CopyNSFFolderToNSF(_Auth, nsfOptions.InputFolderURI, nsfOptions.ProcessFolderURI);
            Log("Clean Input");
            SmbExtensions.DeleteAllFilesInNSF(_Auth, nsfOptions.InputFolderURI);
            return Task.CompletedTask;
        }

        public Task AfterExecute()
        {
            Log("Move Success");
            SmbExtensions.CopyLocalFolderToNSF(_Auth, LocalSuccessFolder, nsfOptions.SuccessFolderURI);
            Log("Move Failure");
            SmbExtensions.CopyLocalFolderToNSF(_Auth, LocalFailureFolder, nsfOptions.FailureFolderURI);
            Log("Clean");
            SmbExtensions.DeleteAllFilesLocal(LocalSuccessFolder);
            SmbExtensions.DeleteAllFilesLocal(LocalFailureFolder);
            SmbExtensions.DeleteAllFilesLocal(LocalProcessFolder);
            SmbExtensions.DeleteAllFilesInNSF(_Auth, nsfOptions.ProcessFolderURI);
            return Task.CompletedTask;
        }

        void Log(string msg)
        {
            LogEvent.Invoke(msg);
        }
    }

}