Imports System.Threading
Imports MIR7Invoice.Services
Imports RPA.Core
Imports RPA.Worker.Core
Imports SAP.Utilities

Public Class ZAccountScrappingWorker
    Inherits TimerWorker(Of WorkerOption)
    Private _cts As CancellationTokenSource
    Private ReadOnly _contextId = "ZACCOUNT"
    Public Sub New(options As ServerOption)
        MyBase.New(options)

    End Sub
    Public Overrides Function LoadWorkerDefaultOption() As WorkerOption
        Return New WorkerOption()
    End Function

    Public Overrides Async Function PreExecute(context As WorkerExecutionContext(Of WorkerOption)) As Task(Of WorkerExecutionContext(Of WorkerOption))

        _cts = New CancellationTokenSource

        Try
            Dim sapSessionId = Await context.RequestSAPSession(_contextId)
            Dim zAccount = New ZACCOUNT(sapSessionId)
            Dim data = zAccount.SearchResult()
            If data Is Nothing OrElse data.Count = 0 Then
                context.Log("Not found any data in ZACCOUNT.")
            Else
                context.Log($"Found {data.Count} rows in ZACCOUNT")
                Dim service = New MIR7InvoiceServices()
                Await service.ZAccountRecord(data)
            End If
            Await context.RequestCloseSAPSession(sapSessionId, _contextId)
            context.Log("--------------------------------------------------")
        Catch ex As Exception
            context.Log(ex.Message & ex.StackTrace)
        End Try


        Return Await MyBase.PreExecute(context)
    End Function

    Public Overrides Async Function ExecutionHandler(context As WorkerExecutionContext(Of WorkerOption)) As Task(Of WorkerExecutionContext(Of WorkerOption))
        Return Await MyBase.ExecutionHandler(context)
    End Function

    Public Overrides Sub HandleSAPError(message As String)
        _cts.Cancel()
        _cts.Dispose()
        _cts = New CancellationTokenSource
    End Sub

    Public Overrides Async Function AfterExecute(context As WorkerExecutionContext(Of WorkerOption)) As Task(Of WorkerExecutionContext(Of WorkerOption))
        Return Await MyBase.AfterExecute(context)
    End Function
End Class
