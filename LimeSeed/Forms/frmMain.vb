Imports System.Runtime.InteropServices

Public Class frmMain
  Private VolS, SeekS, VidThumb, SeekPlay As Boolean
  Private AudWidth, VidWidth, ChapterWidth, SubWidth As Single
  Private sourceRowIndex As Integer = -1
  Private FileTitle As String
  Private FileMainTitle As String
  Private FileSubTitle As String
  Private mFArt As ImageWithName
  Private bgArt As ImageWithName
  Private VidSize As Drawing.Size
  Private CorrectedSize As Drawing.Size
  Private ffAPI As FFDShowAPI.FFDShowAPI = Nothing
  Private taskBar As TaskbarLib.TaskbarList
  Private frmFS As frmFullScreen
  Private cCD As Seed.clsAudioCD
  Private bCD, bDVD As Boolean

  Private Const UNKNOWN_ALBUM As String = "Unknown Album"
  Private Const UNKNOWN_ARTIST As String = "Unknown Artist"
  Private Const MAIN_HEIGHT As Integer = 108
  Private Const MAIN_WIDTH As Integer = 320
  Private Const TOPMOST_MENU_ID As Int64 = &H4815
  Private Const TOPMOST_MENU_TEXT As String = "&Topmost"

  Private WithEvents cTask As TaskbarController
  Private WithEvents macArt As AppleNet
  Private WithEvents joyPad As clsJoyDetection
  Private WithEvents getAlbumInfo As AlbumInfo

  Private Delegate Sub CallBack(Obj As Object)
  Private FirstInit As Boolean = False
  Private volDevice As CoreAudioApi.MMDevice
  Private volControl As New Sound

  Private objDraw As Object
  Private methodDraw As System.Reflection.MethodInfo
  Private NoGlassText As Boolean = True
  <System.Security.SuppressUnmanagedCodeSecurity()>
  Friend Class NativeMethods
    Public Const WM_SYSCOMMAND As Integer = &H112
    <Flags()> _
    Public Enum MenuFlags As Integer
      MF_BYCOMMAND = &H0
      MF_BYPOSITION = &H400
      MF_BITMAP = &H4
      MF_CHECKED = &H8
      MF_DISABLED = &H2
      MF_ENABLED = &H0
      MF_GRAYED = &H1
      MF_MENUBARBREAK = &H20
      MF_MENUBREAK = &H40
      MF_OWNERDRAW = &H100
      MF_POPUP = &H10
      MF_SEPARATOR = &H800
      MF_STRING = &H0
      MF_UNCHECKED = &H0
    End Enum
    <DllImport("user32", CharSet:=CharSet.Auto, setlasterror:=True)>
    Public Shared Function GetSystemMenu(hWnd As IntPtr, bRevert As Boolean) As IntPtr
    End Function
    <DllImport("user32", CharSet:=CharSet.Auto, setlasterror:=True)>
    Public Shared Function AppendMenu(hMenu As IntPtr, uFlags As Integer, uIDNewItem As Integer, lpNewItem As String) As IntPtr
    End Function
    <DllImport("user32", CharSet:=CharSet.Auto, setlasterror:=True)>
    Public Shared Function InsertMenu(hMenu As IntPtr, uPosition As Integer, uFlags As Integer, uIDNewItem As Integer, lpNewItem As String) As Boolean
    End Function
    <DllImport("user32", CharSet:=CharSet.Auto, setlasterror:=True)>
    Public Shared Function ModifyMenu(hMenu As IntPtr, uPosition As Integer, uFlags As Integer, uIDNewItem As Integer, lpNewItem As String) As Boolean
    End Function
  End Class

#Region "Main Controls"
#Region "Progress"
  Private Sub pbProgress_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pbProgress.MouseDown
    If e.Button = Windows.Forms.MouseButtons.Left And Not SeekS Then
      pbProgress.Value = ((e.X - 1) / (pbProgress.Width - 2)) * pbProgress.Maximum
      If bCD Then
        cCD.MovePositionMS(pbProgress.Value)
        SeekS = True
        'SeekPlay = cCD.Status = Seed.clsAudioCD.PlayStatus.Playing
        'If SeekPlay Then cCD.Pause()
      Else
        SeekPlay = mpPlayer.State = Seed.ctlSeed.MediaState.mPlaying
        mpPlayer.Position = pbProgress.Value / 1000
        SeekS = True
        mpPlayer.StateFade = False
        If SeekPlay Then mpPlayer.mpPause()
      End If

    End If
  End Sub

  Private Sub pbProgress_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pbProgress.MouseMove
    If SeekS And e.Button = Windows.Forms.MouseButtons.Left Then
      If e.X > 0 And e.X < pbProgress.Width Then
        pbProgress.Value = ((e.X - 1) / (pbProgress.Width - 2)) * pbProgress.Maximum
        If bCD Then
          cCD.MovePositionMS(pbProgress.Value)
        Else
          mpPlayer.Position = pbProgress.Value / 1000
        End If
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
        pbProgress.Value = ((e.X - 1) / (pbProgress.Width - 2)) * pbProgress.Maximum
        If bCD Then
          cCD.MovePositionMS(pbProgress.Value)
          'If SeekPlay Then cCD.Play()
        Else
          mpPlayer.Position = pbProgress.Value / 1000
          mpPlayer.StateFade = False
          If SeekPlay Then mpPlayer.mpPlay()
          mpPlayer.Invalidate()
        End If
      End If
    End If
    SeekS = False
  End Sub
#End Region

#Region "Buttons"
  Private Sub cmdPlayPause_Click(sender As System.Object, e As System.EventArgs) Handles cmdPlayPause.Click
    If bCD Then
      If cCD.Status = Seed.clsAudioCD.PlayStatus.Playing Then
        cmdPlayPause.Image = My.Resources.button_play
        cCD.Pause()
      Else
        cmdPlayPause.Image = My.Resources.button_pause
        cCD.Play()
      End If
    Else
      If mpPlayer.State = Seed.ctlSeed.MediaState.mPlaying Then
        cmdPlayPause.Image = My.Resources.button_play
        mpPlayer.StateFade = True
        mpPlayer.mpPause()
      ElseIf mpPlayer.State = Seed.ctlSeed.MediaState.mPaused Or mpPlayer.State = Seed.ctlSeed.MediaState.mStopped Then
        cmdPlayPause.Image = My.Resources.button_pause
        mpPlayer.StateFade = True
        mpPlayer.mpPlay()
      End If
    End If
  End Sub

  Private Sub cmdStop_Click(sender As System.Object, e As System.EventArgs) Handles cmdStop.Click
    If bCD Then
      cmdPlayPause.Image = My.Resources.button_play
      cCD.Stop()
    Else
      mpPlayer.StateFade = True
      cmdPlayPause.Image = My.Resources.button_play
      mpPlayer.mpStop()
    End If
  End Sub

  Private Sub cmdFullScreen_Click(sender As System.Object, e As System.EventArgs) Handles cmdFullScreen.Click
    'If FastPC() And Not bDVD And clsGlass.IsCompositionEnabled Then
    '  Dim tmrFade As New Timer
    '  tmrFade.Interval = 25
    '  If frmFS.Visible Then
    '    frmFS.Opacity = 1
    '    Me.Opacity = 1
    '    AddHandler tmrFade.Tick, Sub()
    '                               frmFS.Opacity -= 0.1
    '                               If frmFS.Opacity <= 0.2 And mpPlayer.FullScreen Then
    '                                 mpPlayer.FullScreen = False
    '                                 mpPlayer.FullScreenObj = Nothing
    '                                 frmFS.pctVideo.Image = Nothing
    '                               End If
    '                               If frmFS.Opacity = 0 Then
    '                                 tmrFade.Stop()
    '                                 frmFS.Hide()
    '                                 Me.Opacity = 1
    '                                 If cTask IsNot Nothing Then
    '                                   cTask.CreatePreview(mpPlayer)
    '                                   VidThumb = True
    '                                 Else
    '                                   VidThumb = False
    '                                 End If
    '                                 SetCursor(True)
    '                               End If
    '                             End Sub
    '  Else
    '    frmFS.Opacity = 0
    '    Me.Opacity = 1
    '    frmFS.Show(Me)
    '    AddHandler tmrFade.Tick, Sub()
    '                               frmFS.Opacity += 0.1
    '                               If frmFS.Opacity >= 0.8 And Not mpPlayer.FullScreen Then
    '                                 mpPlayer.FullScreenObj = frmFS.pctVideo
    '                                 mpPlayer.FullScreen = True
    '                                 If cTask IsNot Nothing Then cTask.CreatePreview(pnlMain)
    '                                 VidThumb = False
    '                               End If
    '                               If frmFS.Opacity = 1 Then
    '                                 Me.Opacity = 0
    '                                 tmrFade.Stop()
    '                               End If
    '                             End Sub
    '  End If
    '  tmrFade.Start()
    'Else
    If frmFS.Visible Then
      mpPlayer.FullScreen = False
      mpPlayer.FullScreenObj = Nothing
      Me.Opacity = 1
      frmFS.Hide()
      If cTask IsNot Nothing Then
        If mpPlayer.HasVid Then cTask.CreatePreview(mpPlayer)
        VidThumb = True
      Else
        VidThumb = False
      End If
      SetCursor(True)
    Else
      If cTask IsNot Nothing Then
        If mpPlayer.HasVid Then
          'If mpPlayer.VideoWidth <= 950 Then
          '  Dim bmpThumb = mpPlayer.GetFileThumbnail(mpPlayer.FileName, False, False)
          '  If bmpThumb IsNot Nothing Then
          '    cTask.CreatePreview(bmpThumb)
          '  Else
          '    cTask.CreatePreview(My.Resources.Logo)
          '  End If
          'Else
          cTask.CreatePreview(My.Resources.Logo)
          'End If
        End If
      End If
      frmFS.Show(Me)
      mpPlayer.FullScreenObj = frmFS.pctVideo
      mpPlayer.FullScreen = True
      Me.Opacity = 0
      VidThumb = False
    End If
    'End If
  End Sub

  Private Sub cmdLoop_Click(sender As System.Object, e As System.EventArgs) Handles cmdLoop.Click
    If bCD Or bDVD Then
      If bDVD Then
        Using tmpCD = New Seed.clsAudioCD
          tmpCD.ChangeDrive(mpPlayer.FileName(0))
          tmpCD.Eject()
        End Using
        bDVD = False
      Else
        cCD.Eject()
        cCD = Nothing
        bCD = False
      End If
      mnuCloseFile.PerformClick()
      cmdShufflePL.Tag = False
      cmdShufflePL.Image = My.Resources.pl_button_order
      dgvPlayList.Rows.Clear()
    Else
      If mpPlayer.Repeat Then
        mpPlayer.Repeat = False
        cmdLoop.Image = My.Resources.button_loop_off
      Else
        mpPlayer.Repeat = True
        cmdLoop.Image = My.Resources.button_loop_on
      End If
    End If
  End Sub

  Private Sub cmdMenu_Click(sender As System.Object, e As System.EventArgs) Handles cmdMenu.Click
    If bDVD Then
      mpPlayer.DVDMenu(DirectShowLib.Dvd.DvdMenuId.Root)
    End If
  End Sub

  Private Sub cmdMute_Click(sender As System.Object, e As System.EventArgs) Handles cmdMute.Click
    If cmdMute.Tag = True Then
      cmdMute.Tag = False
      cmdMute.Image = My.Resources.button_nomute
      If bCD Or mpPlayer.IsFlash Then volControl.SetSound(False) Else mpPlayer.Mute = False
    Else
      cmdMute.Tag = True
      cmdMute.Image = My.Resources.button_mute
      If bCD Or mpPlayer.IsFlash Then volControl.SetSound(True) Else mpPlayer.Mute = True
    End If
  End Sub

  Private Sub pctBeat_Click(sender As System.Object, e As System.EventArgs) Handles pctBeat.Click
    pctBeat.Tag = Not pctBeat.Tag
    My.Settings.BeatBG = pctBeat.Tag
  End Sub
#End Region

#Region "Track Selection"
  Private Sub cmbChapters_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbChapters.SelectedIndexChanged
    If bDVD Then
      If cmbChapters.Tag Is Nothing Then mpPlayer.SetDVDCurrentChapter(cmbChapters.SelectedIndex + 1)
    Else
      If cmbChapters.Tag IsNot Nothing Then
        Dim ChapterCollection As Collection = CType(cmbChapters.Tag, Collection)
        If ChapterCollection.Count > 0 Then
          For Each Chapter In ChapterCollection
            If StrComp(Chapter(1), cmbChapters.Text, CompareMethod.Text) = 0 Then
              mpPlayer.Position = Chapter(0)
              Exit For
            End If
          Next
        End If
      End If
    End If
  End Sub

  Private Sub cmbVidTrack_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbVidTrack.SelectedIndexChanged
    If bDVD Then
      If cmbVidTrack.Tag Is Nothing Then mpPlayer.SetDVDCurrentTitle(cmbVidTrack.SelectedIndex + 1)
    Else
      Debug.Print("I dunno what to do here yet")
    End If
  End Sub

  Private Sub cmbAudTrack_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbAudTrack.SelectedIndexChanged
    If bDVD Then
      If cmbAudTrack.Tag Is Nothing Then mpPlayer.SetDVDCurrentAudioStream(cmbAudTrack.SelectedIndex)
    Else
      If LoadFFDShow() Then
        ffAPI.AudioStream = IIf(cmbVidTrack.Items.Count > 1, cmbVidTrack.Items.Count + 1, 1) + cmbAudTrack.SelectedIndex
      End If
      If String.Compare(IO.Path.GetExtension(mpPlayer.FileName), ".mkv", True) = 0 Then
        Dim mkvHeader As New Seed.clsMKVHeaders(mpPlayer.FileName)
        For Each Track In mkvHeader.TrackEntries
          Dim sTrackTitle As String
          If String.IsNullOrEmpty(Track.TrackName) Then
            sTrackTitle = Track.Language
          Else
            sTrackTitle = Track.TrackName & " [" & Track.Language & "]"
          End If
          Select Case Track.TrackType
            Case &H2
              If cmbAudTrack.Text = (sTrackTitle) Then
                Dim iEncQ As Integer, sCodec As String = Nothing
                MKVAudioCodecs(Track.CodecID, iEncQ, sCodec)
                SetProps(iEncQ, Track.Audio.Channels, -2, sCodec, Track.Audio.Channels)
              End If
          End Select
        Next
        mkvHeader = Nothing
      End If
    End If
  End Sub

  Private Sub cmbSubtitles_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbSubtitles.SelectedIndexChanged
    If bDVD Then
      If cmbSubtitles.Tag Is Nothing Then
        Try
          mpPlayer.SetDVDCurrentSubStream(cmbSubtitles.SelectedIndex - 1)
        Catch ex As Exception
          cmbSubtitles.SelectedIndex = mpPlayer.GetDVDCurrentSubStream + 1
        End Try
      End If
    Else
      If LoadFFDShow() Then
        ffAPI.SubtitleStream = IIf(cmbVidTrack.Items.Count > 1, cmbVidTrack.Items.Count + 1, 1) + IIf(cmbAudTrack.Items.Count > 1, cmbAudTrack.Items.Count, 1) + cmbSubtitles.SelectedIndex
      End If
    End If
  End Sub
#End Region

#Region "Volume"
  Private Sub bpgVolume_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles bpgVolume.MouseDown
    If e.Button = Windows.Forms.MouseButtons.Left And Not VolS Then
      bpgVolume.Value = ((e.X + 1) / (bpgVolume.Width - 2)) * bpgVolume.Maximum
      VolS = True
    End If
  End Sub

  Private Sub bpgVolume_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles bpgVolume.MouseMove
    If VolS And e.Button = Windows.Forms.MouseButtons.Left Then
      If e.X > 0 And e.X < bpgVolume.Width Then bpgVolume.Value = ((e.X + 1) / (bpgVolume.Width - 2)) * bpgVolume.Maximum
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
      End If
    End If
    VolS = False
  End Sub

  Private Sub bpgVolume_ValueChanged(sender As Object, e As System.EventArgs) Handles bpgVolume.ValueChanged
    If bCD Or mpPlayer.IsFlash Then
      volControl.SetVolume((bpgVolume.Value / bpgVolume.Maximum) * 100)
    Else
      mpPlayer.LinearVolume = bpgVolume.Value
    End If
  End Sub
#End Region
#End Region

#Region "Menus"
#Region "File"
  Private Sub mnuOpenFile_Click(sender As System.Object, e As System.EventArgs) Handles mnuOpenFile.Click
    If frmOpen.Visible Then Exit Sub
    If frmOpen.ShowDialog(IIf(frmFS.Visible, frmFS, Me)) = Windows.Forms.DialogResult.OK Then
      txtPlayListTitle.Tag = UNKNOWN_ALBUM
      Dim Results() As String = frmOpen.sResult
      Dim openInvoker As New OpenEventInvoker(AddressOf OpenEvent)
      openInvoker.BeginInvoke(Results, Nothing, Nothing)
    End If
  End Sub

  Private Delegate Sub OpenEventInvoker(Results As Object)
  Private Sub OpenEvent(Results As Object)
    If Me.InvokeRequired Then
      Me.BeginInvoke(New OpenEventInvoker(AddressOf OpenEvent), Results)
    Else
      Dim sResults() As String = Results
      If sResults.Length = 1 Then
        If sResults(0).Substring(1) = ":\" Then
          Select Case My.Computer.FileSystem.GetDriveInfo(sResults(0).Substring(0, 3)).DriveFormat
            Case "CDFS"
              OpenCD(sResults(0))
            Case "UDF"
              OpenDVD(sResults(0))
            Case Else
              MsgBox("Unable to open Disc. Unknown Format: " & My.Computer.FileSystem.GetDriveInfo(sResults(0).Substring(0, 3)).DriveFormat, MsgBoxStyle.Critical, "Open Failure")
          End Select
        ElseIf My.Computer.FileSystem.DirectoryExists(sResults(0)) Then
          If sResults(0).EndsWith("VIDEO_TS") Then
            OpenDVD(sResults(0))
          Else
            If dgvPlayList.Rows.Count = 0 Then
              txtPlayListTitle.Tag = IO.Path.GetFileName(sResults(0))
              AddDirToPlayListAndMaybePlay(sResults(0))
            Else
              AddDirToPlayList(sResults(0))
            End If
          End If
        Else
          Select Case IO.Path.GetExtension(sResults(0)).ToLower
            Case ".llpl", ".m3u", ".pls"
              cmdShufflePL.Tag = False
              cmdShufflePL.Image = My.Resources.pl_button_order
              dgvPlayList.Rows.Clear()
              OpenPlayList(sResults(0), True)
            Case Else
              mpPlayer.SetNoQueue()
              OpenFile(sResults(0), True)
              ThreadedInitial()
          End Select
        End If
      Else
        'dgvPlayList.Rows.Clear()
        For Each item In sResults
          If My.Computer.FileSystem.DirectoryExists(item) Then
            AddDirToPlayList(item)
          Else
            AddToPlayList(item, , , False)
          End If
          Application.DoEvents()
        Next
        If dgvPlayList.Rows.Count > 0 And (mpPlayer.State = Seed.ctlSeed.MediaState.mClosed Or mpPlayer.State = Seed.ctlSeed.MediaState.mPaused Or mpPlayer.State = Seed.ctlSeed.MediaState.mStopped) Then
          dgvPlayList.Rows(0).Selected = True
          StartPlayList()
        End If
        QueueFullPlayListData()
      End If
    End If
  End Sub

  Private Sub mnuCloseFile_Click(sender As System.Object, e As System.EventArgs) Handles mnuCloseFile.Click
    If bCD Then
      bCD = False
      cCD.Stop()
      cCD.Dispose()
      cCD = Nothing
    Else
      bDVD = False
      mpPlayer.SetNoQueue()
      mpPlayer.FileName = String.Empty
    End If
    FirstInit = True
    InitialData()
  End Sub

  Private Sub mnuProperties_Click(sender As System.Object, e As System.EventArgs) Handles mnuProperties.Click
    If bCD Then
      MsgBox("CDs do not have properties.", MsgBoxStyle.Information, "No Properties")
    Else
      Dim frmProperties As New frmProps With {.Tag = Me}
      frmProperties.ShowData(mpPlayer.FileName)
    End If
  End Sub

  Private Sub mnuExit_Click(sender As System.Object, e As System.EventArgs) Handles mnuExit.Click
    Me.Close()
  End Sub
#End Region

#Region "Options"
  Private Sub mnuOptions_DropDownOpening(sender As Object, e As System.EventArgs) Handles mnuOptions.DropDownOpening
    For I As Integer = mnuOutDev.DropDownItems.Count - 1 To 1 Step -1
      mnuOutDev.DropDownItems.RemoveAt(I)
    Next
    Dim fItem As Integer = 2
    For Each item As DirectShowLib.DsDevice In Seed.ctlSeed.GetRenderers("audio renderers")
      If item.Name.StartsWith("DirectSound: ") Then
        Dim sItem As String = item.Name.Substring(13)
        If sItem.Contains(" (") Then sItem = sItem.Substring(0, sItem.IndexOf(" ("))
        Dim devItem As New ToolStripRadioButtonMenuItem(sItem) ' = mnuOutDev.DropDownItems.Add(sItem)
        mnuOutDev.DropDownItems.Add(devItem)
        devItem.Tag = item.Name.Substring(13)
        devItem.Checked = (item.Name.Substring(13) = My.Settings.Device)
        AddHandler devItem.Click, AddressOf mnuOutItem_Click
        'devItem.ShortcutKeyDisplayString = "F" & fItem
        devItem.ShortcutKeys = 111 + fItem
        fItem += 1
      End If
    Next
    mnuOutDefault.Checked = String.IsNullOrEmpty(My.Settings.Device)
  End Sub

  Private Sub mnuOutDefault_Click(sender As System.Object, e As System.EventArgs) Handles mnuOutDefault.Click
    My.Settings.Device = String.Empty
    mpPlayer.AudioDevice = My.Settings.Device
  End Sub

  Private Sub mnuOutItem_Click(sender As System.Object, e As System.EventArgs)
    Dim mSend As ToolStripRadioButtonMenuItem = sender
    My.Settings.Device = mSend.Tag
    mpPlayer.AudioDevice = My.Settings.Device
  End Sub

  Private Sub mnuSettings_Click(sender As System.Object, e As System.EventArgs) Handles mnuSettings.Click
    If frmSettings.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
      My.Settings.Gapless = frmSettings.numGapless.Value / 1000
      My.Settings.SingleInstance = frmSettings.chkSingleInstance.Checked
      If frmSettings.cmbAudioOutput.SelectedIndex = 0 Then
        My.Settings.Device = String.Empty
      Else
        My.Settings.Device = frmSettings.cmbAudioOutput.Text
      End If
      My.Settings.DefaultLocale = frmSettings.cmbLocale.Text
      My.Settings.Subtitles = frmSettings.chkSubtitles.Checked
      mpPlayer.QueueTime = My.Settings.Gapless
      mpPlayer.AudioDevice = My.Settings.Device

      My.Settings.Visualization = frmSettings.lstVis.Text
      My.Settings.Visualization_Rate = frmSettings.txtRate.Value

      My.Settings.Keyboard = frmSettings.chkKeyboard.Checked
      My.Settings.Keyboard_About = frmSettings.txtKeyAbout.Text
      My.Settings.Keyboard_AddToPL = frmSettings.txtKeyAddToPL.Text
      My.Settings.Keyboard_ClearPL = frmSettings.txtKeyClearPL.Text
      My.Settings.Keyboard_Close = frmSettings.txtKeyClose.Text
      My.Settings.Keyboard_DiscEject = frmSettings.txtKeyDiscEject.Text
      My.Settings.Keyboard_DVDMenu = frmSettings.txtKeyDVDMenu.Text
      My.Settings.Keyboard_FS = frmSettings.txtKeyFullScreen.Text
      My.Settings.Keyboard_Last = frmSettings.txtKeyLast.Text
      My.Settings.Keyboard_Mute = frmSettings.txtKeyMute.Text
      My.Settings.Keyboard_Next = frmSettings.txtKeyNext.Text
      My.Settings.Keyboard_Open = frmSettings.txtKeyOpen.Text
      My.Settings.Keyboard_OpenPL = frmSettings.txtKeyOpenPL.Text
      My.Settings.Keyboard_PlayPause = frmSettings.txtKeyPlayPause.Text
      My.Settings.Keyboard_Props = frmSettings.txtKeyFileProperties.Text
      My.Settings.Keyboard_RemoveFromPL = frmSettings.txtKeyRemoveFromPL.Text
      My.Settings.Keyboard_RenamePL = frmSettings.txtKeyRenamePL.Text
      My.Settings.Keyboard_RepeatPL = frmSettings.txtKeyRepeatPL.Text
      My.Settings.Keyboard_RepeatTrack = frmSettings.txtKeyRepeatTrack.Text
      My.Settings.Keyboard_SavePL = frmSettings.txtKeySavePL.Text
      My.Settings.Keyboard_Settings = frmSettings.txtKeySettings.Text
      My.Settings.Keyboard_Shuffle = frmSettings.txtKeyShuffle.Text
      My.Settings.Keyboard_SkipBack = frmSettings.txtKeySkipBack.Text
      My.Settings.Keyboard_SkipFwd = frmSettings.txtKeySkipFwd.Text
      My.Settings.Keyboard_Stop = frmSettings.txtKeyStop.Text
      My.Settings.Keyboard_VolDown = frmSettings.txtKeyVolDown.Text
      My.Settings.Keyboard_VolUp = frmSettings.txtKeyVolUp.Text
      My.Settings.Keyboard_Webpage = frmSettings.txtKeyWebpage.Text

      My.Settings.Gamepad = frmSettings.chkGamepad.Checked
      My.Settings.Gamepad_About = frmSettings.txtPadAbout.Text
      My.Settings.Gamepad_AddToPL = frmSettings.txtPadAddToPL.Text
      My.Settings.Gamepad_ClearPL = frmSettings.txtPadClearPL.Text
      My.Settings.Gamepad_Close = frmSettings.txtPadClose.Text
      My.Settings.Gamepad_DiscEject = frmSettings.txtPadDiscEject.Text
      My.Settings.Gamepad_DVDMenu = frmSettings.txtPadDVDMenu.Text
      My.Settings.Gamepad_FS = frmSettings.txtPadFullScreen.Text
      My.Settings.Gamepad_Last = frmSettings.txtPadLast.Text
      My.Settings.Gamepad_Mute = frmSettings.txtPadMute.Text
      My.Settings.Gamepad_Next = frmSettings.txtPadNext.Text
      My.Settings.Gamepad_Open = frmSettings.txtPadOpen.Text
      My.Settings.Gamepad_OpenPL = frmSettings.txtPadOpenPL.Text
      My.Settings.Gamepad_PlayPause = frmSettings.txtPadPlayPause.Text
      My.Settings.Gamepad_Props = frmSettings.txtPadProps.Text
      My.Settings.Gamepad_RemoveFromPL = frmSettings.txtPadRemoveFromPL.Text
      My.Settings.Gamepad_RenamePL = frmSettings.txtPadRenamePL.Text
      My.Settings.Gamepad_RepeatPL = frmSettings.txtPadRepeatPL.Text
      My.Settings.Gamepad_RepeatTrack = frmSettings.txtPadRepeatTrack.Text
      My.Settings.Gamepad_SavePL = frmSettings.txtPadSavePL.Text
      My.Settings.Gamepad_Settings = frmSettings.txtPadSettings.Text
      My.Settings.Gamepad_Shuffle = frmSettings.txtPadShuffle.Text
      My.Settings.Gamepad_SkipBack = frmSettings.txtPadSkipBack.Text
      My.Settings.Gamepad_SkipFwd = frmSettings.txtPadSkipFwd.Text
      My.Settings.Gamepad_Stop = frmSettings.txtPadStop.Text
      My.Settings.Gamepad_VolDown = frmSettings.txtPadVolDown.Text
      My.Settings.Gamepad_VolUp = frmSettings.txtPadVolUp.Text
      My.Settings.Gamepad_Webpage = frmSettings.txtPadWebpage.Text
      LabelShortcuts()
      SetVisualizations()
    End If
  End Sub
#End Region

#Region "Help"
  Private Sub mnuWebpage_Click(sender As System.Object, e As System.EventArgs) Handles mnuWebpage.Click
    Process.Start("http://lime.realityripple.com")
  End Sub

  Private Sub mnuAbout_Click(sender As System.Object, e As System.EventArgs) Handles mnuAbout.Click
    frmAbout.Show(Me)
  End Sub
#End Region

