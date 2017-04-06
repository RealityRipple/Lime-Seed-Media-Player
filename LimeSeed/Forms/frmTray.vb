Public Class frmTray
  Public ownerForm As frmMain
  Public Perma As Boolean
  Public GlassText As String
  Private sGlassText As String
  Private iActivated As Integer = 0
  Private Const ButtonTop As Integer = 12
  Private ButtonLeft As Integer() = {3, 32, 60, 90, 125}
  Private ButtonNorm_Play As Drawing.Image() = {My.Resources.traybutton_rw.ToBitmap, My.Resources.traybutton_play.ToBitmap, My.Resources.traybutton_stop.ToBitmap, My.Resources.traybutton_ff.ToBitmap, My.Resources.traybutton_restore.ToBitmap}
  Private ButtonNorm_Pause As Drawing.Image() = {My.Resources.traybutton_rw.ToBitmap, My.Resources.traybutton_pause.ToBitmap, My.Resources.traybutton_stop.ToBitmap, My.Resources.traybutton_ff.ToBitmap, My.Resources.traybutton_restore.ToBitmap}
  Private ButtonHover_Play As Drawing.Image() = {My.Resources.traybutton_rw_hover.ToBitmap, My.Resources.traybutton_play_hover.ToBitmap, My.Resources.traybutton_stop_hover.ToBitmap, My.Resources.traybutton_ff_hover.ToBitmap, My.Resources.traybutton_restore_hover.ToBitmap}
  Private ButtonHover_Pause As Drawing.Image() = {My.Resources.traybutton_rw_hover.ToBitmap, My.Resources.traybutton_pause_hover.ToBitmap, My.Resources.traybutton_stop_hover.ToBitmap, My.Resources.traybutton_ff_hover.ToBitmap, My.Resources.traybutton_restore_hover.ToBitmap}
  Private ButtonDisabled_Play As Drawing.Image() = {My.Resources.traybutton_rw_disabled.ToBitmap, My.Resources.traybutton_play_disabled.ToBitmap, My.Resources.traybutton_stop_disabled.ToBitmap, My.Resources.traybutton_ff_disabled.ToBitmap, My.Resources.traybutton_restore.ToBitmap}
  Private ButtonDisabled_Pause As Drawing.Image() = {My.Resources.traybutton_rw_disabled.ToBitmap, My.Resources.traybutton_pause_disabled.ToBitmap, My.Resources.traybutton_stop_disabled.ToBitmap, My.Resources.traybutton_ff_disabled.ToBitmap, My.Resources.traybutton_restore.ToBitmap}
  Private ReadOnly Property ButtonNorm As Drawing.Image()
    Get
      If IsPlaying Then Return ButtonNorm_Pause
      Return ButtonNorm_Play
    End Get
  End Property
  Private ReadOnly Property ButtonHover As Drawing.Image()
    Get
      If IsPlaying Then Return ButtonHover_Pause
      Return ButtonHover_Play
    End Get
  End Property
  Private ReadOnly Property ButtonDisabled As Drawing.Image()
    Get
      If IsPlaying Then Return ButtonDisabled_Pause
      Return ButtonDisabled_Play
    End Get
  End Property
  ReadOnly Property IsPlaying As Boolean
    Get
      Return CompareImages(ownerForm.cmdPlayPause.Image, My.Resources.button_pause)
    End Get
  End Property
  ReadOnly Property HoveringPlayPause As Boolean
    Get
      Return pctPlayPause.Tag IsNot Nothing
    End Get
  End Property
