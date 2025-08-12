

Public Class SAPGuiModels
    Public Property Name As String
    Public Property Type As String
    Public Property Document As String
    Public Property Methods As List(Of SAPObjectMethod) = New List(Of SAPObjectMethod)
    Public Property Members As List(Of SAPEnumMember) = New List(Of SAPEnumMember)
    Public Property Properties As List(Of SAPObjectProperty) = New List(Of SAPObjectProperty)
End Class


Public Class SAPObjectMethod
    Public Property MethodName As String = String.Empty
    Public Property Contructor As String = String.Empty
    Public Property InheritFrom As String = String.Empty
    Public Property Description As String = String.Empty
    Public Property ReturnType As String = String.Empty
    Public Property Params As List(Of SAPMethodParam) = New List(Of SAPMethodParam)()
End Class

Public Class SAPMethodParam
    Public Property IsOptional As Boolean = False
    Public Property Name As String = String.Empty
    Public Property Type As String = String.Empty
    Public Property ArgumentPassingType As String = String.Empty
    Public Property Value As String = String.Empty

End Class

Public Class SAPEnumMember
    Public Property Member As String = String.Empty
    Public Property Value As String = String.Empty
    Public Property Description As String = String.Empty
End Class

Public Class SAPObjectProperty
    Public Property PropertyName As String = String.Empty
    Public Property Contructor As String = String.Empty
    Public Property InheritFrom As String = String.Empty
    Public Property Description As String = String.Empty
    Public Property Access As String = String.Empty
    Public Property PropertyType As String = String.Empty
End Class