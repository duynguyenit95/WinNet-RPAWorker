using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.FormRecognizer
{
    public class FormRecognizerAPIResult
    {
        public List<string> Content { get; set; } = new List<string>();
        public bool IsSuccess { get; set; } = false;
    }
}
