
Imports System.Data
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports MIR7InvoiceWorker.Model

Public Class ZMMR0005Input
    Public Property Factories As IEnumerable(Of String) = Nothing
    Public Property PurchaseOrders As IEnumerable(Of String) = Nothing
    Public Property Reference As IEnumerable(Of String) = Nothing
    Public Property Materials As IEnumerable(Of String) = Nothing
    Public Property Suppliers As IEnumerable(Of String) = Nothing
End Class

Public Class ZMMR0005Result

    Public Property Buyer As String ' 采购员 - USERNAME
    Public Property RequestNo As String ' 请求号码 - BEDNR
    Public Property Reference As String ' 参照 - XBLNR
    Public Property PO As String ' 采购凭证 - EBELN
    Public Property POItem As String ' 项目 - EBELP
    Public Property PlannedTrip As String ' 计划行 - ETENR
    Public Property Material As String ' 物料 - MATNR
    Public Property MaterialDescription As String ' 物料描述 - MAKTX
    Public Property SO As String ' 销售单号 - VBELN
    Public Property POItemIndex As String ' 项目 - ZEILE
    Public Property GridDescription As String ' 网格值 - J_3ASIZE
    Public Property Unit As String ' 单位 - ERFME
    Public Property Quantity As String ' 数量 - ERFMG
    Public Property Currency As String ' 币别 - WAERS
    Public Property Price As String ' 单价 - NETPR
    Public Property PriceUnit As String ' 价格单位- PEINH
    Public Property Amount As String ' 金额 - NETWR
End Class

