Module modFuncts
  Public Const LATIN_1 As Integer = 28591
  Public Const UTF_8 As Integer = 949
  Public objStorageLock As New Object

  Public Function GenNextRndFile(Optional Extension As String = "mp3") As String
    SyncLock objStorageLock
      Static rI As Integer
      Dim sTmp As String
      Do
        rI += 1
        If rI > &HFFFFFF Then rI = 1
        sTmp = IO.Path.GetTempPath & "seedTEMP" & BufferHex(rI, 6) & "." & Extension
      Loop While IO.File.Exists(sTmp)
      Return sTmp
    End SyncLock
  End Function

  Public Function CheckMPEG(bFile As Byte(), ByVal lPos As Long) As Boolean
    Dim lLen As Long
    Dim cMPTest As New clsMPEG(GetDWORD(bFile, lPos))
    If cMPTest.CheckValidity Then
      lLen = cMPTest.GetFrameSize
      lPos += lLen
      If lPos = bFile.Length Or lPos = bFile.Length - &H80 Then Return True 'End of File, used to be false, but set to true because technically an MP3 file could only contain a single frame and then end...
      If lPos > bFile.Length Then Return False 'However, it should not be greater than this end, because that would be an incomplete frame or a fake header.
      cMPTest = New clsMPEG(GetDWORD(bFile, lPos))
      If cMPTest.CheckValidity Then
        lLen = cMPTest.GetFrameSize
        lPos += lLen
        If lPos = bFile.Length Or lPos = bFile.Length - &H80 Then Return True
        If lPos > bFile.Length Then Return False
        cMPTest = New clsMPEG(GetDWORD(bFile, lPos))
        If cMPTest.CheckValidity Then
          lLen = cMPTest.GetFrameSize
          lPos += lLen
          If lPos > bFile.Length Then Return False
          Return True
        End If
      End If
    End If
    cMPTest = Nothing
    Return False
  End Function

  Public Sub GetMKVDisplaySize(ByRef mkvHeader As Seed.clsMKVHeaders, ByRef Size As Drawing.Size, ByRef Crop As Drawing.Rectangle)
    For I As Integer = 0 To mkvHeader.TrackEntries.Length - 1
      If mkvHeader.TrackEntries(I).Video.Exists Then
        If mkvHeader.TrackEntries(I).Video.PixelCropLeft > 0 Or mkvHeader.TrackEntries(I).Video.PixelCropTop > 0 Or mkvHeader.TrackEntries(I).Video.PixelCropBottom > 0 Or mkvHeader.TrackEntries(I).Video.PixelCropRight > 0 Then Crop = New Drawing.Rectangle(mkvHeader.TrackEntries(I).Video.PixelCropLeft, mkvHeader.TrackEntries(I).Video.PixelCropTop, mkvHeader.TrackEntries(I).Video.PixelCropRight, mkvHeader.TrackEntries(I).Video.PixelCropBottom)
        Dim pX As UInt64 = mkvHeader.TrackEntries(I).Video.PixelWidth
        Dim pY As UInt64 = mkvHeader.TrackEntries(I).Video.PixelHeight
        Dim pRX As UInt64 = mkvHeader.TrackEntries(I).Video.DisplayWidth
        Dim pRY As UInt64 = mkvHeader.TrackEntries(I).Video.DisplayHeight
        If pRX > 0 And pRY > 0 Then
          Select Case mkvHeader.TrackEntries(I).Video.DisplayUnit
            Case 0
              If pRX < 100 And pRY < 100 Then
                'probably a ratio, not a resolution!
                If pX / pRX * pRY = pY Then
                  'No need to change!
                  Size = New Drawing.Size(pX, pY)
                  Exit For
                ElseIf pX / pRX * pRY > pY Then
                  Size = New Drawing.Size(pX, pX / pRX * pRY)
                  Exit For
                Else
                  Size = New Drawing.Size(pX / pRX * pRY, pY)
                  Exit For
                End If
                Size = New Drawing.Size(pRX, mkvHeader.TrackEntries(I).Video.DisplayHeight)
                Exit For
              Else
                Size = New Drawing.Size(pRX, pRY)
                Exit For
              End If
            Case 1
              Dim g As Drawing.Graphics = Drawing.Graphics.FromImage(New Drawing.Bitmap(1, 1))
              Size = New Drawing.Size(pRX * (g.DpiX * 2.54), pRY * (g.DpiY * 2.54))
              Exit For
            Case 2
              Dim g As Drawing.Graphics = Drawing.Graphics.FromImage(New Drawing.Bitmap(1, 1))
              Size = New Drawing.Size(pRX * g.DpiX, pRY * g.DpiY)
              Exit For
            Case 3
              'ensure ratio, increase a dimension if necessary
              If pX > 0 And pY > 0 Then
                If pX / pRX * pRY = pY Then
                  'No need to change!
                  Size = New Drawing.Size(pX, pY)
                  Exit For
                ElseIf pX / pRX * pRY > pY Then
                  Size = New Drawing.Size(pX, pX / pRX * pRY)
                  Exit For
                Else
                  Debug.Print("Less")
                  Size = New Drawing.Size(pX / pRX * pRY, pY)
                  Exit For
                End If
              End If
            Case 255
              If pRX < 100 And pRY < 100 Then
                'probably a ratio, not a resolution!
                If pX / pRX * pRY = pY Then
                  'No need to change!
                  Size = New Drawing.Size(pX, pY)
                  Exit For
                ElseIf pX / pRX * pRY > pY Then
                  Size = New Drawing.Size(pX, pX / pRX * pRY)
                  Exit For
                Else
                  Size = New Drawing.Size(pX / pRX * pRY, pY)
                  Exit For
                End If
                Size = New Drawing.Size(pRX, mkvHeader.TrackEntries(I).Video.DisplayHeight)
                Exit For
              Else
                Size = New Drawing.Size(pRX, pRY)
                Exit For
              End If
          End Select
        End If
        If pX > 0 And pY > 0 Then
          Size = New Drawing.Size(pX, pY)
          Exit For
        End If
      End If
    Next
  End Sub


  Public Function HRMessage(hr As Integer) As String
    Select Case hr
      Case &H80040200 : Return "Invalid Media Type"
      Case &H80040201 : Return "Invalid Media Subtype"
      Case &H80040202 : Return "Media Needs Owner"
      Case &H80040203 : Return "Numerated Object Out of Sync"
      Case &H80040204 : Return "Pin Already Connected"
      Case &H80040205 : Return "Filter Already Active"
      Case &H80040206 : Return "No Media Types"
      Case &H80040207 : Return "No Acceptable Types"
      Case &H80040208 : Return "Two pins of the same direction cannot be connected"
      Case &H80040209 : Return "Pins Not Connected"
      Case &H8004020A : Return "No Sample Buffer Allocator Avaliable"
      Case &H8004020B : Return "Run-time Error"
      Case &H8004020C : Return "No Buffer Space"
      Case &H8004020D : Return "Buffer too Small"
      Case &H8004020E : Return "Invalid Alignment"
      Case &H8004020F : Return "Allocator Already Committed"
      Case &H80040210 : Return "Buffer Still Active"
      Case &H80040211 : Return "Allocator not Active"
      Case &H80040212 : Return "No Size Set"
      Case &H80040213 : Return "No Clock Defined"
      Case &H80040214 : Return "No Quality Sink Defined"
      Case &H80040215 : Return "Interface not Implemented"
      Case &H80040216 : Return "Object not Found"
      Case &H80040217 : Return "Can't Connect"
      Case &H80040218 : Return "Can't Render"
      Case &H80040219 : Return "Can't Change Formats"
      Case &H8004021A : Return "No Color Key"
      Case &H8004021B : Return "Not iOverlay"
      Case &H8004021C : Return "Not iMemInputPin"
      Case &H8004021D : Return "Palette Already Set"
      Case &H8004021E : Return "Color Key Already Set"
      Case &H8004021F : Return "No Color Key"
      Case &H80040220 : Return "No Palette"
      Case &H80040221 : Return "Display has no Palette"
      Case &H80040222 : Return "Too many Colors"
      Case &H80040223 : Return "State Changed while Waiting"
      Case &H80040224 : Return "Not Stopped"
      Case &H80040225 : Return "Not Paused"
      Case &H80040226 : Return "Not Running"
      Case &H80040227 : Return "Wrong State"
      Case &H80040228 : Return "Start is after End"
      Case &H80040229 : Return "Invalid Rect"
      Case &H8004022A : Return "Type not Accepted"
      Case &H8004022B : Return "Sample Can't be Rendered"
      Case &H8004022C : Return "End of Stream"
      Case &H8004022D : Return "Duplicate Name"
      Case &H8004022E : Return "Time-out"
      Case &H8004022F : Return "Invalid File Format"
      Case &H80040230 : Return "Enum Out of Range"
      Case &H80040231 : Return "Filter Graph is Circular"
      Case &H80040232 : Return "Not allowed to Save"
      Case &H80040233 : Return "Time Already Passed"
      Case &H80040234 : Return "Command Already Canceled"
      Case &H80040235 : Return "File Corrupt"
      Case &H80040236 : Return "iOverlay Already Set"
      Case &H80040238 : Return "No Full-Screen Modes Available"
      Case &H80040239 : Return "No Advise Set"
      Case &H8004023A : Return "No Full-Screen"
      Case &H8004023B : Return "In Full-Screen"
      Case &H80040240 : Return "Unknown File Type"
      Case &H80040241 : Return "Can't Load Filter"
      Case &H80040243 : Return "File Incomplete"
      Case &H80040244 : Return "Invalid File Version"
      Case &H80040247 : Return "Invalid CLSID"
      Case &H80040248 : Return "Invalid Media Type"
      Case &H80040249 : Return "No Timestamp"
      Case &H80040251 : Return "No Media Time"
      Case &H80040252 : Return "No Media Time Selected"
      Case &H80040253 : Return "Audio is Mono"
      Case &H80040255 : Return "No Decompressor"
      Case &H80040256 : Return "No Audio Hardware"
      Case &H80040259 : Return "RPZA not Supported"
      Case &H8004025B : Return "Processor can't play MPEG"
      Case &H8004025C : Return "Unsupported Audio"
      Case &H8004025D : Return "Unsupported Video"
      Case &H8004025E : Return "MPEG not Constrained"
      Case &H8004025F : Return "Object not in Graph"
      Case &H80040261 : Return "Can't access Time Format"
      Case &H80040262 : Return "Read Only"
      Case &H80040264 : Return "Buffer is Not Full Enough"
      Case &H80040265 : Return "Stream Not Supported"
      Case &H80040266 : Return "Transport Not Supported"
      Case &H80040269 : Return "Bad Video CD"
      Case &H80040270 : Return "No Stop Time"
      Case &H80040271 : Return "Video out of Memory"
      Case &H80040272 : Return "Video Port Negotiation Failed"
      Case &H80040273 : Return "DirectDraw can't run on this Video Card"
      Case &H80040274 : Return "No Video Port"
      Case &H80040275 : Return "No Capture Hardware"
      Case &H80040276 : Return "Prohibited DVD Action"
      Case &H80040277 : Return "Invaild DVD Domain"
      Case &H80040278 : Return "DVD Button Not Available"
      Case &H80040279 : Return "DVD Graph Not Ready"
      Case &H8004027A : Return "DVD Playback Render Failed"
      Case &H8004027B : Return "DVD Playback needs Decoder"
      Case &H8004027C : Return "DirectDraw Version 5+ Required"
      Case &H8004027D : Return "Copy Protection could not be Enabled"
      Case &H8004027F : Return "Seek Timed Out"
      Case &H80040281 : Return "Wrong DVD Playback Speed"
      Case &H80040282 : Return "DVD Menu Does not Exist"
      Case &H80040283 : Return "DVD Command Cancelled"
      Case &H80040284 : Return "Wrong DVD State Version"
      Case &H80040285 : Return "DVD State Corrupt"
      Case &H80040286 : Return "DVD State for Wrong Disc"
      Case &H80040287 : Return "Wrong DVD Region"
      Case &H80040288 : Return "Attributes Don't Exist"
      Case &H80040289 : Return "No GoUp Program Chain Available"
      Case &H8004028A : Return "Parental Settings Restrict this DVD"
      Case &H8004028B : Return "DVD not in Karaoke Mode"
      Case &H8004028E : Return "Frame Stepping not Supported"
      Case &H8004028F : Return "DVD Stream Disabled"
      Case &H80040290 : Return "DVD Title Unknown"
      Case &H80040291 : Return "DVD Disc Invalid"
      Case &H80040292 : Return "No DVD Resume Information"
      Case &H80040293 : Return "Blocked on Caller"
      Case &H80040294 : Return "Blocked on Thread"
      Case &H80040295 : Return "Certification Failure"
      Case &H80040296 : Return "VMR not in Mixing Mode"
      Case &H80040297 : Return "No Allocator-Presenter Supplied"
      Case &H80040298 : Return "No De-Interlacing Hardware"
      Case &H80040299 : Return "No ProcAmp Controls"
      Case &H8004029A : Return "DVD not Compatible with VMR-9"
      Case &H8004029B : Return "No Content Output Protection Hardware"
      Case &H800403F2 : Return "Registry Corruption"
      Case &H8004029C : Return "DVD Action Pending"
      Case &H8004029D : Return "DVD Graph contains more than one Video Renderer"
      Case &H8004029E : Return "DVD Navigator Already has Renderer"
      Case &H8004029F : Return "DVD Resolution Output Wrong"
      Case &H80040310 : Return "Codec Parameter has Linear Range, not Enumeration"
      Case &H80040311 : Return "Codec Parameter has Enumeration, not Linear Range"
      Case &H80040313 : Return "Codec Parameter has no Default"
      Case &H80040314 : Return "Codec Parameter has no Value"
      Case &H80070490 : Return "Property ID not Supported"
      Case &H80070492 : Return "Property Set not Supported"
      Case Else : Return Runtime.InteropServices.Marshal.GetExceptionForHR(hr).Message  'Return "0x" & BufferHex(hr, 8)
    End Select
  End Function
