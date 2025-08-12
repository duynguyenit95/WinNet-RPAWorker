
Imports System.Data
Imports System.IO

Public Class ZTF003Input
    Public Property CompanyCodes As IEnumerable(Of String) = Nothing
    Public Property Factories As IEnumerable(Of String) = Nothing
    Public Property PurDate As IEnumerable(Of String) = Nothing
    Public Property Customers As IEnumerable(Of String) = Nothing
    Public Property SaleOrders As IEnumerable(Of String) = Nothing
    Public Property PurchaseOrders As IEnumerable(Of String) = Nothing
    Public Property Suppliers As IEnumerable(Of String) = Nothing
    Public Property Materials As IEnumerable(Of String) = Nothing
    Public Property PurchaseGroups As IEnumerable(Of String) = Nothing
    Public Property MaterialGroups As IEnumerable(Of String) = Nothing
End Class

Public Class ZTF003
    Inherits SAPSessionBase

    Private ReadOnly _InputCompanyCode As String = "wnd[0]/usr/ctxtS_BUKRS-LOW"
    Private ReadOnly _InputFromPurDate As String = "wnd[0]/usr/ctxtS_BLDAT-LOW"
    Private ReadOnly _CompanyCodeButton As String = "wnd[0]/usr/btn%_S_BUKRS_%_APP_%-VALU_PUSH"
    Private ReadOnly _FactoryButton As String = "wnd[0]/usr/btn%_S_WERKS_%_APP_%-VALU_PUSH"
    Private ReadOnly _PurDateButton As String = "wnd[0]/usr/btn%_S_BLDAT_%_APP_%-VALU_PUSH"
    Private ReadOnly _CustomerButton As String = "wnd[0]/usr/btn%_S_KUNNR_%_APP_%-VALU_PUSH"
    Private ReadOnly _SaleOrderButton As String = "wnd[0]/usr/btn%_S_VBELN_%_APP_%-VALU_PUSH"
    Private ReadOnly _PurOrderButton As String = "wnd[0]/usr/btn%_S_EBELN_%_APP_%-VALU_PUSH"
    Private ReadOnly _SupplierButton As String = "wnd[0]/usr/btn%_S_LIFNR_%_APP_%-VALU_PUSH"
    Private ReadOnly _MaterialButton As String = "wnd[0]/usr/btn%_S_MATNR_%_APP_%-VALU_PUSH"
    Private ReadOnly _PurGroupButton As String = "wnd[0]/usr/btn%_S_EKGRP_%_APP_%-VALU_PUSH"
    Private ReadOnly _MaterialGroupButton As String = "wnd[0]/usr/btn%_S_MATKL_%_APP_%-VALU_PUSH"


    Private ReadOnly _ResultGridView As String = "wnd[0]/usr/cntlGRID1/shellcont/shell"

    Private ReadOnly _ExportSpreadSheetMenuItem As String = "wnd[0]/mbar/menu[0]/menu[3]/menu[1]"
    Private ReadOnly _ExportFileNameInput As String = "wnd[1]/usr/ctxtDY_FILENAME"
    Private ReadOnly _ExportDirectoryInput As String = "wnd[1]/usr/ctxtDY_PATH"
    Private ReadOnly _ExportGenerateButton As String = "wnd[1]/tbar[0]/btn[0]"
    Public Sub New(inpSessionStrID As String)
        MyBase.New(inpSessionStrID)
        OpenTcode("ZTF003")
    End Sub

    Public Sub Search(inp As ZTF003Input)
        InputFilterValue(inp)
        Execute()
    End Sub

    Public Function ExportResultToXmlExcel(fileNameWithoutExtension As String) As String
        If FindElementByID(_ResultGridView).RowCount = 0 Then
            Return "No Data"
        End If

        FindElementByID(_ExportSpreadSheetMenuItem).Select()
        WaitLoading()
        Dim dir = FindElementByID(_ExportDirectoryInput).Text
        Dim fileName = $"{fileNameWithoutExtension}.XML"
        FindElementByID(_ExportFileNameInput).Text = fileName
        Dim exportSuccess = False
        Dim fullfilePath = Path.Combine(dir, fileName)
        While Not exportSuccess
            FindElementByID(_ExportGenerateButton).Press()
            WaitLoading()
            Dim statusBar = ReadSessionStatusPane()
            AddLog(statusBar.MessageText)
            If statusBar.MessageText.Contains("bytes") And statusBar.MessageText.Contains("transferred") Then
                Return fullfilePath
            ElseIf statusBar.MessageText.Contains("already exists") Then
                System.IO.File.Delete(fullfilePath)
            End If
        End While
        Return String.Empty
    End Function

    Public Function ReadResult(ByRef exception As String) As DataTable
        Try
            Dim gridResult = FindElementByID(_ResultGridView)
            Return GridViewToDataTable(gridResult)
        Catch ex As Exception
            exception = ex.ToString()
        End Try
        Return Nothing
    End Function

    Public Sub InputFilterValue(inp As ZTF003Input)
        If Not IsNothing(inp.CompanyCodes) Then
            FindElementByID(_CompanyCodeButton).Press()
            HandleMultipleSingleValueInput(inp.CompanyCodes)
        End If

        If Not IsNothing(inp.Factories) Then
            FindElementByID(_FactoryButton).Press()
            HandleMultipleSingleValueInput(inp.Factories)
        End If

        If Not IsNothing(inp.PurDate) Then
            FindElementByID(_PurDateButton).Press()
            HandleMultipleSingleValueInput(inp.PurDate)
        End If

        If Not IsNothing(inp.Customers) Then
            FindElementByID(_CustomerButton).Press()
            HandleMultipleSingleValueInput(inp.Customers)
        End If

        If Not IsNothing(inp.SaleOrders) Then
            FindElementByID(_SaleOrderButton).Press()
            HandleMultipleSingleValueInput(inp.SaleOrders)
        End If

        If Not IsNothing(inp.PurchaseOrders) Then
            FindElementByID(_PurOrderButton).Press()
            HandleMultipleSingleValueInput(inp.PurchaseOrders)
        End If

        If Not IsNothing(inp.Suppliers) Then
            FindElementByID(_SupplierButton).Press()
            HandleMultipleSingleValueInput(inp.Suppliers)
        End If

        If Not IsNothing(inp.Materials) Then
            FindElementByID(_MaterialButton).Press()
            HandleMultipleSingleValueInput(inp.Materials)
        End If

        If Not IsNothing(inp.PurchaseGroups) Then
            FindElementByID(_PurGroupButton).Press()
            HandleMultipleSingleValueInput(inp.PurchaseGroups)
        End If

        If Not IsNothing(inp.MaterialGroups) Then
            FindElementByID(_MaterialGroupButton).Press()
            HandleMultipleSingleValueInput(inp.MaterialGroups)
        End If
    End Sub

    Public Sub SetupFirstInput(input As ZTF003Input)
        'Input required Field First
        FindElementByID(_InputCompanyCode).Text = "2502"
        FindElementByID(_InputFromPurDate).Text = "01.01.2022"
        InputFilterValue(input)
    End Sub

End Class
