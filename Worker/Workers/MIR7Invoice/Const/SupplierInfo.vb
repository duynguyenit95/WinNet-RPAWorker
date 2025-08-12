Imports OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup
Imports RPA.Core

Public Class SupplierInfo
    Public Function GetSupplierInfo(supIndex As Integer) As Supplier
        Select Case supIndex
            Case 1
                Return New Supplier() With {
                    .ID = 1,
                    .SAPID = "0000500855",
                    .Name = "BEST PACIFIC TEXTILE LTD.",
                    .ProcessingType = MIR7InvoiceProcessingType.Invoice,
                    .Ignore517 = True
            }
            Case 2
                Return New Supplier() With {
                    .ID = 2,
                    .SAPID = "0000500333",
                    .Name = "SML (HONG KONG) Limited.",
                    .ProcessingType = MIR7InvoiceProcessingType.Invoice
            }
            Case 3
                Return New Supplier() With {
                    .ID = 3,
                    .SAPID = "0000502350",
                    .Name = "Nilorn East Asia Limited",
                    .ProcessingType = MIR7InvoiceProcessingType.DeliveryNote
            }
            Case 4
                Return New Supplier() With {
                    .ID = 4,
                    .SAPID = "0000500190",
                    .Name = "INTERNATIONAL TRIMMINGS & LABELS",
                    .ProcessingType = MIR7InvoiceProcessingType.PO
            }
            Case 5
                Return New Supplier() With {
                    .ID = 5,
                    .SAPID = "0000500119",
                    .Name = "FineLine Technologies LLC LTD",
                    .ProcessingType = MIR7InvoiceProcessingType.Invoice
            }
            Case 6
                Return New Supplier() With {
                    .ID = 6,
                    .SAPID = "0000500449",
                    .Name = "YVONNE INDUSTRIAL COMPANY LTD",
                    .ProcessingType = MIR7InvoiceProcessingType.Invoice
            }
            Case 7
                Return New Supplier() With {
                    .ID = 7,
                    .SAPID = "0000500341",
                    .Name = "STRETCHLINE (HONG KONG) LTD",
                    .ProcessingType = MIR7InvoiceProcessingType.Invoice
            }
            Case 8
                Return New Supplier() With {
                    .ID = 8,
                    .SAPID = "0000500295",
                    .Name = "PRYM INTIMATES HONG KONG LIMITED",
                    .ProcessingType = MIR7InvoiceProcessingType.PO
            }
            Case 9
                Return New Supplier() With {
                    .ID = 9,
                    .SAPID = "0000502054",
                    .Name = "New Horizon Investment (Hong Kong)",
                    .ProcessingType = MIR7InvoiceProcessingType.Invoice,
                    .Ignore517 = True
            }
            Case 10
                Return New Supplier() With {
                    .ID = 10,
                    .SAPID = "0000500283",
                    .Name = "PIONEER ELASTIC (HONG KONG) Ltd",
                    .ProcessingType = MIR7InvoiceProcessingType.Invoice
            }
            Case 11
                Return New Supplier() With {
                    .ID = 11,
                    .SAPID = "0000500158",
                    .Name = "HING YIP PRODUCTS 1971 LIMITED",
                    .ProcessingType = MIR7InvoiceProcessingType.Invoice,
                    .ProcessingTypeBackup = MIR7InvoiceProcessingType.DeliveryNote
            }
            Case 12
                Return New Supplier() With {
                    .ID = 12,
                    .SAPID = "0000500587",
                    .Name = "LIJUN (HK) INDUSTRIAL CO.LTD",
                    .ProcessingType = MIR7InvoiceProcessingType.Invoice
            }
            Case 13
                Return New Supplier() With {
                    .ID = 13,
                    .SAPID = "0000500380",
                    .Name = "TIANHAI LACE CO., LTD",
                    .ProcessingType = MIR7InvoiceProcessingType.Invoice
            }
            Case 14
                Return New Supplier() With {
                    .ID = 14,
                    .SAPID = "0000500538",
                    .Name = "SILVER PRINTING COMPANY LTD",
                    .ProcessingType = MIR7InvoiceProcessingType.DeliveryNote
            }
            Case 15
                Return New Supplier() With {
                    .ID = 15,
                    .SAPID = "0000502485",
                    .Name = "INSPIRE PLASTICS LIMITED",
                    .ProcessingType = MIR7InvoiceProcessingType.DeliveryNote
            }
            Case 16
                Return New Supplier() With {
                    .ID = 16,
                    .SAPID = "0000501091",
                    .Name = "Hung Hon (4K) Limited",
                    .ProcessingType = MIR7InvoiceProcessingType.DeliveryNote,
                    .ProcessingTypeBackup = MIR7InvoiceProcessingType.Invoice
            }
            Case 17
                Return New Supplier() With {
                    .ID = 17,
                    .SAPID = "0000500353",
                    .Name = "SUNRISE TEXTILE ACCESSORIES (TRADING)",
                    .ProcessingType = MIR7InvoiceProcessingType.DeliveryNote
            }
            Case 18
                Return New Supplier() With {
                    .ID = 18,
                    .SAPID = "0000500266",
                    .Name = "PACIFIC TEXTILES LTD",
                    .ProcessingType = MIR7InvoiceProcessingType.DeliveryNote
            }
            Case 19
                Return New Supplier() With {
                    .ID = 19,
                    .SAPID = "0000500029",
                    .Name = "BILLION RISE KNITTING (HK) Limited",
                    .ProcessingType = MIR7InvoiceProcessingType.DeliveryNote
            }
            Case 20
                Return New Supplier() With {
                    .ID = 20,
                    .SAPID = "0000501245",
                    .Name = "FASTECH ASIA WORLDWIDE LIMITED",
                    .ProcessingType = MIR7InvoiceProcessingType.DeliveryNote
            }
            Case 21
                Return New Supplier() With {
                    .ID = 21,
                    .SAPID = "0000500523",
                    .Name = "JIANGMEN XINHUI CHARMING Industry",
                    .ProcessingType = MIR7InvoiceProcessingType.Invoice,
                    .ProcessingTypeBackup = MIR7InvoiceProcessingType.DeliveryNote
            }
            Case 22
                Return New Supplier() With {
                    .ID = 22,
                    .SAPID = "0000500364",
                    .Name = "TAI HING PLASTIC METAL LTD",
                    .ProcessingType = MIR7InvoiceProcessingType.PO,
                    .ProcessingTypeBackup = MIR7InvoiceProcessingType.DeliveryNote
            }
            Case Else
                Return Nothing
        End Select
    End Function
End Class