#Region "Form Events"
  Private Sub frmTray_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
    If ownerForm.cmdBackPL.Enabled Then
      pctRewind.Image = ButtonNorm(0)
    Else
      pctRewind.Image = ButtonDisabled(0)
    End If
    If ownerForm.cmdPlayPause.Enabled Then
      pctPlayPause.Image = ButtonNorm(1)
    Else
      pctPlayPause.Image = ButtonDisabled(1)
    End If
    If ownerForm.cmdStop.Enabled Then
      pctStop.Image = ButtonNorm(2)
    Else
      pctStop.Image = ButtonDisabled(2)
    End If
    If ownerForm.cmdNextPL.Enabled Then
      pctFastForward.Image = ButtonNorm(3)
    Else
      pctFastForward.Image = ButtonDisabled(3)
    End If
    pctRestore.Image = ButtonNorm(4)
    clsGlass.SetGlass(Me, -1, -1, -1, -1)
    iActivated = Environment.TickCount
    Me.Activate()
  End Sub
  Private Sub frmTray_Deactivate(sender As Object, e As System.EventArgs) Handles Me.Deactivate
    If Not Perma And iActivated > 0 Then
      If Environment.TickCount - iActivated > 30 Then
        Me.Close()
        Me.Dispose()
      End If
    End If
  End Sub
  Private Sub frmTray_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
    If e.Button = Windows.Forms.MouseButtons.Left Then ClickDrag(Me.Handle)
  End Sub
  Private Sub frmTray_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
    If Not String.IsNullOrEmpty(sGlassText) Then DrawGlassText(True)
  End Sub
#End Region
#Region "Button Events"
  Private Sub pctButton_MouseDown(sender As System.Object, e As MouseEventArgs) Handles pctRewind.MouseDown, pctPlayPause.MouseDown, pctStop.MouseDown, pctFastForward.MouseDown, pctRestore.MouseDown
    Dim pctButton As PictureBox = sender
    For I As Integer = 0 To ButtonLeft.Length - 1
      If Math.Abs(pctButton.Left - ButtonLeft(I)) < 3 Then
        If pctButton.Image IsNot ButtonDisabled(I) Then
          pctButton.Location = New Drawing.Point(ButtonLeft(I) + 1, ButtonTop + 1)
          pctButton.Image = ButtonHover(I)
        End If
        Exit For
      End If
    Next
  End Sub
  Private Sub pctButton_MouseUp(sender As System.Object, e As MouseEventArgs) Handles pctRewind.MouseUp, pctPlayPause.MouseUp, pctStop.MouseUp, pctFastForward.MouseUp, pctRestore.MouseUp
    Dim pctButton As PictureBox = sender
    For I As Integer = 0 To ButtonLeft.Length - 1
      If Math.Abs(pctButton.Left - ButtonLeft(I)) < 3 Then
        If pctButton.Image IsNot ButtonDisabled(I) Then
          pctButton.Location = New Drawing.Point(ButtonLeft(I), ButtonTop)
          pctButton.Image = ButtonHover(I)
        End If
        Exit For
      End If
    Next
  End Sub
  Private Sub pctButton_MouseEnter(sender As Object, e As System.EventArgs) Handles pctRewind.MouseEnter, pctPlayPause.MouseEnter, pctStop.MouseEnter, pctFastForward.MouseEnter, pctRestore.MouseEnter
    Dim pctButton As PictureBox = sender
    pctButton.Tag = New Object
    For I As Integer = 0 To ButtonLeft.Length - 1
      If Math.Abs(pctButton.Left - ButtonLeft(I)) < 3 Then
        If pctButton.Image IsNot ButtonDisabled(I) Then
          pctButton.Location = New Drawing.Point(ButtonLeft(I), ButtonTop)
          pctButton.Image = ButtonHover(I)
        End If
        Exit For
      End If
    Next
  End Sub
  Private Sub pctButton_MouseLeave(sender As Object, e As System.EventArgs) Handles pctRewind.MouseLeave, pctPlayPause.MouseLeave, pctStop.MouseLeave, pctFastForward.MouseLeave, pctRestore.MouseLeave
    Dim pctButton As PictureBox = sender
    pctButton.Tag = Nothing
    For I As Integer = 0 To ButtonLeft.Length - 1
      If Math.Abs(pctButton.Left - ButtonLeft(I)) < 3 Then
        If pctButton.Image IsNot ButtonDisabled(I) Then
          pctButton.Location = New Drawing.Point(ButtonLeft(I), ButtonTop)
          pctButton.Image = ButtonNorm(I)
        End If
        Exit For
      End If
    Next
  End Sub
  Private Sub pctRewind_Click(sender As System.Object, e As System.EventArgs) Handles pctRewind.Click
    If pctRewind.Image Is ButtonDisabled(0) Then Return
    ownerForm.cmdBackPL_Click(New Object, New EventArgs)
  End Sub
  Private Sub pctPlayPause_Click(sender As System.Object, e As System.EventArgs) Handles pctPlayPause.Click
    If pctPlayPause.Image Is ButtonDisabled(1) Then Return
    ownerForm.cmdPlayPause_Click(New Object, New EventArgs)
  End Sub
  Private Sub pctStop_Click(sender As System.Object, e As System.EventArgs) Handles pctStop.Click
    If pctStop.Image Is ButtonDisabled(2) Then Return
    ownerForm.cmdStop_Click(New Object, New EventArgs)
  End Sub
  Private Sub pctFastForward_Click(sender As System.Object, e As System.EventArgs) Handles pctFastForward.Click
    If pctFastForward.Image Is ButtonDisabled(3) Then Return
    ownerForm.cmdNextPL_Click(New Object, New EventArgs)
  End Sub
  Private Sub pctRestore_Click(sender As System.Object, e As System.EventArgs) Handles pctRestore.Click
    If Not ownerForm.WindowState = FormWindowState.Normal Then
      ownerForm.Show()
      ownerForm.WindowState = FormWindowState.Normal
      ownerForm.Top = (Me.Top + (Me.Height / 2)) - (ownerForm.Height / 2)
      ownerForm.Left = (Me.Left + (Me.Width / 2)) - (ownerForm.Width / 2)
      Dim scrBounds As Drawing.Rectangle = Screen.GetBounds(ownerForm.Location)
      If ownerForm.Left > scrBounds.Width - ownerForm.Width Then ownerForm.Left = scrBounds.Width - ownerForm.Width
      If ownerForm.Top > scrBounds.Height - ownerForm.Height Then ownerForm.Top = scrBounds.Height - ownerForm.Height
      If ownerForm.Left < scrBounds.Left Then ownerForm.Left = scrBounds.Left
      If ownerForm.Top < scrBounds.Top Then ownerForm.Top = scrBounds.Top
    Else
      ownerForm.Top = (Me.Top + (Me.Height / 2)) - (ownerForm.Height / 2)
      ownerForm.Left = (Me.Left + (Me.Width / 2)) - (ownerForm.Width / 2)
      Dim scrBounds As Drawing.Rectangle = Screen.GetBounds(ownerForm.Location)
      If ownerForm.Left > scrBounds.Width - ownerForm.Width Then ownerForm.Left = scrBounds.Width - ownerForm.Width
      If ownerForm.Top > scrBounds.Height - ownerForm.Height Then ownerForm.Top = scrBounds.Height - ownerForm.Height
      If ownerForm.Left < scrBounds.Left Then ownerForm.Left = scrBounds.Left
      If ownerForm.Top < scrBounds.Top Then ownerForm.Top = scrBounds.Top
      ownerForm.Show()
    End If
    ownerForm.trayIcon.Visible = False
    Me.Close()
    Me.Dispose()
    ownerForm.Activate()
  End Sub
