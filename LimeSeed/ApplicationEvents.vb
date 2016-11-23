Namespace My

  ' The following events are available for MyApplication:
  ' 
  ' Startup: Raised when the application starts, before the startup form is created.
  ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
  ' UnhandledException: Raised if the application encounters an unhandled exception.
  ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
  ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
  Partial Friend Class MyApplication

    Private Sub MyApplication_Shutdown(sender As Object, e As System.EventArgs) Handles Me.Shutdown
      WipeRndFiles()
    End Sub

    Private Sub MyApplication_Startup(sender As Object, e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
      Me.IsSingleInstance = My.Settings.SingleInstance
      If lastNextInstance = 0 Then lastNextInstance = Environment.TickCount
      If My.Settings.MustUpgrade Then
        My.Settings.Upgrade()
        My.Settings.MustUpgrade = False
        My.Settings.Save()
      End If
      WipeRndFiles()
      FastPC(True)
      TransferListen()
      'Dim transferInvoker As New MethodInvoker(AddressOf TransferListen)
      'transferInvoker.BeginInvoke(Nothing, Nothing)
    End Sub

    Private lastNextInstance As Integer
    Private instanceIterate As Integer
    Private Sub MyApplication_StartupNextInstance(sender As Object, e As Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs) Handles Me.StartupNextInstance
      If My.Settings.SingleInstance Then
        If e.CommandLine.Count > 0 Then
          If My.Forms.frmMain IsNot Nothing Then
            My.Forms.frmMain.tmrCommandCycle.Stop()
            Dim commands As New ObjectModel.Collection(Of String)
            If My.Forms.frmMain.tmrCommandCycle.Tag IsNot Nothing Then commands = My.Forms.frmMain.tmrCommandCycle.Tag
            For Each item In e.CommandLine
              commands.Add(item)
            Next
            My.Forms.frmMain.tmrCommandCycle.Tag = commands
            My.Forms.frmMain.tmrCommandCycle.Start()
          End If
          e.BringToForeground = False
        Else
          e.BringToForeground = True
        End If
      Else
        If lastNextInstance = 0 Then lastNextInstance = Environment.TickCount
        If Math.Abs(Environment.TickCount - lastNextInstance) < 10000 Or Not My.Forms.frmMain.cmdPlayPause.Enabled Then
          lastNextInstance = Environment.TickCount
          If e.CommandLine.Count > 0 Then
            If My.Forms.frmMain IsNot Nothing Then
              My.Forms.frmMain.tmrCommandCycle.Stop()
              Dim commands As New ObjectModel.Collection(Of String)
              If My.Forms.frmMain.tmrCommandCycle.Tag IsNot Nothing Then commands = My.Forms.frmMain.tmrCommandCycle.Tag
              For Each item In e.CommandLine
                commands.Add(item)
              Next
              My.Forms.frmMain.tmrCommandCycle.Tag = commands
              My.Forms.frmMain.tmrCommandCycle.Start()
            End If
            e.BringToForeground = False
          Else
            e.BringToForeground = True
          End If
        Else
          lastNextInstance = Environment.TickCount
          instanceIterate += 1
          Dim frmTmp As New frmMain
          frmTmp.Tag = "NOCMD"
          frmTmp.instanceID = instanceIterate
          frmTmp.Show()
          frmTmp.tmrCommandCycle.Stop()
          Dim commands As New ObjectModel.Collection(Of String)
          If frmTmp.tmrCommandCycle.Tag IsNot Nothing Then commands = frmTmp.tmrCommandCycle.Tag
          For Each item In e.CommandLine
            commands.Add(item)
          Next
          frmTmp.tmrCommandCycle.Tag = commands
          frmTmp.tmrCommandCycle.Start()
          e.BringToForeground = False
        End If
      End If
    End Sub

    'Private notifyIco As NotifyIcon
    'Private notifyTimer As Threading.Timer
    Private Sub MyApplication_UnhandledException(sender As Object, e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
      Try
        If e.Exception.Message.Contains("Could not load file or assembly") Then
          MsgBox("A critical file is missing. Please ensure " & My.Application.Info.ProductName & " has been fully installed." & vbNewLine & e.Exception.ToString, MsgBoxStyle.Critical, "Could not load File or Assembly.")
          e.ExitApplication = True
        Else
          Dim frmError As New Form With
            {
              .FormBorderStyle = Windows.Forms.FormBorderStyle.SizableToolWindow,
              .Text = "Error in " & My.Application.Info.ProductName,
              .ShowIcon = False,
              .MinimizeBox = False,
              .MaximizeBox = False,
              .HelpButton = False,
              .Size = New Drawing.Size(486, 250),
               .StartPosition = FormStartPosition.CenterParent,
              .MinimumSize = New Drawing.Size(340, 200),
              .TopMost = True
            }
          Dim pnlError As New TableLayoutPanel With
            {
              .RowCount = 3,
              .ColumnCount = 4,
              .Dock = DockStyle.Fill
            }
          pnlError.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 48))
          pnlError.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
          pnlError.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
          pnlError.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
          pnlError.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
          pnlError.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
          pnlError.RowStyles.Add(New RowStyle(SizeType.AutoSize))
          Dim sErrorText As String = My.Application.Info.ProductName & " has Encountered an Error"
          If e.Exception.TargetSite IsNot Nothing Then sErrorText &= " in " & e.Exception.TargetSite.Name
          Dim lblError As New Label With
            {
              .Font = New Drawing.Font(frmError.Font.FontFamily, 12, Drawing.FontStyle.Bold),
              .Text = sErrorText,
              .Dock = DockStyle.Fill,
              .AutoEllipsis = True,
              .TextAlign = Drawing.ContentAlignment.MiddleLeft
            }
          Dim pctError As New PictureBox With
            {
              .Image = Drawing.SystemIcons.Error.ToBitmap,
              .SizeMode = PictureBoxSizeMode.AutoSize,
              .BackColor = Drawing.Color.Transparent,
              .Anchor = AnchorStyles.None
            }
          Dim txtError As New TextBox With
            {
              .ReadOnly = True,
              .BorderStyle = BorderStyle.Fixed3D,
              .Text = "Error: " & e.Exception.Message,
              .Dock = DockStyle.Fill,
              .Multiline = True,
              .ScrollBars = ScrollBars.Vertical
            }
          txtError.Text = "Error: " & e.Exception.Message
          If Not String.IsNullOrEmpty(e.Exception.StackTrace) Then
            If e.Exception.StackTrace.Contains(vbCr) Then
              txtError.Text &= vbNewLine & e.Exception.StackTrace.Substring(0, e.Exception.StackTrace.IndexOf(vbCr))
            Else
              txtError.Text &= vbNewLine & e.Exception.StackTrace
            End If
          Else
            If Not String.IsNullOrEmpty(e.Exception.Source) Then
              txtError.Text &= vbNewLine & " @ " & e.Exception.Source
              If e.Exception.TargetSite IsNot Nothing Then txtError.Text &= "." & e.Exception.TargetSite.Name
            Else
              If e.Exception.TargetSite IsNot Nothing Then txtError.Text &= vbNewLine & " @ " & e.Exception.TargetSite.Name
            End If
          End If
          Dim cmdReport As New Button With
            {
              .Text = "Report Error",
              .AutoSize = True,
              .Padding = New Padding(4),
              .Anchor = AnchorStyles.Right,
              .FlatStyle = FlatStyle.System,
              .Enabled = False
            }
          AddHandler cmdReport.Click, Sub()
                                        frmError.DialogResult = Windows.Forms.DialogResult.OK
                                        frmError.Close()
                                      End Sub
          Dim lblReport As New Label With
            {
              .AutoSize = True,
              .Padding = New Padding(3),
              .Text = "The reporting system is disabled." & vbNewLine & "This is a work in progress.",
              .Anchor = AnchorStyles.Right
            }
          Dim cmdIgnore As New Button With
            {
              .Text = "Ignore and Continue",
              .AutoSize = True,
              .Padding = New Padding(4),
              .Anchor = AnchorStyles.Right,
              .FlatStyle = FlatStyle.System
            }
          AddHandler cmdIgnore.Click, Sub()
                                        frmError.DialogResult = Windows.Forms.DialogResult.Ignore
                                        frmError.Close()
                                      End Sub
          Dim cmdExit As New Button With
            {
              .Text = "Exit Application",
              .AutoSize = True,
              .Padding = New Padding(4),
              .Anchor = AnchorStyles.Right,
              .FlatStyle = FlatStyle.System
            }
          AddHandler cmdExit.Click, Sub()
                                      frmError.DialogResult = Windows.Forms.DialogResult.Abort
                                      frmError.Close()
                                    End Sub
          frmError.Controls.Add(pnlError)
          pnlError.Controls.Add(pctError, 0, 0)
          pnlError.Controls.Add(lblError, 1, 0)
          pnlError.Controls.Add(txtError, 1, 1)
          pnlError.Controls.Add(lblReport, 0, 2)
          pnlError.Controls.Add(cmdIgnore, 2, 2)
          pnlError.Controls.Add(cmdExit, 3, 2)
          pnlError.SetColumnSpan(lblError, 3)
          pnlError.SetColumnSpan(txtError, 3)
          pnlError.SetColumnSpan(cmdReport, 2)
          frmError.AcceptButton = cmdReport
          frmError.CancelButton = cmdIgnore
          My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Hand)
          'Dim tmrCheck As New Timer With
          '  {
          '    .Interval = 400,
          '    .Enabled = False
          '  }
          'AddHandler frmError.Shown, Sub()
          '                             tmrCheck.Enabled = True
          '                           End Sub
          'AddHandler tmrCheck.Tick, Sub()
          '                            tmrCheck.Enabled = False
          '                            Select Case clsUpdate.QuickCheckVersion
          '                              Case clsUpdate.CheckEventArgs.ResultType.NewUpdate
          '                                cmdReport.Visible = False
          '                                pnlError.Controls.Remove(cmdReport)
          '                                pnlError.Controls.Add(lblReport, 0, 2)
          '                                pnlError.SetColumnSpan(lblReport, 2)
          '                                lblReport.Text = "New version available. Please update before reporting errors."
          '                              Case clsUpdate.CheckEventArgs.ResultType.NewBeta
          '                                cmdReport.Visible = False
          '                                pnlError.Controls.Remove(cmdReport)
          '                                pnlError.Controls.Add(lblReport, 0, 2)
          '                                pnlError.SetColumnSpan(lblReport, 2)
          '                                lblReport.Text = "New BETA version available. Errors are often fixed in BETA versions before a final release."
          '                              Case clsUpdate.CheckEventArgs.ResultType.NoUpdate : cmdReport.Enabled = True
          '                            End Select
          '                          End Sub
          Select Case frmError.ShowDialog
            Case DialogResult.OK
              MsgBox("This program, by its very nature, is too unstable to set up an error reporting system." & vbNewLine & "The code is just here for future use", MsgBoxStyle.Information, "You can't see this message!")
              'e.ExitApplication = False
              'Dim sRet As String = MantisReporter.ReportIssue(e.Exception)
              'If sRet = "OK" Then
              '  MsgDlg(Nothing, "Thank you for reporting the error." & vbNewLine & vbNewLine & "<a href=""http://bugs.realityripple.com/set_project.php?project_id=2"">View Details about the Error</a>", "The error has been reported.", "Error Report Sent!", MessageBoxButtons.OK, _TaskDialogIcon.MoveToNetwork, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2, , , , , True)
              'Else
              '  Dim sErrRep As String = "http://bugs.realityripple.com/set_project.php?project_id=2&make_default=no&ref=bug_report_page.php"
              '  sErrRep &= "?platform=" & IIf(Environment.Is64BitProcess, "x64", IIf(Environment.Is64BitOperatingSystem, "x86-64", "x86"))
              '  sErrRep &= "%2526os=" & DoubleEncode(My.Computer.Info.OSFullName.Trim)
              '  sErrRep &= "%2526os_build=" & DoubleEncode(My.Computer.Info.OSVersion.Trim)
              '  Dim sSum As String = e.Exception.Message
              '  If sSum.Length > 80 Then sSum = sSum.Substring(0, 77) & "..."
              '  sErrRep &= "%2526summary=" & DoubleEncode(sSum)
              '  Dim sDesc As String = e.Exception.Message
              '  If Not String.IsNullOrEmpty(e.Exception.StackTrace) Then
              '    sDesc &= vbNewLine & e.Exception.StackTrace.Substring(0, e.Exception.StackTrace.IndexOf(vbCr))
              '  Else
              '    If Not String.IsNullOrEmpty(e.Exception.Source) Then
              '      sDesc &= vbNewLine & " @ " & e.Exception.Source
              '      If e.Exception.TargetSite IsNot Nothing Then sDesc &= "." & e.Exception.TargetSite.Name
              '    Else
              '      If e.Exception.TargetSite IsNot Nothing Then sDesc &= vbNewLine & " @ " & e.Exception.TargetSite.Name
              '    End If
              '  End If
              '  sDesc &= vbNewLine & "Version " & Windows.Forms.Application.ProductVersion.Trim
              '  sErrRep &= "%2526description=" & DoubleEncode(sDesc)
              '  MsgDlg(Nothing, sRet & vbNewLine & vbNewLine & "<a href=""" & sErrRep & """>Report the Error Manually</a>", "The error could not be reported.", "Error Report Failed!", MessageBoxButtons.OK, _TaskDialogIcon.InternetRJ45, MessageBoxIcon.Error, , , , , , True)
              'End If
            Case DialogResult.Ignore : e.ExitApplication = False
            Case DialogResult.Abort : e.ExitApplication = True
            Case DialogResult.Cancel : e.ExitApplication = False
          End Select
        End If
      Catch ex As Exception
        MsgBox("There was an error while handling another error." & vbNewLine & ex.ToString & vbNewLine & vbNewLine & "Original Error:" & vbNewLine & e.Exception.Message, MsgBoxStyle.Critical, "Error Report Error")
      End Try
      'Dim msg As String = "Time: " & Now.ToLongTimeString & ", Date: " & Now.ToLongDateString
      'If notifyIco Is Nothing Then
      '  notifyIco = New NotifyIcon With
      '    {
      '      .Icon = My.Resources.norm,
      '      .Text = "This message will disappear in 15 seconds...",
      '      .BalloonTipIcon = ToolTipIcon.Error,
      '      .BalloonTipTitle = "Error in Lime Seed"
      '    }
      'End If
      'notifyIco.Visible = True
      'Log.WriteException(e.Exception, TraceEventType.Error, msg)
      'e.ExitApplication = False
      'notifyIco.BalloonTipText = e.Exception.Message & vbNewLine & vbNewLine & "An entry has been added to the log."
      'notifyIco.ShowBalloonTip(15000)
      'notifyTimer = New Threading.Timer(New Threading.TimerCallback(AddressOf TimerTick), Nothing, 15000, 5000)
    End Sub

    'Private Sub TimerTick(state As Object)
    '  If notifyTimer IsNot Nothing Then
    '    notifyTimer = Nothing
    '    notifyIco.Visible = False
    '    notifyIco = Nothing
    '  End If
    'End Sub

