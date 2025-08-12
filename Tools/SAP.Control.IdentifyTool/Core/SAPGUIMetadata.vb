Imports Newtonsoft.Json
Module SAPGUIMetadata
    Public Property Controls As List(Of SAPGuiModels)
    Sub Load()
        Dim path = System.IO.Path.Combine(My.Application.Info.DirectoryPath, "SAPDocumentParser", "SAPGuiObj.json")
        Dim jsonContent = System.IO.File.ReadAllText(path)
        Controls = JsonConvert.DeserializeObject(Of List(Of SAPGuiModels))(jsonContent)

        Console.ReadLine()
    End Sub

    Function GetControlMetadata(control As Object, controlType As String) As SAPGuiModels
        Dim controlMetadata = SAPGUIMetadata.Controls.FirstOrDefault(Function(x) x.Name = controlType)
        If controlMetadata.Properties.Any(Function(x) x.PropertyName = "SubType") Then
            Dim subTypeValue = control.SubType.ToString()
            If Not subTypeValue.Contains("Gui") Then
                subTypeValue = "Gui" + subTypeValue
            End If
            controlMetadata = SAPGUIMetadata.Controls.First(Function(x) x.Name = subTypeValue)
        End If
        Return controlMetadata
    End Function
    Function IsContainer(ControlName As String)
        Return Controls.Any(Function(x) x.Name = ControlName And x.Methods.Any(Function(y) (Not IsNothing(y.InheritFrom)) And y.InheritFrom.Contains("GuiContainer")))
    End Function

    Function MethodExists(ControlName As String, MethodName As String) As Integer
        Return Controls.Any(Function(x) x.Name = ControlName And x.Methods.Any(Function(y) y.MethodName = MethodName))
    End Function

    Function PropertyExists(ControlName As String, PropertyName As String) As Integer
        Return Controls.Any(Function(x) x.Name = ControlName And x.Properties.Any(Function(y) y.PropertyName = PropertyName))
    End Function
    Function GetRootProperty(ControlName As String, PropertyName As String) As SAPObjectProperty
        Dim control = Controls.FirstOrDefault(Function(x) x.Name = ControlName)
        Dim prop = control.Properties.FirstOrDefault(Function(y) y.PropertyName = PropertyName)
        Return prop
    End Function
    Function GetRootMethod(ControlName As String, MethodName As String) As SAPObjectMethod
        Dim control = Controls.FirstOrDefault(Function(x) x.Name = ControlName)
        Dim prop = control.Methods.FirstOrDefault(Function(y) y.MethodName = MethodName)
        Return prop
    End Function

    Function IsCollection(propType As String) As Boolean
        Return Controls.Any(Function(x) x.Type = "Collection" And x.Name = propType)
    End Function

    Function IsEnumType(enumType As String) As Boolean
        Return Controls.Any(Function(x) x.Type = "Enum" And x.Name = enumType)
    End Function
    Function GetEnumValueMember(enumType As String, value As String) As String
        Dim model = Controls.FirstOrDefault(Function(x) x.Type = "Enum" And x.Name = enumType)
        Return model.Members.FirstOrDefault(Function(x) x.Value = value).Member
    End Function
End Module
