Imports System.Text
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Collections.Generic

Public Class SocketErrorEventArgs
  Inherits EventArgs
  Dim c_Error As String
  Public Sub New()
    c_Error = Nothing
  End Sub
  Public Sub New(ByVal sError As String)
    c_Error = sError
  End Sub
  Public Property ErrorDetails As String
    Get
      Return c_Error
    End Get
    Set(ByVal value As String)
      c_Error = value
    End Set
  End Property
End Class
Public Class SocketReceivedEventArgs
  Inherits EventArgs
  Dim c_Data As Byte()
  Public Sub New()
    c_Data = Nothing
  End Sub
  Public Sub New(ByVal bData As Byte())
    c_Data = bData
  End Sub
  Public Property Data As Byte()
    Get
      Return c_Data
    End Get
    Set(ByVal value As Byte())
      c_Data = value
    End Set
  End Property
End Class
Public Class SocketWrapper
  Inherits Connection

  Public Event SocketConnected(ByVal sender As Object, ByVal e As EventArgs)
  Public Event SocketReceived(ByVal sender As Object, ByVal e As SocketReceivedEventArgs)
  Public Event SocketDisconnected(ByVal sender As Object, ByVal e As SocketErrorEventArgs)
  Public Enum ProxyType
    SOCKS4 = 4
    SOCKS5 = 5
    HTTP = 1
  End Enum
  Private Enum SOCKS4Status
    Granted = &H5A
    Failed = &H5B
    NoIdent = &H5C
    IdentFail = &H5D
  End Enum
  Private Enum SOCKS5AuthMethods
    None = &H0
    GSSAPI = &H1
    UserPass = &H2
    ChallengeHandshake = &H3
    ChallengeResponse = &H5
    SSL = &H6
    NDS = &H7
    MultiAuthFramework = &H8
  End Enum
  Private Enum SOCKS5Status
    Granted = &H0
    Failure = &H1
    NotAllowedByRuleset = &H2
    NetworkUnreachable = &H3
    HostUnreachable = &H4
    RefusedByDestination = &H5
    TTLExpired = &H6
    CommandNotSupported = &H7
    AddressNotSupported = &H8
  End Enum
  Private Enum SOCKS5AddressType
    IPv4 = &H1
    Domain = &H3
    IPv6 = &H4
  End Enum
  Private Enum SOCKS5ConnectState
    Handshake
    Authentication
    Authentication2
    Authentication3
    Authentication4
    Authentication5
    Authentication6
    Request
    Complete
  End Enum
  Private Structure ProxyData
    Public Dest As DnsEndPoint
    Public Ver As ProxyType
    Public User As String
    Public Pass As String
    Public Vars As SOCKS5Variables
    Public bData As Byte()
    Public DataIndex As Integer
  End Structure
  Private Structure SOCKS5Variables
    Public AuthMethod As SOCKS5AuthMethods
    Public ConnectionState As SOCKS5ConnectState
  End Structure
  Public Structure ProxySettings
    Public Dest As DnsEndPoint
    Public Ver As ProxyType
    Public User As String
    Public Pass As String
  End Structure
  Public Proxy As ProxySettings
  Public UseProxy As Boolean
  Public Sub New()
    MyBase.New()
  End Sub
  Protected Overrides Sub Connected()
    If UseProxy Then
      Dim pData As ProxyData
      pData.Dest = Proxy.Dest
      pData.Ver = Proxy.Ver
      pData.User = Proxy.User
      pData.Pass = Proxy.Pass
      pData.Vars.ConnectionState = SOCKS5ConnectState.Request
      pData.bData = Nothing
      pData.DataIndex = 0
      PROXY_BEGIN()
      MyBase.ReceiveNoLock(&H100000, pData)
    Else
      RaiseEvent SocketConnected(New Object, New EventArgs)
      MyBase.ReceiveNoLock(&H100000, "DATA")
    End If
  End Sub
  Protected Overrides Sub ReceivedData(ByVal e As SocketAsyncEventArgs)
    If (TypeOf (e.UserToken) Is String) Or Not UseProxy Then
      Dim bData(e.BytesTransferred - 1) As Byte
      Array.Copy(e.Buffer, bData, e.BytesTransferred)
      RaiseEvent SocketReceived(New Object, New SocketReceivedEventArgs(bData))
      MyBase.ReceiveNoLock(&H100000, "DATA")
    Else
      Dim pData As ProxyData = e.UserToken
      ReDim Preserve pData.bData(e.BytesTransferred + pData.DataIndex - 1)
      Array.Copy(e.Buffer, 0, pData.bData, pData.DataIndex, e.BytesTransferred)
      pData.DataIndex += e.BytesTransferred
      Dim pktRead As New DataReader(pData.bData)
      Select Case pData.Ver
        Case ProxyType.HTTP
          Dim lLen As Integer = pktRead.Length
          Dim bData As Byte() = pktRead.ReadByteArray(lLen)
          Dim sData As String = System.Text.Encoding.ASCII.GetString(bData)
          If InStr(sData, vbNewLine & vbNewLine) > 0 Then
            If InStr(sData, " 200") > 0 Then
              RaiseEvent SocketConnected(New Object, New EventArgs)
              MyBase.ReceiveNoLock(&H100000, "DATA")
            Else
              MyBase.Disconnect(sData)
            End If
          Else
            MyBase.ReceiveNoLock(&H100000, pData)
          End If
        Case ProxyType.SOCKS4
          Dim bNull As Byte = pktRead.ReadByte
          Dim bStatus As Byte = pktRead.ReadByte
          Dim bPort As Byte() = pktRead.ReadByteArray(2)
          Dim bIP As Byte() = pktRead.ReadByteArray(2)
          Select Case bStatus
            Case &H5A
              RaiseEvent SocketConnected(New Object, New EventArgs)
              MyBase.ReceiveNoLock(&H100000, "DATA")
            Case &H5B
              MyBase.Disconnect("Request Rejected or Connection Failed.")
            Case &H5C
              MyBase.Disconnect("Request Failed because client did not respond to ident.")
            Case &H5D
              MyBase.Disconnect("Request Failed because client's ident could not be confirmed.")
            Case Else
              MyBase.Disconnect("Unknown Request Response: " & Hex(bStatus) & ".")
          End Select
        Case ProxyType.SOCKS5
          Select Case pData.Vars.ConnectionState
            Case SOCKS5ConnectState.Handshake
              Dim bVersion As Byte = pktRead.ReadByte
              Dim bMethod As Byte = pktRead.ReadByte
              Select Case bMethod
                Case &H0, &H1, &H2, &H3, &H5, &H6, &H7, &H8
                  pData.Vars.AuthMethod = bMethod
                  'Dim pktTmp As New DataBuffer
                  Select Case pData.Vars.AuthMethod
                    Case SOCKS5AuthMethods.None
                      pData.Vars.ConnectionState = SOCKS5ConnectState.Request
                      SOCKS5_REQUEST_Send()
                      MyBase.ReceiveNoLock(&H100000, pData)
                    Case SOCKS5AuthMethods.GSSAPI
                      pData.Vars.ConnectionState = SOCKS5ConnectState.Authentication
                      Stop
                    Case SOCKS5AuthMethods.UserPass
                      pData.Vars.ConnectionState = SOCKS5ConnectState.Authentication
                      SOCKS5_USERPASS_Send()
                    Case SOCKS5AuthMethods.ChallengeHandshake
                      pData.Vars.ConnectionState = SOCKS5ConnectState.Authentication
                      Stop
                      'pktTmp.InsertByte(&H1)
                      'pktTmp.InsertByte(CHAPATAs.Charset)
                      'pktTmp.InsertString(System.Text.Encoding.ASCII.BodyName, 20127)
                      'pktTmp.InsertByte(CHAPATAs.Algorithms)
                      'pktTmp.InsertString("HMAC-MD5", 20127)
                    Case SOCKS5AuthMethods.ChallengeResponse
                      pData.Vars.ConnectionState = SOCKS5ConnectState.Authentication
                      Stop
                    Case SOCKS5AuthMethods.SSL
                      pData.Vars.ConnectionState = SOCKS5ConnectState.Authentication
                      Stop
                    Case SOCKS5AuthMethods.NDS
                      pData.Vars.ConnectionState = SOCKS5ConnectState.Authentication
                      Stop
                    Case SOCKS5AuthMethods.MultiAuthFramework
                      pData.Vars.ConnectionState = SOCKS5ConnectState.Authentication
                      Stop
                  End Select
                Case &HFF
                  MyBase.Disconnect("No valid authentication method listed.")
                Case Else
                  MyBase.Disconnect("Unknown authentication method: " & Hex(bMethod) & ".")
              End Select
            Case SOCKS5ConnectState.Authentication
              Select Case pData.Vars.AuthMethod
                Case SOCKS5AuthMethods.GSSAPI
                  Stop
                Case SOCKS5AuthMethods.UserPass
                  Dim bVersion As Byte = pktRead.ReadByte
                  Dim bStatus As Byte = pktRead.ReadByte
                  If bStatus = &H0 Then
                    pData.Vars.ConnectionState = SOCKS5ConnectState.Request
                    SOCKS5_REQUEST_Send()
                    MyBase.ReceiveNoLock(&H100000, pData)
                  Else
                    MyBase.Disconnect("Login failed [" & Hex(bStatus) & "].")
                  End If
                Case SOCKS5AuthMethods.ChallengeHandshake
                  Stop
                Case SOCKS5AuthMethods.ChallengeResponse
                  Stop
                Case SOCKS5AuthMethods.SSL
                  Stop
                Case SOCKS5AuthMethods.NDS
                  Stop
                Case SOCKS5AuthMethods.MultiAuthFramework
                  Stop
              End Select
            Case SOCKS5ConnectState.Authentication2
              Stop
            Case SOCKS5ConnectState.Request
              Dim bVersion As Byte = pktRead.ReadByte
              Dim bStatus As SOCKS5Status = pktRead.ReadByte
              Dim bReserved As Byte = pktRead.ReadByte
              Dim bAddressType As SOCKS5AddressType = pktRead.ReadByte
              Dim bAddress As Byte()
              Select Case bAddressType
                Case SOCKS5AddressType.IPv4
                  bAddress = pktRead.ReadByteArray(4)
                Case SOCKS5AddressType.Domain
                  Dim bLen As Byte = pktRead.ReadByte
                  bAddress = pktRead.ReadByteArray(bLen)
                Case SOCKS5AddressType.IPv6
                  bAddress = pktRead.ReadByteArray(16)
              End Select
              Dim bPort As Byte() = pktRead.ReadByteArray(2)
              Select Case bStatus
                Case SOCKS5Status.Granted
                  RaiseEvent SocketConnected(New Object, New EventArgs)
                  MyBase.ReceiveNoLock(&H100000, "DATA")
                Case SOCKS5Status.Failure
                  MyBase.Disconnect("General request failure.")
                Case SOCKS5Status.NotAllowedByRuleset
                  MyBase.Disconnect("Connection not allowed by ruleset.")
                Case SOCKS5Status.NetworkUnreachable
                  MyBase.Disconnect("Network unreachable.")
                Case SOCKS5Status.HostUnreachable
                  MyBase.Disconnect("Host unreachable.")
                Case SOCKS5Status.RefusedByDestination
                  MyBase.Disconnect("Connection refused by destination host.")
                Case SOCKS5Status.TTLExpired
                  MyBase.Disconnect("TTL expired.")
                Case SOCKS5Status.CommandNotSupported
                  MyBase.Disconnect("Command not supported/Protocol error.")
                Case SOCKS5Status.AddressNotSupported
                  MyBase.Disconnect("Address type not supported.")
              End Select
            Case SOCKS5ConnectState.Complete
              Debug.Print("?")
              Debug.Print(pktRead.ToString)
              Stop
          End Select
      End Select
    End If
  End Sub

  Protected Overrides Sub Disconnected(Optional ByVal sError As String = Nothing)
    RaiseEvent SocketDisconnected(New Object, New SocketErrorEventArgs(sError))
  End Sub

  Private Sub PROXY_BEGIN()
    Using pktTmp As New DataBuffer
      Select Case Proxy.Ver
        Case ProxyType.HTTP
          pktTmp.InsertByteArray(System.Text.Encoding.ASCII.GetBytes("CONNECT " & Proxy.Dest.Host & ":" & Proxy.Dest.Port.ToString.Trim & " HTTP/1.0" & vbCrLf & "User-agent: LimeSeedMediaPlayer/" & Application.ProductVersion & vbCrLf & vbCrLf))
        Case ProxyType.SOCKS4
          pktTmp.InsertByte(&H4)
          pktTmp.InsertByte(&H1)
          pktTmp.InsertByteArray({&H17, &HE0})
          pktTmp.InsertByteArray(Dns.GetHostAddresses(Proxy.Dest.Host).First.GetAddressBytes)
          pktTmp.InsertCString(Environment.UserName)
        Case ProxyType.SOCKS5
          'Hope Ident is on!
          pktTmp.InsertByte(&H5)
          pktTmp.InsertByte(&H2)
          'pktTmp.InsertByteArray({SOCKS5AuthMethods.None, SOCKS5AuthMethods.GSSAPI, SOCKS5AuthMethods.UserPass, SOCKS5AuthMethods.ChallengeHandshake, SOCKS5AuthMethods.ChallengeResponse, SOCKS5AuthMethods.SSL, SOCKS5AuthMethods.NDS, SOCKS5AuthMethods.MultiAuthFramework})
          pktTmp.InsertByteArray({SOCKS5AuthMethods.None, SOCKS5AuthMethods.UserPass})
        Case Else
          MyBase.Disconnect("Unknown proxy selection!")
          Return
      End Select
      MyBase.Send(pktTmp.GetData)
    End Using
  End Sub

  Private Sub SOCKS5_REQUEST_Send()
    Using pktTmp As New DataBuffer
      pktTmp.InsertByte(&H5)
      pktTmp.InsertByte(&H1)
      pktTmp.InsertByte(&H0)
      pktTmp.InsertByte(SOCKS5AddressType.IPv4)
      pktTmp.InsertByteArray(Dns.GetHostAddresses(Proxy.Dest.Host).First.GetAddressBytes)
      pktTmp.InsertByteArray({&H17, &HE0})
      MyBase.Send(pktTmp.GetData)
    End Using
  End Sub

  Private Sub SOCKS5_USERPASS_Send()
    Using pktTmp As New DataBuffer
      pktTmp.InsertByte(&H1)
      pktTmp.InsertByte(Proxy.User.Length)
      If Proxy.User.Length > 0 Then pktTmp.InsertByteArray(System.Text.Encoding.ASCII.GetBytes(Proxy.User))
      pktTmp.InsertByte(Proxy.Pass.Length)
      If Proxy.Pass.Length > 0 Then pktTmp.InsertString(Proxy.Pass, 20127)
      MyBase.Send(pktTmp.GetData)
    End Using
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
    Set(ByVal value As Boolean)
      m_IsConnected = value
    End Set
  End Property
  Private m_IsConnected As Boolean
  Public Property RemoteEndPoint() As DnsEndPoint
    Get
      Return m_RemoteEndPoint
    End Get
    Set(ByVal value As DnsEndPoint)
      m_RemoteEndPoint = value
    End Set
  End Property
  Private m_RemoteEndPoint As DnsEndPoint
