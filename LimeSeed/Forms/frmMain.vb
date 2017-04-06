Imports System.Runtime.InteropServices

Public Class frmMain
#Region "Properties"
  Friend instanceID As Integer = 0
  Friend Property SelectedPlayListItem As Integer
    Get
      If bCD Then Return cCD.CurrentTrack
      Return PLItems.GetSelected
    End Get
    Set(value As Integer)
      If bCD Then
        If value = -1 Then
          'TODO: stop cd i guess?
        Else
          cCD.CurrentTrack = value
        End If
      Else
        PLItems.SelectTrack(value)
      End If
    End Set
  End Property
  Friend WithEvents PLItems As New PlayListItems
#End Region
#Region "Constants"
  Private Const UNKNOWN_ALBUM As String = "Unknown Album"
  Private Const UNKNOWN_ARTIST As String = "Unknown Artist"
  Private Const LOADING_ALBUM As String = "Loading Album..."
  Private Const LOADING_ARTIST As String = "Loading Artist..."
  Private Const MAIN_HEIGHT As Integer = 108
  Private Const MAIN_WIDTH As Integer = 320
  Private Const TOPMOST_MENU_ID As Int64 = &H4815
  Private Const TOPMOST_MENU_TEXT As String = "&Topmost"
  Private Const HIDE_MENU_ID As Int64 = &H4816
  Private Const HIDE_MENU_TEXT As String = "&Hide in Tray"
  Private Const BIGWAIT As Integer = 128
  Private Const SMALLWAIT As Integer = 8
#End Region
#Region "Private Variables"

  Private VolS, SeekS, VidThumb, SeekPlay As Boolean
  Private AudWidth, VidWidth, ChapterWidth, SubWidth As Single
  Private sourceRowIndex As Integer = -1

  Private mFArt As ImageWithName
  Private bgArt As ImageWithName
  Private VidSize As Drawing.Size
  Private CorrectedSize As Drawing.Size
  Private ffAPI As FFDShowAPI.FFDShowAPI = Nothing
  Private taskBar As TaskbarLib.TaskbarList
  Private frmFS As frmFullScreen
  Private frmTI As frmTray

  Private taskCancel As Threading.CancellationTokenSource
  Private taskContext As Threading.Tasks.TaskScheduler

  Private Delegate Sub CallBack(Obj As Object)
  Private FirstInit As Boolean = False
  Private volDevice As CoreAudioApi.MMDevice
  Private volControl As New Sound

  Private audDeviceName As String

  Private videoSize_Width As Integer
  Private videoSize_Height As Integer

  Private SpecificChapterList As List(Of ChapterListing)
  Private objDraw As Object
  Private methodDraw As System.Reflection.MethodInfo
  Private NoGlassText As Boolean = True
  Private iEveryHalfMinuteTimer As Integer
#Region "Media"
  Private FileTitle As String
  Private FileMainTitle As String
  Private FileSubTitle As String
#Region "CD"
  Private cCD As Seed.clsAudioCD
  Private lastCDSubTitle As String
  Private bCD As Boolean
  Private bDVD As Boolean
#End Region
#End Region
#Region "PlayList"
  Private bLoadingPlayList As Boolean
  'Private taskPlayList As Threading.Tasks.TaskFactory(Of PlayListItemResponse)
  Private bDefaultedPlayListTitle As Boolean
  Private pLastPlayListCursor As Drawing.Point
  Private iLastPlayListScroll As Integer
  Private sLastQueuedTrack As String
#End Region
#Region "Glass"
  Private iGlassLastX As Integer
  Private bGlassLastLeft As Boolean
#End Region
#End Region
#Region "Private Controls"
  Private WithEvents cTask As TaskbarController
  Private WithEvents macArt As AppleNet
  Private WithEvents joyPad As clsJoyDetection
  Private WithEvents getAlbumInfo As AlbumInfo

#End Region
#Region "Form Events"

#End Region

#Region "Form Actions"

#End Region
#Region "Media Events"