End Module


Public Class clsAudioCD
  Implements IDisposable

#Region "Members"

  Private _CurrentDrive As Char
  Private _CDStatus As PlayStatus = PlayStatus.Stopped

  Private CDDoorOpen As Boolean = False

  Private ReturnCode As System.Text.StringBuilder
  Private Timer As System.Windows.Forms.Timer

  Private aliasID As String = "cd"

#End Region

#Region "Properties"

  Public ReadOnly Property TotalTracks() As UInt32
    Get
      Try
        Return Convert.ToUInt32(Me.CDMediaInfo(MediaOption.Tracks, Me.CurrentTrack))
      Catch ex As Exception
        Return 0
      End Try
    End Get
  End Property

  Public Property CurrentTrack() As UInt32
    Get
      Try
        Return Me.CDMediaInfo(MediaOption.CurrentTrack)
      Catch ex As Exception
        Return 0
      End Try
    End Get
    Set(value As UInt32)
      Me.SetTimeFormat(TimeFormat.TMSF)
      If Me.CurrentTrack <> value And value > 0 And value <= Me.TotalTracks Then Me.SeekPlay(SeekOption.Seek, value)
    End Set
  End Property

  Public ReadOnly Property TrackDurationSeconds(Optional Track As Integer = 0) As Integer
    Get
      Try
        Me.SetTimeFormat(TimeFormat.MS)
        If Track = 0 Then Track = Me.CurrentTrack
        Return Convert.ToUInt32(Me.CDMediaInfo(MediaOption.TrackLength, Track)) / 1000
      Catch ex As Exception
        Return 0
      End Try
    End Get
  End Property

  Public Property TrackPositionSeconds() As Integer
    Get
      Try
        Me.SetTimeFormat(TimeFormat.MS)
        Return (Convert.ToUInt32(Me.CDMediaInfo(MediaOption.CurrentPosition)) - Me.TrackStart) / 1000
      Catch ex As Exception
        Return 0
      End Try
    End Get
    Set(value As Integer)
      Me.MovePositionMS(value * 1000)
    End Set
  End Property

  Public ReadOnly Property TrackDurationMS(Optional Track As Integer = 0) As Integer
    Get
      Try
        Me.SetTimeFormat(TimeFormat.MS)
        If Track = 0 Then Track = Me.CurrentTrack
        Return Convert.ToUInt32(Me.CDMediaInfo(MediaOption.TrackLength, Track))
      Catch ex As Exception
        Return 0
      End Try
    End Get
  End Property

  Public Property TrackPositionMS() As Integer
    Get
      Try
        Me.SetTimeFormat(TimeFormat.MS)
        Return Convert.ToUInt32(Me.CDMediaInfo(MediaOption.CurrentPosition)) - Me.TrackStart
      Catch ex As Exception
        Return 0
      End Try
    End Get
    Set(value As Integer)
      Me.MovePositionMS(value)
    End Set
  End Property

  Public Property CurrentDrive() As Char
    Get
      Return _CurrentDrive
    End Get
    Set(value As Char)
      _CurrentDrive = value
      aliasID = "cd" & value
      Me.ChangeDevice()
    End Set
  End Property

  Public ReadOnly Property Status() As PlayStatus
    Get
      Return Me._CDStatus
    End Get
  End Property

  Public ReadOnly Property CDAvailable() As Boolean
    Get
      Return Me.CdMediaStatus()
    End Get
  End Property