#End Region
#Region "Connection Constructor"
  Public Sub New(ByVal remoteEp As DnsEndPoint)
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
    'connectingResetEvent.WaitOne()
    If Not IsConnected Then
      underlyingSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
      Dim connectAsyncEventArgs = connectionPool.Pull()
      connectAsyncEventArgs.RemoteEndPoint = RemoteEndPoint
      If connectAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler connectAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      If Not underlyingSocket.ConnectAsync(connectAsyncEventArgs) Then
        Disconnect("Connection to " & underlyingSocket.RemoteEndPoint.ToString & " failed. " & ErrorDetails(connectAsyncEventArgs))
      End If
      'connectingVerifyResetEvent.WaitOne()
    End If
  End Sub
  Public Sub Disconnect(Optional ByVal Details As String = Nothing)
    'connectingResetEvent.WaitOne()
    If IsConnected And underlyingSocket IsNot Nothing Then
      If Not Details = "Ignore" Then IsConnected = False
      'Dim disconnectAsyncEventArgs = connectionPool.Pull
      'disconnectAsyncEventArgs.RemoteEndPoint = RemoteEndPoint
      'If disconnectAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler disconnectAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      'disconnectAsyncEventArgs.DisconnectReuseSocket = True
      'disconnectAsyncEventArgs.UserToken = Details
      'underlyingSocket.Disconnect(False)
      'If Not underlyingSocket.DisconnectAsync(disconnectAsyncEventArgs) Then
      '  Debug.Print("Async Disconnect Failed")
      'End If
      underlyingSocket.Close()
      underlyingSocket.Dispose()
      underlyingSocket = Nothing
      If Not Details = "Ignore" Then Disconnected(Details)
      'connectingVerifyResetEvent.WaitOne()
    End If
  End Sub
  Public Function Bind(ByVal LocalEndPoint As IPEndPoint) As Boolean
    If LocalEndPoint Is Nothing Then Return False
    Try
      underlyingSocket = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
      Dim bindAsyncEventArgs = transmitPool.Pull()
      underlyingSocket.Bind(LocalEndPoint)
      IsConnected = True
      Return True
      ReceiveFrom(&H100000, "DATA")
    Catch ex As Exception
      Return False
    End Try
  End Function
  Public Sub Send(ByVal data As Byte())
    If data Is Nothing Then Throw New ArgumentNullException("data")
    If IsConnected Then
      Dim sendAsyncEventArgs = transmitPool.Pull()
      sendAsyncEventArgs.RemoteEndPoint = RemoteEndPoint
      If sendAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler sendAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      sendAsyncEventArgs.SetBuffer(data, 0, data.Length)
      If Not underlyingSocket.SendAsync(sendAsyncEventArgs) Then AsyncEventCompleted(Me, sendAsyncEventArgs)
    End If
  End Sub
  Public Sub SendTo(ByVal endPoint As EndPoint, ByVal data As Byte())
    If data Is Nothing Then Throw New ArgumentNullException("data")
    If IsConnected Then
      Dim sendToAsyncEventArgs = transmitPool.Pull
      sendToAsyncEventArgs.RemoteEndPoint = endPoint
      If sendToAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler sendToAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      sendToAsyncEventArgs.SetBuffer(data, 0, data.Length)
      If Not underlyingSocket.SendToAsync(sendToAsyncEventArgs) Then AsyncEventCompleted(Me, sendToAsyncEventArgs)
    End If
  End Sub
  Public Sub Receive(ByVal length As Integer, ByVal token As Object)
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
  Protected Sub ReceiveNoLock(ByVal length As Integer, ByVal token As Object)
    If token Is Nothing Then Return
    If IsConnected Then
      Dim receiveAsyncEventArgs = transmitPool.Pull()
      receiveAsyncEventArgs.RemoteEndPoint = RemoteEndPoint
      If receiveAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler receiveAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      receiveAsyncEventArgs.UserToken = token
      receiveAsyncEventArgs.SetBuffer(New Byte(length - 1) {}, 0, length)
      If underlyingSocket IsNot Nothing Then If Not underlyingSocket.ReceiveAsync(receiveAsyncEventArgs) Then AsyncEventCompleted(Me, receiveAsyncEventArgs)
    End If
  End Sub
  Public Sub ReceiveFrom(ByVal length As Integer, ByVal token As Object)
    If IsConnected Then
      'receivingResetEvent.WaitOne()
      Dim receiveFromAsyncEventArgs = transmitPool.Pull
      If receiveFromAsyncEventArgs.LastOperation = SocketAsyncOperation.None Then AddHandler receiveFromAsyncEventArgs.Completed, AddressOf AsyncEventCompleted
      receiveFromAsyncEventArgs.UserToken = token
      receiveFromAsyncEventArgs.SetBuffer(New Byte(length - 1) {}, 0, length)
      If Not underlyingSocket.ReceiveFromAsync(receiveFromAsyncEventArgs) Then AsyncEventCompleted(Me, receiveFromAsyncEventArgs)
    End If
  End Sub
  Protected Overridable Sub Connected()
  End Sub
  Protected Overridable Sub ReceivedData(ByVal e As SocketAsyncEventArgs)
  End Sub
  Protected Overridable Sub Disconnected(Optional ByVal sError As String = Nothing)
  End Sub
  Private Sub AsyncEventCompleted(ByVal sender As Object, ByVal e As SocketAsyncEventArgs)
    Select Case e.SocketError
      Case SocketError.Success
        Select Case e.LastOperation
          Case SocketAsyncOperation.Connect
            connectingResetEvent.[Set]()
            connectingVerifyResetEvent.[Set]()
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
              Return
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
              Return
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
  Private Function ErrorDetails(ByVal e As SocketAsyncEventArgs) As String
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
  Private Sub Dispose(ByVal disposing As Boolean)
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
  Public Sub Push(ByVal item As T)
    SyncLock sync
      items.Push(item)
    End SyncLock
  End Sub