#End Region
#Region "Glass Text Functions"
  Private glassText_Value As String
  Private glassText_X As Integer
  Private glassText_Left As Boolean
  Private Sub DrawGlassText(autoRedraw As Boolean, Optional newText As String = Nothing)
    Const BarTop As Integer = 28
    Const BarHeight As Integer = 32
    Const EdgeMargin As Integer = 8
    Dim TextFont As New Drawing.Font(Drawing.SystemFonts.CaptionFont.Name, 8)
    Const GlowSize As Integer = 12
    If Not String.IsNullOrEmpty(newText) Then glassText_Value = newText
    If Not clsGlass.IsCompositionEnabled Then
      ownerForm.Show()
      ownerForm.trayIcon.Visible = False
      Me.Close()
      Me.Dispose()
      ownerForm.Activate()
      Return
    End If
    Dim iTextWidth As Integer = CreateGraphics.MeasureString(glassText_Value, TextFont).Width + (EdgeMargin * 3)
    If iTextWidth <= Me.DisplayRectangle.Width Then
      Dim SimpleRect As New Drawing.Rectangle(0, BarTop, Me.DisplayRectangle.Width, BarHeight)
      clsGlass.DrawTextOnGlass(Me.Handle, glassText_Value, TextFont, SimpleRect, EdgeMargin, GlowSize)
      Return
    End If
    If autoRedraw Then
      Dim AutoRect As New Drawing.Rectangle(glassText_X, BarTop, Me.DisplayRectangle.Width - glassText_X, BarHeight)
      clsGlass.DrawTextOnGlass(Me.Handle, glassText_Value, TextFont, AutoRect, EdgeMargin, GlowSize)
      Return
    End If
    If glassText_X >= 0 And Not glassText_Left Then
      glassText_Left = True
      glassText_X = 0
    ElseIf glassText_X <= Me.DisplayRectangle.Width - iTextWidth And glassText_Left Then
      glassText_Left = False
      glassText_X = Me.DisplayRectangle.Width - iTextWidth
    Else
      Dim iExtra As Integer = Me.DisplayRectangle.Width - iTextWidth
      Dim iStep As Integer
      If iExtra >= -5 Then
        iStep = 0
      ElseIf iExtra >= -10 Then
        iStep = 1
      ElseIf iExtra >= -50 Then
        iStep = 2
      Else
        iStep = 3
      End If
      If glassText_Left Then
        glassText_X -= iStep
      Else
        glassText_X += iStep
      End If
    End If
    If glassText_X > 0 Then glassText_X = 0
    If glassText_X < Me.DisplayRectangle.Width - iTextWidth Then glassText_X = Me.DisplayRectangle.Width - iTextWidth
    Dim MovingRect As New Drawing.Rectangle(glassText_X, BarTop, Me.DisplayRectangle.Width - glassText_X, BarHeight)
    clsGlass.DrawTextOnGlass(Me.Handle, glassText_Value, TextFont, MovingRect, EdgeMargin, GlowSize)
  End Sub
  Private Sub tmrUpdate_Tick(sender As System.Object, e As System.EventArgs) Handles tmrUpdate.Tick
    If String.IsNullOrEmpty(GlassText) OrElse GlassText = My.Application.Info.Title Then
      sGlassText = Nothing
      Me.Invalidate()
    Else
      If GlassText = sGlassText Then
        DrawGlassText(False)
      Else
        DrawGlassText(False, GlassText)
        sGlassText = GlassText
      End If
    End If
    If ownerForm.cmdBackPL.Enabled Then
      If pctRewind.Tag Is Nothing Then
        pctRewind.Image = ButtonNorm(0)
      Else
        pctRewind.Image = ButtonHover(0)
      End If
    Else
      pctRewind.Image = ButtonDisabled(0)
    End If
    If ownerForm.cmdPlayPause.Enabled Then
      If pctPlayPause.Tag Is Nothing Then
        pctPlayPause.Image = ButtonNorm(1)
      Else
        pctPlayPause.Image = ButtonHover(1)
      End If
    Else
      pctPlayPause.Image = ButtonDisabled(1)
    End If

    If ownerForm.cmdStop.Enabled Then
      If pctStop.Tag Is Nothing Then
        pctStop.Image = ButtonNorm(2)
      Else
        pctStop.Image = ButtonHover(2)
      End If
    Else
      pctStop.Image = ButtonDisabled(2)
    End If

    If ownerForm.cmdNextPL.Enabled Then
      If pctFastForward.Tag Is Nothing Then
        pctFastForward.Image = ButtonNorm(3)
      Else
        pctFastForward.Image = ButtonHover(3)
      End If
    Else
      pctFastForward.Image = ButtonDisabled(3)
    End If

    If pctRestore.Tag Is Nothing Then
      pctRestore.Image = ButtonNorm(4)
    Else
      pctRestore.Image = ButtonHover(4)
    End If

    pbProgress.Style = ownerForm.pbProgress.Style
    If pbProgress.Style = ProgressBarStyle.Marquee Then
      If pbProgress.Maximum < 1 Then pbProgress.Maximum = 1
      pbProgress.Value = 1
      pbProgress.Maximum = 100
    Else
      If Not pbProgress.Maximum = ownerForm.pbProgress.Maximum Then
        If pbProgress.Value > pbProgress.Maximum Then pbProgress.Value = ownerForm.pbProgress.Value
        pbProgress.Maximum = ownerForm.pbProgress.Maximum
      End If
      If Not pbProgress.Value = ownerForm.pbProgress.Value Then pbProgress.Value = ownerForm.pbProgress.Value
    End If
    Dim nowStr As String = ownerForm.TimeString
    If Not ttInfo.GetToolTip(pbProgress) = nowStr Then ttInfo.SetToolTip(pbProgress, nowStr)
  End Sub
#End Region
End Class