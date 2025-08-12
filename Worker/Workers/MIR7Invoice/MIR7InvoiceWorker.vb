Imports RPA.Core
Imports RPA.Worker.Core
Imports RPA.Tools
Imports System.Threading
Imports System.IO
Imports MIR7Invoice.Services
Imports System.Collections.Concurrent
Imports iText.Layout.Splitting

Public Class MIR7InvoiceWorker
    Inherits TimerWorker(Of WorkerOption)
    Private _limitTaskCount = 4
    Private _cts As CancellationTokenSource
    Private _parentFolder = "\\172.19.18.69\fs01\QASRPA\QASRPA\01.Invoice - Yoyo - Howard\Test"
    Private ReadOnly services As MIR7InvoiceServices
    Private ReadOnly _processingTask As ConcurrentDictionary(Of Integer, Task)
    Private ReadOnly _ignoreSupplier As List(Of Integer) = New List(Of Integer) From {0}
    Public Sub New(options As ServerOption)
        MyBase.New(options)
        services = New MIR7InvoiceServices
        _processingTask = New ConcurrentDictionary(Of Integer, Task)
    End Sub

    Public Overrides Function LoadWorkerDefaultOption() As WorkerOption
        Return New WorkerOption() With
            {
                .NSFOpt = New NSFOptions() With
                {
                     .InputFolderURI = "\\172.19.18.69\fs01\QASRPA\QASRPA\01.Invoice - Yoyo - Howard\Test"
                }
            }
    End Function


    Public Overrides Async Function PreExecute(context As WorkerExecutionContext(Of WorkerOption)) As Task(Of WorkerExecutionContext(Of WorkerOption))
        _cts = New CancellationTokenSource

        '_cts.CancelAfter((New TimeSpan(11, 30, 0) - Date.Now.TimeOfDay).TotalMilliseconds)



        Try
            'Dim auth As New NtlmPasswordAuthentication(context.WorkerOptions.NSFOpt.AuthDomain, context.WorkerOptions.NSFOpt.AuthUsername, context.WorkerOptions.NSFOpt.AuthPassword)
            Dim parentFolder As New DirectoryInfo(context.WorkerOptions.NSFOpt.InputFolderURI)
            'Dim parentFolder As New DirectoryInfo("\\172.19.18.69\fs01\RPA\PID135Invoice")
            Dim standardFolder = parentFolder.GetDirectories().Where(Function(x) Not x.Name.EndsWith(".")).ToList()
            Dim childFolder = standardFolder.OrderBy(Of Integer)(Function(x) Integer.Parse(x.Name.Substring(0, x.Name.IndexOf("."))).ToString()).ToList()
            Dim listSupplier As New List(Of String)
            Dim supplierInfo As New SupplierInfo
            context.Log($"Processing Task Count (Init): {_processingTask.Count}")
            If _processingTask.Count = _limitTaskCount Then
                For Each process In _processingTask
                    context.Log($"SupplierId: {process.Key}")
                Next
                context.Log($"Limition task !!!")
                Await Task.Delay(5000)
                Return Await MyBase.PreExecute(context)
            End If

            For Each child In childFolder
                If _processingTask.Count = _limitTaskCount Then
                    For Each process In _processingTask
                        context.Log($"SupplierId: {process.Key}")
                    Next
                    context.Log($"Limition task !!!")
                    Exit For
                End If


                ' Check if total task reach limitation
                context.Log($"Folder: {child.FullName}")
                Dim index As Integer
                If Not Integer.TryParse(child.Name.Substring(0, child.Name.IndexOf(".")), index) Then
                    context.Log($"Folder name is not valid: {child.Name}")
                    Continue For
                End If
                ' Ignore supplier
                If _ignoreSupplier.Any(Function(x) x = index) Then
                    context.Log($"Ignore supplier: {index}")
                    Continue For
                End If

                ' Check if a task for supplier is running already
                Dim currentRunningTask As Task
                If _processingTask.TryGetValue(index, currentRunningTask) Then
                    context.Log("Task is running already")
                    Continue For
                End If

                Dim supplier = supplierInfo.GetSupplierInfo(index)
                If (IsNothing(supplier)) Then
                    context.Log("Supplier is not found")
                    Continue For
                End If

                Dim newFolder = child.GetDirectories("01.New").FirstOrDefault()
                If IsNothing(newFolder) Then
                    context.Log("New folder not found")
                    Continue For
                End If

                'Check folder 02.Processing, move all file to 01.New
                Dim processingFolder = child.GetDirectories("02.Processing").FirstOrDefault()
                If Not IsNothing(processingFolder) Then
                    Dim processFiles = processingFolder.GetFiles("*.pdf").ToList()
                    If processFiles.Count > 0 Then
                        context.Log("Move all invoice file from 02.Processing to 01.New")
                        For Each processFile In processFiles
                            processFile.MoveTo(Path.Combine(newFolder.FullName, processFile.Name))
                        Next
                    End If
                End If

                If listSupplier.Any(Function(x) x = supplier.SAPID) Then
                    context.Log($"Supplier {supplier.Name} is processing")
                    Continue For
                End If
                listSupplier.Add(supplier.SAPID)
                Dim files = newFolder.GetFiles("*.pdf").ToList()
                context.Log($"{supplier.Name}: {files.Count} invoice")
                If files.Count = 0 Then
                    context.Log("No invoice found")
                    Continue For
                End If



                Dim newTask = Task.Run(Async Function()
                                           Dim sapSessionId = Await context.RequestSAPSession(supplier.ID.ToString())
                                           Await Task.Delay(5000)
                                           Dim unit = New MIR7InvoiceUnit(files, sapSessionId, supplier, 10,
                                                                       processFolder:=Path.Combine(child.FullName, "02.Processing"),
                                                                       successFolder:=Path.Combine(child.FullName, "03.Successed"),
                                                                       errorFolder:=Path.Combine(child.FullName, "04.Errors"),
                                                                       RPAFailFolder:=Path.Combine(child.FullName, "09.RPAFail"))
                                           AddHandler unit.OnLog, AddressOf context.Log
                                           Try
                                               context.Log("Start unit...")
                                               Await unit.Start(_cts.Token)
                                           Catch ex As Exception
                                               context.Log(ex.ToString())
                                           Finally
                                               RemoveHandler unit.OnLog, AddressOf context.Log
                                           End Try

                                           Await context.RequestCloseSAPSession(sapSessionId, supplier.ID.ToString())
                                       End Function, _cts.Token).ContinueWith(Sub()
                                                                                  Dim outputTask As New Task(Sub()

                                                                                                             End Sub)
                                                                                  Dim remove = _processingTask.TryRemove(supplier.ID, outputTask)
                                                                                  context.Log($"Remove task {remove}: {supplier.ID}.{supplier.Name}")
                                                                                  context.Log($"Processing Task Count (After Removed): {_processingTask.Count}")
                                                                              End Sub)
                Dim add = _processingTask.TryAdd(supplier.ID, newTask)
                context.Log($"Add new task {add}: {supplier.ID}.{supplier.Name}")
                context.Log($"Processing Task Count (After added): {_processingTask.Count}")
                Await Task.Delay(10000)
            Next
        Catch ex As Exception
            context.Log(ex.ToString())
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
