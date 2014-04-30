Public Class frmTransfer
  Private WithEvents udpBroadcast As UDPWrapper
  Private WithEvents tcpRequest As TCPWrapper

  Private Sub cmdRefresh_Click(sender As System.Object, e As System.EventArgs) Handles cmdRefresh.Click
    LoadServerList()
  End Sub

  Private Sub frmTransfer_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
    If udpBroadcast IsNot Nothing Then
      udpBroadcast.Disconnect()
      udpBroadcast.Dispose()
      udpBroadcast = Nothing
    End If
    If tcpRequest IsNot Nothing Then
      tcpRequest.Disconnect()
      tcpRequest.Dispose()
      tcpRequest = Nothing
    End If
  End Sub

  Private Sub frmTransfer_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
    InitNet()
    lvServers.Items.Clear()
    cmdTransfer.Enabled = False
    cmdTransfer.Text = "Transfer To ? >>"
    cmdRefresh.Enabled = True
    cmdCancel.Enabled = True
    cmbIPs.Enabled = True
    lvServers.Enabled = True
    tmrTry.Tag = TriState.True
    tmrTry.Interval = 250
    tmrTry.Start()
  End Sub

  Private Sub tmrTry_Tick(sender As System.Object, e As System.EventArgs) Handles tmrTry.Tick
    If Not tmrTry.Interval = 5000 Then tmrTry.Interval = 5000
    If tmrTry.Tag = TriState.True Then
      LoadServerList()
      tmrTry.Tag = TriState.False
    ElseIf tmrTry.Tag = TriState.False Then
      cmdRefresh.Enabled = True
      lblIP.Enabled = True
      cmbIPs.Enabled = True
      If lvServers.Items.Count = 0 Then
        Dim lvTmp As New ListViewItem
        lvTmp.SubItems.Add("No Lime Seed Servers Found!")
        lvTmp.SubItems.Add("")
        lvServers.Items.Add(lvTmp)
        tmrTry.Tag = TriState.UseDefault
      Else
        tmrTry.Stop()
        tmrTry.Tag = TriState.True
      End If
    ElseIf tmrTry.Tag = TriState.UseDefault Then
      lvServers.Items.Clear()
      tmrTry.Stop()
      tmrTry.Tag = TriState.True
    End If
  End Sub

  Private Sub lvServers_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles lvServers.SelectedIndexChanged
    If lvServers.SelectedItems.Count > 0 Then
      If lvServers.SelectedItems(0).Text = "OCCUPIED" Then
        cmdTransfer.Enabled = False
        cmdTransfer.Text = "Transfer To ? >>"
      ElseIf lvServers.SelectedItems(0).Text = "ACCEPT" Or lvServers.SelectedItems(0).Text = "ACTIVE" Then
        cmdTransfer.Enabled = True
        cmdTransfer.Text = "Transfer To " & lvServers.SelectedItems(0).SubItems(1).Text & " >>"
      Else
        cmdTransfer.Enabled = False
        cmdTransfer.Text = "Transfer To ? >>"
      End If
    Else
      cmdTransfer.Enabled = False
      cmdTransfer.Text = "Transfer To ? >>"
    End If
  End Sub

  Private Sub cmdTransfer_Click(sender As System.Object, e As System.EventArgs) Handles cmdTransfer.Click
    tmrTry.Stop()
    tmrTry.Tag = TriState.True
    cmdTransfer.Enabled = False
    cmdRefresh.Enabled = False
    cmdCancel.Enabled = False
    cmbIPs.Enabled = False
    lvServers.Enabled = False
    SendRequest(lvServers.SelectedItems(0).SubItems(2).Text)
  End Sub


  Private Sub InitNet()
    Dim myIP As Object = cmbIPs.SelectedItem
    cmbIPs.Items.Clear()
    For Each ip In System.Net.Dns.GetHostAddresses("")
      If ip.AddressFamily = Net.Sockets.AddressFamily.InterNetwork Then
        cmbIPs.Items.Add(ip.ToString)
        'Exit For
      End If
    Next
    If cmbIPs.Items.Count = 0 Then cmbIPs.Items.Add("127.0.0.1")
    If myIP IsNot Nothing AndAlso cmbIPs.Items.Contains(myIP) Then
      cmbIPs.SelectedItem = myIP
    Else
      cmbIPs.SelectedIndex = 0
    End If
  End Sub

  Private Sub LoadServerList()
    cmdRefresh.Enabled = False
    lblIP.Enabled = False
    cmbIPs.Enabled = False
    lvServers.Items.Clear()
    Application.DoEvents()
    InitNet()
    Dim myIP As String = cmbIPs.SelectedItem
    If udpBroadcast IsNot Nothing Then
      udpBroadcast.Disconnect()
      udpBroadcast.Dispose()
      udpBroadcast = Nothing
    End If
    udpBroadcast = New UDPWrapper
    udpBroadcast.RemoteEndPoint = New Net.IPEndPoint(Net.IPAddress.Any, 57941)
    udpBroadcast.Bind(New Net.IPEndPoint(Net.IPAddress.Any, 57942))
    Dim sOut As String = "LSMP(" & Net.Dns.GetHostName & ")"
    sOut = BufferHex(sOut.Length, 4) & sOut
    Dim bOut() As Byte = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(sOut)
    udpBroadcast.SendTo(New Net.IPEndPoint(StrToLong(myIP.Substring(0, myIP.LastIndexOf(".") + 1) & "255"), 57941), bOut)
    tmrTry.Tag = TriState.False
    tmrTry.Start()
  End Sub


  Private Sub SendRequest(RemoteServer As String)
    Dim myIP As String = cmbIPs.SelectedItem
    If tcpRequest IsNot Nothing Then
      tcpRequest.Disconnect()
      tcpRequest.Dispose()
      tcpRequest = Nothing
    End If
    tcpRequest = New TCPWrapper
    tcpRequest.RemoteEndPoint = New Net.IPEndPoint(StrToLong(RemoteServer), 57941)
    tcpRequest.Connect()
  End Sub

  Private Delegate Sub FailureInvoker(message As String)
  Private Sub Failure(message As String)
    If Me.InvokeRequired Then
      Me.BeginInvoke(New FailureInvoker(AddressOf Failure), message)
    Else
      If Not String.IsNullOrEmpty(message) Then MsgBox(message, MsgBoxStyle.Exclamation, "Connection Failure!")
      cmdTransfer.Enabled = False
      cmdTransfer.Text = "Transfer To ? >>"
      cmdRefresh.Enabled = True
      cmdCancel.Enabled = True
      cmbIPs.Enabled = True
      lvServers.Enabled = True
    End If
  End Sub

  Private Sub tcpRequest_SocketConnected(sender As Object, e As System.EventArgs) Handles tcpRequest.SocketConnected
    If Me.InvokeRequired Then
      Me.BeginInvoke(New EventHandler(AddressOf tcpRequest_SocketConnected))
    Else
      Debug.Print("TCP Request Connected")
      Dim bOut() As Byte = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes("TRANSFER(" & Net.Dns.GetHostName & ")")
      tcpRequest.Send(bOut)
    End If
  End Sub

  Private Sub sckListener_SocketDisconnected(sender As Object, e As SocketErrorEventArgs) Handles tcpRequest.SocketDisconnected
    Debug.Print("TCP Request Diconnected: " & e.ErrorDetails)
    'Failure(e.ErrorDetails)
  End Sub

  Private Sub tcpRequest_SocketReceived(sender As Object, e As SocketReceivedEventArgs) Handles tcpRequest.SocketReceived
    If Me.InvokeRequired Then
      Me.BeginInvoke(New EventHandler(Of SocketReceivedEventArgs)(AddressOf tcpRequest_SocketReceived), sender, e)
    Else
      Dim fromIP As Net.IPEndPoint = e.RemoteEndPoint
      Dim sIn As String = System.Text.Encoding.GetEncoding(LATIN_1).GetString(e.Data)
      Dim sState As String = sIn.Substring(0, sIn.IndexOf("("))
      Dim sName As String = sIn.Substring(sIn.IndexOf("(") + 1, sIn.IndexOf(")") - (sIn.IndexOf("(") + 1))
      Dim sIP As String = fromIP.Address.ToString  ' sIn.Substring(sIn.IndexOf("/") + 1, sIn.IndexOf(")") - (sIn.IndexOf("/") + 1))
      If sState = "TRANSFER_OK" Then
        Dim sResponse As String = BufferHex(CType(Me.Owner, frmMain).dgvPlayList.RowCount, 4) & "(" & Net.Dns.GetHostName & ")" & vbLf
        For Each item As DataGridViewRow In CType(Me.Owner, frmMain).dgvPlayList.Rows
          Dim thisPath As String = item.Tag(0)
          If thisPath.ToLower.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic).ToLower) Then
            thisPath = "%MUSIC%" & thisPath.Substring(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic).Length)
            sResponse &= thisPath & vbLf
          ElseIf thisPath.ToLower.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic).ToLower) Then
            thisPath = "%MUSIC%" & thisPath.Substring(Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic).Length)
            sResponse &= thisPath & vbLf
          ElseIf thisPath.ToLower.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos).ToLower) Then
            thisPath = "%VIDEO%" & thisPath.Substring(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos).Length)
            sResponse &= thisPath & vbLf
          ElseIf thisPath.ToLower.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos).ToLower) Then
            thisPath = "%VIDEO%" & thisPath.Substring(Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos).Length)
            sResponse &= thisPath & vbLf
          End If
        Next
        sResponse &= CType(Me.Owner, frmMain).GetSelectedPlayListItem & vbLf & CType(Me.Owner, frmMain).mpPlayer.Position
        'current track and location in track should be in sResponse
        Dim bOut() As Byte = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(sResponse)
        tcpRequest.Send(bOut)
      ElseIf sState = "PLAYING" Then
        Me.DialogResult = Windows.Forms.DialogResult.Yes
        Me.Close()
      Else
        Failure("Remote Server " & sName & " (" & sIP & ") could not transfer: " & sState)
      End If
    End If
  End Sub

  Private udpBuffer() As Byte
  Private Sub udpBroadcast_SocketReceived(sender As Object, e As SocketReceivedEventArgs) Handles udpBroadcast.SocketReceived
    If Me.InvokeRequired Then
      Me.BeginInvoke(New EventHandler(Of SocketReceivedEventArgs)(AddressOf udpBroadcast_SocketReceived), sender, e)
    Else
      Dim fromIP As Net.IPEndPoint = e.RemoteEndPoint
      Dim bIn() As Byte = e.Data
      If udpBuffer Is Nothing Then
        udpBuffer = bIn
      Else
        Dim uStart As Integer = udpBuffer.Length
        ReDim Preserve udpBuffer(uStart + bIn.Length - 1)
        For I As Integer = 0 To bIn.Length - 1
          udpBuffer(uStart + I) = bIn(I)
        Next
      End If
      Dim sIn As String = System.Text.Encoding.GetEncoding(LATIN_1).GetString(udpBuffer)
      If sIn.Length >= 4 Then
        Dim udpLength As String = sIn.Substring(0, 4)
        Dim udpLen As Integer = Integer.Parse(udpLength, Globalization.NumberStyles.HexNumber)
        If sIn.Length - 4 >= udpLen Then
          sIn = sIn.Substring(4)
          Erase udpBuffer
          Dim sState As String = sIn.Substring(0, sIn.IndexOf("("))
          Dim sName As String = sIn.Substring(sIn.IndexOf("(") + 1, sIn.IndexOf(")") - (sIn.IndexOf("(") + 1))
          Dim sIP As String = fromIP.Address.ToString
          Dim myIP As Object = cmbIPs.SelectedItem
          If sIP = myIP Then Exit Sub
          If sState = "ACCEPT" Or sState = "ACTIVE" Or sState = "OCCUPIED" Then
            Dim lItem As New ListViewItem
            lItem.Text = sState
            lItem.SubItems.Add(sName)
            lItem.SubItems.Add(sIP)
            lvServers.Items.Add(lItem)
            If Not cmdRefresh.Enabled Then
              tmrTry.Stop()
              cmdRefresh.Enabled = True
              lblIP.Enabled = True
              cmbIPs.Enabled = True
              tmrTry.Tag = TriState.True
            End If
          End If
        End If
      End If
    End If

  End Sub
End Class