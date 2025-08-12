Public Class Main
    Dim SapGuiAuto
    Dim Application
    ''' <summary>
    ''' Error Mark
    ''' </summary>
    Dim IsError = False
    ''' <summary>
    ''' Standard - Primitive Type
    ''' </summary>
    Dim StandardType As List(Of String) = New List(Of String) From {"Long", "String", "Byte", "Integer", "Boolean"}
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SAPGUIMetadata.Load()
        txtInpSetValue.Enabled = False
        btnSetValue.Enabled = False
        txtSelectedNode.ResetText()
        lbSelectedNodeType.ResetText()
        lbSelectedProperty.ResetText()
    End Sub
    ''' <summary>
    ''' Load SAP Scripting and control list
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click, btnExpand.Click
        treeObject.Nodes.Clear()
        IsError = False
        Try
            SapGuiAuto = GetObject("SAPGUI")
            Application = SapGuiAuto.GetScriptingEngine
            Dim newNode = CreateControlNode(Application)
            treeObject.Nodes.Add(newNode)
            ParseChild(Application, newNode)
        Catch ex As Exception
            IsError = True
            MessageBox.Show(ex.ToString())
        End Try
    End Sub
    ''' <summary>
    ''' Generate a node name for SAP GuiControl object
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    Function CreateControlNode(obj As Object) As TreeNode
        Dim newNode = New TreeNode() With {
            .Name = obj.Id,
            .Text = GenerateNodeTitle(obj),
            .ToolTipText = obj.Id,
            .Tag = obj
        }
        Return newNode
    End Function
    ''' <summary>
    ''' Loading child controls in container object
    ''' </summary>
    ''' <param name="control"></param>
    ''' <param name="node"></param>
    Sub ParseChild(control As Object, node As TreeNode)
        If SAPGUIMetadata.IsContainer(control.Type) Then
            Dim children = control.Children
            For Each child In children
                Dim childNode = CreateControlNode(child)
                node.Nodes.Add(childNode)
                ParseChild(child, childNode)
            Next
        End If
    End Sub
    ''' <summary>
    ''' Visualize Gui Control after a node is selected
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub treeObject_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles treeObject.AfterSelect
        If IsError Then
            Return
        End If
        Dim nodeName = e.Node.Name
        Dim control = FindElementByID(nodeName)
        VisualizeControl(control, True)
        My.Computer.Clipboard.SetText(nodeName)
    End Sub

    ''' <summary>
    ''' Remove Visualize before new node is selected
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub treeObject_BeforeSelect(sender As Object, e As TreeViewCancelEventArgs) Handles treeObject.BeforeSelect
        If IsError Then
            Return
        End If
        Dim selectedNode = treeObject.SelectedNode
        If Not IsNothing(selectedNode) Then
            Dim nodeName = selectedNode.Name
            Dim control = FindElementByID(nodeName)
            VisualizeControl(control, False)
        End If

    End Sub
    ''' <summary>
    ''' Generate control node title
    ''' </summary>
    ''' <param name="control"></param>
    ''' <returns></returns>
    Function GenerateNodeTitle(control) As String
        Dim displayText = control.Type

        If SAPGUIMetadata.PropertyExists(control.Type, "Name") Then
            displayText += " - " + control.Name
        End If

        If SAPGUIMetadata.PropertyExists(control.Type, "Text") Then
            displayText += " - " + control.Text
        End If

        If SAPGUIMetadata.PropertyExists(control.Type, "SubType") Then
            displayText += " - " + control.SubType
        End If

        Return displayText
    End Function
    ''' <summary>
    ''' Expand tree object
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnExpand_Click(sender As Object, e As EventArgs) Handles btnExpand.Click
        treeObject.ExpandAll()
    End Sub
    ''' <summary>
    ''' Collapse tree object
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnCollapse_Click(sender As Object, e As EventArgs) Handles btnCollapse.Click
        treeObject.CollapseAll()
    End Sub
    ''' <summary>
    ''' Try get sap control by Id
    ''' return nothing if not found
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Function FindElementByID(id)
        Try
            Return Application.FindById(id)
        Catch ex As Exception
            IsError = True
            MessageBox.Show("Control is not found. Please reload if you do any action that make change to SAP Gui !")
            Return Nothing
        End Try
    End Function
    ''' <summary>
    ''' Load Object Details
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnLoadObject_Click(sender As Object, e As EventArgs) Handles btnLoadObject.Click
        treeObjectDetails.Nodes.Clear()
        Dim selectedNode = treeObject.SelectedNode
        If Not IsNothing(selectedNode) Then
            Dim nodeName = selectedNode.Name
            Dim control = FindElementByID(nodeName)
            If Not IsNothing(control) Then
                Dim rootNode = CreateControlNode(control)
                treeObjectDetails.Nodes.Add(rootNode)
                LoadProperties(rootNode, control)
            Else
                MsgBox("Could not Identify Object - Please reload")
            End If

        Else
            MsgBox("Please Select Object")
        End If
    End Sub
    ''' <summary>
    ''' Generate property node name
    ''' </summary>
    ''' <param name="rootNode"></param>
    ''' <param name="prop"></param>
    ''' <returns></returns>
    Function GeneratePropertyNodeName(rootNode As TreeNode, prop As SAPObjectProperty) As String
        Return $"{rootNode.Name}|{prop.PropertyName}-{prop.PropertyType}"
    End Function
    ''' <summary>
    ''' Extract property type from node name
    ''' </summary>
    ''' <param name="nodeName"></param>
    ''' <returns></returns>
    Function ExtractNodePropertyType(nodeName As String) As String
        Return nodeName.Substring(nodeName.LastIndexOf("|") + 1).Split("-").LastOrDefault()
    End Function
    ''' <summary>
    ''' Extract property name from node name
    ''' </summary>
    ''' <param name="nodeName"></param>
    ''' <returns></returns>
    Function ExtractNodePropertyName(nodeName As String) As String
        Return nodeName.Substring(nodeName.LastIndexOf("|") + 1).Split("-").FirstOrDefault()
    End Function

    ''' <summary>
    ''' Create property node under a root node
    ''' </summary>
    ''' <param name="rootNode"></param>
    ''' <param name="prop"></param>
    ''' <param name="control"></param>
    Sub CreatePropertyNode(rootNode As TreeNode, prop As SAPObjectProperty, control As Object)
        Dim newNode = New TreeNode()

        If String.IsNullOrEmpty(prop.Access) Then
            newNode.Text = "(Access Denied) - " + prop.PropertyName
            rootNode.Nodes.Add(newNode)
            Return
        End If

        Try
            newNode.Name = GeneratePropertyNodeName(rootNode, prop)
            Dim propValue = CallByName(control, prop.PropertyName, CallType.Get)
            If IsNothing(propValue) Then
                newNode.Text = prop.PropertyName + " - Nothing"
            ElseIf prop.PropertyType = "Unknown" Then
                newNode.Text = prop.PropertyName + " - Unknown"
            ElseIf StandardType.Contains(prop.PropertyType) Then
                newNode.Text = prop.PropertyName + " - " + CStr(propValue)
            ElseIf SAPGUIMetadata.IsEnumType(prop.PropertyType) Then
                newNode.Text = prop.PropertyName + " - " + SAPGUIMetadata.GetEnumValueMember(prop.PropertyType, propValue)
            Else
                ' Value is a object
                newNode.Tag = propValue
                newNode.Text = $"{prop.PropertyName} - {prop.PropertyType}"
                If SAPGUIMetadata.IsCollection(prop.PropertyType) Then
                    newNode.Text += " - Count: " + CStr(propValue.Count)
                End If
            End If
        Catch ex As Exception
            newNode.Text = $"{prop.PropertyName} - {prop.PropertyType}: {ex.Message}"
        End Try

        newNode.Text = prop.Access + " - " + newNode.Text
        rootNode.Nodes.Add(newNode)
    End Sub

    ''' <summary>
    ''' Load collection under root node
    ''' </summary>
    ''' <param name="control"></param>
    ''' <param name="node"></param>
    Sub LoadCollection(control As Object, node As TreeNode)
        ' For ComboBox Entries Only
        If ExtractNodePropertyName(node.Name) = "Entries" Then
            For i = 0 To control.Count - 1
                Dim childNode = New TreeNode() With {
                    .Tag = control.ElementAt(i),
                    .Name = $"{node.Name}|Entries-GuiComboBoxEntry",
                    .Text = $"Item No:{CStr(i)}"
                }
                node.Nodes.Add(childNode)
            Next
        Else
            For i = 0 To control.Count - 1
                Dim item = control.ElementAt(i)
                Dim itemType = item.GetType()
                If itemType.Name = "__ComObject" Then
                    Try
                        Dim childNode = CreateControlNode(control.ElementAt(i))
                        node.Nodes.Add(childNode)
                    Catch ex As Exception
                        Dim childNode = New TreeNode() With {
                            .Tag = control.ElementAt(i),
                            .Name = $"{node.Name}|ItemNo{CStr(i)}-{item.Type}",
                            .Text = item.Type + CStr(i)
                        }
                        node.Nodes.Add(childNode)
                    End Try
                Else
                    Dim childNode = New TreeNode() With {
                            .Name = node.Name + "|" + CStr(i),
                            .Text = CStr(item)
                        }
                    node.Nodes.Add(childNode)
                End If
            Next
        End If

    End Sub

    ''' <summary>
    ''' Load SAP object's properties under root node
    ''' </summary>
    ''' <param name="rootNode"></param>
    ''' <param name="control"></param>
    ''' <param name="propType"></param>
    Private Sub LoadProperties(rootNode As TreeNode, control As Object, Optional propType As String = Nothing)
        If String.IsNullOrEmpty(propType) Then
            propType = control.Type
        End If
        Dim controlDefinition = SAPGUIMetadata.GetControlMetadata(control, propType)
        For Each prop In controlDefinition.Properties
            If Not String.IsNullOrEmpty(prop.InheritFrom) Then
                prop = SAPGUIMetadata.GetRootProperty(prop.InheritFrom, prop.PropertyName)
            End If
            CreatePropertyNode(rootNode, prop, control)
        Next
    End Sub

    Private Sub treeObjectDetails_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles treeObjectDetails.NodeMouseDoubleClick
        NodeMouseDoubleClick(sender, e)
    End Sub

    ''' <summary>
    ''' Expand node if it's a sap object
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs)
        If e.Node.Nodes.Count < 1 Then
            Dim control = e.Node.Tag
            If Not IsNothing(control) Then
                Dim controlType As String
                Try
                    controlType = control.Type
                Catch ex As Exception
                    controlType = ExtractNodePropertyType(e.Node.Name)
                End Try

                Try
                    If (SAPGUIMetadata.IsCollection(controlType)) Then
                        LoadCollection(control, e.Node)
                    End If
                    LoadProperties(e.Node, control, controlType)
                Catch ex As Exception
                    MsgBox(ex.ToString())
                End Try
                e.Node.Expand()
            End If
        End If
    End Sub

    Private Sub treeObjectDetails_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles treeObjectDetails.NodeMouseClick
        'Handle SAP Control Node
        Dim control = e.Node.Tag
        If Not IsNothing(control) Then
            Dim controlType
            Try
                controlType = control.Type
            Catch ex As Exception
                controlType = ExtractNodePropertyType(e.Node.Name)
            End Try
            Dim controlMetadata = SAPGUIMetadata.GetControlMetadata(control, controlType) ' .Controls.FirstOrDefault(Function(x) x.Name = controlType)
            If Not IsNothing(controlMetadata) Then
                WebBrowser1.DocumentText = controlMetadata.Document
                cbbMethods.DataSource = controlMetadata.Methods
                cbbMethods.DisplayMember = "MethodName"
            Else
                WebBrowser1.DocumentText = "No information"
            End If

            txtSelectedNode.Text = e.Node.Name
            lbSelectedNodeType.Text = controlMetadata.Name

            Return
        End If

        'Handle Property Node
        Dim parentNode = e.Node.Parent
        If Not IsNothing(parentNode) Then
            Dim parentControl = parentNode.Tag
            If IsNothing(parentControl) Then
                Return
            End If
            Try
                Dim propertyName = ExtractNodePropertyName(e.Node.Name)
                If String.IsNullOrEmpty(propertyName) Then
                    Return
                End If
                Dim controlMetadata = SAPGUIMetadata.GetControlMetadata(parentControl, parentControl.Type)
                Dim propertyMetadata = controlMetadata.Properties.FirstOrDefault(Function(x) x.PropertyName = propertyName)
                If Not String.IsNullOrEmpty(propertyMetadata.InheritFrom) Then
                    propertyMetadata = SAPGUIMetadata.GetRootProperty(propertyMetadata.InheritFrom, propertyName)
                End If
                If propertyMetadata.Access = "(Read-write)" Then
                    txtSelectedNode.Text = parentControl.Id
                    lbSelectedNodeType.Text = parentControl.Type
                    lbSelectedProperty.Text = propertyName
                    txtInpSetValue.ResetText()
                    txtInpSetValue.Enabled = True
                    btnSetValue.Enabled = True
                    Return
                Else
                    lbSelectedProperty.ResetText()
                    txtInpSetValue.Enabled = False
                    btnSetValue.Enabled = False
                End If
            Catch ex As Exception
                txtInpSetValue.Enabled = False
                btnSetValue.Enabled = False
            End Try
        End If
    End Sub

    Private Sub btnSetValue_Click(sender As Object, e As EventArgs) Handles btnSetValue.Click
        Dim control = FindElementByID(txtSelectedNode.Text)
        If Not IsNothing(control) Then
            Try
                CallByName(control, lbSelectedProperty.Text, CallType.Set, txtInpSetValue.Text)
            Catch ex As Exception
                MsgBox(ex)
            End Try
        End If
    End Sub

    Private Sub txtSelectedControlID_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub cbbMethods_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbbMethods.SelectedIndexChanged
        Dim method = CType(cbbMethods.SelectedItem, SAPObjectMethod)
        If Not String.IsNullOrEmpty(method.InheritFrom) Then
            method = SAPGUIMetadata.GetRootMethod(method.InheritFrom, method.MethodName)
        End If
        dgvInput.DataSource = method.Params.Where(Function(x) Not String.IsNullOrEmpty(x.Name)).ToList()
        For Each column As DataGridViewColumn In dgvInput.Columns
            If column.Name = "Value" Then
                column.DisplayIndex = 0
                column.ReadOnly = False
            Else
                column.ReadOnly = True
            End If
        Next
    End Sub

    Private Sub btnInvoke_Click(sender As Object, e As EventArgs) Handles btnInvoke.Click
        treeViewResult.Nodes.Clear()
        Dim treeDetailNode = treeObjectDetails.SelectedNode
        If Not IsNothing(treeDetailNode) Then
            Dim control = treeDetailNode.Tag
            If Not IsNothing(control) Then
                Dim method = CType(cbbMethods.SelectedItem, SAPObjectMethod)
                If Not String.IsNullOrEmpty(method.InheritFrom) Then
                    method = SAPGUIMetadata.GetRootMethod(method.InheritFrom, method.MethodName)
                End If

                Try
                    TryInvokeFunction(method, control)
                Catch ex As Exception
                    MsgBox(ex.ToString())
                End Try
            End If
        End If

    End Sub

    Sub LoadInvokeResult(invokeResult As Object, method As SAPObjectMethod, control As Object)
        If IsNothing(invokeResult) Then
            Return
        End If

        Dim rootNode = New TreeNode() With {
                    .Name = "root",
                    .Text = $"Result"
                }
        treeViewResult.Nodes.Add(rootNode)
        If control.Type = "GuiGridView" And method.MethodName = "GetColumnTitles" Then
            Dim index = 0
            For Each item In invokeResult
                Dim newNode = New TreeNode() With {
                    .Name = index,
                    .Text = $"{index}- {item}"
                }
                rootNode.Nodes.Add(newNode)
                index += 1
            Next
        ElseIf SAPGUIMetadata.IsCollection(method.ReturnType) Then
            LoadCollection(invokeResult, rootNode)
        Else
            Try
                Dim controlNode = CreateControlNode(invokeResult)
                rootNode.Nodes.Add(controlNode)
            Catch ex As Exception
                Dim newNode = New TreeNode() With {
                    .Name = invokeResult.ToString(),
                    .Text = invokeResult.ToString()
                }
                rootNode.Nodes.Add(newNode)
            End Try
        End If
    End Sub

    Sub TryInvokeFunction(method As SAPObjectMethod, control As Object)
        Dim invokeResult
        Select Case method.Params.Count
            Case 1
                invokeResult = CallByName(control, method.MethodName, CallType.Method,
                                          ConvertInputValue(method.Params.First()))
            Case 2
                invokeResult = CallByName(control, method.MethodName, CallType.Method,
                           ConvertInputValue(method.Params(0)),
                           ConvertInputValue(method.Params(1)))
            Case 3
                invokeResult = CallByName(control, method.MethodName, CallType.Method,
                           ConvertInputValue(method.Params(0)),
                           ConvertInputValue(method.Params(1)),
                           ConvertInputValue(method.Params(2))
                           )
            Case 4
                invokeResult = CallByName(control, method.MethodName, CallType.Method,
                           ConvertInputValue(method.Params(0)),
                           ConvertInputValue(method.Params(1)),
                           ConvertInputValue(method.Params(2)),
                           ConvertInputValue(method.Params(3))
                           )
            Case 5
                invokeResult = CallByName(control, method.MethodName, CallType.Method,
                           ConvertInputValue(method.Params(0)),
                           ConvertInputValue(method.Params(1)),
                           ConvertInputValue(method.Params(2)),
                           ConvertInputValue(method.Params(3)),
                           ConvertInputValue(method.Params(4))
                           )
            Case 6
                invokeResult = CallByName(control, method.MethodName, CallType.Method,
                           ConvertInputValue(method.Params(0)),
                           ConvertInputValue(method.Params(1)),
                           ConvertInputValue(method.Params(2)),
                           ConvertInputValue(method.Params(3)),
                           ConvertInputValue(method.Params(4)),
                           ConvertInputValue(method.Params(5))
                           )
            Case Else
                invokeResult = CallByName(control, method.MethodName, CallType.Method)
        End Select
        LoadInvokeResult(invokeResult, method, control)
    End Sub

    Function ConvertInputValue(param As SAPMethodParam) As Object
        Select Case param.Type
            Case "Variant"
                Return param.Value
            Case "Long"
                Return CLng(param.Value)
            Case "Byte"
                Return CByte(param.Value)
            Case "Integer"
                Return CInt(param.Value)
            Case "GuiEnum"
                Return CInt(param.Value)
            Case "ULong"
                Return CULng(param.Value)
            Case "Boolean"
                Return CBool(param.Value)
            Case Else
                Return param.Value
        End Select
        Return 0
    End Function

    Private Sub treeViewResult_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles treeViewResult.NodeMouseDoubleClick
        NodeMouseDoubleClick(sender, e)
    End Sub

    Sub VisualizeControl(control, enable)
        Try
            If (Not IsNothing(control)) Then
                If SAPGUIMetadata.MethodExists(control.Type, "Visualize") And control.Type <> "GuiGOSShell" Then
                    control.Visualize(enable)
                End If
            End If
        Catch ex As Exception
            MsgBox(ex.ToString())
        End Try

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim control = FindElementByID(txtSelectedNode.Text)
        If Not IsNothing(control) Then
            Try
                txtInpSetValue.Text = CallByName(control, lbSelectedProperty.Text, CallType.Get, Nothing)
            Catch ex As Exception
                MsgBox(ex)
            End Try
        End If
    End Sub
End Class

