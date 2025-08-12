using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RPAPIOC.Lib
{
    public interface IPDFReader
    {
        PIData ReadMeta(string pdfText);
    }
}
