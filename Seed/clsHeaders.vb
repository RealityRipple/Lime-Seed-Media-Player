Public Class clsHeaderLoader
  Implements IDisposable
  Public cMPEG As clsMPEG
  Public cXING As clsXING
  Public cVBRI As clsVBRI
  Public cID3v1 As clsID3v1
  Public cID3v2 As clsID3v2
  Private lSize As Long
  '0 = CBR
  '1 = VBR XING
  '2 = VBR Fraunhofer
  Private bFmt As Byte
  Private fStart As Long
  Private fLen As Long
  Private sPath As String

  Public Sub New(FilePath As String)
    If String.IsNullOrEmpty(FilePath) Then Return
    If Not My.Computer.FileSystem.FileExists(FilePath) Then Return
    If My.Computer.FileSystem.GetFileInfo(FilePath).Length >= 1024L * 1024L * 1024L * 2L Then Return
    sPath = FilePath
    Dim bFile As Byte() = My.Computer.FileSystem.ReadAllBytes(FilePath)
    If bFile.Length <= &H80 Then Return
    lSize = bFile.Length
    Dim Id3v1 As Boolean = (GetString(bFile, bFile.Length - &H80, 3) = "TAG")
    Dim lFram As Integer = -1
    Do
      lFram = GetBytePos(bFile, &HFF, lFram)
      If lFram >= 0 Then
        If CheckMPEG(bFile, lFram) Then
          Dim cMPGHead As New clsMPEG(GetDWORD(bFile, lFram)) ' Left$(sFile, 4)))
          Dim lFrameSize As Long = cMPGHead.GetFrameSize
          Dim bTmp(lFrameSize - 1) As Byte
          Array.ConstrainedCopy(bFile, lFram, bTmp, 0, lFrameSize)
          ReDim bFile(lFrameSize - 1)
          Array.Copy(bTmp, bFile, lFrameSize)
          If lFram = -1 Then lFram = 0
          Exit Do
        End If
      Else
        Exit Do
      End If
      lFram = lFram + 1
    Loop
    If lFram = -1 Then Return
    fStart = lFram
    fLen = lSize - fStart - IIf(Id3v1, &H80, 0)
    cMPEG = New clsMPEG(GetDWORD(bFile))
    cXING = New clsXING(bFile, cMPEG)
    cVBRI = New clsVBRI(bFile)
    cID3v1 = New clsID3v1(FilePath)
    cID3v2 = New clsID3v2(FilePath)
    If cXING.HeaderID = "Xing" Or cXING.HeaderID = "Info" Then
      bFmt = 1
    ElseIf cVBRI.HeaderID = "VBRI" Then
      bFmt = 2
    Else
      bFmt = 3
    End If
  End Sub

  Private Function GetVBRDuration() As Double
    Dim dDur As Double = 0
    Dim bFile As Byte() = My.Computer.FileSystem.ReadAllBytes(sPath)
    Dim lFram As Integer = -1
    Dim lFrames As UInt32 = 0
    Do
      lFram = GetBytePos(bFile, &HFF, lFram)
      If lFram >= 0 Then
        If CheckMPEG(bFile, lFram) Then
          lFrames += 1
          Dim cMPGHead As New clsMPEG(GetDWORD(bFile, lFram)) ' Left$(sFile, 4)))
          Dim dFrameDur As Double = (GetSamplesPerFame(cMPGHead) / cMPGHead.GetSampleRate) '(cMPGHead.GetFrameSize / cMPGHead.GetBitrate) * 8
          'debug.Print("Frame " & lFrames & ": Frame Size = " & cMPGHead.GetFrameSize & ", Bitrate = " & cMPGHead.GetBitrate & ", Frame Duration = " & dFrameDur)
          'Debug.Print("Frame " & lFrames & ": Samples per Frame = " & GetSamplesPerFame(cMPGHead) & ", Sample Rate = " & cMPGHead.GetSampleRate & ", Frame Duration = " & dFrameDur)
          dDur += dFrameDur
          cMPGHead = Nothing
          If lFram = -1 And dDur > 0 Then Exit Do
        End If
      Else
        If dDur > 0 Then Exit Do
      End If
      lFram += 1
    Loop

    'If lFram = -1 Then Return -1
    'Debug.Print(dDur)
    Return dDur
  End Function

  Public ReadOnly Property RateFormat() As String
    Get
      Select Case bFmt
        Case 1 : RateFormat = cXING.VBRMethod & " (XING)"
        Case 2 : RateFormat = "VBR (Fraunhofer)"
        Case 3 : RateFormat = "CBR"
        Case Else : RateFormat = "Unknown"
      End Select
    End Get
  End Property

  Public ReadOnly Property Duration() As Double
    Get
      If bFmt = 1 Or bFmt = 2 Then
        Return GetVBRDuration()
      Else
        Return Frames * GetSamplesPerFame(cMPEG) / cMPEG.GetSampleRate
      End If
    End Get
  End Property

  Private Function GetSamplesPerFame(ByRef cMPEG As clsMPEG) As Long
    Select Case cMPEG.GetMPEGVer
      Case 1
        Select Case cMPEG.GetMPEGLayer
          Case 1 : Return 384
          Case 2, 3 : Return 1152
          Case Else : Return 0
        End Select
      Case 2, 3
        Select Case cMPEG.GetMPEGLayer
          Case 1 : Return 384
          Case 2 : Return 1152
          Case 3 : Return 576
          Case Else : Return 0
        End Select
      Case Else
        Return 0
    End Select
  End Function

  Public ReadOnly Property Bitrate() As Long
    Get
      Select Case bFmt
        Case 1, 2 : Bitrate = CLng(fLen / (Frames * GetSamplesPerFame(cMPEG) / cMPEG.GetSampleRate) * 8)
        Case 3 : Bitrate = cMPEG.GetBitrate
        Case Else : Bitrate = 0
      End Select
    End Get
  End Property

  Public ReadOnly Property Frames() As Long
    Get
      Select Case bFmt
        Case 1 : Frames = cXING.FrameCount
        Case 2 : Frames = cVBRI.Frames
        Case 3 : Frames = lSize / cMPEG.GetFrameSize
        Case Else : Frames = 0
      End Select
    End Get
  End Property

  Public ReadOnly Property MPEGStart() As Long
    Get
      MPEGStart = fStart
    End Get
  End Property

  Public ReadOnly Property MPEGLength() As Long
    Get
      MPEGLength = fLen
    End Get
  End Property

#Region "IDisposable Support"
  Private disposedValue As Boolean
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        cMPEG = Nothing
        cVBRI = Nothing
        cXING = Nothing
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
