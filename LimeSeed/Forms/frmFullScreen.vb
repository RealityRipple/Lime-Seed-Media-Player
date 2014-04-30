Public Class frmFullScreen
  Private VolS, SeekS, SeekPlay As Boolean
  Public ParentPlayer As frmMain

  Private Sub tmrHide_Tick(sender As System.Object, e As System.EventArgs) Handles tmrHide.Tick
    Static lastPosition As Drawing.Point, lastTime As Integer
    If frmMain.mnuVideo.Visible Or frmMain.mnuAlbumArt.Visible Then
      tmrHide.Enabled = False
      tmrHide.Tag = 2
      SetVisuals(False)
      Exit Sub
    End If
    If Me.DesktopBounds.Contains(MousePosition) Then
      If lastTime = 0 Or (Math.Abs(MousePosition.X - lastPosition.X) > 3 Or Math.Abs(MousePosition.Y - lastPosition.Y) > 3) Then
        lastPosition = MousePosition
        lastTime = Environment.TickCount
      End If
      If Environment.TickCount - lastTime > 2500 And cmdPlayPause.Top < pctVideo.Height Then 'And cmdPlayPause.Visible Then
        tmrHide.Enabled = False
        tmrHide.Tag = 2
        SetVisuals(True)
      ElseIf Environment.TickCount - lastTime <= 2500 And cmdPlayPause.Top > pctVideo.Height - 26 Then 'And Not cmdPlayPause.Visible Then
        If ParentPlayer.mpPlayer.State = Seed.ctlSeed.MediaState.mPlaying Then
          If MousePosition.Y > (Me.DesktopBounds.Top + Me.DesktopBounds.Height) - (Me.DesktopBounds.Height / 4) Then
            tmrHide.Enabled = False
            tmrHide.Tag = 0
            SetVisuals(False)
          Else
            tmrHide.Enabled = False
            tmrHide.Tag = 2
            SetVisuals(True)
          End If
        Else
          tmrHide.Enabled = False
          tmrHide.Tag = 0
          SetVisuals(False)
        End If
      ElseIf Environment.TickCount - lastTime <= 2500 And cmdPlayPause.Top = pctVideo.Height - 26 And ParentPlayer.mpPlayer.State = Seed.ctlSeed.MediaState.mPlaying Then
        If MousePosition.Y <= (Me.DesktopBounds.Top + Me.DesktopBounds.Height) - (Me.DesktopBounds.Height / 4) Then
          tmrHide.Enabled = False
          tmrHide.Tag = 2
          SetVisuals(True)
        End If
      End If
    Else
      If cmdPlayPause.Top < pctVideo.Height Then
        tmrHide.Enabled = False
        tmrHide.Tag = 1
        SetVisuals(True)
        SetCursor(True)
      Else
        SetCursor(True)
      End If
    End If
  End Sub

  Private Sub EnableHide()
    If Me.InvokeRequired Then
      Me.Invoke(New MethodInvoker(AddressOf EnableHide))
    Else
      tmrHide.Enabled = True
    End If
  End Sub

  Public Sub SetVisuals(Hide As Boolean)
    If FastPC() And clsGlass.IsCompositionEnabled Then
      tmrScroll.Interval = 25
      tmrScroll.Tag = Hide
      If Hide Then
        If ParentPlayer.mpPlayer.State <> Seed.ctlSeed.MediaState.mPlaying Then
          If Me.Visible Then EnableHide()
          Exit Sub
        End If
        If Me.Visible Then SetCursor(False)
      Else
        If Me.Visible Then SetCursor(True)
        cmdPlayPause.Visible = True
        cmdStop.Visible = True
        cmdFullScreen.Visible = True
        cmdMute.Visible = True
        pbProgress.Visible = True
        bpgVolume.Visible = True
      End If
      tmrScroll.Start()
    Else
      If Me.Visible Then SetCursor(Not Hide)
      cmdPlayPause.Visible = Not Hide
      If Hide Then
        cmdPlayPause.Top = pctVideo.Height + 1
      Else
        cmdPlayPause.Top = pctVideo.Height - 26
      End If
      cmdStop.Visible = Not Hide
      cmdFullScreen.Visible = Not Hide
      cmdMute.Visible = Not Hide
      pbProgress.Visible = Not Hide
      bpgVolume.Visible = Not Hide
      If Me.Visible Then EnableHide()
    End If
  End Sub

  Private Sub cmdPlayPause_Click(sender As System.Object, e As System.EventArgs) Handles cmdPlayPause.Click
    ParentPlayer.cmdPlayPause.PerformClick()
  End Sub

  Private Sub cmdStop_Click(sender As System.Object, e As System.EventArgs) Handles cmdStop.Click
    ParentPlayer.cmdStop.PerformClick()
  End Sub

  Private Sub cmdFullScreen_Click(sender As System.Object, e As System.EventArgs) Handles cmdFullScreen.Click
    ParentPlayer.cmdFullScreen.PerformClick()
  End Sub

  Private Sub cmdMute_Click(sender As System.Object, e As System.EventArgs) Handles cmdMute.Click
    ParentPlayer.cmdMute.PerformClick()
  End Sub

  Private Sub pbProgress_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pbProgress.MouseDown
    If e.Button = Windows.Forms.MouseButtons.Left And Not SeekS Then
      pbProgress.Value = (e.X / pbProgress.Width) * pbProgress.Maximum
      ParentPlayer.mpPlayer.Position = pbProgress.Value / 1000
      SeekS = True
      SeekPlay = ParentPlayer.mpPlayer.State = Seed.ctlSeed.MediaState.mPlaying
      ParentPlayer.mpPlayer.StateFade = False
      If SeekPlay Then ParentPlayer.mpPlayer.mpPause()
    End If
  End Sub

  Private Sub pbProgress_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pbProgress.MouseMove
    If SeekS And e.Button = Windows.Forms.MouseButtons.Left Then
      If e.X > 0 And e.X < pbProgress.Width Then
        pbProgress.Value = (e.X / pbProgress.Width) * pbProgress.Maximum
        ParentPlayer.mpPlayer.Position = pbProgress.Value / 1000
      End If
    End If
    If e.X > 0 And e.X < pbProgress.Width Then
      Dim sTime As String = ConvertTimeVal(((e.X - 1) / (pbProgress.Width - 2)) * pbProgress.Maximum / 1000)
      If ttDisp.GetToolTip(pbProgress) <> sTime Then ttDisp.SetToolTip(pbProgress, sTime)
    End If
  End Sub

  Private Sub pbProgress_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pbProgress.MouseUp
    If SeekS And e.Button = Windows.Forms.MouseButtons.Left Then
      If e.X > 0 And e.X < pbProgress.Width Then
        pbProgress.Value = (e.X / pbProgress.Width) * pbProgress.Maximum
        ParentPlayer.mpPlayer.Position = pbProgress.Value / 1000
        ParentPlayer.mpPlayer.StateFade = False
        If SeekPlay Then ParentPlayer.mpPlayer.mpPlay()
      End If
    End If
    SeekS = False
  End Sub

  Private Sub bpgVolume_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles bpgVolume.MouseDown
    If e.Button = Windows.Forms.MouseButtons.Left And Not VolS Then
      bpgVolume.Value = ((e.X + 1) / (bpgVolume.Width - 2)) * bpgVolume.Maximum
      ParentPlayer.bpgVolume.Value = bpgVolume.Value
      VolS = True
    End If
  End Sub

  Private Sub bpgVolume_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles bpgVolume.MouseMove
    If VolS And e.Button = Windows.Forms.MouseButtons.Left Then
      If e.X > 0 And e.X < bpgVolume.Width Then
        bpgVolume.Value = ((e.X + 1) / (bpgVolume.Width - 2)) * bpgVolume.Maximum
        ParentPlayer.bpgVolume.Value = bpgVolume.Value
      End If
    End If
    If e.X > 0 And e.X < bpgVolume.Width Then
      Dim dVol As Double = ((e.X - 1) / (bpgVolume.Width - 2)) * bpgVolume.Maximum
      Dim sPercent As String = "Volume: " & Math.Floor(dVol) - bpgVolume.Maximum & " dB"
      If ttDisp.GetToolTip(bpgVolume) <> sPercent Then ttDisp.SetToolTip(bpgVolume, sPercent)
    End If
  End Sub

  Private Sub bpgVolume_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles bpgVolume.MouseUp
    If VolS And e.Button = Windows.Forms.MouseButtons.Left Then
      If e.X > 0 And e.X < bpgVolume.Width Then
        bpgVolume.Value = ((e.X + 1) / (bpgVolume.Width - 2)) * bpgVolume.Maximum
        ParentPlayer.bpgVolume.Value = bpgVolume.Value
      End If
    End If
    VolS = False
  End Sub

  Private Sub pctVideo_DoubleClick(sender As Object, e As System.EventArgs) Handles pctVideo.DoubleClick
    ParentPlayer.cmdFullScreen.PerformClick()
  End Sub

  Private Sub frmFullScreen_MouseWheel(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseWheel, pctVideo.MouseWheel
    If e.Delta > 0 Then
      bpgVolume.Value += (bpgVolume.Maximum - bpgVolume.Minimum) / 20
    ElseIf e.Delta < 0 Then
      bpgVolume.Value -= (bpgVolume.Maximum - bpgVolume.Minimum) / 20
    End If
    ParentPlayer.bpgVolume.Value = bpgVolume.Value
  End Sub

  Private Sub frmFullScreen_VisibleChanged(sender As Object, e As System.EventArgs) Handles Me.VisibleChanged
    If Me.Visible Then
      Dim ppLoc As Drawing.Point
      ppLoc.X = ParentPlayer.Location.X + (ParentPlayer.Width / 2)
      ppLoc.Y = ParentPlayer.Location.Y + (ParentPlayer.Height / 2)
      Dim xScreen = Screen.GetBounds(ppLoc)
      Me.WindowState = FormWindowState.Normal
      Me.Location = xScreen.Location
      Me.Size = xScreen.Size
      Me.WindowState = FormWindowState.Maximized
      If Not bpgVolume.Value = ParentPlayer.bpgVolume.Value Then bpgVolume.Value = ParentPlayer.bpgVolume.Value
      cmdPlayPause.Top = pctVideo.Height - 26
      cmdStop.Top = pctVideo.Height - 26
      cmdFullScreen.Top = pctVideo.Height - 26
      cmdMute.Top = pctVideo.Height - 26
      pbProgress.Top = pctVideo.Height - 22
      bpgVolume.Top = pctVideo.Height - 27
    End If
    tmrHide.Enabled = Me.Visible
    tmrUpdate.Enabled = Me.Visible
    pctVideo.AllowDrop = True
  End Sub

  Private Sub tmrUpdate_Tick(sender As System.Object, e As System.EventArgs) Handles tmrUpdate.Tick
    If Not cmdPlayPause.Enabled = ParentPlayer.cmdPlayPause.Enabled Then cmdPlayPause.Enabled = ParentPlayer.cmdPlayPause.Enabled
    If Not cmdPlayPause.Image.GetHashCode = ParentPlayer.cmdPlayPause.Image.GetHashCode Then cmdPlayPause.Image = ParentPlayer.cmdPlayPause.Image
    If Not cmdStop.Enabled = ParentPlayer.cmdStop.Enabled Then cmdStop.Enabled = ParentPlayer.cmdStop.Enabled
    If Not cmdMute.Image.GetHashCode = ParentPlayer.cmdMute.Image.GetHashCode Then cmdMute.Image = ParentPlayer.cmdMute.Image
    If Not pbProgress.Enabled = ParentPlayer.pbProgress.Enabled Then pbProgress.Enabled = ParentPlayer.pbProgress.Enabled
    If Not pbProgress.Maximum = ParentPlayer.pbProgress.Maximum Then pbProgress.Maximum = ParentPlayer.pbProgress.Maximum
    If Not SeekS Then If Not pbProgress.Value = ParentPlayer.pbProgress.Value Then pbProgress.Value = ParentPlayer.pbProgress.Value
    If ParentPlayer.mpPlayer.HasVid Then
      If Not pctVideo.ContextMenuStrip Is ParentPlayer.mnuVideo Then pctVideo.ContextMenuStrip = ParentPlayer.mnuVideo
    Else
      If Not pctVideo.ContextMenuStrip Is ParentPlayer.mnuAlbumArt Then pctVideo.ContextMenuStrip = ParentPlayer.mnuAlbumArt
    End If
  End Sub

  Private Sub tmrScroll_Tick(sender As Object, e As System.EventArgs) Handles tmrScroll.Tick
    If tmrScroll.Tag Then
      If cmdPlayPause.Top <= pctVideo.Height Then
        cmdPlayPause.Top += 2
        cmdStop.Top += 2
        cmdFullScreen.Top += 2
        cmdMute.Top += 2
        pbProgress.Top += 2
        bpgVolume.Top += 2
      End If
      If Me.DesktopBounds.Contains(MousePosition) And tmrHide.Tag < 2 Then
        tmrScroll.Stop()
        cmdPlayPause.Refresh()
        cmdStop.Refresh()
        cmdFullScreen.Refresh()
        cmdMute.Refresh()
        pbProgress.Refresh()
        bpgVolume.Refresh()
        EnableHide()
        Exit Sub
      End If
      If cmdPlayPause.Top > pctVideo.Height Then
        tmrScroll.Stop()
        cmdPlayPause.Visible = False
        cmdStop.Visible = False
        cmdFullScreen.Visible = False
        cmdMute.Visible = False
        pbProgress.Visible = False
        bpgVolume.Visible = False
        EnableHide()
      End If
      pctVideo.Invalidate()
      pbProgress.Invalidate()
    Else
      If cmdPlayPause.Top > pctVideo.Height - cmdPlayPause.Height - 2 Then
        cmdPlayPause.Top -= 4
        cmdStop.Top -= 4
        cmdFullScreen.Top -= 4
        cmdMute.Top -= 4
        pbProgress.Top -= 4
        bpgVolume.Top -= 4
      End If
      If Not Me.DesktopBounds.Contains(MousePosition) Then
        tmrScroll.Stop()
        cmdPlayPause.Refresh()
        cmdStop.Refresh()
        cmdFullScreen.Refresh()
        cmdMute.Refresh()
        pbProgress.Refresh()
        bpgVolume.Refresh()
        EnableHide()
        Exit Sub
      End If
      If cmdPlayPause.Top <= pctVideo.Height - cmdPlayPause.Height - 2 Then
        tmrScroll.Stop()
        cmdPlayPause.Top = pctVideo.Height - 26
        cmdStop.Top = pctVideo.Height - 26
        cmdFullScreen.Top = pctVideo.Height - 26
        cmdMute.Top = pctVideo.Height - 26
        pbProgress.Top = pctVideo.Height - 22
        bpgVolume.Top = pctVideo.Height - 27
        EnableHide()
      End If
      pctVideo.Invalidate()
      pbProgress.Invalidate()
      End If
  End Sub

  Private Sub pctVideo_DragDrop(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles pctVideo.DragDrop
    ParentPlayer.DragDropEvent(sender, e)
  End Sub

  Private Sub pctVideo_DragEnter(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles pctVideo.DragEnter
    ParentPlayer.DragEnterEvent(sender, e)
  End Sub

  Private Sub pctVideo_DragOver(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles pctVideo.DragOver
    ParentPlayer.DragOverEvent(sender, e)
  End Sub
End Class
