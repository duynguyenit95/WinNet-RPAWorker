Imports System
Imports MIR7Invoice.Services
Imports RPA.Core.Models.SAP
Imports SAP.Utilities

Module Program
    Public Sub Main()
        Try
            ' Lấy SAP GUI Scripting Object
            Dim sapGuiAuto = GetObject("SAPGUI")
            If sapGuiAuto Is Nothing Then
                Console.WriteLine("Không thể kết nối với SAP GUI.")
                Return
            End If

            Dim sapApp = sapGuiAuto.GetScriptingEngine()
            Dim session = sapApp.FindById("/app/con[0]/ses[0]")

            If session Is Nothing Then
                Console.WriteLine("Không tìm thấy session SAP.")
                Return
            End If

            ' Lấy dữ liệu từ SAP
            Dim zAccount = New ZACCOUNT("/app/con[0]/ses[0]")
            Dim data = zAccount.SearchResult()

            If data Is Nothing OrElse data.Count = 0 Then
                Console.WriteLine("Không tìm thấy dữ liệu từ ZACCOUNT.")
                Return
            End If

            ' Cập nhật vào DB
            Dim service = New MIR7InvoiceServices()
            Dim result = service.ZAccountRecord(data).ConfigureAwait(False).GetAwaiter().GetResult()

            Console.WriteLine($"Kết quả cập nhật: {result}")
        Catch ex As Exception
            Console.WriteLine($"Lỗi: {ex.Message}")
        End Try
    End Sub
End Module
