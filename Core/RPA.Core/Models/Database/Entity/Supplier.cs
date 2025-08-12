using System.Text.RegularExpressions;

namespace RPA.Core
{
    public class Supplier : Entity
    {
        public string Name { get; set; }
        public string InvoiceFormRecognizerModel { get; set; }        
        public string PIOCFormRecognizerModel { get; set; }
        public string SAPID { get; set; }
        public string PIC { get; set; }
        public MIR7InvoiceProcessingType ProcessingType { get; set; }
        public MIR7InvoiceProcessingType ProcessingTypeBackup { get; set; } = MIR7InvoiceProcessingType.None;
        public bool Ignore517 { get; set; } = false;
    }
}