#End Region
#Region "Media Functions"

  Private Sub Open_Single(sPath As String)
    If String.IsNullOrEmpty(sPath) Then Return
    If sPath.Substring(1) = ":\" Then
      Dim sDisc As String = sPath.Substring(0, 3)
      Try
        Dim dDisc As New IO.DriveInfo(sDisc)
        If Not dDisc.IsReady Then
          MsgBox("Unable to read a disc in drive """ & sDisc & """.", MsgBoxStyle.Critical, My.Application.Info.Title)
          Return
        End If
        Select Case dDisc.DriveFormat
          Case "CDFS" : OpenCD(sPath)
          Case "UDF" : OpenDVD(sPath)
          Case Else
            MsgBox("Unable to open the disc in drive """ & sDisc & """. Unknown Format: " & dDisc.DriveFormat, MsgBoxStyle.Critical, My.Application.Info.Title)
            Return
        End Select
      Catch ex As Exception
        MsgBox("Unable to open the disc in drive """ & sDisc & """." & vbNewLine & ex.Message, MsgBoxStyle.Critical, My.Application.Info.Title)
        Return
      End Try
    ElseIf IO.Directory.Exists(sPath) Then
      If sPath.EndsWith("VIDEO_TS") Then
        OpenDVD(sPath)
        Return
      End If
      If PLItems.Count = 0 Then
        txtPlayListTitle.Tag = IO.Path.GetFileName(sPath)
        bDefaultedPlayListTitle = True
        AddDirToPlayListAndMaybePlay(sPath)
        Return
      End If
      AddDirToPlayList(sPath)
    Else
      Select Case IO.Path.GetExtension(sPath).ToLower
        Case ".llpl", ".m3u", ".m3u8", ".pls"
          cmdShufflePL.Tag = False
          cmdShufflePL.Image = My.Resources.pl_button_order
          If mnuPLDuplicates.Checked Then mnuPLDuplicates.Checked = False
          PLItems.Clear()
          OpenPlayList(sPath, True)
        Case Else
          mpPlayer.SetNoQueue()
          OpenFile(sPath, True)
          ThreadedInitial()
      End Select
    End If
  End Sub

  Private Sub Open_Multiple(sPaths As String())
    For Each sFile As String In GetAllCompletePathLists(sPaths)
      Dim sExt As String = IO.Path.GetExtension(sFile).ToLower
      Select Case sExt
        Case ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".dib", ".ini", ".db"
          Continue For
        Case Else
          If Not sFile.ToLower.StartsWith("http://") AndAlso Not IO.File.Exists(sFile) Then
            PLShowMissing(sFile)
            Continue For
          End If
          If mnuPLDuplicates.Checked AndAlso PLItems.PlayListIndicies(sFile).Length > 0 Then
            PLShowMissing(IO.Path.GetFileName(sFile), "File Already Exists")
          Else
            PLItems.Add(New PlayListItem(sFile))
          End If
          If dgvPlayList.Rows.Count > 0 And (mpPlayer.State = Seed.ctlSeed.MediaState.mClosed Or mpPlayer.State = Seed.ctlSeed.MediaState.mPaused Or mpPlayer.State = Seed.ctlSeed.MediaState.mStopped) Then
            dgvPlayList.Rows(0).Selected = True
            StartPlayList()
          End If
      End Select
    Next
  End Sub

#End Region
#Region "PlayList Functions"
  Private Class PlayListItemResponse
    Public Item As PlayListItem
    Public Fail As Boolean
    Public FailMessage As String
    Public Sub New(itm As PlayListItem)
      Item = itm
      Fail = False
      FailMessage = Nothing
    End Sub
    Public Sub New(Optional Failure As String = Nothing)
      Item = Nothing
      Fail = True
      FailMessage = Failure
    End Sub
  End Class
  Private Function PlayList_ParseItem(item As PlayListItem) As PlayListItemResponse
    If Not Me.InvokeRequired Then Debug.Print("Parsing is NOT on MAIN THREAD")
    Dim _Path As String = item.Path
    Dim _Title As String = String.Empty
    Dim _Artist As String = String.Empty
    Dim _Album As String = String.Empty
    Dim _Genre As String = String.Empty
    Dim _Duration As Double = 0
    Dim _Length As String = String.Empty
    Dim _grabDuration As Boolean = True
    If _Path.ToLower.StartsWith("http://") Then
      _Title = item.Title
      _Length = "--:--"
      _grabDuration = False
    ElseIf IO.Path.GetExtension(_Path).ToLower = ".mp3" Then
      Using ID3v2Tags As New Seed.clsID3v2(_Path)
        If ID3v2Tags.HasID3v2Tag Then
          If ID3v2Tags.FindFrame("TP1") Then
            _Artist = ID3v2Tags.FindFrameMatchString("TP1")
            If ID3v2Tags.FindFrame("TP2") Then
              Dim sBand As String = ID3v2Tags.FindFrameMatchString("TP2")
              Dim sOut As String = Nothing
              If MatchNames_Minimal(_Artist, sBand, sOut) Then
                _Artist = sOut
              Else
                _Artist = sBand & " (" & _Artist & ")"
              End If
            End If
          ElseIf ID3v2Tags.FindFrame("TP2") Then
            _Artist = ID3v2Tags.FindFrameMatchString("TP2")
          End If
          If ID3v2Tags.FindFrame("TT2") Then
            _Title = ID3v2Tags.FindFrameMatchString("TT2")
            If ID3v2Tags.FindFrame("TT3") Then _Title &= " (" & ID3v2Tags.FindFrameMatchString("TT3") & ")"
          End If
          If ID3v2Tags.FindFrame("TAL") Then _Album = ID3v2Tags.FindFrameMatchString("TAL")
          If ID3v2Tags.FindFrame("TCO") Then _Genre = ID3v2Tags.FindFrameMatchString("TCO")
        End If
      End Using
      Using ID3v1Tags As New Seed.clsID3v1(_Path)
        If ID3v1Tags.HasID3v1Tag Then
          If String.IsNullOrEmpty(_Title) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Title) Then _Title = ID3v1Tags.Title
          If String.IsNullOrEmpty(_Artist) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Artist) Then _Artist = ID3v1Tags.Artist
          If String.IsNullOrEmpty(_Album) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Album) Then _Album = ID3v1Tags.Album
          If String.IsNullOrEmpty(_Genre) AndAlso Not ID3v1Tags.Genre = &HFF Then _Genre = Seed.clsID3v1.GenreName(ID3v1Tags.Genre)
        End If
      End Using
    ElseIf IO.Path.GetExtension(_Path).ToLower = ".ogg" Or IO.Path.GetExtension(_Path).ToLower = ".ogm" Or IO.Path.GetExtension(_Path).ToLower = ".flac" Then
      Using cVorbis As New Seed.clsVorbis(_Path)
        If cVorbis.HasVorbis Then
          If Not String.IsNullOrEmpty(cVorbis.Title) Then _Title = cVorbis.Title
          If Not String.IsNullOrEmpty(cVorbis.Artist) Then _Artist = cVorbis.Artist
          If Not String.IsNullOrEmpty(cVorbis.Album) Then _Album = cVorbis.Album
          If Not String.IsNullOrEmpty(cVorbis.Genre) Then _Genre = cVorbis.Genre
        End If
      End Using
    ElseIf IO.Path.GetExtension(_Path).ToLower = ".mkv" Then
      Dim cMKV As New Seed.clsMKVHeaders(_Path)
      If String.IsNullOrEmpty(_Title) AndAlso Not String.IsNullOrEmpty(cMKV.SegmentInfo.Title) Then _Title = cMKV.SegmentInfo.Title
      If String.IsNullOrEmpty(_Title) Then
        For Each track In cMKV.TrackEntries
          If track.TrackType = 1 Then
            If Not String.IsNullOrEmpty(track.TrackName) Then _Title = track.TrackName
            Exit For
          End If
        Next
      End If
    ElseIf IO.Path.GetExtension(_Path).ToLower = ".cda" Or _Path.Substring(1, 7) = ":\Track" Then
      Dim tNo As Integer = TrackToNo(_Path)
      Using cDrive As New LimeSeed.CDDrive
        cDrive.Open(_Path(0))
        If cDrive.IsCDReady And cDrive.Refresh And cDrive.GetNumTracks > 0 Then
          _Duration = cDrive.TrackSize(tNo) / 176400
          _Length = ConvertTimeVal(_Duration)
        End If
      End Using
      _grabDuration = False
    ElseIf IO.Path.GetExtension(_Path).ToLower = ".jpg" Or IO.Path.GetExtension(_Path).ToLower = ".jpeg" Or IO.Path.GetExtension(_Path).ToLower = ".bmp" Or IO.Path.GetExtension(_Path).ToLower = ".gif" Or IO.Path.GetExtension(_Path).ToLower = ".png" Or IO.Path.GetExtension(_Path).ToLower = ".db" Or IO.Path.GetExtension(_Path).ToLower = ".ini" Then
      item.Title = "DELETE"
      Return New PlayListItemResponse(item)
    End If
    If String.IsNullOrEmpty(_Title) Then _Title = IO.Path.GetFileNameWithoutExtension(_Path)
    If String.IsNullOrEmpty(_Artist) Then _Artist = UNKNOWN_ARTIST
    If String.IsNullOrEmpty(_Album) Then _Album = UNKNOWN_ALBUM
    If _grabDuration Then
      If IO.Path.GetExtension(_Path).ToLower = ".swf" Or IO.Path.GetExtension(_Path).ToLower = ".spl" Then
        Dim dDur As Double = mpPlayer.GetFileDuration(_Path)
        _Duration = dDur
        _Length = dDur & " frames"
      Else
        Using mpDuration As New Seed.ctlSeed
          Dim dDur As Double = mpDuration.GetFileDuration(_Path)
          If dDur <= 0 Then
            item.Title = "DELETE"
            Return New PlayListItemResponse()
          End If
          _Duration = dDur
          _Length = ConvertTimeVal(dDur)
        End Using
      End If
    End If
    If Not String.IsNullOrEmpty(_Title) Then item.Title = _Title
    If Not String.IsNullOrEmpty(_Artist) Then item.Artist = _Artist
    If Not String.IsNullOrEmpty(_Album) Then item.Album = _Album
    If Not String.IsNullOrEmpty(_Genre) Then item.Genre = _Genre
    If Not _Duration = 0 Then item.Duration = _Duration
    Return New PlayListItemResponse(item)
  End Function
  Private Function PlayList_ItemParsed(task As Threading.Tasks.Task(Of PlayListItemResponse)) As PlayListItemResponse
    If Me.Disposing Or Me.IsDisposed Then Return Nothing
    If task.IsCanceled Then Return Nothing
    If task.Result Is Nothing Then Return Nothing
    If task.Result.Fail Then
      If String.IsNullOrEmpty(task.Result.FailMessage) Then
        PLShowMissing(task.AsyncState)
      Else
        PLShowMissing(task.AsyncState, task.Result.FailMessage)
      End If
      Return Nothing
    End If
    Dim changeRows() As Integer = PLItems.PlayListIndicies(task.Result.Item.Path)
    If changeRows.Count = 0 Then Return Nothing
    Dim isFirst As Boolean = changeRows.Contains(0)
    If Not isFirst AndAlso dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader Then isFirst = True
    Dim isLast As Boolean = True  'changeRows.Contains(PLItems.Count - 1)
    For I As Integer = 0 To dgvPlayList.RowCount - 1
      If dgvPlayList.Rows(I).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText Then
        isLast = False
        Exit For
      End If
    Next
    SyncLock dgvPlayList
      If isFirst Then
        If dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader Then
          Dim col1W As Integer = dgvPlayList.Columns(1).Width
          dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
          dgvPlayList.Columns(1).Width = col1W
        End If
      End If
      For Each I In changeRows
        Dim dgvX As DataGridViewRow = dgvPlayList.Rows(I)
        If dgvX Is Nothing Then Continue For
        If task.Result.Item.Title = "DELETE" Then
          PLShowMissing(task.Result.Item.Path)
          PLItems.Remove(I)
        Else
          If Not dgvX.Cells(0).Value = task.Result.Item.Title Then dgvX.Cells(0).Value = task.Result.Item.Title
          If Not dgvX.Cells(1).Value = task.Result.Item.Length Then dgvX.Cells(1).Value = task.Result.Item.Length
          If dgvX.Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText Then dgvX.Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
          PLItems.PlayListItem(I).Title = task.Result.Item.Title
          PLItems.PlayListItem(I).Artist = task.Result.Item.Artist
          PLItems.PlayListItem(I).Album = task.Result.Item.Album
          PLItems.PlayListItem(I).Genre = task.Result.Item.Genre
          PLItems.PlayListItem(I).Duration = task.Result.Item.Duration
          Dim sTT As String = PLItems.PlayListItem(I).ToolTipText(False)
          If Not dgvX.Cells(0).ToolTipText = sTT Then dgvX.Cells(0).ToolTipText = sTT
          If Not dgvX.Cells(1).ToolTipText = sTT Then dgvX.Cells(1).ToolTipText = sTT
        End If
      Next
      If isLast AndAlso Not dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader Then dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader
    End SyncLock
    Return Nothing
  End Function
#End Region
#Region "Helper Functions"

  Private Function GetAllCompletePathLists(Paths As String()) As String()
    Dim sPaths As New List(Of String)
    For Each sPath As String In Paths
      If IO.Directory.Exists(sPath) Then
        sPaths.AddRange(GetCompletePathList(sPath))
      Else
        sPaths.Add(sPath)
      End If
    Next
    Return sPaths.ToArray
  End Function
  Private Function GetCompletePathList(Path As String) As String()
    Dim sPaths As New List(Of String)
    For Each sFile As String In IO.Directory.GetFiles(Path)
      sPaths.Add(sFile)
    Next
    For Each sDir As String In IO.Directory.GetDirectories(Path)
      sPaths.AddRange(GetCompletePathList(sDir))
    Next
    Return sPaths.ToArray
  End Function
#End Region
#Region "Helper Classes"
  Friend Class PlayListItem
    Public Path As String
    Public Title As String
    Public Artist As String
    Public Album As String
    Public Genre As String
    Public Duration As Double
    Public OrderIndex As Integer
    Public DisplayIndex As Integer
    Public ReadOnly Property Length As String
      Get
        Return ConvertTimeVal(Duration)
      End Get
    End Property
    Public ReadOnly Property ToolTipText(Loading As Boolean) As String
      Get
        Dim sTooltip As String = "Title: " & Title
        If Loading Then
          If Not String.IsNullOrEmpty(Artist) Then sTooltip &= vbNewLine & "Artist: " & Artist
          If Not String.IsNullOrEmpty(Album) Then sTooltip &= vbNewLine & "Album: " & Album
          If Not String.IsNullOrEmpty(Genre) AndAlso (Not Genre = "Other") Then sTooltip &= vbNewLine & "Genre: " & Genre
          If Not String.IsNullOrEmpty(Length) Then
            If Length = "--:--" Then
              sTooltip &= vbNewLine & "Length: " & Length
            Else
              sTooltip &= vbNewLine & "Length: ~" & Length
            End If
          End If
        Else
          If Not String.IsNullOrEmpty(Artist) AndAlso (Not Artist = LOADING_ARTIST) Then sTooltip &= vbNewLine & "Artist: " & Artist
          If Not String.IsNullOrEmpty(Album) AndAlso (Not Album = LOADING_ALBUM) Then sTooltip &= vbNewLine & "Album: " & Album
          If Not String.IsNullOrEmpty(Genre) AndAlso (Not Genre = "Other") Then sTooltip &= vbNewLine & "Genre: " & Genre
          If Not String.IsNullOrEmpty(Length) AndAlso Not Length = "--:--" Then sTooltip &= vbNewLine & "Length: " & Length
        End If
        Return sTooltip
      End Get
    End Property
    Public Sub New(sPath As String)
      Path = sPath
      Title = Nothing
      Artist = LOADING_ARTIST
      Album = LOADING_ALBUM
      Genre = Nothing
      Duration = 0
      OrderIndex = -1
      DisplayIndex = -1
    End Sub
    Public Sub New(sPath As String, dDuration As Double)
      Path = sPath
      Title = Nothing
      Artist = LOADING_ARTIST
      Album = LOADING_ALBUM
      Genre = Nothing
      Duration = dDuration
      OrderIndex = -1
      DisplayIndex = -1
    End Sub
    Public Sub New(sPath As String, sTitle As String, dDuration As Double)
      Path = sPath
      Title = sTitle
      Artist = LOADING_ARTIST
      Album = LOADING_ALBUM
      Genre = Nothing
      Duration = dDuration
      OrderIndex = -1
      DisplayIndex = -1
    End Sub
  End Class
  Friend Class PlayListItems
    Private mPL As List(Of PlayListItem)
    Private mSelPath As String
    Private mSelIndex As Integer = -1
    Public Class PLUpdateEventArgs
      Inherits EventArgs
      Public Enum ChangeTypes
        Added
        Selected
        NewIndex
        Updated
        Focused
        Removed
        Cleared
      End Enum
      Public ChangeType As ChangeTypes
      Public SourceIndex As Integer
      Public DestIndex As Integer
      Public Sub New(chg As ChangeTypes, src As Integer, Optional dest As Integer = -1)
        ChangeType = chg
        SourceIndex = src
        DestIndex = dest
      End Sub
    End Class
    Event PlayListUpdate(sender As Object, e As PLUpdateEventArgs)
    Public Sub New()
      mPL = New List(Of PlayListItem)
    End Sub
    Public Sub Add(Item As PlayListItem)
      Dim newDisp As Integer = Item.DisplayIndex
      Dim newOrd As Integer = Item.OrderIndex
      If newDisp > -1 And newOrd > -1 Then
        For I As Integer = 0 To mPL.Count - 1
          If newDisp > -1 AndAlso mPL(I).DisplayIndex >= newDisp Then
            Dim oldIndex As Integer = mPL(I).DisplayIndex
            Dim newIndex As Integer = mPL(I).DisplayIndex + 1
            mPL(I).DisplayIndex = newIndex
            RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.NewIndex, oldIndex, newIndex))
          End If
          If newOrd > -1 AndAlso mPL(I).OrderIndex >= newOrd Then mPL(I).OrderIndex += 1
        Next
      End If
      If newDisp = -1 Then newDisp = mPL.Count
      If newOrd = -1 Then newOrd = mPL.Count
      Item.DisplayIndex = newDisp
      Item.OrderIndex = newOrd
      mPL.Add(Item)
      RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Added, newDisp))
    End Sub
    Public Sub Remove(DisplayIndex As Integer)
      If DisplayIndex < 0 Then
        Return
      End If
      Dim remOrd As Integer = -1
      For I As Integer = 0 To mPL.Count - 1
        If mPL(I).DisplayIndex = DisplayIndex Then
          remOrd = mPL(I).OrderIndex
          If mPL(I).Path = mSelPath Then SelectTrack(-1)
          mPL.RemoveAt(I)
          Exit For
        End If
      Next
      If remOrd = -1 Then
        Return
      End If
      Dim newSel As Integer = -1
      Dim changeLow As Integer = mPL.Count
      Dim changeHigh As Integer = 0
      For I As Integer = 0 To mPL.Count - 1
        If mPL(I).OrderIndex > remOrd Then mPL(I).OrderIndex -= 1
        If mPL(I).DisplayIndex > DisplayIndex Then
          Dim oldIndex As Integer = mPL(I).DisplayIndex
          Dim newIndex As Integer = mPL(I).DisplayIndex - 1
          If oldIndex < changeLow Then changeLow = oldIndex
          If oldIndex > changeHigh Then changeHigh = oldIndex
          If newIndex < changeLow Then changeLow = newIndex
          If newIndex > changeHigh Then changeHigh = newIndex
          mPL(I).DisplayIndex = newIndex
          If mPL(I).Path = mSelPath Then newSel = newIndex
        End If
      Next
      If changeHigh >= mPL.Count Then changeHigh = mPL.Count - 1
      RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Removed, DisplayIndex))
      If changeLow = 0 And changeHigh = mPL.Count - 1 Then
        RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Updated, -1))
      Else
        RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Updated, changeLow, changeHigh))
      End If
      If newSel > -1 Then SelectTrack(newSel)
    End Sub
    Public Sub Clear()
      Dim RemIndexList(mPL.Count - 1) As Integer
      For I As Integer = 0 To mPL.Count - 1
        RemIndexList(I) = mPL(I).DisplayIndex
      Next
      mPL.Clear()
      RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Cleared, -1))
    End Sub
    Public Sub ClearDuplicates()
      Dim remList As New List(Of Integer)
      For I As Integer = 0 To mPL.Count - 2
        For J As Integer = mPL.Count - 1 To I + 1 Step -1
          If mPL(I).Path = mPL(J).Path Then
            If Not remList.Contains(J) Then remList.Add(J)
          End If
        Next
      Next
      If remList.Count = 0 Then Return
      remList.Sort()
      For I As Integer = 0 To remList.Count - 1
        Remove(remList(I))
      Next
    End Sub
    Public Sub ChangeDisplayIndex(OldIndex As Integer, NewIndex As Integer)
      If OldIndex = NewIndex Then Return
      Dim changeList As New SortedList(Of Integer, Integer())
      If OldIndex > NewIndex Then
        For I As Integer = 0 To mPL.Count - 1
          If mPL(I).DisplayIndex >= NewIndex And mPL(I).DisplayIndex < OldIndex Then
            changeList.Add(I, {mPL(I).DisplayIndex, mPL(I).DisplayIndex + 1})
          End If
        Next
      ElseIf OldIndex < NewIndex Then
        For I As Integer = 0 To mPL.Count - 1
          If mPL(I).DisplayIndex > OldIndex And mPL(I).DisplayIndex <= NewIndex Then
            changeList.Add(I, {mPL(I).DisplayIndex, mPL(I).DisplayIndex - 1})
          End If
        Next
      End If
      For I As Integer = 0 To mPL.Count - 1
        If mPL(I).DisplayIndex = OldIndex Then
          changeList.Add(I, {OldIndex, NewIndex})
          Exit For
        End If
      Next
      If changeList.Count > 0 Then
        For Each changeI As Integer In changeList.Keys
          Dim changeNew As Integer = changeList(changeI)(1)
          mPL(changeI).DisplayIndex = changeNew
        Next
        For Each changeI As Integer In changeList.Keys
          Dim changeOld As Integer = changeList(changeI)(0)
          Dim changeNew As Integer = changeList(changeI)(1)
          RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.NewIndex, changeOld, changeNew))
          If mPL(changeI).Path = mSelPath Then SelectTrack(changeNew)
        Next
      End If
    End Sub
    Public Sub ChangeIndex(OldIndex As Integer, NewIndex As Integer)
      If OldIndex = NewIndex Then Return
      Dim changeList As New SortedList(Of Integer, Integer())
      If OldIndex > NewIndex Then
        For I As Integer = 0 To mPL.Count - 1
          If mPL(I).OrderIndex >= NewIndex And mPL(I).OrderIndex < OldIndex Then
            changeList.Add(I, {mPL(I).OrderIndex, mPL(I).OrderIndex + 1})
          End If
        Next
      ElseIf OldIndex < NewIndex Then
        For I As Integer = 0 To mPL.Count - 1
          If mPL(I).OrderIndex > OldIndex And mPL(I).OrderIndex <= NewIndex Then
            changeList.Add(I, {mPL(I).OrderIndex, mPL(I).OrderIndex - 1})
          End If
        Next
      End If
      For I As Integer = 0 To mPL.Count - 1
        If mPL(I).OrderIndex = OldIndex Then
          changeList.Add(I, {OldIndex, NewIndex})
          Exit For
        End If
      Next
      If changeList.Count > 0 Then
        For Each changeI As Integer In changeList.Keys
          Dim changeNew As Integer = changeList(changeI)(1)
          mPL(changeI).OrderIndex = changeNew
        Next
      End If
    End Sub
    Public Sub Shuffle(SelOnTop As Boolean)
      SyncLock mPL
        Dim newOrder(mPL.Count - 1) As Integer
        Dim newSel As Integer = -1
        For I As Integer = 0 To newOrder.Length - 1
          newOrder(I) = -1
        Next
        Dim rnd As New Random
        If Not String.IsNullOrEmpty(mSelPath) Then
          If SelOnTop Then
            For I As Integer = 0 To newOrder.Length - 1
              If mPL(I).Path = mSelPath Then
                newOrder(I) = 0
                Exit For
              End If
            Next
          End If
        End If
        For I As Integer = 0 To newOrder.Length - 1
          If Not String.IsNullOrEmpty(mSelPath) AndAlso mPL(I).Path = mSelPath Then Continue For
          Dim xRND As Integer = -1
          Do While xRND = -1
            xRND = rnd.Next(0, newOrder.Length)
            For N As Integer = 0 To newOrder.Length - 1
              If newOrder(N) = xRND Then
                xRND = -1
                Exit For
              End If
            Next
          Loop
          newOrder(I) = xRND
        Next
        For I As Integer = 0 To mPL.Count - 1
          Dim oldIndex As Integer = mPL(I).DisplayIndex
          Dim newIndex As Integer = newOrder(I)
          mPL(I).DisplayIndex = newIndex
          If Not String.IsNullOrEmpty(mSelPath) Then
            If mPL(I).Path = mSelPath Then
              If SelOnTop Then newSel = newIndex
            End If
          End If
        Next
        Dim dIDXs(mPL.Count - 1) As Boolean
        For I As Integer = 0 To mPL.Count - 1
          For J As Integer = 0 To mPL.Count - 1
            If mPL(I).DisplayIndex = J Then
              If dIDXs(J) Then
                Debug.Print("TWICE")
                Stop
              Else
                dIDXs(J) = True
              End If
            End If
          Next
        Next
        RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Updated, -1))
        SelectTrack(newSel)
      End SyncLock
    End Sub
    Public Sub UnShuffle()
      SyncLock mPL
        Dim newSel As Integer = -1
        For I As Integer = 0 To mPL.Count - 1
          mPL(I).DisplayIndex = mPL(I).OrderIndex
          If mPL(I).Path = mSelPath Then newSel = mPL(I).OrderIndex
        Next
        RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Updated, -1))
        If newSel > -1 Then SelectTrack(newSel)
      End SyncLock

    End Sub
    Public ReadOnly Property PlayListItem(Index As Integer) As PlayListItem
      Get
        For I As Integer = 0 To mPL.Count - 1
          If mPL(I).DisplayIndex = Index Then Return mPL(I)
        Next
        Return Nothing
      End Get
    End Property
    Public ReadOnly Property PlayListItemOrdered(Index As Integer) As PlayListItem
      Get
        For I As Integer = 0 To mPL.Count - 1
          If mPL(I).OrderIndex = Index Then Return mPL(I)
        Next
        Return Nothing
      End Get
    End Property
    Public ReadOnly Property PlayListIndicies(Path As String) As Integer()
      Get
        Dim changeRows As New List(Of Integer)
        For I As Integer = 0 To mPL.Count - 1
          Dim rowData As PlayListItem = mPL(I)
          If rowData Is Nothing Then Continue For
          If Not rowData.Path = Path Then Continue For
          changeRows.Add(rowData.DisplayIndex)
        Next
        Return changeRows.ToArray
      End Get
    End Property
    Public ReadOnly Property PlayListIndiciesOrdered(Path As String) As Integer()
      Get
        Dim changeRows As New List(Of Integer)
        For I As Integer = 0 To mPL.Count - 1
          Dim rowData As PlayListItem = mPL(I)
          If rowData Is Nothing Then Continue For
          If Not rowData.Path = Path Then Continue For
          changeRows.Add(rowData.OrderIndex)
        Next
        Return changeRows.ToArray
      End Get
    End Property
    'Public Sub SelectTrack(sPath As String)
    '  Dim mOldIndex As Integer = mSelIndex
    '  For I As Integer = 0 To mPL.Count - 1
    '    If mPL(I).Path = sPath Then
    '      mSelPath = sPath
    '      mSelIndex = mPL(I).DisplayIndex
    '      RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Selected, mOldIndex, mPL(I).DisplayIndex))
    '      Return
    '    End If
    '  Next
    'End Sub
    Public Sub SelectTrack(DisplayIndex As Integer)
      Dim mOldIndex As Integer = mSelIndex
      If DisplayIndex = -1 Then
        mSelPath = Nothing
        mSelIndex = -1
        RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Selected, mOldIndex, -1))
        Return
      End If
      For I As Integer = 0 To mPL.Count - 1
        If mPL(I).DisplayIndex = DisplayIndex Then
          mSelPath = mPL(I).Path
          mSelIndex = mPL(I).DisplayIndex
          RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Selected, mOldIndex, mPL(I).DisplayIndex))
          Return
        End If
      Next
    End Sub
    Public Sub FindTrack(SearchTerms As String)
      If String.IsNullOrEmpty(SearchTerms) Then Return
      For I As Integer = 0 To mPL.Count - 1
        If mPL(I).Title.ToLower.Contains(SearchTerms.ToLower) Then
          RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Focused, mPL(I).DisplayIndex))
          Return
        End If
        If mPL(I).Artist.ToLower.Contains(SearchTerms.ToLower) Then
          RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Focused, mPL(I).DisplayIndex))
          Return
        End If
        If mPL(I).Album.ToLower.Contains(SearchTerms.ToLower) Then
          RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Focused, mPL(I).DisplayIndex))
          Return
        End If
      Next
      For I As Integer = 0 To mPL.Count - 1
        If mPL(I).Path.ToLower.Contains(SearchTerms.ToLower) Then
          RaiseEvent PlayListUpdate(Me, New PLUpdateEventArgs(PLUpdateEventArgs.ChangeTypes.Focused, mPL(I).DisplayIndex))
          Return
        End If
      Next
    End Sub
    Public ReadOnly Property GetSelected As Integer
      Get
        For I As Integer = 0 To mPL.Count - 1
          If mPL(I).Path = mSelPath Then Return mPL(I).DisplayIndex
        Next
        Return -1
      End Get
    End Property
    Public ReadOnly Property Count As Integer
      Get
        Return mPL.Count
      End Get
    End Property
  End Class

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

  Friend Class ChapterListing
    Public StartIndex As Double
    Public ChapterName As String
    Public ChapterLang As String
    Public Sub New(Index As Double, [Name] As String, Lang As String)
      StartIndex = Index
      ChapterName = [Name]
      ChapterLang = Lang
    End Sub
  End Class
#End Region

  Public Sub New()
    InitializeComponent()
    taskContext = Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext
    taskCancel = New Threading.CancellationTokenSource
    'taskPlayList = New Threading.Tasks.TaskFactory(Of PlayListItemResponse)(taskCancel.Token)
  End Sub

#Region "Main Controls"
#Region "Progress"
  Private Sub pbProgress_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pbProgress.MouseDown
    If (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left And Not SeekS Then
      pbProgress.Value = ((e.X - 1) / (pbProgress.Width - 2)) * pbProgress.Maximum
      If bCD Then
        cCD.MovePositionMS(pbProgress.Value)
        SeekS = True
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
    If SeekS And (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Then
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
    If SeekS And (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Then
      If e.X > 0 And e.X < pbProgress.Width Then
        pbProgress.Value = ((e.X - 1) / (pbProgress.Width - 2)) * pbProgress.Maximum
        If bCD Then
          cCD.MovePositionMS(pbProgress.Value)
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
  Friend Sub cmdPlayPause_Click(sender As System.Object, e As System.EventArgs) Handles cmdPlayPause.Click
    If bCD Then
      If cCD.Status = Seed.clsAudioCD.PlayStatus.Playing Then
        SetPlayPause(True, TriState.True)
        cCD.Pause()
      Else
        SetPlayPause(False, TriState.True)
        cCD.Play()
      End If
    ElseIf mpPlayer.IsStreaming Then
      Dim oldName As String = mpPlayer.StreamURL
      mpPlayer.SetNoQueue()
      mpPlayer.FileName = String.Empty
      Application.DoEvents()
      OpenFile(oldName, True)
    Else
      If mpPlayer.State = Seed.ctlSeed.MediaState.mPlaying Then
        SetPlayPause(True, TriState.True)
        mpPlayer.StateFade = True
        mpPlayer.mpPause()
      ElseIf mpPlayer.State = Seed.ctlSeed.MediaState.mPaused Or mpPlayer.State = Seed.ctlSeed.MediaState.mStopped Then
        SetPlayPause(False, TriState.True)
        mpPlayer.StateFade = True
        mpPlayer.mpPlay()
      End If
    End If
  End Sub

  Friend Sub cmdStop_Click(sender As System.Object, e As System.EventArgs) Handles cmdStop.Click
    If bCD Then
      SetPlayPause(True, TriState.True)
      cCD.Stop()
    Else
      mpPlayer.StateFade = True
      SetPlayPause(True, TriState.True)
      mpPlayer.mpStop()
    End If
  End Sub

  Private Sub cmdFullScreen_Click(sender As System.Object, e As System.EventArgs) Handles cmdFullScreen.Click
    If frmFS.Visible Then
      mpPlayer.FullScreen = False
      mpPlayer.FullScreenObj = Nothing
      Me.Opacity = 1
      frmFS.Hide()
      If cTask IsNot Nothing Then
        If mpPlayer.HasVid Then cTask.ShowClip(mpPlayer)
        VidThumb = True
      Else
        VidThumb = False
      End If
      SetCursor(True)

      If ffAPI IsNot Nothing Then
        If videoSize_Width = VidSize.Width And videoSize_Height = VidSize.Height Then
          ffAPI.DoResize = False
        Else
          If videoSize_Width Mod 2 = 1 Then videoSize_Width += 1
          If videoSize_Height Mod 2 = 1 Then videoSize_Height += 1
          ffAPI.DoResize = False
          ffAPI.ResizeModeFreeResize = True
          ffAPI.ResizeHorizontal = videoSize_Width
          ffAPI.ResizeVertical = videoSize_Height
          ffAPI.DoResize = True
          ffAPI.ResizeKeepAspectRatio = mnuRatioForce.Checked ' False
        End If
      End If
    Else
      If cTask IsNot Nothing Then
        If mpPlayer.HasVid Then
          cTask.ShowImage(My.Resources.Logo)
        End If
      End If
      frmFS.Show(Me)
      mpPlayer.FullScreenObj = frmFS.pctVideo
      mpPlayer.FullScreen = True
      Me.Opacity = 0
      VidThumb = False

      If ffAPI IsNot Nothing Then
        Dim useW As Integer = frmFS.Width
        Dim useH As Integer = frmFS.Height

        If (useW / useH) < (videoSize_Width / videoSize_Height) Then
          useH = (videoSize_Height / videoSize_Width) * useW
        ElseIf (useW / useH) > (videoSize_Width / videoSize_Height) Then
          useW = (videoSize_Width / videoSize_Height) * useH

        End If

        If useW = VidSize.Width And useH = VidSize.Height Then
          ffAPI.DoResize = False
        Else
          If useW Mod 2 = 1 Then useW += 1
          If useH Mod 2 = 1 Then useH += 1
          ffAPI.DoResize = False
          ffAPI.ResizeModeFreeResize = True
          ffAPI.ResizeHorizontal = useW
          ffAPI.ResizeVertical = useH

          ffAPI.DoResize = True
          ffAPI.ResizeKeepAspectRatio = mnuRatioForce.Checked ' False
        End If
      End If
    End If
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
      If mnuPLDuplicates.Checked Then mnuPLDuplicates.Checked = False
      'dgvPlayList.Rows.Clear()
      PLItems.Clear()
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
      If cmbChapters.Tag Is Nothing Then
        If SpecificChapterList.Count > 0 Then
          For Each Chapter In SpecificChapterList
            If StrComp(Chapter.ChapterName, cmbChapters.Text, CompareMethod.Text) = 0 Then
              mpPlayer.Position = Chapter.StartIndex
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
      Debug.Print("Video")
    End If
  End Sub

  Private Sub cmbAudTrack_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbAudTrack.SelectedIndexChanged
    If bDVD Then
      If cmbAudTrack.Tag Is Nothing Then mpPlayer.SetDVDCurrentAudioStream(cmbAudTrack.SelectedIndex)
    Else
      If LoadFFDShow() Then
        ffAPI.AudioStream = IIf(cmbVidTrack.Items.Count > 1, cmbVidTrack.Items.Count + 1, 1) + cmbAudTrack.SelectedIndex
        cmbAudTrack.SelectedIndex = ffAPI.AudioStream - IIf(cmbVidTrack.Items.Count > 1, cmbVidTrack.Items.Count + 1, 1)
        If cmbSubtitles.Items.Count > ffAPI.AudioStream - IIf(cmbVidTrack.Items.Count > 1, cmbVidTrack.Items.Count + 1, 1) Then cmbSubtitles.SelectedIndex = ffAPI.AudioStream - IIf(cmbVidTrack.Items.Count > 1, cmbVidTrack.Items.Count + 1, 1)
      End If
      If String.Compare(IO.Path.GetExtension(mpPlayer.FileName), ".mkv", True) = 0 Then
        Dim mkvHeader As New Seed.clsMKVHeaders(mpPlayer.FileName)
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
          Dim trackCounts(&HFF) As Integer
          For Each Track In mkvHeader.TrackEntries
            trackCounts(Track.TrackType) += 1
            Dim sTrackTitle As String
            If String.IsNullOrEmpty(Track.TrackName) Then
              sTrackTitle = "#" & trackCounts(Track.TrackType) & ": Untitled"
            Else
              sTrackTitle = "#" & trackCounts(Track.TrackType) & ": " & Track.TrackName
            End If
            If Not (String.IsNullOrEmpty(Track.Language) OrElse Track.Language = "und") And Not sTrackTitle.ToLower.Contains(Track.Language.ToLower) Then sTrackTitle &= " [" & Track.Language & "]"
            Select Case Track.TrackType
              Case &H1
                sVidInfo = MKVVideoCodecs(Track.CodecID)
                If Track.DefaultDuration > 0 Then sVidInfo &= " (" & Math.Round(1 / (Track.DefaultDuration / 1000000000), 3) & " fps " & IIf(Track.Video.FlagInterlaced, "interlaced", "progressive") & ")"
              Case &H2
                If cmbAudTrack.Text = sTrackTitle Then
                  Dim g As Drawing.Graphics = cmbAudTrack.CreateGraphics
                  Dim lWidth As Single = g.MeasureString(sTrackTitle, cmbAudTrack.Font).Width + 10
                  If lWidth > AudWidth Then AudWidth = lWidth
                  Dim iQ As Long = ((New IO.FileInfo(mpPlayer.FileName)).Length * 8) / dDuration
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
                End If
            End Select
          Next
          SetProps(iEncQ, iChannels, iqR, vbNewLine & "Video: " & sVidInfo & vbNewLine & "Audio: " & sAudInfo, iChannels, sBitrate)

          'For Each Track In mkvHeader.TrackEntries
          '  Dim sTrackTitle As String
          '  If String.IsNullOrEmpty(Track.TrackName) Then
          '    sTrackTitle = Track.Language
          '  Else
          '    sTrackTitle = Track.TrackName & " [" & Track.Language & "]"
          '  End If
          '  Select Case Track.TrackType
          '    Case &H2
          '      If cmbAudTrack.Text = (sTrackTitle) Then
          '        Dim iEncQ As Integer, sCodec As String = Nothing
          '        MKVAudioCodecs(Track.CodecID, iEncQ, sCodec)
          '        SetProps(iEncQ, Track.Audio.Channels, -2, sCodec, Track.Audio.Channels)
          '      End If
          '  End Select
          'Next
        End If

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
        If ffAPI.SubtitleStream > -1 Then cmbSubtitles.SelectedIndex = ffAPI.SubtitleStream - (IIf(cmbVidTrack.Items.Count > 1, cmbVidTrack.Items.Count + 1, 1) + IIf(cmbAudTrack.Items.Count > 1, cmbAudTrack.Items.Count, 1))
      End If
    End If
  End Sub
#End Region

#Region "Volume"
  Private Sub bpgVolume_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles bpgVolume.MouseDown
    Dim dVol As Double = ((e.X - 1) / (bpgVolume.Width - 2)) * bpgVolume.Maximum
    If (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left And Not VolS Then
      bpgVolume.Value = dVol
      VolS = True
    End If
  End Sub

  Private Sub bpgVolume_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles bpgVolume.MouseMove
    Dim dVol As Double = ((e.X - 1) / (bpgVolume.Width - 2)) * bpgVolume.Maximum
    If VolS And (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Then
      If e.X > 0 And e.X < bpgVolume.Width Then bpgVolume.Value = dVol
    End If
    If e.X > 0 And e.X < bpgVolume.Width Then
      Dim volClick As Integer = Math.Round(dVol) - bpgVolume.Maximum
      Dim sPercent As String = Nothing
      If Not VolS Then sPercent = "Set Volume: " & volClick & " dB"
      If ttDisp.GetToolTip(bpgVolume) <> sPercent Then ttDisp.SetToolTip(bpgVolume, sPercent)
    End If
  End Sub

  Private Sub bpgVolume_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles bpgVolume.MouseUp
    Dim dVol As Double = ((e.X - 1) / (bpgVolume.Width - 2)) * bpgVolume.Maximum
    If VolS And (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Then
      If e.X > 0 And e.X < bpgVolume.Width Then
        bpgVolume.Value = dVol
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
    Dim sResults As String() = Nothing
    Using fOpen As New frmOpen
      If Not fOpen.ShowDialog(IIf(frmFS.Visible, frmFS, Me)) = Windows.Forms.DialogResult.OK Then Return
      txtPlayListTitle.Tag = UNKNOWN_ALBUM
      bDefaultedPlayListTitle = True
      sResults = fOpen.sResult
    End Using
    If sResults Is Nothing Then Return
    If sResults.Length = 0 Then Return
    If sResults.Length = 1 Then
      Open_Single(sResults(0))
      Return
    End If
    Open_Multiple(sResults)
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
    If PLItems.Count = 1 Then
      ClearPlayList()
    Else
      PLItems.SelectTrack(-1)
    End If
    If ffAPI IsNot Nothing Then ffAPI.DoResize = False
    FirstInit = True
    ThreadedInitial()
  End Sub

  Private Sub mnuProperties_Click(sender As System.Object, e As System.EventArgs) Handles mnuProperties.Click
    If bCD Then
      MsgBox("CDs do not have properties.", MsgBoxStyle.Information, My.Application.Info.Title)
    ElseIf mpPlayer.IsStreaming Then
      MsgBox("Streams do not have properties.", MsgBoxStyle.Information, My.Application.Info.Title)
    Else
      For Each frm As Form In Application.OpenForms
        If frm.GetType Is GetType(frmProps) Then
          If CType(frm, frmProps).sFile = mpPlayer.FileName Then
            frm.Focus()
            Return
          End If
        End If
      Next
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
        devItem.Checked = (item.Name.Substring(13) = audDeviceName)
        AddHandler devItem.Click, AddressOf mnuOutItem_Click
        'devItem.ShortcutKeyDisplayString = "F" & fItem
        devItem.ShortcutKeys = 111 + fItem
        fItem += 1
      End If
    Next
    mnuOutDefault.Checked = String.IsNullOrEmpty(audDeviceName)
  End Sub

  Private Sub mnuOutDefault_Click(sender As System.Object, e As System.EventArgs) Handles mnuOutDefault.Click
    audDeviceName = Nothing
    My.Settings.Device = audDeviceName
    mpPlayer.AudioDevice = audDeviceName
    SetBeatDevice()
  End Sub

  Private Sub mnuOutItem_Click(sender As System.Object, e As System.EventArgs)
    Dim mSend As ToolStripRadioButtonMenuItem = sender
    audDeviceName = mSend.Tag
    mpPlayer.AudioDevice = audDeviceName
    My.Settings.Device = audDeviceName
    SetBeatDevice()
  End Sub

  Private Sub mnuSettings_Click(sender As System.Object, e As System.EventArgs) Handles mnuSettings.Click
    If frmSettings.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
      My.Settings.Gapless = frmSettings.numGapless.Value / 1000
      My.Settings.SingleInstance = frmSettings.chkSingleInstance.Checked
      If frmSettings.cmbAudioOutput.SelectedIndex = 0 Then
        audDeviceName = String.Empty
      Else
        audDeviceName = frmSettings.cmbAudioOutput.Text
      End If
      SetBeatDevice()
      My.Settings.DefaultLocale = frmSettings.cmbLocale.Text
      My.Settings.Subtitles = frmSettings.chkSubtitles.Checked
      My.Settings.Captions = frmSettings.chkCaptions.Checked
      My.Settings.Device = audDeviceName
      mpPlayer.QueueTime = My.Settings.Gapless
      mpPlayer.AudioDevice = audDeviceName

      My.Settings.ID3_Modify = frmSettings.chkTags.Checked
      My.Settings.ID3_Art = frmSettings.chkArt.Checked
      My.Settings.ID3_Clean = frmSettings.chkCleanup.Checked
      If frmSettings.chkID3v2.Checked Then
        My.Settings.ID3_Ver = frmSettings.cmbID3v2.SelectedIndex + 2
      Else
        My.Settings.ID3_Ver = 0
      End If

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
      If My.Settings.Gamepad Then
        If joyPad Is Nothing Then joyPad = New clsJoyDetection
      Else
        If joyPad IsNot Nothing Then joyPad = Nothing
      End If
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
    Dim rowData As PlayListItem = PLItems.PlayListItem(RowIndex)
    If rowData Is Nothing Then
      Beep()
      Return
    End If
    OpenFile(rowData.Path, True)
    SelectedPlayListItem = RowIndex
    InitialQueue(Nothing)
  End Sub

  Private Sub mnuPLQueue_Click(sender As System.Object, e As System.EventArgs) Handles mnuPLQueue.Click
    Dim RowIndex As Integer = mnuPL.Tag
    Dim rowData As PlayListItem = PLItems.PlayListItem(RowIndex)
    If rowData Is Nothing Then
      Beep()
      Return
    End If
    Dim selIndex As Integer = SelectedPlayListItem
    If selIndex < 0 Or Not mpPlayer.State = Seed.ctlSeed.MediaState.mPlaying Then
      If selIndex = RowIndex Then Return
      OpenFile(rowData.Path, True)
      SelectedPlayListItem = RowIndex
      InitialQueue(Nothing)
    Else
      If selIndex = RowIndex Then Return
      Dim setHeaders As Boolean = False
      If dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader Then
        setHeaders = True
        Dim col1W As Integer = dgvPlayList.Columns(1).Width
        dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        dgvPlayList.Columns(1).Width = col1W
      End If
      If selIndex > RowIndex Then
        PLItems.ChangeDisplayIndex(RowIndex, selIndex)
        If Not cmdShufflePL.Tag = True Then PLItems.ChangeIndex(RowIndex, selIndex)
        ThreadedQueue()
      ElseIf Not RowIndex = selIndex + 1 Then
        PLItems.ChangeDisplayIndex(RowIndex, selIndex + 1)
        If Not cmdShufflePL.Tag = True Then PLItems.ChangeIndex(RowIndex, selIndex + 1)
        ThreadedQueue()
      End If
      If setHeaders Then
        If Not dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader Then
          Debug.Print("Resetting Length Column because queued next item")
          dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader
        End If
      End If
    End If
  End Sub

  Private Sub mnuPLDelete_Click(sender As System.Object, e As System.EventArgs) Handles mnuPLDelete.Click
    Dim RowIndex As Integer = mnuPL.Tag
    RemoveFromPlayList(RowIndex)
  End Sub

  Private Sub mnuPLOpenFile_Click(sender As System.Object, e As System.EventArgs) Handles mnuPLOpenFile.Click
    Dim RowIndex As Integer = mnuPL.Tag
    Dim rowData As PlayListItem = PLItems.PlayListItem(RowIndex)
    If rowData Is Nothing Then
      Beep()
      Return
    End If
    If IO.File.Exists(rowData.Path) Then
      Process.Start("explorer", "/select,""" & rowData.Path & """")
    ElseIf IO.Directory.Exists(IO.Path.GetDirectoryName(rowData.Path)) Then
      Process.Start("explorer", """" & IO.Path.GetDirectoryName(rowData.Path) & """")
    End If
  End Sub

  Private Sub mnuPLProps_Click(sender As System.Object, e As System.EventArgs) Handles mnuPLProps.Click
    Dim RowIndex As Integer = mnuPL.Tag
    Dim rowData As PlayListItem = PLItems.PlayListItem(RowIndex)
    If rowData Is Nothing Then
      Beep()
      Return
    End If
    For Each frm As Form In Application.OpenForms
      If frm.GetType Is GetType(frmProps) Then
        If CType(frm, frmProps).sFile = rowData.Path Then
          frm.Focus()
          Return
        End If
      End If
    Next
    Dim frmProperties As New frmProps With {.Tag = Me}
    frmProperties.ShowData(rowData.Path)
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
    If FileArt Is Nothing OrElse FileArt.FileName = "UNKNOWNALBUM" Then
      FileArt = New ImageWithName(My.Resources.loadingartlarge, "LOADING")
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
      If frmFS IsNot Nothing Then
        frmFS.pctVideo.Image = Nothing
        If frmFS.Visible Then cmdFullScreen.PerformClick()
      End If
    End If
  End Sub

  Private Sub munAlbumArt_Opening(sender As System.Object, e As System.ComponentModel.CancelEventArgs) Handles mnuAlbumArt.Opening
    mnuArtShow.Checked = Not pctAlbumArt.BackgroundImage Is Nothing
    mnuArtVisShow.Checked = methodDraw IsNot Nothing
    mnuArtVisSelect.DropDownItems.Clear()
    If IO.Directory.Exists(IO.Path.Combine(Application.StartupPath, "Visualizations")) Then
      For Each file In IO.Directory.GetFiles(IO.Path.Combine(Application.StartupPath, "Visualizations"))
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
    'If dgvPlayList.RowCount > 0 Then
    If PLItems.Count > 0 Then
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
      bDefaultedPlayListTitle = False
      txtPlayListTitle.BackColor = Drawing.SystemColors.Control
      txtPlayListTitle.BorderStyle = BorderStyle.None
      txtPlayListTitle.ReadOnly = True
    End If
  End Sub
#End Region

#Region "List"
#Region "Mouse"

  Private Sub dgvPlayList_CellMouseClick(sender As Object, e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles dgvPlayList.CellMouseClick
    If (e.Button And Windows.Forms.MouseButtons.Right) = Windows.Forms.MouseButtons.Right Then
      mnuPL.Tag = e.RowIndex
      mnuPL.Show(MousePosition)
    End If
  End Sub

  Private Sub dgvPlayList_CellMouseDoubleClick(sender As Object, e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles dgvPlayList.CellMouseDoubleClick
    Dim rowData As PlayListItem = PLItems.PlayListItem(e.RowIndex)
    If rowData Is Nothing Then
      Beep()
      Return
    End If
    OpenFile(rowData.Path, True)
    SelectedPlayListItem = e.RowIndex
    InitialQueue(Nothing)
  End Sub

  Private Sub dgvPlayList_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles dgvPlayList.MouseDown
    sourceRowIndex = -1
    If (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left And dgvPlayList.SelectedRows.Count > 0 Then
      sourceRowIndex = dgvPlayList.HitTest(e.X, e.Y).RowIndex
      If sourceRowIndex >= 0 Then
        dgvPlayList.Rows(sourceRowIndex).Selected = True
        dgvPlayList.Refresh()
      End If
    End If
  End Sub

  Private Sub dgvPlayList_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles dgvPlayList.MouseMove
    If (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left And dgvPlayList.SelectedRows.Count > 0 And sourceRowIndex >= 0 Then
      If Math.Abs(pLastPlayListCursor.X - e.Location.X) > 3 Or Math.Abs(pLastPlayListCursor.Y - e.Location.Y) > 3 Then
        dgvPlayList.Rows(sourceRowIndex).Selected = True
        dgvPlayList.Refresh()
        dgvPlayList.DoDragDrop(dgvPlayList.Rows(sourceRowIndex), DragDropEffects.Move)
      End If
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
        'If dgvPlayList.SelectedRows.Count > 0 Then
        If PLItems.Count > 0 Then
          StartPlayList()
        Else
          cmdAddToPL_Click(New Object, New EventArgs)
        End If
        e.Handled = True
      Case My.Settings.Keyboard_RemoveFromPL
        'If dgvPlayList.SelectedRows.Count > 0 Then
        If PLItems.Count > 0 Then
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
    DragEnterEvent(sender, e, True)
  End Sub

  Private Sub dgvPlayList_DragOver(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles dgvPlayList.DragOver
    If sourceRowIndex >= 0 Then
      Dim ptRet As Drawing.Point = dgvPlayList.PointToClient(New Drawing.Point(e.X, e.Y))
      Dim TargetRowIndex As Integer = dgvPlayList.HitTest(ptRet.X, ptRet.Y).RowIndex
      If TargetRowIndex > -1 Then
        Dim scrollHeight As Integer = dgvPlayList.Rows(TargetRowIndex).Height / 2
        If ptRet.Y < scrollHeight Then
          Dim ScrollDist As Integer = 10 - (scrollHeight - ptRet.Y)
          If dgvPlayList.FirstDisplayedScrollingRowIndex > 0 Then
            If Environment.TickCount - iLastPlayListScroll > ScrollDist * 50 Then
              dgvPlayList.FirstDisplayedScrollingRowIndex -= 1
              iLastPlayListScroll = Environment.TickCount
            End If
          End If
        ElseIf ptRet.Y > (dgvPlayList.Height - 1) - scrollHeight Then
          Dim ScrollDist As Integer = 10 - (ptRet.Y - ((dgvPlayList.Height - 1) - scrollHeight))
          If dgvPlayList.FirstDisplayedScrollingRowIndex < dgvPlayList.RowCount - 1 Then
            If Environment.TickCount - iLastPlayListScroll > ScrollDist * 50 Then
              dgvPlayList.FirstDisplayedScrollingRowIndex += 1
              iLastPlayListScroll = Environment.TickCount
            End If
          End If
        End If
        If sourceRowIndex <> TargetRowIndex Then
          PLItems.ChangeDisplayIndex(sourceRowIndex, TargetRowIndex)
          If Not cmdShufflePL.Tag = True Then PLItems.ChangeIndex(sourceRowIndex, TargetRowIndex)
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
      DragOverEvent(sender, e, True)
    End If
  End Sub
#End Region

  Private Delegate Sub PLShowMissingInvoker(sMissingPath As String, sReason As String)
  Private Sub PLShowMissing(Optional sMissingPath As String = Nothing, Optional sReason As String = Nothing)
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New PLShowMissingInvoker(AddressOf PLShowMissing), sMissingPath, sReason)
      Catch ex As Exception
      End Try
      Return
    End If
    Threading.ThreadPool.QueueUserWorkItem(AddressOf PLShowMissing_Really, {sMissingPath, sReason})
  End Sub

  Private Sub PLShowMissing_Really(param As Object)
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New Threading.WaitCallback(AddressOf PLShowMissing_Really), param)
      Catch ex As Exception
      End Try
      Return
    End If
    Dim sMissingPath As String = param(0)
    Dim sReason As String = param(1)
    If String.IsNullOrEmpty(sMissingPath) And String.IsNullOrEmpty(sReason) Then Return
    If Not String.IsNullOrEmpty(lblPLAlert.Text) Then
      tmrHideAlert.Enabled = False
      Dim realHeight As Integer = pnlPlayList.RowStyles(1).Height
      pnlPlayList.RowStyles(1).Height = 75
      If String.IsNullOrEmpty(sMissingPath) Then
        lblPLAlert.Text &= vbNewLine & sReason
      Else
        If String.IsNullOrEmpty(sReason) Then
          lblPLAlert.Text &= vbNewLine & "Unable to add """ & sMissingPath & """ to the PlayList!"
        Else
          lblPLAlert.Text &= vbNewLine & "Unable to add """ & sMissingPath & """ to the PlayList: " & sReason
        End If
      End If
      Dim myHeight As Integer = lblPLAlert.Height
      pnlPlayList.RowStyles(1).Height = realHeight
      Do Until pnlPlayList.RowStyles(1).Height >= myHeight
        pnlPlayList.RowStyles(1).Height += 1
        Application.DoEvents()
      Loop
      tmrHideAlert.Enabled = True
    Else
      pnlPlayList.RowStyles(1).Height = 75
      If String.IsNullOrEmpty(sMissingPath) Then
        lblPLAlert.Text = sReason
      Else
        If String.IsNullOrEmpty(sReason) Then
          lblPLAlert.Text = "Unable to add """ & sMissingPath & """ to the PlayList!"
        Else
          lblPLAlert.Text = "Unable to add """ & sMissingPath & """ to the PlayList: " & sReason
        End If
      End If
      Dim myHeight As Integer = lblPLAlert.Height
      pnlPlayList.RowStyles(1).Height = 0
      Do Until pnlPlayList.RowStyles(1).Height >= myHeight
        pnlPlayList.RowStyles(1).Height += 1
        Application.DoEvents()
      Loop
      tmrHideAlert.Enabled = True
    End If
  End Sub
#End Region

#Region "Buttons"
  Friend Sub cmdBackPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdBackPL.Click
    If bDVD Then
      mpPlayer.DVDPrevious()
    ElseIf PLItems.Count > 0 Then
      If SelectedPlayListItem = -1 Then Return
      Dim SelItem As Integer = SelectedPlayListItem - 1
      If SelItem < 0 Then Return
      Dim rowData As PlayListItem = PLItems.PlayListItem(SelItem)
      If rowData Is Nothing Then
        Beep()
        Return
      End If
      OpenFile(rowData.Path, True)
      SelectedPlayListItem = SelItem
      InitialQueue(Nothing)
    End If
  End Sub

  Friend Sub cmdNextPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdNextPL.Click
    If bDVD Then
      mpPlayer.DVDNext()
    ElseIf PLItems.Count > 0 Then
      If SelectedPlayListItem = -1 Then Return
      Dim SelItem As Integer = SelectedPlayListItem + 1
      If SelItem > PLItems.Count - 1 Then Return
      Dim rowData As PlayListItem = PLItems.PlayListItem(SelItem)
      If rowData Is Nothing Then
        Beep()
        Return
      End If
      OpenFile(rowData.Path, True)
      SelectedPlayListItem = SelItem
      InitialQueue(Nothing)
    End If
  End Sub

  Private Sub cmdShufflePL_Click(sender As System.Object, e As System.EventArgs) Handles cmdShufflePL.Click
    If cmdShufflePL.Tag = True Then
      cmdShufflePL.Tag = False
      cmdShufflePL.Image = My.Resources.pl_button_order
      PLItems.UnShuffle()
    Else
      cmdShufflePL.Tag = True
      cmdShufflePL.Image = My.Resources.pl_button_shuffle
      PLItems.Shuffle(Not String.IsNullOrEmpty(mpPlayer.FileName))
    End If
    ThreadedQueue()
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

  Private Sub cmdFindPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdFindPL.Click
    If dgvPlayList.Rows.Count = 0 Then
      Beep()
      Return
    End If
    If My.Computer.Keyboard.ShiftKeyDown OrElse PLItems.GetSelected = -1 Then
      Dim sFind As String = InputBox("Enter your search term:", "Search PlayList")
      If String.IsNullOrEmpty(sFind) Then Return
      PLItems.FindTrack(sFind)
      Return
    End If
    Dim selItem As Integer = SelectedPlayListItem
    If selItem = -1 Then
      Beep()
      Return
    End If
    Dim visibleCells As Integer = Math.Floor(dgvPlayList.Height / dgvPlayList.RowTemplate.Height)
    If visibleCells < 0 Or dgvPlayList.Height = 1 Then visibleCells = 0
    If visibleCells > 0 Then
      Dim centerPos As Integer = selItem - (Math.Ceiling(visibleCells / 2) - 1)
      If centerPos > -1 Then
        dgvPlayList.FirstDisplayedScrollingRowIndex = centerPos
      Else
        dgvPlayList.FirstDisplayedScrollingRowIndex = selItem
      End If
    End If
  End Sub

  Private Sub cmdAddToPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdAddToPL.Click
    Dim cdlBrowse As New OpenFileDialog With {.Filter = "All Files|*.*", .Multiselect = True, .Title = "Add to PlayList..."}
    If cdlBrowse.ShowDialog() = Windows.Forms.DialogResult.OK Then
      Threading.ThreadPool.QueueUserWorkItem(AddressOf AddAndQueue, cdlBrowse.FileNames)
    End If
  End Sub

  Private Sub AddAndQueue(FileNames As Object)
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New Threading.WaitCallback(AddressOf AddAndQueue), FileNames)
      Catch ex As Exception
      End Try
      Return
    End If
    Dim sFiles() As String = FileNames
    For Each item In sFiles
      Select Case IO.Path.GetExtension(item).ToLower
        Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(item)
        Case Else : AddToPlayList(item, , , False)
      End Select
    Next
  End Sub

  Private Sub cmdRemoveFromPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdRemoveFromPL.Click
    If dgvPlayList.SelectedRows.Count > 0 Then
      RemoveFromPlayList(dgvPlayList.SelectedRows(0).Index)
    End If
  End Sub

  Private Sub cmdClearPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdClearPL.Click
    If MsgBox("Are you sure you want to clear the PlayList?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo, My.Application.Info.Title) = MsgBoxResult.Yes Then
      ClearPlayList()
    End If
  End Sub

  Private Sub ClearPlayList()
    taskCancel.Cancel()
    mpPlayer.SetNoQueue()
    cmdShufflePL.Tag = False
    cmdShufflePL.Image = My.Resources.pl_button_order
    If mnuPLDuplicates.Checked Then mnuPLDuplicates.Checked = False
    PLItems.Clear()
    txtPlayListTitle.Tag = UNKNOWN_ALBUM
    bDefaultedPlayListTitle = True
    SaveTempPL(False)
    taskCancel = New Threading.CancellationTokenSource
  End Sub

  Private Sub cmdOpenPL_Click(sender As System.Object, e As System.EventArgs) Handles cmdOpenPL.Click
    mnuOpenFile_Click(sender, e)
  End Sub

  Private Sub cmdSavePL_Click(sender As System.Object, e As System.EventArgs) Handles cmdSavePL.Click
    Dim cdlBrowse As New SaveFileDialog With {.Filter = "Lime Light PlayList|*.llpl|MP3 PlayList|*.m3u|UTF-8 Encoded MP3 PlayList|*.m3u8|PlayList File|*.pls|Any File Type|*.*", .FileName = txtPlayListTitle.Tag}
    If cdlBrowse.ShowDialog() = Windows.Forms.DialogResult.OK Then SavePlayList(cdlBrowse.FileName, False)
  End Sub
#End Region

#Region "Routines"
#Region "Open"
  Public Sub OpenPlayList(Path As String, Optional ByVal AutoPlay As Boolean = False)
    bLoadingPlayList = True
    txtPlayListTitle.Tag = UNKNOWN_ALBUM
    bDefaultedPlayListTitle = True
    Select Case IO.Path.GetExtension(Path).ToLower
      Case ".llpl" : OpenBinaryPL(Path)
      Case ".m3u" : OpenM3U(Path, False)
      Case ".m3u8" : OpenM3U(Path, True)
      Case ".pls" : OpenPLS(Path)
      Case Else
        MsgBox("Unknown file extension: " & IO.Path.GetExtension(Path).Substring(1).ToUpper & "!" & vbNewLine & "Attempting standard text, line delimited read.", MsgBoxStyle.Critical, My.Application.Info.Title)
        OpenTextPL(Path)
    End Select
    bLoadingPlayList = False
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
      bDefaultedPlayListTitle = False
      Dim tmpItems As Integer = bReader.ReadInt32
      For I As Integer = 1 To tmpItems
        tmpLen = bReader.ReadInt16
        Dim sPath As String = System.Text.Encoding.GetEncoding(LATIN_1).GetString(bReader.ReadBytes(tmpLen))
        tmpLen = bReader.ReadInt16
        Dim sTitle As String = System.Text.Encoding.GetEncoding(LATIN_1).GetString(bReader.ReadBytes(tmpLen))
        If sPath.Substring(1, 2) <> ":\" And sPath.Substring(0, 7) <> "http://" Then sPath = Path.Substring(0, Path.LastIndexOf("\") - 1) & sPath
        AddToPlayList(sPath, sTitle, , False)
        'If I Mod WAITEVERY = 0 Then Application.DoEvents()
      Next
      bReader.Close()
    ElseIf Ver = 2 Then
      Dim plTitle As String = bReader.ReadString
      Dim tmpItems As UInt64 = bReader.ReadUInt64
      txtPlayListTitle.Tag = plTitle
      bDefaultedPlayListTitle = False
      For I As UInt64 = 1 To tmpItems
        Dim sPath As String = bReader.ReadString
        Dim sTitle As String = bReader.ReadString
        If sPath.Substring(1, 2) <> ":\" And sPath.Substring(0, 7) <> "http://" Then sPath = Path.Substring(0, Path.LastIndexOf("\") - 1) & sPath
        AddToPlayList(sPath, sTitle, , False)
        'If I Mod WAITEVERY = 0 Then Application.DoEvents()
      Next
      bReader.Close()
    Else
      bReader.Close()
      OpenTextPL(Path)
    End If
  End Sub

  Private Sub OpenTextPL(Path As String)
    Dim sReader As New IO.StreamReader(Path, True)
    'Dim I As UInt64 = 0
    Do
      Dim sPath As String = sReader.ReadLine
      Dim sTitle As String = sReader.ReadLine
      If Not String.IsNullOrEmpty(sPath.Trim) AndAlso Asc(sPath) > 0 Then
        If sPath.Substring(1, 2) <> ":\" And sPath.Substring(0, 7) <> "http://" Then sPath = Path.Substring(0, Path.LastIndexOf("\") + 1) & sPath
        AddToPlayList(sPath, sTitle, , False)
        'If I Mod WAITEVERY = 0 Then Application.DoEvents()
        'I += 1
      End If
    Loop Until sReader.EndOfStream
    sReader.Close()
  End Sub

  Private Sub OpenM3U(Path As String, UTF8 As Boolean)
    Dim encoding As System.Text.Encoding
    If UTF8 Then
      encoding = System.Text.Encoding.UTF8
    Else
      encoding = System.Text.Encoding.GetEncoding(LATIN_1)
    End If
    Try
      Using sReader As New IO.StreamReader(Path, encoding)
        If sReader.EndOfStream Then
          PLShowMissing(Path, "PlayList is Empty!")
          Return
        End If
        Dim firstLine As String = sReader.ReadLine
        If String.IsNullOrEmpty(firstLine) Then
          If sReader.EndOfStream Then
            PLShowMissing(Path, "PlayList is Empty!")
            Return
          End If
          Do While String.IsNullOrEmpty(firstLine)
            firstLine = sReader.ReadLine
            If sReader.EndOfStream Then
              PLShowMissing(Path, "PlayList is Empty!")
              Return
            End If
          Loop
        Else
          If sReader.EndOfStream Then
            PLShowMissing(Path, "PlayList contains only the message """ & firstLine & """!")
            Return
          End If
        End If
        If firstLine = "#EXTM3U" Then
          'Dim I As UInt64 = 0
          Do
            Dim sINFO As String = sReader.ReadLine
            If sReader.EndOfStream Then
              PLShowMissing(, "PlayList contains the message """ & sINFO & """ at the end!")
              Exit Do
            End If
            Dim sPath As String = sReader.ReadLine
            If String.IsNullOrEmpty(sINFO) And String.IsNullOrEmpty(sPath) Then Continue Do
            Dim lTime As Double = 0
            Dim sTitle As String = Nothing
            If Not String.IsNullOrEmpty(sINFO) Then
              If String.IsNullOrEmpty(sPath) Then
                PLShowMissing(, "PlayList contains an empty path after """ & sINFO & """!")
                Exit Do
              End If
              If sINFO.Contains(",") Then
                lTime = Val(Split(sINFO, ",", 2)(0).Substring(sINFO.IndexOf(":") + 1))
                sTitle = Split(sINFO, ",", 2)(1)
              Else
                sTitle = sINFO
              End If
            End If
            If sPath.Substring(1, 2) <> ":\" And sPath.Substring(0, 7) <> "http://" Then sPath = Path.Substring(0, Path.LastIndexOf("\") + 1) & sPath
            If lTime = 0 Then
              If String.IsNullOrEmpty(sTitle) Then
                AddToPlayList(sPath, , , False)
              Else
                AddToPlayList(sPath, sTitle, , False)
              End If
            Else
              AddToPlayList(sPath, sTitle, lTime, False)
            End If
            'If I Mod WAITEVERY = 0 Then Application.DoEvents()
            'I += 1
          Loop Until sReader.EndOfStream
        Else
          sReader.BaseStream.Position = 0
          'Dim I As UInt64 = 0
          Do
            Dim sPath As String = sReader.ReadLine
            If String.IsNullOrEmpty(sPath) Then Continue Do
            If sPath.Substring(1, 2) <> ":\" And sPath.Substring(0, 7) <> "http://" Then sPath = Path.Substring(0, Path.LastIndexOf("\") + 1) & sPath
            AddToPlayList(sPath, , , False)
            'If I Mod WAITEVERY = 0 Then Application.DoEvents()
            'I += 1
          Loop Until sReader.EndOfStream
        End If
      End Using
    Catch
      PLShowMissing(Path, "PlayList is Corrupt!")
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
          AddToPlayList(sPath, sTitle, lTime, False)
          'If I Mod WAITEVERY = 0 Then Application.DoEvents()
        Next
      Else
        PLShowMissing(Path, "PLS v" & iniPLS.GetValue("playlist", "Version") & " is not recognized!")
      End If
      iniPLS = Nothing
    Catch
      PLShowMissing(Path, "PlayList is Corrupt!")
    End Try
  End Sub

  Private Sub LoadTempPL()
    Dim Loaded As Boolean = False
    If IO.File.Exists(IO.Path.GetTempPath & "seed" & instanceID & ".m3u8") Then
      Me.Size = New Drawing.Size(Me.MinimumSize.Width, Me.MinimumSize.Height + tbsView.GetTabRect(0).Height + 4 + 252)
      PLItems.Clear()
      'dgvPlayList.Rows.Clear()
      Loaded = True
      OpenPlayList(IO.Path.GetTempPath & "seed" & instanceID & ".m3u8")
      If IO.File.Exists(IO.Path.GetTempPath & "seed" & instanceID & ".ini") Then
        Dim iData As String = IO.File.ReadAllText(IO.Path.GetTempPath & "seed" & instanceID & ".ini")
        If iData.Contains(vbNewLine) Then
          Dim sSel As String = Split(iData, vbNewLine)(0)
          Dim sPos As String = Split(iData, vbNewLine)(1)
          If IsNumeric(sSel) Then
            If Val(sSel) >= 0 And Val(sSel) < dgvPlayList.RowCount Then
              Dim iSel As Integer = CInt(sSel)
              If IsNumeric(sPos) AndAlso sPos > 0 Then
                Dim dPos As Double = CDbl(sPos)
                Dim bMute As Boolean = mpPlayer.Mute
                mpPlayer.Mute = True
                Dim rowData As PlayListItem = PLItems.PlayListItem(iSel)
                If rowData Is Nothing Then Return
                OpenFile(rowData.Path, True)
                SelectedPlayListItem = iSel
                InitialQueue(Nothing)
                If dPos >= mpPlayer.Duration Then
                  mpPlayer.Position = dPos
                  mpPlayer.Mute = bMute
                  cmdNextPL.PerformClick()
                ElseIf dPos = 0 Then
                  mpPlayer.mpPause()
                  mpPlayer.Position = 0
                  mpPlayer.Mute = bMute
                  SetPlayPause(False)
                Else
                  mpPlayer.Position = dPos
                  mpPlayer.Mute = bMute
                  SetPlayPause(True)
                  mpPlayer.StateFade = False
                  mpPlayer.mpPlay()
                End If
              Else
                SelectedPlayListItem = iSel
              End If
            End If
          End If
        End If
      End If
    End If
    Dim iItems As Integer = 0
    For iTest As Integer = 1 To 99
      Dim tPL_PATH As String = IO.Path.GetTempPath & "seed" & iTest & ".m3u8"
      If IO.File.Exists(tPL_PATH) Then
        iItems += 1
      End If
    Next
    If iItems > 0 Then
      Dim iStart As Integer = 1
      If Not Loaded Then
        Dim tmpID As Integer = 1
        For iTest As Integer = 1 To 99
          Dim tPL_PATH As String = IO.Path.GetTempPath & "seed" & iTest & ".m3u8"
          If IO.File.Exists(tPL_PATH) Then
            tmpID = iTest
            Exit For
          End If
        Next
        If IO.File.Exists(IO.Path.GetTempPath & "seed" & tmpID & ".m3u8") Then
          PLItems.Clear()
          'dgvPlayList.Rows.Clear()
          Loaded = True
          OpenPlayList(IO.Path.GetTempPath & "seed" & tmpID & ".m3u8")
          If IO.File.Exists(IO.Path.GetTempPath & "seed" & tmpID & ".ini") Then
            Dim iData As String = IO.File.ReadAllText(IO.Path.GetTempPath & "seed" & tmpID & ".ini")
            If iData.Contains(vbNewLine) Then
              Dim sSel As String = Split(iData, vbNewLine)(0)
              Dim sPos As String = Split(iData, vbNewLine)(1)
              If IsNumeric(sSel) Then
                If Val(sSel) >= 0 And Val(sSel) < dgvPlayList.RowCount Then
                  Dim iSel As Integer = CInt(sSel)
                  SelectedPlayListItem = iSel
                  If IsNumeric(sPos) Then
                    If sPos > 0 Then
                      Dim dPos As Double = CDbl(sPos)
                      Dim bMute As Boolean = mpPlayer.Mute
                      mpPlayer.Mute = True
                      Dim rowData As PlayListItem = PLItems.PlayListItem(iSel)
                      If rowData Is Nothing Then Return
                      OpenFile(rowData.Path, True)
                      SelectedPlayListItem = iSel
                      InitialQueue(Nothing)
                      If dPos >= mpPlayer.Duration Then
                        mpPlayer.Position = dPos
                        mpPlayer.Mute = bMute
                        cmdNextPL.PerformClick()
                      ElseIf dPos = 0 Then
                        mpPlayer.mpPause()
                        mpPlayer.Position = 0
                        mpPlayer.Mute = bMute
                        SetPlayPause(False)
                      Else
                        mpPlayer.Position = dPos
                        mpPlayer.Mute = bMute
                        SetPlayPause(True)
                        mpPlayer.StateFade = False
                        mpPlayer.mpPlay()
                      End If
                    End If
                  Else
                    SelectedPlayListItem = iSel
                  End If
                End If
              End If
            End If
            IO.File.Delete(IO.Path.GetTempPath & "seed" & tmpID & ".ini")
          End If
          IO.File.Delete(IO.Path.GetTempPath & "seed" & tmpID & ".m3u8")
        End If
        iStart = tmpID + 1
        iItems -= 1
        If iItems < 1 Then Return
      End If
      Dim sPromt As String = "There are " & iItems & " other PlayLists saved as well." & vbNewLine & "Would you like to open new players for them?"
      If iItems = 1 Then sPromt = "There is another PlayList saved as well." & vbNewLine & "Would you like to open a new player for it?"
      If MsgBox(sPromt, MsgBoxStyle.Question Or MsgBoxStyle.YesNo, My.Application.Info.Title) = MsgBoxResult.Yes Then
        Dim iInst As Integer = 0
        For iTest As Integer = iStart To 99
          Dim tPL_PATH As String = IO.Path.GetTempPath & "seed" & iTest & ".m3u8"
          If IO.File.Exists(tPL_PATH) Then
            iInst += 1
            Dim frmTmp As New frmMain
            frmTmp.Tag = "NOCMD"
            frmTmp.instanceID = iInst
            frmTmp.Show()
            frmTmp.tmrCommandCycle.Stop()
            Dim commands As New ObjectModel.Collection(Of String)
            frmTmp.tmrCommandCycle.Tag = commands
            frmTmp.tmrCommandCycle.Start()
            frmTmp.PLItems.Clear()
            'frmTmp.dgvPlayList.Rows.Clear()
            frmTmp.OpenPlayList(tPL_PATH)
            Dim tPL_INI As String = IO.Path.GetTempPath & "seed" & iTest & ".ini"
            If IO.File.Exists(tPL_INI) Then
              Dim iData As String = IO.File.ReadAllText(tPL_INI)
              If iData.Contains(vbNewLine) Then
                Dim sSel As String = Split(iData, vbNewLine)(0)
                Dim sPos As String = Split(iData, vbNewLine)(1)
                If IsNumeric(sSel) Then
                  If Val(sSel) >= 0 And Val(sSel) < frmTmp.dgvPlayList.RowCount Then
                    Dim iSel As Integer = CInt(sSel)
                    frmTmp.SelectedPlayListItem = iSel
                    If IsNumeric(sPos) Then
                      If sPos > 0 Then
                        Dim dPos As Double = CDbl(sPos)
                        Dim bMute As Boolean = frmTmp.mpPlayer.Mute
                        frmTmp.mpPlayer.Mute = True
                        Dim rowData As PlayListItem = frmTmp.PLItems.PlayListItem(iSel)
                        If rowData Is Nothing Then Return
                        frmTmp.OpenFile(rowData.Path, True)
                        frmTmp.SelectedPlayListItem = iSel
                        frmTmp.InitialQueue(Nothing)
                        If dPos >= frmTmp.mpPlayer.Duration Then
                          frmTmp.mpPlayer.Position = dPos
                          frmTmp.mpPlayer.Mute = bMute
                          frmTmp.cmdNextPL.PerformClick()
                          frmTmp.cmdPlayPause.PerformClick()
                        ElseIf dPos = 0 Then
                          frmTmp.mpPlayer.mpPause()
                          frmTmp.mpPlayer.Position = 0
                          frmTmp.mpPlayer.Mute = bMute
                          frmTmp.SetPlayPause(True)
                        Else
                          frmTmp.mpPlayer.Position = dPos
                          frmTmp.mpPlayer.mpPause()
                          frmTmp.mpPlayer.Mute = bMute
                          frmTmp.SetPlayPause(True)
                        End If
                      End If
                    Else
                      frmTmp.SelectedPlayListItem = iSel
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

#Region "Save"
  Public Sub SavePlayList(Path As String, AsDisplayed As Boolean)
    Select Case IO.Path.GetExtension(Path).ToLower
      Case ".llpl" : SaveBinaryPL(Path, AsDisplayed)
      Case ".m3u" : SaveM3U(Path, False, AsDisplayed)
      Case ".m3u8" : SaveM3U(Path, True, AsDisplayed)
      Case ".pls" : SavePLS(Path, AsDisplayed)
      Case Else : SaveTextPL(Path, AsDisplayed)
    End Select
  End Sub

  Private Sub SaveBinaryPL(Path As String, AsDisplayed As Boolean)
    Dim bWriter As New IO.BinaryWriter(New IO.FileStream(Path, IO.FileMode.Create))
    bWriter.Write(CByte(2))
    bWriter.Write(CStr(txtPlayListTitle.Tag))
    bWriter.Write(CULng(dgvPlayList.RowCount))
    For I As Integer = 0 To PLItems.Count - 1
      Dim rowData As PlayListItem = Nothing
      If AsDisplayed Then
        rowData = PLItems.PlayListItem(I)
      Else
        rowData = PLItems.PlayListItemOrdered(I)
      End If
      If rowData Is Nothing Then Continue For
      bWriter.Write(CStr(rowData.Path))
      bWriter.Write(CStr(rowData.Title))
    Next
    bWriter.Close()
  End Sub

  Private Sub SaveTextPL(Path As String, AsDisplayed As Boolean)
    Dim sWriter As IO.StreamWriter = New IO.StreamWriter(Path, False)
    For I As Integer = 0 To PLItems.Count - 1
      Dim rowData As PlayListItem = Nothing
      If AsDisplayed Then
        rowData = PLItems.PlayListItem(I)
      Else
        rowData = PLItems.PlayListItemOrdered(I)
      End If
      If rowData Is Nothing Then Continue For
      sWriter.WriteLine(rowData.Path)
      sWriter.WriteLine(rowData.Title)
    Next
    sWriter.Close()
  End Sub

  Private Sub SaveM3U(Path As String, UTF8 As Boolean, AsDisplayed As Boolean)
    Dim encoding As System.Text.Encoding
    If UTF8 Then
      encoding = System.Text.Encoding.UTF8
    Else
      encoding = System.Text.Encoding.GetEncoding(LATIN_1)
    End If
    Dim sWriter As IO.StreamWriter = New IO.StreamWriter(Path, False, encoding)
    sWriter.WriteLine("#EXTM3U")
    For I As Integer = 0 To PLItems.Count - 1
      Dim rowData As PlayListItem = Nothing
      If AsDisplayed Then
        rowData = PLItems.PlayListItem(I)
      Else
        rowData = PLItems.PlayListItemOrdered(I)
      End If
      If rowData Is Nothing Then Continue For
      sWriter.WriteLine("#EXTINF:" & Math.Floor(rowData.Duration) & "," & rowData.Title)
      sWriter.WriteLine(rowData.Path)
    Next
    sWriter.Close()
  End Sub

  Private Sub SavePLS(Path As String, AsDisplayed As Boolean)
    Dim sWriter As IO.StreamWriter = New IO.StreamWriter(Path, False)
    sWriter.WriteLine("[playlist]")
    sWriter.WriteLine()
    Dim idx As Integer = 0
    For I As Integer = 0 To PLItems.Count - 1
      Dim rowData As PlayListItem = Nothing
      If AsDisplayed Then
        rowData = PLItems.PlayListItem(I)
      Else
        rowData = PLItems.PlayListItemOrdered(I)
      End If
      If rowData Is Nothing Then Continue For
      sWriter.WriteLine("File" & idx & "=" & rowData.Path)
      sWriter.WriteLine("Title" & idx & "=" & rowData.Title)
      sWriter.WriteLine("Length" & idx & "=" & Math.Floor(rowData.Duration))
      sWriter.WriteLine()
      idx += 1
    Next I
    sWriter.WriteLine("NumberOfEntries=" & idx)
    sWriter.WriteLine()
    sWriter.WriteLine("Version=2")
    sWriter.WriteLine()
    sWriter.Close()
  End Sub

  Private Sub SaveTempPL(JustINI As Boolean)
    If PLItems.Count > 0 AndAlso Not mpPlayer.IsStreaming Then
      Select Case mpPlayer.State
        Case Seed.ctlSeed.MediaState.mPlaying, Seed.ctlSeed.MediaState.mPaused
          Try
            If Not JustINI Then SavePlayList(IO.Path.GetTempPath & "seed" & instanceID & ".m3u8", True)
            IO.File.WriteAllText(IO.Path.GetTempPath & "seed" & instanceID & ".ini", SelectedPlayListItem & vbNewLine & mpPlayer.Position)
          Catch ex As Exception
            instanceID += 1
          End Try
        Case Seed.ctlSeed.MediaState.mStopped
          If SelectedPlayListItem = dgvPlayList.RowCount - 1 Then
            Try
              If Not JustINI Then If IO.File.Exists(IO.Path.GetTempPath & "seed" & instanceID & ".m3u8") Then IO.File.Delete(IO.Path.GetTempPath & "seed" & instanceID & ".m3u8")
            Catch ex As Exception
            End Try
            Try
              If IO.File.Exists(IO.Path.GetTempPath & "seed" & instanceID & ".ini") Then IO.File.Delete(IO.Path.GetTempPath & "seed" & instanceID & ".ini")
            Catch ex As Exception
            End Try
          Else
            Try
              If Not JustINI Then SavePlayList(IO.Path.GetTempPath & "seed" & instanceID & ".m3u8", True)
              IO.File.WriteAllText(IO.Path.GetTempPath & "seed" & instanceID & ".ini", SelectedPlayListItem & vbNewLine & "0")
            Catch ex As Exception
              instanceID += 1
            End Try
          End If
        Case Else
          Try
            If Not JustINI Then SavePlayList(IO.Path.GetTempPath & "seed" & instanceID & ".m3u8", True)
            If IO.File.Exists(IO.Path.GetTempPath & "seed" & instanceID & ".ini") Then IO.File.Delete(IO.Path.GetTempPath & "seed" & instanceID & ".ini")
          Catch ex As Exception
            instanceID += 1
          End Try
      End Select
    Else
      Try
        If Not JustINI Then If IO.File.Exists(IO.Path.GetTempPath & "seed" & instanceID & ".m3u8") Then IO.File.Delete(IO.Path.GetTempPath & "seed" & instanceID & ".m3u8")
      Catch ex As Exception
      End Try
      Try
        If IO.File.Exists(IO.Path.GetTempPath & "seed" & instanceID & ".ini") Then IO.File.Delete(IO.Path.GetTempPath & "seed" & instanceID & ".ini")
      Catch ex As Exception
      End Try
    End If
  End Sub
#End Region


  Private lastPoolSave As Integer
  Private lastPoolSave2 As Integer

  Private Sub StartPlayList(Optional TrackNumber As Integer = 1)
    If TrackNumber > dgvPlayList.RowCount Then TrackNumber = 1
    dgvPlayList_CellMouseDoubleClick(New Object, New DataGridViewCellMouseEventArgs(0, TrackNumber - 1, 0, 0, New MouseEventArgs(Windows.Forms.MouseButtons.Left, 2, 0, 0, 0)))
  End Sub

  Private Sub AddDirToPlayList(Path As String)
    For Each item In IO.Directory.GetFiles(Path)
      AddToPlayList(item, , , False)
      Application.DoEvents()
    Next
    For Each item In IO.Directory.GetDirectories(Path)
      AddDirToPlayList(item)
    Next
  End Sub

  Private Sub AddDirToPlayListAndMaybePlay(Item As String, Optional ByVal AutoQueue As Boolean = True)
    If dgvPlayList.Rows.Count > 0 Then
      AddDirToPlayList(Item)
    Else
      AddDirToPlayList(Item)
      Application.DoEvents()
      If AutoQueue Then
        Do While (PLItems.PlayListItem(PLItems.Count - 1).Duration = 0)
          Application.DoEvents()
          Threading.Thread.Sleep(1)
          If PLItems.Count = 0 Then Return
        Loop
      End If
      If dgvPlayList.Rows.Count > 0 Then
        dgvPlayList.Rows(0).Selected = True
        StartPlayList()
      End If
    End If
  End Sub

  Private Sub AddToPlayList(Path As String, Optional Title As String = Nothing, Optional Length As Double = 0, Optional GetData As Boolean = True)
    Dim sExt As String = IO.Path.GetExtension(Path).ToLower
    Select Case sExt
      Case ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".dib", ".ini", ".db"
        Return
      Case Else
        If Not Path.ToLower.StartsWith("http://") AndAlso Not IO.File.Exists(Path) Then
          PLShowMissing(Path)
          Return
        End If
        If String.IsNullOrEmpty(Title) Then Title = IO.Path.GetFileNameWithoutExtension(Path)
        Dim Artist As String = LOADING_ARTIST
        Dim Album As String = LOADING_ALBUM
        If mnuPLDuplicates.Checked AndAlso PLItems.PlayListIndicies(Path).Length > 0 Then
          PLShowMissing(IO.Path.GetFileName(Path), "File Already Exists")
        Else
          PLItems.Add(New PlayListItem(Path, Title, Length))
        End If
    End Select
  End Sub

  Private Sub RemoveFromPlayList(Index As Integer)
    PLItems.Remove(Index)
    ThreadedQueue()
    SaveTempPL(False)
  End Sub

  Private Sub QueueNextTrack(state As Object)
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New Threading.WaitCallback(AddressOf QueueNextTrack), state)
      Catch ex As Exception
      End Try
      Return
    End If
    Dim SelItem As Integer = SelectedPlayListItem + 1
    If dgvPlayList.Rows.Count > 0 Then
      If dgvPlayList.Rows.Count - 1 > SelItem And SelItem >= 0 Then
        Dim rowData As PlayListItem = PLItems.PlayListItem(SelItem)
        If rowData Is Nothing Then Return
        Dim sTrack As String = rowData.Path
        If IO.File.Exists(sTrack) And Not IO.Path.GetExtension(sTrack).ToLower = ".cda" Then
          If Not sLastQueuedTrack = sTrack Then
            sLastQueuedTrack = sTrack
            mpPlayer.FileQueue(sTrack)
          End If
        Else
          sLastQueuedTrack = Nothing
          mpPlayer.SetNoQueue()
        End If
      Else
        sLastQueuedTrack = Nothing
        mpPlayer.SetNoQueue()
      End If
    Else
      sLastQueuedTrack = Nothing
      mpPlayer.SetNoQueue()
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
    taskCancel.Cancel()
    tmrUpdate.Stop()
    tmrVis.Stop()
    tmrCommandCycle.Stop()
    If frmFS IsNot Nothing Then
      frmFS.Dispose()
      frmFS = Nothing
    End If
    If frmTI IsNot Nothing Then
      frmTI.Dispose()
      frmTI = Nothing
    End If
    frmText.Close()
    If Me.WindowState <> FormWindowState.Normal Then Me.WindowState = FormWindowState.Normal
    If mpPlayer.State <> Seed.ctlSeed.MediaState.mClosed Then mpPlayer.FileName = String.Empty
    If mpPlayer IsNot Nothing Then mpPlayer.Dispose()
    My.Settings.Location = Me.Location
    My.Settings.Save()
  End Sub

  Private Sub frmMain_GotFocus(sender As Object, e As System.EventArgs) Handles Me.GotFocus
    If frmFS IsNot Nothing AndAlso frmFS.Visible Then frmFS.Focus()
  End Sub

  Private Sub frmMain_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
    If (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Then ClickDrag(Me.Handle)
  End Sub

  Private Sub frmMain_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
    DrawGlassText(True)
  End Sub

  Private Sub frmMain_Resize(sender As Object, e As System.EventArgs) Handles Me.Resize
    If cTask Is Nothing Then Return
    If VidThumb Then
      If mpPlayer.Size.Height > 0 Then
        cTask.UpdateClip(mpPlayer)
      Else
        cTask.UpdateClip(pnlMain)
      End If
    Else
      If Not cTask.ImageEnabled Then cTask.UpdateClip(pnlMain)
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
    If cTask Is Nothing Then Return
    If VidThumb Then
      If mpPlayer.Size.Height > 0 Then
        cTask.UpdateClip(mpPlayer)
      Else
        cTask.UpdateClip(pnlMain)
      End If
    Else
      If Not cTask.ImageEnabled Then cTask.UpdateClip(pnlMain)
    End If
  End Sub

  Private Sub cTask_AppEvent(sender As Object, e As TabbedThumbnailAppEventArgs) Handles cTask.AppEvent
    Select Case e.EventType
      Case TabbedThumbnailAppEventArgs.Events.Closed : Me.Close()
      Case TabbedThumbnailAppEventArgs.Events.Minimized : Me.WindowState = FormWindowState.Minimized
      Case TabbedThumbnailAppEventArgs.Events.Maximized : Me.WindowState = FormWindowState.Maximized
      Case TabbedThumbnailAppEventArgs.Events.Activated
        If Me.WindowState = FormWindowState.Minimized Then Me.WindowState = FormWindowState.Normal
        Me.Activate()
    End Select
  End Sub

  Private Sub cTask_Button_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles cTask.Button_Click
    Select Case sender.name
      Case "ttbBack" : cmdBackPL_Click(New Object, New EventArgs)
      Case "ttbNext" : cmdNextPL_Click(New Object, New EventArgs)
      Case "ttbPlayPause" : cmdPlayPause_Click(New Object, New EventArgs)
      Case "ttbStop" : cmdStop_Click(New Object, New EventArgs)
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
    NativeMethods.InsertMenu(hSysMenu, 1, NativeMethods.MenuFlags.MF_STRING Or NativeMethods.MenuFlags.MF_BYPOSITION, HIDE_MENU_ID, HIDE_MENU_TEXT)
    NativeMethods.InsertMenu(hSysMenu, 2, NativeMethods.MenuFlags.MF_SEPARATOR Or NativeMethods.MenuFlags.MF_BYPOSITION, 0, String.Empty)
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
        Case HIDE_MENU_ID
          trayIcon.Visible = True
          If Not Me.WindowState = FormWindowState.Normal Then Me.WindowState = FormWindowState.Normal
          Me.Hide()
      End Select
    End If
  End Sub
#End Region
#End Region

#Region "View Tab Strip"
  Private tbsView_LastSize As Drawing.Size
  Private Sub tbsView_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles tbsView.SelectedIndexChanged
    If tbsView.SelectedTab Is Nothing Then
      Me.Size = Me.MinimumSize
      Me.Top += (tbsView_LastSize.Height - Me.Height)
      KeepInTheScreen()
      tbsView_LastSize = Me.MinimumSize
      Return
    End If
    If tbsView.SelectedTab Is tabRipper Then ripBox.RefreshDriveList()
    Dim BorderSize As New Drawing.Size(Me.Width - mpPlayer.Width, Me.Height - mpPlayer.Height)
    Select Case tbsView.SelectedTab.Handle.ToInt64
      Case tabArt.Handle.ToInt64
        tbsView_LastSize = Me.Size
        If artList.Visible Then
          Me.Size = New Drawing.Size(600, 450)
          Me.Top += (tbsView_LastSize.Height - Me.Height)
          KeepInTheScreen()
          Return
        End If
        If mnuArtShow.Checked Then
          If FileArt IsNot Nothing AndAlso FileArt.Art IsNot Nothing Then
            tbsView_LastSize = Me.Size
            Me.Width = Me.MinimumSize.Width
            Dim lSize As Drawing.Size
            lSize.Width = pctAlbumArt.Width
            lSize.Height = FileArt.Art.Height / FileArt.Art.Width * lSize.Width
            SetWindowSize(lSize.Width, lSize.Height, Me.Width - pctAlbumArt.Width, Me.Height - pctAlbumArt.Height)
            'Me.Top += (lastSize.Height - Me.Height)
            KeepInTheScreen()
            Return
          End If
        End If
        If methodDraw IsNot Nothing Then
          Me.Size = New Drawing.Size(Me.MinimumSize.Width, Me.MinimumSize.Height + tbsView.GetTabRect(0).Height + My.Resources.Logo.Height + 8)
          Me.Top += (tbsView_LastSize.Height - Me.Height)
          KeepInTheScreen()
          Return
        End If
        Me.Size = New Drawing.Size(Me.MinimumSize.Width, Me.MinimumSize.Height + tbsView.GetTabRect(0).Height + 4)
        Me.Top += (tbsView_LastSize.Height - Me.Height)
      Case tabVideo.Handle.ToInt64
        If mpPlayer.HasVid Then
          tbsView_LastSize = Me.Size
          SetWindowSize(mpPlayer.VideoWidth, mpPlayer.VideoHeight, Me.Width - mpPlayer.Width, Me.Height - mpPlayer.Height)
        End If
      Case tabRipper.Handle.ToInt64
        tbsView_LastSize = Me.Size
        Me.Size = New Drawing.Size(720, 540)
        Me.Top += (tbsView_LastSize.Height - Me.Height)
      Case Else
        If tbsView_LastSize.Height > 0 Then
          Me.Top -= (tbsView_LastSize.Height - Me.Height)
          Me.Height = tbsView_LastSize.Height
          Me.Width = tbsView_LastSize.Width
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
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New Threading.WaitCallback(AddressOf mpPlayer_CantOpen), Message)
      Catch ex As Exception
      End Try
      Return
    End If
    MsgBox("Can't Open File:" & vbNewLine & Message, MsgBoxStyle.Exclamation, My.Application.Info.Title)
  End Sub

  Private Sub mpPlayer_EndOfFile(Status As Integer) Handles mpPlayer.EndOfFile
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New Threading.WaitCallback(AddressOf mpPlayer_EndOfFile), Status)
      Catch ex As Exception
      End Try
      Return
    End If
    If mpPlayer.IsStreaming Then
      'ThreadedInitial()
      Return
    End If
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
      If dgvPlayList.Rows.Count > 0 And SelectedPlayListItem > -1 Then
        Dim SelItem As Integer = SelectedPlayListItem + 1
        If SelItem <= dgvPlayList.Rows.Count - 1 Then
          SelectedPlayListItem = SelItem
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
    Else
      If dgvPlayList.Rows.Count > 0 And SelectedPlayListItem > -1 Then
        Dim SelItem As Integer = SelectedPlayListItem + 1
        If SelItem <= dgvPlayList.Rows.Count - 1 Then
          Dim rowData As PlayListItem = PLItems.PlayListItem(SelItem)
          If rowData Is Nothing Then Return
          OpenFile(rowData.Path, True)
          SelectedPlayListItem = SelItem
          ThreadedInitial()
          Return
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
      SetPlayPause(True)
      mpPlayer.StateFade = False
      mpPlayer.mpStop()
      mpPlayer.Position = 0
    End If
    FirstInit = True
    ThreadedInitial()
    Select Case finalAct
      Case 1
        Application.DoEvents()
        If cmdShufflePL.Tag = True Then
          cmdShufflePL.Tag = False
          cmdShufflePL_Click(Me, New EventArgs)
          StartPlayList(2)
        Else
          StartPlayList()
        End If
      Case 2
        ClearPlayList()
        SaveTempPL(False)
        Application.DoEvents()
        mnuExit.PerformClick()
      Case 3
        ClearPlayList()
        SaveTempPL(False)
        Application.DoEvents()
        Process.Start("shutdown", "/r /t 0 /d p:0:0 /f")
      Case 4
        ClearPlayList()
        SaveTempPL(False)
        Application.DoEvents()
        Process.Start("shutdown", "/s /t 0 /d p:0:0 /f")
      Case 5
        If PowerProfile.CanHibernate Then
          ClearPlayList()
          SaveTempPL(False)
          Application.DoEvents()
          Process.Start("shutdown", "/h /t 0 /d p:0:0 /f")
        End If
    End Select
  End Sub

  Private ReadOnly Property ThreadPoolHasThreads As Boolean
    Get
      Dim minworkers, minports As Integer
      Threading.ThreadPool.GetMinThreads(minworkers, minports)
      Dim maxworkers, maxports As Integer
      Threading.ThreadPool.GetMaxThreads(maxworkers, maxports)
      Dim workers, ports As Integer
      Threading.ThreadPool.GetAvailableThreads(workers, ports)
      Return workers > maxworkers - minworkers
    End Get
  End Property

  Friend Sub InitialQueue(state As Object)
    InitialData(Nothing)
    ThreadedQueue()
  End Sub

  Friend Sub ThreadedQueue()
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New MethodInvoker(AddressOf ThreadedQueue))
      Catch ex As Exception
      End Try
      Return
    End If
    If ThreadPoolHasThreads Then
      Threading.ThreadPool.QueueUserWorkItem(AddressOf QueueNextTrack)
    Else
      QueueNextTrack(Nothing)
    End If
  End Sub

  Friend Sub ThreadedInitial()
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New MethodInvoker(AddressOf ThreadedInitial))
      Catch ex As Exception
      End Try
      Return
    End If
    If ThreadPoolHasThreads Then
      Threading.ThreadPool.QueueUserWorkItem(AddressOf InitialData)
    Else
      InitialData(Nothing)
    End If
  End Sub

  Private Sub mpPlayer_MediaError(e As Seed.ctlSeed.MediaErrorEventArgs) Handles mpPlayer.MediaError
    Debug.Print("Media Player Error: " & e.E.Message & " in " & e.Funct)
  End Sub

  Private Sub mpPlayer_VidClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles mpPlayer.VidClick
    If (e.Button And Windows.Forms.MouseButtons.Right) = Windows.Forms.MouseButtons.Right Then mnuVideo.Show(MousePosition)
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

  Private Sub CursorText(Message As String, Icon As DragDropEffects)
    If Not frmText.Visible Then frmText.Show(Me)
    frmText.SetText(Message, Icon)
    Me.Activate()
  End Sub

  Private Sub SetFFDShowVals()
    If cmbVidTrack.Visible AndAlso Not cmbVidTrack.DroppedDown Then cmbVidTrack.SelectedIndex = 0
    If cmbAudTrack.Visible AndAlso Not cmbAudTrack.DroppedDown And ffAPI.AudioStream > 0 Then cmbAudTrack.SelectedIndex = ffAPI.AudioStream - IIf(cmbVidTrack.Items.Count > 1, cmbVidTrack.Items.Count + 1, 1)
    If cmbSubtitles.Visible AndAlso Not cmbSubtitles.DroppedDown And ffAPI.SubtitleStream > 0 Then cmbSubtitles.SelectedIndex = ffAPI.SubtitleStream - cmbAudTrack.Items.Count - IIf(cmbVidTrack.Items.Count > 1, cmbVidTrack.Items.Count + 1, 1)
  End Sub

  Private Sub SetTaskbarStyle([Style] As TaskbarLib.TBPFLAG)
    If taskBar IsNot Nothing Then taskBar.SetProgressState(Me.Handle, Style)
  End Sub

  Private Sub SetPlayPause(Play As Boolean, Optional Enabled As TriState = TriState.UseDefault)
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
    If Not Enabled = TriState.UseDefault Then If Not cmdPlayPause.Enabled = (Enabled = TriState.True) Then cmdPlayPause.Enabled = (Enabled = TriState.True)
    If cTask IsNot Nothing Then If Not cTask.PlayPauseEnabled = cmdPlayPause.Enabled Then cTask.PlayPauseEnabled = cmdPlayPause.Enabled
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
    VidSize.Width = width
    VidSize.Height = height
    CorrectedSize = VidSize
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
    videoSize_Width = width
    videoSize_Height = height
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
    videoSize_Width = width * Scale
    videoSize_Height = height * Scale
    mpPlayer.VideoWidth = width * Scale
    mpPlayer.VideoHeight = height * Scale
    SetWindowSize(width * Scale, height * Scale, (Me.Width - mpPlayer.Width), (Me.Height - mpPlayer.Height))
    frmMain_ResizeEnd(New Object, New EventArgs)
  End Sub

  Private Sub SetWindowSize(vW As Integer, vH As Integer, bW As Integer, bH As Integer)
    Dim vbW As Integer = vW + bW
    Dim vbH As Integer = vH + bH
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
    For Each drive As IO.DriveInfo In IO.DriveInfo.GetDrives
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

  Private Sub InitialData(state As Object)
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New Threading.WaitCallback(AddressOf InitialData), state)
      Catch ex As Exception
      End Try
      Return
    End If
    Dim sTitleVal As String = IO.Path.GetFileNameWithoutExtension(mpPlayer.FileName)
    Dim sArtistVal As String = UNKNOWN_ARTIST
    If FirstInit Then
      VidSize = Drawing.Size.Empty
      volControl.SetSound(False)
      volControl.SetVolume(100)
    End If
    mnuCloseFile.Enabled = True
    mnuProperties.Enabled = True
    If bCD Then
      LoadData_CD(sTitleVal, sArtistVal)
    ElseIf String.IsNullOrEmpty(mpPlayer.FileName) Then
      LoadData_Void(sTitleVal, sArtistVal)
    ElseIf mpPlayer.HasVid Or bDVD Then
      LoadData_Video(sTitleVal, sArtistVal)
    Else
      LoadData_Audio(sTitleVal, sArtistVal)
    End If
    LabelShortcuts()
    SetTitleArtist(sTitleVal, sArtistVal)
    FirstInit = False
  End Sub

  Private Sub LoadData_Void(ByRef sTitleVal As String, ByRef sArtistVal As String)
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
    pctAlbumArt.Image = Nothing
    If frmFS IsNot Nothing Then frmFS.pctVideo.Image = Nothing
    mnuCloseFile.Enabled = False
    mnuProperties.Enabled = False
  End Sub

  Private Sub LoadData_CD(ByRef sTitleVal As String, ByRef sArtistVal As String)
    EnableScreenSaver(True)
    SetTabs(True, False)
    If SelectedPlayListItem > -1 Then
      Dim dTag = dgvPlayList.Rows(SelectedPlayListItem).Tag
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
    SetProps(4, 2, -1, "CD Audio", "Stereo")
    SetCombos(False, False, False, False)
    VidThumb = False
    cmdLoop.Enabled = True
    cmdLoop.Image = My.Resources.button_eject
  End Sub

  Private Sub LoadData_Video(ByRef sTitleVal As String, ByRef sArtistVal As String)
    If FirstInit Then
      EnableScreenSaver(False)
      SetTabs(False, True)
      tbsView.SelectedTab = tabVideo
      FileArt = Nothing
      If LoadFFDShow() Then
        ffAPI.DoResize = False
        Application.DoEvents()
      End If
    End If
    'FileTitle = IO.Path.GetFileNameWithoutExtension(mpPlayer.FileName)
    Dim selVid As Integer = -1
    Dim VidTrackList As New Dictionary(Of String, String)
    Dim selAud As Integer = -1
    Dim AudTrackList As New Dictionary(Of String, String)
    Dim selSub As Integer = -1
    Dim SubTrackList As New Dictionary(Of String, String)
    Dim selChapter As Integer = -1
    Dim ChapterList As New List(Of String)
    If bDVD Then
      LoadData_Video_DVD(sTitleVal, sArtistVal, VidTrackList, selVid, AudTrackList, selAud, SubTrackList, selSub, ChapterList, selChapter)
    ElseIf String.Compare(IO.Path.GetExtension(mpPlayer.FileName), ".mkv", True) = 0 Then
      LoadData_Video_MKV(sTitleVal, sArtistVal, VidTrackList, selVid, AudTrackList, selAud, SubTrackList, selSub, ChapterList, selChapter)
    ElseIf String.Compare(IO.Path.GetExtension(mpPlayer.FileName), ".ogm", True) = 0 Then
      LoadData_Video_OGM(sTitleVal, sArtistVal, VidTrackList, selVid, AudTrackList, selAud, SubTrackList, selSub, ChapterList, selChapter)
    ElseIf String.Compare(IO.Path.GetExtension(mpPlayer.FileName), ".avi", True) = 0 Then
      LoadData_Video_AVI(sTitleVal, sArtistVal, VidTrackList, selVid, AudTrackList, selAud, SubTrackList, selSub, ChapterList, selChapter)
    ElseIf LoadFFDShow() Then
      LoadData_Video_FFDShow(sTitleVal, sArtistVal, VidTrackList, selVid, AudTrackList, selAud, SubTrackList, selSub, ChapterList, selChapter)
    Else
      SetCombos(False, False, False, False)
      SetVideoResolution(-1, -1)
      Dim iQ As Long = ((New IO.FileInfo(mpPlayer.FileName)).Length * 8) / (mpPlayer.Duration)
      SetProps(-1, 2, 0, , "Unknown Speaker Setup", KRater(iQ, "bps"))
    End If
    cmbVidTrack.DroppedDown = False
    cmbAudTrack.DroppedDown = False
    cmbSubtitles.DroppedDown = False
    cmbChapters.DroppedDown = False
    If Not frmFS.Visible Then
      SetCombos(VidTrackList.Count > 1, AudTrackList.Count > 1, ChapterList.Count > 1, SubTrackList.Count > 1)
      cmbAudTrack.Tag = True
      cmbChapters.Tag = True
      cmbSubtitles.Tag = True
      cmbVidTrack.Tag = True

      If VidTrackList.Count > 1 Then
        For Each vidTrack In VidTrackList
          Dim sTrackTitle As String = vidTrack.Key
          If Not CompareLanguages(vidTrack.Value, "und") And Not sTrackTitle.ToLower.Contains(vidTrack.Value.ToLower) Then sTrackTitle &= " [" & vidTrack.Value & "]"
          cmbVidTrack.Items.Add(sTrackTitle)
          mnuVideoTrack.DropDownItems.Add(sTrackTitle)
          Dim g As Drawing.Graphics = cmbVidTrack.CreateGraphics
          Dim lWidth As Single = g.MeasureString(sTrackTitle, cmbVidTrack.Font).Width + 10
          If lWidth > VidWidth Then VidWidth = lWidth
        Next
        If selVid = -1 Then selVid = 0
        If selVid < cmbVidTrack.Items.Count Then cmbVidTrack.SelectedIndex = selVid
      End If

      Dim audLang As String = "und"
      If AudTrackList.Count > 1 Then
        For Each audTrack In AudTrackList
          Dim sTrackTitle As String = audTrack.Key
          If Not CompareLanguages(audTrack.Value, "und") And Not sTrackTitle.ToLower.Contains(audTrack.Value.ToLower) Then sTrackTitle &= " [" & audTrack.Value & "]"
          cmbAudTrack.Items.Add(sTrackTitle)
          mnuAudioTrack.DropDownItems.Add(sTrackTitle)
          Dim g As Drawing.Graphics = cmbAudTrack.CreateGraphics
          Dim lWidth As Single = g.MeasureString(sTrackTitle, cmbAudTrack.Font).Width + 10
          If lWidth > AudWidth Then AudWidth = lWidth
        Next
        If Not String.IsNullOrEmpty(My.Settings.DefaultLocale) Then
          For I As Integer = 0 To AudTrackList.Count - 1
            If CompareLanguages(My.Settings.DefaultLocale, AudTrackList.Values(I)) Then
              selAud = I
              Exit For
            End If
          Next
        End If
        If selAud = -1 Then selAud = 0
        If selAud < cmbAudTrack.Items.Count Then
          cmbAudTrack.SelectedIndex = selAud
          audLang = cmbAudTrack.SelectedItem
        End If
      End If

      If SubTrackList.Count > 1 Then
        For Each subTrack In SubTrackList
          Dim sTrackTitle As String = subTrack.Key
          If Not CompareLanguages(subTrack.Value, "und") And Not sTrackTitle.ToLower.Contains(subTrack.Value.ToLower) Then sTrackTitle &= " [" & subTrack.Value & "]"
          cmbSubtitles.Items.Add(sTrackTitle)
          mnuSubtitleTrack.DropDownItems.Add(sTrackTitle)
          Dim g As Drawing.Graphics = cmbSubtitles.CreateGraphics
          Dim lWidth As Single = g.MeasureString(sTrackTitle, cmbSubtitles.Font).Width + 10
          If lWidth > SubWidth Then SubWidth = lWidth
        Next
        If CompareLanguages(audLang, "und") Then
          If My.Settings.Captions Or My.Settings.Subtitles Then
            If selSub = -1 Then
              If Not String.IsNullOrEmpty(My.Settings.DefaultLocale) Then
                For I As Integer = 0 To SubTrackList.Count - 1
                  If CompareLanguages(My.Settings.DefaultLocale, SubTrackList.Values(I)) Then
                    selSub = I
                    Exit For
                  End If
                Next
              End If
            End If
            If selSub = -1 Then selSub = 0
          Else
            For I As Integer = 0 To SubTrackList.Count - 1
              If SubTrackList.Keys(I) = "No subtitles" Then
                selSub = I
                Exit For
              End If
            Next
          End If
        ElseIf CompareLanguages(audLang, My.Settings.DefaultLocale) Then
          If My.Settings.Captions Then
            If selSub = -1 Then
              For I As Integer = 0 To SubTrackList.Count - 1
                If CompareLanguages(audLang, SubTrackList.Values(I)) Then
                  selSub = I
                  Exit For
                End If
              Next
            End If
          Else
            For I As Integer = 0 To SubTrackList.Count - 1
              If SubTrackList.Keys(I) = "No subtitles" Then
                selSub = I
                Exit For
              End If
            Next
          End If
        Else
          If My.Settings.Captions Then
            If selSub = -1 Then
              For I As Integer = 0 To SubTrackList.Count - 1
                If CompareLanguages(audLang, SubTrackList.Values(I)) Then
                  selSub = I
                  Exit For
                End If
              Next
              If selSub = -1 Then
                For I As Integer = 0 To SubTrackList.Count - 1
                  If CompareLanguages(My.Settings.DefaultLocale, SubTrackList.Values(I)) Then
                    selSub = I
                    Exit For
                  End If
                Next
              End If
            End If
          ElseIf My.Settings.Subtitles Then
            If selSub = -1 Then
              If Not CompareLanguages(audLang, My.Settings.DefaultLocale) Then
                For I As Integer = 0 To SubTrackList.Count - 1
                  If CompareLanguages(My.Settings.DefaultLocale, SubTrackList.Values(I)) Then
                    selSub = I
                    Exit For
                  End If
                Next
                If selSub = -1 Then
                  For I As Integer = 0 To SubTrackList.Count - 1
                    If CompareLanguages(audLang, SubTrackList.Values(I)) Then
                      selSub = I
                      Exit For
                    End If
                  Next
                End If
              End If
            End If
          Else
            For I As Integer = 0 To SubTrackList.Count - 1
              If SubTrackList.Keys(I) = "No subtitles" Then
                selSub = I
                Exit For
              End If
            Next
          End If
        End If
        If selSub = -1 Then
          For I As Integer = 0 To SubTrackList.Count - 1
            If SubTrackList.Keys(I) = "No subtitles" Then
              selSub = I
              Exit For
            End If
          Next
        End If
        If selSub < cmbSubtitles.Items.Count Then cmbSubtitles.SelectedIndex = selSub
      End If

      If ChapterList.Count > 1 Then
        For Each chapter In ChapterList
          cmbChapters.Items.Add(chapter)
          mnuChapterTrack.DropDownItems.Add(chapter)
          Dim g As Drawing.Graphics = cmbChapters.CreateGraphics
          Dim lWidth As Single = g.MeasureString(chapter, cmbChapters.Font).Width + 10
          If lWidth > ChapterWidth Then ChapterWidth = lWidth
        Next
        If selChapter = -1 Then selChapter = 0
        If selChapter < cmbChapters.Items.Count Then cmbChapters.SelectedIndex = selChapter
      End If

      cmbAudTrack.Tag = Nothing
      cmbChapters.Tag = Nothing
      cmbSubtitles.Tag = Nothing
      cmbVidTrack.Tag = Nothing
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
      End If
    End If
    pctAlbumArt.Image = Nothing
    If frmFS IsNot Nothing Then frmFS.pctVideo.Image = Nothing
  End Sub

  Private Sub LoadData_Video_DVD(ByRef sTitleVal As String, ByRef sArtistVal As String, ByRef VidTrackList As Dictionary(Of String, String), ByRef selVid As Integer, ByRef AudTrackList As Dictionary(Of String, String), ByRef selAud As Integer, ByRef SubTrackList As Dictionary(Of String, String), ByRef selSub As Integer, ByRef ChapterList As List(Of String), ByRef selChapter As Integer)
    sTitleVal = (New IO.DriveInfo(mpPlayer.FileName.Substring(0, 3))).VolumeLabel
    If FirstInit Then
      cmdLoop.Enabled = True
      cmdLoop.Image = My.Resources.button_eject
    End If
    If mpPlayer.GetDVDTitles() > 1 Then
      For I As Integer = 1 To mpPlayer.GetDVDTitles
        Dim titleData = mpPlayer.GetDVDTitleAttributes(I)
        If titleData.ulNumberOfAudioStreams = 0 Then
          VidTrackList.Add("Title " & I & ": " & titleData.VideoAttributes.sourceResolutionX & "x" & titleData.VideoAttributes.sourceResolutionY & " (No Audio)", "und")
        ElseIf titleData.ulNumberOfAudioStreams = 1 Then
          VidTrackList.Add("Title " & I & ": " & titleData.VideoAttributes.sourceResolutionX & "x" & titleData.VideoAttributes.sourceResolutionY & " (" & titleData.AudioAttributes(0).AudioFormat.ToString & " " & titleData.AudioAttributes(0).bNumberOfChannels & "ch)", titleData.AudioAttributes(0).Language)
        Else
          VidTrackList.Add("Title " & I & ": " & titleData.VideoAttributes.sourceResolutionX & "x" & titleData.VideoAttributes.sourceResolutionY & " (" & titleData.ulNumberOfAudioStreams & " audio tracks)", "und")
        End If
      Next
      selVid = mpPlayer.GetDVDCurrentTitle - 1
    End If
    If mpPlayer.GetDVDAvailableAudioStreams > 0 Then
      For I As Integer = 0 To mpPlayer.GetDVDAvailableAudioStreams - 1
        Dim audData = mpPlayer.GetDVDAudioInfo(I)
        AudTrackList.Add(audData.bNumberOfChannels & " channel " & audData.dwFrequency & "Hz " & audData.AudioFormat.ToString, audData.Language)
      Next
      selAud = mpPlayer.GetDVDCurrentAudioStream
    End If
    If mpPlayer.GetDVDAvailableSubStreams > 0 Then
      SubTrackList.Add("No subtitles", "und")
      For I As Integer = 0 To mpPlayer.GetDVDAvailableSubStreams - 1
        Dim subData = mpPlayer.GetDVDSubAttributes(I)
        SubTrackList.Add("Subtitle " & (I + 1) & ": " & subData.LanguageExtension.ToString, subData.Language)
      Next
      selSub = mpPlayer.GetDVDCurrentSubStream + 1
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
  End Sub

  Private Sub LoadData_Video_MKV(ByRef sTitleVal As String, ByRef sArtistVal As String, ByRef VidTrackList As Dictionary(Of String, String), ByRef selVid As Integer, ByRef AudTrackList As Dictionary(Of String, String), ByRef selAud As Integer, ByRef SubTrackList As Dictionary(Of String, String), ByRef selSub As Integer, ByRef ChapterList As List(Of String), ByRef selChapter As Integer)
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
    Dim chapterCollection As New List(Of ChapterListing)
    Dim RunningTime As ULong = 0
    If mkvHeader.ChapterInfo.Editions IsNot Nothing Then
      ChapterWidth = 0
      FindAdditionalMKVChapters(mpPlayer.FileName, 0, chapterCollection, RunningTime)
      If RunningTime > 0 Then
        mpPlayer.Position = RunningTime
      End If
      For Each Edition In mkvHeader.ChapterInfo.Editions
        If Not Edition.FlagHidden Then
          Dim LastAtomEnd As Double = 0.0
          For Each Atom In Edition.Atoms
            If Not Atom.FlagHidden Then
              Dim dChapterStart As Double = CDbl(RunningTime + Atom.TimeStart / 1000000000.0)
              If dChapterStart = 0.0 Then dChapterStart = LastAtomEnd
              LastAtomEnd = CDbl(RunningTime + Atom.TimeEnd / 1000000000.0)
              Dim sChapterText As String = Atom.Display(0).Title & ": " & ConvertTimeVal(dChapterStart)
              Dim sChapterLang As String
              If Atom.Display(0).Language Is Nothing OrElse Atom.Display(0).Language.Count = 0 Then
                sChapterLang = "und"
              ElseIf Atom.Display(0).Language.Count = 1 Then
                sChapterLang = Atom.Display(0).Language(0)
              Else
                sChapterLang = Join(Atom.Display(0).Language, ", ")
              End If
              chapterCollection.Add(New ChapterListing(dChapterStart, sChapterText, sChapterLang))
            End If
          Next
        End If
      Next
      If mkvHeader.SegmentInfo.Duration > 0 Then
        RunningTime += (mkvHeader.SegmentInfo.Duration / 1000)
      Else
        RunningTime += mpPlayer.Duration
      End If
      FindAdditionalMKVChapters(mpPlayer.FileName, 1, chapterCollection, RunningTime)
      SpecificChapterList = chapterCollection
      For Each chapterData In chapterCollection
        ChapterList.Add(chapterData.ChapterName)
      Next
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
      Dim trackCounts(&HFF) As Integer
      For Each Track In mkvHeader.TrackEntries
        trackCounts(Track.TrackType) += 1
        Dim sTrackTitle As String
        If String.IsNullOrEmpty(Track.TrackName) Then
          sTrackTitle = "#" & trackCounts(Track.TrackType) & ": Untitled"
        Else
          sTrackTitle = "#" & trackCounts(Track.TrackType) & ": " & Track.TrackName
        End If
        If String.IsNullOrEmpty(Track.Language) Then Track.Language = "und"
        Select Case Track.TrackType
          Case &H1
            If Not String.IsNullOrEmpty(Track.TrackName) AndAlso String.IsNullOrEmpty(sTitleVal) Then sTitleVal = Track.TrackName
            VidTrackList.Add(sTrackTitle, Track.Language)
            sVidInfo = MKVVideoCodecs(Track.CodecID)
            If Track.DefaultDuration > 0 Then sVidInfo &= " (" & Math.Round(1 / (Track.DefaultDuration / 1000000000), 3) & " fps " & IIf(Track.Video.FlagInterlaced, "interlaced", "progressive") & ")"
          Case &H2
            AudTrackList.Add(sTrackTitle, Track.Language)
            Dim iQ As Long = ((New IO.FileInfo(mpPlayer.FileName)).Length * 8) / dDuration
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
            SubTrackList.Add(sTrackTitle, Track.Language)
        End Select
      Next
      SetProps(iEncQ, iChannels, iqR, vbNewLine & "Video: " & sVidInfo & vbNewLine & "Audio: " & sAudInfo, iChannels, sBitrate)
      If SubTrackList.Count > 0 Then SubTrackList.Add("No subtitles", "und")
    End If
  End Sub

  Private Sub LoadData_Video_OGM(ByRef sTitleVal As String, ByRef sArtistVal As String, ByRef VidTrackList As Dictionary(Of String, String), ByRef selVid As Integer, ByRef AudTrackList As Dictionary(Of String, String), ByRef selAud As Integer, ByRef SubTrackList As Dictionary(Of String, String), ByRef selSub As Integer, ByRef ChapterList As List(Of String), ByRef selChapter As Integer)
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
      Dim iQ As Long = ((New IO.FileInfo(mpPlayer.FileName)).Length * 8) / (mpPlayer.Duration)
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
      For Each chapter In ffChaps
        ChapterList.Add(chapter.Value)
      Next
    End If
    If ffAPI.AudioStreams.Count > 0 Then
      For Each stream In ffAPI.AudioStreams
        Dim sTrackTitle As String = stream.Value.name
        If sTrackTitle.StartsWith("A: ") Then sTrackTitle = sTrackTitle.Substring(3)
        AudTrackList.Add(sTrackTitle, stream.Value.languageName)
        If stream.Value.enabled Then selAud = AudTrackList.Count - 1
      Next
    End If
    If ffAPI.SubtitleStreams.Count > 0 Then
      For Each stream In ffAPI.SubtitleStreams
        Dim sTrackTitle As String = stream.Value.name
        If sTrackTitle.StartsWith("S: ") Then sTrackTitle = sTrackTitle.Substring(3)
        SubTrackList.Add(sTrackTitle, stream.Value.languageName)
        If stream.Value.enabled Then selSub = SubTrackList.Count - 1
      Next
    End If
    SetVideoResolution(-1, -1)
  End Sub

  Private Sub LoadData_Video_AVI(ByRef sTitleVal As String, ByRef sArtistVal As String, ByRef VidTrackList As Dictionary(Of String, String), ByRef selVid As Integer, ByRef AudTrackList As Dictionary(Of String, String), ByRef selAud As Integer, ByRef SubTrackList As Dictionary(Of String, String), ByRef selSub As Integer, ByRef ChapterList As List(Of String), ByRef selChapter As Integer)
    Using cRiff As New Seed.clsRIFF(mpPlayer.FileName)
      Dim iQ As Long = ((New IO.FileInfo(mpPlayer.FileName)).Length * 8) / (mpPlayer.Duration)
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
        Dim vidTracks As New List(Of Seed.clsRIFF.AVISTREAMHEADER)
        Dim audTracks As New List(Of Seed.clsRIFF.AVISTREAMHEADER)
        For I As Integer = 0 To cRiff.AVIStreamCount - 1
          If cRiff.AVIStreamData(I).fccType = "vids" Then
            vidTracks.Add(cRiff.AVIStreamData(I))
            If String.IsNullOrEmpty(ve) AndAlso Not String.IsNullOrEmpty(cRiff.AVIStreamData(I).fccHandler) Then ve = cRiff.AVIStreamData(I).fccHandler
            If cRiff.AVIStreamData(I).dwScale > 0 Then vr = Math.Round(cRiff.AVIStreamData(I).dwRate / cRiff.AVIStreamData(I).dwScale, 3) & " fps " & IIf(cRiff.AVIMainData.dwFlags And Seed.clsRIFF.AVIMAINHEADER_FLAGS.AVIF_ISINTERLEAVED, "Interleaved", "Progressive")
          ElseIf cRiff.AVIStreamData(I).fccType = "auds" Then
            audTracks.Add(cRiff.AVIStreamData(I))
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
        If LoadFFDShow() Then
          Dim ffChaps = ffAPI.ChaptersList
          SetCombos(vidTracks.Count > 1, audTracks.Count > 1, ffChaps.Count > 0, ffAPI.SubtitleStreams.Count > 1)
          If ffChaps.Count > 0 Then
            Dim chapterCollection As New List(Of ChapterListing)
            For Each chapter In ffChaps
              ChapterList.Add(chapter.Value)
              chapterCollection.Add(New ChapterListing(chapter.Key, chapter.Value, "und"))
            Next
            SpecificChapterList = chapterCollection
          End If
          If ffAPI.SubtitleStreams.Count > 0 Then
            SubWidth = 0
            For Each stream In ffAPI.SubtitleStreams
              Dim sStreamText As String = stream.Value.name
              If sStreamText = "Show Subtitles" Or sStreamText.StartsWith("[Local]") Then Continue For
              If sStreamText = "Hide Subtitles" Then sStreamText = "No subtitles"
              SubTrackList.Add(sStreamText, stream.Value.languageName)
              If stream.Value.enabled Then selSub = SubTrackList.Count - 1
            Next
          End If
        Else
          SetCombos(vidTracks.Count > 1, audTracks.Count > 1, False, False)
        End If
        For v As Integer = 0 To vidTracks.Count - 1
          Dim sV As String = "Video Track " & v + 1 & " (" & vidTracks(v).fccHandler & ")"
          If cRiff.AVIINFOData IsNot Nothing Then If cRiff.AVIINFOData.ContainsKey("IVS" & v + 1) Then sV &= " [" & cRiff.AVIINFOData("IVS" & v + 1) & "]"
          VidTrackList.Add(sV, vidTracks(v).wLanguage)
        Next
        For a As Integer = 0 To audTracks.Count - 1
          Dim sA As String = "Audio Track " & a + 1
          If cRiff.AVIINFOData IsNot Nothing Then If cRiff.AVIINFOData.ContainsKey("IAS" & a + 1) Then sA &= " [" & cRiff.AVIINFOData("IAS" & a + 1) & "]"
          AudTrackList.Add(sA, audTracks(a).wLanguage)
        Next
      End If
      SetProps(enc, chan, rate, sEnc, sChan, sRate)
    End Using
  End Sub

  Private Sub LoadData_Video_FFDShow(ByRef sTitleVal As String, ByRef sArtistVal As String, ByRef VidTrackList As Dictionary(Of String, String), ByRef selVid As Integer, ByRef AudTrackList As Dictionary(Of String, String), ByRef selAud As Integer, ByRef SubTrackList As Dictionary(Of String, String), ByRef selSub As Integer, ByRef ChapterList As List(Of String), ByRef selChapter As Integer)
    Dim ffChaps = ffAPI.ChaptersList
    SetCombos(False, ffAPI.AudioStreams.Count > 1, ffChaps.Count > 0, ffAPI.SubtitleStreams.Count > 1)
    If ffChaps.Count > 0 Then
      Dim chapterCollection As New List(Of ChapterListing)
      For Each chapter In ffChaps
        ChapterList.Add(chapter.Value)
        chapterCollection.Add(New ChapterListing(chapter.Key, chapter.Value, "und"))
      Next
      SpecificChapterList = chapterCollection
    End If
    If ffAPI.AudioStreams.Count > 0 Then
      For Each stream In ffAPI.AudioStreams
        AudTrackList.Add(stream.Value.name, stream.Value.languageName)
        If stream.Value.enabled Then selAud = AudTrackList.Count - 1
      Next
    End If
    If ffAPI.SubtitleStreams.Count > 0 Then
      For Each stream In ffAPI.SubtitleStreams
        Dim sStreamText As String = stream.Value.name
        If sStreamText = "Show Subtitles" Or sStreamText.StartsWith("[Local]") Then Continue For
        If sStreamText = "Hide Subtitles" Then sStreamText = "No subtitles"
        SubTrackList.Add(sStreamText, stream.Value.languageName)
        If stream.Value.enabled Then selSub = SubTrackList.Count - 1
      Next
    End If
    SetVideoResolution(-1, -1)
    Dim iQ As Long = ((New IO.FileInfo(mpPlayer.FileName)).Length * 8) / (mpPlayer.Duration)
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
  End Sub

  Private Sub LoadData_Audio(ByRef sTitleVal As String, ByRef sArtistVal As String)
    EnableScreenSaver(True)
    SetTabs(True, False)
    If Not mpPlayer.IsStreaming Then
      CleanupID3(mpPlayer.FileName)
    End If
    sTitleVal = GetTitle(mpPlayer.FileName, True, False)
    sArtistVal = GetTitle(mpPlayer.FileName, True, True)
    FileArt = GetArt(mpPlayer.FileName, False, False)
    Select Case IO.Path.GetExtension(mpPlayer.FileName).Substring(1).ToLower
      Case "mp3" ', "mpe", "mpg", "mpeg", "m1v", "mp2", "m2v", "mp2v", "mpv2", "mpa", "aac", "m2ts" , "m4a", "m4p", "m4v", "mp4"
        LoadData_Audio_MP3()
      Case "ogg", "flac"
        LoadData_Audio_Vorbis()
      Case "wav"
        LoadData_Audio_WAV()
      Case Else
        Dim iQ As Long = ((New IO.FileInfo(mpPlayer.FileName)).Length * 8) / (mpPlayer.Duration)
        SetProps(-1, -1, 0, , , KRater(iQ, "bps"))
    End Select
    SetCombos(False, False, False, False)
    If mpPlayer.Repeat Then
      cmdLoop.Image = My.Resources.button_loop_on
    Else
      cmdLoop.Image = My.Resources.button_loop_off
    End If
    cmdLoop.Tag = Nothing
    VidThumb = False
  End Sub

  Private Sub LoadData_Audio_MP3()
    Using cHeader As New Seed.clsHeaderLoader(mpPlayer.FileName)
      If cHeader.cMPEG IsNot Nothing Then
        Dim iQual, iChannels As Integer
        Select Case cHeader.RateFormat
          Case "ABR", "ABR (XING)", "2-Pass ABR (XING)" : iQual = 1
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
  End Sub

  Private Sub LoadData_Audio_Vorbis()
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
      Dim iQ As Long = ((New IO.FileInfo(mpPlayer.FileName)).Length * 8) / (mpPlayer.Duration)
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
  End Sub

  Private Sub LoadData_Audio_WAV()
    Dim iQ As Long = ((New IO.FileInfo(mpPlayer.FileName)).Length * 8) / (mpPlayer.Duration)
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
      cTask.Title = FileTitle
      cTask.Tooltip = FileTitle
      cTask.Icon = Me.Icon
      If frmFS IsNot Nothing AndAlso frmFS.Visible Then
        cTask.ShowClip(frmFS.pctVideo)
      ElseIf VidThumb Then
        cTask.ShowClip(mpPlayer)
      ElseIf FileArt IsNot Nothing AndAlso FileArt.Art IsNot Nothing Then
        cTask.ShowImage(FileArt.Art) 'TODO: THIS IS A THING
      Else
        cTask.ShowClip(pnlMain)
      End If
    End If
  End Sub

  Public ReadOnly Property TimeString As String
    Get
      If bCD Then
        Return ConvertTimeVal(cCD.TrackPositionSeconds) & "/" & ConvertTimeVal(cCD.TrackDurationSeconds)
      ElseIf Not String.IsNullOrEmpty(mpPlayer.FileName) Then
        If mpPlayer.IsFlash Then
          Return mpPlayer.Position & "/" & mpPlayer.Duration & " frames"
        ElseIf mpPlayer.IsStreaming Then
          Return "Streaming"
        Else
          Return ConvertTimeVal(mpPlayer.Position) & "/" & ConvertTimeVal(mpPlayer.Duration)
        End If
      Else
        Return "--:--"
      End If
    End Get
  End Property

  Private Sub DrawTitleArtist()
    Dim sTimeString As String = " (" & TimeString & ")"
    If clsGlass.IsCompositionEnabled And (bCD Or (Not String.IsNullOrEmpty(FileSubTitle) And Not String.IsNullOrEmpty(mpPlayer.FileName))) Then
      If String.IsNullOrEmpty(FileSubTitle) Then
        If String.IsNullOrEmpty(FileMainTitle) Then
          If Not Me.Text = "Lime Seed Media Player" & sTimeString Then
            Me.Text = "Lime Seed Media Player" & sTimeString
            SetTrayText(Me.Text)
            NoGlassText = True
            DrawNormal()
          End If
        Else
          If Not Me.Text = FileMainTitle & sTimeString Then
            Me.Text = FileMainTitle & sTimeString
            SetTrayText(Me.Text)
            NoGlassText = True
            DrawNormal()
          End If
        End If
      Else
        If String.IsNullOrEmpty(FileMainTitle) Then
          If Not Me.Text = FileSubTitle & sTimeString Then
            Me.Text = FileSubTitle & sTimeString
            SetTrayText(Me.Text)
            NoGlassText = True
            DrawNormal()
          End If
        Else
          If Not Me.Text = FileMainTitle Then Me.Text = FileMainTitle
          Dim sSubTitle As String = FileSubTitle & sTimeString
          NoGlassText = False
          If Not sSubTitle = lastCDSubTitle Then
            lastCDSubTitle = sSubTitle
            DrawGlassText(True)
          End If
          SetTrayText(FileMainTitle & " - " & FileSubTitle)
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
      SetTrayText(Me.Text)
      NoGlassText = True
      DrawNormal()
    End If
  End Sub

  Private Sub SetTrayText(Message As String)
    If Not trayIcon.Text = Message Then
      SetNotifyIconText(trayIcon, Message)
      If frmTI IsNot Nothing AndAlso Not frmTI.IsDisposed Then
        frmTI.GlassText = Message
      End If
    End If
  End Sub

  Private Sub SetNotifyIconText(ni As NotifyIcon, text As String)
    If text.Length > 127 Then
      If text.Contains("""") Then
        If text.Substring(0, 123).Count(Function(lookFor As Char) As Boolean
                                          If lookFor = """" Then Return True
                                          Return False
                                        End Function) Mod 2 = 1 Then
          text = text.Substring(0, 123) & "..."""
        End If
      Else
        text = text.Substring(0, 124) & "..."
      End If
    End If
    Dim t As Type = GetType(NotifyIcon)
    Dim hidden As Reflection.BindingFlags = Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance
    t.GetField("text", hidden).SetValue(ni, text)
    If CBool(t.GetField("added", hidden).GetValue(ni)) Then
      Try
        t.GetMethod("UpdateIcon", hidden).Invoke(ni, New Object() {True})
      Catch ex As Exception
      End Try
    End If
  End Sub

  Private Sub DrawGlassText(autoRedraw As Boolean)
    If Me.WindowState = FormWindowState.Minimized Then Return
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
      Dim sFileTime As String = "(0:00/0:00)"
      If TimeString = "Streaming" Then
        If mpPlayer.IsStreaming Then
          If mpPlayer.StreamInfo IsNot Nothing Then
            If Not String.IsNullOrEmpty(mpPlayer.StreamInfo.Album) Then
              sFileTime = mpPlayer.StreamInfo.Album
            Else
              sFileTime = TimeString
            End If
          Else
            sFileTime = TimeString
          End If
        Else
          sFileTime = TimeString
        End If
      Else
        sFileTime = TimeString
      End If
      Dim iTitleWidth As Integer = CreateGraphics.MeasureString(FileSubTitle, TextFont).Width + EdgeMargin
      Dim iTimeWidth As Integer = CreateGraphics.MeasureString("_" & sFileTime & "_", TextFont).Width + EdgeMargin

      If iTitleWidth + iTimeWidth > Me.DisplayRectangle.Width Then
        If Not autoRedraw Then
          If iGlassLastX >= 0 And Not bGlassLastLeft Then
            bGlassLastLeft = True
            iGlassLastX = 0
          ElseIf iGlassLastX <= Me.DisplayRectangle.Width - iTimeWidth - iTitleWidth And bGlassLastLeft Then
            bGlassLastLeft = False
            iGlassLastX = Me.DisplayRectangle.Width - iTimeWidth - iTitleWidth
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
            If bGlassLastLeft Then
              iGlassLastX -= iStep
            Else
              iGlassLastX += iStep
            End If
          End If
          If iGlassLastX > 0 Then iGlassLastX = 0
          If iGlassLastX < Me.DisplayRectangle.Width - iTimeWidth - iTitleWidth Then iGlassLastX = Me.DisplayRectangle.Width - iTimeWidth - iTitleWidth
        End If

        Dim TitleRect As New Drawing.Rectangle(iGlassLastX, 0, Me.DisplayRectangle.Width - iTimeWidth - iGlassLastX, BarHeight)
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
    Dim SelItem As Integer = SelectedPlayListItem
    If bCD Then
      pbProgress.Style = ProgressBarStyle.Continuous
      If cCD.CurrentTrack > 0 Then
        'If Not SelectedPlayListItem = cCD.CurrentTrack - 1 Then
        '  SelectedPlayListItem = cCD.CurrentTrack - 1
        '  InitialData()
        'End If
      Else
        'If Not SelectedPlayListItem = -1 Then
        '  SelectedPlayListItem = -1
        '  InitialData()
        'End If
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
      pbProgress.Style = ProgressBarStyle.Continuous
      SetTabs(False, False)
      If Not pbProgress.Maximum = 1000 Then
        If Not Me.Text = "Lime Seed Media Player" Then Me.Text = "Lime Seed Media Player"
        If pnlMain.Dock = DockStyle.None Then DrawGlassText(True)
        If Not pbProgress.Value = 0 Then pbProgress.Value = 0
        If Not pbProgress.Maximum = 1 Then pbProgress.Maximum = 1
      End If
      If pbProgress.Enabled Then pbProgress.Enabled = False
    Else
      If mpPlayer.IsStreaming Then
        pbProgress.Style = ProgressBarStyle.Marquee
        If mpPlayer.StreamInfo IsNot Nothing Then
          Dim sArtist As String = mpPlayer.StreamInfo.Artist
          Dim sTitle As String = mpPlayer.StreamInfo.Title
          SetTitleArtist(sTitle, sArtist)
          DrawTitleArtist()
          DrawGlassText(True)
          SetTabs(True, False)
          pbProgress.Value = 0
          pbProgress.Maximum = 100
          If pbProgress.Enabled Then pbProgress.Enabled = False
        Else
          SetTabs(True, False)
          pbProgress.Value = 0
          pbProgress.Maximum = 100
          If pbProgress.Enabled Then pbProgress.Enabled = False
        End If
      Else
        pbProgress.Style = ProgressBarStyle.Continuous
        If mpPlayer.HasVid Then
          SetTabs(False, True)
          If cmbChapters.Visible And Not cmbChapters.DroppedDown And cmbChapters.Tag Is Nothing Then
            cmbChapters.Tag = True
            If SpecificChapterList IsNot Nothing Then
              Dim lastVal As String = Nothing
              For Each chapter In SpecificChapterList
                If mpPlayer.Position >= chapter.StartIndex Then lastVal = chapter.ChapterName
              Next
              If String.IsNullOrEmpty(lastVal) Then
                cmbChapters.SelectedIndex = 0
              Else
                cmbChapters.Text = lastVal
              End If
            End If
            cmbChapters.Tag = Nothing
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
    End If
    If bCD Then
      If cCD.Status = Seed.clsAudioCD.PlayStatus.Playing Then
        SetPlayPause(False, TriState.True)
        SetStop(True)
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NORMAL)
      ElseIf cCD.Status = Seed.clsAudioCD.PlayStatus.Paused Then
        SetPlayPause(True, TriState.True)
        SetStop(True)
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_PAUSED)
      ElseIf cCD.Status = Seed.clsAudioCD.PlayStatus.Stopped Then
        SetPlayPause(True, TriState.True)
        SetStop(False)
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NOPROGRESS)
      End If
      cmdMenu.Visible = False
      'If cCD.CurrentTrack > 0 Then
      '  SetSelectedPlayListItem(cCD.CurrentTrack)
      'Else
      '  SetSelectedPlayListItem(-1)
      'End If
    ElseIf bDVD Then
      'SetSelectedPlayListItem(-1)
      Dim uOps = mpPlayer.GetDVDCurrentUOPS
      Dim bPaused As Boolean = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.PauseOn) = DirectShowLib.Dvd.ValidUOPFlag.PauseOn)
      Dim bResume As Boolean = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.Resume) = DirectShowLib.Dvd.ValidUOPFlag.Resume)
      Dim bStop As Boolean = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.Stop) = DirectShowLib.Dvd.ValidUOPFlag.Stop)
      SetPlayPause(Not bPaused, IIf(bResume, TriState.True, TriState.False))
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
      If mpPlayer.IsStreaming Then
        If mpPlayer.State = Seed.ctlSeed.MediaState.mStopped Then
          SetPlayPause(True, TriState.True)
          SetStop(False)
        Else
          SetPlayPause(False, TriState.False)
          SetStop(True)
        End If
        If cmdLoop.Enabled Then cmdLoop.Enabled = False
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NOPROGRESS)
      ElseIf mpPlayer.State = Seed.ctlSeed.MediaState.mPlaying Then
        SetPlayPause(False, TriState.True)
        SetStop(True)
        If Not cmdLoop.Enabled Then cmdLoop.Enabled = True
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NORMAL)
      ElseIf mpPlayer.State = Seed.ctlSeed.MediaState.mPaused Then
        SetPlayPause(True, TriState.True)
        SetStop(True)
        If Not cmdLoop.Enabled Then cmdLoop.Enabled = True
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_PAUSED)
      ElseIf mpPlayer.State = Seed.ctlSeed.MediaState.mStopped Then
        SetPlayPause(True, TriState.True)
        SetStop(False)
        If Not cmdLoop.Enabled Then cmdLoop.Enabled = True
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NOPROGRESS)
      ElseIf mpPlayer.State = Seed.ctlSeed.MediaState.mClosed Then
        SetPlayPause(True, TriState.False)
        SetStop(False)
        If cmdLoop.Enabled Then cmdLoop.Enabled = False
        SetTaskbarStyle(TaskbarLib.TBPFLAG.TBPF_NOPROGRESS)
      End If
      'SetSelectedPlayListItem(SelItem)
      cmdMenu.Visible = False
    End If
    If bDVD Then
      If tbsView.TabPages.Contains(tabPlayList) Then tbsView.TabPages.Remove(tabPlayList)
      Try
        Dim uOps = mpPlayer.GetDVDCurrentUOPS
        Dim bPrevious As Boolean = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.PlayPrevOrReplay_Chapter) = DirectShowLib.Dvd.ValidUOPFlag.PlayPrevOrReplay_Chapter)
        Dim bNext As Boolean = Not CBool((uOps And DirectShowLib.Dvd.ValidUOPFlag.PlayNextChapter) = DirectShowLib.Dvd.ValidUOPFlag.PlayNextChapter)
        If Not txtPlayListTitle.Text = (New IO.DriveInfo(mpPlayer.FileName.Substring(0, 3))).VolumeLabel & " (--:--/--:--)" Then txtPlayListTitle.Text = (New IO.DriveInfo(mpPlayer.FileName.Substring(0, 3))).VolumeLabel & " (--:--/--:--)"
        If Not txtPlayListTitle.Tag = (New IO.DriveInfo(mpPlayer.FileName.Substring(0, 3))).VolumeLabel Then txtPlayListTitle.Tag = (New IO.DriveInfo(mpPlayer.FileName.Substring(0, 3))).VolumeLabel
        If cmdLoopPL.Enabled Then cmdLoopPL.Enabled = False
        If cmdFindPL.Enabled Then cmdFindPL.Enabled = False
        If cmdShufflePL.Enabled Then cmdShufflePL.Enabled = False
        If cmdBackPL.Enabled <> bPrevious Then cmdBackPL.Enabled = bPrevious
        If cmdNextPL.Enabled <> bNext Then cmdNextPL.Enabled = bNext
        If cmdRemoveFromPL.Enabled Then cmdRemoveFromPL.Enabled = False
        If mnuTransferFile.Enabled Then mnuTransferFile.Enabled = False
        If cmdClearPL.Enabled Then cmdClearPL.Enabled = False
      Catch ex As Exception
        mnuCloseFile.PerformClick()
        cmdShufflePL.Tag = False
        cmdShufflePL.Image = My.Resources.pl_button_order
        If mnuPLDuplicates.Checked Then mnuPLDuplicates.Checked = False
        'dgvPlayList.Rows.Clear()
        PLItems.Clear()
      End Try
    Else
      If Not String.IsNullOrEmpty(mpPlayer.FileName) AndAlso mpPlayer.IsStreaming Then
        If tbsView.TabPages.Contains(tabPlayList) Then tbsView.TabPages.Remove(tabPlayList)
        If mpPlayer.StreamInfo IsNot Nothing Then
          If String.IsNullOrEmpty(mpPlayer.StreamInfo.Album) Then
            If Not txtPlayListTitle.Text = "Streaming (--:--/--:--)" Then txtPlayListTitle.Text = "Streaming (--:--/--:--)"
            If Not txtPlayListTitle.Tag = UNKNOWN_ALBUM Then txtPlayListTitle.Tag = UNKNOWN_ALBUM
          Else
            If Not txtPlayListTitle.Text = mpPlayer.StreamInfo.Album & " (--:--/--:--)" Then txtPlayListTitle.Text = mpPlayer.StreamInfo.Album & " (--:--/--:--)"
            If Not txtPlayListTitle.Tag = mpPlayer.StreamInfo.Album Then txtPlayListTitle.Tag = mpPlayer.StreamInfo.Album
          End If
          If cmdLoopPL.Enabled Then cmdLoopPL.Enabled = False
          If cmdFindPL.Enabled Then cmdFindPL.Enabled = False
          If cmdShufflePL.Enabled Then cmdShufflePL.Enabled = False
          If cmdBackPL.Enabled Then cmdBackPL.Enabled = False
          If cmdNextPL.Enabled Then cmdNextPL.Enabled = False
          If cmdRemoveFromPL.Enabled Then cmdRemoveFromPL.Enabled = False
          If mnuTransferFile.Enabled Then mnuTransferFile.Enabled = False
          If cmdClearPL.Enabled Then cmdClearPL.Enabled = False
        End If
      Else
        If Not tbsView.TabPages.Contains(tabPlayList) Then tbsView.TabPages.Add(tabPlayList)
        If dgvPlayList.Rows.Count > 0 Then

          Dim plCurrent, plTotal As Double
          For I As Integer = 0 To PLItems.Count - 1
            Dim rowData As PlayListItem = PLItems.PlayListItem(I)
            If rowData Is Nothing Then Continue For
            Dim myTimeVal As Double = rowData.Duration
            plTotal += myTimeVal
            If SelItem = I Then
              If bCD Then
                plCurrent += cCD.TrackPositionSeconds
              Else
                plCurrent += mpPlayer.Position
              End If
            End If
            If SelItem > I Then plCurrent += myTimeVal
          Next

          If txtPlayListTitle.ReadOnly Then
            Dim sPLTitle As String = txtPlayListTitle.Tag
            If bDefaultedPlayListTitle Then
              If PLItems.Count = 1 Then
                Dim rowData As PlayListItem = PLItems.PlayListItem(0)
                If rowData Is Nothing Then
                  sPLTitle = UNKNOWN_ALBUM
                ElseIf Not String.IsNullOrEmpty(rowData.Album) AndAlso Not rowData.Album = UNKNOWN_ALBUM Then
                  sPLTitle = rowData.Album
                ElseIf Not String.IsNullOrEmpty(rowData.Artist) AndAlso Not rowData.Artist = UNKNOWN_ARTIST Then
                  sPLTitle = rowData.Title
                Else
                  sPLTitle = UNKNOWN_ALBUM
                End If
              Else
                Dim allArtist As String = Nothing
                Dim allAlbum As String = Nothing
                For I As Integer = 0 To PLItems.Count - 1
                  Dim rowData As PlayListItem = PLItems.PlayListItem(I)
                  If rowData Is Nothing Then Continue For
                  If String.IsNullOrEmpty(allArtist) Then allArtist = rowData.Artist
                  If String.IsNullOrEmpty(allAlbum) Then allAlbum = rowData.Album
                  If (Not String.IsNullOrEmpty(allArtist)) And (Not String.IsNullOrEmpty(allAlbum)) Then Exit For
                Next
                If Not String.IsNullOrEmpty(allAlbum) Then If allAlbum.ToLowerInvariant.Contains(" disc ") AndAlso IsNumeric(allAlbum.Substring(allAlbum.ToLowerInvariant.LastIndexOf(" disc ") + 6)) Then allAlbum = allAlbum.Substring(0, allAlbum.ToLowerInvariant.LastIndexOf(" disc "))
                For I As Integer = 0 To PLItems.Count - 1
                  Dim rowData As PlayListItem = PLItems.PlayListItem(I)
                  If rowData Is Nothing Then Continue For
                  If Not String.IsNullOrEmpty(allAlbum) And Not String.IsNullOrEmpty(rowData.Album) Then
                    Dim sMatchVal As String = Nothing
                    If MatchNames_Minimal(allAlbum, rowData.Album, sMatchVal) Then
                      If Not String.IsNullOrEmpty(sMatchVal) Then allAlbum = sMatchVal
                    Else
                      allAlbum = Nothing
                    End If
                  End If
                  If Not String.IsNullOrEmpty(allArtist) And Not String.IsNullOrEmpty(rowData.Artist) Then
                    Dim sMatchVal As String = Nothing
                    If MatchNames_Minimal(allArtist, rowData.Artist, sMatchVal) Then
                      If Not String.IsNullOrEmpty(sMatchVal) Then allArtist = sMatchVal
                    Else
                      allArtist = Nothing
                    End If
                  End If
                  If (String.IsNullOrEmpty(allArtist)) And (String.IsNullOrEmpty(allAlbum)) Then Exit For
                Next
                If Not String.IsNullOrEmpty(allAlbum) Then
                  If allAlbum.ToLowerInvariant.Contains(" disc ") AndAlso IsNumeric(allAlbum.Substring(allAlbum.ToLowerInvariant.LastIndexOf(" disc ") + 6)) Then allAlbum = allAlbum.Substring(0, allAlbum.ToLowerInvariant.LastIndexOf(" disc "))
                  sPLTitle = allAlbum.Trim
                ElseIf Not String.IsNullOrEmpty(allArtist) Then
                  sPLTitle = allArtist.Trim
                Else
                  sPLTitle = UNKNOWN_ALBUM
                End If
              End If

              If Not txtPlayListTitle.Tag = sPLTitle Then txtPlayListTitle.Tag = sPLTitle
            End If
            sPLTitle &= " (" & ConvertTimeVal(plCurrent) & "/" & ConvertTimeVal(plTotal) & ")"
            If Not txtPlayListTitle.Text = sPLTitle Then txtPlayListTitle.Text = sPLTitle
          End If
          If Not cmdLoopPL.Enabled Then cmdLoopPL.Enabled = True
          If Not cmdFindPL.Enabled Then cmdFindPL.Enabled = True
          If Not cmdShufflePL.Enabled Then cmdShufflePL.Enabled = True
          If Not cmdBackPL.Enabled = (SelItem > 0) Then cmdBackPL.Enabled = (SelItem > 0)
          If Not cmdNextPL.Enabled = (SelItem < dgvPlayList.Rows.Count - 1) Then cmdNextPL.Enabled = (SelItem < dgvPlayList.Rows.Count - 1)
          If Not cmdRemoveFromPL.Enabled Then cmdRemoveFromPL.Enabled = True
          If Not mnuTransferFile.Enabled Then mnuTransferFile.Enabled = True
          If Not cmdClearPL.Enabled Then cmdClearPL.Enabled = True
        Else
          If Not txtPlayListTitle.Text = UNKNOWN_ALBUM & " (--:--/--:--)" Then txtPlayListTitle.Text = UNKNOWN_ALBUM & " (--:--/--:--)"
          If Not txtPlayListTitle.Tag = UNKNOWN_ALBUM Then txtPlayListTitle.Tag = UNKNOWN_ALBUM
          If cmdLoopPL.Enabled Then cmdLoopPL.Enabled = False
          If cmdFindPL.Enabled Then cmdFindPL.Enabled = False
          If cmdShufflePL.Enabled Then cmdShufflePL.Enabled = False
          If cmdBackPL.Enabled Then cmdBackPL.Enabled = False
          If cmdNextPL.Enabled Then cmdNextPL.Enabled = False
          If cmdRemoveFromPL.Enabled Then cmdRemoveFromPL.Enabled = False
          If mnuTransferFile.Enabled Then mnuTransferFile.Enabled = False
          If cmdClearPL.Enabled Then cmdClearPL.Enabled = False
        End If
      End If
    End If
    If VisualStyles.VisualStyleInformation.DisplayName = "Aero style" And TaskbarFinder.TaskbarVisible Then
      If taskBar Is Nothing Then taskBar = New TaskbarLib.TaskbarList
      If cTask Is Nothing Then
        cTask = New TaskbarController(Me.Handle,
                                      New ThumbnailToolBarButtons(
                                        New ThumbnailToolBarButtons.ButtonAndToolTip(ImgToIco(My.Resources.pl_button_back), "Back"),
                                        New ThumbnailToolBarButtons.ButtonAndToolTip(ImgToIco(My.Resources.button_play), "Play"),
                                        New ThumbnailToolBarButtons.ButtonAndToolTip(ImgToIco(My.Resources.button_pause), "Pause"),
                                        New ThumbnailToolBarButtons.ButtonAndToolTip(ImgToIco(My.Resources.button_stop), "Stop"),
                                        New ThumbnailToolBarButtons.ButtonAndToolTip(ImgToIco(My.Resources.pl_button_next), "Next")), Me.Icon, Me.Text, Me.Text)
        cTask.ShowClip(pnlMain)
      End If
    End If
    If cTask IsNot Nothing Then
      If Not cTask.BackEnabled = cmdBackPL.Enabled Then cTask.BackEnabled = cmdBackPL.Enabled
      If Not cTask.NextEnabled = cmdNextPL.Enabled Then cTask.NextEnabled = cmdNextPL.Enabled
    End If
    If Not cmdFullScreen.Enabled = mpPlayer.HasVid Or methodDraw IsNot Nothing Then cmdFullScreen.Enabled = mpPlayer.HasVid Or methodDraw IsNot Nothing
    If Me.Visible Then
      iEveryHalfMinuteTimer += 1
      If iEveryHalfMinuteTimer = 200 Then
        iEveryHalfMinuteTimer = 0
        CheckDiscs()
        If mpPlayer.HasVid Then
          If LoadFFDShow() Then SetFFDShowVals()
        End If
      ElseIf iEveryHalfMinuteTimer Mod 10 = 0 And bDVD Then
        ThreadedInitial()
      End If
    End If
    If Environment.TickCount - lastPoolSave > 30 * 1000 Then
      SaveTempPL(False)
      lastPoolSave = Environment.TickCount
    ElseIf SelItem > -1 AndAlso Environment.TickCount - lastPoolSave2 > 1000 Then
      SaveTempPL(True)
      lastPoolSave2 = Environment.TickCount
    End If
  End Sub

  Private Function MatchNames_Minimal(NameA As String, NameB As String, ByRef Match As String) As Boolean
    If NameA.ToLowerInvariant = NameB.ToLowerInvariant Then Match = NameA : Return True
    If NameA.ToLowerInvariant.StartsWith(NameB.ToLowerInvariant) Then Match = NameB : Return True
    If NameB.ToLowerInvariant.StartsWith(NameA.ToLowerInvariant) Then Match = NameA : Return True
    Dim matchData As String = Nothing
    For I As Integer = 0 To NameA.Length - 1
      If Char.ToLowerInvariant(NameA(I)) = Char.ToLowerInvariant(NameB(I)) Then
        matchData &= NameA(I)
      Else
        Exit For
      End If
    Next
    If String.IsNullOrEmpty(matchData) Then Return False
    If matchData.Length < 3 Then Return False
    Match = matchData : Return True
  End Function

  'Private Function MatchNames_Full(NameA As String, NameB As String, ByRef Match As String) As Boolean
  '  If NameA.ToLowerInvariant = NameB.ToLowerInvariant Then Match = NameA : Return True
  '  'If NameA.ToLowerInvariant.StartsWith(NameB.ToLowerInvariant) Then Match = NameA : Return True
  '  'If NameB.ToLowerInvariant.StartsWith(NameA.ToLowerInvariant) Then Match = NameB : Return True
  '  If NameA.ToLowerInvariant.Contains(NameB.ToLowerInvariant) Then Match = NameA : Return True
  '  If NameB.ToLowerInvariant.Contains(NameA.ToLowerInvariant) Then Match = NameB : Return True
  '  If (NameA = "Various Artists" Or NameA = "Unknown Artist") And (Not NameB = "Various Artists" And Not NameB = "Unknown Artist") Then Match = NameB : Return True
  '  If (Not NameA = "Various Artists" And Not NameA = "Unknown Artist") And (NameB = "Various Artists" Or NameB = "Unknown Artist") Then Match = NameA : Return True
  '  Dim splitA As New List(Of String)
  '  If NameA.Contains("/") Or NameA.Contains(";") Or NameA.Contains(",") Or NameA.ToLower.Contains(" vs ") Or NameA.ToLower.Contains(" vs. ") Or NameA.Contains(" versus ") Or NameA.ToLower.Contains(" and ") Or NameA.Contains(" & ") Then
  '    Dim sTmp As String = Nothing
  '    For I As Integer = 0 To NameA.Length - 1
  '      If NameA(I) = "/"c Or NameA(I) = ";"c Or NameA(I) = ","c Then
  '        If Not String.IsNullOrEmpty(sTmp) Then splitA.Add(sTmp.Trim)
  '        sTmp = Nothing
  '      ElseIf NameA.Length > I + 2 AndAlso NameA.Substring(I, 3) = " & " Then
  '        If Not String.IsNullOrEmpty(sTmp) Then splitA.Add(sTmp.Trim)
  '        sTmp = Nothing
  '        I += 2
  '      ElseIf NameA.Length > I + 3 AndAlso NameA.Substring(I, 4) = " vs " Then
  '        If Not String.IsNullOrEmpty(sTmp) Then splitA.Add(sTmp.Trim)
  '        sTmp = Nothing
  '        I += 3
  '      ElseIf NameA.Length > I + 4 AndAlso (NameA.Substring(I, 5) = " vs. " Or NameA.Substring(I, 5) = " and ") Then
  '        If Not String.IsNullOrEmpty(sTmp) Then splitA.Add(sTmp.Trim)
  '        sTmp = Nothing
  '        I += 4
  '      ElseIf NameA.Length > I + 7 AndAlso NameA.Substring(I, 8) = " versus " Then
  '        If Not String.IsNullOrEmpty(sTmp) Then splitA.Add(sTmp.Trim)
  '        sTmp = Nothing
  '        I += 7
  '      Else
  '        sTmp &= NameA(I)
  '      End If
  '    Next
  '    If (Not String.IsNullOrEmpty(sTmp)) AndAlso (Not sTmp = "Various Artists" And Not sTmp = "Unknown Artist") Then splitA.Add(sTmp.Trim)
  '    sTmp = Nothing
  '  End If
  '  Dim splitB As New List(Of String)
  '  If NameB.Contains("/") Or NameB.Contains(";") Or NameB.Contains(",") Or NameB.ToLower.Contains(" vs ") Or NameB.ToLower.Contains(" vs. ") Or NameB.Contains(" versus ") Or NameB.ToLower.Contains(" and ") Or NameB.Contains(" & ") Then
  '    Dim sTmp As String = Nothing
  '    For I As Integer = 0 To NameB.Length - 1
  '      If NameB(I) = "/"c Or NameB(I) = ";"c Or NameB(I) = ","c Then
  '        If Not String.IsNullOrEmpty(sTmp) Then splitB.Add(sTmp.Trim)
  '        sTmp = Nothing
  '      ElseIf NameB.Length > I + 2 AndAlso NameB.Substring(I, 3) = " & " Then
  '        If Not String.IsNullOrEmpty(sTmp) Then splitB.Add(sTmp.Trim)
  '        sTmp = Nothing
  '        I += 2
  '      ElseIf NameB.Length > I + 3 AndAlso NameB.Substring(I, 4) = " vs " Then
  '        If Not String.IsNullOrEmpty(sTmp) Then splitB.Add(sTmp.Trim)
  '        sTmp = Nothing
  '        I += 3
  '      ElseIf NameB.Length > I + 4 AndAlso (NameB.Substring(I, 5) = " vs. " Or NameB.Substring(I, 5) = " and ") Then
  '        If Not String.IsNullOrEmpty(sTmp) Then splitB.Add(sTmp.Trim)
  '        sTmp = Nothing
  '        I += 4
  '      ElseIf NameB.Length > I + 7 AndAlso NameB.Substring(I, 8) = " versus " Then
  '        If Not String.IsNullOrEmpty(sTmp) Then splitB.Add(sTmp.Trim)
  '        sTmp = Nothing
  '        I += 7
  '      Else
  '        sTmp &= NameB(I)
  '      End If
  '    Next
  '    If (Not String.IsNullOrEmpty(sTmp)) AndAlso (Not sTmp = "Various Artists" And Not sTmp = "Unknown Artist") Then splitB.Add(sTmp.Trim)
  '    sTmp = Nothing
  '  End If
  '  Dim uniqueNames As New List(Of String)
  '  Dim sharedNames As New List(Of String)
  '  For Each sA As String In splitA
  '    Dim isUnique As Boolean = True
  '    Dim sT As String = sA
  '    For Each sB As String In splitB
  '      Dim sTmp As String = Nothing
  '      If MatchNames_FullChild(sA, sB, sTmp) Then
  '        If sharedNames.Count > 0 Then
  '          For S As Integer = 0 To sharedNames.Count - 1
  '            Dim sTm2 As String = Nothing
  '            If Not MatchNames_FullChild(sTmp, sharedNames(S), sTm2) Then
  '              sharedNames.Add(sTmp)
  '            Else
  '              sharedNames(S) = sTm2
  '            End If
  '          Next
  '        Else
  '          sharedNames.Add(sTmp)
  '        End If
  '        isUnique = False
  '      End If
  '    Next
  '    If uniqueNames.Count > 0 Then
  '      For U As Integer = 0 To uniqueNames.Count - 1
  '        Dim sTmp As String = Nothing
  '        If MatchNames_FullChild(sA, uniqueNames(U), sTmp) Then
  '          uniqueNames(U) = sTmp
  '          isUnique = False
  '        End If
  '      Next
  '    End If
  '    If isUnique Then uniqueNames.Add(sT)
  '  Next
  '  For Each sB As String In splitB
  '    Dim isUnique As Boolean = True
  '    Dim sT As String = sB
  '    For Each sA As String In splitA
  '      Dim sTmp As String = Nothing
  '      If MatchNames_FullChild(sB, sA, sTmp) Then
  '        If sharedNames.Count > 0 Then
  '          For S As Integer = 0 To sharedNames.Count - 1
  '            Dim sTm2 As String = Nothing
  '            If Not MatchNames_FullChild(sTmp, sharedNames(S), sTm2) Then
  '              sharedNames.Add(sTmp)
  '            Else
  '              sharedNames(S) = sTm2
  '            End If
  '          Next
  '        End If
  '        isUnique = False
  '      End If
  '    Next
  '    If uniqueNames.Count > 0 Then
  '      For U As Integer = 0 To uniqueNames.Count - 1
  '        Dim sTmp As String = Nothing
  '        If MatchNames_FullChild(sB, uniqueNames(U), sTmp) Then
  '          uniqueNames(U) = sTmp
  '          isUnique = False
  '        End If
  '      Next
  '    End If
  '    If isUnique Then uniqueNames.Add(sT)
  '  Next
  '  If sharedNames.Count > 0 Or uniqueNames.Count > 0 Then
  '    Dim sAllNames As String = Nothing
  '    If sharedNames.Count > 0 Then sAllNames = Join(sharedNames.ToArray, ", ")
  '    If uniqueNames.Count > 0 Then
  '      If String.IsNullOrEmpty(sAllNames) Then
  '        sAllNames = Join(uniqueNames.ToArray, ", ")
  '      Else
  '        sAllNames &= ", " & Join(uniqueNames.ToArray, ", ")
  '      End If
  '    End If
  '    Match = sAllNames
  '    Return True
  '  End If
  '  Return False
  'End Function

  'Private Function MatchNames_FullChild(NameA As String, NameB As String, ByRef Match As String) As Boolean
  '  If NameA.ToLowerInvariant = NameB.ToLowerInvariant Then Match = NameA : Return True
  '  If NameA.ToLowerInvariant.StartsWith(NameB.ToLowerInvariant) Then Match = NameA : Return True
  '  If NameB.ToLowerInvariant.StartsWith(NameA.ToLowerInvariant) Then Match = NameB : Return True
  '  'Debug.Print("Failing to compare " & NameA & " and " & NameB & "...")
  '  Return False
  'End Function

  Private Sub tmrVis_Tick(sender As System.Object, e As System.EventArgs) Handles tmrVis.Tick
    If volDevice Is Nothing Then Return
    tmrVis.Enabled = False
    Dim Channels As Integer = -1
    Try
      Channels = volDevice.AudioMeterInformation.PeakValues.Count
    Catch ex As Exception
      Channels = -1
      SetBeatDevice()
      Try
        Channels = volDevice.AudioMeterInformation.PeakValues.Count
      Catch ex2 As Exception
        Channels = -1
      End Try
    End Try
    If Channels < 1 Then
      If pctBeat.Image IsNot Nothing Then
        pctBeat.Image.Dispose()
        pctBeat.Image = Nothing
      End If
      If ttDisp.GetToolTip(pctBeat) = "No Output Channels!" Then ttDisp.SetToolTip(pctBeat, "No Output Channels!")
      tmrVis.Enabled = True
      Return
    End If
    Dim ChanVals(Channels - 1) As Double
    For I As Integer = 0 To Channels - 1
      Try
        ChanVals(I) = volDevice.AudioMeterInformation.PeakValues(I) * 100.0#
      Catch ex As Exception
        ChanVals(I) = 0.0#
      End Try
    Next
    If pctBeat.Image IsNot Nothing Then pctBeat.Image.Dispose()
    pctBeat.Image = DrawChannelLevels(Channels, ChanVals, pctBeat.Tag)
    If ttDisp.GetToolTip(pctBeat) <> "Output Channels: " & Channels Then ttDisp.SetToolTip(pctBeat, "Output Channels: " & Channels)
    If methodDraw IsNot Nothing Then
      If frmFS.Visible And Not mpPlayer.HasVid Then
        If Not frmFS.pctVideo.SizeMode = PictureBoxSizeMode.CenterImage Then frmFS.pctVideo.SizeMode = PictureBoxSizeMode.CenterImage
        If frmFS.pctVideo.DisplayRectangle.Height > 0 And frmFS.pctVideo.DisplayRectangle.Width > 0 Then
          If frmFS.pctVideo.Image IsNot Nothing Then frmFS.pctVideo.Image.Dispose()
          Try
            frmFS.pctVideo.Image = CType(methodDraw.Invoke(objDraw, New Object() {Channels, ChanVals, frmFS.pctVideo.DisplayRectangle.Size}), Drawing.Image)
          Catch ex As Exception
          End Try
        End If
      Else
        If Not pctAlbumArt.SizeMode = PictureBoxSizeMode.CenterImage Then pctAlbumArt.SizeMode = PictureBoxSizeMode.CenterImage
        If pctAlbumArt.DisplayRectangle.Width > 0 And pctAlbumArt.DisplayRectangle.Height > 0 Then
          If pctAlbumArt.Image IsNot Nothing Then pctAlbumArt.Image.Dispose()
          Try
            pctAlbumArt.Image = CType(methodDraw.Invoke(objDraw, New Object() {Channels, ChanVals, pctAlbumArt.DisplayRectangle.Size}), Drawing.Image)
          Catch ex As Exception
          End Try
        End If
      End If
    End If
    tmrVis.Enabled = True
  End Sub

  Private Sub SetVisualizations()
    tmrVis.Enabled = False
    If methodDraw IsNot Nothing Then methodDraw = Nothing
    If objDraw IsNot Nothing Then objDraw = Nothing
    If My.Settings.Visualization <> "None" Then
      Dim selPath As String = IO.Path.Combine(Application.StartupPath, "Visualizations", My.Settings.Visualization & ".dll")
      If IO.File.Exists(selPath) Then
        Dim assem As System.Reflection.Assembly = System.Reflection.Assembly.Load(IO.File.ReadAllBytes(selPath))
        For Each typeT As Type In assem.GetTypes
          methodDraw = typeT.GetMethod("Draw")
          If methodDraw Is Nothing Then Continue For
          objDraw = Activator.CreateInstance(typeT)
          If My.Settings.Visualization_Rate < 6 Then My.Settings.Visualization_Rate = 6
          If My.Settings.Visualization_Rate > 120 Then My.Settings.Visualization_Rate = 120
          tmrVis.Interval = Int(1000 / My.Settings.Visualization_Rate)
          tmrVis.Enabled = True
          Return
        Next
      End If
    End If
    tmrVis.Enabled = True
    If pctAlbumArt.Image IsNot Nothing Then
      pctAlbumArt.Image.Dispose()
      pctAlbumArt.Image = Nothing
    End If
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
      If IO.File.Exists(IO.Path.Combine(Application.StartupPath, "lame_enc.dll")) Then IO.File.Delete(IO.Path.Combine(Application.StartupPath, "lame_enc.dll"))
      If Environment.Is64BitProcess Then
        IO.File.WriteAllBytes(IO.Path.Combine(Application.StartupPath, "lame_enc.dll"), My.Resources.lame_enc_x64)
      Else
        IO.File.WriteAllBytes(IO.Path.Combine(Application.StartupPath, "lame_enc.dll"), My.Resources.lame_enc_x86)
      End If
      MediaClientSetup(True)
      FFDShowRemote(True)
      If IO.File.Exists(IO.Path.Combine(Application.StartupPath, "LSFA.exe")) Then Process.Start(IO.Path.Combine(Application.StartupPath, "LSFA.exe"), "Associate: MPC AC3 AIF ASF AU MID APE FLAC OFR TTA WAV AVI DIVX IVF MKV MKA FLV SPL SWF CDA DVD MPE MPG M1V MP2 M2V MP3 MPA AAC M2TS M4A M4P M4V OGG OGM MOV DV 3GP 3G2 RA RM RV VFW WMP WM WMA WMV M3U PLS LLPL DIR ")
      Me.Close()
    ElseIf Command() = "/uninstall" Then
      MediaClientSetup(False)
      FFDShowRemote(False)
      If IO.File.Exists(IO.Path.Combine(Application.StartupPath, "LSFA.exe")) Then Process.Start(IO.Path.Combine(Application.StartupPath, "LSFA.exe"), "Associate: ")
      Me.Close()
    Else
      volControl.SetVolume(90)
      trayIcon.Icon = My.Resources.tray
      SetTrayText(My.Application.Info.Title)
      trayIcon.Visible = False
      frmText.SetText("Loading", DragDropEffects.None)
      frmText.Hide()

      If VisualStyles.VisualStyleInformation.DisplayName = "Aero style" And TaskbarFinder.TaskbarVisible Then
        taskBar = New TaskbarLib.TaskbarList
        cTask = New TaskbarController(Me.Handle,
                                      New ThumbnailToolBarButtons(
                                        New ThumbnailToolBarButtons.ButtonAndToolTip(ImgToIco(My.Resources.pl_button_back), "Back"),
                                        New ThumbnailToolBarButtons.ButtonAndToolTip(ImgToIco(My.Resources.button_play), "Play"),
                                        New ThumbnailToolBarButtons.ButtonAndToolTip(ImgToIco(My.Resources.button_pause), "Pause"),
                                        New ThumbnailToolBarButtons.ButtonAndToolTip(ImgToIco(My.Resources.button_stop), "Stop"),
                                        New ThumbnailToolBarButtons.ButtonAndToolTip(ImgToIco(My.Resources.pl_button_next), "Next")), Me.Icon, Me.Text, Me.Text)
        cTask.ShowClip(pnlMain)
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
      Dim tX, tY As Integer
      tX = My.Settings.Location.X
      tY = My.Settings.Location.Y
      If tX < 0 Then tX = 0
      If tX > My.Computer.Screen.Bounds.Width - Me.MinimumSize.Width Then tX = My.Computer.Screen.Bounds.Width - Me.MinimumSize.Width
      If tY < 0 Then tY = 0
      If tY > My.Computer.Screen.Bounds.Height - Me.MinimumSize.Height Then tY = My.Computer.Screen.Bounds.Height - Me.MinimumSize.Height
      If Me.instanceID > 0 Then
        tX += (Me.instanceID * 20)
        tY += (Me.instanceID * 20)
      End If
      Me.Location = New Drawing.Point(tX, tY)
      Me.Size = Me.MinimumSize
      audDeviceName = My.Settings.Device
      mpPlayer.AudioDevice = audDeviceName
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
        If Me.Tag <> "NOCMD" Then Debug.Print("Some command? " & Me.Tag)
        Me.Tag = Nothing
      End If
      InitialData(Nothing)
      SetTabs(False, False)
      CheckDiscs()
      DrawGlassText(True)
      If My.Settings.Gamepad Then joyPad = New clsJoyDetection
      LabelShortcuts()
      SetBeatDevice()
      SetVisualizations()
    End If
  End Sub

  Private Sub SetBeatDevice()
    Try
      Dim devEnum As New CoreAudioApi.MMDeviceEnumerator
      Dim devEnum2 = devEnum.EnumerateAudioEndPoints(CoreAudioApi.EDataFlow.eRender, CoreAudioApi.ERole.eMultimedia)
      Dim bFound As Boolean = False
      For I As Integer = 0 To devEnum2.Count - 1
        If devEnum2.Item(I).FriendlyName = audDeviceName Or audDeviceName.StartsWith(devEnum2.Item(I).FriendlyName) Then
          bFound = True
          volDevice = devEnum2.Item(I)
          Exit For
        End If
      Next
      If Not bFound Then
        volDevice = devEnum.GetDefaultAudioEndpoint(CoreAudioApi.EDataFlow.eRender, CoreAudioApi.ERole.eMultimedia)
      End If
    Catch ex As Exception
      Debug.Print("Beat Device Error: " & ex.ToString)
    End Try
  End Sub

  Private Sub frmMain_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
    Randomize()
    Dim commands As ObjectModel.Collection(Of String) = tmrCommandCycle.Tag
    If commands IsNot Nothing AndAlso commands.Count = 0 And instanceID = 0 Then LoadTempPL()
    tmrUpdate.Start()
  End Sub

  Public Sub StartupRun(Commands As Collections.ObjectModel.ReadOnlyCollection(Of String))
    If Commands.Count = 0 Then Return
    If Commands.Count = 1 Then
      Dim sCmd As String = Commands(0)
      If sCmd.ToLower = "/min" Or sCmd.ToLower = "-min" Then
        trayIcon.Visible = True
        If Not Me.WindowState = FormWindowState.Normal Then Me.WindowState = FormWindowState.Normal
        Me.Hide()
        Return
      End If
      If sCmd.StartsWith("""") And sCmd.EndsWith("""") Then sCmd = sCmd.Substring(1, sCmd.Length - 2)
      If mpPlayer.State = Seed.ctlSeed.MediaState.mClosed Then
        If IO.File.Exists(sCmd) Then
          Select Case IO.Path.GetExtension(sCmd).ToLower
            Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sCmd, True)
            Case Else
              mpPlayer.SetNoQueue()
              OpenFile(sCmd, True)
              ThreadedInitial()
          End Select
        ElseIf IO.Directory.Exists(sCmd) Then
          If sCmd.EndsWith("VIDEO_TS") Then
            OpenDVD(sCmd)
          Else
            txtPlayListTitle.Tag = IO.Path.GetFileName(sCmd)
            bDefaultedPlayListTitle = False
            AddDirToPlayListAndMaybePlay(sCmd)
          End If
        End If
      Else
        If IO.File.Exists(sCmd) Then
          Select Case IO.Path.GetExtension(sCmd).ToLower
            Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sCmd)
            Case Else
              If dgvPlayList.Rows.Count > 0 Then
                AddToPlayList(sCmd)
              Else
                OpenFile(sCmd, True)
                ThreadedInitial()
              End If
          End Select
        ElseIf IO.Directory.Exists(sCmd) Then
          If sCmd.EndsWith("VIDEO_TS") Then
            AddToPlayList(sCmd)
          Else
            txtPlayListTitle.Tag = IO.Path.GetFileName(sCmd)
            bDefaultedPlayListTitle = False
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
          If IO.File.Exists(sCmd) Then
            Select Case IO.Path.GetExtension(sCmd).ToLower
              Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sCmd)
              Case Else
                AddToPlayList(sCmd, , , False)
            End Select
          ElseIf IO.Directory.Exists(sCmd) Then
            txtPlayListTitle.Tag = IO.Path.GetFileName(sCmd)
            bDefaultedPlayListTitle = True
            AddDirToPlayListAndMaybePlay(sCmd)
          End If
        Next
        If dgvPlayList.Rows.Count > 0 Then
          dgvPlayList.Rows(0).Selected = True
          StartPlayList()
        End If
      Else
        For I As Integer = 0 To Commands.Count - 1
          Dim sCmd As String = Commands(I)
          If sCmd.StartsWith("""") And sCmd.EndsWith("""") Then sCmd = sCmd.Substring(1, sCmd.Length - 2)
          If IO.File.Exists(sCmd) Then
            Select Case IO.Path.GetExtension(sCmd).ToLower
              Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sCmd)
              Case Else : AddToPlayList(sCmd, , , False)
            End Select
          ElseIf IO.Directory.Exists(sCmd) Then
            txtPlayListTitle.Tag = IO.Path.GetFileName(sCmd)
            bDefaultedPlayListTitle = True
            AddDirToPlayListAndMaybePlay(sCmd)
          End If
        Next
      End If
    End If
  End Sub

  Private Sub tmrCommandCycle_Tick(sender As System.Object, e As System.EventArgs) Handles tmrCommandCycle.Tick
    tmrCommandCycle.Stop()
    If tmrCommandCycle.Tag Is Nothing Then Return
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
      dragDropInvoker.BeginInvoke(Data, e.Effect, Nothing, Nothing)
    Else
      e.Effect = DragDropEffects.None
    End If
  End Sub

  Private Delegate Sub DragDropFilesInvoker(Data As Object, Effect As DragDropEffects)
  Private Sub DragDropFiles(Data As Object, Effect As DragDropEffects)
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New DragDropFilesInvoker(AddressOf DragDropFiles), Data, Effect)
      Catch ex As Exception
      End Try
      Return
    End If
    If Data Is Nothing Then Return
    Dim sData() As String = Data
    If sData.Length = 0 Then Return
    If sData.Length = 1 Then
      Dim sPath As String = sData(0)
      Select Case Effect
        Case DragDropEffects.All, DragDropEffects.Copy 'ADD
          If IO.File.Exists(sPath) Then
            If dgvPlayList.RowCount = 0 And Not String.IsNullOrEmpty(mpPlayer.FileName) Then
              AddToPlayList(mpPlayer.FileName)
              SelectedPlayListItem = 0
            End If
            Select Case IO.Path.GetExtension(sPath).ToLower
              Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sPath)
              Case Else : AddToPlayList(sPath)
            End Select
          ElseIf IO.Directory.Exists(sPath) Then
            If dgvPlayList.RowCount = 0 And Not String.IsNullOrEmpty(mpPlayer.FileName) Then
              AddToPlayList(mpPlayer.FileName)
              SelectedPlayListItem = 0
            End If
            AddDirToPlayList(sPath)
          Else
            PLShowMissing(sPath)
          End If
        Case DragDropEffects.Move 'PLAY
          If IO.File.Exists(sPath) Then
            Select Case IO.Path.GetExtension(sPath).ToLower
              Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sPath, True)
              Case Else
                OpenFile(sPath, True)
                ThreadedInitial()
            End Select
          ElseIf IO.Directory.Exists(sPath) Then
            ' TODO: Make sure "AndMaybePlay" is the right call
            AddDirToPlayListAndMaybePlay(sPath)
          End If
        Case DragDropEffects.Link 'ADD AND PLAY
          If dgvPlayList.RowCount = 0 Then
            If Not String.IsNullOrEmpty(mpPlayer.FileName) Then
              AddToPlayList(mpPlayer.FileName)
              SelectedPlayListItem = 0
              If IO.File.Exists(sPath) Then
                Select Case IO.Path.GetExtension(sPath).ToLower
                  Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sPath)
                  Case Else : AddToPlayList(sPath)
                End Select
              ElseIf IO.Directory.Exists(sPath) Then
                AddDirToPlayList(sPath)
              Else
                PLShowMissing(sPath)
              End If
            Else
              If IO.File.Exists(sPath) Then
                Select Case IO.Path.GetExtension(sPath).ToLower
                  Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sPath, True)
                  Case Else
                    AddToPlayList(sPath)
                    StartPlayList()
                End Select
              ElseIf IO.Directory.Exists(sPath) Then
                txtPlayListTitle.Tag = IO.Path.GetFileName(sPath)
                bDefaultedPlayListTitle = True
                ' TODO: Make sure "AndMaybePlay" is the right call
                AddDirToPlayListAndMaybePlay(sPath)
              Else
                PLShowMissing(sPath)
              End If
            End If
          Else
            If IO.File.Exists(sPath) Then
              Select Case IO.Path.GetExtension(sPath).ToLower
                Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sPath, True)
                Case Else
                  AddToPlayList(sPath)
                  StartPlayList()
              End Select
            ElseIf IO.Directory.Exists(sPath) Then
              ' TODO: Make sure "AndMaybePlay" is the right call
              AddDirToPlayListAndMaybePlay(sPath)
            Else
              PLShowMissing(sPath)
            End If
          End If
      End Select
    Else
      Select Case Effect
        Case DragDropEffects.All, DragDropEffects.Copy 'ADD
          If dgvPlayList.RowCount = 0 And Not String.IsNullOrEmpty(mpPlayer.FileName) Then
            AddToPlayList(mpPlayer.FileName)
            SelectedPlayListItem = 0
          End If
          For Each sPath In sData
            If IO.File.Exists(sPath) Then
              Select Case IO.Path.GetExtension(sPath).ToLower
                Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sPath)
                Case Else : AddToPlayList(sPath)
              End Select
            ElseIf IO.Directory.Exists(sPath) Then
              AddDirToPlayList(sPath)
            Else
              PLShowMissing(sPath)
            End If
            Application.DoEvents()
          Next
        Case DragDropEffects.Move 'PLAY
          For Each sPath In sData
            If IO.File.Exists(sPath) Then
              Select Case IO.Path.GetExtension(sPath).ToLower
                Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sPath)
                Case Else : AddToPlayList(sPath)
              End Select
            ElseIf IO.Directory.Exists(sPath) Then
              AddDirToPlayList(sPath)
            Else
              PLShowMissing(sPath)
            End If
            Application.DoEvents()
          Next
          StartPlayList()
        Case DragDropEffects.Link 'ADD AND PLAY
          If dgvPlayList.RowCount = 0 Then
            If Not String.IsNullOrEmpty(mpPlayer.FileName) Then
              AddToPlayList(mpPlayer.FileName)
              SelectedPlayListItem = 0
              For Each sPath In sData
                If IO.File.Exists(sPath) Then
                  Select Case IO.Path.GetExtension(sPath).ToLower
                    Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sPath)
                    Case Else : AddToPlayList(sPath)
                  End Select
                ElseIf IO.Directory.Exists(sPath) Then
                  AddDirToPlayList(sPath)
                Else
                  PLShowMissing(sPath)
                End If
                Application.DoEvents()
              Next
            Else
              For Each sPath In sData
                If IO.File.Exists(sPath) Then
                  Select Case IO.Path.GetExtension(sPath).ToLower
                    Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sPath)
                    Case Else : AddToPlayList(sPath)
                  End Select
                ElseIf IO.Directory.Exists(sPath) Then
                  AddDirToPlayList(sPath)
                Else
                  PLShowMissing(sPath)
                End If
                Application.DoEvents()
              Next
              StartPlayList()
            End If
          Else
            For Each sPath In sData
              If IO.File.Exists(sPath) Then
                Select Case IO.Path.GetExtension(sPath).ToLower
                  Case ".llpl", ".m3u", ".m3u8", ".pls" : OpenPlayList(sPath)
                  Case Else : AddToPlayList(sPath)
                End Select
              ElseIf IO.Directory.Exists(sPath) Then
                AddDirToPlayList(sPath)
              Else
                PLShowMissing(sPath)
              End If
              Application.DoEvents()
            Next
          End If
      End Select
    End If
  End Sub

  Public Sub DragEnterEvent(sender As Object, e As DragEventArgs, Optional ForceCtrl As Boolean = False)
    e.Effect = DragDropEffects.All
  End Sub

  Public Sub DragOverEvent(sender As Object, e As DragEventArgs, Optional ForceCtrl As Boolean = False)
    If e.Data.GetFormats(True).Contains("FileDrop") Then
      Dim Data = e.Data.GetData("FileDrop")
      Dim sTrackData As String
      Dim eTrackEffect As DragDropEffects = DragDropEffects.None
      If dgvPlayList.RowCount = 0 Then
        If Not String.IsNullOrEmpty(mpPlayer.FileName) And (My.Computer.Keyboard.CtrlKeyDown Or ForceCtrl) Then
          eTrackEffect = DragDropEffects.Copy
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
          If UBound(Data) = 0 AndAlso IO.File.Exists(Data(0)) Then
            If My.Computer.Keyboard.CtrlKeyDown Or ForceCtrl Then
              sTrackData = "Add """ & IO.Path.GetFileName(Data(0)) & """ to PlayList and Play"
              eTrackEffect = DragDropEffects.Link
            Else
              sTrackData = "Play """ & IO.Path.GetFileName(Data(0)) & """"
              eTrackEffect = DragDropEffects.Move
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
            eTrackEffect = DragDropEffects.Link
          End If
        End If
      Else
        eTrackEffect = DragDropEffects.Copy
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
      CursorText(sTrackData, eTrackEffect)
      e.Effect = eTrackEffect
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
      cCD = New Seed.clsAudioCD
      cCD.ChangeDrive(Path(0))
      If cCD.CDAvailable Then
        cCD.Stop()
        cCD.CurrentTrack = TrackToNo(Path)
        cCD.Play()
        bCD = True
      End If
    Else
      mpPlayer.FileName = Path
      If mpPlayer.FileName = Path Or System.IO.File.Exists(mpPlayer.FileName) Then
        pbProgress.Maximum = mpPlayer.Duration * 1000
        pbProgress.Value = mpPlayer.Position
        mpPlayer.LinearVolume = bpgVolume.Value
        mpPlayer.StateFade = False
        mpPlayer.Position = 0
        If AutoPlay Then mpPlayer.mpPlay()
      ElseIf String.IsNullOrEmpty(mpPlayer.FileName) Then
        pbProgress.Maximum = 1000
        pbProgress.Value = 0
        mpPlayer.FileName = Nothing
      Else
        Dim sTmp As String = mpPlayer.FileName
        mpPlayer.FileName = Nothing
        MsgBox("Lime Seed was unable to play the file """ & IO.Path.GetFileName(Path) & """. Error returned: " & vbNewLine & sTmp, MsgBoxStyle.Exclamation, My.Application.Info.Title)
      End If
    End If
  End Sub

  Private Sub OpenCD(Drive As String)
    mpPlayer.FileName = String.Empty
    If cCD Is Nothing Then cCD = New Seed.clsAudioCD
    cCD.ChangeDrive(Drive(0))
    cmdShufflePL.Tag = False
    cmdShufflePL.Image = My.Resources.pl_button_order
    If mnuPLDuplicates.Checked Then mnuPLDuplicates.Checked = False
    PLItems.Clear()
    'dgvPlayList.Rows.Clear()
    cCD.SetTimeFormat(Seed.clsAudioCD.TimeFormat.MS)
    For I As Integer = 1 To cCD.TotalTracks
      AddToPlayList(Drive.Substring(0, 3) & "Track" & I, "Track " & I, cCD.CDMediaInfo(Seed.clsAudioCD.MediaOption.TrackLength, I) / 1000, False)
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
    Dim _Artist As String = String.Empty
    Dim _Title As String = String.Empty
    If IO.Path.GetExtension(Path).ToLower = ".mp3" Then
      Using ID3v2Tags As New Seed.clsID3v2(Path)
        If ID3v2Tags.HasID3v2Tag Then
          If ID3v2Tags.FindFrame("TP1") Then
            _Artist = ID3v2Tags.FindFrameMatchString("TP1")
            If ID3v2Tags.FindFrame("TP2") Then
              Dim sBand As String = ID3v2Tags.FindFrameMatchString("TP2")
              Dim sOut As String = Nothing
              If MatchNames_Minimal(_Artist, sBand, sOut) Then
                _Artist = sOut
              Else
                _Artist = sBand & " (" & _Artist & ")"
              End If
            End If
          ElseIf ID3v2Tags.FindFrame("TP2") Then
            _Artist = ID3v2Tags.FindFrameMatchString("TP2")
          End If
          If ID3v2Tags.FindFrame("TT2") Then
            _Title = ID3v2Tags.FindFrameMatchString("TT2")
            If ID3v2Tags.FindFrame("TT3") Then _Title &= " (" & ID3v2Tags.FindFrameMatchString("TT3") & ")"
          End If
        End If
      End Using
      Using ID3v1Tags As New Seed.clsID3v1(Path)
        If ID3v1Tags.HasID3v1Tag Then
          If String.IsNullOrEmpty(_Artist) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Artist) Then _Artist = ID3v1Tags.Artist
          If String.IsNullOrEmpty(_Title) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Title) Then _Title = ID3v1Tags.Title
        End If
      End Using
      If Path.Contains(IO.Path.Combine(IO.Path.GetTempPath, "seedTemp")) Then
        If mpPlayer.StreamInfo IsNot Nothing Then
          If String.IsNullOrEmpty(_Artist) Then _Artist = mpPlayer.StreamInfo.Artist
          If String.IsNullOrEmpty(_Title) Then _Title = mpPlayer.StreamInfo.Title
        End If
      End If
    ElseIf IO.Path.GetExtension(Path).ToLower = ".ogg" Or IO.Path.GetExtension(Path).ToLower = ".ogm" Or IO.Path.GetExtension(Path).ToLower = ".flac" Then
      Using cVorbis As New Seed.clsVorbis(Path)
        If cVorbis.HasVorbis Then
          If Not String.IsNullOrEmpty(cVorbis.Title) Then _Title = cVorbis.Title
          If Not String.IsNullOrEmpty(cVorbis.Artist) Then _Artist = cVorbis.Artist
        End If
      End Using
    Else
      _Artist = UNKNOWN_ARTIST
      _Title = IO.Path.GetFileNameWithoutExtension(Path)
    End If
    If separateArtist Then
      If getArtist Then
        If String.IsNullOrEmpty(_Artist) Then
          Return Nothing
        Else
          Return _Artist
        End If
      Else
        If String.IsNullOrEmpty(_Title) Then
          Return IO.Path.GetFileNameWithoutExtension(Path)
        Else
          Return _Title
        End If
      End If
    Else
      If String.IsNullOrEmpty(_Artist) Then
        If String.IsNullOrEmpty(_Title) Then
          Return IO.Path.GetFileNameWithoutExtension(Path)
        Else
          Return _Title
        End If
      Else
        If String.IsNullOrEmpty(_Title) Then
          Return _Artist & " - " & IO.Path.GetFileNameWithoutExtension(Path)
        Else
          Return _Artist & " - " & _Title
        End If
      End If
    End If
  End Function

  Private Function LastDitchv1Genre(sFromGenres() As String) As Byte
    For Each fromGenre As String In sFromGenres
      If fromGenre = "Soul And R&B" Then Return &H2A
      If fromGenre = "Electronica" Then Return &H34
      If fromGenre = "Electronica & Dance" Then Return &H3
    Next
    For Each fromGenre As String In sFromGenres
      If fromGenre.Contains(" & ") Then
        Dim sBitGenres() As String = Split(fromGenre, " & ")
        For Each bitGenre As String In sBitGenres
          For I As Integer = 0 To &HBF
            If StrComp(bitGenre, Seed.clsID3v1.GenreName(I), CompareMethod.Text) = 0 Then
              Return I
            End If
          Next
        Next
      End If
    Next
    For Each fromGenre As String In sFromGenres
      If fromGenre.ToLower.Contains(" and ") Then
        Dim sBitGenres() As String = Split(fromGenre.ToLower, " and ")
        For Each bitGenre As String In sBitGenres
          For I As Integer = 0 To &HBF
            If StrComp(bitGenre, Seed.clsID3v1.GenreName(I), CompareMethod.Text) = 0 Then
              Return I
            End If
          Next
        Next
      End If
    Next
    Return &HC
  End Function

  Private Sub CleanupID3(MP3Path As String)
    If Not IO.Path.GetExtension(MP3Path).ToLower = ".mp3" Then Return
    If Not My.Settings.ID3_Modify Then Return
    If Not IO.File.Exists(MP3Path) Then Return
    Dim Track As String = Nothing
    Dim Track1 As Byte = 0
    Dim Track2 As String = Nothing
    Dim Title As String = Nothing
    Dim Title1 As String = Nothing
    Dim Title2 As String = Nothing
    Dim Artist As String = Nothing
    Dim Artist1 As String = Nothing
    Dim Artist2 As String = Nothing
    Dim Album As String = Nothing
    Dim Album1 As String = Nothing
    Dim Album2 As String = Nothing
    Dim Year As String = Nothing
    Dim Year1 As String = Nothing
    Dim Year2 As String = Nothing
    Dim Genre As String = Nothing
    Dim Genre1 As String = Nothing
    Dim Genre2 As String = Nothing
    Using ID3v1Tags As New Seed.clsID3v1(MP3Path)
      Track1 = ID3v1Tags.Track
      Title1 = ID3v1Tags.Title
      Artist1 = ID3v1Tags.Artist
      Album1 = ID3v1Tags.Album
      Year1 = ID3v1Tags.Year
      If ID3v1Tags.Genre < &HC0 Then Genre1 = Seed.clsID3v1.GenreName(ID3v1Tags.Genre)
    End Using
    Dim vSVer As String = String.Format("2.{0}.0", My.Settings.ID3_Ver)
    Dim v2Ver As String = Nothing
    Using ID3v2Tags As New Seed.clsID3v2(MP3Path)
      If ID3v2Tags.HasID3v2Tag Then
        v2Ver = ID3v2Tags.ID3v2Ver
        If ID3v2Tags.FindFrame("TRK") Then Track2 = ID3v2Tags.FindFrameMatchString("TRK")
        If ID3v2Tags.FindFrame("TT2") Then Title2 = ID3v2Tags.FindFrameMatchString("TT2")
        If ID3v2Tags.FindFrame("TP2") Then
          Artist2 = ID3v2Tags.FindFrameMatchString("TP2")
        ElseIf ID3v2Tags.FindFrame("TP1") Then
          Artist2 = ID3v2Tags.FindFrameMatchString("TP1")
        End If
        If ID3v2Tags.FindFrame("TSST") Then
          Album2 = ID3v2Tags.FindFrameMatchString("TSST")
        ElseIf ID3v2Tags.FindFrame("TAL") Then
          Album2 = ID3v2Tags.FindFrameMatchString("TAL")
        End If
        If ID3v2Tags.FindFrame("TRD") Then
          Year2 = ID3v2Tags.FindFrameMatchString("TRD")
        ElseIf ID3v2Tags.FindFrame("TYE") Then
          Year2 = ID3v2Tags.FindFrameMatchString("TYE")
        End If
        If ID3v2Tags.FindFrame("TCO") Then Genre2 = ID3v2Tags.Genre
      End If
    End Using

    If Not Track1 = Val(Track2) Then
      Dim empty1 As Boolean = False
      Dim empty2 As Boolean = False
      empty1 = (Track1 = 0 Or Track1 = &HFF)
      If String.IsNullOrEmpty(Track2) Then
        empty2 = True
      Else
        If Track2.Contains("/") Then Track2 = Track2.Substring(0, Track2.IndexOf("/")).Trim
        If Not IsNumeric(Track2) Then
          empty2 = True
        ElseIf Val(Track2) = 0 Or Val(Track2) = &HFF Then
          empty2 = True
        End If
      End If
      If empty1 And empty2 Then
        Track = Nothing
      ElseIf empty2 Then
        Track = Track1
      ElseIf empty1 Then
        Track = Val(Track2)
      Else
        If Val(Track2) = Track1 Then
          Track = Track1
        Else
          If Track1 > Val(Track2) Then
            Track = Track1
          Else
            Track = Track2
          End If
        End If
      End If
    ElseIf Track1 = 0 Then
      Track = Nothing
    Else
      Track = Track1
    End If

    If Not Title1 = Title2 Then
      If String.IsNullOrEmpty(Title1) And String.IsNullOrEmpty(Title2) Then
        Title = Nothing
      ElseIf String.IsNullOrEmpty(Title1) Then
        Title = Title2
      ElseIf String.IsNullOrEmpty(Title2) Then
        Title = Title1
      Else
        If Title1.Length > Title2.Length Then
          Title = Title1
        Else
          Title = Title2
        End If
      End If
    Else
      Title = Title1
    End If

    If Not Artist1 = Artist2 Then
      If String.IsNullOrEmpty(Artist1) And String.IsNullOrEmpty(Artist2) Then
        Artist = Nothing
      ElseIf String.IsNullOrEmpty(Artist1) Then
        Artist = Artist2
      ElseIf String.IsNullOrEmpty(Artist2) Then
        Artist = Artist1
      Else
        If Artist1.Length > Artist2.Length Then
          Artist = Artist1
        Else
          Artist = Artist2
        End If
      End If
    Else
      Artist = Artist1
    End If

    If Not Album1 = Album2 Then
      If String.IsNullOrEmpty(Album1) And String.IsNullOrEmpty(Album2) Then
        Album = Nothing
      ElseIf String.IsNullOrEmpty(Album1) Then
        Album = Album2
      ElseIf String.IsNullOrEmpty(Album2) Then
        Album = Album1
      Else
        If Album1.Length > Album2.Length Then
          Album = Album1
        Else
          Album = Album2
        End If
      End If
    Else
      Album = Album1
    End If

    If Not Year1 = Year2 Then
      Dim empty1 As Boolean = False
      Dim empty2 As Boolean = False
      If String.IsNullOrEmpty(Year1) Then
        empty1 = True
      ElseIf Not IsNumeric(Year1) Then
        empty1 = True
      ElseIf Val(Year1) = 0 Then
        empty1 = True
      End If
      If String.IsNullOrEmpty(Year2) Then
        empty2 = True
      ElseIf Not IsNumeric(Year2) Then
        empty2 = True
      ElseIf Val(Year2) = 0 Then
        empty2 = True
      End If
      If empty1 And empty2 Then
        Year = Nothing
      ElseIf empty2 Then
        Year = Year1
      Else
        Year = Year2
      End If
    Else
      Year = Year1
    End If

    If Not Genre1 = Genre2 Then
      Dim empty1 As Boolean = False
      Dim empty2 As Boolean = False
      If String.IsNullOrEmpty(Genre1) Then
        empty1 = True
      ElseIf Genre1 = "Other" Then
        empty1 = True
      End If
      If String.IsNullOrEmpty(Genre2) Then
        empty2 = True
      ElseIf Genre2 = "Other" Then
        empty2 = True
      End If

      If empty1 And empty2 Then
        Genre = Nothing
      ElseIf empty2 Then
        Genre = Genre1
      ElseIf empty1 Then
        Genre = Genre2
      Else
        If Genre2.Contains(vbNewLine) Then
          For Each fGenre In Split(Genre2, vbNewLine)
            If StrComp(Genre1, Genre2, CompareMethod.Text) = 0 Then
              Genre = Genre1
              Exit For
            End If
          Next
          If String.IsNullOrEmpty(Genre) Then Genre = Genre2.Substring(0, Genre2.IndexOf(vbNewLine))
        Else
          If StrComp(Genre1, Genre2, CompareMethod.Text) = 0 Then
            Genre = Genre1
          Else
            Genre = Genre2
          End If
        End If
      End If
    Else
      Genre = Genre1
    End If

    If My.Settings.ID3_Clean Then
      Using ID3v1Tags As New Seed.clsID3v1(MP3Path)
        If Not String.IsNullOrEmpty(Track) AndAlso (Val(Track) < 256) Then ID3v1Tags.Track = Track
        If Not String.IsNullOrEmpty(Title) Then ID3v1Tags.Title = Title
        If Not String.IsNullOrEmpty(Artist) Then ID3v1Tags.Artist = Artist
        If Not String.IsNullOrEmpty(Album) Then ID3v1Tags.Album = Album
        If Not String.IsNullOrEmpty(Year) Then ID3v1Tags.Year = Year
        If ID3v1Tags.Genre > &HBF Then ID3v1Tags.Genre = &HC
        If Not String.IsNullOrEmpty(Genre) Then
          If Genre.Contains(vbNewLine) Then
            Dim bFound As Boolean = False
            For Each fGenre In Split(Genre, vbNewLine)
              For I As Integer = 0 To &HBF
                If StrComp(fGenre, Seed.clsID3v1.GenreName(I), CompareMethod.Text) = 0 Then
                  bFound = True
                  ID3v1Tags.Genre = I
                  Exit For
                End If
              Next
              If bFound Then Exit For
            Next
            If Not bFound Then
              Dim out As Byte = LastDitchv1Genre(Split(Genre, vbNewLine))
              If out = &HC Then
                Debug.Print("Unable to convert " & Replace(Genre, vbNewLine, "/") & " to ID3v1 Genre List!")
              Else
                ID3v1Tags.Genre = out
              End If
            End If
          Else
            Dim bFound As Boolean = False
            For I As Integer = 0 To &HBF
              If StrComp(Genre, Seed.clsID3v1.GenreName(I), CompareMethod.Text) = 0 Then
                bFound = True
                ID3v1Tags.Genre = I
                Exit For
              End If
            Next
            If Not bFound Then
              Dim out As Byte = LastDitchv1Genre({Genre})
              If out = &HC Then
                Debug.Print("Unable to convert " & Genre & " to ID3v1 Genre List!")
              Else
                ID3v1Tags.Genre = out
              End If
            End If
          End If
        End If
        Try
          ID3v1Tags.Save()
        Catch ex As Exception
        End Try
      End Using
      Using ID3v2Tags As New Seed.clsID3v2(MP3Path)
        Dim id3v2V(1) As Byte
        If Not ID3v2Tags.HasID3v2Tag Then
          If vSVer = "2.0.0" Then
            ID3v2Tags.ID3v2Ver = "2.3.0"
          Else
            ID3v2Tags.ID3v2Ver = vSVer
          End If
        End If
        Dim vData() As String = Split(ID3v2Tags.ID3v2Ver, ".")
        If vData.Length = 3 Then
          id3v2V(0) = Val(vData(1))
          id3v2V(1) = Val(vData(2))
        Else
          id3v2V(0) = 3
          id3v2V(1) = 0
        End If
        If Not String.IsNullOrEmpty(Track) Then If Not ID3v2Tags.FindFrame("TRK") Then ID3v2Tags.AddTextFrame("TRK", New Seed.clsID3v2.EncodedText(Track))
        If Not String.IsNullOrEmpty(Title) Then If Not ID3v2Tags.FindFrame("TT2") Then ID3v2Tags.AddTextFrame("TT2", New Seed.clsID3v2.EncodedText(Title))
        If Not String.IsNullOrEmpty(Artist) Then
          If Not ID3v2Tags.FindFrame("TP2") Then ID3v2Tags.AddTextFrame("TP2", New Seed.clsID3v2.EncodedText(Artist))
          If Not ID3v2Tags.FindFrame("TP1") Then ID3v2Tags.AddTextFrame("TP1", New Seed.clsID3v2.EncodedText(Artist))
        End If
        If Not String.IsNullOrEmpty(Album) Then If Not ID3v2Tags.FindFrame("TAL") Then ID3v2Tags.AddTextFrame("TAL", New Seed.clsID3v2.EncodedText(Album))
        If Not String.IsNullOrEmpty(Year) Then If Not ID3v2Tags.FindFrame("TYE") Then ID3v2Tags.AddTextFrame("TYE", New Seed.clsID3v2.EncodedText(Year))
        If Not String.IsNullOrEmpty(Genre) Then If Not ID3v2Tags.FindFrame("TCO") Then ID3v2Tags.Genre = Genre
        If ID3v2Tags.FindFrame("PIC") Then
          Dim picList As Seed.clsID3v2.ParseResponse() = ID3v2Tags.FindFrameMatches("PIC")
          If picList IsNot Nothing Then
            For I As Integer = picList.Length - 1 To 0 Step -1
              If picList(I).GetType IsNot GetType(Seed.clsID3v2.Parsed_APIC) Then Continue For
              If CType(picList(I), Seed.clsID3v2.Parsed_APIC).Type = Seed.clsID3v2.ID3_PIC_TYPE.INVALID Then
                ID3v2Tags.RemoveFrame("PIC", I)
                Continue For
              End If
            Next
            picList = ID3v2Tags.FindFrameMatches("PIC")
            If picList IsNot Nothing Then
              For I As Integer = picList.Length - 1 To 0 Step -1
                If picList(I).GetType IsNot GetType(Seed.clsID3v2.Parsed_APIC) Then Continue For
                If id3v2V(0) = 2 Then
                  If CType(picList(I), Seed.clsID3v2.Parsed_APIC).Type = Seed.clsID3v2.ID3_PIC_TYPE.FRONT_COVER Then
                    ID3v2Tags.RemoveFrame("PIC", I)
                    ID3v2Tags.AddAPICFrame(CType(picList(I), Seed.clsID3v2.Parsed_APIC).Image, Seed.clsID3v2.ID3_PIC_TYPE.OTHER, CType(picList(I), Seed.clsID3v2.Parsed_APIC).MIME, New Seed.clsID3v2.EncodedText(CType(picList(I), Seed.clsID3v2.Parsed_APIC).Description))
                    Continue For
                  End If
                Else
                  If CType(picList(I), Seed.clsID3v2.Parsed_APIC).Type = Seed.clsID3v2.ID3_PIC_TYPE.OTHER Then
                    ID3v2Tags.RemoveFrame("PIC", I)
                    ID3v2Tags.AddAPICFrame(CType(picList(I), Seed.clsID3v2.Parsed_APIC).Image, Seed.clsID3v2.ID3_PIC_TYPE.FRONT_COVER, CType(picList(I), Seed.clsID3v2.Parsed_APIC).MIME, New Seed.clsID3v2.EncodedText(CType(picList(I), Seed.clsID3v2.Parsed_APIC).Description))
                    Continue For
                  End If
                End If
              Next
            End If
          End If
        End If

        If ID3v2Tags.FindFrame("PRIV") Then
          Dim privList As Seed.clsID3v2.ParseResponse() = ID3v2Tags.FindFrameMatches("PRIV")
          If privList IsNot Nothing Then
            For I As Integer = privList.Length - 1 To 0 Step -1
              If privList(I).GetType IsNot GetType(Seed.clsID3v2.Parsed_PRIV) Then Continue For
              If String.IsNullOrEmpty(CType(privList(I), Seed.clsID3v2.Parsed_PRIV).Owner) Then
                Debug.Print("Ownerless Private Frame:")
                Debug.Print(CType(privList(I), Seed.clsID3v2.Parsed_PRIV).DataString)
                Continue For
              End If
              If CType(privList(I), Seed.clsID3v2.Parsed_PRIV).Owner.StartsWith("WM/") Then
                ID3v2Tags.RemoveFrame("PRIV", I)
                Continue For
              End If
              If CType(privList(I), Seed.clsID3v2.Parsed_PRIV).Owner = "AverageLevel" Then Continue For
              If CType(privList(I), Seed.clsID3v2.Parsed_PRIV).Owner = "PeakValue" Then Continue For
              Debug.Print("Unknown Private Frame: " & CType(privList(I), Seed.clsID3v2.Parsed_PRIV).Owner)
              Debug.Print(CType(privList(I), Seed.clsID3v2.Parsed_PRIV).DataString)
              Continue For
            Next
          End If
        End If
        If ID3v2Tags.FindFrame("GEO") Then
          Dim objList As Seed.clsID3v2.ParseResponse() = ID3v2Tags.FindFrameMatches("GEO")
          If objList IsNot Nothing Then
            For I As Integer = objList.Length - 1 To 0 Step -1
              If objList(I).GetType IsNot GetType(Seed.clsID3v2.Parsed_GEOB) Then Continue For
              If Not String.IsNullOrEmpty(CType(objList(I), Seed.clsID3v2.Parsed_GEOB).Description) AndAlso CType(objList(I), Seed.clsID3v2.Parsed_GEOB).Description = "RealJukebox:Metadata" Then
                ID3v2Tags.RemoveFrame("GEO", I)
                Continue For
              End If
            Next
          End If
        End If
        If ID3v2Tags.FindFrame("COM") Then
          Dim comList As Seed.clsID3v2.ParseResponse() = ID3v2Tags.FindFrameMatches("COM")
          If comList IsNot Nothing Then
            For I As Integer = comList.Length - 1 To 0 Step -1
              If comList(I).GetType IsNot GetType(Seed.clsID3v2.Parsed_COMM) Then Continue For
              If String.IsNullOrEmpty(CType(comList(I), Seed.clsID3v2.Parsed_COMM).Comment) And String.IsNullOrEmpty(CType(comList(I), Seed.clsID3v2.Parsed_COMM).Description) Then
                ID3v2Tags.RemoveFrame("COM", I)
                Continue For
              End If
              If Not String.IsNullOrEmpty(CType(comList(I), Seed.clsID3v2.Parsed_COMM).Comment) AndAlso CType(comList(I), Seed.clsID3v2.Parsed_COMM).Comment = "Track:Comments" Then
                ID3v2Tags.RemoveFrame("COM", I)
                Continue For
              End If
              If Not String.IsNullOrEmpty(CType(comList(I), Seed.clsID3v2.Parsed_COMM).Description) AndAlso CType(comList(I), Seed.clsID3v2.Parsed_COMM).Description.StartsWith("iTun") Then
                ID3v2Tags.RemoveFrame("COM", I)
                Continue For
              End If
              Debug.Print("Other Comment Data:")
              Debug.Print(" Language: " & CType(comList(I), Seed.clsID3v2.Parsed_COMM).Language)
              Debug.Print(" Description: " & CType(comList(I), Seed.clsID3v2.Parsed_COMM).Description)
              Debug.Print(" Comment: " & CType(comList(I), Seed.clsID3v2.Parsed_COMM).Comment)
              Continue For
            Next
          End If
        End If
        Try
          ID3v2Tags.Save()
        Catch ex As Exception
        End Try
      End Using
    End If

    If Not vSVer = "2.0.0" Then
      If Not v2Ver = vSVer Then
        Using ID3v2Tags As New Seed.clsID3v2(MP3Path)
          If Not ID3v2Tags.HasID3v2Tag Then Return
          ID3v2Tags.ID3v2Ver = vSVer
          Try
            ID3v2Tags.Save()
          Catch ex As Exception
          End Try
        End Using
      End If
    End If
  End Sub

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
      If IO.File.Exists(fileName) Then
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
        If IO.File.Exists(value) Then
          mFile = value
          mArt = PathToImg(value)
        Else
          mFile = value
        End If
      End Set
    End Property
  End Class

#Region "Shortcut Controls"
  Private KeyPreviewVals As Collection
  Protected Overrides Function ProcessKeyPreview(ByRef m As System.Windows.Forms.Message) As Boolean
    If Not My.Settings.Keyboard Then Return MyBase.ProcessKeyPreview(m)
    Select Case Me.ActiveControl.Name
      Case ripBox.Name, artList.Name, txtPlayListTitle.Name,
           cmdPlayPause.Name, cmdStop.Name, cmdFullScreen.Name, cmdLoop.Name, cmdMenu.Name, cmbVidTrack.Name, cmbAudTrack.Name, cmbChapters.Name, cmbSubtitles.Name, cmdMute.Name,
           cmdShufflePL.Name, cmdLoopPL.Name, cmdAddToPL.Name, cmdRemoveFromPL.Name, cmdClearPL.Name, cmdSavePL.Name, cmdOpenPL.Name,
           dgvPlayList.Name
        Return MyBase.ProcessKeyPreview(m)
    End Select
    If KeyPreviewVals Is Nothing Then KeyPreviewVals = New Collection
    Select Case m.Msg
      Case &H100, &H104
        Dim iKey As Integer = m.WParam.ToInt32
        Dim Key As Keys = iKey
        If Not KeyPreviewVals.Contains(iKey) Then KeyPreviewVals.Add(KeyToStr(Key), iKey)
      Case &H101, &H105
        Dim iKey As Integer = m.WParam.ToInt32
        Dim Key As Keys = iKey
        If Not KeyPreviewVals.Contains(iKey) Then KeyPreviewVals.Add(KeyToStr(Key), iKey)
        Dim sVals As String = String.Empty
        For Each sKey In KeyPreviewVals
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
        If KeyPreviewVals.Contains(iKey) Then KeyPreviewVals.Remove(CStr(iKey))
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
    If Not My.Settings.Gamepad Then Return
    If bDVD And mpPlayer.GetDVDCurrentDomain = DirectShowLib.Dvd.DvdDomain.VideoTitleSetMenu Then
      If SearchFor = "X Axis Left" Then
        mpPlayer.DVDSelectButton(DirectShowLib.Dvd.DvdRelativeButton.Left)
        Return
      ElseIf SearchFor = "X Axis Right" Then
        mpPlayer.DVDSelectButton(DirectShowLib.Dvd.DvdRelativeButton.Right)
        Return
      ElseIf SearchFor = "Y Axis Top" Then
        mpPlayer.DVDSelectButton(DirectShowLib.Dvd.DvdRelativeButton.Upper)
        Return
      ElseIf SearchFor = "Y Axis Bottom" Then
        mpPlayer.DVDSelectButton(DirectShowLib.Dvd.DvdRelativeButton.Lower)
        Return
      ElseIf SearchFor = My.Settings.Gamepad_PlayPause Then
        mpPlayer.DVDActivateButton()
        Return
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
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New EventHandler(Of AppleNet.ChoicesEventArgs)(AddressOf macArt_Choices), sender, e)
      Catch ex As Exception
      End Try
      Return
    End If
    Dim Artist, Album As String
    Artist = String.Empty
    Album = String.Empty
    If bCD Then
      'Artist = dgvPlayList.Rows(SelectedPlayListItem).Tag(2)
      'Album = dgvPlayList.Rows(SelectedPlayListItem).Tag(3)
      Artist = PLItems.PlayListItem(SelectedPlayListItem).Artist
      Album = PLItems.PlayListItem(SelectedPlayListItem).Album
    Else
      If IO.Path.GetExtension(mpPlayer.FileName).ToLower = ".mp3" Then
        Using ID3v2Tags As New Seed.clsID3v2(mpPlayer.FileName)
          If ID3v2Tags.HasID3v2Tag Then
            If ID3v2Tags.FindFrame("TP1") Then
              Artist = ID3v2Tags.FindFrameMatchString("TP1")
              If ID3v2Tags.FindFrame("TP2") Then
                Dim sBand As String = ID3v2Tags.FindFrameMatchString("TP2")
                Dim sOut As String = Nothing
                If MatchNames_Minimal(Artist, sBand, sOut) Then
                  Artist = sOut
                Else
                  Artist = sBand & " (" & Artist & ")"
                End If
              End If
            ElseIf ID3v2Tags.FindFrame("TP2") Then
              Artist = ID3v2Tags.FindFrameMatchString("TP2")
            End If
            If ID3v2Tags.FindFrame("TAL") Then Album = ID3v2Tags.FindFrameMatchString("TAL")
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
            Return
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
  End Sub

  Private Sub macArt_Progress(sender As Object, e As System.Net.DownloadProgressChangedEventArgs) Handles macArt.Progress
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New Net.DownloadProgressChangedEventHandler(AddressOf macArt_Progress), sender, e)
      Catch ex As Exception
      End Try
      Return
    End If
    If e.TotalBytesToReceive > 0 Then
      If e.ProgressPercentage > 0 And e.ProgressPercentage < 100 Then
        pbArt.Visible = True
        pbArt.Maximum = e.TotalBytesToReceive
        If e.BytesReceived < e.TotalBytesToReceive Then pbArt.Value = e.BytesReceived
        ttDisp.SetToolTip(pbArt, "Downloading Art: " & ByteSize(e.BytesReceived) & "/" & ByteSize(e.TotalBytesToReceive) & " - " & e.ProgressPercentage & "%")
      Else
        pbArt.Visible = False
        pbArt.Value = 0
        ttDisp.SetToolTip(pbArt, "")
      End If
    End If
  End Sub

  Private Sub macArt_Complete(sender As Object, e As AppleNet.CompleteEventArgs) Handles macArt.Complete
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New EventHandler(Of AppleNet.CompleteEventArgs)(AddressOf macArt_Complete), sender, e)
      Catch ex As Exception
      End Try
      Return
    End If
    CleanupID3(mpPlayer.FileName)
    Dim sArt As String = IO.Path.Combine(IO.Path.GetDirectoryName(mpPlayer.FileName), "Folder.jpg")
    FileArt = New ImageWithName(Drawing.Image.FromStream(New IO.MemoryStream(e.Cover)), sArt)
    If cTask IsNot Nothing Then cTask.ShowImage(FileArt.Art) 'TODO: THIS IS A THING
    If Not IO.File.Exists(sArt) Then
      Try
        FileArt.Art.Save(sArt, Drawing.Imaging.ImageFormat.Jpeg)
        IO.File.SetAttributes(sArt, IO.FileAttributes.Hidden)
      Catch ex As Exception
        Debug.Print("Unable to save art: " & ex.Message)
      End Try
      SaveArtToMP3(mpPlayer.FileName, sArt)
    Else
      If CompareArtSizes(sArt, FileArt.Art) Then
        Try
          IO.File.Delete(sArt)
          FileArt.Art.Save(sArt, Drawing.Imaging.ImageFormat.Jpeg)
          IO.File.SetAttributes(sArt, IO.FileAttributes.Hidden)
        Catch ex As Exception
          Debug.Print("Unable to save art: " & ex.Message)
        End Try
        SaveArtToMP3(mpPlayer.FileName, sArt)
      Else
        Debug.Print("Larger Folder.jpg already exists.")
      End If
    End If
    If Me.WindowState = FormWindowState.Normal And tbsView.SelectedTab Is tabArt Then tbsView_SelectedIndexChanged(New Object, New EventArgs)
  End Sub

  Private Sub macArt_Failed(sender As Object, e As AppleNet.FailEventArgs) Handles macArt.Failed
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New EventHandler(Of AppleNet.FailEventArgs)(AddressOf macArt_Failed), sender, e)
      Catch ex As Exception
      End Try
      Return
    End If
    Debug.Print("Art Failed: " & e.Error.Message)
    FileArt = New ImageWithName(My.Resources.UnknownAlbum, "UNKNOWNALBUM")
  End Sub
#End Region

  Private Function SendArtA(Artist As String, Album As String, DoSearch As Boolean) As ImageWithName
    If String.IsNullOrEmpty(Artist) And String.IsNullOrEmpty(Album) Then Return Nothing
    If String.IsNullOrEmpty(Artist) Then
      If DoSearch Then
        macArt = New AppleNet(Album, AppleNet.Term.Album)
        Return New ImageWithName(My.Resources.loadingartlarge, "loadingartlarge")
      End If
      Return Nothing
    End If
    If String.IsNullOrEmpty(Album) Then
      If DoSearch Then
        macArt = New AppleNet(Artist, AppleNet.Term.Artist)
        Return New ImageWithName(My.Resources.loadingartlarge, "loadingartlarge")
      End If
      Return Nothing
    End If
    If DoSearch Then
      macArt = New AppleNet(Artist & " " & Album, AppleNet.Term.Any)
      Return New ImageWithName(My.Resources.loadingartlarge, "loadingartlarge")
    End If
    Return Nothing
  End Function

  Private Function SendArtT(Artist As String, Title As String, DoSearch As Boolean) As ImageWithName
    If String.IsNullOrEmpty(Artist) And String.IsNullOrEmpty(Title) Then Return Nothing
    If String.IsNullOrEmpty(Artist) Then
      If DoSearch Then
        macArt = New AppleNet(Title, AppleNet.Term.Title)
        Return New ImageWithName(My.Resources.loadingartlarge, "loadingartlarge")
      End If
      Return Nothing
    End If
    If String.IsNullOrEmpty(Title) Then
      If DoSearch Then
        macArt = New AppleNet(Artist, AppleNet.Term.Artist)
        Return New ImageWithName(My.Resources.loadingartlarge, "loadingartlarge")
      End If
      Return Nothing
    End If
    If DoSearch Then
      macArt = New AppleNet(Artist & " " & Title, AppleNet.Term.Any)
      Return New ImageWithName(My.Resources.loadingartlarge, "loadingartlarge")
    End If
    Return Nothing
  End Function

  Private Sub SaveArtToMP3(MP3Path As String, ImagePath As String)
    If Not IO.Path.GetExtension(MP3Path).ToLower = ".mp3" Then Return
    If Not My.Settings.ID3_Modify Then Return
    If Not My.Settings.ID3_Art Then Return
    If Not IO.File.Exists(ImagePath) Then Return
    If Not IO.File.Exists(MP3Path) Then Return
    Using ID3v2Tags As New Seed.clsID3v2(MP3Path)
      If Not ID3v2Tags.HasID3v2Tag Then Return
      Dim shouldSave As Boolean = False
      Dim shouldAdd As Boolean = True
      If ID3v2Tags.FindFrame("PIC") Then
        Dim picList() As Seed.clsID3v2.ParseResponse = ID3v2Tags.FindFrameMatches("PIC")
        If picList IsNot Nothing Then
          For I As Integer = 0 To picList.Length - 1
            Dim iPic As Seed.clsID3v2.Parsed_APIC = picList(I)
            If iPic.Type = Seed.clsID3v2.ID3_PIC_TYPE.INVALID Then
              If My.Settings.ID3_Clean Then ID3v2Tags.RemoveFrame("PIC", I) : shouldSave = True
              Continue For
            End If
            If ID3v2Tags.ID3v2Ver = "2.2.0" Then
              If iPic.Type = Seed.clsID3v2.ID3_PIC_TYPE.OTHER Then
                If CompareArtSizes(iPic.Image, ImagePath) Then
                  If String.IsNullOrEmpty(iPic.Description) OrElse (iPic.Description.ToLower.Contains("cover") Or iPic.Description = "thumbnail") Then
                    If My.Settings.ID3_Clean Then ID3v2Tags.RemoveFrame("PIC", I) : shouldSave = True
                  Else
                    Debug.Print("Not removing """ & iPic.Description & """ image!")
                  End If
                Else
                  shouldAdd = False
                End If
              End If
            Else
              If iPic.Type = Seed.clsID3v2.ID3_PIC_TYPE.FRONT_COVER Then
                If CompareArtSizes(iPic.Image, ImagePath) Then
                  If String.IsNullOrEmpty(iPic.Description) OrElse (iPic.Description.ToLower.Contains("cover") Or iPic.Description = "thumbnail") Then
                    If My.Settings.ID3_Clean Then ID3v2Tags.RemoveFrame("PIC", I) : shouldSave = True
                  Else
                    Debug.Print("Not removing """ & iPic.Description & """ image!")
                  End If
                Else
                  shouldAdd = False
                End If
              End If
            End If
          Next
        End If
      End If
      If shouldAdd Then shouldSave = True
      Try
        If shouldSave Then
          If shouldAdd Then
            If ID3v2Tags.ID3v2Ver = "2.2.0" Then
              If Not ID3v2Tags.AddAPICFrame(IO.File.ReadAllBytes(ImagePath), Seed.clsID3v2.ID3_PIC_TYPE.OTHER, Seed.clsID3v2.ExtToMIME(ImagePath), Seed.clsID3v2.EncodedText.Empty) = Seed.clsID3v2.ID3Returns.Added Then Return
            Else
              If Not ID3v2Tags.AddAPICFrame(IO.File.ReadAllBytes(ImagePath), Seed.clsID3v2.ID3_PIC_TYPE.FRONT_COVER, Seed.clsID3v2.ExtToMIME(ImagePath), Seed.clsID3v2.EncodedText.Empty) = Seed.clsID3v2.ID3Returns.Added Then Return
            End If
          End If
          If Not ID3v2Tags.Save() Then Debug.Print("Didn't Save art!")
        End If
      Catch ex As Exception

      End Try
    End Using
  End Sub

  Private Function GetArt(Path As String, DoSearch As Boolean, ForceSearch As Boolean) As ImageWithName
    If bCD Then
      If DoSearch Then
        'macArt = New AppleNet(dgvPlayList.Rows(SelectedPlayListItem).Tag(3), AppleNet.Term.Album)
        macArt = New AppleNet(PLItems.PlayListItem(SelectedPlayListItem).Album, AppleNet.Term.Album)
        Return New ImageWithName(My.Resources.loadingartlarge, "loadingartlarge")
      End If
      Return Nothing
    End If
    Dim Artist As String = String.Empty
    Dim Album As String = String.Empty
    Select Case IO.Path.GetExtension(Path).ToLower
      Case ".mp3"
        If Not mpPlayer.IsStreaming Then CleanupID3(Path)
        If ForceSearch Then
          Using ID3v2Tags As New Seed.clsID3v2(Path)
            If ID3v2Tags.HasID3v2Tag Then
              If ID3v2Tags.FindFrame("TP1") Then
                Artist = ID3v2Tags.FindFrameMatchString("TP1")
                If ID3v2Tags.FindFrame("TP2") Then
                  Dim sBand As String = ID3v2Tags.FindFrameMatchString("TP2")
                  Dim sOut As String = Nothing
                  If MatchNames_Minimal(Artist, sBand, sOut) Then
                    Artist = sOut
                  Else
                    Artist = sBand & " (" & Artist & ")"
                  End If
                End If
              ElseIf ID3v2Tags.FindFrame("TP2") Then
                Artist = ID3v2Tags.FindFrameMatchString("TP2")
              End If
              If ID3v2Tags.FindFrame("TAL") Then Album = ID3v2Tags.FindFrameMatchString("TAL")
            End If
          End Using
          Using ID3v1Tags As New Seed.clsID3v1(Path)
            If ID3v1Tags.HasID3v1Tag Then
              If String.IsNullOrEmpty(Artist) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Artist) Then Artist = ID3v1Tags.Artist
              If String.IsNullOrEmpty(Album) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Album) Then Album = ID3v1Tags.Album
            End If
          End Using
          Return SendArtA(Artist, Album, DoSearch)
        End If
        Using ID3v2Tags As New Seed.clsID3v2(Path)
          If ID3v2Tags.HasID3v2Tag Then
            If ID3v2Tags.FindFrame("PIC") Then
              Dim fData() As Seed.clsID3v2.ParseResponse = ID3v2Tags.FindFrameMatches("PIC")
              For Each picType In Seed.clsID3v2.ID3_PIC_TYPE_DISPLAYORDER
                For Each pData As Seed.clsID3v2.Parsed_APIC In fData
                  If pData.Type = picType Then
                    If pData.Picture IsNot Nothing Then

                      Dim j2Path As String = IO.Path.Combine(IO.Path.GetDirectoryName(Path), "Folder.jpg")
                      If IO.File.Exists(j2Path) Then
                        If Not (IO.File.GetAttributes(j2Path) Or IO.FileAttributes.Hidden) = IO.FileAttributes.Hidden Then IO.File.SetAttributes(j2Path, IO.FileAttributes.Hidden)
                        Dim myReturn As Boolean = False
                        If CompareArtSizes(pData.Picture, j2Path) Then myReturn = True
                        If Not mpPlayer.IsStreaming Then SaveArtToMP3(Path, j2Path)
                        If myReturn Then Return New ImageWithName(j2Path)
                      End If

                      Return (New ImageWithName(pData.Picture, Path))

                    End If
                  End If
                Next
              Next
            End If
          End If
        End Using
        Using ID3v2Tags As New Seed.clsID3v2(Path)
          If ID3v2Tags.HasID3v2Tag Then
            If ID3v2Tags.FindFrame("TP1") Then
              Artist = ID3v2Tags.FindFrameMatchString("TP1")
              If ID3v2Tags.FindFrame("TP2") Then
                Dim sBand As String = ID3v2Tags.FindFrameMatchString("TP2")
                Dim sOut As String = Nothing
                If MatchNames_Minimal(Artist, sBand, sOut) Then
                  Artist = sOut
                Else
                  Artist = sBand & " (" & Artist & ")"
                End If
              End If
            ElseIf ID3v2Tags.FindFrame("TP2") Then
              Artist = ID3v2Tags.FindFrameMatchString("TP2")
            End If
            If ID3v2Tags.FindFrame("TAL") Then Album = ID3v2Tags.FindFrameMatchString("TAL")
          End If
        End Using
        Using ID3v1Tags As New Seed.clsID3v1(Path)
          If ID3v1Tags.HasID3v1Tag Then
            If String.IsNullOrEmpty(Artist) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Artist) Then Artist = ID3v1Tags.Artist
            If String.IsNullOrEmpty(Album) AndAlso Not String.IsNullOrWhiteSpace(ID3v1Tags.Album) Then Album = ID3v1Tags.Album
          End If
        End Using
        Dim jPath As String = IO.Path.Combine(IO.Path.GetDirectoryName(Path), "Folder.jpg")
        If IO.File.Exists(jPath) Then
          If Not (IO.File.GetAttributes(jPath) Or IO.FileAttributes.Hidden) = IO.FileAttributes.Hidden Then IO.File.SetAttributes(jPath, IO.FileAttributes.Hidden)
          If Not mpPlayer.IsStreaming Then SaveArtToMP3(Path, jPath)
          Return New ImageWithName(jPath)
        End If
        Return SendArtA(Artist, Album, DoSearch)
      Case ".ogg", ".ogm", ".flac"
        If ForceSearch Then
          Using cVorbis As New Seed.clsVorbis(Path)
            If cVorbis.HasVorbis Then
              If Not String.IsNullOrEmpty(cVorbis.Album) Then Album = cVorbis.Album
              If Not String.IsNullOrEmpty(cVorbis.Artist) Then Artist = cVorbis.Artist
            End If
          End Using
          Return SendArtA(Artist, Album, DoSearch)
        End If
        Using cVorbis As New Seed.clsVorbis(Path)
          If cVorbis.HasVorbis Then
            If cVorbis.Pictures IsNot Nothing Then
              If cVorbis.Pictures.Count > 0 Then
                For Each picType In Seed.clsID3v2.ID3_PIC_TYPE_DISPLAYORDER
                  For I As Integer = 0 To cVorbis.Pictures.Count - 1
                    If cVorbis.Pictures(I).PicType = picType Then
                      Return cVorbis.Pictures(I).Image.Clone
                    End If
                  Next
                Next
                Return cVorbis.Pictures(0).Image.Clone
              End If
            End If
          End If
        End Using
        Using cVorbis As New Seed.clsVorbis(Path)
          If cVorbis.HasVorbis Then
            If Not String.IsNullOrEmpty(cVorbis.Album) Then Album = cVorbis.Album
            If Not String.IsNullOrEmpty(cVorbis.Artist) Then Artist = cVorbis.Artist
          End If
        End Using
        Dim jPath As String = IO.Path.Combine(IO.Path.GetDirectoryName(Path), "Folder.jpg")
        If IO.File.Exists(jPath) Then
          If Not (IO.File.GetAttributes(jPath) Or IO.FileAttributes.Hidden) = IO.FileAttributes.Hidden Then IO.File.SetAttributes(jPath, IO.FileAttributes.Hidden)
          Return New ImageWithName(jPath)
        End If
        Return SendArtA(Artist, Album, DoSearch)
      Case Else
        If ForceSearch Then
          Dim Title As String = String.Empty
          If IO.Path.GetFileNameWithoutExtension(Path).Contains(" - ") Then
            Artist = Split(IO.Path.GetFileNameWithoutExtension(Path), " - ", 2)(0)
            Title = Split(IO.Path.GetFileNameWithoutExtension(Path), " - ", 2)(1)
          Else
            Title = IO.Path.GetFileNameWithoutExtension(Path)
          End If
          Return SendArtT(Artist, Title, DoSearch)
        End If
        Dim jPath As String = IO.Path.Combine(IO.Path.GetDirectoryName(Path), "Folder.jpg")
        If IO.File.Exists(jPath) Then
          If Not (IO.File.GetAttributes(jPath) Or IO.FileAttributes.Hidden) = IO.FileAttributes.Hidden Then IO.File.SetAttributes(jPath, IO.FileAttributes.Hidden)
          Return New ImageWithName(jPath)
        End If
        Return Nothing
    End Select
  End Function

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
          ElseIf mFArt.FileName = "LOADING" Or mFArt.FileName = "UNKNOWNALBUM" Then
            If Not pctAlbumArt.BackgroundImageLayout = ImageLayout.Zoom Then pctAlbumArt.BackgroundImageLayout = ImageLayout.Zoom
          Else
            If Not pctAlbumArt.BackgroundImageLayout = ImageLayout.Zoom Then pctAlbumArt.BackgroundImageLayout = ImageLayout.Zoom
          End If
          If Not bgArt.FileName = mFArt.FileName Then
            pctAlbumArt.BackgroundImage = mFArt.Art
            bgArt = mFArt
          End If
        Else
          If Not String.IsNullOrEmpty(mpPlayer.FileName) Then
            pctAlbumArt.BackgroundImage = My.Resources.UnknownAlbum
            If Not pctAlbumArt.BackgroundImageLayout = ImageLayout.Zoom Then pctAlbumArt.BackgroundImageLayout = ImageLayout.Zoom
            mFArt = New ImageWithName(My.Resources.UnknownAlbum, "UNKNOWNALBUM")
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
      End If
    End Set
  End Property

  Private Sub getAlbumInfo_Artwork(Picture As System.Drawing.Image) Handles getAlbumInfo.Artwork
    FileArt = New ImageWithName(Picture.Clone, "CDImage")
    If cTask IsNot Nothing Then cTask.ShowImage(FileArt.Art) 'TODO: THIS IS A THING
    'If Me.WindowState = FormWindowState.Normal And tbsView.SelectedTab Is tabArt Then tbsView_SelectedIndexChanged(New Object, New EventArgs)
  End Sub

  Private Sub getAlbumInfo_Info(Artist As String, Album As String, Tracks() As AlbumInfo.TrackInfo, Genre As String) Handles getAlbumInfo.Info
    For I As Integer = 0 To Tracks.Length - 1
      Dim indx As Integer = -1
      For P As Integer = 0 To PLItems.Count - 1
        If TrackToNo(PLItems.PlayListItem(P).Path) = I + 1 Then
          indx = P
          Exit For
        End If
      Next
      If indx > -1 Then Return
      PLItems.PlayListItem(indx).Title = Tracks(I).Title
      PLItems.PlayListItem(indx).Artist = Tracks(I).Artist
      PLItems.PlayListItem(indx).Album = Album
      PLItems.PlayListItem(indx).Genre = Genre
      dgvPlayList.Rows(indx).Cells(0).Value = PLItems.PlayListItem(indx).Title
      dgvPlayList.Rows(indx).Cells(0).ToolTipText = PLItems.PlayListItem(indx).ToolTipText(False)
      dgvPlayList.Rows(indx).Cells(1).ToolTipText = PLItems.PlayListItem(indx).ToolTipText(False)
      If SelectedPlayListItem = indx Then
        SetTitleArtist(PLItems.PlayListItem(indx).Title, PLItems.PlayListItem(indx).Artist)
        FileTitle = PLItems.PlayListItem(indx).Artist & " - " & PLItems.PlayListItem(indx).Title
      End If

      'Dim dgvX = (From item As DataGridViewRow In dgvPlayList.Rows Where TrackToNo(item.Tag(0)) = indx + 1).FirstOrDefault
      'If dgvX IsNot Nothing Then
      '  Dim _Path As String = dgvX.Tag(0)
      '  Dim _Title As String = Tracks(I).Title
      '  Dim _Artist As String = Tracks(I).Artist
      '  Dim _Album As String = Album
      '  Dim _Genre As String = Genre
      '  Dim _Length As Double = dgvX.Tag(5)
      '  dgvX.Cells(0).Value = _Title
      '  dgvX.Tag = {_Path, _Title, _Artist, _Album, _Genre, _Length}
      '  Dim sTooltip As String = String.Empty
      '  sTooltip = "Title: " & _Title
      '  If Not String.IsNullOrEmpty(_Artist) AndAlso (Not _Artist = LOADING_ARTIST) Then sTooltip &= vbNewLine & "Artist: " & _Artist
      '  If Not String.IsNullOrEmpty(_Album) AndAlso (Not _Album = LOADING_ALBUM) Then sTooltip &= vbNewLine & "Album: " & _Album
      '  If Not String.IsNullOrEmpty(_Genre) AndAlso (Not _Genre = Seed.clsID3v1.GenreName(13)) Then sTooltip &= vbNewLine & "Genre: " & _Genre
      '  If Not _Length = 0 Then sTooltip &= vbNewLine & "Length: " & ConvertTimeVal(_Length)
      '  dgvX.Cells(0).ToolTipText = sTooltip
      '  If SelectedPlayListItem = dgvX.Index Then
      '    SetTitleArtist(_Title, _Artist)
      '    FileTitle = _Artist & " - " & _Title
      '  End If
      'End If
    Next
    txtPlayListTitle.Tag = Album
    bDefaultedPlayListTitle = False
  End Sub

  Private Function CompareArtSizes(OldArt As Drawing.Image, NewArt As Drawing.Image) As Boolean
    Return OldArt.Width * OldArt.Height < NewArt.Width * NewArt.Height
  End Function

  Private Function CompareArtSizes(OldArt As String, NewArt As Drawing.Image) As Boolean
    Using iOldArt As Drawing.Image = Drawing.Image.FromFile(OldArt)
      If iOldArt Is Nothing Then Return True
      Return iOldArt.Width * iOldArt.Height < NewArt.Width * NewArt.Height
    End Using
  End Function

  Private Function CompareArtSizes(OldArt As Drawing.Image, NewArt As String) As Boolean
    Using iNewArt As Drawing.Image = Drawing.Image.FromFile(NewArt)
      If iNewArt Is Nothing Then Return False
      Return OldArt.Width * OldArt.Height < iNewArt.Width * iNewArt.Height
    End Using
  End Function

  Private Function CompareArtSizes(OldArt As String, NewArt As String) As Boolean
    Using iOldArt As Drawing.Image = Drawing.Image.FromFile(OldArt)
      If iOldArt Is Nothing Then Return True
      Using iNewArt As Drawing.Image = Drawing.Image.FromFile(NewArt)
        If iNewArt Is Nothing Then Return False
        Return iOldArt.Width * iOldArt.Height < iNewArt.Width * iNewArt.Height
      End Using
    End Using
  End Function

  Private Function CompareArtSizes(OldArt As Byte(), NewArt As String) As Boolean
    Dim sTmpPath As String = IO.Path.GetTempFileName
    IO.File.WriteAllBytes(sTmpPath, OldArt)
    Try
      Using iOldArt As Drawing.Image = PathToImg(sTmpPath)
        If iOldArt Is Nothing Then Return True
        Using iNewArt As Drawing.Image = Drawing.Image.FromFile(NewArt)
          If iNewArt Is Nothing Then Return False
          Return iOldArt.Width * iOldArt.Height < iNewArt.Width * iNewArt.Height
        End Using
      End Using
    Finally
      IO.File.Delete(sTmpPath)
    End Try
  End Function
#End Region

  Private showingFFVid As Boolean
  Private Sub pctQuality_MouseDoubleClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pctQuality.MouseDoubleClick
    If Not showingFFVid AndAlso (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Then
      showingFFVid = True
      mpPlayer.ShowProperties("ffdshow Video Decoder", 0)
      showingFFVid = False
    End If
  End Sub

  Private showingDS As Boolean
  Private showingVid As Boolean
  Private Sub pctBitrate_MouseDoubleClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pctBitrate.MouseDoubleClick
    If Not showingDS AndAlso (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Then
      showingDS = True
      mpPlayer.ShowProperties("Default DirectSound Device", 0)
      showingDS = False
    End If
    If Not showingVid AndAlso (e.Button And Windows.Forms.MouseButtons.Right) = Windows.Forms.MouseButtons.Right Then
      showingVid = True
      mpPlayer.ShowProperties("Video Renderer", 0)
      showingVid = False
    End If
  End Sub

  Private showingFFAud As Boolean
  Private Sub pctChannels_MouseDoubleClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles pctChannels.MouseDoubleClick
    If Not showingFFAud AndAlso (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Then
      showingFFAud = True
      mpPlayer.ShowProperties("ffdshow Audio Decoder", 0)
      showingFFAud = False
    End If
  End Sub

  Private Sub mnuTransferFile_Click(sender As System.Object, e As System.EventArgs) Handles mnuTransferFile.Click
    If frmTransfer.ShowDialog(Me) = Windows.Forms.DialogResult.Yes Then
      ClearPlayList()
      SaveTempPL(False)
      Me.Close()
    End If
  End Sub

  Private Sub mpPlayer_NetDownload(sender As Object, e As Seed.ctlSeed.DownloadChangedEventArgs) Handles mpPlayer.NetDownload
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New EventHandler(AddressOf mpPlayer_NetDownload), sender, e)
      Catch ex As Exception
      End Try
      Return
    End If
    pbProgress.Enabled = False
    pbProgress.Maximum = 1000
    pbProgress.Value = Math.Round(e.BytesReceived / e.TotalBytesToReceive * 100, 1) * 10
    If e.UserState = "BUFFER" Then
      If Not Me.Text = "Buffering (" & pbProgress.Value \ 10 & "%)" Then Me.Text = "Buffering (" & pbProgress.Value \ 10 & "%)"
    Else
      If Me.Text.StartsWith("Lime Seed Media Player") AndAlso Not Me.Text = "Lime Seed Media Player (" & pbProgress.Value \ 10 & "%)" Then Me.Text = "Lime Seed Media Player (" & pbProgress.Value \ 10 & "%)"
    End If
  End Sub

  Private Sub trayIcon_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles trayIcon.MouseClick
    If clsGlass.IsCompositionEnabled Then
      If (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Then
        If frmTI IsNot Nothing AndAlso Not frmTI.IsDisposed Then
          frmTI.Dispose()
          frmTI = Nothing
        Else
          frmTI = New frmTray
          frmTI.ownerForm = Me
          frmTI.Perma = True
          frmTI.Show()
          frmTI.GlassText = trayIcon.Text
          frmTI.Location = New Drawing.Point(MousePosition.X, MousePosition.Y - frmTI.Height - (DefaultCursor.Size.Height / 2))
        End If
      ElseIf (e.Button And Windows.Forms.MouseButtons.Right) = Windows.Forms.MouseButtons.Right Then
        If frmTI IsNot Nothing Then
          frmTI.Dispose()
          frmTI = Nothing
        End If
        frmTI = New frmTray
        frmTI.ownerForm = Me
        frmTI.Perma = False
        frmTI.Show()
        frmTI.GlassText = trayIcon.Text
        frmTI.Location = New Drawing.Point(MousePosition.X, MousePosition.Y - frmTI.Height - (DefaultCursor.Size.Height / 2))
      End If
    Else
      If (e.Button And Windows.Forms.MouseButtons.Left) = Windows.Forms.MouseButtons.Left Or (e.Button And Windows.Forms.MouseButtons.Right) = Windows.Forms.MouseButtons.Right Then
        Me.Show()
        trayIcon.Visible = False
      End If
    End If
  End Sub

  Private Sub tmrHideAlert_Tick(sender As System.Object, e As System.EventArgs) Handles tmrHideAlert.Tick
    tmrHideAlert.Enabled = False
    Do Until pnlPlayList.RowStyles(1).Height < 1
      pnlPlayList.RowStyles(1).Height -= 1
      Application.DoEvents()
    Loop
    lblPLAlert.Text = Nothing
  End Sub

  Private Sub tmrTray_Tick(sender As System.Object, e As System.EventArgs) Handles tmrTray.Tick
    If Not cmdStop.Enabled Then
      trayIcon.Icon = My.Resources.tray_stop
    ElseIf (bCD AndAlso cCD.Status = Seed.clsAudioCD.PlayStatus.Paused) OrElse (Not bCD AndAlso mpPlayer.State = Seed.ctlSeed.MediaState.mPaused) Then
      If CompareImages(trayIcon.Icon, My.Resources.tray_pause0) Then
        trayIcon.Icon = My.Resources.tray_pause1
      Else
        trayIcon.Icon = My.Resources.tray_pause0
      End If
    ElseIf (bCD AndAlso cCD.Status = Seed.clsAudioCD.PlayStatus.Playing) OrElse (Not bCD AndAlso mpPlayer.State = Seed.ctlSeed.MediaState.mPlaying) Then
      If CompareImages(trayIcon.Icon, My.Resources.tray_play0) Then
        trayIcon.Icon = My.Resources.tray_play1
      ElseIf CompareImages(trayIcon.Icon, My.Resources.tray_play1) Then
        trayIcon.Icon = My.Resources.tray_play2
      ElseIf CompareImages(trayIcon.Icon, My.Resources.tray_play2) Then
        trayIcon.Icon = My.Resources.tray_play3
      ElseIf CompareImages(trayIcon.Icon, My.Resources.tray_play3) Then
        trayIcon.Icon = My.Resources.tray_play4
      ElseIf CompareImages(trayIcon.Icon, My.Resources.tray_play4) Then
        trayIcon.Icon = My.Resources.tray_play5
      ElseIf CompareImages(trayIcon.Icon, My.Resources.tray_play5) Then
        trayIcon.Icon = My.Resources.tray_play6
      ElseIf CompareImages(trayIcon.Icon, My.Resources.tray_play6) Then
        trayIcon.Icon = My.Resources.tray_play7
      Else
        trayIcon.Icon = My.Resources.tray_play0
      End If
    Else
      trayIcon.Icon = My.Resources.tray
    End If
  End Sub

  Private Sub PLItems_PlayListUpdate(sender As Object, e As PlayListItems.PLUpdateEventArgs) Handles PLItems.PlayListUpdate
    If Me.IsDisposed Or Me.Disposing Then Return
    If Me.InvokeRequired Then
      Try
        Me.Invoke(New EventHandler(Of PlayListItems.PLUpdateEventArgs)(AddressOf PLItems_PlayListUpdate), sender, e)
      Catch ex As Exception
      End Try
      Return
    End If
    SyncLock dgvPlayList
      Select Case e.ChangeType
        Case PlayListItems.PLUpdateEventArgs.ChangeTypes.Added
          Dim SourceItem As PlayListItem = PLItems.PlayListItem(e.SourceIndex)
          Dim Title As String = SourceItem.Title
          Dim Artist As String = SourceItem.Artist
          Dim Album As String = SourceItem.Album
          Dim Genre As String = SourceItem.Genre
          Dim Length As String = SourceItem.Length
          Dim TT As String = SourceItem.ToolTipText(True)
          Dim dgvX As DataGridViewRow = dgvPlayList.Rows(dgvPlayList.Rows.Add({Title, Length}))
          dgvX.Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
          dgvX.Cells(0).ToolTipText = TT
          dgvX.Cells(1).ToolTipText = TT
          ThreadedQueue()

          Dim tRun As New Threading.Tasks.Task(Of PlayListItemResponse)(AddressOf PlayList_ParseItem, PLItems.PlayListItem(dgvX.Index), taskCancel.Token)
          tRun.ContinueWith(AddressOf PlayList_ItemParsed, taskContext)
          If taskCancel.Token.IsCancellationRequested Then Return
          tRun.Start()

          'Dim tRun As New List(Of Threading.Tasks.Task(Of PlayListItemResponse))

          'tRun.Add(taskPlayList.StartNew(AddressOf PlayList_ParseItem, PLItems.PlayListItem(dgvX.Index)))

          'taskPlayList.ContinueWhenAll(Of PlayListItemResponse)(tRun.ToArray, AddressOf PlayList_ItemsParsed)

          'GetPLData(dgvX.Index)
          'If dgvX.Index Mod BIGWAIT = 0 Then Application.DoEvents()
        Case PlayListItems.PLUpdateEventArgs.ChangeTypes.Selected
          Dim visibleCells As Integer = Math.Floor(dgvPlayList.Height / dgvPlayList.RowTemplate.Height)
          If visibleCells < 0 Or dgvPlayList.Height = 1 Then visibleCells = 0
          Dim wasInVisRange As Boolean = False
          If e.SourceIndex = -1 Then
            wasInVisRange = True
          Else
            For I As Integer = dgvPlayList.FirstDisplayedScrollingRowIndex To (dgvPlayList.FirstDisplayedScrollingRowIndex + visibleCells)
              If e.SourceIndex = I Then
                wasInVisRange = True
                Exit For
              End If
            Next
          End If
          For I As Integer = 0 To dgvPlayList.RowCount - 1
            If e.DestIndex = I Then
              If I Mod 2 = 0 Then
                If Not dgvPlayList.Rows(I).Cells(0).Style.BackColor = Drawing.SystemColors.GradientActiveCaption Then dgvPlayList.Rows(I).Cells(0).Style.BackColor = Drawing.SystemColors.GradientActiveCaption
                If Not dgvPlayList.Rows(I).Cells(1).Style.BackColor = Drawing.SystemColors.GradientActiveCaption Then dgvPlayList.Rows(I).Cells(1).Style.BackColor = Drawing.SystemColors.GradientActiveCaption
              Else
                If Not dgvPlayList.Rows(I).Cells(0).Style.BackColor = Drawing.SystemColors.ActiveCaption Then dgvPlayList.Rows(I).Cells(0).Style.BackColor = Drawing.SystemColors.ActiveCaption
                If Not dgvPlayList.Rows(I).Cells(1).Style.BackColor = Drawing.SystemColors.ActiveCaption Then dgvPlayList.Rows(I).Cells(1).Style.BackColor = Drawing.SystemColors.ActiveCaption
              End If
              If dgvPlayList.Rows(I).DefaultCellStyle.ForeColor = Drawing.SystemColors.WindowText Then dgvPlayList.Rows(I).DefaultCellStyle.ForeColor = Drawing.SystemColors.ActiveCaptionText
              If wasInVisRange Then
                If visibleCells > 0 Then
                  Dim centerPos As Integer = I - (Math.Ceiling(visibleCells / 2) - 1)
                  If centerPos > -1 Then
                    dgvPlayList.FirstDisplayedScrollingRowIndex = centerPos
                  Else
                    dgvPlayList.FirstDisplayedScrollingRowIndex = I
                  End If
                End If
              End If
            Else
              If Not dgvPlayList.Rows(I).Cells(0).Style.BackColor = dgvPlayList.Rows(I).DefaultCellStyle.BackColor Then dgvPlayList.Rows(I).Cells(0).Style.BackColor = dgvPlayList.Rows(I).DefaultCellStyle.BackColor
              If Not dgvPlayList.Rows(I).Cells(1).Style.BackColor = dgvPlayList.Rows(I).DefaultCellStyle.BackColor Then dgvPlayList.Rows(I).Cells(1).Style.BackColor = dgvPlayList.Rows(I).DefaultCellStyle.BackColor
              If dgvPlayList.Rows(I).DefaultCellStyle.ForeColor = Drawing.SystemColors.ActiveCaptionText Then dgvPlayList.Rows(I).DefaultCellStyle.ForeColor = Drawing.SystemColors.WindowText
            End If
          Next
        Case PlayListItems.PLUpdateEventArgs.ChangeTypes.NewIndex
          If dgvPlayList.RowCount > e.SourceIndex Then
            Dim SourceItem As PlayListItem = PLItems.PlayListItem(e.SourceIndex)
            If SourceItem IsNot Nothing Then
              Dim isLoading As Boolean = SourceItem.Artist = LOADING_ARTIST And SourceItem.Album = LOADING_ALBUM
              Dim Title As String = SourceItem.Title
              Dim Length As String = SourceItem.Length
              Dim TT As String = SourceItem.ToolTipText(isLoading)
              If Not dgvPlayList.Rows(e.SourceIndex).Cells(0).Value = Title Then dgvPlayList.Rows(e.SourceIndex).Cells(0).Value = Title
              If Not dgvPlayList.Rows(e.SourceIndex).Cells(1).Value = Length Then dgvPlayList.Rows(e.SourceIndex).Cells(1).Value = Length
              If Not dgvPlayList.Rows(e.SourceIndex).Cells(0).ToolTipText = TT Then dgvPlayList.Rows(e.SourceIndex).Cells(0).ToolTipText = TT
              If Not dgvPlayList.Rows(e.SourceIndex).Cells(1).ToolTipText = TT Then dgvPlayList.Rows(e.SourceIndex).Cells(1).ToolTipText = TT
              If isLoading Then
                If dgvPlayList.Rows(e.SourceIndex).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText Then dgvPlayList.Rows(e.SourceIndex).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
              Else
                If dgvPlayList.Rows(e.SourceIndex).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText Then dgvPlayList.Rows(e.SourceIndex).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
              End If
            End If
          End If
          Dim DestItem As PlayListItem = PLItems.PlayListItem(e.DestIndex)
          If DestItem IsNot Nothing Then
            Dim isLoading As Boolean = DestItem.Artist = LOADING_ARTIST And DestItem.Album = LOADING_ALBUM
            Dim Title As String = DestItem.Title
            Dim Length As String = DestItem.Length
            Dim TT As String = DestItem.ToolTipText(isLoading)
            If Not dgvPlayList.Rows(e.DestIndex).Cells(0).Value = Title Then dgvPlayList.Rows(e.DestIndex).Cells(0).Value = Title
            If Not dgvPlayList.Rows(e.DestIndex).Cells(1).Value = Length Then dgvPlayList.Rows(e.DestIndex).Cells(1).Value = Length
            If Not dgvPlayList.Rows(e.DestIndex).Cells(0).ToolTipText = TT Then dgvPlayList.Rows(e.DestIndex).Cells(0).ToolTipText = TT
            If Not dgvPlayList.Rows(e.DestIndex).Cells(1).ToolTipText = TT Then dgvPlayList.Rows(e.DestIndex).Cells(1).ToolTipText = TT
            If isLoading Then
              If dgvPlayList.Rows(e.DestIndex).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText Then dgvPlayList.Rows(e.DestIndex).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
            Else
              If dgvPlayList.Rows(e.DestIndex).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText Then dgvPlayList.Rows(e.DestIndex).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
            End If
          End If
        Case PlayListItems.PLUpdateEventArgs.ChangeTypes.Updated
          Dim lo As Integer = 0
          Dim hi As Integer = PLItems.Count - 1
          If e.SourceIndex > -1 And e.DestIndex > -1 Then
            lo = e.SourceIndex
            hi = e.DestIndex
          End If
          Dim setHeaders As Boolean = False
          If dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader Then
            setHeaders = True
            'Dim col0W As Integer = dgvPlayList.Columns(0).Width
            Dim col1W As Integer = dgvPlayList.Columns(1).Width
            'dgvPlayList.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            'dgvPlayList.Columns(0).Width = col0W
            dgvPlayList.Columns(1).Width = col1W
          End If
          For I As Integer = lo To hi
            Dim itm As PlayListItem = PLItems.PlayListItem(I)
            If itm Is Nothing Then Continue For
            Dim isLoading As Boolean = itm.Artist = LOADING_ARTIST And itm.Album = LOADING_ALBUM
            Dim itmTitle As String = itm.Title
            Dim itmLen As String = itm.Length
            Dim itmTT As String = itm.ToolTipText(isLoading)
            If Not dgvPlayList.Rows(I).Cells(0).Value = itmTitle Then dgvPlayList.Rows(I).Cells(0).Value = itmTitle
            If Not dgvPlayList.Rows(I).Cells(1).Value = itmLen Then dgvPlayList.Rows(I).Cells(1).Value = itmLen
            If Not dgvPlayList.Rows(I).Cells(0).ToolTipText = itmTT Then dgvPlayList.Rows(I).Cells(0).ToolTipText = itmTT
            If Not dgvPlayList.Rows(I).Cells(1).ToolTipText = itmTT Then dgvPlayList.Rows(I).Cells(1).ToolTipText = itmTT
            If isLoading Then
              If dgvPlayList.Rows(I).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText Then dgvPlayList.Rows(I).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText
            Else
              If dgvPlayList.Rows(I).Cells(0).Style.ForeColor = Drawing.SystemColors.GrayText Then dgvPlayList.Rows(I).Cells(0).Style.ForeColor = Drawing.SystemColors.WindowText
            End If
          Next
          If setHeaders Then
            If Not dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader Then
              'dgvPlayList.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
              Debug.Print("Resetting Length Column because headers set")
              dgvPlayList.Columns(1).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader
            End If
          End If
        Case PlayListItems.PLUpdateEventArgs.ChangeTypes.Focused
          Dim visibleCells As Integer = Math.Floor(dgvPlayList.Height / dgvPlayList.RowTemplate.Height)
          If visibleCells < 0 Or dgvPlayList.Height = 1 Then visibleCells = 0
          If visibleCells > 0 Then
            Dim centerPos As Integer = e.SourceIndex - (Math.Ceiling(visibleCells / 2) - 1)
            If centerPos > -1 Then
              dgvPlayList.FirstDisplayedScrollingRowIndex = centerPos
            Else
              dgvPlayList.FirstDisplayedScrollingRowIndex = e.SourceIndex
            End If
          End If
          dgvPlayList.Rows(e.SourceIndex).Selected = True
        Case PlayListItems.PLUpdateEventArgs.ChangeTypes.Removed
          dgvPlayList.Rows.RemoveAt(e.SourceIndex)
        Case PlayListItems.PLUpdateEventArgs.ChangeTypes.Cleared
          dgvPlayList.Rows.Clear()
      End Select
    End SyncLock
  End Sub

  Private Sub mnuPLDuplicates_Click(sender As System.Object, e As System.EventArgs) Handles mnuPLDuplicates.Click
    If mnuPLDuplicates.Checked Then
      PLItems.ClearDuplicates()
    End If
  End Sub
End Class
