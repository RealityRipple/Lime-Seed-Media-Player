Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Public Class SocketErrorEventArgs
  Inherits EventArgs
  Dim c_Error As String
  Public Sub New()
    c_Error = Nothing
  End Sub
  Public Sub New(sError As String)
    c_Error = sError
  End Sub
  Public Property ErrorDetails As String
    Get
      Return c_Error
    End Get
    Set(value As String)
      c_Error = value
    End Set
  End Property
End Class
Public Class SocketReceivedEventArgs
  Inherits EventArgs
  Private c_Data() As Byte
  Private c_RemoteEndPoint As IPEndPoint
  Public Sub New()
    c_Data = Nothing
  End Sub
  Public Sub New(bData() As Byte)
    c_Data = bData
  End Sub
  Public Sub New(bData() As Byte, remoteEP As IPEndPoint)
    c_Data = bData
    c_RemoteEndPoint = remoteEP
  End Sub
  Public ReadOnly Property Data As Byte()
    Get
      Return c_Data
    End Get
  End Property
  Public ReadOnly Property RemoteEndPoint As IPEndPoint
    Get
      Return c_RemoteEndPoint
    End Get
  End Property
End Class

Public Class SocketAcceptedEventArgs
  Inherits EventArgs
  Dim c_Socket As Socket
  Public Sub New()
    c_Socket = Nothing
  End Sub
  Public Sub New(sck As Socket)
    c_Socket = sck
  End Sub
  Public Property Data As Socket
    Get
      Return c_Socket
    End Get
    Set(value As Socket)
      c_Socket = value
    End Set
  End Property
End Class

Public Class TCPWrapper
  Inherits Connection
  Public Event SocketAccepted(sender As Object, e As SocketAcceptedEventArgs)
  Public Event SocketConnected(sender As Object, e As EventArgs)
  Public Event SocketReceived(sender As Object, e As SocketReceivedEventArgs)
  Public Event SocketDisconnected(sender As Object, e As SocketErrorEventArgs)
  Public Sub New()
    MyBase.New()
  End Sub
  Protected Overrides Sub Accepted(acceptedSocket As System.Net.Sockets.Socket)
    RaiseEvent SocketAccepted(Me, New SocketAcceptedEventArgs(acceptedSocket))
    MyBase.ReceiveNoLock(&H100000, "DATA")
  End Sub
  Protected Overrides Sub Connected()
    RaiseEvent SocketConnected(Me, New EventArgs)
    MyBase.ReceiveNoLock(&H100000, "DATA")
  End Sub
  Protected Overrides Sub ReceivedData(e As System.Net.Sockets.SocketAsyncEventArgs)
    RaiseEvent SocketReceived(Me, New SocketReceivedEventArgs(e.Buffer, e.RemoteEndPoint))
    MyBase.ReceiveNoLock(&H100000, "DATA")
  End Sub
  Protected Overrides Sub Disconnected(Optional sError As String = Nothing)
    RaiseEvent SocketDisconnected(Me, New SocketErrorEventArgs(sError))
  End Sub
End Class
Public Class UDPWrapper
  Inherits Connection
  Public Event SocketReceived(sender As Object, e As SocketReceivedEventArgs)
  Public Sub New()
    MyBase.New()
  End Sub
  Protected Overrides Sub ReceivedData(e As System.Net.Sockets.SocketAsyncEventArgs)
    Dim bTmp(e.BytesTransferred - 1) As Byte
    Array.Copy(e.Buffer, bTmp, e.BytesTransferred)
    RaiseEvent SocketReceived(Me, New SocketReceivedEventArgs(bTmp, e.RemoteEndPoint))
    MyBase.ReceiveFrom(&H100000, "DATA")
  End Sub
End Class

Public MustInherit Class Connection
  Implements IDisposable
#Region "Connection Fields"
  Protected connectingResetEvent As AutoResetEvent
  Protected connectingVerifyResetEvent As AutoResetEvent
  Protected receivingResetEvent As AutoResetEvent
  Protected underlyingSocket As Socket
  Protected connectionPool As MemoryPool(Of SocketAsyncEventArgs)
  Protected transmitPool As MemoryPool(Of SocketAsyncEventArgs)
