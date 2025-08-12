using System.Collections.Generic;

namespace RPA.Core
{
    public class SAPLogonOptions
    {
        public string Client { get; set; } 
        public string UserName { get; set; } 
        public string Password { get; set; } 
        public string Language { get; set; }
        public string ConnectionName { get; set; }
        public string TestConnectionName { get; set; }
    }

    public enum SAPMultipleLogonAction
    {
        ContinueAndEndOther,
        ContinueWithoutEndOther,
        Terminate
    }

    public class SAPLogonResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
