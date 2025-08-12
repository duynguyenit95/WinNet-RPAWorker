Imports System.Data
Imports System.IO
Public Class IRPEKey
    Public Property IrpeId As Integer
End Class
Public Class ZPPE031Input
    Inherits IRPEKey
    Public Property MO As String = Nothing
    Public Property Color As String = Nothing
    Public Property MaterialNo As String = Nothing
    Public Property Sizes As List(Of SizeQuantity)
End Class

Public Class SizeQuantity
    Public Property Size As String
    Public Property Quantity As Integer
End Class

Public Class ZPPE031Result
    Inherits IRPEKey
    Public Property Result As Boolean
    Public Property RequireCompensationQuantity As Decimal
    Public Property Message As String
End Class
Public Class ZPPE031Exception
    Inherits Exception
    Public Property Type As String

    Public Sub New(pType As String, pMessage As String)
        MyBase.New(pMessage)
        Type = pType
    End Sub
End Class
Public Class ZPPE031
    Inherits SAPSessionBase

    'Input Screen
    Private ReadOnly _InputMO As String = "wnd[0]/usr/ctxtS_AUFNR-LOW"
    Private ReadOnly _InputColor As String = "wnd[0]/usr/ctxtP_COLOR"
    Private ReadOnly _ButtonExecute As String = "wnd[0]/tbar[1]/btn[8]"


    'Select Material Screen
    Private ReadOnly _GridMaterial As String = "wnd[0]/usr/cntlSCR1/shellcont/shell"
    Private ReadOnly _ButtonAggreation As String = "wnd[0]/usr/btnBUT3"

    'Calculation Screen
    Private ReadOnly _GridCalculation As String = "wnd[0]/usr/cntlSCR2/shellcont/shell"
    Private ReadOnly _ButtonCalculate As String = "wnd[0]/usr/btnBUT1"
    Private ReadOnly _ButtonBack As String = "wnd[0]/usr/btnBUT3"

    Public Sub New(inpSessionStrID As String)
        MyBase.New(inpSessionStrID)
        OpenTcode("ZPPE031")
    End Sub

    Public Async Function Process(inp As ZPPE031Input) As Task(Of ZPPE031Result)
        Try
            Search(inp)
            'Await Task.Delay(5000)
            SelectMaterial(inp)
            'Await Task.Delay(5000)
            Dim result = Calculate(inp)
            result.IrpeId = inp.IrpeId
            Refresh()
            Return result
        Catch ex As Exception
            Return New ZPPE031Result() With {
                .IrpeId = inp.IrpeId,
                .Result = False,
                .Message = ex.Message
            }
        End Try

    End Function

    Public Sub Refresh()
        FindElementByID(_ButtonBack).Press()
        WaitLoading()
        Back()
    End Sub
    Public Function Calculate(inp As ZPPE031Input) As ZPPE031Result
        Dim result = New ZPPE031Result() With {
            .Result = False
            }

        Try
            Dim gridCal = FindElementByID(_GridCalculation)
            For Each size In inp.Sizes
                Dim MOqty = Convert.ToDecimal(gridCal.GetCellValue(0, size.Size))
                If MOqty < size.Quantity Then
                    result.Message = $"Size {size.Size} has MO quantity is {MOqty}. Reject order"
                    Return result
                End If
                gridCal.ModifyCell(2, size.Size, size.Quantity)
            Next
            FindElementByID(_ButtonCalculate).Press()
            WaitLoading()
            Dim requireQuantity = gridCal.GetCellValue(3, "HZONG")
            If IsNothing(requireQuantity) OrElse requireQuantity.ToString() = "" Then
                Dim pane = ReadSessionStatusPane()
                result.Message = pane.MessageType & "." & pane.MessageText
            Else
                result.RequireCompensationQuantity = Convert.ToDecimal(requireQuantity)
                result.Result = True
                result.Message = "OK"
            End If

        Catch ex As Exception
            result.Message = ex.Message
        End Try
        Return result
    End Function
    Public Sub SelectMaterial(inp As ZPPE031Input)
        Dim found As Boolean = False
        Dim gridMaterial = FindElementByID(_GridMaterial)
        For index = 0 To gridMaterial.RowCount - 1
            If gridMaterial.GetCellValue(index, "IDNRK") = inp.MaterialNo Then
                found = True
                gridMaterial.ModifyCell(index, "CHOOSE", "X")
                WaitLoading()
            End If
        Next
        If found Then
            FindElementByID(_ButtonAggreation).Press()
        Else
            Throw New Exception("Material not found: " & inp.MaterialNo)
        End If
    End Sub
    Public Sub Search(inp As ZPPE031Input)
        InputFilterValue(inp)
        FindElementByID(_ButtonExecute).Press()
        WaitLoading()
    End Sub



    Public Sub InputFilterValue(inp As ZPPE031Input)
        FindElementByID(_InputMO).Text = inp.MO
        FindElementByID(_InputColor).Text = inp.Color
    End Sub
End Class