#End Region
#Region "Connection Properties"
  Public Property IsConnected() As Boolean
    Get
      Return m_IsConnected
    End Get
    Set(value As Boolean)
      m_IsConnected = value
    End Set
  End Property
  Private m_IsConnected As Boolean
  Public Property IsConnecting() As Boolean
    Get
      Return m_IsConnecting
    End Get
    Set(value As Boolean)
      m_IsConnecting = value
    End Set
  End Property
  Private m_IsConnecting As Boolean

  Public Property RemoteEndPoint() As IPEndPoint
    Get
      Return m_RemoteEndPoint
    End Get
    Set(value As IPEndPoint)
      m_RemoteEndPoint = value
    End Set
  End Property
  Private m_RemoteEndPoint As IPEndPoint 
#End Region
#Region "Connection Constructor"
  Public Sub New(remoteEp As IPEndPoint)
    If remoteEp Is Nothing Then Throw New ArgumentNullException("remoteEp")
    receivingResetEvent = New AutoResetEvent(True)
    ' TODO: Test with multiple pending threads .. do we need to make this a ManualResetEvent .. ?
    connectingResetEvent = New AutoResetEvent(True)
    ' Slight hack needed. Our first reset event blocks other threads, this one signals a connection was made
    connectingVerifyResetEvent = New AutoResetEvent(False)
    connectionPool = New MemoryPool(Of SocketAsyncEventArgs)()
    transmitPool = New MemoryPool(Of SocketAsyncEventArgs)()
    RemoteEndPoint = remoteEp
  End Sub
  Public Sub New()
    receivingResetEvent = New AutoResetEvent(True)
    ' TODO: Test with multiple pending threads .. do we need to make this a ManualResetEvent .. ?
    connectingResetEvent = New AutoResetEvent(True)
    ' Slight hack needed. Our first reset event blocks other threads, this one signals a connection was made
    connectingVerifyResetEvent = New AutoResetEvent(False)
    connectionPool = New MemoryPool(Of SocketAsyncEventArgs)()
    transmitPool = New MemoryPool(Of SocketAsyncEventArgs)()
  End Sub
