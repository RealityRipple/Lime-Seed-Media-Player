Public Class clsHeaderLoader
  Implements IDisposable
  Public cMPEG As clsMPEG
  Public cXING As clsXING
  Public cVBRI As clsVBRI
  Public cID3v1 As clsID3v1
  Public cID3v2 As clsID3v2
  'Private lSize As Long
  '1 = XING
  '2 = VBR Fraunhofer
  '3 = CBR
  '4 = ABR
  Private bFmt As Byte
  'Private fStart As Long
  'Private fLen As Long
  Private sPath As String
  Private reallyCBR As Boolean
  Private mFrameCount As UInt32

  Public Sub New(FilePath As String)
    reallyCBR = False
    If String.IsNullOrEmpty(FilePath) Then Return
    If Not IO.File.Exists(FilePath) Then Return
    If (New IO.FileInfo(FilePath)).Length >= 1024L * 1024L * 1024L * 2L Then Return
    sPath = FilePath
    Dim bFile As Byte() = IO.File.ReadAllBytes(FilePath)
    If bFile.Length <= &H80 Then Return
    'lSize = bFile.Length
    'Dim Id3v1 As Boolean = (GetString(bFile, bFile.Length - &H80, 3) = "TAG")
    'Dim lFram As Integer = -1
    'Do
    '  lFram = GetBytePos(bFile, &HFF, lFram)
    '  If lFram >= 0 Then
    '    If CheckMPEG(bFile, lFram) Then
    '      Dim cMPGHead As New clsMPEG(GetDWORD(bFile, lFram)) ' Left$(sFile, 4)))
    '      Dim lFrameSize As Long = cMPGHead.GetFrameSize
    '      Dim bTmp(lFrameSize - 1) As Byte
    '      Array.ConstrainedCopy(bFile, lFram, bTmp, 0, lFrameSize)
    '      ReDim bFile(lFrameSize - 1)
    '      Array.Copy(bTmp, bFile, lFrameSize)
    '      If lFram = -1 Then lFram = 0
    '      Exit Do
    '    End If
    '  Else
    '    Exit Do
    '  End If
    '  lFram = lFram + 1
    'Loop
    'If lFram = -1 Then Return
    'fStart = lFram
    'fLen = lSize - fStart - IIf(Id3v1, &H80, 0)
    Dim lFram As Integer = -1
    Do
      If lFram = -1 Then
        lFram = Array.IndexOf(Of Byte)(bFile, &HFF)
      Else
        lFram = Array.IndexOf(Of Byte)(bFile, &HFF, lFram)
      End If
      If lFram < 0 Then Exit Do
      If CheckMPEG(bFile, lFram) Then Exit Do
      lFram += 1
    Loop
    If lFram = -1 Then Return
    cMPEG = New clsMPEG(GetDWORD(bFile, lFram))
    cXING = New clsXING(bFile, cMPEG, lFram)
    cVBRI = New clsVBRI(bFile, lFram)
    cID3v1 = New clsID3v1(FilePath)
    cID3v2 = New clsID3v2(FilePath)
    '    mFrameCount = GetRealFrameCount()
    If cXING.HeaderID = "Xing" Or cXING.HeaderID = "Info" Then
      mFrameCount = cXING.FrameCount
      bFmt = 1
    ElseIf cVBRI.HeaderID = "VBRI" Then
      mFrameCount = cVBRI.Frames
      bFmt = 2
    Else
      mFrameCount = GetRealFrameCount()
      If reallyCBR Then
        bFmt = 3
      Else
        bFmt = 4
      End If
    End If
  End Sub

  Private Function GetRealFrameCount() As UInt32
    Dim bFile As Byte() = IO.File.ReadAllBytes(sPath)
    Dim lFram As Integer = -1
    Dim variance As Boolean = False
    Dim lastDur As Double = -1
    Dim foundFrames As New Dictionary(Of clsMPEG, Integer)
    Do
      If lFram = -1 Then
        lFram = Array.IndexOf(Of Byte)(bFile, &HFF)
      Else
        lFram = Array.IndexOf(Of Byte)(bFile, &HFF, lFram)
      End If
      If lFram < 0 Then Exit Do
      If CheckMPEG(bFile, lFram) Then
        Dim thisFrame As New clsMPEG(GetDWORD(bFile, lFram))
        Dim foundFrame As clsMPEG = Nothing
        For Each frame In foundFrames
          If frame.Key.Equals(thisFrame) Then
            foundFrame = frame.Key
            Exit For
          End If
        Next
        If foundFrame Is Nothing Then
          foundFrames.Add(thisFrame, 1)
        Else
          foundFrames(foundFrame) += 1
        End If
        thisFrame = Nothing
      End If
      lFram += 1
    Loop
    Dim lFrames As UInt32 = 0
    Dim iVar As Integer = 0
    For Each frameList In foundFrames
      If frameList.Value = 1 Then
        'Debug.Print("Found a single invalid frame of type: " & frameList.Key.ToString)
      ElseIf frameList.Value < 10 Then
        'Debug.Print("Found two invalid frames of type: " & frameList.Key.ToString)
      ElseIf frameList.Value < 100 Then
        Dim dFrameDur As Double = (GetSamplesPerFame(frameList.Key) / frameList.Key.GetSampleRate)
        Debug.Print("Found " & frameList.Value & " valid frames of type: " & frameList.Key.ToString & " which span " & dFrameDur & " seconds...")
        lFrames += frameList.Value
        iVar += 1
      Else
        'Debug.Print("Found " & frameList.Value & " valid frames of type: " & frameList.Key.ToString)
        lFrames += frameList.Value
        iVar += 1
      End If
    Next
    If iVar = 1 Then reallyCBR = True
    Return lFrames
  End Function

  Private Function GetVBRDuration() As Double
    Dim bFile As Byte() = io.file.readallbytes(sPath)
    Dim lFram As Integer = -1

    Dim foundFrames As New Dictionary(Of clsMPEG, Integer)
    Do
      If lFram = -1 Then
        lFram = Array.IndexOf(Of Byte)(bFile, &HFF)
      Else
        lFram = Array.IndexOf(Of Byte)(bFile, &HFF, lFram)
      End If
      If lFram < 0 Then Exit Do
      If CheckMPEG(bFile, lFram) Then
        Dim thisFrame As New clsMPEG(GetDWORD(bFile, lFram))
        Dim foundFrame As clsMPEG = Nothing
        For Each frame In foundFrames
          If frame.Key.Equals(thisFrame) Then
            foundFrame = frame.Key
            Exit For
          End If
        Next
        If foundFrame Is Nothing Then
          foundFrames.Add(thisFrame, 1)
        Else
          foundFrames(foundFrame) += 1
        End If
        thisFrame = Nothing
      End If
      lFram += 1
    Loop
    Dim dDur As Double = 0
    For Each frameList In foundFrames
      If frameList.Value = 1 Then

      ElseIf frameList.Value < 10 Then

      ElseIf frameList.Value < 100 Then
        Dim dFrameDur As Double = (GetSamplesPerFame(frameList.Key) / frameList.Key.GetSampleRate)
        dDur += dFrameDur * frameList.Value
      Else
        Dim dFrameDur As Double = (GetSamplesPerFame(frameList.Key) / frameList.Key.GetSampleRate)
        dDur += dFrameDur * frameList.Value
      End If
    Next

    'Dim FirstFrame As clsMPEG = Nothing
    'Do
    '  lFram = GetBytePos(bFile, &HFF, lFram)
    '  If lFram < 0 Then Exit Do
    '  If CheckMPEG(bFile, lFram) Then
    '    Dim thisFrame As New clsMPEG(GetDWORD(bFile, lFram))
    '    If FirstFrame Is Nothing Then FirstFrame = thisFrame
    '    If Not thisFrame.Equals(FirstFrame) Then
    '      lFram += 1
    '      Continue Do
    '    End If
    '    Dim dFrameDur As Double = (GetSamplesPerFame(thisFrame) / thisFrame.GetSampleRate)
    '    If lastDur = -1 Then
    '      lastDur = dFrameDur
    '    Else
    '      If Not lastDur = dFrameDur Then variance = True
    '    End If
    '    dDur += dFrameDur
    '    thisFrame = Nothing
    '  End If
    '  lFram += 1
    'Loop
    Return dDur
  End Function

  Public ReadOnly Property RateFormat() As String
    Get
      Select Case bFmt
        Case 1 : Return cXING.VBRMethod & " (XING)"
        Case 2 : Return "VBR (Fraunhofer)"
        Case 3 : Return "CBR"
        Case 4 : Return "ABR"
        Case Else : Return "Unknown"
      End Select
    End Get
  End Property

  Private m_DurVal As Double = -1
  Public ReadOnly Property Duration() As Double
    Get
      If m_DurVal = -1 Then
        Dim rFrames As UInt32 = 0
        Dim mCBR As Double = Frames * GetSamplesPerFame(cMPEG) / cMPEG.GetSampleRate
        If bFmt = 1 Then
          If cXING.VBRMethod.Contains("CBR") Then
            m_DurVal = mCBR
          Else
            m_DurVal = GetVBRDuration()
          End If
        ElseIf bFmt = 2 Then
          m_DurVal = GetVBRDuration()
        ElseIf bFmt = 3 Then
          m_DurVal = mCBR
        ElseIf bFmt = 4 Then
          m_DurVal = GetVBRDuration()
        Else
          m_DurVal = GetVBRDuration()
        End If
      End If
      Return m_DurVal
    End Get
  End Property

  Private Function GetSamplesPerFame(cMPEG As clsMPEG) As Long
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
        Case 1 : Bitrate = CLng(cXING.ByteCount / (Frames * GetSamplesPerFame(cMPEG) / cMPEG.GetSampleRate) * 8)
        Case 2 : Bitrate = CLng(cVBRI.Bytes / (Frames * GetSamplesPerFame(cMPEG) / cMPEG.GetSampleRate) * 8)
        Case 3 : Bitrate = cMPEG.GetBitrate
        Case 4 : Bitrate = cMPEG.GetBitrate
        Case Else : Bitrate = 0
      End Select
    End Get
  End Property

  Public ReadOnly Property Frames() As Long
    Get
      Return mFrameCount
      'Select Case bFmt
      '  Case 1 : Return cXING.FrameCount
      '  Case 2 : Return cVBRI.Frames
      '  Case Else : Return mFrameCount
      'End Select
    End Get
  End Property

  'Public ReadOnly Property MPEGStart() As Long
  '  Get
  '    MPEGStart = fStart
  '  End Get
  'End Property

  'Public ReadOnly Property MPEGLength() As Long
  '  Get
  '    MPEGLength = fLen
  '  End Get
  'End Property

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