End Class
Public Class DataBuffer
  Implements IDisposable
  Private m_Stream As MemoryStream
  Private m_Length As Integer
  Public Sub New()
    m_Stream = New MemoryStream
  End Sub

  Public Sub InsertBoolean(ByVal bVal As Boolean)
    InsertUInt32(IIf(bVal, 1, 0))
  End Sub

  Public Sub InsertByte(ByVal bVal As Byte)
    m_Stream.WriteByte(bVal)
    m_Length += 1
  End Sub

  Public Sub InsertString(ByVal sVal As String, Optional ByVal CodePage As Integer = 0)
    InsertByteArray(System.Text.Encoding.GetEncoding(CodePage).GetBytes(sVal))
  End Sub

  Public Sub InsertByteArray(ByVal bVal As Byte())
    If bVal Is Nothing Then Return
    m_Stream.Write(bVal, 0, bVal.Length)
    m_Length += bVal.Length
  End Sub

  Public Sub InsertSByte(ByVal bVal As SByte)
    m_Stream.WriteByte(bVal)
    m_Length += 1
  End Sub

  Public Sub InsertSByteArray(ByVal bVal() As SByte)
    If bVal Is Nothing Then Return
    Dim bResult(bVal.Length - 1) As Byte
    Buffer.BlockCopy(bVal, 0, bResult, 0, bResult.Length)
    m_Stream.Write(bResult, 0, bResult.Length)
    m_Length += bResult.Length
  End Sub

  Public Sub InsertInt16(ByVal iVal As Int16)
    m_Stream.Write(BitConverter.GetBytes(iVal), 0, 2)
    m_Length += 2
  End Sub

  Public Sub InsertUInt16(ByVal iVal As UInt16)
    m_Stream.Write(BitConverter.GetBytes(iVal), 0, 2)
    m_Length += 2
  End Sub

  Public Sub InsertInt16Array(ByVal iVal() As Int16)
    If iVal Is Nothing Then Return
    Dim bResult(iVal.Length * 2 - 1) As Byte
    Buffer.BlockCopy(iVal, 0, bResult, 0, bResult.Length)
    m_Stream.Write(bResult, 0, bResult.Length)
    m_Length += bResult.Length
  End Sub

  Public Sub InsertUInt16Array(ByVal iVal() As UInt16)
    If iVal Is Nothing Then Return
    Dim bResult(iVal.Length * 2 - 1) As Byte
    Buffer.BlockCopy(iVal, 0, bResult, 0, bResult.Length)
    m_Stream.Write(bResult, 0, bResult.Length)
    m_Length += bResult.Length
  End Sub

  Public Sub InsertInt32(ByVal iVal As Int32)
    m_Stream.SetLength(m_Length + 4)
    m_Stream.Write(BitConverter.GetBytes(iVal), 0, 4)
    m_Length += 4
  End Sub

  Public Sub InsertUInt32(ByVal iVal As UInt32)
    m_Stream.Write(BitConverter.GetBytes(iVal), 0, 4)
    m_Length += 4
  End Sub

  Public Sub InsertInt32Array(ByVal iVal() As Int32)
    If iVal Is Nothing Then Return
    Dim bResult(iVal.Length * 4 - 1) As Byte
    Buffer.BlockCopy(iVal, 0, bResult, 0, bResult.Length)
    m_Stream.Write(bResult, 0, bResult.Length)
    m_Length += bResult.Length
  End Sub

  Public Sub InsertUInt32Array(ByVal iVal() As UInt32)
    If iVal Is Nothing Then Return
    Dim bResult(iVal.Length * 4 - 1) As Byte
    Buffer.BlockCopy(iVal, 0, bResult, 0, bResult.Length)
    m_Stream.Write(bResult, 0, bResult.Length)
    m_Length += bResult.Length
  End Sub

  Public Sub InsertInt64(ByVal iVal As Int64)
    m_Stream.Write(BitConverter.GetBytes(iVal), 0, 8)
    m_Length += 8
  End Sub

  Public Sub InsertUInt64(ByVal iVal As UInt64)
    m_Stream.Write(BitConverter.GetBytes(iVal), 0, 8)
    m_Length += 8
  End Sub

  Public Sub InsertInt64Array(ByVal iVal() As Int64)
    If iVal Is Nothing Then Return
    Dim bResult(iVal.Length * 8 - 1) As Byte
    Buffer.BlockCopy(iVal, 0, bResult, 0, bResult.Length)
    m_Stream.Write(bResult, 0, bResult.Length)
    m_Length += bResult.Length
  End Sub

  Public Sub InsertUInt64Array(ByVal iVal() As UInt64)
    If iVal Is Nothing Then Return
    Dim bResult(iVal.Length * 8 - 1) As Byte
    Buffer.BlockCopy(iVal, 0, bResult, 0, bResult.Length)
    m_Stream.Write(bResult, 0, bResult.Length)
    m_Length += bResult.Length
  End Sub

  Public Sub InsertCString(ByVal sVal As String)
    InsertCString(sVal, Encoding.ASCII)
  End Sub

  Public Sub InsertCString(ByVal sVal As String, ByVal eVal As Encoding)
    If String.IsNullOrEmpty(sVal) Then
      InsertByte(0)
    Else
      InsertByteArray(eVal.GetBytes(sVal))
      Dim bNull(eVal.GetByteCount(vbNullChar) - 1) As Byte
      InsertByteArray(bNull)
    End If
  End Sub

  Public Sub InsertDwordString(ByVal sVal As String)
    InsertDwordString(sVal, 0)
  End Sub

  Public Sub InsertDWORDString(ByVal sVal As String, ByVal bPadding As Byte)
    If sVal.Length < 4 Then
      Dim iNulls As Integer = 4 - sVal.Length
      For I As Integer = 0 To iNulls - 1
        InsertByte(bPadding)
      Next
    End If
    Dim bVal As Byte() = Encoding.ASCII.GetBytes(sVal)
    For I As Integer = bVal.Length - 1 To 0 Step -1
      InsertByte(bVal(I))
    Next
  End Sub

  Public Overridable Function GetData() As Byte()
    If m_Length > 0 Then
      Dim bData(m_Length - 1) As Byte
      Buffer.BlockCopy(m_Stream.GetBuffer, 0, bData, 0, m_Length)
      Return bData
    Else
      Return Nothing
    End If
  End Function

  Public Overridable ReadOnly Property Length() As Integer
    Get
      Return m_Length
    End Get
  End Property