#End Region
#Region "Connection Methods"
  Public Sub Connect()
    If Not IsConnected Then
      IsConnecting = True
      underlyingSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
      underlyingSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, True)
      Dim connectAsyncEventArgs = connectionPool.Pull()
      If RemoteEndPoint IsNot Nothing Then connectAsyncEventArgs.RemoteEndPoint = RemoteEndPoint
      If connectAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler connectAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      If Not underlyingSocket.ConnectAsync(connectAsyncEventArgs) Then AsyncEventCompleted(Me, connectAsyncEventArgs)
    End If
  End Sub
  Public Sub Disconnect(Optional Details As String = Nothing)
    Try
      If underlyingSocket IsNot Nothing Then
        IsConnecting = False
        If IsConnected Then
          IsConnected = False
          underlyingSocket.Close()
          If underlyingSocket IsNot Nothing Then
            underlyingSocket.Dispose()
            underlyingSocket = Nothing
            Disconnected(Details)
          End If
        Else
          underlyingSocket.Close()
          If underlyingSocket IsNot Nothing Then
            underlyingSocket.Dispose()
            underlyingSocket = Nothing
          End If

          Disconnected(Details)
        End If
      End If
    Catch ex As Exception
      Disconnected(Details)
    End Try
  End Sub
  Public Function Listen(LocalEndPoint As IPEndPoint, backlog As Integer) As Boolean
    If LocalEndPoint Is Nothing Then Return False
    Try
      underlyingSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
      underlyingSocket.Bind(LocalEndPoint)
      underlyingSocket.Listen(backlog)
      Dim listenAsyncEventArgs = connectionPool.Pull()
      IsConnecting = True
      IsConnected = False
      If listenAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler listenAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      If Not underlyingSocket.AcceptAsync(listenAsyncEventArgs) Then AsyncEventCompleted(Me, listenAsyncEventArgs)
      Return True
    Catch ex As Exception
      Return False
    End Try
  End Function
  Public Function Bind(LocalEndPoint As IPEndPoint) As Boolean
    If LocalEndPoint Is Nothing Then Return False
    Try
      underlyingSocket = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
      Dim bindAsyncEventArgs = transmitPool.Pull()
      underlyingSocket.Bind(LocalEndPoint)
      IsConnecting = False
      IsConnected = True
      If m_RemoteEndPoint IsNot Nothing Then ReceiveFrom(&H100000, "DATA")
      Return True
    Catch ex As Exception
      Return False
    End Try
  End Function
  Public Sub Send(data As Byte())
    If data Is Nothing Then Throw New ArgumentNullException("data")
    If IsConnected Then
      Dim sendAsyncEventArgs = transmitPool.Pull()
      sendAsyncEventArgs.RemoteEndPoint = RemoteEndPoint
      If sendAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler sendAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      sendAsyncEventArgs.SetBuffer(data, 0, data.Length)
      If Not underlyingSocket.SendAsync(sendAsyncEventArgs) Then AsyncEventCompleted(Me, sendAsyncEventArgs)
    End If
  End Sub
  Public Sub SendTo(endPoint As IPEndPoint, data As Byte())
    If data Is Nothing Then Throw New ArgumentNullException("data")
    If IsConnected Then
      Dim sendToAsyncEventArgs = transmitPool.Pull
      sendToAsyncEventArgs.RemoteEndPoint = endPoint
      If sendToAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler sendToAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      sendToAsyncEventArgs.SetBuffer(data, 0, data.Length)
      If Not underlyingSocket.SendToAsync(sendToAsyncEventArgs) Then AsyncEventCompleted(Me, sendToAsyncEventArgs)
    End If
  End Sub
  Public Sub Receive(length As Integer, token As Object)
    If IsConnected Then
      receivingResetEvent.WaitOne()
      Dim receiveAsyncEventArgs = transmitPool.Pull()
      receiveAsyncEventArgs.RemoteEndPoint = RemoteEndPoint
      If receiveAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler receiveAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      receiveAsyncEventArgs.UserToken = token
      receiveAsyncEventArgs.SetBuffer(New Byte(length - 1) {}, 0, length)
      If Not underlyingSocket.ReceiveAsync(receiveAsyncEventArgs) Then AsyncEventCompleted(Me, receiveAsyncEventArgs)
    End If
  End Sub
  Protected Sub ReceiveNoLock(length As Integer, token As Object)
    If token Is Nothing Then Exit Sub
    If IsConnected Then
      Dim receiveAsyncEventArgs = transmitPool.Pull()
      receiveAsyncEventArgs.RemoteEndPoint = RemoteEndPoint
      If receiveAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler receiveAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      receiveAsyncEventArgs.UserToken = token
      receiveAsyncEventArgs.SetBuffer(New Byte(length - 1) {}, 0, length)
      If underlyingSocket IsNot Nothing AndAlso Not underlyingSocket.ReceiveAsync(receiveAsyncEventArgs) Then AsyncEventCompleted(Me, receiveAsyncEventArgs)
    End If
  End Sub
  Public Sub ReceiveFrom(length As Integer, token As Object)
    If IsConnected Then
      'receivingResetEvent.WaitOne()
      Dim receiveFromAsyncEventArgs = transmitPool.Pull
      receiveFromAsyncEventArgs.RemoteEndPoint = RemoteEndPoint
      If receiveFromAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler receiveFromAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      receiveFromAsyncEventArgs.UserToken = token
      receiveFromAsyncEventArgs.SetBuffer(New Byte(length - 1) {}, 0, length)
      If underlyingSocket IsNot Nothing AndAlso Not underlyingSocket.ReceiveFromAsync(receiveFromAsyncEventArgs) Then AsyncEventCompleted(Me, receiveFromAsyncEventArgs)
    End If
  End Sub
  Protected Overridable Sub Accepted(acceptedSocket As Socket)
  End Sub
  Protected Overridable Sub Connected()
  End Sub
  Protected Overridable Sub ReceivedData(e As SocketAsyncEventArgs)
  End Sub
  Protected Overridable Sub Disconnected(Optional sError As String = Nothing)
  End Sub
  Private Sub AsyncEventCompleted(sender As Object, e As SocketAsyncEventArgs)
    Select Case e.SocketError
      Case SocketError.Success
        Select Case e.LastOperation
          Case SocketAsyncOperation.Accept
            connectingResetEvent.[Set]()
            connectingVerifyResetEvent.[Set]()
            IsConnecting = False
            IsConnected = True
            'Stop
            underlyingSocket = e.AcceptSocket
            Accepted(e.AcceptSocket)
            'Connected()
          Case SocketAsyncOperation.Connect
            connectingResetEvent.[Set]()
            connectingVerifyResetEvent.[Set]()
            IsConnecting = False
            IsConnected = True
            Connected()
          Case SocketAsyncOperation.Receive
            If e.BytesTransferred = 0 Then
              If e.LastOperation = SocketAsyncOperation.Connect Then
                connectionPool.Push(e)
              Else
                transmitPool.Push(e)
              End If
              If e.SocketError = SocketError.Success Then
                Disconnect("Connection dropped.")
              Else
                Disconnect(ErrorDetails(e))
              End If
              Exit Sub
            Else
              ReceivedData(e)
            End If
          Case SocketAsyncOperation.ReceiveFrom
            If e.BytesTransferred = 0 Then
              If e.LastOperation = SocketAsyncOperation.Connect Then
                connectionPool.Push(e)
              Else
                transmitPool.Push(e)
              End If
              If e.SocketError = SocketError.Success Then
                Disconnect("Connection dropped.")
              Else
                Disconnect(ErrorDetails(e))
              End If
              Exit Sub
            Else
              ReceivedData(e)
            End If
          Case SocketAsyncOperation.Disconnect
            Debug.Print("Async Disconnect Completed")
          Case Else
            'Debug.Print("Unhandled async event completed: " & e.LastOperation.ToString)
        End Select
        If e.LastOperation = SocketAsyncOperation.Connect Then
          connectionPool.Push(e)
        Else
          transmitPool.Push(e)
        End If
      Case Else
        Select Case e.LastOperation
          Case SocketAsyncOperation.Connect
            connectionPool.Push(e)
          Case SocketAsyncOperation.Disconnect
            Debug.Print("Async Disconnect Errored")
          Case Else
            transmitPool.Push(e)
        End Select
        Disconnect(ErrorDetails(e))
    End Select
  End Sub
