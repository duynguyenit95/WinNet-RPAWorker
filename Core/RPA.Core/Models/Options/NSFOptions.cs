using System.Collections.Generic;

namespace RPA.Core
{
    public class NSFOptions
    {
        public string AuthDomain { get; set; }
        public string AuthUsername { get; set; }
        public string AuthPassword { get; set; }

        public string InputFolderURI { get; set; }
        public string ProcessFolderURI { get; set; } 
        public string SuccessFolderURI { get; set; } 
        public string FailureFolderURI { get; set; }
    }
}