#Region "IDisposable Support"
  Private disposedValue As Boolean
  Protected Overridable Sub Dispose(ByVal disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        m_Stream = Nothing
        m_Length = 0
      End If
    End If
    Me.disposedValue = True
  End Sub
  Public Sub Dispose() Implements IDisposable.Dispose
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region
End Class
Public Class DataReader
  Implements IDisposable
  Private m_Data As Byte()
  Private m_Index As Integer
  Public Sub New(ByVal sStream As Stream, ByVal iLength As Integer)
    ReDim m_Data(iLength - 1)
    sStream.Read(m_Data, 0, iLength)
  End Sub

  Public Sub New(ByVal bData As Byte())
    m_Data = bData
  End Sub

  Public Function GetAllBytes() As Byte()
    Return m_Data
  End Function

  Public Function ReadBoolean() As Boolean
    If m_Index > Length Then Return Nothing
    Return ReadInt32() <> 0
  End Function

  Public Function ReadByte() As Byte
    If m_Index > Length Then Return Nothing
    m_Index += 1
    Return m_Data(m_Index - 1)
  End Function

  Public Function ReadByteArray(ByVal iItems As Integer) As Byte()
    If m_Index + iItems > Length Then Return Nothing
    Dim bData(iItems - 1) As Byte
    Buffer.BlockCopy(m_Data, m_Index, bData, 0, iItems)
    m_Index += iItems
    Return bData
  End Function

  Public Function ReadNullTerminatedByteArray() As Byte()
    If m_Index > Length Then Return Nothing
    Dim I As Integer = m_Index
    While I < (m_Data.Length - 1) And m_Data(I) > 0
      I += 1
    End While
    Dim bBytes(I - m_Index - 1) As Byte
    Buffer.BlockCopy(m_Data, m_Index, bBytes, 0, bBytes.Length)
    m_Index = I + 1
    Return bBytes
  End Function

  Public Function ReadInt16() As Int16
    If m_Index + 1 > Length Then Return Nothing
    Dim iRet As Int16 = BitConverter.ToInt16(m_Data, m_Index)
    m_Index += 2
    Return iRet
  End Function

  Public Function ReadInt16Array(ByVal iItems As Integer) As Int16()
    If m_Index + 1 + (iItems * 2) > Length Then Return Nothing
    Dim iData(iItems - 1) As Int16
    Buffer.BlockCopy(m_Data, m_Index, iData, 0, iItems * 2)
    m_Index += iItems * 2
    Return iData
  End Function

  Public Function ReadUInt16() As UInt16
    If m_Index + 1 > Length Then Return Nothing
    Dim iRet As UInt16 = BitConverter.ToUInt16(m_Data, m_Index)
    m_Index += 2
    Return iRet
  End Function

  Public Function ReadUInt16Array(ByVal iItems As Integer) As UInt16()
    If m_Index + 1 + (iItems * 2) > Length Then Return Nothing
    Dim iData(iItems - 1) As UInt16
    Buffer.BlockCopy(m_Data, m_Index, iData, 0, iItems * 2)
    m_Index += iItems * 2
    Return iData
  End Function

  Public Function ReadInt32() As Int32
    If m_Index + 3 > Length Then Return Nothing
    Dim iRet As Int32 = BitConverter.ToInt32(m_Data, m_Index)
    m_Index += 4
    Return iRet
  End Function

  Public Function ReadInt32Array(ByVal iItems As Integer) As Int32()
    If m_Index + 3 + (iItems * 4) > Length Then Return Nothing
    Dim iData(iItems - 1) As Int32
    Buffer.BlockCopy(m_Data, m_Index, iData, 0, iItems * 4)
    m_Index += iItems * 4
    Return iData
  End Function

  Public Function ReadUInt32() As UInt32
    If m_Index + 3 > Length Then Return Nothing
    Dim iRet As UInt32 = BitConverter.ToUInt32(m_Data, m_Index)
    m_Index += 4
    Return iRet
  End Function

  Public Function ReadUInt32Array(ByVal iItems As Integer) As UInt32()
    If m_Index + 3 + (iItems * 4) > Length Then Return Nothing
    Dim iData(iItems - 1) As UInt32
    Buffer.BlockCopy(m_Data, m_Index, iData, 0, iItems * 4)
    m_Index += iItems * 4
    Return iData
  End Function

  Public Function ReadInt64() As Int64
    If m_Index + 7 > Length Then Return Nothing
    Dim iRet As Int64 = BitConverter.ToInt64(m_Data, m_Index)
    m_Index += 8
    Return iRet
  End Function

  Public Function ReadInt64Array(ByVal iItems As Integer) As Int64()
    If m_Index + 7 + (iItems * 8) > Length Then Return Nothing
    Dim iData(iItems - 1) As Int64
    Buffer.BlockCopy(m_Data, m_Index, iData, 0, iItems * 8)
    m_Index += iItems * 8
    Return iData
  End Function

  Public Function ReadUInt64() As UInt64
    If m_Index + 7 > Length Then Return Nothing
    Dim iRet As UInt64 = BitConverter.ToUInt64(m_Data, m_Index)
    m_Index += 8
    Return iRet
  End Function

  Public Function ReadUInt64Array(ByVal iItems As Integer) As UInt64()
    If m_Index + 7 + (iItems * 8) > Length Then Return Nothing
    Dim iData(iItems - 1) As UInt64
    Buffer.BlockCopy(m_Data, m_Index, iData, 0, iItems * 8)
    m_Index += iItems * 8
    Return iData
  End Function

  Public Function Peek() As Integer
    If m_Index >= m_Data.Length - 1 Then Return -1
    Return m_Data(m_Index)
  End Function

  Public Function ReadDWORDString() As String
    If m_Index + 3 > Length Then Return Nothing
    Dim bVal As Byte() = ReadByteArray(4)
    If bVal Is Nothing Then Return Nothing
    Dim sRet As String = Encoding.ASCII.GetString(bVal)
    Return StrReverse(Replace(sRet, vbNullChar, String.Empty))
  End Function

  Public Function ReadCStringArray(ByVal Count As Integer) As String()
    If m_Index > Length Then Return Nothing
    Dim sTmp(Count - 1) As String
    For I As Integer = 0 To Count - 1
      sTmp(I) = ReadCString()
    Next
    Return sTmp
  End Function

  Public Function ReadCString() As String
    If m_Index > Length Then Return Nothing
    Return ReadCString(Encoding.ASCII)
  End Function

  Public Function ReadCString(ByVal eVal As Encoding) As String
    If m_Index > Length Then Return Nothing
    If m_Index >= m_Data.Length Then Return String.Empty
    Dim i As Integer = m_Index
    If eVal.Equals(Encoding.Unicode) Or eVal.Equals(Encoding.BigEndianUnicode) Then
      While ((i < m_Data.Length - 1) And ((i + 1 < m_Data.Length - 1) And m_Data(i) > 0))
        i += 1
      End While
    Else
      While ((i < (m_Data.Length - 1)) And (m_Data(i) > 0))
        i += 1
      End While
    End If
    Dim sRet As String = eVal.GetString(m_Data, m_Index, i - m_Index)
    m_Index = i + 1
    Return sRet
  End Function

  Public Overridable ReadOnly Property Length As Integer
    Get
      Return m_Data.Length
    End Get
  End Property

#Region "IDisposable Support"
  Private disposedValue As Boolean
  Protected Overridable Sub Dispose(ByVal disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        m_Index = 0
        m_Data = Nothing
      End If
    End If
    Me.disposedValue = True
  End Sub
  Public Sub Dispose() Implements IDisposable.Dispose
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region
End Class