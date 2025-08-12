Imports RPA.Core
Imports RPA.Tools
Imports RPA.Worker.Core
Imports RPA.Worker.Framework
Public Class SAPManagerOptions
    Inherits WorkerOption
    Public Property SAPCredential As SAPLogonOptions
    Public Property SAPExeLocation As String
    Public Property SAPSessionsLimitedNumber As String = 6

End Class
