using System;
using System.Text.RegularExpressions;

namespace RPA.Core
{
    public class WorkerConfiguration : Entity
    {
        public Guid WorkerId { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }        
        public string JSONOptions { get; set; }
    }
}