#Region "Transfer System"
    Private WithEvents udpListener As UDPWrapper
    Private WithEvents tcpListener As TCPWrapper
    Private Sub TransferListen()
      Try
        udpListener = New UDPWrapper()
        udpListener.RemoteEndPoint = New Net.IPEndPoint(Net.IPAddress.Any, 57942)
        udpListener.Bind(New Net.IPEndPoint(Net.IPAddress.Any, 57941))
      Catch ex As Exception
        udpListener = Nothing
        Debug.Print("UDP Fail: " & ex.Message)
      End Try
      Try
        tcpListener = New TCPWrapper()
        tcpListener.RemoteEndPoint = New Net.IPEndPoint(Net.IPAddress.Any, 57941)
        tcpListener.Listen(New Net.IPEndPoint(Net.IPAddress.Any, 57941), 16)
      Catch ex As Exception
        tcpListener = Nothing
        Debug.Print("TCP Fail: " & ex.Message)
      End Try
    End Sub

    Private udpBuffer As Byte()
    Private Sub udpListener_SocketReceived(sender As Object, e As SocketReceivedEventArgs) Handles udpListener.SocketReceived
      Dim fromIP As Net.IPEndPoint = e.RemoteEndPoint
      Dim bIn As Byte() = e.Data
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
          If sIn.Substring(0, 4) = "LSMP" Then
            'Dim sServerData As String = sIn.Substring(5)
            'sServerData = sServerData.Substring(0, sServerData.Length - 1)
            'Dim sServerInfo() As String = sServerData.Split("/")

            Dim sStatus As String = "ACCEPT"
            If My.Forms.frmMain.cmdPlayPause.Enabled Then
              If My.Settings.SingleInstance Then
                sStatus = "OCCUPIED"
              Else
                sStatus = "ACCEPT"
              End If
            Else
              If My.Settings.SingleInstance Then
                sStatus = "ACTIVE"
              Else
                sStatus = "ACCEPT"
              End If
            End If
            Dim sOut As String = sStatus & "(" & Net.Dns.GetHostName & ")"
            sOut = BufferHex(sOut.Length, 4) & sOut
            Dim bOut As Byte() = System.Text.Encoding.GetEncoding(LATIN_1).GetBytes(sOut)
            udpListener.SendTo(fromIP, bOut)
          End If
        End If
      End If
    End Sub

    Private Sub tcpListener_SocketAccepted(sender As Object, e As SocketAcceptedEventArgs) Handles tcpListener.SocketAccepted
      Debug.Print("TCP Listener Accepted: " & e.Data.RemoteEndPoint.ToString)
    End Sub

    Private Sub tcpListener_SocketConnected(sender As Object, e As System.EventArgs) Handles tcpListener.SocketConnected
      Debug.Print("TCP Listener Connected")
    End Sub

    Private Sub tcpListener_SocketDisconnected(sender As Object, e As SocketErrorEventArgs) Handles tcpListener.SocketDisconnected
      If Threading.Thread.CurrentThread.GetApartmentState = Threading.ApartmentState.MTA Then
        For Each frm In OpenForms
          frm.BeginInvoke(New EventHandler(Of SocketErrorEventArgs)(AddressOf tcpListener_SocketDisconnected), sender, e)
          Exit For
        Next
      Else
        Debug.Print("TCP Listener Disconnected: " & e.ErrorDetails)
        Try
          tcpListener = New TCPWrapper()
          tcpListener.RemoteEndPoint = New Net.IPEndPoint(Net.IPAddress.Any, 57941)
          tcpListener.Listen(New Net.IPEndPoint(Net.IPAddress.Any, 57941), 16)
        Catch ex As Exception
          tcpListener = Nothing
          Debug.Print("TCP Fail: " & ex.Message)
        End Try
      End If
    End Sub

    Private Sub tcpListener_SocketReceived(sender As Object, e As SocketReceivedEventArgs) Handles tcpListener.SocketReceived
      If Threading.Thread.CurrentThread.GetApartmentState = Threading.ApartmentState.MTA Then
        For Each frm In OpenForms
          frm.BeginInvoke(New EventHandler(Of SocketReceivedEventArgs)(AddressOf tcpListener_SocketReceived), sender, e)
          Exit For
        Next
      Else
        Dim fromIP As Net.IPEndPoint = e.RemoteEndPoint
        Dim sIn As String = System.Text.Encoding.GetEncoding(LATIN_1).GetString(e.Data)
        If sIn.Substring(0, 8) = "TRANSFER" Then
          Dim bOut As Byte() = System.Text.Encoding.GetEncoding("latin1").GetBytes("TRANSFER_OK(" & Net.Dns.GetHostName & ")")
          tcpListener.Send(bOut)
        ElseIf sIn.Contains(vbLf) Then
          Dim sRows() As String = sIn.Split(vbLf)
          Dim iRowCount As Integer = Integer.Parse(sRows(0).Substring(0, 4), Globalization.NumberStyles.HexNumber)
          Dim sFiles As New List(Of String)
          For I As Integer = 1 To iRowCount
            Dim sRow As String = sRows(I)
            sRow = Replace(sRow, "%MUSIC%", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic))
            sRow = Replace(sRow, "%VIDEO%", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos))
            If My.Computer.FileSystem.FileExists(sRow) Then
              sFiles.Add("""" & sRow & """")
            Else
              sRow = sRows(I)
              sRow = Replace(sRow, "%MUSIC%", Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic))
              sRow = Replace(sRow, "%VIDEO%", Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos))
              If My.Computer.FileSystem.FileExists(sRow) Then sFiles.Add("""" & sRow & """")
            End If
          Next
          Dim iTrack As Integer = sRows(iRowCount + 1)
          Dim dTime As Double = sRows(iRowCount + 2)
          Dim uMS As Integer = Environment.TickCount
          'Debug.Print("Skip to track " & iTrack & " and play from " & dTime)
          If sFiles.Count > 0 Then
            If My.Settings.SingleInstance Then
              If My.Forms.frmMain IsNot Nothing Then
                Dim commands As New ObjectModel.ReadOnlyCollection(Of String)(sFiles)
                My.Forms.frmMain.StartupRun(commands)
                If iTrack > -1 Then
                  My.Forms.frmMain.dgvPlayList.Rows(0).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
                  My.Forms.frmMain.dgvPlayList.Rows(iTrack).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
                  Dim bMute As Boolean = My.Forms.frmMain.mpPlayer.Mute
                  My.Forms.frmMain.mpPlayer.Mute = True
                  My.Forms.frmMain.OpenFile(My.Forms.frmMain.dgvPlayList.Rows(iTrack).Tag(0), True)
                  My.Forms.frmMain.ThreadedInitial()
                  My.Forms.frmMain.ThreadedQueue()
                  dTime += (Environment.TickCount - uMS) / 1000
                  If dTime >= My.Forms.frmMain.mpPlayer.Duration Then
                    My.Forms.frmMain.mpPlayer.Position = dTime
                    My.Forms.frmMain.mpPlayer.Mute = bMute
                    My.Forms.frmMain.cmdNextPL.PerformClick()
                  ElseIf dTime = 0 Then
                    My.Forms.frmMain.mpPlayer.mpPause()
                    My.Forms.frmMain.mpPlayer.Position = 0
                    My.Forms.frmMain.mpPlayer.Mute = bMute
                    My.Forms.frmMain.cmdPlayPause.Image = My.Resources.button_play
                  Else
                    My.Forms.frmMain.mpPlayer.Position = dTime
                    My.Forms.frmMain.mpPlayer.Mute = bMute
                    My.Forms.frmMain.cmdPlayPause.Image = My.Resources.button_pause
                    My.Forms.frmMain.mpPlayer.StateFade = False
                    My.Forms.frmMain.mpPlayer.mpPlay()
                  End If
                End If
              End If
            Else
              If Not My.Forms.frmMain.cmdPlayPause.Enabled Then
                If My.Forms.frmMain IsNot Nothing Then
                  Dim commands As New ObjectModel.ReadOnlyCollection(Of String)(sFiles)
                  My.Forms.frmMain.StartupRun(commands)
                  If iTrack > -1 Then
                    My.Forms.frmMain.dgvPlayList.Rows(0).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
                    My.Forms.frmMain.dgvPlayList.Rows(iTrack).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
                    Dim bMute As Boolean = My.Forms.frmMain.mpPlayer.Mute
                    My.Forms.frmMain.mpPlayer.Mute = True
                    My.Forms.frmMain.OpenFile(My.Forms.frmMain.dgvPlayList.Rows(iTrack).Tag(0), True)
                    My.Forms.frmMain.ThreadedInitial()
                    My.Forms.frmMain.ThreadedQueue()
                    dTime += (Environment.TickCount - uMS) / 1000
                    If dTime >= My.Forms.frmMain.mpPlayer.Duration Then
                      My.Forms.frmMain.mpPlayer.Position = dTime
                      My.Forms.frmMain.mpPlayer.Mute = bMute
                      My.Forms.frmMain.cmdNextPL.PerformClick()
                    ElseIf dTime = 0 Then
                      My.Forms.frmMain.mpPlayer.mpPause()
                      My.Forms.frmMain.mpPlayer.Position = 0
                      My.Forms.frmMain.mpPlayer.Mute = bMute
                      My.Forms.frmMain.cmdPlayPause.Image = My.Resources.button_play
                    Else
                      My.Forms.frmMain.mpPlayer.Position = dTime
                      My.Forms.frmMain.mpPlayer.Mute = bMute
                      My.Forms.frmMain.cmdPlayPause.Image = My.Resources.button_pause
                      My.Forms.frmMain.mpPlayer.StateFade = False
                      My.Forms.frmMain.mpPlayer.mpPlay()
                    End If
                  End If
                End If
              Else
                instanceIterate += 1
                Dim frmTmp As New frmMain
                frmTmp.Tag = "NOCMD"
                frmTmp.instanceID = instanceIterate
                frmTmp.Show()
                frmTmp.tmrCommandCycle.Stop()
                Dim cTmp As New ObjectModel.Collection(Of String)
                If frmTmp.tmrCommandCycle.Tag IsNot Nothing Then cTmp = frmTmp.tmrCommandCycle.Tag
                frmTmp.tmrCommandCycle.Tag = cTmp
                frmTmp.tmrCommandCycle.Start()
                Dim commands As New ObjectModel.ReadOnlyCollection(Of String)(sFiles)
                frmTmp.StartupRun(commands)
                If iTrack > -1 Then
                  frmTmp.dgvPlayList.Rows(0).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
                  frmTmp.dgvPlayList.Rows(iTrack).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
                  Dim bMute As Boolean = frmTmp.mpPlayer.Mute
                  frmTmp.mpPlayer.Mute = True
                  frmTmp.OpenFile(frmTmp.dgvPlayList.Rows(iTrack).Tag(0), True)
                  frmTmp.ThreadedInitial()
                  frmTmp.ThreadedQueue()
                  dTime += (Environment.TickCount - uMS) / 1000
                  If dTime >= frmTmp.mpPlayer.Duration Then
                    frmTmp.mpPlayer.Position = dTime
                    frmTmp.mpPlayer.Mute = bMute
                    frmTmp.cmdNextPL.PerformClick()
                  ElseIf dTime = 0 Then
                    frmTmp.mpPlayer.mpPause()
                    frmTmp.mpPlayer.Position = 0
                    frmTmp.mpPlayer.Mute = bMute
                    frmTmp.cmdPlayPause.Image = My.Resources.button_play
                  Else
                    frmTmp.mpPlayer.Position = dTime
                    frmTmp.mpPlayer.Mute = bMute
                    frmTmp.cmdPlayPause.Image = My.Resources.button_pause
                    frmTmp.mpPlayer.StateFade = False
                    frmTmp.mpPlayer.mpPlay()
                  End If
                End If
              End If
            End If
            Dim bOut As Byte() = System.Text.Encoding.GetEncoding("latin1").GetBytes("PLAYING(" & Net.Dns.GetHostName & ")")
            tcpListener.Send(bOut)
          Else
            Dim bOut As Byte() = System.Text.Encoding.GetEncoding("latin1").GetBytes("NOT_FOUND(" & Net.Dns.GetHostName & ")")
            tcpListener.Send(bOut)
          End If
        End If
      End If
    End Sub
#End Region


  End Class
End Namespace

