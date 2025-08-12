using System.Text.RegularExpressions;

namespace RPA.Core
{
    public class RegexInfor : Entity
    {
        public int CustomerID { get; set; }
        public string Name { get; set; }
        public string Pattern { get; set; }
        public string DateFormat { get; set; }
        public RegexOptions Options { get; set; } = RegexOptions.None;
        public bool SplitContent { get; set; } = false;
        public int ValueIndex { get; set; } = 0;
    }
}
