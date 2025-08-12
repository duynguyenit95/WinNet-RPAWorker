Imports System.Threading
Imports RPA.Core
Imports RPA.Worker.Core
Imports System.Net.Http
Imports System.Text.Json
Imports SAP.Utilities
Imports Serilog
Imports System.Text
Imports iText.Layout.Borders

Public Class Zppe031CalculationWorker
    Inherits TimerWorker(Of WorkerOption)
    Private _cts As CancellationTokenSource
    Private ReadOnly _contextId = "IRPE"
    'Private ReadOnly _serverUrl = "http://localhost:64137/"
    Private ReadOnly _serverUrl = "http://ros:8103/"
    'Private ReadOnly Log As ILogger

    Public Sub New(options As ServerOption)
        MyBase.New(options)

        'Log = New LoggerConfiguration().
        '        WriteTo.File(
        '            path:=AppDomain.CurrentDomain.BaseDirectory + $"Log/{_contextId}log.txt",
        '            rollingInterval:=RollingInterval.Day,
        '            rollOnFileSizeLimit:=True,
        '            retainedFileCountLimit:=30,
        '            [shared]:=True).
        '        CreateLogger()
    End Sub


    Public Async Function GetRequireCompensationQuantity() As Task(Of List(Of ZPPE031Input))
        Using client As New HttpClient()
            Dim response = Await client.GetAsync(_serverUrl & "irpe/rpa/getRequireCompensationQuantity")
            response.EnsureSuccessStatusCode()

            Dim jsonString = Await response.Content.ReadAsStringAsync()

            Dim inputs = JsonSerializer.Deserialize(Of List(Of ZPPE031Input))(jsonString, New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True
        })

            Return inputs
        End Using
    End Function
    Public Async Function UpdateRequireCompensationQuantity(zppe031Result As ZPPE031Result) As Task(Of HttpResponseMessage)
        Using client As New HttpClient()

            Dim jsonString = JsonSerializer.Serialize(zppe031Result)
            Dim content = New StringContent(jsonString, Encoding.UTF8, "application/json")

            Dim response = Await client.PutAsync(_serverUrl & "irpe/rpa/updateRequireCompensationQuantity", content)

            Return response

        End Using
    End Function
    Public Overrides Function LoadWorkerDefaultOption() As WorkerOption
        Return New WorkerOption()
    End Function
    Public Overrides Async Function PreExecute(context As WorkerExecutionContext(Of WorkerOption)) As Task(Of WorkerExecutionContext(Of WorkerOption))
        context.Log("--------------------------------------------------")
        Try
            Dim irpeId = Int32.Parse(context.JsonData)
            context.Log(irpeId)
        Catch ex As Exception
            context.Log(ex.Message)
        End Try

        _cts = New CancellationTokenSource

        Try
            Dim inputs = Await GetRequireCompensationQuantity()
            context.Log($"Found {inputs.Count} inputs to process.")
            If inputs.Count = 0 Then
                context.Log("No inputs found to process.")
                Return Await MyBase.PreExecute(context)
            End If

            Dim sapSessionId = Await context.RequestSAPSession(_contextId)
            'Await Task.Delay(5000)
            Dim zppe031 = New ZPPE031(sapSessionId)
            For Each inp In inputs
                context.Log($"Processing MO: {inp.MO}, Color: {inp.Color}, MaterialNo: {inp.MaterialNo}")
                'Log sizes info
                context.Log($"Sizes(Quantity): [{String.Join(" / ", inp.Sizes.Select(Function(s) $"{s.Size}({s.Quantity})"))}]")

                Dim result = Await zppe031.Process(inp)
                context.Log($"Result: {result.Message} - {result.RequireCompensationQuantity}")

                Dim reponse = Await UpdateRequireCompensationQuantity(result)
                context.Log(reponse.IsSuccessStatusCode)
                context.Log(reponse.StatusCode)
            Next
            Await context.RequestCloseSAPSession(sapSessionId, _contextId)
            context.Log("--------------------------------------------------")
        Catch ex As Exception
            context.Log(ex.Message & ex.StackTrace)
        End Try


        Return Await MyBase.PreExecute(context)
    End Function

    Public Overrides Async Function ExecutionHandler(context As WorkerExecutionContext(Of WorkerOption)) As Task(Of WorkerExecutionContext(Of WorkerOption))
        context.Log("--------------------------------------------------")
        Try
            Dim irpeId = Int32.Parse(context.JsonData)
            context.Log(irpeId)
        Catch ex As Exception
            context.Log(ex.Message)
        End Try
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