#End Region

#Region "Enums"

  Public Enum PlayStatus
    Playing = 0
    Stopped = 1
    Paused = 2
  End Enum

  Public Enum AudioOption
    OpenCDAudio = 0
    CloseCDAudio = 1
  End Enum

  Public Enum DoorOption
    Close = 0
    Open = 1
  End Enum

  Public Enum ActionOption
    Play = 0
    Pause = 1
    [Stop] = 2
  End Enum

  Public Enum TimeFormat
    MS = 0
    MSF = 1
    TMSF = 2
  End Enum

  Public Enum MediaOption
    Length = 0
    Tracks = 1
    CurrentPosition = 2
    TrackLength = 3
    TrackPosition = 4
    CurrentTrack = 5
  End Enum

  Public Enum SeekOption
    Play = 0
    Seek = 1
  End Enum

#End Region

#Region "Constructor"

  Public Sub New()
    ' Timer
    Me.Timer = New System.Windows.Forms.Timer()
    Me.Timer.Enabled = True
    Me.Timer.Interval = 200
    AddHandler Me.Timer.Tick, New System.EventHandler(AddressOf Me.Timer_Tick)
  End Sub

#End Region

#Region "Methods"

  Public Sub Closing()
    Try
      Me.BasicAction(ActionOption.Stop)
      Me.AccessCDAudio(AudioOption.CloseCDAudio)
    Catch
    End Try
  End Sub

  Public Sub Eject()
    Me.Timer.Enabled = False

    If Me.CDDoorOpen Then
      Me.ActivateDoor(DoorOption.Close)
      Me.Timer.Enabled = False
      Me.CurrentTrack = 1
      Me.Timer.Enabled = True
    Else
      Me.ActivateDoor(DoorOption.Open)
    End If
    Me.CDDoorOpen = Not Me.CDDoorOpen

    Me.Timer.Enabled = True
  End Sub

  Public Sub Play()
    Me.Timer.Enabled = False

    If Me.CdMediaStatus() Then
      If Me._CDStatus = PlayStatus.Playing Then
        Me.BasicAction(ActionOption.Pause)
        Me._CDStatus = PlayStatus.Paused
      ElseIf Me._CDStatus = PlayStatus.Paused Then
        Dim TempPosition As Integer = Me.TrackPositionMS
        Me.PlayTrack(Me.CurrentTrack)
        Me._CDStatus = PlayStatus.Playing
        MovePositionMS(TempPosition)
      ElseIf Me._CDStatus = PlayStatus.Stopped Then
        Me.PlayTrack(Me.CurrentTrack)
        Me._CDStatus = PlayStatus.Playing
      End If
    End If

    Me.Timer.Enabled = True
  End Sub

  Public Sub Pause()
    Me.Timer.Enabled = False

    If Me.CdMediaStatus() Then
      If Me._CDStatus = PlayStatus.Playing Then
        Me.BasicAction(ActionOption.Pause)
        Me._CDStatus = PlayStatus.Paused
      ElseIf Me._CDStatus = PlayStatus.Paused OrElse Me._CDStatus = PlayStatus.Stopped Then
        Dim TempPosition As Integer = Me.TrackPositionMS
        Me.PlayTrack(Me.CurrentTrack)
        Me._CDStatus = PlayStatus.Playing
        MovePositionMS(TempPosition)
      End If
    End If

    Me.Timer.Enabled = True
  End Sub

  Public Sub [Stop]()
    Me.Timer.Enabled = False

    If Me.CdMediaStatus() Then
      Me.BasicAction(ActionOption.Stop)
      Me._CDStatus = PlayStatus.Stopped
    End If

    Me.Timer.Enabled = True
  End Sub

  Public Sub Previous()
    If Me.CurrentTrack > 1 Then
      If Me._CDStatus = PlayStatus.Paused Then Me.Play()
      Me.CurrentTrack -= 1
      If Me._CDStatus = PlayStatus.Playing Then PlayTrack(Me.CurrentTrack)
    End If
  End Sub

  Public Sub [Next]()
    If Me.CurrentTrack < Me.TotalTracks Then
      If Me._CDStatus = PlayStatus.Paused Then Me.Play()
      Me.CurrentTrack += 1
      If Me._CDStatus = PlayStatus.Playing Then PlayTrack(Me.CurrentTrack)
    End If
  End Sub

  Public Sub ChangeDrive(Drive As Char)
    Me.Timer.Enabled = False
    Me.Stop()
    Me.CurrentDrive = Drive
    Me.CurrentTrack = 1
    Me.Timer.Enabled = True
  End Sub

  Public Sub MovePositionMS(PositionInMS As Integer)
    Dim Position As Integer = Me.TrackStart + PositionInMS
    If Me.CdMediaStatus() Then
      Me.Timer.Enabled = False
      If Me._CDStatus = PlayStatus.Playing Then
        Me.SetTimeFormat(TimeFormat.MS)
        Me.SeekPlay(SeekOption.Play, Position)
      End If
      Me.Timer.Enabled = True
    End If
  End Sub

  Public ReadOnly Property TrackStart(Optional Track As UInt32 = 0)
    Get
      Me.SetTimeFormat(TimeFormat.MS)
      If Track = 0 Then Track = Me.CurrentTrack
      Return Convert.ToInt32(Me.CDMediaInfo(MediaOption.TrackPosition, Track))
    End Get
  End Property

  Private Sub Timer_Tick(sender As Object, e As System.EventArgs)
    If Me.CdMediaStatus() Then
      If (Me._CDStatus = PlayStatus.Playing) AndAlso (Me.TrackPositionMS >= Me.TrackDurationMS) Then
        If Me.CurrentTrack < Me.TotalTracks Then
          Me.CurrentTrack += 1
        Else
          Me.Stop()
        End If
      End If
    End If
  End Sub

  Private Sub PlayTrack(Track As Integer)
    Me.CurrentTrack = Track
    If Me.CdMediaStatus() Then
      Me.SetTimeFormat(TimeFormat.TMSF)
      Me.SeekPlay(SeekOption.Play, Me.CurrentTrack)
    End If
  End Sub

  Private Sub ChangeDevice()
    If Me.CDMediaStatus() Then Me.BasicAction(ActionOption.Stop)
    Me.AccessCDAudio(AudioOption.CloseCDAudio)
    If Me.CDMediaStatus() Then Me.BasicAction(ActionOption.Stop)
    Dim Result As Integer
    Result = Me.AccessCDAudio(AudioOption.OpenCDAudio)
    Result = Me.SetTimeFormat(TimeFormat.TMSF)
  End Sub

  Private Function AccessCDAudio(Action As AudioOption) As Integer
    ReturnCode = New System.Text.StringBuilder(StrDup(256, " "c))
    Dim CommandLine As String = String.Empty
    Select Case Action
      Case AudioOption.OpenCDAudio : CommandLine = "open """ & CurrentDrive & ":\"" type cdaudio alias " & aliasID
      Case AudioOption.CloseCDAudio : CommandLine = "close " & aliasID
    End Select
    Return SeedFuncts.mciSendString(CommandLine, ReturnCode, ReturnCode.Length, New IntPtr(0))
  End Function

  Private Function BasicAction(Action As ActionOption) As Integer
    ReturnCode = New System.Text.StringBuilder(StrDup(256, " "c))
    Dim CommandLine As String = String.Empty
    Select Case Action
      Case ActionOption.Pause : CommandLine = "pause " & aliasID
      Case ActionOption.Play : CommandLine = "play " & aliasID
      Case ActionOption.Stop : CommandLine = "stop " & aliasID & " wait"
    End Select
    Return SeedFuncts.mciSendString(CommandLine, ReturnCode, ReturnCode.Length, New IntPtr(0))
  End Function

  Public Function SetTimeFormat(TF As TimeFormat) As Integer
    ReturnCode = New System.Text.StringBuilder(StrDup(256, " "c))
    Dim CommandLine As String = "set " & aliasID & " time format "
    Select Case TF
      Case TimeFormat.MS : CommandLine &= "ms"
      Case TimeFormat.MSF : CommandLine &= "msf wait"
      Case TimeFormat.TMSF : CommandLine &= "tmsf wait"
    End Select
    Return SeedFuncts.mciSendString(CommandLine, ReturnCode, ReturnCode.Length, New IntPtr(0))
  End Function

  Private Function ActivateDoor(Action As DoorOption) As Integer
    ReturnCode = New System.Text.StringBuilder(StrDup(256, " "c))
    Dim CommandLine As String = "set " & aliasID & " door "
    Select Case Action
      Case DoorOption.Open : CommandLine &= "open"
      Case DoorOption.Close : CommandLine &= "closed"
    End Select
    Return SeedFuncts.mciSendString(CommandLine, ReturnCode, ReturnCode.Length, New IntPtr(0))
  End Function

  Public Shared Sub CDTray(Drive As Char, action As DoorOption)
    Dim ReturnC As New System.Text.StringBuilder(StrDup(256, " "c))
    SeedFuncts.mciSendString("open """ & Drive & ":\"" type cdaudio alias tray", ReturnC, ReturnC.Length, New IntPtr(0))
    ReturnC = New System.Text.StringBuilder(StrDup(256, " "c))
    Dim CommandLine As String = "set tray door "
    Select Case action
      Case DoorOption.Open : CommandLine &= "open"
      Case DoorOption.Close : CommandLine &= "closed"
    End Select
    SeedFuncts.mciSendString(CommandLine, ReturnC, ReturnC.Length, New IntPtr(0))
    ReturnC = New System.Text.StringBuilder(StrDup(256, " "c))
    SeedFuncts.mciSendString("close tray", ReturnC, ReturnC.Length, New IntPtr(0))
  End Sub

  Public Function CDMediaInfo(Action As MediaOption, Optional Track As Integer = 0) As String
    ReturnCode = New System.Text.StringBuilder(StrDup(256, " "c))
    Dim CommandLine As String = String.Empty
    Select Case Action
      Case MediaOption.Length : CommandLine = "status " & aliasID & " length wait"
      Case MediaOption.Tracks : CommandLine = "status " & aliasID & " number of tracks wait"
      Case MediaOption.CurrentPosition : CommandLine = "status " & aliasID & " position"
      Case MediaOption.TrackPosition
        If Track = 0 Then Track = Me.CurrentTrack
        CommandLine = "status " & aliasID & " position track " & Convert.ToString(Track)
      Case MediaOption.TrackLength
        If Track = 0 Then Track = Me.CurrentTrack
        CommandLine = "status " & aliasID & " length track " & Convert.ToString(Track)
      Case MediaOption.CurrentTrack : CommandLine = "status " & aliasID & " current track"
    End Select
    SeedFuncts.mciSendString(CommandLine, ReturnCode, ReturnCode.Length, New IntPtr(0))
    Return FixString(ReturnCode.ToString())
  End Function

  Private Function SeekPlay(Action As SeekOption, Track As Integer) As Boolean
    ReturnCode = New System.Text.StringBuilder(StrDup(256, " "c))
    Dim CommandLine As String = String.Empty
    Select Case Action
      Case SeekOption.Play : CommandLine = "play " & aliasID & " from " & Track.ToString()
        Exit Select
      Case SeekOption.Seek : CommandLine = "seek " & aliasID & " to " & Track.ToString()
        Exit Select
    End Select
    Return Convert.ToBoolean(SeedFuncts.mciSendString(CommandLine, ReturnCode, ReturnCode.Length, New IntPtr(0)))
  End Function

  Private Function CDMediaStatus() As Boolean
    ReturnCode = New System.Text.StringBuilder(StrDup(256, " "c))
    Dim CommandLine As String = "status " & aliasID & " media present"
    SeedFuncts.mciSendString(CommandLine, ReturnCode, ReturnCode.Length, New IntPtr(0))
    If ReturnCode.ToString().Trim() <> "" Then
      Return Convert.ToBoolean(FixString(ReturnCode.ToString()))
    Else
      Return False
    End If
  End Function

  Private Function FixString(BadString As String) As String
    Dim FixedString As String = ""
    Dim TempString As String = ""
    For i As Integer = 0 To BadString.Length - 1
      TempString = BadString.Substring(i, 1)
      Dim character As Char = TempString(0)
      If (AscW(character) > 32) AndAlso (AscW(character) < 123) Then
        FixedString += TempString
      End If
    Next
    Return FixedString
  End Function
#End Region

#Region "IDisposable Support"
  Private disposedValue As Boolean 
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        Closing()
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