#Region "PlayList"
  Private Sub mnuPLPlay_Click(sender As System.Object, e As System.EventArgs) Handles mnuPLPlay.Click
    Dim RowIndex As Integer = mnuPL.Tag
    If GetSelectedPlayListItem() >= 0 Then dgvPlayList.Rows(GetSelectedPlayListItem).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
    OpenFile(dgvPlayList.Rows(RowIndex).Tag(0), True)
    InitialData()
    'If dgvPlayList.Rows.Count > RowIndex + 1 Then
    '  mpPlayer.FileQueue(dgvPlayList.Rows(RowIndex + 1).Tag(0))
    'Else
    '  mpPlayer.SetNoQueue()
    'End If
    dgvPlayList.Rows(RowIndex).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
    ThreadedQueue()
  End Sub

  Private Sub mnuPLDelete_Click(sender As System.Object, e As System.EventArgs) Handles mnuPLDelete.Click
    Dim RowIndex As Integer = mnuPL.Tag
    RemoveFromPlayList(RowIndex)
  End Sub

  Private Sub mnuPLProps_Click(sender As System.Object, e As System.EventArgs) Handles mnuPLProps.Click
    Dim RowIndex As Integer = mnuPL.Tag
    Dim frmProperties As New frmProps With {.Tag = Me}
    frmProperties.ShowData(dgvPlayList.Rows(RowIndex).Tag(0))
  End Sub

  Private Sub mnuPLOpenFile_Click(sender As System.Object, e As System.EventArgs) Handles mnuPLOpenFile.Click
    Dim RowIndex As Integer = mnuPL.Tag
    Process.Start("explorer", "/select,""" & dgvPlayList.Rows(RowIndex).Tag(0) & """")
  End Sub
#End Region

#Region "Video"
  Private Sub mnuFullScreen_Click(sender As System.Object, e As System.EventArgs) Handles mnuFullScreen.Click
    cmdFullScreen_Click(New Object, New EventArgs)
  End Sub

  Private Sub mnuRatioForce_Click(sender As System.Object, e As System.EventArgs) Handles mnuRatioForce.Click
    If ffAPI IsNot Nothing Then ffAPI.ResizeKeepAspectRatio = mnuRatioForce.Checked
  End Sub

  Private Sub mnuRatioAutomatic_Click(sender As System.Object, e As System.EventArgs) Handles mnuRatioAutomatic.Click
    mnuRatioStandard.Checked = False
    mnuRatioWide.Checked = False
    mnuRatioAutomatic.Checked = True
    If bDVD And Not mnuRatio.Tag Then mpPlayer.DVDSetPreferredRatio(DirectShowLib.Dvd.DvdPreferredDisplayMode.DisplayContentDefault)
    CalcScaleRatio()
  End Sub

  Private Sub mnuRatioStandard_Click(sender As System.Object, e As System.EventArgs) Handles mnuRatioStandard.Click
    mnuRatioStandard.Checked = True
    mnuRatioWide.Checked = False
    mnuRatioAutomatic.Checked = False
    If bDVD And Not mnuRatio.Tag Then mpPlayer.DVDSetPreferredRatio(DirectShowLib.Dvd.DvdPreferredDisplayMode.Display4x3PanScanPreferred)
    CalcScaleRatio()
  End Sub

  Private Sub mnuRatioWide_Click(sender As System.Object, e As System.EventArgs) Handles mnuRatioWide.Click
    mnuRatioStandard.Checked = False
    mnuRatioWide.Checked = True
    mnuRatioAutomatic.Checked = False
    If bDVD And Not mnuRatio.Tag Then mpPlayer.DVDSetPreferredRatio(DirectShowLib.Dvd.DvdPreferredDisplayMode.Display16x9)
    CalcScaleRatio()
  End Sub

  Private Sub mnuScaleHalf_Click(sender As System.Object, e As System.EventArgs) Handles mnuScaleHalf.Click
    mnuScaleHalf.Checked = True
    mnuScaleNorm.Checked = False
    mnuScaleTwice.Checked = False
    CalcScaleRatio()
  End Sub

  Private Sub mnuScaleNorm_Click(sender As System.Object, e As System.EventArgs) Handles mnuScaleNorm.Click
    mnuScaleHalf.Checked = False
    mnuScaleNorm.Checked = True
    mnuScaleTwice.Checked = False
    CalcScaleRatio()
  End Sub

  Private Sub mnuScaleTwice_Click(sender As System.Object, e As System.EventArgs) Handles mnuScaleTwice.Click
    mnuScaleHalf.Checked = False
    mnuScaleNorm.Checked = False
    mnuScaleTwice.Checked = True
    CalcScaleRatio()
  End Sub

  Private Sub GetScaleRatio(ByRef Scale As Double, ByRef Ratio As Byte)
    If mnuScaleHalf.Checked Then
      Scale = 0.5
    ElseIf mnuScaleNorm.Checked Then
      Scale = 1
    ElseIf mnuScaleTwice.Checked Then
      Scale = 2
    Else
      Scale = &HFF
    End If
    If mnuRatioAutomatic.Checked Then
      Ratio = 0
    ElseIf mnuRatioStandard.Checked Then
      Ratio = 1
    ElseIf mnuRatioWide.Checked Then
      Ratio = 2
    Else
      Ratio = &HFF
    End If
  End Sub

  Private Sub CalcScaleRatio()
    Dim Ratio As Byte, Scale As Double
    GetScaleRatio(Scale, Ratio)
    If CorrectedSize.Equals(Drawing.Size.Empty) Then CorrectedSize = VidSize
    Select Case Ratio
      Case 0 : SetScaledVideoSize(CorrectedSize.Width, CorrectedSize.Height, Scale)
      Case 1 : SetScaledVideoSize(CorrectedSize.Height / 3 * 4, CorrectedSize.Height, Scale)
      Case 2 : SetScaledVideoSize(CorrectedSize.Height / 9 * 16, CorrectedSize.Height, Scale)
    End Select
  End Sub

  Private Sub mnuCopyScreenshot_Click(sender As System.Object, e As System.EventArgs) Handles mnuCopyScreenshot.Click
    If mpPlayer.State <> Seed.ctlSeed.MediaState.mClosed Then
      Dim bFile As Drawing.Bitmap = mpPlayer.GetFileThumbnail(, , False)
      Clipboard.SetImage(bFile)
    End If
  End Sub

  Private Sub mnuVideoTrack_DropDownOpening(sender As Object, e As System.EventArgs) Handles mnuVideoTrack.DropDownOpening
    For Each subItem As ToolStripMenuItem In mnuVideoTrack.DropDownItems
      subItem.Checked = (subItem.Text = cmbVidTrack.Text)
      AddHandler subItem.Click, AddressOf mnuVideoTrackItem_Click
    Next
  End Sub

  Private Sub mnuVideoTrackItem_Click(sender As Object, e As EventArgs)
    cmbVidTrack.Text = sender.Text
  End Sub

  Private Sub mnuAudioTrack_DropDownOpening(sender As Object, e As System.EventArgs) Handles mnuAudioTrack.DropDownOpening
    For Each subItem As ToolStripMenuItem In mnuAudioTrack.DropDownItems
      subItem.Checked = (subItem.Text = cmbAudTrack.Text)
      AddHandler subItem.Click, AddressOf mnuAudioTrackItem_Click
    Next
  End Sub

  Private Sub mnuAudioTrackItem_Click(sender As Object, e As EventArgs)
    cmbAudTrack.Text = sender.Text
  End Sub

  Private Sub mnuSubtitleTrack_DropDownOpening(sender As Object, e As System.EventArgs) Handles mnuSubtitleTrack.DropDownOpening
    For Each subItem As ToolStripMenuItem In mnuSubtitleTrack.DropDownItems
      subItem.Checked = (subItem.Text = cmbSubtitles.Text)
      AddHandler subItem.Click, AddressOf mnuSubtitleTrackItem_Click
    Next
  End Sub

  Private Sub mnuSubtitleTrackItem_Click(sender As Object, e As EventArgs)
    cmbSubtitles.Text = sender.Text
  End Sub

  Private Sub mnuChapterTrack_DropDownOpening(sender As Object, e As System.EventArgs) Handles mnuChapterTrack.DropDownOpening
    For Each subItem As ToolStripMenuItem In mnuChapterTrack.DropDownItems
      subItem.Checked = (subItem.Text = cmbChapters.Text)
      AddHandler subItem.Click, AddressOf mnuChapterTrackItem_Click
    Next
  End Sub

  Private Sub mnuChapterTrackItem_Click(sender As Object, e As EventArgs)
    cmbChapters.Text = sender.Text
  End Sub
#End Region

#Region "Art"
  Private Sub mnuArtShow_Click(sender As System.Object, e As System.EventArgs) Handles mnuArtShow.Click
    If mnuArtShow.Checked Then
      pctAlbumArt.BackgroundImage = mFArt.Art
    Else
      pctAlbumArt.BackgroundImage = Nothing
    End If
  End Sub

  Private Sub mnuArtFind_Click(sender As System.Object, e As System.EventArgs) Handles mnuArtFind.Click
    If FileArt Is Nothing Then
      pctAlbumArt.Tag = Nothing
      FileArt = GetArt(mpPlayer.FileName, True, False)
    Else
      pctAlbumArt.Tag = FileArt
      FileArt = GetArt(mpPlayer.FileName, True, True)
    End If
  End Sub

  Private Sub mnuArtVisShow_Click(sender As System.Object, e As System.EventArgs) Handles mnuArtVisShow.Click
    If methodDraw IsNot Nothing Then methodDraw = Nothing
    If mnuArtVisShow.Checked Then
      If My.Settings.Visualization <> "None" Then
        SetVisualizations()
      ElseIf Not String.IsNullOrEmpty(tmrVis.Tag) Then
        If tmrVis.Tag <> "None" Then
          My.Settings.Visualization = tmrVis.Tag
          SetVisualizations()
        End If
      Else
        My.Settings.Visualization = "Random"
        SetVisualizations()
      End If
    Else
      tmrVis.Tag = My.Settings.Visualization
      My.Settings.Visualization = "None"
      SetVisualizations()
      pctAlbumArt.Image = Nothing
      If frmFS IsNot Nothing Then frmFS.pctVideo.Image = Nothing
    End If
  End Sub

  Private Sub munAlbumArt_Opening(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles mnuAlbumArt.Opening
    mnuArtShow.Checked = Not pctAlbumArt.BackgroundImage Is Nothing
    mnuArtVisShow.Checked = methodDraw IsNot Nothing
    mnuArtVisSelect.DropDownItems.Clear()
    If My.Computer.FileSystem.DirectoryExists(Application.StartupPath & "\Visualizations") Then
      For Each file In My.Computer.FileSystem.GetFiles(Application.StartupPath & "\Visualizations")
        If file.EndsWith(".dll") Then
          Dim mnuTmp As New ToolStripRadioButtonMenuItem(IO.Path.GetFileNameWithoutExtension(file))
          mnuTmp.Name = "mnuVis" & mnuTmp.Text.Replace(" "c, "_"c)
          If mnuTmp.Text = My.Settings.Visualization Then mnuTmp.Checked = True
          AddHandler mnuTmp.CheckedChanged, AddressOf mnuVis_CheckChanged
          mnuArtVisSelect.DropDownItems.Add(mnuTmp)
        End If
      Next
    End If
    mnuArtVisSelect.Enabled = mnuArtVisSelect.HasDropDownItems
  End Sub

  Private Sub mnuVis_CheckChanged(sender As System.Object, e As System.EventArgs)
    If sender.checked And Not String.Compare(My.Settings.Visualization, sender.text, True) = 0 Then
      My.Settings.Visualization = sender.text
      SetVisualizations()
    End If
  End Sub
#End Region
#End Region

#Region "PlayList"
#Region "Title Box"
  Private Sub txtPlayListTitle_Click(sender As System.Object, e As System.EventArgs) Handles txtPlayListTitle.Click
    If dgvPlayList.RowCount > 0 Then
      If txtPlayListTitle.ReadOnly Then
        txtPlayListTitle.BackColor = Drawing.SystemColors.Window
        txtPlayListTitle.BorderStyle = BorderStyle.Fixed3D
        txtPlayListTitle.Text = txtPlayListTitle.Tag
        txtPlayListTitle.SelectAll()
        txtPlayListTitle.ReadOnly = False
      End If
    Else
      dgvPlayList.Focus()
    End If
  End Sub

  Private Sub txtPlayListTitle_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtPlayListTitle.KeyDown
    If e.KeyCode = Keys.Enter Then
      e.Handled = True
      e.SuppressKeyPress = True
    End If
  End Sub

  Private Sub txtPlayListTitle_KeyUp(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtPlayListTitle.KeyUp
    If e.KeyCode = Keys.Enter Then
      dgvPlayList.Focus()
      e.Handled = True
      e.SuppressKeyPress = True
    End If
  End Sub

  Private Sub txtPlayListTitle_LostFocus(sender As Object, e As System.EventArgs) Handles txtPlayListTitle.LostFocus
    If Not txtPlayListTitle.ReadOnly Then
      txtPlayListTitle.Tag = txtPlayListTitle.Text
      txtPlayListTitle.BackColor = Drawing.SystemColors.Control
      txtPlayListTitle.BorderStyle = BorderStyle.None
      txtPlayListTitle.ReadOnly = True
    End If
  End Sub
#End Region

#Region "List"
#Region "Mouse"

  Private Sub dgvPlayList_CellMouseClick(sender As Object, e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles dgvPlayList.CellMouseClick
    If e.Button = Windows.Forms.MouseButtons.Right Then
      mnuPL.Tag = e.RowIndex
      mnuPL.Show(MousePosition)
    End If
  End Sub

  Private Sub dgvPlayList_CellMouseDoubleClick(sender As Object, e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles dgvPlayList.CellMouseDoubleClick
    If GetSelectedPlayListItem() >= 0 Then dgvPlayList.Rows(GetSelectedPlayListItem).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
    OpenFile(dgvPlayList.Rows(e.RowIndex).Tag(0), True)
    If dgvPlayList.RowCount > e.RowIndex Then dgvPlayList.Rows(e.RowIndex).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
    ThreadedInitialQueue()
  End Sub

  Private Sub dgvPlayList_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles dgvPlayList.MouseDown
    sourceRowIndex = -1
    If e.Button = Windows.Forms.MouseButtons.Left And dgvPlayList.SelectedRows.Count > 0 Then
      sourceRowIndex = dgvPlayList.HitTest(e.X, e.Y).RowIndex
      If sourceRowIndex >= 0 Then
        dgvPlayList.Rows(sourceRowIndex).Selected = True
        dgvPlayList.Refresh()
      End If
    End If
  End Sub

  Private Sub dgvPlayList_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles dgvPlayList.MouseMove
    Static lastLoc As Drawing.Point
    If e.Button = Windows.Forms.MouseButtons.Left And dgvPlayList.SelectedRows.Count > 0 And sourceRowIndex >= 0 Then
      If Math.Abs(lastLoc.X - e.Location.X) > 3 Or Math.Abs(lastLoc.Y - e.Location.Y) > 3 Then
        dgvPlayList.Rows(sourceRowIndex).Selected = True
        dgvPlayList.Refresh()
        dgvPlayList.DoDragDrop(dgvPlayList.Rows(sourceRowIndex), DragDropEffects.Move)
      End If
      'Debug.Print("Y: " & e.Location.Y)
    End If
  End Sub

  Private Sub dgvPlayList_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles dgvPlayList.MouseUp
    sourceRowIndex = -1
  End Sub

  Private Sub dgvPlayList_MouseDoubleClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles dgvPlayList.MouseDoubleClick
    If e.Y > dgvPlayList.Rows.GetRowsHeight(DataGridViewElementStates.Visible) Then
      cmdAddToPL_Click(New Object, New EventArgs)
    End If
  End Sub
#End Region
#Region "Keyboard"
  Private Sub dgvPlayList_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles dgvPlayList.KeyDown
    Select Case KeyToStr(e.KeyCode)
      Case My.Settings.Keyboard_PlayPause
        If dgvPlayList.SelectedRows.Count > 0 Then
          StartPlayList()
        Else
          cmdAddToPL_Click(New Object, New EventArgs)
        End If
        e.Handled = True
      Case My.Settings.Keyboard_RemoveFromPL
        If dgvPlayList.SelectedRows.Count > 0 Then
          cmdRemoveFromPL_Click(New Object, New EventArgs)
        Else
          Beep()
        End If
        e.Handled = True
    End Select
  End Sub

  Private Sub dgvPlayList_KeyUp(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles dgvPlayList.KeyUp
    Select Case KeyToStr(e.KeyCode)
      Case My.Settings.Keyboard_PlayPause, My.Settings.Keyboard_RemoveFromPL
        e.Handled = True
    End Select
  End Sub
#End Region
#Region "Drag/Drop"
  Private Sub dgvPlayList_DragDrop(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles dgvPlayList.DragDrop
    If sourceRowIndex >= 0 Then
      sourceRowIndex = -1
      ThreadedQueue()
    Else
      DragDropEvent(sender, e)
    End If
  End Sub

  Private Sub dgvPlayList_DragEnter(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles dgvPlayList.DragEnter
    DragEnterEvent(sender, e)
  End Sub

  Private Sub dgvPlayList_DragOver(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles dgvPlayList.DragOver
    If sourceRowIndex >= 0 Then
      Dim ptRet As Drawing.Point = dgvPlayList.PointToClient(New Drawing.Point(e.X, e.Y))
      Dim TargetRowIndex As Integer = dgvPlayList.HitTest(ptRet.X, ptRet.Y).RowIndex
      If TargetRowIndex > -1 Then
        Dim scrollHeight As Integer = dgvPlayList.Rows(TargetRowIndex).Height / 2
        Static LastScroll As Integer
        If ptRet.Y < scrollHeight Then
          Dim ScrollDist As Integer = 10 - (scrollHeight - ptRet.Y)
          'Debug.Print(ScrollDist & "/10 Top Intensity")
          If dgvPlayList.FirstDisplayedScrollingRowIndex > 0 Then
            If Environment.TickCount - LastScroll > ScrollDist * 50 Then
              dgvPlayList.FirstDisplayedScrollingRowIndex -= 1
              LastScroll = Environment.TickCount
            End If
          End If
        ElseIf ptRet.Y > (dgvPlayList.Height - 1) - scrollHeight Then
          Dim ScrollDist As Integer = 10 - (ptRet.Y - ((dgvPlayList.Height - 1) - scrollHeight))
          'Debug.Print(ScrollDist & "/10 Bottom Intensity")
          If dgvPlayList.FirstDisplayedScrollingRowIndex < dgvPlayList.RowCount - 1 Then
            If Environment.TickCount - LastScroll > ScrollDist * 50 Then
              dgvPlayList.FirstDisplayedScrollingRowIndex += 1
              LastScroll = Environment.TickCount
            End If
          End If
        End If

        If sourceRowIndex <> TargetRowIndex Then
          MoveRow(sourceRowIndex, TargetRowIndex)

          'Dim tmpRow As DataGridViewRow = dgvPlayList.Rows(sourceRowIndex)
          'dgvPlayList.Rows.RemoveAt(sourceRowIndex)
          'dgvPlayList.Rows.Insert(TargetRowIndex, tmpRow)
          sourceRowIndex = TargetRowIndex
          dgvPlayList.Rows(TargetRowIndex).Selected = True
          dgvPlayList.ResumeLayout()
          ThreadedQueue()
        End If

        e.Effect = DragDropEffects.Move Or DragDropEffects.Scroll
      Else
        e.Effect = DragDropEffects.None
      End If
    Else
      DragOverEvent(sender, e)
    End If
  End Sub

  Private Sub MoveRow(iFrom As Integer, iTo As Integer)
    Dim fromData() As Object
    Dim fromTT() As String
    Dim CellCount As Integer = dgvPlayList.Rows(iFrom).Cells.Count
    Dim fromStyle() As DataGridViewCellStyle
    ReDim fromData(CellCount)
    ReDim fromTT(CellCount - 1)
    ReDim fromStyle(CellCount - 1)
    For I As Integer = 0 To CellCount - 1
      fromData(I) = dgvPlayList.Rows(iFrom).Cells(I).Value
      fromTT(I) = dgvPlayList.Rows(iFrom).Cells(I).ToolTipText
    Next I
    fromData(CellCount) = dgvPlayList.Rows(iFrom).Tag
    If Math.Abs(iFrom - iTo) = 1 Then
      For I As Integer = 0 To CellCount - 1
        dgvPlayList.Rows(iFrom).Cells(I).Value = dgvPlayList.Rows(iTo).Cells(I).Value
        dgvPlayList.Rows(iFrom).Cells(I).ToolTipText = dgvPlayList.Rows(iTo).Cells(I).ToolTipText
        dgvPlayList.Rows(iFrom).Cells(I).Style = dgvPlayList.Rows(iTo).Cells(I).Style
      Next
      dgvPlayList.Rows(iFrom).Tag = dgvPlayList.Rows(iTo).Tag
    Else
      If iFrom > iTo Then
        For R As Integer = iFrom - 1 To iTo Step -1
          For I As Integer = 0 To dgvPlayList.Rows(R).Cells.Count - 1
            dgvPlayList.Rows(R + 1).Cells(I).Value = dgvPlayList.Rows(R).Cells(I).Value
            dgvPlayList.Rows(R + 1).Cells(I).ToolTipText = dgvPlayList.Rows(R).Cells(I).ToolTipText
            dgvPlayList.Rows(R + 1).Cells(I).Style = dgvPlayList.Rows(R).Cells(I).Style
          Next
          dgvPlayList.Rows(R + 1).Tag = dgvPlayList.Rows(R).Tag
        Next
      Else
        For R As Integer = iFrom + 1 To iTo
          For I As Integer = 0 To dgvPlayList.Rows(R).Cells.Count - 1
            dgvPlayList.Rows(R - 1).Cells(I).Value = dgvPlayList.Rows(R).Cells(I).Value
            dgvPlayList.Rows(R - 1).Cells(I).ToolTipText = dgvPlayList.Rows(R).Cells(I).ToolTipText
            dgvPlayList.Rows(R - 1).Cells(I).Style = dgvPlayList.Rows(R).Cells(I).Style
          Next
          dgvPlayList.Rows(R - 1).Tag = dgvPlayList.Rows(R).Tag
        Next
      End If
    End If
    For I As Integer = 0 To CellCount - 1
      dgvPlayList.Rows(iTo).Cells(I).Value = fromData(I)
      dgvPlayList.Rows(iTo).Cells(I).ToolTipText = fromTT(I)
      dgvPlayList.Rows(iTo).Cells(I).Style = fromStyle(I)
    Next
    dgvPlayList.Rows(iTo).Tag = fromData(CellCount)
    Erase fromData
    Erase fromTT
    Erase fromStyle
  End Sub
#End Region
#End Region

#Region "Buttons"
  Private Sub cmdBackPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdBackPL.Click
    If bDVD Then
      mpPlayer.DVDPrevious()
    ElseIf dgvPlayList.Rows.Count > 0 Then
      If GetSelectedPlayListItem() >= 0 Then
        Dim SelItem As Integer = GetSelectedPlayListItem()
        If SelItem > 0 Then
          dgvPlayList.Rows(SelItem).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
          dgvPlayList.Rows(SelItem - 1).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
          OpenFile(dgvPlayList.Rows(SelItem - 1).Tag(0), True)
          ThreadedInitialQueue()
          Exit Sub
        End If
      End If
    End If
  End Sub

  Private Sub cmdNextPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdNextPL.Click
    If bDVD Then
      mpPlayer.DVDNext()
    ElseIf dgvPlayList.Rows.Count > 0 Then
      If GetSelectedPlayListItem() >= 0 Then
        Dim SelItem As Integer = GetSelectedPlayListItem()
        If SelItem < dgvPlayList.Rows.Count - 1 Then
          dgvPlayList.Rows(SelItem).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
          dgvPlayList.Rows(SelItem + 1).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
          OpenFile(dgvPlayList.Rows(SelItem + 1).Tag(0), True)
          ThreadedInitialQueue()
          Exit Sub
        End If
      End If
    End If
  End Sub

  Private OrderedList() As DataGridViewRow
  Private Sub cmdShufflePL_Click(sender As System.Object, e As System.EventArgs) Handles cmdShufflePL.Click
    If cmdShufflePL.Tag = True Then
      cmdShufflePL.Tag = False
      cmdShufflePL.Image = My.Resources.pl_button_order
      dgvPlayList.Rows.Clear()
      For Each row As DataGridViewRow In OrderedList
        dgvPlayList.Rows.Add(row)
      Next
      ThreadedQueue()
    Else
      cmdShufflePL.Tag = True
      cmdShufflePL.Image = My.Resources.pl_button_shuffle
      Dim rCount As Integer = dgvPlayList.RowCount
      ReDim OrderedList(rCount - 1)
      For I As Integer = 0 To rCount - 1
        OrderedList(I) = dgvPlayList.Rows(I)
      Next
      If mpPlayer.State = Seed.ctlSeed.MediaState.mPlaying And GetSelectedPlayListItem() >= 0 Then
        Dim firstRow As DataGridViewRow = dgvPlayList.Rows(GetSelectedPlayListItem)
        Dim NewList(rCount - 1) As DataGridViewRow
        NewList(0) = firstRow
        dgvPlayList.Rows.Remove(firstRow)
        Dim I As Integer = 1
        Do Until dgvPlayList.RowCount = 0
          Dim rndVal As Integer = Int(Rnd() * dgvPlayList.RowCount)
          NewList(I) = dgvPlayList.Rows(rndVal)
          dgvPlayList.Rows.RemoveAt(rndVal)
          I += 1
        Loop
        For I = 0 To rCount - 1
          dgvPlayList.Rows.Add(NewList(I))
        Next
        Erase NewList

      Else
        Dim NewList(rCount - 1) As DataGridViewRow
        Dim I As Integer = 0
        Do Until dgvPlayList.RowCount = 0
          Dim rndVal As Integer = Int(Rnd() * dgvPlayList.RowCount)
          NewList(I) = dgvPlayList.Rows(rndVal)
          dgvPlayList.Rows.RemoveAt(rndVal)
          I += 1
        Loop
        For I = 0 To rCount - 1
          dgvPlayList.Rows.Add(NewList(I))
        Next
        Erase NewList
      End If
      ThreadedQueue()
    End If
  End Sub

  Private Sub cmdLoopPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdLoopPL.Click
    Dim iVal As Integer = cmdLoopPL.Tag
    Select Case iVal
      Case 0
        cmdLoopPL.Tag = 1
        cmdLoopPL.Image = My.Resources.button_loop_on
        ttDisp.SetToolTip(cmdLoopPL, "Loop at End of PlayList")
      Case 1
        cmdLoopPL.Tag = 2
        cmdLoopPL.Image = My.Resources.pl_button_loop_close
        ttDisp.SetToolTip(cmdLoopPL, "Close Lime Seed at End of PlayList")
      Case 2
        cmdLoopPL.Tag = 3
        cmdLoopPL.Image = My.Resources.pl_button_loop_restart
        ttDisp.SetToolTip(cmdLoopPL, "Restart Computer at End of PlayList")
      Case 3
        cmdLoopPL.Tag = 4
        cmdLoopPL.Image = My.Resources.pl_button_loop_shutdown
        ttDisp.SetToolTip(cmdLoopPL, "Shut Down Computer at End of PlayList")
      Case 4
        If PowerProfile.CanHibernate Then
          cmdLoopPL.Tag = 5
          cmdLoopPL.Image = My.Resources.pl_button_loop_hibernate
          ttDisp.SetToolTip(cmdLoopPL, "Hibernate Computer at End of PlayList")
        Else
          cmdLoopPL.Tag = 0
          cmdLoopPL.Image = My.Resources.button_loop_off
          ttDisp.SetToolTip(cmdLoopPL, "Stop at End of PlayList")
        End If
      Case Else
        cmdLoopPL.Tag = 0
        cmdLoopPL.Image = My.Resources.button_loop_off
        ttDisp.SetToolTip(cmdLoopPL, "Stop at End of PlayList")
    End Select
  End Sub

  Private Sub cmdAddToPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdAddToPL.Click
    Dim cdlBrowse As New OpenFileDialog With {.Filter = "All Files|*.*", .Multiselect = True, .Title = "Add to PlayList..."}
    If cdlBrowse.ShowDialog() = Windows.Forms.DialogResult.OK Then
      Dim addInvoker As New AddAndQueueInvoker(AddressOf AddAndQueue)
      addInvoker.BeginInvoke(cdlBrowse.FileNames, Nothing, Nothing)
    End If
  End Sub

  Private Delegate Sub AddAndQueueInvoker(FileNames As Object)
  Private Sub AddAndQueue(FileNames As Object)
    If Me.InvokeRequired Then
      Me.BeginInvoke(New AddAndQueueInvoker(AddressOf AddAndQueue), FileNames)
    Else
      Dim sFiles() As String = FileNames
      For Each item In sFiles
        Select Case IO.Path.GetExtension(item).ToLower
          Case ".llpl", ".m3u", ".pls" : OpenPlayList(item)
          Case Else : AddToPlayList(item, , , False)
        End Select
      Next
      QueueFullPlayListData()
    End If
  End Sub

  Private Sub cmdRemoveFromPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdRemoveFromPL.Click
    If dgvPlayList.SelectedRows.Count > 0 Then RemoveFromPlayList(dgvPlayList.SelectedRows(0).Index)
  End Sub

  Private Sub cmdClearPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdClearPL.Click
    If MsgBox("Are you sure you want to clear the PlayList?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Clear PlayList?") = MsgBoxResult.Yes Then
      ClearPlayList()
    End If
  End Sub

  Private Sub ClearPlayList()
    mpPlayer.SetNoQueue()
    cmdShufflePL.Tag = False
    cmdShufflePL.Image = My.Resources.pl_button_order
    dgvPlayList.Rows.Clear()
    txtPlayListTitle.Tag = UNKNOWN_ALBUM
  End Sub

  Private Sub cmdOpenPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdOpenPL.Click
    mnuOpenFile_Click(sender, e)
  End Sub

  Private Sub cmdSavePL_Click(sender As System.Object, e As System.EventArgs) Handles cmdSavePL.Click
    Dim cdlBrowse As New SaveFileDialog With {.Filter = "Lime Light PlayList (*.llpl)|*.llpl|MP3 PlayList (*.m3u)|*.m3u|PlayList File (*.pls)|*.pls|All Files|*.*", .FileName = txtPlayListTitle.Tag}
    If cdlBrowse.ShowDialog() = Windows.Forms.DialogResult.OK Then SavePlayList(cdlBrowse.FileName)
  End Sub
#End Region

#Region "Routines"
#Region "Open"
  Public Sub OpenPlayList(Path As String, Optional ByVal AutoPlay As Boolean = False)
    txtPlayListTitle.Tag = UNKNOWN_ALBUM
    Select Case IO.Path.GetExtension(Path).ToLower
      Case ".llpl" : OpenBinaryPL(Path)
      Case ".m3u" : OpenM3U(Path)
      Case ".pls" : OpenPLS(Path)
      Case Else
        MsgBox("Unknown file extension: " & IO.Path.GetExtension(Path).Substring(1).ToUpper & "!" & vbNewLine & "Attempting standard text, line delimited read.", MsgBoxStyle.Critical, "Unknown PlayList Extension")
        OpenTextPL(Path)
    End Select
    If dgvPlayList.Rows.Count > 0 And AutoPlay Then
      dgvPlayList.Rows(0).Selected = True
      StartPlayList()
    End If
  End Sub

  Private Sub OpenBinaryPL(Path As String)
    Dim bReader As New IO.BinaryReader(New IO.FileStream(Path, IO.FileMode.Open))
    Dim Ver As Byte = bReader.ReadByte
    If Ver = 1 Then
      Dim tmpLen As Short = bReader.ReadInt16
      Dim plTitle As String = System.Text.Encoding.GetEncoding(LATIN_1).GetString(bReader.ReadBytes(tmpLen))
      txtPlayListTitle.Tag = plTitle
      Dim tmpItems As Integer = bReader.ReadInt32
      For I As Integer = 1 To tmpItems
        tmpLen = bReader.ReadInt16
        Dim sPath As String = System.Text.Encoding.GetEncoding(LATIN_1).GetString(bReader.ReadBytes(tmpLen))
        tmpLen = bReader.ReadInt16
        Dim sTitle As String = System.Text.Encoding.GetEncoding(LATIN_1).GetString(bReader.ReadBytes(tmpLen))
        If sPath.Substring(1, 2) <> ":\" And sPath.Substring(0, 7) <> "http://" Then sPath = Path.Substring(0, Path.LastIndexOf("\") - 1) & sPath
        AddToPlayList(sPath, , , False)
      Next
      bReader.Close()
      QueueFullPlayListData()
    ElseIf Ver = 2 Then
      Dim plTitle As String = bReader.ReadString
      Dim tmpItems As UInt64 = bReader.ReadUInt64
      txtPlayListTitle.Tag = plTitle
      For I As UInt64 = 1 To tmpItems
        Dim sPath As String = bReader.ReadString
        Dim sTitle As String = bReader.ReadString
        If sPath.Substring(1, 2) <> ":\" And sPath.Substring(0, 7) <> "http://" Then sPath = Path.Substring(0, Path.LastIndexOf("\") - 1) & sPath
        AddToPlayList(sPath, sTitle, , False)
      Next
      bReader.Close()
      QueueFullPlayListData()
    Else
      bReader.Close()
      OpenTextPL(Path)
    End If
  End Sub

  Private Sub OpenTextPL(Path As String)
    Dim sReader As IO.StreamReader = My.Computer.FileSystem.OpenTextFileReader(Path)
    Do
      Dim sPath As String = sReader.ReadLine
      Dim sTitle As String = sReader.ReadLine
      If sPath.Substring(1, 2) <> ":\" And sPath.Substring(0, 7) <> "http://" Then sPath = Path.Substring(0, Path.LastIndexOf("\") + 1) & sPath
      AddToPlayList(sPath, sTitle, , False)
    Loop Until sReader.EndOfStream
    sReader.Close()
    QueueFullPlayListData()
  End Sub

  Private Sub OpenM3U(Path As String)
    Try
      Dim sReader As IO.StreamReader = My.Computer.FileSystem.OpenTextFileReader(Path)
      If sReader.ReadLine = "#EXTM3U" Then
        Do
          Dim sINFO As String = sReader.ReadLine
          Dim sPath As String = sReader.ReadLine
          If Not sINFO.Contains(",") Then
            MsgBox("M3U file is corrupt!", MsgBoxStyle.Exclamation, "Corrupt File")
            Exit Do
          End If
          Dim lTime As Double = Val(sINFO.Split(","c)(0).Substring(sINFO.IndexOf(":") + 1))
          Dim sTitle As String = sINFO.Split(","c)(1)
          If sPath.Substring(1, 2) <> ":\" And sPath.Substring(0, 7) <> "http://" Then sPath = Path.Substring(0, Path.LastIndexOf("\") + 1) & sPath
          AddToPlayList(sPath, sTitle, ConvertTimeVal(lTime), False)
        Loop Until sReader.EndOfStream
      Else
        sReader.BaseStream.Position = 0
        Do
          Dim sPath As String = sReader.ReadLine
          If sPath.Substring(1, 2) <> ":\" And sPath.Substring(0, 7) <> "http://" Then sPath = Path.Substring(0, Path.LastIndexOf("\") + 1) & sPath
          AddToPlayList(sPath, , , False)
        Loop Until sReader.EndOfStream
      End If
      sReader.Close()
      QueueFullPlayListData()
    Catch
      MsgBox("M3U file is corrupt!", MsgBoxStyle.Exclamation, "Corrupt File")
    End Try
  End Sub

  Private Sub OpenPLS(Path As String)
    Try
      Dim iniPLS As New INIReader(Path)
      If iniPLS.GetValue("playlist", "Version") = "2" Then
        For I As UInt64 = 1 To iniPLS.GetValue("playlist", "NumberOfEntries")
          Dim sTitle As String = iniPLS.GetValue("playlist", "Title" & I)
          Dim sPath As String = iniPLS.GetValue("playlist", "File" & I)
          Dim lTime As Double = iniPLS.GetValue("playlist", "Length" & I)
          If String.IsNullOrEmpty(sTitle) Then sTitle = IO.Path.GetFileNameWithoutExtension(sPath)
          If sPath.Substring(1, 2) <> ":\" And sPath.Substring(0, 7) <> "http://" Then sPath = Path.Substring(0, Path.LastIndexOf("\") + 1) & sPath
          AddToPlayList(sPath, sTitle, ConvertTimeVal(lTime), False)
        Next
        QueueFullPlayListData()
      Else
        MsgBox("Lime Seed does not recognize PLS v" & iniPLS.GetValue("playlist", "Version") & "!", MsgBoxStyle.Exclamation, "Corrupt File")
      End If
      iniPLS = Nothing
    Catch
      MsgBox("PLS file is corrupt!", MsgBoxStyle.Exclamation, "Corrupt File")
    End Try
  End Sub
#End Region

#Region "Save"
  Public Sub SavePlayList(Path As String)
    Select Case IO.Path.GetExtension(Path).ToLower
      Case ".llpl" : SaveBinaryPL(Path)
      Case ".m3u" : SaveM3U(Path)
      Case ".pls" : SavePLS(Path)
      Case Else : SaveTextPL(Path)
    End Select
  End Sub

  Private Sub SaveBinaryPL(Path As String)
    Dim bWriter As New IO.BinaryWriter(New IO.FileStream(Path, IO.FileMode.OpenOrCreate))
    bWriter.Write(CByte(2))
    bWriter.Write(CStr(txtPlayListTitle.Tag))
    bWriter.Write(CULng(dgvPlayList.RowCount))
    For Each row As DataGridViewRow In dgvPlayList.Rows
      bWriter.Write(CStr(row.Tag(0)))
      bWriter.Write(CStr(row.Tag(1)))
    Next
    bWriter.Close()
  End Sub

  Private Sub SaveTextPL(Path As String)
    Dim sWriter As IO.StreamWriter = New IO.StreamWriter(Path, False)
    For Each row As DataGridViewRow In dgvPlayList.Rows
      sWriter.WriteLine(row.Tag(0))
      sWriter.WriteLine(row.Tag(1))
    Next
    sWriter.Close()
  End Sub

  Private Sub SaveM3U(Path As String)
    Dim sWriter As IO.StreamWriter = New IO.StreamWriter(Path, False)
    sWriter.WriteLine("#EXTM3U")
    For Each row As DataGridViewRow In dgvPlayList.Rows
      sWriter.WriteLine("#EXTINF:" & RevertTimeVal(row.Tag(5)) & "," & CStr(row.Tag(1)))
      sWriter.WriteLine(CStr(row.Tag(0)))
    Next
    sWriter.Close()
  End Sub

  Private Sub SavePLS(Path As String)
    Dim sWriter As IO.StreamWriter = New IO.StreamWriter(Path, False)
    sWriter.WriteLine("[playlist]")
    sWriter.WriteLine()
    For I As Integer = 1 To dgvPlayList.RowCount
      sWriter.WriteLine("File" & I & "=" & dgvPlayList.Rows(I - 1).Tag(0))
      sWriter.WriteLine("Title" & I & "=" & dgvPlayList.Rows(I - 1).Tag(1))
      sWriter.WriteLine("Length" & I & "=" & RevertTimeVal(dgvPlayList.Rows(I - 1).Tag(5)))
      sWriter.WriteLine()
    Next I
    sWriter.WriteLine("NumberOfEntries=" & dgvPlayList.RowCount)
    sWriter.WriteLine()
    sWriter.WriteLine("Version=2")
    sWriter.WriteLine()
    sWriter.Close()
  End Sub

  Friend instanceID As Integer = 0

  Private Sub SaveTempPL()
    If dgvPlayList.RowCount > 0 Then
      Select Case mpPlayer.State
        Case Seed.ctlSeed.MediaState.mPlaying, Seed.ctlSeed.MediaState.mPaused
          SavePlayList(IO.Path.GetTempPath & "seed" & instanceID & ".llpl")
          My.Computer.FileSystem.WriteAllText(IO.Path.GetTempPath & "seed" & instanceID & ".ini", GetSelectedPlayListItem() & vbNewLine & mpPlayer.Position, False)
        Case Seed.ctlSeed.MediaState.mStopped
          If GetSelectedPlayListItem() = dgvPlayList.RowCount - 1 Then
            If My.Computer.FileSystem.FileExists(IO.Path.GetTempPath & "seed" & instanceID & ".llpl") Then My.Computer.FileSystem.DeleteFile(IO.Path.GetTempPath & "seed" & instanceID & ".llpl")
            If My.Computer.FileSystem.FileExists(IO.Path.GetTempPath & "seed" & instanceID & ".ini") Then My.Computer.FileSystem.DeleteFile(IO.Path.GetTempPath & "seed" & instanceID & ".ini")
          Else
            SavePlayList(IO.Path.GetTempPath & "seed" & instanceID & ".llpl")
            My.Computer.FileSystem.WriteAllText(IO.Path.GetTempPath & "seed" & instanceID & ".ini", GetSelectedPlayListItem() & vbNewLine & "0", False)
          End If
        Case Else
          SavePlayList(IO.Path.GetTempPath & "seed" & instanceID & ".llpl")
          If My.Computer.FileSystem.FileExists(IO.Path.GetTempPath & "seed" & instanceID & ".ini") Then My.Computer.FileSystem.DeleteFile(IO.Path.GetTempPath & "seed" & instanceID & ".ini")
      End Select
    Else
      If My.Computer.FileSystem.FileExists(IO.Path.GetTempPath & "seed" & instanceID & ".llpl") Then My.Computer.FileSystem.DeleteFile(IO.Path.GetTempPath & "seed" & instanceID & ".llpl")
      If My.Computer.FileSystem.FileExists(IO.Path.GetTempPath & "seed" & instanceID & ".ini") Then My.Computer.FileSystem.DeleteFile(IO.Path.GetTempPath & "seed" & instanceID & ".ini")
    End If
  End Sub

  Private Sub LoadTempPL()
    Dim Loaded As Boolean = False
    If My.Computer.FileSystem.FileExists(IO.Path.GetTempPath & "seed" & instanceID & ".llpl") Then
      dgvPlayList.Rows.Clear()
      Loaded = True
      OpenPlayList(IO.Path.GetTempPath & "seed" & instanceID & ".llpl")
      If My.Computer.FileSystem.FileExists(IO.Path.GetTempPath & "seed" & instanceID & ".ini") Then
        Dim iData As String = My.Computer.FileSystem.ReadAllText(IO.Path.GetTempPath & "seed" & instanceID & ".ini")
        If iData.Contains(vbNewLine) Then
          Dim sSel As String = Split(iData, vbNewLine)(0)
          Dim sPos As String = Split(iData, vbNewLine)(1)
          If IsNumeric(sSel) Then
            If Val(sSel) >= 0 And Val(sSel) < dgvPlayList.RowCount Then
              Dim iSel As Integer = CInt(sSel)
              dgvPlayList.Rows(iSel).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
              If IsNumeric(sPos) Then
                If sPos > 0 Then
                  Dim dPos As Double = CDbl(sPos)
                  Dim bMute As Boolean = mpPlayer.Mute
                  mpPlayer.Mute = True
                  OpenFile(dgvPlayList.Rows(iSel).Tag(0), True)
                  ThreadedInitialQueue()
                  If dPos >= mpPlayer.Duration Then
                    mpPlayer.Position = dPos
                    mpPlayer.Mute = bMute
                    cmdNextPL.PerformClick()
                  ElseIf dPos = 0 Then
                    mpPlayer.mpPause()
                    mpPlayer.Position = 0
                    mpPlayer.Mute = bMute
                    cmdPlayPause.Image = My.Resources.button_play
                  Else
                    mpPlayer.Position = dPos
                    mpPlayer.Mute = bMute
                    cmdPlayPause.Image = My.Resources.button_pause
                    mpPlayer.StateFade = False
                    mpPlayer.mpPlay()
                  End If
                End If
              End If
            End If
          End If
        End If
      End If
    End If
    Dim iItems As Integer = 0
    For iTest As Integer = 1 To 99
      Dim tPL_PATH As String = IO.Path.GetTempPath & "seed" & iTest & ".llpl"
      If My.Computer.FileSystem.FileExists(tPL_PATH) Then
        iItems += 1
      End If
    Next
    If iItems > 0 Then
      Dim iStart As Integer = 1
      If Not Loaded Then
        Dim tmpID As Integer = 1
        For iTest As Integer = 1 To 99
          Dim tPL_PATH As String = IO.Path.GetTempPath & "seed" & iTest & ".llpl"
          If My.Computer.FileSystem.FileExists(tPL_PATH) Then
            tmpID = iTest
            Exit For
          End If
        Next
        If My.Computer.FileSystem.FileExists(IO.Path.GetTempPath & "seed" & tmpID & ".llpl") Then
          dgvPlayList.Rows.Clear()
          Loaded = True
          OpenPlayList(IO.Path.GetTempPath & "seed" & tmpID & ".llpl")
          If My.Computer.FileSystem.FileExists(IO.Path.GetTempPath & "seed" & tmpID & ".ini") Then
            Dim iData As String = My.Computer.FileSystem.ReadAllText(IO.Path.GetTempPath & "seed" & tmpID & ".ini")
            If iData.Contains(vbNewLine) Then
              Dim sSel As String = Split(iData, vbNewLine)(0)
              Dim sPos As String = Split(iData, vbNewLine)(1)
              If IsNumeric(sSel) Then
                If Val(sSel) >= 0 And Val(sSel) < dgvPlayList.RowCount Then
                  Dim iSel As Integer = CInt(sSel)
                  dgvPlayList.Rows(iSel).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
                  If IsNumeric(sPos) Then
                    If sPos > 0 Then
                      Dim dPos As Double = CDbl(sPos)
                      Dim bMute As Boolean = mpPlayer.Mute
                      mpPlayer.Mute = True
                      OpenFile(dgvPlayList.Rows(iSel).Tag(0), True)
                      ThreadedInitialQueue()
                      If dPos >= mpPlayer.Duration Then
                        mpPlayer.Position = dPos
                        mpPlayer.Mute = bMute
                        cmdNextPL.PerformClick()
                      ElseIf dPos = 0 Then
                        mpPlayer.mpPause()
                        mpPlayer.Position = 0
                        mpPlayer.Mute = bMute
                        cmdPlayPause.Image = My.Resources.button_play
                      Else
                        mpPlayer.Position = dPos
                        mpPlayer.Mute = bMute
                        cmdPlayPause.Image = My.Resources.button_pause
                        mpPlayer.StateFade = False
                        mpPlayer.mpPlay()
                      End If
                    End If
                  End If
                End If
              End If
            End If
            My.Computer.FileSystem.DeleteFile(IO.Path.GetTempPath & "seed" & tmpID & ".ini")
          End If
          My.Computer.FileSystem.DeleteFile(IO.Path.GetTempPath & "seed" & tmpID & ".llpl")
        End If
        iStart = tmpID + 1
        iItems -= 1
        If iItems < 1 Then Exit Sub
      End If
      Dim sPromt As String = "There are " & iItems & " other PlayLists saved as well." & vbNewLine & "Would you like to open new players for them?"
      If iItems = 1 Then sPromt = "There is another PlayList saved as well." & vbNewLine & "Would you like to open a new player for it?"
      If MsgBox(sPromt, MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Multiple PlayLists") = MsgBoxResult.Yes Then
        Dim iInst As Integer = 0
        For iTest As Integer = iStart To 99
          Dim tPL_PATH As String = IO.Path.GetTempPath & "seed" & iTest & ".llpl"
          If My.Computer.FileSystem.FileExists(tPL_PATH) Then
            iInst += 1
            Dim frmTmp As New frmMain
            frmTmp.Tag = "NOCMD"
            frmTmp.instanceID = iInst
            frmTmp.Show()
            frmTmp.tmrCommandCycle.Stop()
            Dim commands As New ObjectModel.Collection(Of String)
            frmTmp.tmrCommandCycle.Tag = commands
            frmTmp.tmrCommandCycle.Start()
            frmTmp.dgvPlayList.Rows.Clear()
            frmTmp.OpenPlayList(tPL_PATH)
            Dim tPL_INI As String = IO.Path.GetTempPath & "seed" & iTest & ".ini"
            If My.Computer.FileSystem.FileExists(tPL_INI) Then
              Dim iData As String = My.Computer.FileSystem.ReadAllText(tPL_INI)
              If iData.Contains(vbNewLine) Then
                Dim sSel As String = Split(iData, vbNewLine)(0)
                Dim sPos As String = Split(iData, vbNewLine)(1)
                If IsNumeric(sSel) Then
                  If Val(sSel) >= 0 And Val(sSel) < frmTmp.dgvPlayList.RowCount Then
                    Dim iSel As Integer = CInt(sSel)
                    frmTmp.dgvPlayList.Rows(iSel).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
                    If IsNumeric(sPos) Then
                      If sPos > 0 Then
                        Dim dPos As Double = CDbl(sPos)
                        Dim bMute As Boolean = frmTmp.mpPlayer.Mute
                        frmTmp.mpPlayer.Mute = True
                        frmTmp.OpenFile(frmTmp.dgvPlayList.Rows(iSel).Tag(0), True)
                        frmTmp.ThreadedInitialQueue()
                        If dPos >= frmTmp.mpPlayer.Duration Then
                          frmTmp.mpPlayer.Position = dPos
                          frmTmp.mpPlayer.Mute = bMute
                          frmTmp.cmdNextPL.PerformClick()
                          frmTmp.cmdPlayPause.PerformClick()
                        ElseIf dPos = 0 Then
                          frmTmp.mpPlayer.mpPause()
                          frmTmp.mpPlayer.Position = 0
                          frmTmp.mpPlayer.Mute = bMute
                          frmTmp.cmdPlayPause.Image = My.Resources.button_play
                        Else
                          frmTmp.mpPlayer.Position = dPos
                          frmTmp.mpPlayer.mpPause()
                          frmTmp.mpPlayer.Mute = bMute
                          frmTmp.cmdPlayPause.Image = My.Resources.button_play
                        End If
                      End If
                    End If
                  End If
                End If
              End If
            End If
          End If
        Next
      End If
    End If
  End Sub
#End Region

#Region "Populate"
  Private Sub QueueFullPlayListData()
    For Each dgvx As DataGridViewRow In dgvPlayList.Rows
      If dgvx.Tag(5) = "--:--" Then
        If FastPC() Then
          Dim poolRun As New CallBack(AddressOf pool_Run)
          poolRun.BeginInvoke(dgvx.Tag, Nothing, Nothing)
          'Threading.ThreadPool.QueueUserWorkItem(New Threading.WaitCallback(AddressOf pool_Run), dgvx.Tag)
        Else
          pool_Run(dgvx.Tag)
          Application.DoEvents()
        End If
      End If
    Next
  End Sub

  Private Sub pool_Done(result As Object)
    SyncLock dgvPlayList
      Dim dgvItems = Array.FindAll(dgvPlayList.Rows.Cast(Of DataGridViewRow).ToArray, Function(item As DataGridViewRow) String.Compare(item.Tag(0), result(0), True) = 0)
      For I As Integer = 0 To dgvItems.Count - 1
        Dim dgvX As DataGridViewRow = dgvItems(I)
        If dgvX Is Nothing Then Exit Sub
        If result(1) = "DELETE" Then
          If dgvX IsNot Nothing Then dgvPlayList.Rows.Remove(dgvX)
        Else
          If dgvX IsNot Nothing Then
            Dim Path As String = result(0)
            Dim Title As String = result(1)
            Dim Artist As String = result(2)
            Dim Album As String = result(3)
            Dim Genre As String = result(4)
            Dim Length As String = result(5)
            dgvX.Cells(0).Value = Title
            dgvX.Cells(1).Value = Length
            dgvX.Tag = {Path, Title, Artist, Album, Genre, Length}
            Dim sTooltip As String = String.Empty
            sTooltip = "Title: " & Title
            If Not String.IsNullOrEmpty(Artist) AndAlso Artist <> UNKNOWN_ARTIST Then sTooltip &= vbNewLine & "Artist: " & Artist
            If Not String.IsNullOrEmpty(Album) AndAlso Album <> UNKNOWN_ALBUM Then sTooltip &= vbNewLine & "Album: " & Album
            If Not String.IsNullOrEmpty(Genre) AndAlso Genre <> "Other" Then sTooltip &= vbNewLine & "Genre: " & Genre
            If Not String.IsNullOrEmpty(Length) AndAlso Length <> "--:--" Then sTooltip &= vbNewLine & "Length: " & Length
            dgvX.Cells(0).ToolTipText = sTooltip
          End If
        End If
      Next
    End SyncLock
  End Sub

  Private Sub pool_Run(state As Object)
    Dim Path As String = state(0)
    Dim Title As String = String.Empty
    Dim Artist As String = String.Empty
    Dim Album As String = String.Empty
    Dim Genre As String = String.Empty
    Dim Duration As String = String.Empty
    Dim noDur As Boolean = False
    If IO.Path.GetExtension(Path).ToLower = ".mp3" Then
      Using ID3v2Tags As New Seed.clsID3v2(Path)
        If ID3v2Tags.HasID3v2Tag Then
          If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TP1")) Then Artist = ID3v2Tags.FindFrame("TP1")
          If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TT2")) Then Title = ID3v2Tags.FindFrame("TT2")
          If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TAL")) Then Album = ID3v2Tags.FindFrame("TAL")
          If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TCO")) Then Genre = ID3v2Tags.FindFrame("TCO")
        End If
      End Using
      Using ID3v1Tags As New Seed.clsID3v1(Path)
        If ID3v1Tags.HasID3v1Tag Then
          If String.IsNullOrEmpty(Title) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Title) Then Title = ID3v1Tags.Title
          If String.IsNullOrEmpty(Artist) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Artist) Then Artist = ID3v1Tags.Artist
          If String.IsNullOrEmpty(Album) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Album) Then Album = ID3v1Tags.Album
          If String.IsNullOrEmpty(Genre) AndAlso Not ID3v1Tags.Genre = &HFF Then Genre = Seed.clsID3v1.GenreName(ID3v1Tags.Genre)
        End If
      End Using
    ElseIf IO.Path.GetExtension(Path).ToLower = ".ogg" Or IO.Path.GetExtension(Path).ToLower = ".ogm" Or IO.Path.GetExtension(Path).ToLower = ".flac" Then
      Using cVorbis As New Seed.clsVorbis(Path)
        If cVorbis.HasVorbis Then
          If Not String.IsNullOrEmpty(cVorbis.Title) Then Title = cVorbis.Title
          If Not String.IsNullOrEmpty(cVorbis.Artist) Then Artist = cVorbis.Artist
          If Not String.IsNullOrEmpty(cVorbis.Album) Then Album = cVorbis.Album
          If Not String.IsNullOrEmpty(cVorbis.Genre) Then Genre = cVorbis.Genre
        End If
      End Using
    ElseIf IO.Path.GetExtension(Path).ToLower = ".mkv" Then
      Dim cMKV As New Seed.clsMKVHeaders(Path)
      If String.IsNullOrEmpty(Title) AndAlso Not String.IsNullOrEmpty(cMKV.SegmentInfo.Title) Then Title = cMKV.SegmentInfo.Title
      If String.IsNullOrEmpty(Title) Then
        For Each track In cMKV.TrackEntries
          If track.TrackType = 1 Then
            If Not String.IsNullOrEmpty(track.TrackName) Then Title = track.TrackName
            Exit For
          End If
        Next
      End If
    ElseIf IO.Path.GetExtension(Path).ToLower = ".cda" Or Path.Substring(1, 7) = ":\Track" Then
      Dim tNo As Integer = TrackToNo(Path)
      Using cDrive As New LimeSeed.CDDrive
        cDrive.Open(Path(0))
        If cDrive.IsCDReady And cDrive.Refresh And cDrive.GetNumTracks > 0 Then
          Duration = ConvertTimeVal(cDrive.TrackSize(tNo) / 176400)
        End If
      End Using
      noDur = True
    ElseIf IO.Path.GetExtension(Path).ToLower = ".jpg" Or IO.Path.GetExtension(Path).ToLower = ".jpeg" Or IO.Path.GetExtension(Path).ToLower = ".bmp" Or IO.Path.GetExtension(Path).ToLower = ".gif" Or IO.Path.GetExtension(Path).ToLower = ".png" Or IO.Path.GetExtension(Path).ToLower = ".db" Or IO.Path.GetExtension(Path).ToLower = ".ini" Then
      Dim delObj As Object = {Path, "DELETE"}
      Me.BeginInvoke(New CallBack(AddressOf pool_Done), delObj)
      Exit Sub
    End If
    If String.IsNullOrEmpty(Title) Then Title = IO.Path.GetFileNameWithoutExtension(Path)
    If String.IsNullOrEmpty(Artist) Then Artist = UNKNOWN_ARTIST
    If String.IsNullOrEmpty(Album) Then Album = UNKNOWN_ALBUM
    If Not noDur Then
      If IO.Path.GetExtension(Path).ToLower = ".swf" Or IO.Path.GetExtension(Path).ToLower = ".spl" Then
        Dim dDur As Double = mpPlayer.GetFileDuration(Path)
        Duration = dDur & " frames"
      Else
        Using mpDuration As New Seed.ctlSeed
          Dim dDur As Double = mpDuration.GetFileDuration(Path)
          If dDur <= 0 Then
            Dim delObj As Object = {Path, "DELETE"}
            Me.BeginInvoke(New CallBack(AddressOf pool_Done), delObj)
            Exit Sub
          End If
          Duration = ConvertTimeVal(dDur)
        End Using
      End If
    End If
    If String.IsNullOrEmpty(Title) Then Title = state(1)
    If String.IsNullOrEmpty(Artist) Then Artist = state(2)
    If String.IsNullOrEmpty(Album) Then Album = state(3)
    If String.IsNullOrEmpty(Genre) Then Genre = state(4)
    If String.IsNullOrEmpty(Duration) Then Duration = state(5)
    Dim infObj As Object = {Path, Title, Artist, Album, Genre, Duration}
    Try
      If Me IsNot Nothing AndAlso Not Me.IsDisposed Then Me.BeginInvoke(New CallBack(AddressOf pool_Done), infObj)
    Catch ex As Exception

    End Try
  End Sub
#End Region

  Private Sub StartPlayList()
    dgvPlayList_CellMouseDoubleClick(New Object, New DataGridViewCellMouseEventArgs(0, 0, 0, 0, New MouseEventArgs(Windows.Forms.MouseButtons.Left, 2, 0, 0, 0)))
  End Sub

  Private Sub AddDirToPlayList(Path As String, Optional AutoQueue As Boolean = True)
    For Each item In My.Computer.FileSystem.GetFiles(Path)
      AddToPlayList(item, , , False)
      Application.DoEvents()
    Next
    For Each item In My.Computer.FileSystem.GetDirectories(Path)
      AddDirToPlayList(item, False)
    Next
    If AutoQueue Then QueueFullPlayListData()
  End Sub

  Private Sub AddDirToPlayListAndMaybePlay(Item As String, Optional ByVal AutoQueue As Boolean = True)
    If dgvPlayList.Rows.Count > 0 Then
      AddDirToPlayList(Item, AutoQueue)
    Else
      AddDirToPlayList(Item, AutoQueue)
      Application.DoEvents()
      If AutoQueue Then
        Do Until dgvPlayList.Rows.Item(dgvPlayList.RowCount - 1).Tag(5) <> "--:--"
          Application.DoEvents()
          If dgvPlayList.Rows.Count = 0 Then Exit Sub
        Loop
      End If
      If dgvPlayList.Rows.Count > 0 Then
        dgvPlayList.Rows(0).Selected = True
        StartPlayList()
      End If
    End If
  End Sub

  Private Sub AddToPlayList(Path As String, Optional Title As String = Nothing, Optional Length As String = "--:--", Optional GetData As Boolean = True)
    Dim sExt As String = IO.Path.GetExtension(Path).ToLower
    Select Case sExt
      Case ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".dib", ".ini", ".db"
        'nothing
      Case Else
        If String.IsNullOrEmpty(Title) Then Title = IO.Path.GetFileNameWithoutExtension(Path)
        Dim Artist As String = UNKNOWN_ARTIST
        Dim Album As String = UNKNOWN_ALBUM
        Dim dgvX As DataGridViewRow = dgvPlayList.Rows(dgvPlayList.Rows.Add({Title, Length}))
        dgvX.Tag = {Path, Title, Artist, Album, "Other", Length}
        dgvX.Cells(0).ToolTipText = "Title: " & Title & vbNewLine & "Artist: " & Artist & vbNewLine & "Album: " & Album & vbNewLine & "Genre: Other" & vbNewLine & "Length: " & Length
        ThreadedQueue()
        If GetData Then
          If FastPC() And Not (IO.Path.GetExtension(Path).ToLower = ".swf" Or IO.Path.GetExtension(Path).ToLower = ".spl") Then
            Dim poolRun As New CallBack(AddressOf pool_Run)
            poolRun.BeginInvoke(dgvX.Tag, Nothing, Nothing)
            'Threading.ThreadPool.QueueUserWorkItem(New Threading.WaitCallback(AddressOf pool_Run), dgvX.Tag)
          Else
            pool_Run(dgvX.Tag)
            Application.DoEvents()
          End If
        End If
    End Select
  End Sub

  Private Sub RemoveFromPlayList(Index As Integer)
    dgvPlayList.Rows.Remove(dgvPlayList.Rows(Index))
    ThreadedQueue()
  End Sub

  Friend Function GetSelectedPlayListItem() As Integer
    Dim ret = (From dgvX As DataGridViewRow In dgvPlayList.Rows Where dgvX.Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText Select dgvX.Index)
    If ret.Count = 0 Then Return -1
    Return ret.FirstOrDefault
  End Function

  Private Sub QueueNextTrack()
    If Me.InvokeRequired Then
      Me.BeginInvoke(New MethodInvoker(AddressOf QueueNextTrack))
    Else
      Static LastQueue As String
      Dim SelItem As Integer = GetSelectedPlayListItem()
      If dgvPlayList.Rows.Count > 0 Then
        If dgvPlayList.Rows.Count - 1 > SelItem And SelItem >= 0 Then
          Dim sTrack As String = dgvPlayList.Rows(SelItem + 1).Tag(0)
          If My.Computer.FileSystem.FileExists(sTrack) And Not IO.Path.GetExtension(sTrack).ToLower = ".cda" Then
            If LastQueue <> sTrack Then
              LastQueue = sTrack
              mpPlayer.FileQueue(sTrack)
            End If
          Else
            LastQueue = Nothing
            mpPlayer.SetNoQueue()
          End If
        Else
          LastQueue = Nothing
          mpPlayer.SetNoQueue()
        End If
      Else
        LastQueue = Nothing
        mpPlayer.SetNoQueue()
      End If
    End If
  End Sub
#End Region
#End Region

#Region "GUI Routines"
#Region "Main Window"
#Region "Drag/Drop"
  Private Sub frmMain_DragDrop(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles Me.DragDrop
    DragDropEvent(sender, e)
  End Sub

  Private Sub frmMain_DragEnter(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles Me.DragEnter
    DragEnterEvent(sender, e)
  End Sub

  Private Sub frmMain_DragOver(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles Me.DragOver
    DragOverEvent(sender, e)
  End Sub
#End Region

  Private Sub frmMain_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
    tmrUpdate.Stop()
    tmrVis.Stop()
    tmrCommandCycle.Stop()
    If frmFS IsNot Nothing Then
      frmFS.Dispose()
      frmFS = Nothing
    End If
    frmText.Close()
    If Me.WindowState <> FormWindowState.Normal Then Me.WindowState = FormWindowState.Normal
    If Me.Width = mpPlayer.VideoWidth + (Me.Width - mpPlayer.Width) And Me.Height = mpPlayer.VideoHeight + (Me.Height - mpPlayer.Height) Then
      My.Settings.Size = New Drawing.Size(300, MAIN_HEIGHT)
    Else
      My.Settings.Size = Me.Size
    End If
    If mpPlayer.State <> Seed.ctlSeed.MediaState.mClosed Then mpPlayer.FileName = String.Empty
    My.Settings.Location = Me.Location
    My.Settings.Save()
  End Sub

  Private Sub frmMain_GotFocus(sender As Object, e As System.EventArgs) Handles Me.GotFocus
    If frmFS IsNot Nothing AndAlso frmFS.Visible Then frmFS.Focus()
  End Sub

  Private Sub frmMain_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
    If e.Button = Windows.Forms.MouseButtons.Left Then ClickDrag(Me.Handle)
  End Sub

  Private Sub frmMain_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
    DrawGlassText(True)
  End Sub

  Private Sub frmMain_Resize(sender As Object, e As System.EventArgs) Handles Me.Resize
    If VidThumb Then
      If mpPlayer.Size.Height > 0 Then
        cTask.UpdatePreview(mpPlayer)
      Else
        cTask.UpdatePreview(pnlMain)
      End If
    End If
  End Sub

  Private Sub frmMain_ResizeEnd(sender As Object, e As System.EventArgs) Handles Me.ResizeEnd
    If cmbAudTrack.Visible Then
      If cmbAudTrack.Width < AudWidth Then
        cmbAudTrack.DropDownWidth = AudWidth
      Else
        cmbAudTrack.DropDownWidth = cmbAudTrack.Width
      End If
    End If
    If cmbVidTrack.Visible Then
      If cmbVidTrack.Width < VidWidth Then
        cmbVidTrack.DropDownWidth = VidWidth
      Else
        cmbVidTrack.DropDownWidth = cmbVidTrack.Width
      End If
    End If
    If cmbChapters.Visible Then
      If cmbChapters.Width < ChapterWidth Then
        cmbChapters.DropDownWidth = ChapterWidth
      Else
        cmbChapters.DropDownWidth = cmbChapters.Width
      End If
    End If
    If cmbSubtitles.Visible Then
      If cmbSubtitles.Width < SubWidth Then
        cmbSubtitles.DropDownWidth = SubWidth
      Else
        cmbSubtitles.DropDownWidth = cmbSubtitles.Width
      End If
    End If
    If VidThumb Then
      If mpPlayer.Size.Height > 0 Then
        cTask.UpdatePreview(mpPlayer)
      Else
        cTask.UpdatePreview(pnlMain)
      End If
    End If
  End Sub

  Private Sub cTask_AppEvent(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles cTask.AppEvent
    Select Case sender
      Case "APP_Close" : Me.Close()
      Case "APP_Minimize" : Me.WindowState = FormWindowState.Minimized
      Case "APP_Maximize" : Me.WindowState = FormWindowState.Maximized
      Case "APP_Activated"
        If Me.WindowState = FormWindowState.Minimized Then Me.WindowState = FormWindowState.Normal
        Me.Activate()
    End Select
  End Sub

  Private Sub cTask_Button_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles cTask.Button_Click
    Select Case sender
      Case "Back" : cmdBackPL_Click(New Object, New EventArgs)
      Case "Next" : cmdNextPL_Click(New Object, New EventArgs)
      Case "PlayPause" : cmdPlayPause_Click(New Object, New EventArgs)
      Case "Stop" : cmdStop_Click(New Object, New EventArgs)
    End Select
  End Sub

#Region "App Menu"
  Protected Overrides Sub OnHandleCreated(e As System.EventArgs)
    MyBase.OnHandleCreated(e)
    Dim hSysMenu As IntPtr = NativeMethods.GetSystemMenu(Me.Handle, False)
    Me.TopMost = My.Settings.Topmost
    If Me.TopMost Then
      NativeMethods.InsertMenu(hSysMenu, 0, NativeMethods.MenuFlags.MF_STRING Or NativeMethods.MenuFlags.MF_CHECKED Or NativeMethods.MenuFlags.MF_BYPOSITION, TOPMOST_MENU_ID, TOPMOST_MENU_TEXT)
    Else
      NativeMethods.InsertMenu(hSysMenu, 0, NativeMethods.MenuFlags.MF_STRING Or NativeMethods.MenuFlags.MF_UNCHECKED Or NativeMethods.MenuFlags.MF_BYPOSITION, TOPMOST_MENU_ID, TOPMOST_MENU_TEXT)
    End If
    NativeMethods.InsertMenu(hSysMenu, 1, NativeMethods.MenuFlags.MF_SEPARATOR Or NativeMethods.MenuFlags.MF_BYPOSITION, 0, String.Empty)
  End Sub
  Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
    MyBase.WndProc(m)
    If m.Msg = NativeMethods.WM_SYSCOMMAND Then
      Select Case m.WParam.ToInt64
        Case TOPMOST_MENU_ID
          Me.TopMost = Not Me.TopMost
          My.Settings.Topmost = Me.TopMost
          Dim hSysMenu As IntPtr = NativeMethods.GetSystemMenu(Me.Handle, False)
          If Me.TopMost Then
            NativeMethods.ModifyMenu(hSysMenu, TOPMOST_MENU_ID, NativeMethods.MenuFlags.MF_STRING Or NativeMethods.MenuFlags.MF_CHECKED, TOPMOST_MENU_ID, TOPMOST_MENU_TEXT)
          Else
            NativeMethods.ModifyMenu(hSysMenu, TOPMOST_MENU_ID, NativeMethods.MenuFlags.MF_STRING Or NativeMethods.MenuFlags.MF_UNCHECKED, TOPMOST_MENU_ID, TOPMOST_MENU_TEXT)
          End If
      End Select
    End If
  End Sub
#End Region
#End Region

#Region "View Tab Strip"
  Private Sub tbsView_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles tbsView.SelectedIndexChanged
    Static lastSize As Drawing.Size
    If tbsView.SelectedTab Is tabRipper Then ripBox.RefreshDriveList()
    Dim BorderSize As New Drawing.Size(Me.Width - mpPlayer.Width, Me.Height - mpPlayer.Height)
    Select Case tbsView.SelectedTab.Handle.ToInt64
      Case tabArt.Handle.ToInt64
        lastSize = Me.Size
        If artList.Visible Then
          Me.Size = New Drawing.Size(600, 450)
          Me.Top += (lastSize.Height - Me.Height)
          KeepInTheScreen()
          Exit Sub
        End If
        If mnuArtShow.Checked Then
          If FileArt IsNot Nothing Then
            lastSize = Me.Size
            Me.Width = Me.MinimumSize.Width
            Dim lSize As Drawing.Size
            lSize.Width = pctAlbumArt.Width
            lSize.Height = FileArt.Art.Height / FileArt.Art.Width * lSize.Width
            SetWindowSize(lSize.Width, lSize.Height, Me.Width - pctAlbumArt.Width, Me.Height - pctAlbumArt.Height)
            'Me.Top += (lastSize.Height - Me.Height)
            KeepInTheScreen()
            Exit Sub
          End If
        End If
        If methodDraw IsNot Nothing Then
          Me.Size = New Drawing.Size(550, 400)
          Me.Top += (lastSize.Height - Me.Height)
          KeepInTheScreen()
          Exit Sub
        End If
        Me.Size = New Drawing.Size(Me.MinimumSize.Width, Me.MinimumSize.Height + tbsView.GetTabRect(0).Height + 4)
        Me.Top += (lastSize.Height - Me.Height)
      Case tabVideo.Handle.ToInt64
        If mpPlayer.HasVid Then
          lastSize = Me.Size
          SetWindowSize(mpPlayer.VideoWidth, mpPlayer.VideoHeight, Me.Width - mpPlayer.Width, Me.Height - mpPlayer.Height)
        End If
      Case tabRipper.Handle.ToInt64
        lastSize = Me.Size
        Me.Size = New Drawing.Size(720, 540)
        Me.Top += (lastSize.Height - Me.Height)
      Case Else
        If lastSize.Height > 0 Then
          Me.Top -= (lastSize.Height - Me.Height)
          Me.Height = lastSize.Height
          Me.Width = lastSize.Width
        End If
    End Select
    KeepInTheScreen()
  End Sub

  Private Sub KeepInTheScreen()
    If Me.Top < Screen.FromRectangle(Me.DesktopBounds).WorkingArea.Top Then Me.Top = Screen.FromRectangle(Me.DesktopBounds).WorkingArea.Top
    If Me.Left < Screen.FromRectangle(Me.DesktopBounds).WorkingArea.Left Then Me.Left = Screen.FromRectangle(Me.DesktopBounds).WorkingArea.Left
    If Me.Bottom > Screen.FromRectangle(Me.DesktopBounds).WorkingArea.Bottom Then Me.Top = Screen.FromRectangle(Me.DesktopBounds).WorkingArea.Bottom - Me.Height
    If Me.Right > Screen.FromRectangle(Me.DesktopBounds).WorkingArea.Right Then Me.Left = Screen.FromRectangle(Me.DesktopBounds).WorkingArea.Right - Me.Width
  End Sub
#End Region

#Region "Player"
#Region "Drag/Drop"
  Private Sub mpPlayer_DragDrop(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles mpPlayer.DragDrop
    DragDropEvent(sender, e)
  End Sub

  Private Sub mpPlayer_DragEnter(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles mpPlayer.DragEnter
    DragEnterEvent(sender, e)
  End Sub

  Private Sub mpPlayer_DragOver(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles mpPlayer.DragOver
    DragOverEvent(sender, e)
  End Sub
#End Region

  Private Sub mpPlayer_CantOpen(Message As String) Handles mpPlayer.CantOpen
    MsgBox("Can't Open File:" & vbNewLine & Message, MsgBoxStyle.Exclamation, "Media Error")
  End Sub

  Private Sub mpPlayer_EndOfFile(Status As Integer) Handles mpPlayer.EndOfFile
    Dim finalAct As Integer = -1
    If artList.Visible Then
      artList.Visible = False
      pctAlbumArt.Visible = True
      Dim lastHeight As Integer = Me.Height
      Me.Size = artList.Tag
      Me.Size = New Drawing.Size(Me.MinimumSize.Width, Me.MinimumSize.Height + tbsView.GetTabRect(0).Height + 4)
      Me.Top += (lastHeight - Me.Height)
      If pctAlbumArt.Tag IsNot Nothing Then
        FileArt = pctAlbumArt.Tag
        pctAlbumArt.Tag = Nothing
      End If
    End If
    If Status = -1 Then
      If dgvPlayList.Rows.Count > 0 Then
        Dim SelItem As Integer = GetSelectedPlayListItem()
        If SelItem >= 0 Then
          If SelItem < dgvPlayList.Rows.Count - 1 Then
            dgvPlayList.Rows(SelItem).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
            dgvPlayList.Rows(SelItem + 1).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
            'If dgvPlayList.Rows.Count > SelItem + 2 Then
            '  mpPlayer.FileQueue(dgvPlayList.Rows(SelItem + 2).Tag(0))
            'Else
            '  mpPlayer.SetNoQueue()
            'End If
            ThreadedQueue()
          Else
            Dim iLoop As Integer = cmdLoopPL.Tag
            Select Case iLoop
              Case 1 : finalAct = 1
              Case 2 : finalAct = 2
              Case 3 : finalAct = 3
              Case 4 : finalAct = 4
              Case 5 : finalAct = 5
            End Select
          End If
        End If
      End If
    Else
      If dgvPlayList.Rows.Count > 0 Then
        Dim SelItem As Integer = GetSelectedPlayListItem()
        If SelItem >= 0 Then
          If SelItem < dgvPlayList.Rows.Count - 1 Then
            dgvPlayList.Rows(SelItem).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
            dgvPlayList.Rows(SelItem + 1).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
            OpenFile(dgvPlayList.Rows(SelItem + 1).Tag(0), True)
            ThreadedInitial()
            Exit Sub
          Else
            Dim iLoop As Integer = cmdLoopPL.Tag
            Select Case iLoop
              Case 1 : finalAct = 1
              Case 2 : finalAct = 2
              Case 3 : finalAct = 3
              Case 4 : finalAct = 4
              Case 5 : finalAct = 5
            End Select
          End If
        End If
      End If
      cmdPlayPause.Image = My.Resources.button_play
      mpPlayer.StateFade = False
      mpPlayer.mpStop()
      mpPlayer.Position = 0
    End If
    FirstInit = True
    ThreadedInitial()
    Select Case finalAct
      Case 1
        Application.DoEvents()
        StartPlayList()
      Case 2
        ClearPlayList()
        SaveTempPL()
        Application.DoEvents()
        mnuExit.PerformClick()
      Case 3
        ClearPlayList()
        SaveTempPL()
        Application.DoEvents()
        Process.Start("shutdown", "/r /t 0 /d p:0:0 /f")
      Case 4
        ClearPlayList()
        SaveTempPL()
        Application.DoEvents()
        Process.Start("shutdown", "/s /t 0 /d p:0:0 /f")
      Case 5
        If PowerProfile.CanHibernate Then
          ClearPlayList()
          SaveTempPL()
          Application.DoEvents()
          Process.Start("shutdown", "/h /t 0 /d p:0:0 /f")
        End If
    End Select
  End Sub

  Friend Sub ThreadedInitialQueue()
    Dim iqInvoker As New MethodInvoker(AddressOf InitialQueue)
    iqInvoker.BeginInvoke(Nothing, Nothing)
  End Sub

  Friend Sub InitialQueue()
    InitialData()
    QueueNextTrack()
  End Sub

  Friend Sub ThreadedQueue()
    Dim queueInvoker As New MethodInvoker(AddressOf QueueNextTrack)
    queueInvoker.BeginInvoke(Nothing, Nothing)
  End Sub

  Friend Sub ThreadedInitial()
    Dim initialInvoker As New MethodInvoker(AddressOf InitialData)
    initialInvoker.BeginInvoke(Nothing, Nothing)
  End Sub

  Private Sub mpPlayer_MediaError(e As Seed.ctlSeed.MediaErrorEventArgs) Handles mpPlayer.MediaError
    Debug.Print(e.E.Message & " in " & e.Funct)
  End Sub

  Private Sub mpPlayer_VidClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles mpPlayer.VidClick
    If e.Button = Windows.Forms.MouseButtons.Right Then mnuVideo.Show(MousePosition)
  End Sub

  Private Sub mpPlayer_VidDoubleClick(sender As Object, e As EventArgs) Handles mpPlayer.VidDoubleClick
    cmdFullScreen_Click(sender, e)
  End Sub

  Private Sub mpPlayer_MouseWheel(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles mpPlayer.MouseWheel
    If e.Delta > 0 Then
      bpgVolume.Value += (bpgVolume.Maximum - bpgVolume.Minimum) / 20
    ElseIf e.Delta < 0 Then
      bpgVolume.Value -= (bpgVolume.Maximum - bpgVolume.Minimum) / 20
    End If
    If frmFS IsNot Nothing AndAlso frmFS.Visible Then If Not frmFS.bpgVolume.Value = bpgVolume.Value Then frmFS.bpgVolume.Value = bpgVolume.Value
  End Sub
#End Region

  Private Sub CursorText(Message As String)
    If Not frmText.Visible Then frmText.Show(Me)
    frmText.SetText(Message)
    Me.Focus()
  End Sub

  Private Sub SetFFDShowVals()
    If cmbVidTrack.Visible AndAlso Not cmbVidTrack.DroppedDown Then cmbVidTrack.SelectedIndex = 0
    If cmbAudTrack.Visible AndAlso Not cmbAudTrack.DroppedDown And ffAPI.AudioStream > 0 Then cmbAudTrack.SelectedIndex = ffAPI.AudioStream - IIf(cmbVidTrack.Items.Count > 1, cmbVidTrack.Items.Count + 1, 1)
    If cmbSubtitles.Visible AndAlso Not cmbSubtitles.DroppedDown And ffAPI.SubtitleStream > 0 Then cmbSubtitles.SelectedIndex = ffAPI.SubtitleStream - cmbAudTrack.Items.Count - IIf(cmbVidTrack.Items.Count > 1, cmbVidTrack.Items.Count + 1, 1)
  End Sub

  Private Sub SetTaskbarStyle([Style] As TaskbarLib.TBPFLAG)
    If taskBar IsNot Nothing Then taskBar.SetProgressState(Me.Handle, Style)
  End Sub

  Private Sub SetPlayPause(Play As Boolean, ByVal Enabled As Boolean)
    If Play Then
      If cmdPlayPause.Tag <> 1 Then
        cmdPlayPause.Image = My.Resources.button_play
        cmdPlayPause.Tag = 1
        If cTask IsNot Nothing Then cTask.IsPause = False
      End If
    Else
      If cmdPlayPause.Tag <> 0 Then
        cmdPlayPause.Image = My.Resources.button_pause
        cmdPlayPause.Tag = 0
        If cTask IsNot Nothing Then cTask.IsPause = True
      End If
    End If
    If Not cmdPlayPause.Enabled = Enabled Then cmdPlayPause.Enabled = Enabled
    If cTask IsNot Nothing Then If Not cTask.PlayPauseEnabled = Enabled Then cTask.PlayPauseEnabled = Enabled
  End Sub

  Private Sub SetStop(Enabled As Boolean)
    If Not cmdStop.Enabled = Enabled Then cmdStop.Enabled = Enabled
    If cTask IsNot Nothing Then If Not cTask.StopEnabled = Enabled Then cTask.StopEnabled = Enabled
  End Sub

  Private Sub SetProps(encoding As Integer, channels As Integer, bitrate As Integer, Optional sEncoding As String = Nothing, Optional sChannels As String = Nothing, Optional sBitrate As String = Nothing)
    Select Case encoding
      Case 0 : pctBitrate.Image = pctBitrate.Image
      Case 1 : pctBitrate.Image = My.Resources.ba
      Case 2 : pctBitrate.Image = My.Resources.bc
      Case 3 : pctBitrate.Image = My.Resources.bv
      Case 4 : pctBitrate.Image = My.Resources.bu
      Case Else : pctBitrate.Image = Nothing
    End Select
    Select Case channels
      Case 0 : pctChannels.Image = pctChannels.Image
      Case 1 : pctChannels.Image = My.Resources.c1_0
      Case 2 : pctChannels.Image = My.Resources.c2_0
      Case -2 : pctChannels.Image = My.Resources.c2_0j
      Case 3 : pctChannels.Image = My.Resources.c2_1
      Case 4 : pctChannels.Image = My.Resources.c4_0
      Case 5 : pctChannels.Image = My.Resources.c5_0
      Case 6 : pctChannels.Image = My.Resources.c5_1
      Case 7 : pctChannels.Image = My.Resources.c6_1
      Case 8 : pctChannels.Image = My.Resources.c7_1
      Case Else : pctChannels.Image = Nothing
    End Select
    Select Case bitrate
      Case -2 : pctQuality.Image = pctQuality.Image
      Case 0 : pctQuality.Image = My.Resources.q0
      Case 1 : pctQuality.Image = My.Resources.q1
      Case 2 : pctQuality.Image = My.Resources.q2
      Case 3 : pctQuality.Image = My.Resources.q3
      Case 4 : pctQuality.Image = My.Resources.q4
      Case 5 : pctQuality.Image = My.Resources.q5
      Case 6 : pctQuality.Image = My.Resources.q6
      Case 7 : pctQuality.Image = My.Resources.q7
      Case 8 : pctQuality.Image = My.Resources.q8
      Case 9 : pctQuality.Image = My.Resources.q9
      Case Else : pctQuality.Image = Nothing
    End Select
    If encoding <> 0 Then
      If String.IsNullOrEmpty(sEncoding) Or encoding = -1 Then
        ttDisp.SetToolTip(pctBitrate, Nothing)
      Else
        Dim sTTEnd As String = String.Empty
        If mpPlayer.HasAud And mpPlayer.HasVid Then
          sTTEnd = vbNewLine & "(Double-Click for DirectSound Device properties" & vbNewLine & "Double-Right-Click for Video Renderer properties)"
        ElseIf mpPlayer.HasAud Then
          sTTEnd = vbNewLine & "(Double-Click for DirectSound Device properties)"
        ElseIf mpPlayer.HasVid Then
          sTTEnd = vbNewLine & "(Double-Right-Click for Video Renderer properties)"
        End If
        ttDisp.SetToolTip(pctBitrate, "Encoding: " & sEncoding & sTTEnd)
      End If
    End If
    If bitrate <> -2 Then
      If String.IsNullOrEmpty(sBitrate) Or bitrate = -1 Then
        ttDisp.SetToolTip(pctQuality, Nothing)
      Else
        ttDisp.SetToolTip(pctQuality, "Bitrate: " & sBitrate & IIf(mpPlayer.HasVid, vbNewLine & "(Double-Click for FFDShow Video Decoder)", Nothing))
      End If
    End If
    If channels <> 0 Then
      If String.IsNullOrEmpty(sChannels) Or channels = -1 Then
        ttDisp.SetToolTip(pctChannels, Nothing)
      Else
        ttDisp.SetToolTip(pctChannels, "Channels: " & sChannels & IIf(mpPlayer.HasAud, vbNewLine & "(Double-Click for FFDShow Audio Decoder)", Nothing))
      End If
    End If
  End Sub

  Private Sub SetTabs(Art As Boolean, ByVal Vid As Boolean)
    Dim Redo As Boolean = False
    If Art Then
      If Not tbsView.TabPages.Contains(tabArt) Then Redo = True
    Else
      If tbsView.TabPages.Contains(tabArt) Then Redo = True
    End If
    If Vid Then
      If Not tbsView.TabPages.Contains(tabVideo) Then Redo = True
    Else
      If tbsView.TabPages.Contains(tabVideo) Then Redo = True
    End If
    If Redo Then
      If tbsView.TabPages.Contains(tabArt) Then tbsView.TabPages.Remove(tabArt)
      If tbsView.TabPages.Contains(tabVideo) Then tbsView.TabPages.Remove(tabVideo)
      If Art Then tbsView.TabPages.Insert(0, tabArt)
      If Vid Then tbsView.TabPages.Insert(IIf(Art, 1, 0), tabVideo)
    End If
  End Sub

  Private Sub SetCombos(Video As Boolean, ByVal Audio As Boolean, ByVal Chapters As Boolean, ByVal Subtitles As Boolean)
    Dim Percent As Integer = 0
    If Video Then
      cmbVidTrack.Visible = True
      cmbVidTrack.Items.Clear()
      mnuVideoTrack.Visible = True
      mnuVideoTrack.DropDownItems.Clear()
      Percent = 100
    Else
      cmbVidTrack.Tag = Nothing
      cmbVidTrack.Visible = False
      cmbVidTrack.Items.Clear()
      mnuVideoTrack.Visible = False
      mnuVideoTrack.DropDownItems.Clear()
    End If
    If Audio Then
      cmbAudTrack.Visible = True
      cmbAudTrack.Items.Clear()
      mnuAudioTrack.Visible = True
      mnuAudioTrack.DropDownItems.Clear()
      If Percent = 0 Then
        Percent = 100
      ElseIf Percent = 100 Then
        Percent = 50
      End If
    Else
      cmbAudTrack.Tag = Nothing
      cmbAudTrack.Visible = False
      cmbAudTrack.Items.Clear()
      mnuAudioTrack.Visible = False
      mnuAudioTrack.DropDownItems.Clear()
    End If
    If Chapters Then
      cmbChapters.Visible = True
      cmbChapters.Items.Clear()
      mnuChapterTrack.Visible = True
      mnuChapterTrack.DropDownItems.Clear()
      If Percent = 0 Then
        Percent = 100
      ElseIf Percent = 100 Then
        Percent = 50
      ElseIf Percent = 50 Then
        Percent = 33
      End If
    Else
      cmbChapters.Tag = Nothing
      cmbChapters.Visible = False
      cmbChapters.Items.Clear()
      mnuChapterTrack.Visible = False
      mnuChapterTrack.DropDownItems.Clear()
    End If
    If Subtitles Then
      cmbSubtitles.Visible = True
      cmbSubtitles.Items.Clear()
      mnuSubtitleTrack.Visible = True
      mnuSubtitleTrack.DropDownItems.Clear()
      If Percent = 0 Then
        Percent = 100
      ElseIf Percent = 100 Then
        Percent = 50
      ElseIf Percent = 50 Then
        Percent = 33
      ElseIf Percent = 33 Then
        Percent = 25
      End If
    Else
      cmbSubtitles.Tag = Nothing
      cmbSubtitles.Visible = False
      cmbSubtitles.Items.Clear()
      mnuSubtitleTrack.Visible = False
      mnuSubtitleTrack.DropDownItems.Clear()
    End If
    If Video Or Audio Or Chapters Or Subtitles Then
      pnlVidOpts.Visible = True
      If Video Then
        pnlVidOpts.ColumnStyles(0).SizeType = SizeType.Percent
        pnlVidOpts.ColumnStyles(0).Width = Percent
      Else
        pnlVidOpts.ColumnStyles(0).SizeType = SizeType.Absolute
        pnlVidOpts.ColumnStyles(0).Width = 0
      End If
      If Audio Then
        pnlVidOpts.ColumnStyles(1).SizeType = SizeType.Percent
        pnlVidOpts.ColumnStyles(1).Width = Percent
      Else
        pnlVidOpts.ColumnStyles(1).SizeType = SizeType.Absolute
        pnlVidOpts.ColumnStyles(1).Width = 0
      End If
      If Chapters Then
        pnlVidOpts.ColumnStyles(2).SizeType = SizeType.Percent
        pnlVidOpts.ColumnStyles(2).Width = Percent
      Else
        pnlVidOpts.ColumnStyles(2).SizeType = SizeType.Absolute
        pnlVidOpts.ColumnStyles(2).Width = 0
      End If
      If Subtitles Then
        pnlVidOpts.ColumnStyles(3).SizeType = SizeType.Percent
        pnlVidOpts.ColumnStyles(3).Width = Percent
      Else
        pnlVidOpts.ColumnStyles(3).SizeType = SizeType.Absolute
        pnlVidOpts.ColumnStyles(3).Width = 0
      End If
    Else
      pnlVidOpts.Visible = False
    End If
  End Sub

  Private Sub SetVideoResolution(width As Integer, height As Integer)
    If (width = -1 Or height = -1) Then
      mpPlayer.VideoWidth = -1
      mpPlayer.VideoHeight = -1
      Application.DoEvents()
      If width = -1 Then width = mpPlayer.VideoWidth
      If height = -1 Then height = mpPlayer.VideoHeight
    End If
    'Debug.Print("Resoultion initialized as: " & width & "x" & height)
    VidSize.Width = width
    VidSize.Height = height
    SetVideoSize(width, height)
  End Sub

  Private Sub SetVideoSize(width As Integer, height As Integer)
    If ffAPI IsNot Nothing Then
      If width = VidSize.Width And height = VidSize.Height Then
        ffAPI.DoResize = False
      Else
        If width Mod 2 = 1 Then width += 1
        If height Mod 2 = 1 Then height += 1
        ffAPI.DoResize = False
        ffAPI.ResizeModeFreeResize = True
        ffAPI.ResizeHorizontal = width
        ffAPI.ResizeVertical = height
        ffAPI.DoResize = True
        ffAPI.ResizeKeepAspectRatio = mnuRatioForce.Checked ' False
      End If
    End If
    'Debug.Print("Resoultion set to: " & Width & "x" & Height)
    mpPlayer.VideoWidth = width
    mpPlayer.VideoHeight = height
    SetWindowSize(width, height, (Me.Width - mpPlayer.Width), (Me.Height - mpPlayer.Height))
    frmMain_ResizeEnd(New Object, New EventArgs)
  End Sub

  Private Sub SetScaledVideoSize(width As Integer, height As Integer, Scale As Double)
    If ffAPI IsNot Nothing Then
      If width = VidSize.Width And height = VidSize.Height Then
        ffAPI.DoResize = False
      Else
        If width Mod 2 = 1 Then width += 1
        If height Mod 2 = 1 Then height += 1
        ffAPI.DoResize = False
        ffAPI.ResizeModeFreeResize = True
        ffAPI.ResizeHorizontal = width
        ffAPI.ResizeVertical = height
        ffAPI.DoResize = True
        ffAPI.ResizeKeepAspectRatio = mnuRatioForce.Checked ' False
      End If
    End If
    'Debug.Print("Resoultion set to: " & Width & "x" & Height)
    mpPlayer.VideoWidth = width * Scale
    mpPlayer.VideoHeight = height * Scale
    SetWindowSize(width * Scale, height * Scale, (Me.Width - mpPlayer.Width), (Me.Height - mpPlayer.Height))
    frmMain_ResizeEnd(New Object, New EventArgs)
  End Sub

  Private Sub SetWindowSize(vW As Integer, vH As Integer, bW As Integer, bH As Integer)
    'Debug.Print("Video Size: " & vW & "x" & vH)
    'Debug.Print("Border Size: " & bW & "x" & bH)
    Dim vbW As Integer = vW + bW
    Dim vbH As Integer = vH + bH
    'Debug.Print("Requested Size: " & vbW & "x" & vbH)
    Dim sW As Integer = Screen.FromRectangle(Me.DesktopBounds).WorkingArea.Width
    Dim sH As Integer = Screen.FromRectangle(Me.DesktopBounds).WorkingArea.Height
    Dim sArea As Drawing.Rectangle = Screen.FromRectangle(Me.DesktopBounds).WorkingArea
    Dim lastHeight As Integer = Me.Height
    Dim mW, mH As Integer
    Dim okW, okH As Boolean
    okW = sW > vbW
    okH = sH > vbH

    If okW And okH Then
      mW = vbW
      mH = vbH
    ElseIf okW Then
      mH = sH
      mW = (vW / vH) * (sH - bH) + bW
      If mW > sW Then
        mW = sW
        mH = sH
      End If
    ElseIf okH Then
      mW = sW
      mH = (vH / vW) * (sW - bW) + bH
      If mH > sH Then
        mH = sH
        mW = sW
      End If
    Else
      mH = sH
      mW = (vW / vH) * (sH - bH) + bW
      If mW > sW Then
        mW = sW
        mH = (vH / vW) * (sW - bW) + bH
        If mH > sH Then
          mH = sH
          mW = sW
        End If
      End If
    End If
    'Debug.Print("Final Size: " & mW & "x" & mH)
    Me.Size = New Drawing.Size(mW, mH)

    Dim newPos As Drawing.Rectangle = Me.DesktopBounds

    newPos.Y = newPos.Top - (newPos.Height - lastHeight)
    newPos.X = Me.Left
    If newPos.Top < sArea.Top Then newPos.Y = sArea.Top
    If newPos.Left < sArea.Left Then newPos.X = sArea.Left

    If newPos.Bottom > sArea.Bottom Then newPos.Y = sArea.Bottom - newPos.Height
    If newPos.Right > sArea.Right Then newPos.X = sArea.Right - newPos.Width

    Me.DesktopBounds = newPos
  End Sub

  Private Sub CheckDiscs()
    Dim cdDisc As Boolean = False
    For Each drive In My.Computer.FileSystem.Drives
      If drive.DriveType = IO.DriveType.CDRom Then
        If drive.IsReady AndAlso drive.DriveFormat = "CDFS" Then
          Using drv As New CDDrive
            drv.Open(drive.RootDirectory.FullName(0))
            If drv.IsCDReady And drv.Refresh And drv.GetNumAudioTracks > 0 Then
              cdDisc = True
              Exit For
            End If
          End Using
          'cdDisc = True
          'Exit For
        End If
      End If
    Next
    If cdDisc Then
      If Not tbsView.TabPages.Contains(tabRipper) Then
        tbsView.TabPages.Add(tabRipper)
      End If
      If tbsView.SelectedTab Is tabRipper Then
        Application.DoEvents()
        ripBox.RefreshDriveList()
      End If
    Else
      If tbsView.TabPages.Contains(tabRipper) Then
        ripBox.CancelActions()
        tbsView.TabPages.Remove(tabRipper)
      End If
    End If
  End Sub

  Private Sub InitialData()
    If Me.InvokeRequired Then
      Me.BeginInvoke(New MethodInvoker(AddressOf InitialData))
    Else
      Dim sTitleVal As String = IO.Path.GetFileNameWithoutExtension(mpPlayer.FileName)
      Dim sArtistVal As String = UNKNOWN_ARTIST
      If FirstInit Then
        VidSize = Drawing.Size.Empty
        volControl.SetSound(False)
        volControl.SetVolume(100)
      End If
      If bCD Then
        EnableScreenSaver(True)
        SetTabs(True, False)
        If GetSelectedPlayListItem() >= 0 Then
          Dim dTag = dgvPlayList.Rows(GetSelectedPlayListItem).Tag
          If dTag IsNot Nothing Then
            If String.IsNullOrEmpty(dTag(2)) Or dTag(2) = UNKNOWN_ARTIST Then
              If String.IsNullOrEmpty(dTag(1)) Then
                sTitleVal = "Track " & cCD.CurrentTrack
              Else
                sTitleVal = dTag(1)
              End If
            Else
              If String.IsNullOrEmpty(dTag(1)) Then
                sTitleVal = "Track " & cCD.CurrentTrack
                sArtistVal = dTag(2)
              Else
                sTitleVal = dTag(1)
                sArtistVal = dTag(2)
              End If
            End If
          Else
            sTitleVal = "Track " & cCD.CurrentTrack
          End If
        Else
          sTitleVal = "Track " & cCD.CurrentTrack
        End If
        If sTitleVal = IO.Path.GetFileNameWithoutExtension(mpPlayer.FileName) Then sTitleVal = "Track " & cCD.CurrentTrack
        'If FileArt IsNot Nothing And Me.WindowState = FormWindowState.Normal And tbsView.SelectedTab Is tabArt Then tbsView_SelectedIndexChanged(New Object, New EventArgs)
        SetProps(4, 2, -1, "CD Audio", "Stereo")
        SetCombos(False, False, False, False)
        VidThumb = False
        cmdLoop.Enabled = True
        cmdLoop.Image = My.Resources.button_eject
        LabelShortcuts()
      ElseIf String.IsNullOrEmpty(mpPlayer.FileName) Then
        'EMPTY
        sTitleVal = String.Empty
        sArtistVal = String.Empty
        FileArt = Nothing
        SetCombos(False, False, False, False)
        SetProps(-1, -1, -1)
        VidThumb = False
        If ffAPI IsNot Nothing Then ffAPI.DoResize = False
        EnableScreenSaver(True)
        cmdLoop.Enabled = False
        cmdLoop.Image = My.Resources.button_loop_off
        LabelShortcuts()
        pctAlbumArt.Image = Nothing
        If frmFS IsNot Nothing Then frmFS.pctVideo.Image = Nothing
      ElseIf mpPlayer.HasVid Or bDVD Then
        'sArtistVal = "Video"
        'VIDEO
        If FirstInit Then
          EnableScreenSaver(False)
          SetTabs(False, True)
          tbsView.SelectedTab = tabVideo
          FileArt = Nothing
          If LoadFFDShow() Then
            ffAPI.DoResize = False
            Application.DoEvents()
            'Debug.Print("Do Resize: " & ffAPI.DoResize)
            'Debug.Print("Dimensions: " & ffAPI.ResizeHorizontal & "x" & ffAPI.ResizeVertical)
            'Debug.Print("Keep Aspect: " & ffAPI.ResizeKeepAspectRatio)
            'Debug.Print("Fit to screen: " & ffAPI.ResizeModeFitToScreen)
            'Debug.Print("Free Resize: " & ffAPI.ResizeModeFreeResize)
          End If
        End If
        'FileTitle = IO.Path.GetFileNameWithoutExtension(mpPlayer.FileName)
        If bDVD Then
          sTitleVal = My.Computer.FileSystem.GetDriveInfo(mpPlayer.FileName.Substring(0, 3)).VolumeLabel
          If FirstInit Then
            cmdLoop.Enabled = True
            cmdLoop.Image = My.Resources.button_eject
            LabelShortcuts()
          End If
          If Not cmbAudTrack.DroppedDown And Not cmbChapters.DroppedDown And Not cmbSubtitles.DroppedDown And Not cmbVidTrack.DroppedDown And Not frmFS.Visible Then
            SetCombos(mpPlayer.GetDVDTitles > 1, mpPlayer.GetDVDAvailableAudioStreams > 1, mpPlayer.GetDVDChapters(mpPlayer.GetDVDCurrentTitle) > 1, mpPlayer.GetDVDAvailableSubStreams > 1)
            cmbAudTrack.Tag = True
            cmbChapters.Tag = True
            cmbSubtitles.Tag = True
            cmbVidTrack.Tag = True
            If mpPlayer.GetDVDAvailableAudioStreams > 1 Then
              For I As Integer = 0 To mpPlayer.GetDVDAvailableAudioStreams - 1
                Dim audData = mpPlayer.GetDVDAudioInfo(I)
                cmbAudTrack.Items.Add(audData.bNumberOfChannels & " channel " & audData.AudioFormat.ToString & " [" & audData.Language & "]")
                mnuAudioTrack.DropDownItems.Add(audData.bNumberOfChannels & " channel " & audData.AudioFormat.ToString & " [" & audData.Language & "]")
              Next
              If mpPlayer.GetDVDCurrentAudioStream < cmbAudTrack.Items.Count Then cmbAudTrack.SelectedIndex = mpPlayer.GetDVDCurrentAudioStream
            End If

            If mpPlayer.GetDVDAvailableSubStreams > 1 Then
              cmbSubtitles.Items.Add("No subtitles")
              mnuSubtitleTrack.DropDownItems.Add("No subtitles")
              For I As Integer = 0 To mpPlayer.GetDVDAvailableSubStreams - 1
                Dim subData = mpPlayer.GetDVDSubAttributes(I)
                cmbSubtitles.Items.Add("Subtitle " & I & ": " & subData.Language)
                mnuSubtitleTrack.DropDownItems.Add("Subtitle " & I & ": " & subData.Language)
              Next
              If mpPlayer.GetDVDCurrentSubStream + 1 < cmbSubtitles.Items.Count Then cmbSubtitles.SelectedIndex = mpPlayer.GetDVDCurrentSubStream + 1
            End If

            If mpPlayer.GetDVDTitles() > 1 Then
              For I As Integer = 1 To mpPlayer.GetDVDTitles
                Dim titleData = mpPlayer.GetDVDTitleAttributes(I)
                cmbVidTrack.Items.Add("Title " & I & ": " & titleData.VideoAttributes.sourceResolutionX & "x" & titleData.VideoAttributes.sourceResolutionY & " " & titleData.AudioAttributes(0).AudioFormat.ToString & " " & titleData.AudioAttributes(0).bNumberOfChannels & "ch")
                mnuVideoTrack.DropDownItems.Add("Title " & I & ": " & titleData.VideoAttributes.sourceResolutionX & "x" & titleData.VideoAttributes.sourceResolutionY & " " & titleData.AudioAttributes(0).AudioFormat.ToString & " " & titleData.AudioAttributes(0).bNumberOfChannels & "ch")
              Next
              If mpPlayer.GetDVDCurrentTitle <= cmbVidTrack.Items.Count Then cmbVidTrack.SelectedIndex = mpPlayer.GetDVDCurrentTitle - 1
            End If

            If mpPlayer.GetDVDChapters(mpPlayer.GetDVDCurrentTitle) > 1 Then
              For I As Integer = 1 To mpPlayer.GetDVDChapters(mpPlayer.GetDVDCurrentTitle)
                cmbChapters.Items.Add("Chapter " & I)
                mnuChapterTrack.DropDownItems.Add("Chapter " & I)
              Next
              If mpPlayer.GetDVDCurrentChapter <= cmbChapters.Items.Count Then cmbChapters.SelectedIndex = mpPlayer.GetDVDCurrentChapter - 1
            End If
            cmbAudTrack.Tag = Nothing
            cmbChapters.Tag = Nothing
            cmbSubtitles.Tag = Nothing
            cmbVidTrack.Tag = Nothing
          End If
          Dim AudInfo = mpPlayer.GetDVDAudioInfo(mpPlayer.GetDVDCurrentAudioStream)
          Dim sAudFmt As String = "None"
          Select Case AudInfo.AudioFormat
            Case DirectShowLib.Dvd.DvdAudioFormat.AC3 : sAudFmt = "AC3"
            Case DirectShowLib.Dvd.DvdAudioFormat.DTS : sAudFmt = "DTS"
            Case DirectShowLib.Dvd.DvdAudioFormat.LPCM : sAudFmt = "LPCM"
            Case DirectShowLib.Dvd.DvdAudioFormat.MPEG1 : sAudFmt = "MPEG-1"
            Case DirectShowLib.Dvd.DvdAudioFormat.MPEG1_DRC : sAudFmt = "MPEG-1 DRC"
            Case DirectShowLib.Dvd.DvdAudioFormat.MPEG2 : sAudFmt = "MPEG-2"
            Case DirectShowLib.Dvd.DvdAudioFormat.MPEG2_DRC : sAudFmt = "MPEG-2 DRC"
            Case DirectShowLib.Dvd.DvdAudioFormat.SDDS : sAudFmt = "SDDS"
            Case DirectShowLib.Dvd.DvdAudioFormat.Other : sAudFmt = "Unknown"
          End Select
          sAudFmt &= " (" & KRater(AudInfo.dwFrequency, "Hz") & ")"
          Dim bChannels As Byte = AudInfo.bNumberOfChannels
          Dim sChannel As String
          Select Case bChannels
            Case 1 : sChannel = "Mono"
            Case 2 : sChannel = "Stereo"
            Case 3 : sChannel = "2.1 Stereo"
            Case 4 : sChannel = "Quadraphonic"
            Case 5 : sChannel = "Surround"
            Case 6 : sChannel = "5.1 Surround"
            Case 7 : sChannel = "6.1 Surround"
            Case 8 : sChannel = "7.1 Surround"
            Case Else : sChannel = bChannels & " channels"
          End Select
          Dim vidInfo = mpPlayer.GetDVDVideoInfo
          Dim sVidFormat As String = "None"
          Select Case vidInfo.compression
            Case DirectShowLib.Dvd.DvdVideoCompression.Mpeg1 : sVidFormat = "MPEG-1"
            Case DirectShowLib.Dvd.DvdVideoCompression.Mpeg2 : sVidFormat = "MPEG-2"
            Case DirectShowLib.Dvd.DvdVideoCompression.Other : sVidFormat = "Unknown"
          End Select
          sVidFormat &= " (" & vidInfo.frameRate & "fps)"
          If FirstInit Then SetVideoResolution(vidInfo.sourceResolutionX, vidInfo.sourceResolutionY)
          SetProps(4, bChannels, 9, vbNewLine & "Video: " & sVidFormat & vbNewLine & "Audio: " & sAudFmt, sChannel, "DVD Video")
        ElseIf String.Compare(IO.Path.GetExtension(mpPlayer.FileName), ".mkv", True) = 0 Then
          Dim mkvHeader As New Seed.clsMKVHeaders(mpPlayer.FileName)
          If Not String.IsNullOrEmpty(mkvHeader.SegmentInfo.Title) Then sTitleVal = mkvHeader.SegmentInfo.Title
          Dim mkvSize As Drawing.Size, mkvCrop As Drawing.Rectangle
          GetMKVDisplaySize(mkvHeader, mkvSize, mkvCrop)


          If mkvSize <> Drawing.Size.Empty Then
            Dim CroppedRes As Drawing.Rectangle = New Drawing.Rectangle(Drawing.Point.Empty, mkvSize)
            If mkvCrop <> Drawing.Rectangle.Empty Then
              CroppedRes = New Drawing.Rectangle(mkvCrop.Left, mkvCrop.Top, mkvSize.Width - (mkvCrop.Left + mkvCrop.Width), mkvSize.Height - (mkvCrop.Top + mkvCrop.Height))
              mpPlayer.CropVideo(CroppedRes.Left, CroppedRes.Top, CroppedRes.Width, CroppedRes.Height)
            End If

            Dim mkvPixelSize, mkvDisplaySize As Drawing.Size
            GetMKVPDSizes(mkvHeader, mkvPixelSize, mkvDisplaySize)
            VidSize = mkvPixelSize
            CorrectedSize = CroppedRes.Size
            SetVideoSize(CroppedRes.Width, CroppedRes.Height)
          Else
            SetVideoResolution(-1, -1)
            If mkvCrop <> Drawing.Rectangle.Empty Then
              Dim CroppedRes As New Drawing.Rectangle(mkvCrop.Left, mkvCrop.Top, VidSize.Width - (mkvCrop.Left + mkvCrop.Width), VidSize.Height - (mkvCrop.Top + mkvCrop.Height))
              mpPlayer.CropVideo(CroppedRes.Left, CroppedRes.Top, CroppedRes.Width, CroppedRes.Height)
            End If


          End If
          Dim chapterCollection As New Collection
          Dim RunningTime As ULong = 0
          SetCombos((From Tracks In mkvHeader.TrackEntries Where Tracks.TrackType = 1).ToArray.Length > 1, (From Tracks In mkvHeader.TrackEntries Where Tracks.TrackType = 2).ToArray.Length > 1, mkvHeader.ChapterInfo.Editions IsNot Nothing, (From Tracks In mkvHeader.TrackEntries Where Tracks.TrackType = &H11).ToArray.Length > 0)
          If mkvHeader.ChapterInfo.Editions IsNot Nothing Then
            ChapterWidth = 0
            FindAdditionalMKVChapters(mpPlayer.FileName, 0, cmbChapters, mnuChapterTrack, ChapterWidth, chapterCollection, RunningTime)
            If RunningTime > 0 Then
              mpPlayer.Position = RunningTime
            End If
            For Each Edition In mkvHeader.ChapterInfo.Editions
              If Not Edition.FlagHidden Then
                For Each Atom In Edition.Atoms
                  If Not Atom.FlagHidden Then
                    Dim sChapterText As String
                    If Atom.Display(0).Language Is Nothing Then
                      sChapterText = Atom.Display(0).Title & ": " & ConvertTimeVal(RunningTime + Atom.TimeStart / 1000000000)
                    Else
                      sChapterText = Atom.Display(0).Title & " [" & Join(Atom.Display(0).Language, ", ") & "]: " & ConvertTimeVal(RunningTime + Atom.TimeStart / 1000000000)
                    End If
                    cmbChapters.Items.Add(sChapterText)
                    mnuChapterTrack.DropDownItems.Add(sChapterText)
                    Dim g As Drawing.Graphics = cmbChapters.CreateGraphics
                    Dim lWidth As Single = g.MeasureString(sChapterText, cmbChapters.Font).Width + 10
                    If lWidth > ChapterWidth Then ChapterWidth = lWidth
                    chapterCollection.Add({CDbl(RunningTime + Atom.TimeStart / 1000000000), sChapterText})
                  End If
                Next
              End If
            Next
            If mkvHeader.SegmentInfo.Duration > 0 Then
              RunningTime += (mkvHeader.SegmentInfo.Duration / 1000)
            Else
              RunningTime += mpPlayer.Duration
            End If
            FindAdditionalMKVChapters(mpPlayer.FileName, 1, cmbChapters, mnuChapterTrack, ChapterWidth, chapterCollection, RunningTime)
            cmbChapters.SelectedIndex = 0
            cmbChapters.Tag = chapterCollection
          End If
          If mkvHeader.TrackEntries IsNot Nothing Then
            Dim sVidInfo As String = Nothing
            Dim iEncQ As Integer
            Dim sCodec As String = Nothing
            Dim iqR As Integer = -1
            Dim iChannels As Integer
            Dim sAudInfo As String = Nothing
            Dim sBitrate As String = Nothing
            Dim dDuration As Double = 0.0
            If mkvHeader.SegmentInfo.Duration > 0 Then
              dDuration = mkvHeader.SegmentInfo.Duration / 1000
            Else
              dDuration = mpPlayer.Duration
            End If
            For Each Track In mkvHeader.TrackEntries
              Dim sTrackTitle As String
              If String.IsNullOrEmpty(Track.TrackName) Then
                sTrackTitle = Track.Language
              Else
                sTrackTitle = Track.TrackName & " [" & Track.Language & "]"
              End If
              Select Case Track.TrackType
                Case &H1
                  If Not String.IsNullOrEmpty(Track.TrackName) AndAlso String.IsNullOrEmpty(sTitleVal) Then sTitleVal = Track.TrackName
                  cmbVidTrack.Items.Add(sTrackTitle)
                  mnuVideoTrack.DropDownItems.Add(sTrackTitle)
                  Dim g As Drawing.Graphics = cmbVidTrack.CreateGraphics
                  Dim lWidth As Single = g.MeasureString(sTrackTitle, cmbVidTrack.Font).Width + 10
                  If lWidth > VidWidth Then VidWidth = lWidth

                  sVidInfo = MKVVideoCodecs(Track.CodecID)
                  If Track.DefaultDuration > 0 Then sVidInfo &= " (" & Math.Round(1 / (Track.DefaultDuration / 1000000000), 3) & " fps " & IIf(Track.Video.FlagInterlaced, "interlaced", "progressive") & ")"
                Case &H2
                  cmbAudTrack.Items.Add(sTrackTitle)
                  mnuAudioTrack.DropDownItems.Add(sTrackTitle)
                  Dim g As Drawing.Graphics = cmbAudTrack.CreateGraphics
                  Dim lWidth As Single = g.MeasureString(sTrackTitle, cmbAudTrack.Font).Width + 10
                  If lWidth > AudWidth Then AudWidth = lWidth

                  Dim iQ As Long = (My.Computer.FileSystem.GetFileInfo(mpPlayer.FileName).Length * 8) / dDuration
                  Select Case iQ
                    Case Is < 1024 * 1024 : iqR = 0
                    Case Is < 1024 * 1024 * 1.5 : iqR = 1
                    Case Is < 1024 * 1024 * 2 : iqR = 2
                    Case Is < 1024 * 1024 * 2.5 : iqR = 3
                    Case Is < 1024 * 1024 * 3 : iqR = 4
                    Case Is < 1024 * 1024 * 4 : iqR = 5
                    Case Is < 1024 * 1024 * 6 : iqR = 6
                    Case Is < 1024 * 1024 * 8 : iqR = 7
                    Case Is < 1024 * 1024 * 10 : iqR = 8
                    Case Is >= 1024 * 1024 * 10 : iqR = 9
                  End Select
                  MKVAudioCodecs(Track.CodecID, iEncQ, sCodec)
                  iChannels = Track.Audio.Channels
                  sAudInfo = sCodec & " (" & KRater(Track.Audio.SamplingFrequency, "Hz") & ")"
                  sBitrate = KRater(iQ, "bps")
                Case &H11
                  cmbSubtitles.Items.Add(sTrackTitle)
                  mnuSubtitleTrack.DropDownItems.Add(sTrackTitle)
                  Dim g As Drawing.Graphics = cmbSubtitles.CreateGraphics
                  Dim lWidth As Single = g.MeasureString(sTrackTitle, cmbSubtitles.Font).Width + 10
                  If lWidth > SubWidth Then SubWidth = lWidth
              End Select
            Next
            SetProps(iEncQ, iChannels, iqR, vbNewLine & "Video: " & sVidInfo & vbNewLine & "Audio: " & sAudInfo, iChannels, sBitrate)
            If cmbSubtitles.Visible Then
              cmbSubtitles.Items.Add("No subtitles")
              mnuSubtitleTrack.DropDownItems.Add("No subtitles")
            End If
            Dim nsg As Drawing.Graphics = cmbSubtitles.CreateGraphics
            Dim lNoSubWidth As Single = nsg.MeasureString("No subtitles", cmbSubtitles.Font).Width + 10
            If lNoSubWidth > SubWidth Then SubWidth = lNoSubWidth
          End If
            If cmbVidTrack.Visible Then cmbVidTrack.SelectedIndex = 0
            HandleAudSub()
          ElseIf String.Compare(IO.Path.GetExtension(mpPlayer.FileName), ".ogm", True) = 0 Then
            Using cVorbis As New Seed.clsVorbis(mpPlayer.FileName)
              If Not String.IsNullOrEmpty(cVorbis.Title) Then sTitleVal = cVorbis.Title
              Dim iEnc As Integer, sEnc As String
              If cVorbis.File_MinQuality > 0 And cVorbis.File_MaxQuality > 0 Then
                If cVorbis.File_Quality > 0 Then
                  iEnc = 2
                  sEnc = "Constant Bitrate"
                Else
                  iEnc = 3
                  sEnc = "Limited Variable Bitrate"
                End If
              Else
                If cVorbis.File_Quality > 0 Then
                  iEnc = 1
                  sEnc = "Average/Variable Bitrate"
                Else
                  iEnc = -1
                  sEnc = Nothing
                End If
              End If
              Dim sChannel As String = cVorbis.File_Channels
              Select Case cVorbis.File_Channels
                Case 1 : sChannel = "Mono"
                Case 2 : sChannel = "Stereo"
                Case 3 : sChannel = "2.1 Stereo"
                Case 4 : sChannel = "Quadraphonic"
                Case 5 : sChannel = "Surround"
                Case 6 : sChannel = "5.1 Surround"
                Case 7 : sChannel = "6.1 Surround"
                Case 8 : sChannel = "7.1 Surround"
              End Select
              Dim iQ As Long = (My.Computer.FileSystem.GetFileInfo(mpPlayer.FileName).Length * 8) / (mpPlayer.Duration)
              Dim iqR As Integer = -1
              Select Case iQ
                Case Is < 1024 * 1024 : iqR = 0
                Case Is < 1024 * 1024 * 1.5 : iqR = 1
                Case Is < 1024 * 1024 * 2 : iqR = 2
                Case Is < 1024 * 1024 * 2.5 : iqR = 3
                Case Is < 1024 * 1024 * 3 : iqR = 4
                Case Is < 1024 * 1024 * 4 : iqR = 5
                Case Is < 1024 * 1024 * 6 : iqR = 6
                Case Is < 1024 * 1024 * 8 : iqR = 7
                Case Is < 1024 * 1024 * 10 : iqR = 8
                Case Is >= 1024 * 1024 * 10 : iqR = 9
              End Select
              SetProps(iEnc, cVorbis.File_Channels, iqR, sEnc, sChannel, KRater(iQ, "bps"))
            End Using
            Dim ffChaps = ffAPI.ChaptersList
            SetCombos(False, ffAPI.AudioStreams.Count > 1, ffChaps.Count > 0, ffAPI.SubtitleStreams.Count > 1)
            If ffChaps.Count > 0 Then
              Dim chapterCollection As New Collection
              ChapterWidth = 0
              For Each chapter In ffChaps
                Dim sChapterText As String = chapter.Value
                cmbChapters.Items.Add(sChapterText)
                mnuChapterTrack.DropDownItems.Add(sChapterText)
                Dim g As Drawing.Graphics = cmbChapters.CreateGraphics
                Dim lWidth As Single = g.MeasureString(sChapterText, cmbChapters.Font).Width + 10
                If lWidth > ChapterWidth Then ChapterWidth = lWidth
                chapterCollection.Add({chapter.Key, sChapterText})
              Next
              cmbChapters.SelectedIndex = 0
              cmbChapters.Tag = chapterCollection
            End If
            If ffAPI.AudioStreams.Count > 0 Then
              AudWidth = 0
              For Each stream In ffAPI.AudioStreams
                Dim sStreamText As String = stream.Value.name
                cmbAudTrack.Items.Add(sStreamText)
                mnuAudioTrack.DropDownItems.Add(sStreamText)
                Dim g As Drawing.Graphics = cmbAudTrack.CreateGraphics
                Dim lWidth As Single = g.MeasureString(sStreamText, cmbAudTrack.Font).Width + 10
                If lWidth > ChapterWidth Then AudWidth = lWidth
              Next
            End If
            If ffAPI.SubtitleStreams.Count > 0 Then
              SubWidth = 0
              For Each stream In ffAPI.SubtitleStreams
                Dim sStreamText As String = stream.Value.name
                cmbSubtitles.Items.Add(sStreamText)
                mnuSubtitleTrack.DropDownItems.Add(sStreamText)
                Dim g As Drawing.Graphics = cmbSubtitles.CreateGraphics
                Dim lWidth As Single = g.MeasureString(sStreamText, cmbSubtitles.Font).Width + 10
                If lWidth > ChapterWidth Then SubWidth = lWidth
              Next
              'If cmbSubtitles.Visible Then
              '  cmbSubtitles.Items.Add("No subtitles")
              '  mnuSubtitleTrack.DropDownItems.Add("No subtitles")
              'End If
              'Dim nsg As Drawing.Graphics = cmbSubtitles.CreateGraphics
              'Dim lNoSubWidth As Single = nsg.MeasureString("No subtitles", cmbSubtitles.Font).Width + 10
              'If lNoSubWidth > SubWidth Then SubWidth = lNoSubWidth
            End If
            cmbVidTrack.Items.Add("Default Video Track. Unable to load at this time.")
            mnuVideoTrack.DropDownItems.Add("Default")
            If cmbVidTrack.Visible Then cmbVidTrack.SelectedIndex = 0
            HandleAudSub()
            SetVideoResolution(-1, -1)
          ElseIf String.Compare(IO.Path.GetExtension(mpPlayer.FileName), ".avi", True) = 0 Then
            Using cRiff As New Seed.clsRIFF(mpPlayer.FileName)
              Dim iQ As Long = (My.Computer.FileSystem.GetFileInfo(mpPlayer.FileName).Length * 8) / (mpPlayer.Duration)
              Dim enc, chan, rate As Integer
              Dim sEnc, sChan, sRate As String
              enc = 4
              chan = -1
              rate = 0
              sEnc = "AVI"
              sChan = Nothing
              sRate = KRater(iQ, "bps")
              If cRiff.IsValid Then
                chan = cRiff.AVIAudioData(0).Format.nChannels
                Select Case chan
                  Case 1 : sChan = "Mono"
                  Case 2 : sChan = "Stereo"
                  Case 3 : sChan = "2.1 Stereo"
                  Case 4 : sChan = "Quadraphonic"
                  Case 5 : sChan = "Surround"
                  Case 6 : sChan = "5.1 Surround"
                  Case 7 : sChan = "6.1 Surround"
                  Case 8 : sChan = "7.1 Surround"
                  Case Else : sChan = chan & " Channels"
                End Select
                Dim ve As String = "Unknown Encoder"
                Dim ae As String = "Unknown Encoder"
                Dim vr As String = "Unknown FPS"
                Dim ar As String = "Unknown Hz"
                ve = AVIVideoCodecs(cRiff.AVIVideoData(0).bmiHeader.biCompression)
                ae = WAVAudioCodecs(cRiff.AVIAudioData(0).Format.wFormatTag)
                ar = KRater(cRiff.AVIAudioData(0).Format.nSamplesPerSec, "Hz")
                For I As Integer = 0 To cRiff.AVIStreamCount - 1
                  If cRiff.AVIStreamData(I).fccType = "vids" Then
                    If String.IsNullOrEmpty(ve) AndAlso Not String.IsNullOrEmpty(cRiff.AVIStreamData(I).fccHandler) Then ve = cRiff.AVIStreamData(I).fccHandler
                    If cRiff.AVIStreamData(I).dwScale > 0 Then vr = Math.Round(cRiff.AVIStreamData(I).dwRate / cRiff.AVIStreamData(I).dwScale, 3) & " fps " & IIf(cRiff.AVIMainData.dwFlags And Seed.clsRIFF.AVIMAINHEADER_FLAGS.AVIF_ISINTERLEAVED, "Interleaved", "Progressive")
                  ElseIf cRiff.AVIStreamData(I).fccType = "auds" Then
                    If String.IsNullOrEmpty(ae) AndAlso Not String.IsNullOrEmpty(cRiff.AVIStreamData(I).fccHandler) Then ae = cRiff.AVIStreamData(I).fccHandler
                    If ae = "0 Hz" AndAlso cRiff.AVIStreamData(I).dwScale > 0 Then ar = KRater(cRiff.AVIStreamData(I).dwRate / cRiff.AVIStreamData(I).dwScale, "Hz")
                  End If
                Next
                sEnc = vbNewLine & " Video (" & ve & "): " & vr & vbNewLine & " Audio (" & ae & "): " & ar
                SetVideoResolution(cRiff.AVIMainData.dwWidth, cRiff.AVIMainData.dwHeight)
                Select Case cRiff.AVIVideoData(0).bmiHeader.biBitCount
                  Case 0 'specified or implied
                    rate = 0
                  Case 1 'Monochrome
                    rate = 1
                  Case 4 '16 Color
                    rate = 3
                  Case 8 '256 Color
                    rate = 5
                  Case 16 '16-bit color
                    rate = 7
                  Case 24 '24-bit color
                    rate = 8
                  Case 32 '32-bit color
                    rate = 9
                  Case Else
                    rate = 4
                End Select
              End If
              SetProps(enc, chan, rate, sEnc, sChan, sRate)
            End Using
          ElseIf LoadFFDShow() Then
            Dim ffChaps = ffAPI.ChaptersList
            SetCombos(False, ffAPI.AudioStreams.Count > 1, ffChaps.Count > 0, ffAPI.SubtitleStreams.Count > 1)
            If ffChaps.Count > 0 Then
              Dim chapterCollection As New Collection
              ChapterWidth = 0
              For Each chapter In ffChaps
                Dim sChapterText As String = chapter.Value
                cmbChapters.Items.Add(sChapterText)
                mnuChapterTrack.DropDownItems.Add(sChapterText)
                Dim g As Drawing.Graphics = cmbChapters.CreateGraphics
                Dim lWidth As Single = g.MeasureString(sChapterText, cmbChapters.Font).Width + 10
                If lWidth > ChapterWidth Then ChapterWidth = lWidth
                chapterCollection.Add({chapter.Key, sChapterText})
              Next
              cmbChapters.SelectedIndex = 0
              cmbChapters.Tag = chapterCollection
            End If
            If ffAPI.AudioStreams.Count > 0 Then
              AudWidth = 0
              For Each stream In ffAPI.AudioStreams
                Dim sStreamText As String = stream.Value.name
                cmbAudTrack.Items.Add(sStreamText)
                mnuAudioTrack.DropDownItems.Add(sStreamText)
                Dim g As Drawing.Graphics = cmbAudTrack.CreateGraphics
                Dim lWidth As Single = g.MeasureString(sStreamText, cmbAudTrack.Font).Width + 10
                If lWidth > ChapterWidth Then AudWidth = lWidth
              Next
            End If
            If ffAPI.SubtitleStreams.Count > 0 Then
              SubWidth = 0
              For Each stream In ffAPI.SubtitleStreams
                Dim sStreamText As String = stream.Value.name
                cmbSubtitles.Items.Add(sStreamText)
                mnuSubtitleTrack.DropDownItems.Add(sStreamText)
                Dim g As Drawing.Graphics = cmbSubtitles.CreateGraphics
                Dim lWidth As Single = g.MeasureString(sStreamText, cmbSubtitles.Font).Width + 10
                If lWidth > ChapterWidth Then SubWidth = lWidth
              Next
            End If
            cmbVidTrack.Items.Add("Default Video Track. Unable to load at this time.")
            mnuVideoTrack.DropDownItems.Add("Default")
            If cmbVidTrack.Visible Then cmbVidTrack.SelectedIndex = 0
            HandleAudSub()
            SetVideoResolution(-1, -1)
            Dim iQ As Long = (My.Computer.FileSystem.GetFileInfo(mpPlayer.FileName).Length * 8) / (mpPlayer.Duration)
            Dim Channels As Integer = ffAPI.getIntParam(FFDShowAPI.FFDShowConstants.FFDShowDataId.IDFF_OSDtype_nchannels)
            Dim sChannel As String = Channels & " channels"
            Select Case Channels
              Case -1 : sChannel = String.Empty
              Case 0 : sChannel = "Unknown Speaker Setup" : Channels = 2
              Case 1 : sChannel = "Mono"
              Case 2 : sChannel = "Stereo"
              Case 3 : sChannel = "2.1 Stereo"
              Case 4 : sChannel = "Quadraphonic"
              Case 5 : sChannel = "Surround"
              Case 6 : sChannel = "5.1 Surround"
              Case 7 : sChannel = "6.1 Surround"
              Case 8 : sChannel = "7.1 Surround"
              Case Else : sChannel = Channels & " channels"
            End Select
            SetProps(4, Channels, 0, ffAPI.getFrameRate & " fps " & ffAPI.getIntParam(FFDShowAPI.FFDShowConstants.FFDShowDataId.IDFF_enc_interlacing), sChannel, KRater(iQ, "bps"))

          Else
            SetCombos(False, False, False, False)
            SetVideoResolution(-1, -1)
            Dim iQ As Long = (My.Computer.FileSystem.GetFileInfo(mpPlayer.FileName).Length * 8) / (mpPlayer.Duration)
            SetProps(-1, 2, 0, , "Unknown Speaker Setup", KRater(iQ, "bps"))
          End If
          If FirstInit Then
            If frmFS.Visible Then
              mpPlayer.FullScreenObj = frmFS.pctVideo
              mpPlayer.FullScreen = True
              'If cTask IsNot Nothing Then cTask.CreatePreview(frmFS.pctVideo)
              VidThumb = False
            Else
              If cTask IsNot Nothing Then
                'tmrUpdate_Tick(New Object, New EventArgs)
                'cTask.CreatePreview(mpPlayer)
                VidThumb = True
              Else
                VidThumb = False
              End If
            End If
            'cTask.Title = FileTitle
            'cTask.Icon = Me.Icon
            If mnuRatioAutomatic.Checked Then mnuRatioAutomatic_Click(mnuRatioAutomatic, New EventArgs)
            If mnuRatioStandard.Checked Then mnuRatioStandard_Click(mnuRatioStandard, New EventArgs)
            If mnuRatioWide.Checked Then mnuRatioWide_Click(mnuRatioWide, New EventArgs)
            If mnuScaleHalf.Checked Then mnuScaleHalf_Click(mnuScaleHalf, New EventArgs)
            If mnuScaleNorm.Checked Then mnuScaleNorm_Click(mnuScaleNorm, New EventArgs)
            If mnuScaleTwice.Checked Then mnuScaleTwice_Click(mnuScaleTwice, New EventArgs)
            If Not bDVD Then
              If mpPlayer.Repeat Then
                cmdLoop.Image = My.Resources.button_loop_on
              Else
                cmdLoop.Image = My.Resources.button_loop_off
              End If
              cmdLoop.Tag = Nothing
              LabelShortcuts()
            End If
          End If
          pctAlbumArt.Image = Nothing
          If frmFS IsNot Nothing Then frmFS.pctVideo.Image = Nothing
      Else
        'AUDIO
        EnableScreenSaver(True)
        SetTabs(True, False)
        'FileTitle = GetTitle(mpPlayer.FileName)
        sTitleVal = GetTitle(mpPlayer.FileName, True, False)
        sArtistVal = GetTitle(mpPlayer.FileName, True, True)
        FileArt = GetArt(mpPlayer.FileName, False, False)
        'If FileArt IsNot Nothing And Me.WindowState = FormWindowState.Normal And tbsView.SelectedTab Is tabArt Then tbsView_SelectedIndexChanged(New Object, New EventArgs)
        Select Case IO.Path.GetExtension(mpPlayer.FileName).Substring(1).ToLower
          Case "mp3" ', "mpe", "mpg", "mpeg", "m1v", "mp2", "m2v", "mp2v", "mpv2", "mpa", "aac", "m2ts" , "m4a", "m4p", "m4v", "mp4"
            Using cHeader As New Seed.clsHeaderLoader(mpPlayer.FileName)
              If cHeader.cMPEG IsNot Nothing Then
                Dim iQual, iChannels As Integer
                Select Case cHeader.RateFormat
                  Case "ABR (XING)", "2-Pass ABR (XING)" : iQual = 1
                  Case "CBR", "CBR (XING)", "2-Pass CBR (XING)" : iQual = 2
                  Case "VBR (Fraunhofer)", "VBR rh (XING)", "VBR mt/rh (XING)", "VBR mt (XING)" : iQual = 3
                  Case Else
                    If cHeader.RateFormat.StartsWith("Full VBR Method") Then
                      iQual = 3
                    Else
                      Debug.Print("Unknown RateFormat: " & cHeader.RateFormat)
                      iQual = -1
                    End If
                End Select
                Dim sChannels As String = cHeader.cMPEG.GetChannels
                Select Case sChannels
                  Case "Single Channel" : iChannels = 1
                  Case "Stereo" : iChannels = 2
                  Case "Joint Stereo"
                    iChannels = -2
                    sChannels &= " (" & cHeader.cMPEG.GetModeExtension & ")"
                  Case "Dual Channel" : iChannels = 4
                  Case Else
                    Debug.Print("Unknown Channels: " & sChannels)
                    iChannels = -1
                End Select
                SetProps(iQual, iChannels, cHeader.cMPEG.GetBitQual, cHeader.RateFormat, sChannels, KRater(cHeader.cMPEG.GetBitrate, "bps"))
              End If
            End Using
          Case "ogg", "flac"
            Using cVorbis As New Seed.clsVorbis(mpPlayer.FileName)
              Dim iQual As Integer, sQual As String
              If cVorbis.File_MinQuality > 0 And cVorbis.File_MaxQuality > 0 Then
                If cVorbis.File_Quality > 0 Then
                  iQual = 2
                  sQual = "Constant Bitrate"
                Else
                  iQual = 3
                  sQual = "Limited Variable Bitrate"
                End If
              Else
                If cVorbis.File_Quality > 0 Then
                  iQual = 1
                  sQual = "Average/Variable Bitrate"
                Else
                  iQual = -1
                  sQual = Nothing
                End If
              End If
              Dim sChannel As String = cVorbis.File_Channels
              Select Case cVorbis.File_Channels
                Case 1 : sChannel = "Mono"
                Case 2 : sChannel = "Stereo"
                Case 3 : sChannel = "2.1 Stereo"
                Case 4 : sChannel = "Quadraphonic"
                Case 5 : sChannel = "Surround"
                Case 6 : sChannel = "5.1 Surround"
                Case 7 : sChannel = "6.1 Surround"
                Case 8 : sChannel = "7.1 Surround"
              End Select
              Dim iQ As Long = (My.Computer.FileSystem.GetFileInfo(mpPlayer.FileName).Length * 8) / (mpPlayer.Duration)
              Dim iQR As Integer = -1
              Select Case iQ / (cVorbis.File_Rate * cVorbis.File_Channels)
                Case Is < 0.8 : iQR = 0
                Case Is < 0.9 : iQR = 1
                Case Is < 1.1 : iQR = 2
                Case Is < 1.3 : iQR = 3
                Case Is < 1.5 : iQR = 4
                Case Is < 1.7 : iQR = 5
                Case Is < 1.9 : iQR = 5
                Case Is < 2.5 : iQR = 6
                Case Is < 3 : iQR = 7
                Case Is < 4 : iQR = 8
                Case Is >= 4 : iQR = 9
              End Select
              SetProps(iQual, cVorbis.File_Channels, iQR, sQual, sChannel, KRater(iQ, "bps"))
            End Using
          Case "wav"
            Dim iQ As Long = (My.Computer.FileSystem.GetFileInfo(mpPlayer.FileName).Length * 8) / (mpPlayer.Duration)
            Using cRiff As New Seed.clsRIFF(mpPlayer.FileName)
              Dim enc, chan, rate As Integer
              Dim sEnc, sChan, sRate As String
              enc = 4
              chan = -1
              rate = 0
              sEnc = "WAVE"
              sChan = Nothing
              sRate = KRater(iQ, "bps")
              If cRiff.IsValid Then
                If cRiff.IsDTS Then
                  enc = 1
                  sEnc = "DTS"
                  Select Case cRiff.DTSData.iAMODE
                    Case 0
                      chan = 1
                      sChan = "Mono"
                    Case 1
                      chan = 1
                      sChan = "Dual Mono (A + B)"
                    Case 2
                      chan = 2
                      sChan = "Left and Right Stereo"
                    Case 3
                      chan = -2
                      sChan = "(Left + Right) - (Left - Right) Stereo"
                    Case 4
                      chan = 2
                      sChan = "Totals Stereo"
                    Case 5
                      chan = -2
                      sChan = "Center, Left, Right"
                    Case 6
                      chan = 3
                      sChan = "Left, Right, Surround"
                    Case 7
                      chan = 4
                      sChan = "Center, Left, Right, Surround"
                    Case 8
                      chan = 4
                      sChan = "Left, Right, Surround Left, Surround Right"
                    Case 9
                      chan = 5
                      sChan = "Center, Left, Right, Surround Left, Surround Right"
                    Case 10
                      chan = 7
                      sChan = "Center Left, Center Right, Left, Right, Surround Left, Surround Right"
                    Case 11
                      chan = 7
                      sChan = "Center, Left, Right, Left Rear, Right Rear, Overhead"
                    Case 12
                      chan = 7
                      sChan = "Center Front, Center Rear, Left Front, Right Front, Left Rear, Right Rear"
                    Case 13
                      chan = 7
                      sChan = "Center Left, Center, Center Right, Left, Right, Surround Left, Surround Right"
                    Case 14
                      chan = 8
                      sChan = "Center Left, Center Right, Left, Right, Surround Left 1, Surround Left 2, Surround Right 1, Surround Right 2"
                    Case 15
                      chan = 8
                      sChan = "Center Left, Center, Center Right, Left, Right, Surround Left, Surround, Surround Right"
                    Case Else
                      chan = cRiff.DTSData.iAMODE
                      sChan = cRiff.DTSData.iAMODE
                  End Select
                  If cRiff.DTSData.iLFF > 0 Then sChan &= " & Low Frequency Emitter (" & IIf(cRiff.DTSData.iLFF = 2, "64", "128") & " Interpolation Factor)"
                  Select Case cRiff.DTSData.iRATE
                    Case 0
                      rate = 0
                      sRate = "32 kbps"
                    Case 1
                      rate = 0
                      sRate = "56 kbps"
                    Case 2
                      rate = 0
                      sRate = "64 kbps"
                    Case 3
                      rate = 1
                      sRate = "96 kbps"
                    Case 4
                      rate = 1
                      sRate = "112 kbps"
                    Case 5
                      rate = 2
                      sRate = "128 kbps"
                    Case 6
                      rate = 2
                      sRate = "192 kbps"
                    Case 7
                      rate = 2
                      sRate = "224 kbps"
                    Case 8
                      rate = 3
                      sRate = "256 kbps"
                    Case 9
                      rate = 3
                      sRate = "320 kbps"
                    Case 10
                      rate = 4
                      sRate = "384 kbps"
                    Case 11
                      rate = 4
                      sRate = "448 kbps"
                    Case 12
                      rate = 4
                      sRate = "512 kbps"
                    Case 13
                      rate = 5
                      sRate = "576 kbps"
                    Case 14
                      rate = 5
                      sRate = "640 kbps"
                    Case 15
                      rate = 6
                      sRate = "768 kbps"
                    Case 16
                      rate = 6
                      sRate = "960 kbps"
                    Case 17
                      rate = 7
                      sRate = "1024 kbps"
                    Case 18
                      rate = 7
                      sRate = "1152 kbps"
                    Case 19
                      rate = 7
                      sRate = "1280 kbps"
                    Case 20
                      rate = 8
                      sRate = "1344 kbps"
                    Case 21
                      rate = 8
                      sRate = "1408 kbps"
                    Case 22
                      rate = 9
                      sRate = "1411.2 kbps"
                    Case 23
                      rate = 9
                      sRate = "1472 kbps"
                    Case 24
                      rate = 9
                      sRate = "1536 kbps"
                    Case 25
                      rate = 0
                      sRate = "Open"
                  End Select
                Else
                  sEnc = WAVAudioCodecs(cRiff.WAVData.Format.Format.wFormatTag)
                  Select Case cRiff.WAVData.Format.Format.nChannels
                    Case 1
                      chan = 1
                      sChan = "Mono"
                    Case 2
                      chan = 2
                      sChan = "Stereo"
                    Case 3
                      chan = 3
                      sChan = "2.1 Stereo"
                    Case 4
                      chan = 4
                      sChan = "Quadraphonic"
                    Case 5
                      chan = 5
                      sChan = "Surround"
                    Case 6
                      chan = 6
                      sChan = "5.1 Surround"
                    Case 7
                      chan = 7
                      sChan = "6.1 Surround"
                    Case 8
                      chan = 8
                      sChan = "7.1 Surround"
                    Case Else
                      chan = cRiff.WAVData.Format.Format.nChannels
                      sChan = chan
                  End Select
                  rate = 9
                  sRate = KRater(cRiff.WAVData.Format.Format.nAvgBytesPerSec * 8, "bps")
                  If cRiff.WAVData.Format.cbSize > 0 Or cRiff.WAVData.Format.wBitsPerSample > 0 Then
                    If cRiff.WAVData.Format.cbSize >= 22 Then
                      If cRiff.WAVData.dwChannelMask > 0 Then
                        sChan = String.Empty

                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontLeft) = Seed.clsRIFF.ChannelStruct.FrontLeft Then sChan &= "Front Left, "
                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontCenterLeft) = Seed.clsRIFF.ChannelStruct.FrontCenterLeft Then sChan &= "Front Center Left, "
                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontCenter) = Seed.clsRIFF.ChannelStruct.FrontCenter Then sChan &= "Front Center, "
                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontCenterRight) = Seed.clsRIFF.ChannelStruct.FrontCenterRight Then sChan &= "Front Center Right, "
                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.FrontRight) = Seed.clsRIFF.ChannelStruct.FrontRight Then sChan &= "Front Right, "

                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.SideLeft) = Seed.clsRIFF.ChannelStruct.SideLeft Then sChan &= "Side Left, "
                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.SideRight) = Seed.clsRIFF.ChannelStruct.SideRight Then sChan &= "Side Right, "

                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.RearLeft) = Seed.clsRIFF.ChannelStruct.RearLeft Then sChan &= "Rear Left, "
                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.RearCenter) = Seed.clsRIFF.ChannelStruct.RearCenter Then sChan &= "Rear Center, "
                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.RearRight) = Seed.clsRIFF.ChannelStruct.RearRight Then sChan &= "Rear Right, "

                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopCenter) = Seed.clsRIFF.ChannelStruct.TopCenter Then sChan &= "Top Center, "

                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopFrontLeft) = Seed.clsRIFF.ChannelStruct.TopFrontLeft Then sChan &= "Top Front Left, "
                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopFrontCenter) = Seed.clsRIFF.ChannelStruct.TopFrontCenter Then sChan &= "Top Front Center, "
                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopFrontRight) = Seed.clsRIFF.ChannelStruct.TopFrontRight Then sChan &= "Top Front Right, "

                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopRearLeft) = Seed.clsRIFF.ChannelStruct.TopRearLeft Then sChan &= "Top Rear Left, "
                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopRearCenter) = Seed.clsRIFF.ChannelStruct.TopRearCenter Then sChan &= "Top Rear Center, "
                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.TopRearRight) = Seed.clsRIFF.ChannelStruct.TopRearRight Then sChan &= "Top Rear Right, "

                        If (cRiff.WAVData.dwChannelMask And Seed.clsRIFF.ChannelStruct.LFE) = Seed.clsRIFF.ChannelStruct.LFE Then sChan &= "Low Frequency Emitter, "

                        If sChan.Length > 2 Then sChan = sChan.Substring(0, sChan.Length - 2)
                      End If
                      Select Case cRiff.WAVData.SubFormat.ToString.ToLower
                        Case "6dba3190-67bd-11cf-a0f7-0020afd156e4" : sEnc = "Analog"
                        Case "00000001-0000-0010-8000-00aa00389b71" : sEnc = "PCM"
                        Case "00000003-0000-0010-8000-00aa00389b71" : sEnc = "Float (IEEE)"
                        Case "00000009-0000-0010-8000-00aa00389b71" : sEnc = "DRM"
                        Case "00000006-0000-0010-8000-00aa00389b71" : sEnc = "A-Law"
                        Case "00000007-0000-0010-8000-00aa00389b71" : sEnc = "µ-Law"
                        Case "00000002-0000-0010-8000-00aa00389b71" : sEnc = "ADPCM"
                        Case "00000050-0000-0010-8000-00aa00389b71" : sEnc = "MPEG"
                        Case "4995daee-9ee6-11d0-a40e-00a0c9223196" : sEnc = "RIFF"
                        Case "e436eb8b-524f-11ce-9f53-0020af0ba770" : sEnc = "RIFF WAVE"
                        Case "1d262760-e957-11cf-a5d6-28db04c10000" : sEnc = "MIDI"
                        Case "2ca15fa0-6cfe-11cf-a5d6-28dB04c10000" : sEnc = "MIDI Bus"
                        Case "4995daf0-9ee6-11d0-a40e-00a0c9223196" : sEnc = "RIFF MIDI"
                        Case Else : sEnc = "Unknown {" & cRiff.WAVData.SubFormat.ToString & "}"
                      End Select
                    End If
                  End If
                End If
              End If
              SetProps(enc, chan, rate, sEnc, sChan, sRate)
            End Using
          Case Else
            Dim iQ As Long = (My.Computer.FileSystem.GetFileInfo(mpPlayer.FileName).Length * 8) / (mpPlayer.Duration)
            SetProps(-1, -1, 0, , , KRater(iQ, "bps"))
        End Select
        SetCombos(False, False, False, False)
        If mpPlayer.Repeat Then
          cmdLoop.Image = My.Resources.button_loop_on
        Else
          cmdLoop.Image = My.Resources.button_loop_off
        End If
        cmdLoop.Tag = Nothing
        LabelShortcuts()
        VidThumb = False
      End If
      SetTitleArtist(sTitleVal, sArtistVal)
      FirstInit = False
    End If
  End Sub

  Private Sub HandleAudSub()
    Dim MyLocale As Globalization.CultureInfo = Nothing
    Dim sLoc As String = My.Settings.DefaultLocale
    If sLoc.Contains(" [") Then sLoc = sLoc.Substring(0, sLoc.IndexOf(" ["))
    For Each culture In Globalization.CultureInfo.GetCultures(Globalization.CultureTypes.NeutralCultures)
      If culture.NativeName = sLoc Then
        MyLocale = culture
        Exit For
      End If
    Next
    If MyLocale Is Nothing Then MyLocale = Globalization.CultureInfo.CurrentCulture

    If cmbAudTrack.Visible Then
      Dim bAuded As Boolean = False
      For Each row In cmbAudTrack.Items
        Dim sRow As String = row
        If sRow.ToLower.Contains("commentary") Then sRow = Replace(sRow, "commentary", "", , , CompareMethod.Text)
        If sRow.ToLower.Contains(MyLocale.NativeName) Or
          sRow.ToLower.Contains(MyLocale.EnglishName) Or
          sRow.ToLower.Contains(MyLocale.ThreeLetterISOLanguageName) Or
          sRow.ToLower.Contains(MyLocale.ThreeLetterWindowsLanguageName) Or
          sRow.ToLower.Contains(MyLocale.DisplayName) Or
          sRow.ToLower.Contains(MyLocale.Name) Or
          sRow.ToLower.Contains(MyLocale.IetfLanguageTag) Or
          sRow.ToLower.Contains(MyLocale.TwoLetterISOLanguageName) Then
          cmbAudTrack.Text = row
          bAuded = True
          Exit For
        End If
      Next
      If Not bAuded Then cmbAudTrack.SelectedIndex = 0

      If cmbSubtitles.Visible Then
        If bAuded Or Not My.Settings.Subtitles Then
          If cmbSubtitles.Items.Contains("No subtitles") Then
            cmbSubtitles.Text = "No subtitles"
          Else
            cmbSubtitles.SelectedIndex = 0
          End If
        Else
          Dim bFound As Boolean = False
          For Each row As String In cmbSubtitles.Items
            If row.ToLower.Contains(MyLocale.NativeName) Or
              row.ToLower.Contains(MyLocale.EnglishName) Or
              row.ToLower.Contains(MyLocale.ThreeLetterISOLanguageName) Or
              row.ToLower.Contains(MyLocale.ThreeLetterWindowsLanguageName) Or
              row.ToLower.Contains(MyLocale.DisplayName) Or
              row.ToLower.Contains(MyLocale.Name) Or
              row.ToLower.Contains(MyLocale.IetfLanguageTag) Or
              row.ToLower.Contains(MyLocale.TwoLetterISOLanguageName) Then
              cmbSubtitles.Text = row
              bFound = True
              Exit For
            End If
          Next
          If Not bFound Then
            If cmbSubtitles.Items.Contains("No subtitles") Then
              cmbSubtitles.Text = "No subtitles"
            Else
              cmbSubtitles.SelectedIndex = 0
            End If
          End If
        End If
      End If
    Else
      If cmbSubtitles.Visible Then
        Dim bFound As Boolean = False
        If My.Settings.Subtitles Then
          For Each row As String In cmbSubtitles.Items
            If row.ToLower.Contains(MyLocale.NativeName) Or
              row.ToLower.Contains(MyLocale.EnglishName) Or
              row.ToLower.Contains(MyLocale.ThreeLetterISOLanguageName) Or
              row.ToLower.Contains(MyLocale.ThreeLetterWindowsLanguageName) Or
              row.ToLower.Contains(MyLocale.DisplayName) Or
              row.ToLower.Contains(MyLocale.Name) Or
              row.ToLower.Contains(MyLocale.IetfLanguageTag) Or
              row.ToLower.Contains(MyLocale.TwoLetterISOLanguageName) Then
              cmbSubtitles.Text = row
              bFound = True
              Exit For
            End If
          Next
        End If
        If Not bFound Then
          If cmbSubtitles.Items.Contains("No subtitles") Then
            cmbSubtitles.Text = "No subtitles"
          Else
            cmbSubtitles.SelectedIndex = 0
          End If
        End If
      End If
    End If
  End Sub

  Private Sub SetTitleArtist(sFileTitle As String, sFileArtist As String)
    If String.IsNullOrEmpty(sFileTitle) And String.IsNullOrEmpty(sFileArtist) Then
      FileTitle = String.Empty
    Else
      If clsGlass.IsCompositionEnabled Then
        If sFileArtist = UNKNOWN_ARTIST Then
          FileMainTitle = sFileTitle
          FileTitle = sFileTitle
          FileSubTitle = Nothing
        Else
          FileMainTitle = sFileArtist
          FileSubTitle = sFileTitle
          FileTitle = sFileArtist & " - " & sFileTitle
        End If
      Else
        FileTitle = sFileArtist & " - " & sFileTitle
        FileMainTitle = FileTitle
        FileSubTitle = Nothing
      End If
    End If
    If cTask IsNot Nothing Then
      'tmrUpdate_Tick(New Object, New EventArgs)
      If frmFS IsNot Nothing AndAlso frmFS.Visible Then
        cTask.CreatePreview(frmFS.pctVideo)
      ElseIf VidThumb Then
        cTask.CreatePreview(mpPlayer)
      ElseIf FileArt IsNot Nothing Then
        cTask.CreatePreview(FileArt.Art)
      Else
        cTask.CreatePreview(pnlMain)
      End If
      cTask.Title = FileTitle
      cTask.Tooltip = FileTitle
      cTask.Icon = Me.Icon
    End If
  End Sub

  Private Sub DrawTitleArtist()
    Dim sTimeString As String
    If bCD Then
      sTimeString = " (" & ConvertTimeVal(cCD.TrackPositionSeconds) & "/" & ConvertTimeVal(cCD.TrackDurationSeconds) & ")"
    ElseIf mpPlayer.IsFlash Then
      sTimeString = " (" & mpPlayer.Position & "/" & mpPlayer.Duration & " frames)"
    Else
      sTimeString = " (" & ConvertTimeVal(mpPlayer.Position) & "/" & ConvertTimeVal(mpPlayer.Duration) & ")"
    End If
    If clsGlass.IsCompositionEnabled And (bCD Or (Not String.IsNullOrEmpty(FileSubTitle) And Not String.IsNullOrEmpty(mpPlayer.FileName))) Then
      If String.IsNullOrEmpty(FileSubTitle) Then
        If String.IsNullOrEmpty(FileMainTitle) Then
          If Not Me.Text = "Lime Seed Media Player" & sTimeString Then
            Me.Text = "Lime Seed Media Player" & sTimeString
            NoGlassText = True
            DrawNormal()
          End If
        Else
          If Not Me.Text = FileMainTitle & sTimeString Then
            Me.Text = FileMainTitle & sTimeString
            NoGlassText = True
            DrawNormal()
          End If
        End If
      Else
        If String.IsNullOrEmpty(FileMainTitle) Then
          If Not Me.Text = FileSubTitle & sTimeString Then
            Me.Text = FileSubTitle & sTimeString
            NoGlassText = True
            DrawNormal()
          End If
        Else
          If Not Me.Text = FileMainTitle Then Me.Text = FileMainTitle
          Static lastcdSubTitle As String
          Dim sSubTitle As String = FileSubTitle & sTimeString
          NoGlassText = False
          If Not sSubTitle = lastcdSubTitle Then
            lastcdSubTitle = sSubTitle
            DrawGlassText(True)
          End If
          Dim TextFont As New Drawing.Font(Drawing.SystemFonts.CaptionFont.Name, 9)
          Dim sFileTime As String = "@" & sTimeString & "@"
          Dim iTitleWidth As Integer = CreateGraphics.MeasureString(FileSubTitle, TextFont).Width
          Dim iTimeWidth As Integer = CreateGraphics.MeasureString(sFileTime, TextFont).Width + 12 * 2
          If iTitleWidth + iTimeWidth > Me.DisplayRectangle.Width Then DrawGlassText(False)
        End If
      End If
    Else
      Dim sTitle As String = FileTitle & sTimeString
      If Not Me.Text = sTitle Then Me.Text = sTitle
      NoGlassText = True
      DrawNormal()
    End If
  End Sub

  Private Sub DrawGlassText(autoRedraw As Boolean)
    If Me.WindowState = FormWindowState.Minimized Then Exit Sub
    Const BarHeight As Integer = 32
    Const EdgeMargin As Integer = 8
    Dim TextFont As New Drawing.Font(Drawing.SystemFonts.CaptionFont.Name, 9)
    Const GlowSize As Integer = 12
    If Not NoGlassText And (clsGlass.IsCompositionEnabled And (bCD Or (Not String.IsNullOrEmpty(FileSubTitle) And Not String.IsNullOrEmpty(mpPlayer.FileName)))) Then
      If pnlMain.Dock = DockStyle.Fill Then
        pnlMain.Dock = DockStyle.None
        pnlMain.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
        pnlMain.Location = New Drawing.Point(0, BarHeight + 1)
        pnlMain.Size = New Drawing.Size(Me.DisplayRectangle.Width, Me.DisplayRectangle.Height - BarHeight)
        clsGlass.SetGlass(Me, 0, pnlMain.Top, 0, 0)
        Me.MinimumSize = New Drawing.Size(MAIN_WIDTH, MAIN_HEIGHT + BarHeight)
      End If
      Dim sFileTime As String = "( 0:00 / 0:00 )"
      If bCD Then
        sFileTime = "( " & ConvertTimeVal(cCD.TrackPositionSeconds) & " / " & ConvertTimeVal(cCD.TrackDurationSeconds) & " )"
      ElseIf mpPlayer.IsFlash Then
        sFileTime = "( " & mpPlayer.Position & " / " & mpPlayer.Duration & " frames )"
      Else
        sFileTime = "( " & ConvertTimeVal(mpPlayer.Position) & " / " & ConvertTimeVal(mpPlayer.Duration) & " )"
      End If

      Dim iTitleWidth As Integer = CreateGraphics.MeasureString(FileSubTitle, TextFont).Width + EdgeMargin
      Dim iTimeWidth As Integer = CreateGraphics.MeasureString("_" & sFileTime, TextFont).Width + EdgeMargin

      If iTitleWidth + iTimeWidth > Me.DisplayRectangle.Width Then
        Static lastX As Integer, lastLeft As Boolean
        If Not autoRedraw Then
          If lastX >= 0 And Not lastLeft Then
            lastLeft = True
            lastX = 0
          ElseIf lastX <= Me.DisplayRectangle.Width - iTimeWidth - iTitleWidth And lastLeft Then
            lastLeft = False
            lastX = Me.DisplayRectangle.Width - iTimeWidth - iTitleWidth
          Else
            Dim iExtra As Integer = (Me.DisplayRectangle.Width - iTimeWidth) - iTitleWidth
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
            If lastLeft Then
              lastX -= iStep
            Else
              lastX += iStep
            End If
          End If
          If lastX > 0 Then lastX = 0
          If lastX < Me.DisplayRectangle.Width - iTimeWidth - iTitleWidth Then lastX = Me.DisplayRectangle.Width - iTimeWidth - iTitleWidth
        End If

        Dim TitleRect As New Drawing.Rectangle(lastX, 0, Me.DisplayRectangle.Width - iTimeWidth - lastX, BarHeight)
        Dim TimeRect As New Drawing.Rectangle(Me.DisplayRectangle.Width - iTimeWidth, 0, iTimeWidth, BarHeight)

        clsGlass.DrawTextOnGlass(Me.Handle, FileSubTitle, TextFont, TitleRect, EdgeMargin, GlowSize)
        clsGlass.DrawTextOnGlass(Me.Handle, "  " & sFileTime, TextFont, TimeRect, EdgeMargin, GlowSize)
      Else
        Dim TitleRect As New Drawing.Rectangle(0, 0, Me.DisplayRectangle.Width - iTimeWidth, BarHeight)
        Dim TimeRect As New Drawing.Rectangle(Me.DisplayRectangle.Width - iTimeWidth, 0, iTimeWidth, BarHeight)

        clsGlass.DrawTextOnGlass(Me.Handle, FileSubTitle, TextFont, TitleRect, EdgeMargin, GlowSize)
        clsGlass.DrawTextOnGlass(Me.Handle, "  " & sFileTime, TextFont, TimeRect, EdgeMargin, GlowSize)

      End If

    Else
      DrawNormal()
    End If
  End Sub

  Private Sub DrawNormal()
    If pnlMain.Dock = DockStyle.None Then
      pnlMain.Dock = DockStyle.Fill
      clsGlass.SetGlass(Me, 0, 0, 0, 0)
      Dim bResize As Boolean = False
      If Me.Size = Me.MinimumSize Then bResize = True
      Me.MinimumSize = New Drawing.Size(MAIN_WIDTH, MAIN_HEIGHT)
      If bResize Then Me.Size = Me.MinimumSize
    End If
  End Sub

  Private Sub tmrUpdate_Tick(sender As System.Object, e As System.EventArgs) Handles tmrUpdate.Tick
    If bCD Then
      If GetSelectedPlayListItem() >= 0 Then
        If Not TrackToNo(dgvPlayList.Rows(GetSelectedPlayListItem).Tag(0)) = cCD.CurrentTrack Then
          If dgvPlayList.Rows.Count > 0 Then
            If GetSelectedPlayListItem() >= 0 Then
              Dim SelItem As Integer = GetSelectedPlayListItem()
              If SelItem < dgvPlayList.Rows.Count - 1 Then
                dgvPlayList.Rows(SelItem).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
                dgvPlayList.Rows(cCD.CurrentTrack - 1).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
              End If
            End If
          End If
          InitialData()
        End If
      End If
      SetTabs(True, False)
      DrawTitleArtist()
      If Not pbProgress.Maximum = cCD.TrackDurationMS Then pbProgress.Maximum = cCD.TrackDurationMS
      If Not pbProgress.Enabled Then pbProgress.Enabled = True
      If Not SeekS AndAlso pbProgress.Value <> cCD.TrackPositionMS Then
        pbProgress.Value = cCD.TrackPositionMS
        If taskBar IsNot Nothing Then taskBar.SetProgressValue(Me.Handle, pbProgress.Value, pbProgress.Maximum)
      End If
    ElseIf String.IsNullOrEmpty(mpPlayer.FileName) Then
      SetTabs(False, False)
      If Not Me.Text = "Lime Seed Media Player" Then Me.Text = "Lime Seed Media Player"
      If pnlMain.Dock = DockStyle.None Then DrawGlassText(True)
      If Not pbProgress.Value = 0 Then pbProgress.Value = 0
      If Not pbProgress.Maximum = 1 Then pbProgress.Maximum = 1
      If pbProgress.Enabled Then pbProgress.Enabled = False
    Else
      If mpPlayer.HasVid Then
        SetTabs(False, True)
        If cmbChapters.Visible And Not cmbChapters.DroppedDown And cmbChapters.Tag IsNot Nothing Then
          Dim ChapterCollection As Collection = CType(cmbChapters.Tag, Collection)
          cmbChapters.Tag = Nothing
          If ChapterCollection IsNot Nothing Then
            Dim lastVal As String = String.Empty
            For Each chapter In ChapterCollection
              If chapter(0) > mpPlayer.Position Then
                If String.IsNullOrEmpty(lastVal) Then
                  cmbChapters.SelectedIndex = 0
                Else
                  cmbChapters.Text = lastVal
                End If
                Exit For
              End If
              lastVal = chapter(1)
            Next
          End If
          cmbChapters.Tag = ChapterCollection
        End If
      Else
        SetTabs(True, False)
      End If
      DrawTitleArtist()
      If Not pbProgress.Maximum = mpPlayer.Duration * 1000 Then pbProgress.Maximum = mpPlayer.Duration * 1000
      If Not pbProgress.Enabled Then pbProgress.Enabled = True
      If Not SeekS AndAlso pbProgress.Value <> mpPlayer.Position * 1000 Then
        pbProgress.Value = mpPlayer.Position * 1000
        If taskBar IsNot Nothing Then taskBar.SetProgressValue(Me.Handle, Math.Floor(pbProgress.Value), Math.Floor(pbProgress.Maximum))
      End If
    End If
    If bCD Then
      If cCD.Status = Seed.clsAudioCD.PlayStatus.Playing Then
        SetPlayPause(False, True)
        SetStop(True)
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NORMAL)
      ElseIf cCD.Status = Seed.clsAudioCD.PlayStatus.Paused Then
        SetPlayPause(True, True)
        SetStop(True)
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_PAUSED)
      ElseIf cCD.Status = Seed.clsAudioCD.PlayStatus.Stopped Then
        SetPlayPause(True, True)
        SetStop(False)
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NOPROGRESS)
      End If
      cmdMenu.Visible = False
    ElseIf bDVD Then
      Dim uOps = mpPlayer.GetDVDCurrentUOPS
      Dim bPaused As Boolean = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.PauseOn) = DirectShowLib.Dvd.ValidUOPFlag.PauseOn)
      Dim bResume As Boolean = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.Resume) = DirectShowLib.Dvd.ValidUOPFlag.Resume)
      Dim bStop As Boolean = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.Stop) = DirectShowLib.Dvd.ValidUOPFlag.Stop)
      SetPlayPause(Not bPaused, bResume)
      SetStop(bStop)
      Select Case mpPlayer.GetDVDCurrentDomain
        Case DirectShowLib.Dvd.DvdDomain.FirstPlay
          SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_INDETERMINATE)
          pbProgress.Enabled = False
        Case DirectShowLib.Dvd.DvdDomain.Stop
          SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NOPROGRESS)
          pbProgress.Enabled = False
        Case DirectShowLib.Dvd.DvdDomain.Title
          SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NORMAL)
          pbProgress.Enabled = True
        Case DirectShowLib.Dvd.DvdDomain.VideoTitleSetMenu
          SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_INDETERMINATE)
          pbProgress.Enabled = False
        Case DirectShowLib.Dvd.DvdDomain.VideoManagerMenu
          SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_INDETERMINATE)
          pbProgress.Enabled = False
      End Select
      cmdMenu.Visible = True
      cmdMenu.Enabled = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.ShowMenuRoot) = DirectShowLib.Dvd.ValidUOPFlag.ShowMenuRoot)
      cmbAudTrack.Enabled = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.SelectAudioStream) = DirectShowLib.Dvd.ValidUOPFlag.SelectAudioStream)
      cmbSubtitles.Enabled = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.SelectSubPicStream) = DirectShowLib.Dvd.ValidUOPFlag.SelectSubPicStream)
      cmbVidTrack.Enabled = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.PlayTitle) = DirectShowLib.Dvd.ValidUOPFlag.PlayTitle)
      cmbChapters.Enabled = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.PlayChapter) = DirectShowLib.Dvd.ValidUOPFlag.PlayChapter)
      mnuRatio.Tag = CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.SelectVideoModePreference) = DirectShowLib.Dvd.ValidUOPFlag.SelectVideoModePreference)
    Else
      If mpPlayer.State = Seed.ctlSeed.MediaState.mPlaying Then
        SetPlayPause(False, True)
        SetStop(True)
        If Not cmdLoop.Enabled Then cmdLoop.Enabled = True
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NORMAL)
      ElseIf mpPlayer.State = Seed.ctlSeed.MediaState.mPaused Then
        SetPlayPause(True, True)
        SetStop(True)
        If Not cmdLoop.Enabled Then cmdLoop.Enabled = True
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_PAUSED)
      ElseIf mpPlayer.State = Seed.ctlSeed.MediaState.mStopped Then
        SetPlayPause(True, True)
        SetStop(False)
        If Not cmdLoop.Enabled Then cmdLoop.Enabled = True
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NOPROGRESS)
      ElseIf mpPlayer.State = Seed.ctlSeed.MediaState.mClosed Then
        SetPlayPause(True, False)
        SetStop(False)
        If cmdLoop.Enabled Then cmdLoop.Enabled = False
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NOPROGRESS)
      End If
      cmdMenu.Visible = False
    End If
    If bDVD Then
      Try
        Dim uOps = mpPlayer.GetDVDCurrentUOPS
        Dim bPrevious As Boolean = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.PlayPrevOrReplay_Chapter) = DirectShowLib.Dvd.ValidUOPFlag.PlayPrevOrReplay_Chapter)
        Dim bNext As Boolean = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.PlayNextChapter) = DirectShowLib.Dvd.ValidUOPFlag.PlayNextChapter)
        If Not txtPlayListTitle.Text = My.Computer.FileSystem.GetDriveInfo(mpPlayer.FileName.Substring(0, 3)).VolumeLabel & " (--:--/--:--)" Then txtPlayListTitle.Text = My.Computer.FileSystem.GetDriveInfo(mpPlayer.FileName.Substring(0, 3)).VolumeLabel & " (--:--/--:--)"
        If Not txtPlayListTitle.Tag = My.Computer.FileSystem.GetDriveInfo(mpPlayer.FileName.Substring(0, 3)).VolumeLabel Then txtPlayListTitle.Tag = My.Computer.FileSystem.GetDriveInfo(mpPlayer.FileName.Substring(0, 3)).VolumeLabel
        If cmdLoopPL.Enabled Then cmdLoopPL.Enabled = False
        If cmdShufflePL.Enabled Then cmdShufflePL.Enabled = False
        If cmdBackPL.Enabled <> bPrevious Then cmdBackPL.Enabled = bPrevious
        If cmdNextPL.Enabled <> bNext Then cmdNextPL.Enabled = bNext
        If cmdRemoveFromPL.Enabled Then cmdRemoveFromPL.Enabled = False
        If cmdClearPL.Enabled Then cmdClearPL.Enabled = False
      Catch ex As Exception
        mnuCloseFile.PerformClick()
        cmdShufflePL.Tag = False
        cmdShufflePL.Image = My.Resources.pl_button_order
        dgvPlayList.Rows.Clear()
      End Try
    Else
      If dgvPlayList.Rows.Count > 0 Then
        If StrComp(txtPlayListTitle.Tag, UNKNOWN_ALBUM, CompareMethod.Text) = 0 Then
          Dim sPL As String = String.Empty
          For Each dgvx As DataGridViewRow In dgvPlayList.Rows
            If Not String.IsNullOrEmpty(dgvx.Tag(3)) Then
              If String.IsNullOrEmpty(sPL) Then
                sPL = dgvx.Tag(3)
              ElseIf StrComp(sPL, dgvx.Tag(3), CompareMethod.Text) <> 0 And StrComp(dgvx.Tag(3), UNKNOWN_ALBUM, CompareMethod.Text) = 0 Then
                sPL = UNKNOWN_ALBUM
              End If
            End If
          Next
          txtPlayListTitle.Tag = sPL
        End If
        Dim plCurrent, plTotal As Double
        Dim SelItem As Integer = GetSelectedPlayListItem()
        For I As Integer = 0 To dgvPlayList.RowCount - 1
          plTotal += RevertTimeVal(dgvPlayList.Rows(I).Tag(5))
          If SelItem >= 0 Then
            If SelItem = I Then
              If bCD Then
                plCurrent += cCD.TrackPositionSeconds
              Else
                plCurrent += mpPlayer.Position
              End If
            End If
            If SelItem > I Then plCurrent += RevertTimeVal(dgvPlayList.Rows(I).Tag(5))
          End If
        Next
        If txtPlayListTitle.ReadOnly Then
          Dim sMsg As String = txtPlayListTitle.Tag & " (" & ConvertTimeVal(plCurrent) & "/" & ConvertTimeVal(plTotal) & ")"
          If Not txtPlayListTitle.Text = sMsg Then txtPlayListTitle.Text = sMsg
        End If
        If Not cmdLoopPL.Enabled Then cmdLoopPL.Enabled = True
        If Not cmdShufflePL.Enabled Then cmdShufflePL.Enabled = True
        If Not cmdBackPL.Enabled = (SelItem > 0) Then cmdBackPL.Enabled = (SelItem > 0)
        If Not cmdNextPL.Enabled = (SelItem < dgvPlayList.Rows.Count - 1) Then cmdNextPL.Enabled = (SelItem < dgvPlayList.Rows.Count - 1)
        If Not cmdRemoveFromPL.Enabled Then cmdRemoveFromPL.Enabled = True
        If Not cmdClearPL.Enabled Then cmdClearPL.Enabled = True
      Else
        If Not txtPlayListTitle.Text = UNKNOWN_ALBUM & " (--:--/--:--)" Then txtPlayListTitle.Text = UNKNOWN_ALBUM & " (--:--/--:--)"
        If Not txtPlayListTitle.Tag = UNKNOWN_ALBUM Then txtPlayListTitle.Tag = UNKNOWN_ALBUM
        If cmdLoopPL.Enabled Then cmdLoopPL.Enabled = False
        If cmdShufflePL.Enabled Then cmdShufflePL.Enabled = False
        If cmdBackPL.Enabled Then cmdBackPL.Enabled = False
        If cmdNextPL.Enabled Then cmdNextPL.Enabled = False
        If cmdRemoveFromPL.Enabled Then cmdRemoveFromPL.Enabled = False
        If cmdClearPL.Enabled Then cmdClearPL.Enabled = False
      End If
    End If
    If VisualStyles.VisualStyleInformation.DisplayName = "Aero style" And TaskbarFinder.TaskbarVisible Then
      If taskBar Is Nothing Then taskBar = New TaskbarLib.TaskbarList
      If cTask Is Nothing Then
        cTask = New TaskbarController(Me.Handle, ImgToIco(My.Resources.pl_button_back), "Back", ImgToIco(My.Resources.button_play), "Play", ImgToIco(My.Resources.button_pause), "Pause", ImgToIco(My.Resources.button_stop), "Stop", ImgToIco(My.Resources.pl_button_next), "Next")
        cTask.CreatePreview(pnlMain)
      End If
    End If
    If cTask IsNot Nothing Then
      If Not cTask.BackEnabled = cmdBackPL.Enabled Then cTask.BackEnabled = cmdBackPL.Enabled
      If Not cTask.NextEnabled = cmdNextPL.Enabled Then cTask.NextEnabled = cmdNextPL.Enabled
    End If
    If Not cmdFullScreen.Enabled = mpPlayer.HasVid Or methodDraw IsNot Nothing Then cmdFullScreen.Enabled = mpPlayer.HasVid Or methodDraw IsNot Nothing
    If Me.Visible Then
      Static EveryHalfMinute As Integer
      EveryHalfMinute += 1
      If EveryHalfMinute = 200 Then
        EveryHalfMinute = 0
        CheckDiscs()
        If mpPlayer.HasVid Then
          If LoadFFDShow() Then SetFFDShowVals()
        End If
      ElseIf EveryHalfMinute Mod 10 = 0 And bDVD Then
        ThreadedInitial()
      End If
    End If
    SaveTempPL()
  End Sub

  Private Sub tmrVis_Tick(sender As System.Object, e As System.EventArgs) Handles tmrVis.Tick
    If volDevice IsNot Nothing Then
      tmrVis.Enabled = False
      Dim Channels As Integer = -1
      Try
        Channels = volDevice.AudioMeterInformation.PeakValues.Count
      Catch ex As Exception
        Channels = -1
        Try
          Dim devEnum As New CoreAudioApi.MMDeviceEnumerator
          volDevice = devEnum.GetDefaultAudioEndpoint(CoreAudioApi.EDataFlow.eRender, CoreAudioApi.ERole.eMultimedia)
        Catch ex2 As Exception
          Channels = -1
        End Try
      End Try
      If Channels > 0 Then
        Dim ChanVals(Channels - 1) As Double
        For I As Integer = 0 To Channels - 1
          Try
            ChanVals(I) = volDevice.AudioMeterInformation.PeakValues(I) * 100
          Catch ex As Exception
            ChanVals(I) = 0
          End Try
        Next
        If pctBeat.Image IsNot Nothing Then pctBeat.Image.Dispose()
        pctBeat.Image = DrawChannelLevels(Channels, ChanVals, pctBeat.Tag)
        If ttDisp.GetToolTip(pctBeat) <> "Output Channels: " & Channels Then ttDisp.SetToolTip(pctBeat, "Output Channels: " & Channels)
        If methodDraw IsNot Nothing Then
          If frmFS.Visible And Not mpPlayer.HasVid Then
            If Not frmFS.pctVideo.SizeMode = PictureBoxSizeMode.CenterImage Then frmFS.pctVideo.SizeMode = PictureBoxSizeMode.CenterImage
            If frmFS.pctVideo.DisplayRectangle.Height > 0 And frmFS.pctVideo.DisplayRectangle.Width > 0 Then frmFS.pctVideo.Image = CType(methodDraw.Invoke(objDraw, New Object() {Channels, ChanVals, frmFS.pctVideo.DisplayRectangle.Size}), Drawing.Image)
          Else
            If Not pctAlbumArt.SizeMode = PictureBoxSizeMode.CenterImage Then pctAlbumArt.SizeMode = PictureBoxSizeMode.CenterImage
            If pctAlbumArt.DisplayRectangle.Width > 0 And pctAlbumArt.DisplayRectangle.Height > 0 Then pctAlbumArt.Image = CType(methodDraw.Invoke(objDraw, New Object() {Channels, ChanVals, pctAlbumArt.DisplayRectangle.Size}), Drawing.Image)
          End If
        End If
      End If
      tmrVis.Enabled = True
    End If
  End Sub

  Private Sub SetVisualizations()
    tmrVis.Enabled = False
    If methodDraw IsNot Nothing Then methodDraw = Nothing
    If objDraw IsNot Nothing Then objDraw = Nothing
    If My.Settings.Visualization <> "None" Then
      Dim selPath As String = Application.StartupPath & "\Visualizations\" & My.Settings.Visualization & ".dll"
      If My.Computer.FileSystem.FileExists(selPath) Then
        Dim assem As System.Reflection.Assembly = System.Reflection.Assembly.Load(My.Computer.FileSystem.ReadAllBytes(selPath))
        For Each typeT As Type In assem.GetTypes
          methodDraw = typeT.GetMethod("Draw")
          If methodDraw Is Nothing Then Continue For
          objDraw = Activator.CreateInstance(typeT)
          If My.Settings.Visualization_Rate < 6 Then My.Settings.Visualization_Rate = 6
          If My.Settings.Visualization_Rate > 120 Then My.Settings.Visualization_Rate = 120
          tmrVis.Interval = Int(1000 / My.Settings.Visualization_Rate)
          tmrVis.Enabled = True
          Exit Sub
        Next
      End If
    End If
    tmrVis.Enabled = True
    pctAlbumArt.Image = Nothing
    If frmFS IsNot Nothing Then frmFS.pctVideo.Image = Nothing
  End Sub
#End Region

#Region "Internal Routines"
#Region "Startup Commands"
  Private Sub frmMain_Load(sender As Object, e As System.EventArgs) Handles Me.Load
    If Command() = "/hideicons" Then
      'no start menu icons
      Me.Close()
    ElseIf Command() = "/showicons" Then
      'start menu icons
      Me.Close()
    ElseIf Command() = "/reinstall" Then
      'Registry setup, associations, et al...
      If My.Computer.FileSystem.FileExists(Application.StartupPath & "\lame_enc.dll") Then My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\lame_enc.dll")
      If Environment.Is64BitProcess Then
        My.Computer.FileSystem.WriteAllBytes(Application.StartupPath & "\lame_enc.dll", My.Resources.lame_enc_x64, False)
      Else
        My.Computer.FileSystem.WriteAllBytes(Application.StartupPath & "\lame_enc.dll", My.Resources.lame_enc_x86, False)
      End If
      MediaClientSetup(True)
      FFDShowRemote(True)
      If My.Computer.FileSystem.FileExists(Application.StartupPath & "\LSFA.exe") Then Process.Start(Application.StartupPath & "\LSFA.exe", "Associate: MPC AC3 AIF ASF AU MID APE FLAC OFR TTA WAV AVI DIVX IVF MKV MKA FLV SPL SWF CDA DVD MPE MPG M1V MP2 M2V MP3 MPA AAC M2TS M4A M4P M4V OGG OGM MOV DV 3GP 3G2 RA RM RV VFW WMP WM WMA WMV M3U PLS LLPL DIR ")
      Me.Close()
    ElseIf Command() = "/uninstall" Then
      MediaClientSetup(False)
      FFDShowRemote(False)
      If My.Computer.FileSystem.FileExists(Application.StartupPath & "\LSFA.exe") Then Process.Start(Application.StartupPath & "\LSFA.exe", "Associate: ")
      Me.Close()
    Else
      volControl.SetVolume(90)
      frmText.SetText("Loading")
      frmText.Hide()

      If VisualStyles.VisualStyleInformation.DisplayName = "Aero style" And TaskbarFinder.TaskbarVisible Then
        taskBar = New TaskbarLib.TaskbarList
        cTask = New TaskbarController(Me.Handle, ImgToIco(My.Resources.pl_button_back), "Back", ImgToIco(My.Resources.button_play), "Play", ImgToIco(My.Resources.button_pause), "Pause", ImgToIco(My.Resources.button_stop), "Stop", ImgToIco(My.Resources.pl_button_next), "Next")
        cTask.CreatePreview(pnlMain)
        VidThumb = False
      Else
        taskBar = Nothing
      End If
      mnuMain.Renderer = New ToolStripSystemRenderer(True)
      mnuPL.Renderer = New ToolStripSystemRenderer(True)
      mnuVideo.Renderer = New ToolStripSystemRenderer(True)
      mnuAlbumArt.Renderer = New ToolStripSystemRenderer(True)
      pctBeat.Tag = My.Settings.BeatBG
      mnuScaleNorm.Checked = True
      mnuRatioAutomatic.Checked = True
      tbsView.TabPages.Remove(tabRipper)
      cmdLoopPL.Tag = 0
      frmFS = New frmFullScreen
      frmFS.ParentPlayer = Me
      Dim tW, tH, tX, tY As Integer
      tW = My.Settings.Size.Width
      tH = My.Settings.Size.Height
      If tW > My.Computer.Screen.Bounds.Width Then tW = 640
      If tH > My.Computer.Screen.Bounds.Height Then tH = 480
      tX = My.Settings.Location.X
      tY = My.Settings.Location.Y
      If tX < 0 Then tX = 0
      If tX > My.Computer.Screen.Bounds.Width - tW Then tX = My.Computer.Screen.Bounds.Width - tW
      If tY < 0 Then tY = 0
      If tY > My.Computer.Screen.Bounds.Height - tH Then tY = My.Computer.Screen.Bounds.Height - tH
      If Me.instanceID > 0 Then
        tX += (Me.instanceID * 20)
        tY += (Me.instanceID * 20)
      End If
      Me.Location = New Drawing.Point(tX, tY)
      Me.Size = New Drawing.Size(tW, tH)
      mpPlayer.AudioDevice = My.Settings.Device
      mpPlayer.QueueTime = My.Settings.Gapless
      If Me.Tag Is Nothing Then
        tmrCommandCycle.Stop()
        Dim commands As New ObjectModel.Collection(Of String)
        If tmrCommandCycle.Tag IsNot Nothing Then commands = tmrCommandCycle.Tag
        For Each item In My.Application.CommandLineArgs
          commands.Add(item)
        Next
        tmrCommandCycle.Tag = commands
        tmrCommandCycle.Start()
      Else
        If Me.Tag <> "NOCMD" Then Debug.Print(Me.Tag)
        Me.Tag = Nothing
      End If
      InitialData()
      SetTabs(False, False)
      CheckDiscs()
      DrawGlassText(True)
      joyPad = New clsJoyDetection
      LabelShortcuts()
      Try
        Dim devEnum As New CoreAudioApi.MMDeviceEnumerator
        volDevice = devEnum.GetDefaultAudioEndpoint(CoreAudioApi.EDataFlow.eRender, CoreAudioApi.ERole.eMultimedia)
      Catch ex As Exception

      End Try
      SetVisualizations()
    End If
  End Sub

  Private Sub frmMain_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
    Randomize()
    Dim commands As ObjectModel.Collection(Of String) = tmrCommandCycle.Tag
    If commands IsNot Nothing AndAlso commands.Count = 0 And instanceID = 0 Then LoadTempPL()
    tmrUpdate.Start()
  End Sub

  Public Sub StartupRun(Commands As Collections.ObjectModel.ReadOnlyCollection(Of String))
    If Commands.Count = 0 Then Exit Sub
    If Commands.Count = 1 Then
      Dim sCmd As String = Commands(0)
      If sCmd.StartsWith("""") And sCmd.EndsWith("""") Then sCmd = sCmd.Substring(1, sCmd.Length - 2)
      If mpPlayer.State = Seed.ctlSeed.MediaState.mClosed Then
        If My.Computer.FileSystem.FileExists(sCmd) Then
          Select Case IO.Path.GetExtension(sCmd).ToLower
            Case ".llpl", ".m3u", ".pls" : OpenPlayList(sCmd, True)
            Case Else
              mpPlayer.SetNoQueue()
              OpenFile(sCmd, True)
              ThreadedInitial()
          End Select
        ElseIf My.Computer.FileSystem.DirectoryExists(sCmd) Then
          If sCmd.EndsWith("VIDEO_TS") Then
            OpenDVD(sCmd)
          Else
            txtPlayListTitle.Tag = IO.Path.GetFileName(sCmd)
            AddDirToPlayListAndMaybePlay(sCmd)
          End If
        End If
      Else
        If My.Computer.FileSystem.FileExists(sCmd) Then
          Select Case IO.Path.GetExtension(sCmd).ToLower
            Case ".llpl", ".m3u", ".pls" : OpenPlayList(sCmd, False)
            Case Else
              If dgvPlayList.Rows.Count > 0 Then
                AddToPlayList(sCmd)
              Else
                OpenFile(sCmd, True)
                ThreadedInitial()
              End If
          End Select
        ElseIf My.Computer.FileSystem.DirectoryExists(sCmd) Then
          If sCmd.EndsWith("VIDEO_TS") Then
            AddToPlayList(sCmd)
          Else
            txtPlayListTitle.Tag = IO.Path.GetFileName(sCmd)
            AddDirToPlayListAndMaybePlay(sCmd)
          End If
        End If
      End If
    ElseIf Commands(0) = "/AudioCD" Then
      OpenCD(Commands(1))
    ElseIf Commands(0) = "/DVD" Then
      OpenDVD(Commands(1))
    Else
      If mpPlayer.State = Seed.ctlSeed.MediaState.mClosed Then
        For I As Integer = 0 To Commands.Count - 1
          Dim sCmd As String = Commands(I)
          If sCmd.StartsWith("""") And sCmd.EndsWith("""") Then sCmd = sCmd.Substring(1, sCmd.Length - 2)
          If My.Computer.FileSystem.FileExists(sCmd) Then
            Select Case IO.Path.GetExtension(sCmd).ToLower
              Case ".llpl", ".m3u", ".pls" : OpenPlayList(sCmd, False)
              Case Else
                AddToPlayList(sCmd, , , False)
            End Select
          ElseIf My.Computer.FileSystem.DirectoryExists(sCmd) Then
            txtPlayListTitle.Tag = IO.Path.GetFileName(sCmd)
            AddDirToPlayListAndMaybePlay(sCmd)
          End If
        Next
        If dgvPlayList.Rows.Count > 0 Then
          dgvPlayList.Rows(0).Selected = True
          StartPlayList()
        End If
        QueueFullPlayListData()
      Else
        For I As Integer = 0 To Commands.Count - 1
          Dim sCmd As String = Commands(I)
          If sCmd.StartsWith("""") And sCmd.EndsWith("""") Then sCmd = sCmd.Substring(1, sCmd.Length - 2)
          If My.Computer.FileSystem.FileExists(sCmd) Then
            Select Case IO.Path.GetExtension(sCmd).ToLower
              Case ".llpl", ".m3u", ".pls" : OpenPlayList(sCmd, False)
              Case Else : AddToPlayList(sCmd, , , False)
            End Select
          ElseIf My.Computer.FileSystem.DirectoryExists(sCmd) Then
            txtPlayListTitle.Tag = IO.Path.GetFileName(sCmd)
            AddDirToPlayListAndMaybePlay(sCmd)
          End If
        Next
        QueueFullPlayListData()
      End If
    End If
  End Sub

  Private Sub tmrCommandCycle_Tick(sender As System.Object, e As System.EventArgs) Handles tmrCommandCycle.Tick
    tmrCommandCycle.Stop()
    If tmrCommandCycle.Tag Is Nothing Then Exit Sub
    Dim colTmp As ObjectModel.Collection(Of String) = tmrCommandCycle.Tag
    tmrCommandCycle.Tag = Nothing
    Dim aryTmp(colTmp.Count - 1) As String
    For I As Integer = 0 To colTmp.Count - 1
      aryTmp(I) = colTmp(I)
    Next
    Array.Sort(aryTmp)
    Dim commandCollection As ObjectModel.ReadOnlyCollection(Of String) = New ObjectModel.ReadOnlyCollection(Of String)(aryTmp)
    If commandCollection.Count > 0 Then StartupRun(commandCollection)
  End Sub

  Private Function CombineLabels(sA As String, sB As String, Optional sFmt As String = "%1") As String
    Dim sC As String = String.Empty
    If Not My.Settings.Keyboard Then sA = "None"
    If Not My.Settings.Gamepad Then sB = "None"
    If sA <> "None" Then sC = sA
    If sB <> "None" Then sC &= IIf(String.IsNullOrEmpty(sC), sB, " or " & sB)
    If String.IsNullOrEmpty(sC) Then Return Nothing
    Return Replace(sFmt, "%1", sC.Replace(" + ", "+"))
  End Function

  Private Sub LabelShortcuts()
    mnuAbout.ShortcutKeyDisplayString = CombineLabels(My.Settings.Keyboard_About, My.Settings.Gamepad_About)
    ttDisp.SetToolTip(cmdAddToPL, "Add to Playlist" & CombineLabels(My.Settings.Keyboard_AddToPL, My.Settings.Gamepad_AddToPL, " (%1)"))
    ttDisp.SetToolTip(cmdClearPL, "Clear Playlist" & CombineLabels(My.Settings.Keyboard_ClearPL, My.Settings.Gamepad_ClearPL, " (%1)"))
    mnuCloseFile.ShortcutKeyDisplayString = CombineLabels(My.Settings.Keyboard_Close, My.Settings.Gamepad_Close)
    mnuFullScreen.ShortcutKeyDisplayString = CombineLabels(My.Settings.Keyboard_FS, My.Settings.Gamepad_FS)
    ttDisp.SetToolTip(cmdFullScreen, "Full Screen" & CombineLabels(My.Settings.Keyboard_FS, My.Settings.Gamepad_FS, " (%1)"))
    ttDisp.SetToolTip(cmdBackPL, "Return to Previous Track" & CombineLabels(My.Settings.Keyboard_Last, My.Settings.Gamepad_Last, " (%1)"))
    ttDisp.SetToolTip(cmdMenu, "Disc Menu" & CombineLabels(My.Settings.Keyboard_DVDMenu, My.Settings.Gamepad_DVDMenu, " (%1)"))
    ttDisp.SetToolTip(cmdMute, "Mute" & CombineLabels(My.Settings.Keyboard_Mute, My.Settings.Gamepad_Mute, " (%1)"))
    ttDisp.SetToolTip(cmdNextPL, "Skip to Next Track" & CombineLabels(My.Settings.Keyboard_Next, My.Settings.Gamepad_Next, " (%1)"))
    mnuOpenFile.ShortcutKeyDisplayString = CombineLabels(My.Settings.Keyboard_Open, My.Settings.Gamepad_Open)
    ttDisp.SetToolTip(cmdOpenPL, "Open PlayList" & CombineLabels(My.Settings.Keyboard_OpenPL, My.Settings.Gamepad_OpenPL, " (%1)"))
    ttDisp.SetToolTip(cmdPlayPause, "Play/Pause" & CombineLabels(My.Settings.Keyboard_PlayPause, My.Settings.Gamepad_PlayPause, " (%1)"))
    mnuProperties.ShortcutKeyDisplayString = CombineLabels(My.Settings.Keyboard_Props, My.Settings.Gamepad_Props)
    ttDisp.SetToolTip(cmdRemoveFromPL, "Remove Selected Track" & CombineLabels(My.Settings.Keyboard_RemoveFromPL, My.Settings.Gamepad_RemoveFromPL, " (%1)"))
    ttDisp.SetToolTip(txtPlayListTitle, "Rename PlayList" & CombineLabels(My.Settings.Keyboard_RenamePL, My.Settings.Gamepad_RenamePL, " (%1)"))
    Dim iVal As Integer = cmdLoopPL.Tag
    Select Case iVal
      Case 0 : ttDisp.SetToolTip(cmdLoopPL, "Stop at End of PlayList" & CombineLabels(My.Settings.Keyboard_RepeatPL, My.Settings.Gamepad_RepeatPL, " (%1)"))
      Case 1 : ttDisp.SetToolTip(cmdLoopPL, "Loop at End of PlayList" & CombineLabels(My.Settings.Keyboard_RepeatPL, My.Settings.Gamepad_RepeatPL, " (%1)"))
      Case 2 : ttDisp.SetToolTip(cmdLoopPL, "Close Lime Seed at End of PlayList" & CombineLabels(My.Settings.Keyboard_RepeatPL, My.Settings.Gamepad_RepeatPL, " (%1)"))
      Case 3 : ttDisp.SetToolTip(cmdLoopPL, "Restart Computer at End of PlayList" & CombineLabels(My.Settings.Keyboard_RepeatPL, My.Settings.Gamepad_RepeatPL, " (%1)"))
      Case 4 : ttDisp.SetToolTip(cmdLoopPL, "Shut Down Computer at End of PlayList" & CombineLabels(My.Settings.Keyboard_RepeatPL, My.Settings.Gamepad_RepeatPL, " (%1)"))
      Case 5 : ttDisp.SetToolTip(cmdLoopPL, "Hibernate Computer at End of PlayList" & CombineLabels(My.Settings.Keyboard_RepeatPL, My.Settings.Gamepad_RepeatPL, " (%1)"))
    End Select
    If bCD Or bDVD Then
      ttDisp.SetToolTip(cmdLoop, "Eject" & CombineLabels(My.Settings.Keyboard_DiscEject, My.Settings.Gamepad_DiscEject, " (%1)"))
    Else
      ttDisp.SetToolTip(cmdLoop, "Loop Track" & CombineLabels(My.Settings.Keyboard_RepeatTrack, My.Settings.Gamepad_RepeatTrack, " (%1)"))
    End If
    ttDisp.SetToolTip(cmdSavePL, "Save PlayList" & CombineLabels(My.Settings.Keyboard_SavePL, My.Settings.Gamepad_SavePL, " (%1)"))
    mnuSettings.ShortcutKeyDisplayString = CombineLabels(My.Settings.Keyboard_Settings, My.Settings.Gamepad_Settings)
    ttDisp.SetToolTip(cmdShufflePL, "Shuffle" & CombineLabels(My.Settings.Keyboard_Shuffle, My.Settings.Gamepad_Shuffle, " (%1)"))
    ttDisp.SetToolTip(cmdStop, "Stop" & CombineLabels(My.Settings.Keyboard_Stop, My.Settings.Gamepad_Stop, " (%1)"))
    mnuWebpage.ShortcutKeyDisplayString = CombineLabels(My.Settings.Keyboard_Webpage, My.Settings.Gamepad_Webpage)
  End Sub
#End Region

#Region "Drag/Drop"
  Public Sub DragDropEvent(sender As Object, e As DragEventArgs)
    If e.Data.GetFormats(True).Contains("FileDrop") Then
      Dim Data = e.Data.GetData("FileDrop")
      Dim dragDropInvoker As New DragDropFilesInvoker(AddressOf DragDropFiles)
      dragDropInvoker.BeginInvoke(Data, My.Computer.Keyboard.CtrlKeyDown, Nothing, Nothing)
    Else
      e.Effect = DragDropEffects.None
    End If
  End Sub

  Private Delegate Sub DragDropFilesInvoker(Data As Object, ControlKey As Boolean)
  Private Sub DragDropFiles(Data As Object, ControlKey As Boolean)
    If Me.InvokeRequired Then
      Me.BeginInvoke(New DragDropFilesInvoker(AddressOf DragDropFiles), Data, ControlKey)
    Else
      Dim sData() As String = Data
      If dgvPlayList.RowCount = 0 Then
        If Not String.IsNullOrEmpty(mpPlayer.FileName) And ControlKey Then
          AddToPlayList(mpPlayer.FileName)
          dgvPlayList.Rows(0).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
          'InitialData()
          For Each Item In sData
            If My.Computer.FileSystem.FileExists(Item) Then
              Select Case IO.Path.GetExtension(Item).ToLower
                Case ".llpl", ".m3u", ".pls" : OpenPlayList(Item)
                Case Else : AddToPlayList(Item)
              End Select
            ElseIf My.Computer.FileSystem.DirectoryExists(Item) Then
              AddDirToPlayList(Item)
            End If
            Application.DoEvents()
          Next
        Else
          If UBound(sData) = 0 Then
            If My.Computer.FileSystem.FileExists(sData(0)) Then
              If ControlKey Then
                Select Case IO.Path.GetExtension(sData(0)).ToLower
                  Case ".llpl", ".m3u", ".pls" : OpenPlayList(sData(0))
                  Case Else
                    AddToPlayList(sData(0))
                    StartPlayList()
                End Select
              Else
                Select Case IO.Path.GetExtension(sData(0)).ToLower
                  Case ".llpl", ".m3u", ".pls" : OpenPlayList(sData(0), True)
                  Case Else
                    OpenFile(sData(0), True)
                    ThreadedInitial()
                End Select
              End If
            ElseIf My.Computer.FileSystem.DirectoryExists(sData(0)) Then
              txtPlayListTitle.Tag = IO.Path.GetFileName(sData(0))
              AddDirToPlayListAndMaybePlay(sData(0))
            End If
          Else
            For Each Item In sData
              If My.Computer.FileSystem.FileExists(Item) Then
                Select Case IO.Path.GetExtension(Item).ToLower
                  Case ".llpl", ".m3u", ".pls" : OpenPlayList(Item)
                  Case Else : AddToPlayList(Item)
                End Select
              ElseIf My.Computer.FileSystem.DirectoryExists(Item) Then
                AddDirToPlayList(Item)
              End If
              Application.DoEvents()
            Next
            StartPlayList()
          End If
        End If
      Else
        For Each Item In sData
          If My.Computer.FileSystem.FileExists(Item) Then
            Select Case IO.Path.GetExtension(Item).ToLower
              Case ".llpl", ".m3u", ".pls" : OpenPlayList(Item)
              Case Else : AddToPlayList(Item)
            End Select
          ElseIf My.Computer.FileSystem.DirectoryExists(Item) Then
            AddDirToPlayList(Item)
          End If
          Application.DoEvents()
        Next
      End If
    End If
  End Sub

  Public Sub DragEnterEvent(sender As Object, e As DragEventArgs)
    e.Effect = DragDropEffects.All
  End Sub

  Public Sub DragOverEvent(sender As Object, e As DragEventArgs)
    If e.Data.GetFormats(True).Contains("FileDrop") Then
      Dim Data = e.Data.GetData("FileDrop")
      Dim sTrackData As String
      If dgvPlayList.RowCount = 0 Then
        If Not String.IsNullOrEmpty(mpPlayer.FileName) And My.Computer.Keyboard.CtrlKeyDown Then
          e.Effect = DragDropEffects.Copy
          If UBound(Data) = 0 Then
            sTrackData = "Add """ & IO.Path.GetFileName(Data(0)) & """ to PlayList"
          Else
            sTrackData = "Add " & UBound(Data) + 1 & " Tracks to Playlist" & vbNewLine
            For Each File In Data
              sTrackData &= "  " & IO.Path.GetFileName(File) & vbNewLine
            Next
            sTrackData = sTrackData.Substring(0, sTrackData.Length - 2)
          End If
        Else
          If UBound(Data) = 0 AndAlso My.Computer.FileSystem.FileExists(Data(0)) Then
            If My.Computer.Keyboard.CtrlKeyDown Then
              sTrackData = "Add """ & IO.Path.GetFileName(Data(0)) & """ to PlayList and Play"
              e.Effect = DragDropEffects.Link
            Else
              sTrackData = "Play """ & IO.Path.GetFileName(Data(0)) & """"
              e.Effect = DragDropEffects.Move
            End If
          Else
            If UBound(Data) = 0 Then
              sTrackData = "Add """ & IO.Path.GetFileName(Data(0)) & """ to PlayList and Play First Track"
            Else
              sTrackData = "Add " & UBound(Data) + 1 & " Tracks to Playlist and Play First Track" & vbNewLine
              For Each File In Data
                sTrackData &= "  " & IO.Path.GetFileName(File) & vbNewLine
              Next
              sTrackData = sTrackData.Substring(0, sTrackData.Length - 2)
            End If
            e.Effect = DragDropEffects.Link
          End If
        End If
      Else
        e.Effect = DragDropEffects.Copy
        If UBound(Data) = 0 Then
          sTrackData = "Add """ & IO.Path.GetFileName(Data(0)) & """ to PlayList"
        Else
          sTrackData = "Add " & UBound(Data) + 1 & " Tracks to Playlist" & vbNewLine
          For Each File In Data
            sTrackData &= "  " & IO.Path.GetFileName(File) & vbNewLine
          Next
          sTrackData = sTrackData.Substring(0, sTrackData.Length - 2)
        End If
      End If
      CursorText(sTrackData)
    Else
      e.Effect = DragDropEffects.None
    End If
  End Sub
#End Region

  Private Function LoadFFDShow() As Boolean
    Try
      If ffAPI IsNot Nothing Then
        If StrComp(mpPlayer.FileName, ffAPI.getFileName, CompareMethod.Text) = 0 Then
          If ffAPI.checkFFDShowActive Then Return True
        End If
      End If
      ffAPI = Nothing
      If mpPlayer.Position = 0 And (mpPlayer.State = Seed.ctlSeed.MediaState.mStopped Or mpPlayer.State = Seed.ctlSeed.MediaState.mOpen) Then
        Dim bMute As Boolean = mpPlayer.Mute
        mpPlayer.Mute = True
        mpPlayer.mpPlay()
        Application.DoEvents()
        mpPlayer.mpPause()
        mpPlayer.Position = 0
        mpPlayer.Mute = bMute
      End If
      Dim ffInstances = FFDShowAPI.FFDShowAPI.getFFDShowInstances
      If ffInstances.Count = 0 Then
        FFDShowRemote(True)
      Else
        For I As Integer = 0 To ffInstances.Count - 1
          If StrComp(mpPlayer.FileName, ffInstances(I).fileName, CompareMethod.Text) = 0 Then
            ffAPI = New FFDShowAPI.FFDShowAPI(ffInstances(I).handle)
            Exit For
          End If
        Next
      End If
      If ffAPI IsNot Nothing Then
        Return ffAPI.checkFFDShowActive
      Else
        Return False
      End If
    Catch ex As Exception
      Return False
    End Try
  End Function

  Friend Sub OpenFile(Path As String, Optional ByVal AutoPlay As Boolean = False)
    FirstInit = True
    If bCD Then
      cCD.Dispose()
      cCD = Nothing
    End If
    bCD = False
    bDVD = False
    If IO.Path.GetExtension(Path).ToLower = ".cda" Or Path.Substring(1, 7) = ":\Track" Then
      mpPlayer.FileName = String.Empty
      'CD audio track
      cCD = New Seed.clsAudioCD
      Debug.Print("Open CD Track " & Path)
      cCD.ChangeDrive(Path(0))
      If cCD.CDAvailable Then
        cCD.Stop()
        cCD.CurrentTrack = TrackToNo(Path)
        cCD.Play()
        bCD = True
      End If
    Else
      mpPlayer.FileName = Path
      If mpPlayer.FileName <> Path Then
        Dim sTmp As String = mpPlayer.FileName
        mpPlayer.FileName = Nothing
        MsgBox("Lime Seed was unable to play the file """ & IO.Path.GetFileName(Path) & """. Error returned: " & vbNewLine & sTmp, MsgBoxStyle.Exclamation, "Media Playback Error")
      Else
        pbProgress.Maximum = mpPlayer.Duration * 1000
        pbProgress.Value = mpPlayer.Position
        mpPlayer.LinearVolume = bpgVolume.Value
        mpPlayer.StateFade = False
        mpPlayer.Position = 0
        If AutoPlay Then mpPlayer.mpPlay()
      End If
    End If
  End Sub

  Private Sub OpenCD(Drive As String)
    mpPlayer.FileName = String.Empty
    If cCD Is Nothing Then cCD = New Seed.clsAudioCD
    cCD.ChangeDrive(Drive(0))
    cmdShufflePL.Tag = False
    cmdShufflePL.Image = My.Resources.pl_button_order
    dgvPlayList.Rows.Clear()
    cCD.SetTimeFormat(Seed.clsAudioCD.TimeFormat.MS)
    For I As Integer = 1 To cCD.TotalTracks
      AddToPlayList(Drive.Substring(0, 3) & "Track" & I, "Track " & I, ConvertTimeVal(cCD.CDMediaInfo(Seed.clsAudioCD.MediaOption.TrackLength, I) / 1000), False)
    Next
    getAlbumInfo = New AlbumInfo(New IO.DriveInfo(Drive(0)))
    StartPlayList()
  End Sub

  Private Sub OpenDVD(Drive As String)
    If Not Drive.EndsWith("\") Then Drive &= "\"
    If Drive.EndsWith("\VIDEO_TS\") Then Drive = Drive.Substring(0, Drive.Length - 9)
    OpenFile(Drive & "VIDEO_TS", True)
    If mpPlayer.FileName = Drive & "VIDEO_TS" Then
      bDVD = True
      ThreadedInitial()
    End If
  End Sub

  Private Function GetTitle(Path As String, Optional separateArtist As Boolean = False, Optional getArtist As Boolean = False) As String
    If String.IsNullOrEmpty(Path) Then Return "Lime Seed Media Player"
    Dim Artist As String = String.Empty
    Dim Title As String = String.Empty
    If IO.Path.GetExtension(Path).ToLower = ".mp3" Then
      Using ID3v2Tags As New Seed.clsID3v2(Path)
        If ID3v2Tags.HasID3v2Tag Then
          If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TP1")) Then Artist = ID3v2Tags.FindFrame("TP1")
          If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TT2")) Then Title = ID3v2Tags.FindFrame("TT2")
        End If
      End Using
      Using ID3v1Tags As New Seed.clsID3v1(Path)
        If ID3v1Tags.HasID3v1Tag Then
          If String.IsNullOrEmpty(Artist) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Artist) Then Artist = ID3v1Tags.Artist
          If String.IsNullOrEmpty(Title) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Title) Then Title = ID3v1Tags.Title
        End If
      End Using
    ElseIf IO.Path.GetExtension(Path).ToLower = ".ogg" Or IO.Path.GetExtension(Path).ToLower = ".ogm" Or IO.Path.GetExtension(Path).ToLower = ".flac" Then
      Using cVorbis As New Seed.clsVorbis(Path)
        If cVorbis.HasVorbis Then
          If Not String.IsNullOrEmpty(cVorbis.Title) Then Title = cVorbis.Title
          If Not String.IsNullOrEmpty(cVorbis.Artist) Then Artist = cVorbis.Artist
        End If
      End Using
    Else
      Artist = UNKNOWN_ARTIST
      Title = IO.Path.GetFileNameWithoutExtension(Path)
    End If
    If separateArtist Then
      If getArtist Then
        If String.IsNullOrEmpty(Artist) Then
          Return Nothing
        Else
          Return Artist
        End If
      Else
        If String.IsNullOrEmpty(Title) Then
          Return IO.Path.GetFileNameWithoutExtension(Path)
        Else
          Return Title
        End If
      End If
    Else
      If String.IsNullOrEmpty(Artist) Then
        If String.IsNullOrEmpty(Title) Then
          Return IO.Path.GetFileNameWithoutExtension(Path)
        Else
          Return Title
        End If
      Else
        If String.IsNullOrEmpty(Title) Then
          Return Artist & " - " & IO.Path.GetFileNameWithoutExtension(Path)
        Else
          Return Artist & " - " & Title
        End If
      End If
    End If
  End Function

  Private Function GetArt(Path As String, DoSearch As Boolean, ForceSearch As Boolean) As ImageWithName
    If bCD Then
      If DoSearch Then macArt = New AppleNet(dgvPlayList.Rows(GetSelectedPlayListItem).Tag(2), dgvPlayList.Rows(GetSelectedPlayListItem).Tag(3))
      Return Nothing
    Else
      If ForceSearch Then
        Dim Artist As String = String.Empty
        Dim Album As String = String.Empty
        If IO.Path.GetExtension(Path).ToLower = ".mp3" Then
          Using ID3v2Tags As New Seed.clsID3v2(Path)
            If ID3v2Tags.HasID3v2Tag Then
              If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TP1")) Then Artist = ID3v2Tags.FindFrame("TP1")
              If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TAL")) Then Album = ID3v2Tags.FindFrame("TAL")
            End If
          End Using
          Using ID3v1Tags As New Seed.clsID3v1(Path)
            If ID3v1Tags.HasID3v1Tag Then
              If String.IsNullOrEmpty(Artist) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Artist) Then Artist = ID3v1Tags.Artist
              If String.IsNullOrEmpty(Album) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Album) Then Album = ID3v1Tags.Album
            End If
          End Using
        ElseIf IO.Path.GetExtension(Path).ToLower = ".ogg" Or IO.Path.GetExtension(Path).ToLower = ".ogm" Or IO.Path.GetExtension(Path).ToLower = ".flac" Then
          Using cVorbis As New Seed.clsVorbis(Path)
            If cVorbis.HasVorbis Then
              If Not String.IsNullOrEmpty(cVorbis.Album) Then Album = cVorbis.Album
              If Not String.IsNullOrEmpty(cVorbis.Artist) Then Artist = cVorbis.Artist
            End If
          End Using
        Else
          'idunno yet
        End If
        If DoSearch Then macArt = New AppleNet(Artist, Album)
        Return Nothing
      End If
      Dim jPath As String = IO.Path.GetDirectoryName(Path) & "\Folder.jpg"
      If My.Computer.FileSystem.FileExists(jPath) Then
        If Not (IO.File.GetAttributes(jPath) Or IO.FileAttributes.Hidden) = IO.FileAttributes.Hidden Then IO.File.SetAttributes(jPath, IO.FileAttributes.Hidden)
        Return New ImageWithName(jPath)
      End If
      If IO.Path.GetExtension(Path).ToLower = ".mp3" Then
        Using ID3v2Tags As New Seed.clsID3v2(Path)
          If ID3v2Tags.HasID3v2Tag Then
            Dim sPIC As String = IIf(ID3v2Tags.ID3v2Ver = "2.2.0", "PIC", "APIC")
            If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame(sPIC)) Then
              Dim fData As String = ID3v2Tags.FindFrame(sPIC)
              Dim pData() As String = Split(fData, vbNewLine, 4)
              Dim sTmpPath As String = IO.Path.GetTempFileName
              My.Computer.FileSystem.WriteAllText(sTmpPath, pData(3), False, System.Text.Encoding.GetEncoding("latin1"))
              Try
                Return New ImageWithName(PathToImg(sTmpPath), Path)
              Finally
                My.Computer.FileSystem.DeleteFile(sTmpPath)
              End Try
            End If
          End If
        End Using
      ElseIf IO.Path.GetExtension(Path).ToLower = ".ogg" Or IO.Path.GetExtension(Path).ToLower = ".ogm" Or IO.Path.GetExtension(Path).ToLower = ".flac" Then
        Using cVorbis As New Seed.clsVorbis(Path)
          If cVorbis.HasVorbis Then
            If cVorbis.Pictures IsNot Nothing Then
              Debug.Print(cVorbis.Pictures.Count)
              Return cVorbis.Pictures(0).Image.Clone
            End If
          End If
        End Using
      End If
      If IO.Path.GetExtension(Path).ToLower = ".mp3" Then
        Dim Artist As String = String.Empty
        Dim Album As String = String.Empty
        Using ID3v2Tags As New Seed.clsID3v2(Path)
          If ID3v2Tags.HasID3v2Tag Then
            If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TP1")) Then Artist = ID3v2Tags.FindFrame("TP1")
            If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TAL")) Then Album = ID3v2Tags.FindFrame("TAL")
          End If
        End Using
        Using ID3v1Tags As New Seed.clsID3v1(Path)
          If ID3v1Tags.HasID3v1Tag Then
            If String.IsNullOrEmpty(Artist) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Artist) Then Artist = ID3v1Tags.Artist
            If String.IsNullOrEmpty(Album) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Album) Then Album = ID3v1Tags.Album
          End If
        End Using
        If String.IsNullOrEmpty(Artist) And String.IsNullOrEmpty(Album) Then
          Return Nothing
        Else
          If DoSearch Then macArt = New AppleNet(Artist, Album)
          Return Nothing
        End If
      ElseIf IO.Path.GetExtension(Path).ToLower = ".ogg" Or IO.Path.GetExtension(Path).ToLower = ".ogm" Or IO.Path.GetExtension(Path).ToLower = ".flac" Then
        Dim Artist As String = String.Empty
        Dim Album As String = String.Empty
        Using cVorbis As New Seed.clsVorbis(Path)
          If cVorbis.HasVorbis Then
            If Not String.IsNullOrEmpty(cVorbis.Album) Then Album = cVorbis.Album
            If Not String.IsNullOrEmpty(cVorbis.Artist) Then Artist = cVorbis.Artist
          End If
        End Using
        If String.IsNullOrEmpty(Artist) And String.IsNullOrEmpty(Album) Then
          Return Nothing
        Else
          If DoSearch Then macArt = New AppleNet(Artist, Album)
          Return Nothing
        End If
      Else
        Return Nothing
      End If
    End If
  End Function

  <System.Runtime.InteropServices.DllImportAttribute("user32.dll")> Private Shared Function DestroyIcon(handle As IntPtr) As Boolean
  End Function

  Private Function ImgToIco(Img As Drawing.Image) As Drawing.Icon
    Using cBitmap As New Drawing.Bitmap(Img)
      Dim hIcon As IntPtr = cBitmap.GetHicon
      Dim tIcon As Drawing.Icon = Drawing.Icon.FromHandle(hIcon).Clone
      DestroyIcon(hIcon)
      Return tIcon
    End Using
  End Function

  Private Function DrawChannelLevels(Channels As Integer, Levels() As Double, BlackBG As Boolean) As Drawing.Image
    Const Size As Integer = 20
    Dim HalfSize As Integer = Size / 2
    Using imgChannels As New Drawing.Bitmap(Size, Size)
      Using g As Drawing.Graphics = Drawing.Graphics.FromImage(imgChannels)
        If BlackBG Then
          g.Clear(Drawing.Color.Black)
        Else
          g.Clear(Drawing.Color.Transparent)
        End If
        Dim topBrush As New Drawing.Drawing2D.LinearGradientBrush(New Drawing.Point(0, 0), New Drawing.Point(0, HalfSize + 1), Drawing.Color.Red, Drawing.Color.Yellow)
        Dim bottomBrush As New Drawing.Drawing2D.LinearGradientBrush(New Drawing.Point(0, HalfSize - 1), New Drawing.Point(0, Size), Drawing.Color.Yellow, Drawing.Color.Green)
        For chan As Integer = 0 To Channels - 1
          Dim w As Single = Size / Channels
          Dim x As Single = chan * w
          Dim y As Single = Size - (Levels(chan) / 100 * Size)
          If y < HalfSize Then
            g.FillRectangle(topBrush, x, y, w, HalfSize - y)
            g.FillRectangle(bottomBrush, x, HalfSize, w, Size - (HalfSize - y))
          Else
            g.FillRectangle(bottomBrush, x, y, w, Size - y)
          End If
        Next
      End Using
      Return imgChannels.Clone
    End Using
  End Function

  Private Class ImageWithName
    Private mArt As Drawing.Image
    Private mFile As String
    Public Sub New(fileName As String)
      If My.Computer.FileSystem.FileExists(fileName) Then
        mArt = PathToImg(fileName)
        mFile = fileName
      End If
    End Sub
    Public Sub New(picture As Drawing.Image, fileName As String)
      mArt = picture
      mFile = fileName
    End Sub
    Public ReadOnly Property Art As Drawing.Image
      Get
        Return mArt
      End Get
    End Property
    Public Property FileName As String
      Get
        Return mFile
      End Get
      Set(value As String)
        If My.Computer.FileSystem.FileExists(value) Then
          mFile = value
          mArt = PathToImg(value)
        End If
      End Set
    End Property
  End Class

#Region "Shortcut Controls"

  Protected Overrides Function ProcessKeyPreview(ByRef m As System.Windows.Forms.Message) As Boolean
    If Not My.Settings.Keyboard Then Return MyBase.ProcessKeyPreview(m)
    Select Case Me.ActiveControl.Name
      Case ripBox.Name, artList.Name, txtPlayListTitle.Name,
           cmdPlayPause.Name, cmdStop.Name, cmdFullScreen.Name, cmdLoop.Name, cmdMenu.Name, cmbVidTrack.Name, cmbAudTrack.Name, cmbChapters.Name, cmbSubtitles.Name, cmdMute.Name,
           cmdShufflePL.Name, cmdLoopPL.Name, cmdAddToPL.Name, cmdRemoveFromPL.Name, cmdClearPL.Name, cmdSavePL.Name, cmdOpenPL.Name,
           dgvPlayList.Name
        Return MyBase.ProcessKeyPreview(m)
    End Select
    Static Vals As Collection
    If Vals Is Nothing Then Vals = New Collection
    Select Case m.Msg
      Case &H100, &H104
        Dim iKey As Integer = m.WParam.ToInt32
        Dim Key As Keys = iKey
        If Not Vals.Contains(iKey) Then Vals.Add(KeyToStr(Key), iKey)
      Case &H101, &H105
        Dim iKey As Integer = m.WParam.ToInt32
        Dim Key As Keys = iKey
        If Not Vals.Contains(iKey) Then Vals.Add(KeyToStr(Key), iKey)
        Dim sVals As String = String.Empty
        For Each sKey In Vals
          If sKey = "Alt" And Not (ModifierKeys And Keys.Alt) = Keys.Alt Then

          ElseIf sKey = "Ctrl" And Not (ModifierKeys And Keys.Control) = Keys.Control Then

          ElseIf sKey = "Shift" And Not (ModifierKeys And Keys.Shift) = Keys.Shift Then

          Else
            sVals &= sKey & " + "
          End If
        Next
        If sVals.EndsWith(" + ") Then sVals = sVals.Substring(0, sVals.Length - 3)
        If tbsView.SelectedTab Is tabPlayList Then
          If My.Settings.Keyboard_AddToPL = sVals And cmdAddToPL.Enabled Then cmdAddToPL_Click(New Object, New EventArgs)
          If My.Settings.Keyboard_ClearPL = sVals And cmdClearPL.Enabled Then cmdClearPL_Click(New Object, New EventArgs)
          If My.Settings.Keyboard_RemoveFromPL = sVals And cmdRemoveFromPL.Enabled Then cmdRemoveFromPL_Click(New Object, New EventArgs)
          If My.Settings.Keyboard_OpenPL = sVals And cmdOpenPL.Enabled Then cmdOpenPL_Click(New Object, New EventArgs)
          If My.Settings.Keyboard_SavePL = sVals And cmdSavePL.Enabled Then cmdSavePL_Click(New Object, New EventArgs)
          If My.Settings.Keyboard_RenamePL = sVals And txtPlayListTitle.Enabled Then txtPlayListTitle_Click(New Object, New EventArgs)
          If My.Settings.Keyboard_RepeatPL = sVals And cmdLoopPL.Enabled Then cmdLoopPL_Click(New Object, New EventArgs)
          If My.Settings.Keyboard_Shuffle = sVals And cmdShufflePL.Enabled Then cmdShufflePL_Click(New Object, New EventArgs)
        End If
        If My.Settings.Keyboard_About = sVals And mnuAbout.Enabled Then mnuAbout_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_Close = sVals And mnuCloseFile.Enabled Then mnuCloseFile_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_DiscEject = sVals And cmdLoop.Enabled And (bCD Or bDVD) Then cmdLoop_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_DVDMenu = sVals And cmdMenu.Enabled Then cmdMenu_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_FS = sVals And cmdFullScreen.Enabled Then cmdFullScreen_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_Last = sVals And cmdBackPL.Enabled Then cmdBackPL_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_Mute = sVals And cmdMute.Enabled Then cmdMute_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_Next = sVals And cmdNextPL.Enabled Then cmdNextPL_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_Open = sVals And mnuOpenFile.Enabled Then mnuOpenFile_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_PlayPause = sVals And cmdPlayPause.Enabled Then cmdPlayPause_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_Props = sVals And mnuProperties.Enabled Then mnuProperties_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_RepeatTrack = sVals And cmdLoop.Enabled And Not (bCD Or bDVD) Then cmdLoop_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_Settings = sVals And mnuSettings.Enabled Then mnuSettings_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_SkipBack = sVals And pbProgress.Enabled Then If mpPlayer.Position > 3 Then mpPlayer.Position -= 3
        If My.Settings.Keyboard_SkipFwd = sVals And pbProgress.Enabled Then If mpPlayer.Position < mpPlayer.Duration - 3 Then mpPlayer.Position += 3
        If My.Settings.Keyboard_Stop = sVals And cmdStop.Enabled Then cmdStop_Click(New Object, New EventArgs)
        If My.Settings.Keyboard_VolDown = sVals And bpgVolume.Enabled Then bpgVolume.Value -= ((bpgVolume.Maximum - bpgVolume.Minimum) / 50)
        If My.Settings.Keyboard_VolUp = sVals And bpgVolume.Enabled Then bpgVolume.Value += ((bpgVolume.Maximum - bpgVolume.Minimum) / 50)
        If My.Settings.Keyboard_Webpage = sVals And mnuWebpage.Enabled Then mnuWebpage_Click(New Object, New EventArgs)
        If Vals.Contains(iKey) Then Vals.Remove(CStr(iKey))
    End Select
    Return MyBase.ProcessKeyPreview(m)
  End Function

  Private Function KeyToStr(ByRef Key As Keys) As String
    Select Case Key
      Case Keys.A : Return "A"
      Case Keys.Add : Return "Add"
      Case Keys.Apps : Return "App Menu"
      Case Keys.Attn : Return "Attn"
      Case Keys.B : Return "B"
      Case Keys.Back : Return "Backspace"
      Case Keys.BrowserBack : Return "Browser Back"
      Case Keys.BrowserFavorites : Return "Browser Favorites"
      Case Keys.BrowserForward : Return "Browser Forward"
      Case Keys.BrowserHome : Return "Browser Home"
      Case Keys.BrowserRefresh : Return "Browser Refresh"
      Case Keys.BrowserSearch : Return "Browser Search"
      Case Keys.BrowserStop : Return "Browser Stop"
      Case Keys.C : Return "C"
      Case Keys.Cancel : Return "Cancel"
      Case Keys.CapsLock : Return "CapsLock"
      Case Keys.Clear : Return "Clear"
      Case Keys.Crsel : Return "CrSel"
      Case Keys.ControlKey : Return "Ctrl"
      Case Keys.D : Return "D"
      Case Keys.D0 : Return "0"
      Case Keys.D1 : Return "1"
      Case Keys.D2 : Return "2"
      Case Keys.D3 : Return "3"
      Case Keys.D4 : Return "4"
      Case Keys.D5 : Return "5"
      Case Keys.D6 : Return "6"
      Case Keys.D7 : Return "7"
      Case Keys.D8 : Return "8"
      Case Keys.D9 : Return "9"
      Case Keys.Decimal : Return "Decimal"
      Case Keys.Delete : Return "Delete"
      Case Keys.Divide : Return "Divide"
      Case Keys.Down : Return "Down Arrow"
      Case Keys.E : Return "E"
      Case Keys.End : Return "End"
      Case Keys.Enter : Return "Enter"
      Case Keys.EraseEof : Return "EraseEOF"
      Case Keys.Escape : Return "Escape"
      Case Keys.Execute : Return "Execute"
      Case Keys.Exsel : Return "ExSel"
      Case Keys.F : Return "F"
      Case Keys.F1 : Return "F1"
      Case Keys.F2 : Return "F2"
      Case Keys.F3 : Return "F3"
      Case Keys.F4 : Return "F4"
      Case Keys.F5 : Return "F5"
      Case Keys.F6 : Return "F6"
      Case Keys.F7 : Return "F7"
      Case Keys.F8 : Return "F8"
      Case Keys.F9 : Return "F9"
      Case Keys.F10 : Return "F10"
      Case Keys.F11 : Return "F11"
      Case Keys.F12 : Return "F12"
      Case Keys.F13 : Return "F13"
      Case Keys.F14 : Return "F14"
      Case Keys.F15 : Return "F15"
      Case Keys.F16 : Return "F16"
      Case Keys.F17 : Return "F17"
      Case Keys.F18 : Return "F18"
      Case Keys.F19 : Return "F19"
      Case Keys.F20 : Return "F20"
      Case Keys.F21 : Return "F21"
      Case Keys.F22 : Return "F22"
      Case Keys.F23 : Return "F23"
      Case Keys.F24 : Return "F24"
      Case Keys.FinalMode : Return "IME Final Mode"
      Case Keys.G : Return "G"
      Case Keys.H : Return "H"
      Case Keys.HangulMode : Return "IME Hangul Mode"
      Case Keys.HanjaMode : Return "IME Hanja Mode"
      Case Keys.Help : Return "Help"
      Case Keys.Home : Return "Home"
      Case Keys.I : Return "I"
      Case Keys.IMEAccept : Return "IME Accept"
      Case Keys.IMEConvert : Return "IME Convert"
      Case Keys.IMEModeChange : Return "IME Mode Change"
      Case Keys.IMENonconvert : Return "IME Nonconvert"
      Case Keys.Insert : Return "Insert"
      Case Keys.J : Return "J"
      Case Keys.JunjaMode : Return "IME Junja Mode"
      Case Keys.K : Return "K"
      Case Keys.KanaMode : Return "IME Kana Mode"
      Case Keys.KanjiMode : Return "IME Kanji Mode"
      Case Keys.L : Return "L"
      Case Keys.LaunchApplication1 : Return "Launch App1"
      Case Keys.LaunchApplication2 : Return "Launch App2"
      Case Keys.LaunchMail : Return "Launch Mail"
      Case Keys.LControlKey : Return "Left Ctrl"
      Case Keys.Left : Return "Left Arrow"
      Case Keys.LineFeed : Return "Line Feed"
      Case Keys.LMenu : Return "Left Alt"
      Case Keys.LShiftKey : Return "Left Shift"
      Case Keys.LWin : Return "Left Win"
      Case Keys.M : Return "M"
      Case Keys.MediaNextTrack : Return "Media Next Track"
      Case Keys.MediaPlayPause : Return "Media Play/Pause"
      Case Keys.MediaPreviousTrack : Return "Media Previous Track"
      Case Keys.MediaStop : Return "Media Stop"
      Case Keys.Menu : Return "Alt"
      Case Keys.Multiply : Return "Multiply"
      Case Keys.N : Return "N"
      Case Keys.NumLock : Return "NumLock"
      Case Keys.NumPad0 : Return "NumPad 0"
      Case Keys.NumPad1 : Return "NumPad 1"
      Case Keys.NumPad2 : Return "NumPad 2"
      Case Keys.NumPad3 : Return "NumPad 3"
      Case Keys.NumPad4 : Return "NumPad 4"
      Case Keys.NumPad5 : Return "NumPad 5"
      Case Keys.NumPad6 : Return "NumPad 6"
      Case Keys.NumPad7 : Return "NumPad 7"
      Case Keys.NumPad8 : Return "NumPad 8"
      Case Keys.NumPad9 : Return "NumPad 9"
      Case Keys.O : Return "O"
      Case Keys.Oem8 : Return "OEM 8"
      Case Keys.OemBackslash : Return "Backslash"
      Case Keys.OemClear : Return "OEM Clear"
      Case Keys.OemCloseBrackets : Return "Close Brackets"
      Case Keys.Oemcomma : Return "Comma"
      Case Keys.OemMinus : Return "Minus"
      Case Keys.OemOpenBrackets : Return "Open Brackets"
      Case Keys.OemPeriod : Return "Period"
      Case Keys.OemPipe : Return "Pipe"
      Case Keys.Oemplus : Return "Plus"
      Case Keys.OemQuestion : Return "Question"
      Case Keys.OemQuotes : Return "Quotes"
      Case Keys.OemSemicolon : Return "Semicolon"
      Case Keys.Oemtilde : Return "Tilde"
      Case Keys.P : Return "P"
      Case Keys.Pa1 : Return "Pa1"
      Case Keys.PageDown : Return "Page Down"
      Case Keys.PageUp : Return "Page Up"
      Case Keys.Pause : Return "Pause"
      Case Keys.Play : Return "Play"
      Case Keys.Print : Return "Print"
      Case Keys.PrintScreen : Return "Print Screen"
      Case Keys.ProcessKey : Return "Process"
      Case Keys.Q : Return "Q"
      Case Keys.R : Return "R"
      Case Keys.RControlKey : Return "Right Ctrl"
      Case Keys.Right : Return "Right Arrow"
      Case Keys.RMenu : Return "Right Alt"
      Case Keys.RShiftKey : Return "Right Shift"
      Case Keys.RWin : Return "Right Win"
      Case Keys.S : Return "S"
      Case Keys.Scroll : Return "ScrollLock"
      Case Keys.Select : Return "Select"
      Case Keys.SelectMedia : Return "Select Media"
      Case Keys.Separator : Return "Seperator"
      Case Keys.ShiftKey : Return "Shift"
      Case Keys.Sleep : Return "Sleep"
      Case Keys.Space : Return "Spacebar"
      Case Keys.Subtract : Return "Subtract"
      Case Keys.T : Return "T"
      Case Keys.Tab : Return "Tab"
      Case Keys.U : Return "U"
      Case Keys.Up : Return "Up Arrow"
      Case Keys.V : Return "V"
      Case Keys.VolumeDown : Return "Volume Down"
      Case Keys.VolumeMute : Return "Volume Mute"
      Case Keys.VolumeUp : Return "Volume Up"
      Case Keys.W : Return "W"
      Case Keys.X : Return "X"
      Case Keys.Y : Return "Y"
      Case Keys.Z : Return "Z"
      Case Keys.Zoom : Return "Zoom"
      Case Else : Return Nothing
    End Select
  End Function

  Private Sub JoyRun(SearchFor As String)
    If Not My.Settings.Gamepad Then Exit Sub
    If bDVD And mpPlayer.GetDVDCurrentDomain = DirectShowLib.Dvd.DvdDomain.VideoTitleSetMenu Then
      If SearchFor = "X Axis Left" Then
        mpPlayer.DVDSelectButton(DirectShowLib.Dvd.DvdRelativeButton.Left)
        Exit Sub
      ElseIf SearchFor = "X Axis Right" Then
        mpPlayer.DVDSelectButton(DirectShowLib.Dvd.DvdRelativeButton.Right)
        Exit Sub
      ElseIf SearchFor = "Y Axis Top" Then
        mpPlayer.DVDSelectButton(DirectShowLib.Dvd.DvdRelativeButton.Upper)
        Exit Sub
      ElseIf SearchFor = "Y Axis Bottom" Then
        mpPlayer.DVDSelectButton(DirectShowLib.Dvd.DvdRelativeButton.Lower)
        Exit Sub
      ElseIf SearchFor = My.Settings.Gamepad_PlayPause Then
        mpPlayer.DVDActivateButton()
        Exit Sub
      End If
    End If
    If tbsView.SelectedTab Is tabPlayList Then
      If My.Settings.Gamepad_AddToPL = SearchFor And cmdAddToPL.Enabled Then cmdAddToPL_Click(New Object, New EventArgs)
      If My.Settings.Gamepad_ClearPL = SearchFor And cmdClearPL.Enabled Then cmdClearPL_Click(New Object, New EventArgs)
      If My.Settings.Gamepad_OpenPL = SearchFor And cmdOpenPL.Enabled Then cmdOpenPL_Click(New Object, New EventArgs)
      If My.Settings.Gamepad_RemoveFromPL = SearchFor And cmdRemoveFromPL.Enabled Then cmdRemoveFromPL_Click(New Object, New EventArgs)
      If My.Settings.Gamepad_RenamePL = SearchFor And txtPlayListTitle.Enabled Then txtPlayListTitle_Click(New Object, New EventArgs)
      If My.Settings.Gamepad_RepeatPL = SearchFor And cmdLoopPL.Enabled Then cmdLoopPL_Click(New Object, New EventArgs)
      If My.Settings.Gamepad_SavePL = SearchFor And cmdSavePL.Enabled Then cmdSavePL_Click(New Object, New EventArgs)
    End If
    If My.Settings.Gamepad_About = SearchFor And mnuAbout.Enabled Then mnuAbout_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_Close = SearchFor And mnuCloseFile.Enabled Then mnuCloseFile_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_DiscEject = SearchFor And cmdLoop.Enabled And (bCD Or bDVD) Then cmdLoop_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_DVDMenu = SearchFor And cmdMenu.Enabled Then cmdMenu_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_FS = SearchFor And cmdFullScreen.Enabled Then cmdFullScreen_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_Last = SearchFor And cmdBackPL.Enabled Then cmdBackPL_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_Mute = SearchFor And cmdMute.Enabled Then cmdMute_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_Next = SearchFor And cmdNextPL.Enabled Then cmdNextPL_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_Open = SearchFor And mnuOpenFile.Enabled Then mnuOpenFile_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_PlayPause = SearchFor And cmdPlayPause.Enabled Then cmdPlayPause_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_Props = SearchFor And mnuProperties.Enabled Then mnuProperties_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_RepeatTrack = SearchFor And cmdLoop.Enabled Then cmdLoop_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_Settings = SearchFor And mnuSettings.Enabled Then mnuSettings_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_Shuffle = SearchFor And cmdShufflePL.Enabled Then cmdShufflePL_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_SkipBack = SearchFor And pbProgress.Enabled Then If mpPlayer.Position > 3 Then mpPlayer.Position -= 3
    If My.Settings.Gamepad_SkipFwd = SearchFor And pbProgress.Enabled Then If mpPlayer.Position < mpPlayer.Duration - 3 Then mpPlayer.Position += 3
    If My.Settings.Gamepad_Stop = SearchFor And cmdStop.Enabled Then cmdStop_Click(New Object, New EventArgs)
    If My.Settings.Gamepad_VolDown = SearchFor And bpgVolume.Enabled Then bpgVolume.Value -= ((bpgVolume.Maximum - bpgVolume.Minimum) / 50)
    If My.Settings.Gamepad_VolUp = SearchFor And bpgVolume.Enabled Then bpgVolume.Value += ((bpgVolume.Maximum - bpgVolume.Minimum) / 50)
    If My.Settings.Gamepad_Webpage = SearchFor And mnuWebpage.Enabled Then mnuWebpage_Click(New Object, New EventArgs)
  End Sub

  Private Sub joyPad_ButtonDown(Button As Integer) Handles joyPad.ButtonDown
    JoyRun("Button " & Button)
  End Sub

  Private Sub joyPad_POVSet(Degree As Integer) Handles joyPad.POVSet
    JoyRun("POV " & Degree & "deg")
  End Sub

  Private Sub joyPad_RAxisLeft() Handles joyPad.RAxisLeft
    JoyRun("R Axis Left")
  End Sub

  Private Sub joyPad_RAxisRight() Handles joyPad.RAxisRight
    JoyRun("R Axis Right")
  End Sub

  Private Sub joyPad_UChange(Value As Object) Handles joyPad.UChange
    JoyRun("U Axis " & Value)
  End Sub

  Private Sub joyPad_VChange(Value As Object) Handles joyPad.VChange
    JoyRun("V Axis " & Value)
  End Sub

  Private Sub joyPad_XAxisLeft() Handles joyPad.XAxisLeft
    JoyRun("X Axis Left")
  End Sub

  Private Sub joyPad_XAxisRight() Handles joyPad.XAxisRight
    JoyRun("X Axis Right")
  End Sub

  Private Sub joyPad_YAxisBottom() Handles joyPad.YAxisBottom
    JoyRun("Y Axis Bottom")
  End Sub

  Private Sub joyPad_YAxisTop() Handles joyPad.YAxisTop
    JoyRun("Y Axis Top")
  End Sub

  Private Sub joyPad_ZAxisBottom() Handles joyPad.ZAxisBottom
    JoyRun("Z Axis Bottom")
  End Sub

  Private Sub joyPad_ZAxisTop() Handles joyPad.ZAxisTop
    JoyRun("Z Axis Top")
  End Sub

#End Region
#End Region

#Region "Album Art"
#Region "GUI"
  Private Sub artList_Cancelled() Handles artList.Cancelled
    artList.Visible = False
    pctAlbumArt.Visible = True
    Dim lastHeight As Integer = Me.Height
    Me.Size = artList.Tag
    'Me.Size = New Drawing.Size(Me.MinimumSize.Width, Me.MinimumSize.Height + tbsView.GetTabRect(0).Height + 4)
    Me.Top += (lastHeight - Me.Height)
    If pctAlbumArt.Tag IsNot Nothing Then
      FileArt = pctAlbumArt.Tag
      pctAlbumArt.Tag = Nothing
    End If
  End Sub

  Private Sub artList_NewArt(row As Generic.Dictionary(Of String, Object)) Handles artList.NewArt
    pctAlbumArt.Tag = Nothing
    artList.Visible = False
    macArt.ChooseRow(row)
    pctAlbumArt.Visible = True
  End Sub

  Private Sub pctAlbumArt_DoubleClick(sender As Object, e As System.EventArgs) Handles pctAlbumArt.DoubleClick
    If cmdFullScreen.Enabled Then
      cmdFullScreen_Click(cmdFullScreen, New EventArgs)
    Else
      mnuArtFind_Click(mnuArtFind, New EventArgs)
    End If
  End Sub
#End Region

#Region "Net"
  Private Sub macArt_Choices(sender As Object, e As AppleNet.ChoicesEventArgs) Handles macArt.Choices
    If Me.InvokeRequired Then
      Me.Invoke(New EventHandler(Of AppleNet.ChoicesEventArgs)(AddressOf macArt_Choices), sender, e)
    Else
      Dim Artist, Album As String
      Artist = String.Empty
      Album = String.Empty
      If bCD Then
        Artist = dgvPlayList.Rows(GetSelectedPlayListItem).Tag(2)
        Album = dgvPlayList.Rows(GetSelectedPlayListItem).Tag(3)
      Else
        If IO.Path.GetExtension(mpPlayer.FileName).ToLower = ".mp3" Then
          Using ID3v2Tags As New Seed.clsID3v2(mpPlayer.FileName)
            If ID3v2Tags.HasID3v2Tag Then
              If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TP1")) Then Artist = ID3v2Tags.FindFrame("TP1")
              If Not String.IsNullOrWhiteSpace(ID3v2Tags.FindFrame("TAL")) Then Album = ID3v2Tags.FindFrame("TAL")
            End If
          End Using
          Using ID3v1Tags As New Seed.clsID3v1(mpPlayer.FileName)
            If ID3v1Tags.HasID3v1Tag Then
              If String.IsNullOrEmpty(Artist) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Artist) Then Artist = ID3v1Tags.Artist
              If String.IsNullOrEmpty(Album) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Album) Then Album = ID3v1Tags.Album
            End If
          End Using
        ElseIf IO.Path.GetExtension(mpPlayer.FileName).ToLower = ".ogg" Or IO.Path.GetExtension(mpPlayer.FileName).ToLower = ".ogm" Or IO.Path.GetExtension(mpPlayer.FileName).ToLower = ".flac" Then
          Using cVorbis As New Seed.clsVorbis(mpPlayer.FileName)
            If cVorbis.HasVorbis Then
              If Not String.IsNullOrEmpty(cVorbis.Album) Then Album = cVorbis.Album
              If Not String.IsNullOrEmpty(cVorbis.Artist) Then Artist = cVorbis.Artist
            End If
          End Using
        Else
          Debug.Print("Need to gather info for " & IO.Path.GetExtension(mpPlayer.FileName) & " types.")
        End If
      End If

      If pctAlbumArt.Tag Is Nothing OrElse (Not CType(pctAlbumArt.Tag, ImageWithName).FileName.ToLower.EndsWith("folder.jpg")) Then
        For Each ret In e.Rows
          If StrEquiv(ret("artistName"), Artist) And StrEquiv(ret("collectionName"), Album) Then
            If Not ret("artworkUrl100") Is Nothing Then
              macArt.ChooseRow(ret)
              Exit Sub
            End If
          End If
        Next
      End If
      artList.Tag = Me.Size
      If tbsView.SelectedTab Is tabArt Then
        Dim lastHeight As Integer = Me.Height
        Me.Size = New Drawing.Size(600, 450)
        Me.Top += (lastHeight - Me.Height)
      End If
      artList.Visible = True
      pctAlbumArt.Visible = False
      artList.Display(Artist, Album, e.Rows)
    End If

  End Sub


  Private Sub macArt_Complete(sender As Object, e As AppleNet.CompleteEventArgs) Handles macArt.Complete
    If Me.InvokeRequired Then
      Me.Invoke(New EventHandler(Of AppleNet.CompleteEventArgs)(AddressOf macArt_Complete), sender, e)
    Else
      Dim sArt As String = IO.Path.GetDirectoryName(mpPlayer.FileName) & "\Folder.jpg"
      FileArt = New ImageWithName(Drawing.Image.FromStream(New IO.MemoryStream(e.Cover)), sArt)
      If cTask IsNot Nothing Then cTask.CreatePreview(FileArt.Art)
      If Not My.Computer.FileSystem.FileExists(sArt) Then
        Try
          FileArt.Art.Save(sArt, Drawing.Imaging.ImageFormat.Jpeg)
          IO.File.SetAttributes(sArt, IO.FileAttributes.Hidden)
        Catch ex As Exception
          Debug.Print("Unable to save: " & ex.Message)
        End Try
      Else
        Dim oldArt = Drawing.Image.FromFile(sArt)
        If oldArt.Width * oldArt.Height < FileArt.Art.Width * FileArt.Art.Height Then
          Try
            oldArt.Dispose()
            My.Computer.FileSystem.DeleteFile(sArt)
            FileArt.Art.Save(sArt, Drawing.Imaging.ImageFormat.Jpeg)
            IO.File.SetAttributes(sArt, IO.FileAttributes.Hidden)
          Catch ex As Exception
            Debug.Print("Unable to save: " & ex.Message)
          End Try
        Else
          Debug.Print("Larger Folder.jpg already exists.")
        End If
      End If
      If Me.WindowState = FormWindowState.Normal And tbsView.SelectedTab Is tabArt Then tbsView_SelectedIndexChanged(New Object, New EventArgs)
    End If
  End Sub

  Private Sub macArt_Failed(sender As Object, e As AppleNet.FailEventArgs) Handles macArt.Failed
    If Me.InvokeRequired Then
      Me.Invoke(New EventHandler(Of AppleNet.FailEventArgs)(AddressOf macArt_Failed), sender, e)
    Else
      Debug.Print("Art Failed: " & e.Error.Message)
      FileArt = New ImageWithName(My.Resources.Logo, "LOGO")
    End If
  End Sub
#End Region

  Private Property FileArt As ImageWithName
    Get
      Return mFArt
    End Get
    Set(value As ImageWithName)
      mFArt = value
      If mnuArtShow.Checked Then
        If mFArt IsNot Nothing Then
          If mFArt.FileName = "LOGO" Then
            If Not pctAlbumArt.BackgroundImageLayout = ImageLayout.Center Then pctAlbumArt.BackgroundImageLayout = ImageLayout.Center
          Else
            If Not pctAlbumArt.BackgroundImageLayout = ImageLayout.Zoom Then pctAlbumArt.BackgroundImageLayout = ImageLayout.Zoom
          End If
          If Not bgArt.FileName = mFArt.FileName Then
            pctAlbumArt.BackgroundImage = mFArt.Art
            bgArt = mFArt
          End If
        Else
          If Not pctAlbumArt.BackgroundImageLayout = ImageLayout.Center Then pctAlbumArt.BackgroundImageLayout = ImageLayout.Center
          If pctAlbumArt.BackgroundImage IsNot Nothing Then
            If Not bgArt.FileName = "LOGO" Then
              pctAlbumArt.BackgroundImage = My.Resources.Logo
              bgArt = New ImageWithName(My.Resources.Logo, "LOGO")
            End If
          Else
            pctAlbumArt.BackgroundImage = My.Resources.Logo
            bgArt = New ImageWithName(My.Resources.Logo, "LOGO")
          End If
        End If
      End If
    End Set
  End Property

  Private Sub getAlbumInfo_Artwork(Picture As System.Drawing.Image) Handles getAlbumInfo.Artwork
    FileArt = New ImageWithName(Picture.Clone, "CDImage")
    If cTask IsNot Nothing Then cTask.CreatePreview(FileArt.Art)
    'If Me.WindowState = FormWindowState.Normal And tbsView.SelectedTab Is tabArt Then tbsView_SelectedIndexChanged(New Object, New EventArgs)
  End Sub

  Private Sub getAlbumInfo_Info(Artist As String, Album As String, Tracks() As AlbumInfo.TrackInfo, Genre As String) Handles getAlbumInfo.Info
    For I As Integer = 0 To Tracks.Length - 1
      Dim indx As Integer = I
      Dim dgvX = (From item As DataGridViewRow In dgvPlayList.Rows Where TrackToNo(item.Tag(0)) = indx + 1).FirstOrDefault
      If dgvX IsNot Nothing Then
        Dim _Path As String = dgvX.Tag(0)
        Dim _Title As String = Tracks(I).Title
        Dim _Artist As String = Tracks(I).Artist
        Dim _Album As String = Album
        Dim _Genre As String = Genre
        Dim _Length As String = dgvX.Tag(5)
        dgvX.Cells(0).Value = _Title
        dgvX.Tag = {_Path, _Title, _Artist, _Album, _Genre, _Length}
        Dim sTooltip As String = String.Empty
        sTooltip = "Title: " & _Title
        If Not String.IsNullOrEmpty(_Artist) AndAlso _Artist <> UNKNOWN_ARTIST Then sTooltip &= vbNewLine & "Artist: " & _Artist
        If Not String.IsNullOrEmpty(_Album) AndAlso _Album <> UNKNOWN_ALBUM Then sTooltip &= vbNewLine & "Album: " & _Album
        If Not String.IsNullOrEmpty(_Genre) AndAlso _Genre <> "Other" Then sTooltip &= vbNewLine & "Genre: " & _Genre
        If Not String.IsNullOrEmpty(_Length) AndAlso _Length <> "--:--" Then sTooltip &= vbNewLine & "Length: " & _Length
        dgvX.Cells(0).ToolTipText = sTooltip
        If dgvX.Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText Then
          SetTitleArtist(_Title, _Artist)
          FileTitle = _Artist & " - " & _Title
        End If
      End If

    Next
    txtPlayListTitle.Tag = Album
  End Sub
#End Region

  Private Sub pctQuality_MouseDoubleClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pctQuality.MouseDoubleClick
    If e.Button = Windows.Forms.MouseButtons.Left Then mpPlayer.ShowProperties("ffdshow Video Decoder", 0)
  End Sub

  Private Sub pctBitrate_MouseDoubleClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pctBitrate.MouseDoubleClick
    If e.Button = Windows.Forms.MouseButtons.Left Then
      mpPlayer.ShowProperties("Default DirectSound Device", 0)
    ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
      mpPlayer.ShowProperties("Video Renderer", 0)
    End If
  End Sub

  Private Sub pctChannels_MouseDoubleClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pctChannels.MouseDoubleClick
    If e.Button = Windows.Forms.MouseButtons.Left Then mpPlayer.ShowProperties("ffdshow Audio Decoder", 0)
  End Sub

  Private Sub mnuTransferFile_Click(sender As System.Object, e As System.EventArgs) Handles mnuTransferFile.Click
    If frmTransfer.ShowDialog(Me) = Windows.Forms.DialogResult.Yes Then
      ClearPlayList()
      SaveTempPL()
      Me.Close()
    End If
  End Sub


End Class