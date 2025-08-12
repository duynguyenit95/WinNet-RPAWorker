using System.Text.RegularExpressions;

namespace RPA.Core
{
    public class WorkerInfor : Entity
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string DownloadPath { get; set; }
        public string FileName { get => DownloadPath; set { DownloadPath = value; } }
    }
}