Public Class ZMMR0005
    Inherits SAPSessionBase
    Dim Log As String = String.Empty
    Private ReadOnly _FactoryButton As String = "wnd[0]/usr/btn%_S_WERKS_%_APP_%-VALU_PUSH"
    Private ReadOnly _PurOrderButton As String = "wnd[0]/usr/btn%_S_EBELN_%_APP_%-VALU_PUSH"
    Private ReadOnly _ReferenceButton As String = "wnd[0]/usr/btn%_S_XBLNR_%_APP_%-VALU_PUSH"
    Private ReadOnly _MaterialButton As String = "wnd[0]/usr/btn%_S_MATNR_%_APP_%-VALU_PUSH"
    Private ReadOnly _SupplierButton As String = "wnd[0]/usr/btn%_S_LIFNR_%_APP_%-VALU_PUSH"
    Private ReadOnly _StatusBar As String = "wnd[0]/sbar"
    Private ReadOnly _Grid As String = "wnd[0]/usr/cntlGRID1/shellcont/shell"

    Public Sub New(inpSessionStrID As String)
        MyBase.New(inpSessionStrID)
        OpenTcode("ZMMR0005")
    End Sub

    Public Sub InputFilterValue(inp As ZMMR0005Input)
        If Not IsNothing(inp.Factories) Then
            FindElementByID(_FactoryButton).Press()
            HandleMultipleSingleValueInput(inp.Factories)
        End If

        If Not IsNothing(inp.PurchaseOrders) Then
            FindElementByID(_PurOrderButton).Press()
            HandleMultipleSingleValueInput(inp.PurchaseOrders)
        End If

        If Not IsNothing(inp.Suppliers) Then
            FindElementByID(_SupplierButton).Press()
            HandleMultipleSingleValueInput(inp.Suppliers)
        End If

        If Not IsNothing(inp.Reference) Then
            FindElementByID(_ReferenceButton).Press()
            HandleMultipleSingleValueInput(inp.Reference)
        End If
        If Not IsNothing(inp.Materials) Then
            FindElementByID(_MaterialButton).Press()
            HandleMultipleSingleValueInput(inp.Materials)
        End If
    End Sub
    Public Function ReadStatusBar()
        Return FindElementByID(_StatusBar).Text
    End Function

    Public Sub Search(inp As ZMMR0005Input)
        InputFilterValue(inp)
        Execute()
    End Sub

    Public Function SearchAsDataTable(inp As ZMMR0005Input) As DataTable
        InputFilterValue(inp)
        Execute()
        Return GridViewToDataTable(FindElementByID(_Grid))
    End Function

    Public Function SearchResult(inp As ZMMR0005Input) As List(Of PID135_ZMMR0005Result)
        InputFilterValue(inp)
        Execute()
        Dim grid = FindElementByID(_Grid)
        If IsNothing(grid) Then
            Return Nothing
        End If
        Dim dataTable = GridViewToDataTable(grid)
        Dim zmmr0005Result As New List(Of PID135_ZMMR0005Result)
        For Each row As DataRow In dataTable.Rows
            If String.IsNullOrEmpty(GetSafeValue(row, "XBLNR")) OrElse String.IsNullOrEmpty(GetSafeValue(row, "EBELN")) Then
                Continue For
            End If
            zmmr0005Result.Add(New PID135_ZMMR0005Result() With {
                               .SupplierId = inp.Suppliers.FirstOrDefault(),
                               .Buyer = GetSafeValue(row, "USERNAME"),
                               .RequestNo = GetSafeValue(row, "BEDNR"),
                               .Reference = GetSafeValue(row, "XBLNR"),
                               .PO = GetSafeValue(row, "EBELN"),
                               .POItem = GetSafeValue(row, "EBELP"),
                               .PlannedTrip = GetSafeValue(row, "ETENR"),
                               .Material = GetSafeValue(row, "MATNR"),
                               .MaterialDescription = GetSafeValue(row, "MAKTX"),
                               .SO = GetSafeValue(row, "VBELN"),
                               .POItemIndex = CInt(GetSafeValue(row, "ZEILE")),
                               .GridDescription = GetSafeValue(row, "J_3ASIZE"),
                               .Unit = GetSafeValue(row, "ERFME"),
                               .Quantity = ReadDecimal(GetSafeValue(row, "ERFMG")), 'RevertSAPNumber
                               .Currency = GetSafeValue(row, "WAERS", "WAERS1"),
                               .Price = ReadDecimal(GetSafeValue(row, "NETPR")), 'RevertSAPNumber
                               .PriceUnit = CInt(GetSafeValue(row, "PEINH")),
                               .Amount = ReadDecimal(GetSafeValue(row, "NETWR")), 'RevertSAPNumber
                               .UpdateTime = Date.Now
            })
        Next
        Return zmmr0005Result

    End Function




    Private Property S_WERKS_LOW As String = "wnd[0]/usr/ctxtS_WERKS-LOW"
    Private Property S_WERKS_HIGH As String = "wnd[0]/usr/ctxtS_WERKS-HIGH"
    Private Property Reference_LOW As String = "wnd[0]/usr/txtS_XBLNR-LOW"
    Private Property Reference_HIGH As String = "wnd[0]/usr/txtS_XBLNR-HIGH"
    Private Property Supplier_Code_LOW As String = "wnd[0]/usr/ctxtS_LIFNR-LOW"


    Public Function ExampleTable() As DataTable
        Dim generalInputValue = New ZMMR0005Input() With {
                       .Factories = New List(Of String) From {"2502", "2504", "2505", "2506", "8103"},
                       .Suppliers = New List(Of String) From {"500855"},
                       .Reference = New List(Of String) From {"H23386287-P10"},
                       .PurchaseOrders = New List(Of String) From {"5700124621"}
                   }
        Return SearchAsDataTable(generalInputValue)
    End Function

    Public Sub Example()
        Dim generalInputValue = New ZMMR0005Input() With {
                       .Factories = New List(Of String) From {"2502", "2504", "2505", "2506", "8103"},
                       .Suppliers = New List(Of String) From {"500855"},
                       .Reference = New List(Of String) From {"H23386287-P10"},
                       .PurchaseOrders = New List(Of String) From {"5700124621"}
                   }
        Search(generalInputValue)
    End Sub

End Class