#End Region

#Region "Useful Functions"
  Private Function ErrorDetails(e As SocketAsyncEventArgs) As String
    Select Case e.SocketError
      Case SocketError.Success : Return Nothing
      Case SocketError.AccessDenied : Return "Access to " & e.RemoteEndPoint.ToString & " denied."
      Case SocketError.AddressAlreadyInUse : Return "Address is already in use."
      Case SocketError.AddressFamilyNotSupported : Return "Address family " & e.RemoteEndPoint.AddressFamily.ToString & " not supported."
      Case SocketError.AddressNotAvailable : Return "Address is not available."
      Case SocketError.AlreadyInProgress : Return "Already in progress..."
      Case SocketError.ConnectionAborted : Return "Connection aborted!"
      Case SocketError.ConnectionRefused : Return "Connection refused by remote server!"
      Case SocketError.ConnectionReset : Return "Connection reset by remote peer!"
      Case SocketError.DestinationAddressRequired : Return "Destination address required."
      Case SocketError.Disconnecting : Return "Disconnecting..."
      Case SocketError.Fault : Return "Socket fault. Invalid address pointer. Aborting."
      Case SocketError.HostDown : Return "Remote server down."
      Case SocketError.HostNotFound : Return "Remote server not found."
      Case SocketError.HostUnreachable : Return "Remote server unreachable."
      Case SocketError.InProgress : Return "A blocking operation is in progress."
      Case SocketError.Interrupted : Return "Connection interrupted!"
      Case SocketError.InvalidArgument : Return "Invalid argument specified."
      Case SocketError.IOPending : Return "Overlapped socket operation is pending."
      Case SocketError.IsConnected : Return "Already connected."
      Case SocketError.MessageSize : Return "Message is too long"
      Case SocketError.NetworkDown : Return "Network not available."
      Case SocketError.NetworkReset : Return "Network reset."
      Case SocketError.NetworkUnreachable : Return "Network unreachable."
      Case SocketError.NoBufferSpaceAvailable : Return "Not enough available memory for network buffer."
      Case SocketError.NoData : Return "The requested name or IP address was not found on the name server."
      Case SocketError.NoRecovery : Return "Unrecoverable error. Terminating."
      Case SocketError.NotConnected : Return "Not connected."
      Case SocketError.NotInitialized : Return "Not initialized."
      Case SocketError.NotSocket : Return "Not a socket."
      Case SocketError.OperationAborted : Return "Operation aborted by user."
      Case SocketError.OperationNotSupported : Return "Operation not supported by address family " & e.RemoteEndPoint.AddressFamily.ToString & "."
      Case SocketError.ProcessLimit : Return "Too many processes are using the underlying socket."
      Case SocketError.ProtocolFamilyNotSupported : Return "Protocol faimly not supported."
      Case SocketError.ProtocolNotSupported : Return "Protocol not supported."
      Case SocketError.ProtocolOption : Return "Unknown, invalid, or unsupported protocol option or level."
      Case SocketError.ProtocolType : Return "Protocol type is incorrect for this socket type."
      Case SocketError.SocketNotSupported : Return "This socket is not supported by address family " & e.RemoteEndPoint.AddressFamily & "."
      Case SocketError.SystemNotReady : Return "Network subsystem is not ready."
      Case SocketError.TimedOut : Return "Connection timed out."
      Case SocketError.TooManyOpenSockets : Return "Too many open sockets!"
      Case SocketError.TryAgain : Return "Remote server could not be resolved. Try again later."
      Case SocketError.TypeNotFound : Return "Specified class type not found."
      Case SocketError.VersionNotSupported : Return "Socket version not supported."
      Case SocketError.WouldBlock : Return "Operation of non-blocking socket can not be completed immediately."
      Case Else : Return e.SocketError.ToString
    End Select
  End Function
