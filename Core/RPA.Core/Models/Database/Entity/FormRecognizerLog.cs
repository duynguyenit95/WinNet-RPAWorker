using System.Text.RegularExpressions;

namespace RPA.Core
{
    //[Index(nameof(CustomerID), nameof(ModelID), nameof(FileName), IsUnique = true)]
    public class FormRecognizerLog : Entity
    {
        public int CustomerID { get; set; }
        public int SupplierID { get; set; }
        public string ModelID { get; set; }
        public string FileName { get; set; }
        public string Result { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = true;
    }
}