#End Region
#Region "IDisposable"
#Region "IDisposable Property"
  ''' <summary>
  ''' Determines if the resources have been disposed of already.
  ''' </summary>
  Protected isDisposed As Boolean
#End Region
  ''' <summary>
  ''' Dispose of the underlying connection, any pools, and clean up managed resources.
  ''' </summary>
  Public Sub Dispose() Implements IDisposable.Dispose
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
  Private Sub Dispose(disposing As Boolean)
    If Not isDisposed Then
      If disposing Then
        If underlyingSocket IsNot Nothing Then underlyingSocket.Dispose()

        connectingResetEvent.Dispose()
        connectingVerifyResetEvent.Dispose()
        receivingResetEvent.Dispose()
      End If

      underlyingSocket = Nothing
      connectingResetEvent = Nothing
      receivingResetEvent = Nothing
    End If
  End Sub
#End Region
End Class
Public Class MemoryPool(Of T As New)
  Protected items As Stack(Of T)
  Protected ReadOnly sync As Object
  Public Sub New()
    items = New Stack(Of T)()
    sync = New Object()
  End Sub
  Public Function Pull() As T
    SyncLock sync
      If items.Count = 0 Then
        Return New T()
      Else
        Return items.Pop()
      End If
    End SyncLock
  End Function
  Public Sub Push(item As T)
    SyncLock sync
      items.Push(item)
    End SyncLock
  End Sub
End Class