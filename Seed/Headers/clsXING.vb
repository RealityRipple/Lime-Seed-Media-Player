Public Class clsXING
  Private Structure XINGHeader
    Public headerID As String
    Public FieldFlags As Long
    Public Frames As Long
    Public Bytes As Long
    Public ToC As Byte()
    Public Quality As Long
  End Structure
  Private Structure XINGExtend
    Public EncoderVStr As String
    Public InfoTagRev As Byte
    Public LowpassFliter As Byte
    Public PeakAmplitude As Long
    Public RadioGain As Integer
    Public IdealGain As Integer
    Public EncodFlags As Byte
    Public Bitrate As Byte
    Public EncoderDelay As Long
    Public Misc As Byte
    Public MP3Gain As Byte
    Public PresetFlags As Integer
    Public MusicLength As Long
    Public CRC As Integer
    Public InfoCRC As Integer
  End Structure
  Private Const FIELD_FRAMES As Long = &H1
  Private Const FIELD_BYTES As Long = &H2
  Private Const FIELD_TOC As Long = &H4
  Private Const FIELD_QUALITY As Long = &H8
  Private xHeader As XINGHeader
  Private xExtend As XINGExtend

  Public Sub New(bFrame As Byte(), ByRef cMPEG As clsMPEG)
    Dim I As Integer
    Dim lStrt As Long
    Dim lPos As Long
    If cMPEG.GetMPEGVer = 1 Then
      If cMPEG.GetChannels = "Single Channel" Then
        lStrt = 17 + 4
      Else
        lStrt = 32 + 4
      End If
    Else
      If cMPEG.GetChannels = "Single Channel" Then
        lStrt = 9 + 4
      Else
        lStrt = 17 + 4
      End If
    End If
    lPos = lStrt
    xHeader.headerID = GetString(bFrame, lPos, 4)
    If xHeader.headerID <> "Xing" And xHeader.headerID <> "Info" Then
      xHeader.headerID = vbNullString
      Return
    End If
    lPos += 4
    xHeader.FieldFlags = GetDWORD(bFrame, lPos)
    lPos += 4
    If CBool(xHeader.FieldFlags And FIELD_FRAMES) Then
      xHeader.Frames = GetDWORD(bFrame, lPos)
      lPos += 4
    End If
    If CBool(xHeader.FieldFlags And FIELD_BYTES) Then
      xHeader.Bytes = GetDWORD(bFrame, lPos)
      lPos += 4
    End If
    If CBool(xHeader.FieldFlags And FIELD_TOC) Then
      ReDim xHeader.ToC(99)
      For I = 0 To 99
        xHeader.ToC(I) = bFrame(lPos + I) ' Asc(Mid$(sFrame, lPos + I, 1))
      Next I
      lPos += 100
    End If
    If CBool(xHeader.FieldFlags And FIELD_QUALITY) Then
      xHeader.Quality = GetDWORD(bFrame, lPos)
      lPos += 4
    End If
    xExtend.EncoderVStr = GetString(bFrame, lPos, 9)
    If Replace(Trim(xExtend.EncoderVStr), vbNullChar, vbNullString) = vbNullString Then Return
    lPos += 9
    xExtend.InfoTagRev = bFrame(lPos)
    lPos += 1
    xExtend.LowpassFliter = bFrame(lPos)
    lPos += 1
    xExtend.PeakAmplitude = GetDWORD(bFrame, lPos)
    lPos += 4
    xExtend.RadioGain = GetWORD(bFrame, lPos)
    lPos += 2
    xExtend.IdealGain = GetWORD(bFrame, lPos)
    lPos += 2
    xExtend.EncodFlags = bFrame(lPos)
    lPos += 1
    xExtend.Bitrate = bFrame(lPos)
    lPos += 1
    xExtend.EncoderDelay = CLng("&H00" & BufferHex(bFrame(lPos)) & BufferHex(bFrame(lPos + 1)) & BufferHex(bFrame(lPos + 2)))
    lPos += 3
    xExtend.Misc = bFrame(lPos)
    lPos += 1
    xExtend.MP3Gain = bFrame(lPos)
    lPos += 1
    xExtend.PresetFlags = GetWORD(bFrame, lPos)
    lPos += 2
    xExtend.MusicLength = GetDWORD(bFrame, lPos)
    lPos += 4
    xExtend.CRC = GetWORD(bFrame, lPos)
    lPos += 2
    xExtend.InfoCRC = GetWORD(bFrame, lPos)
    lPos += 2
  End Sub
  Public ReadOnly Property HeaderID() As String
    Get
      Return xHeader.headerID
    End Get
  End Property
  Public ReadOnly Property FrameCount() As Long
    Get
      Return xHeader.Frames
    End Get
  End Property
  Public ReadOnly Property ByteCount() As Long
    Get
      Return xHeader.Bytes
    End Get
  End Property
  Public ReadOnly Property ToC(Index As Integer) As Byte
    Get
      If Index > 0 And Index < 101 Then
        Return xHeader.ToC(Index - 1)
      Else
        Return 0
      End If
    End Get
  End Property
  Public ReadOnly Property Quality() As Long
    Get
      Return xHeader.Quality
    End Get

  End Property
  Public ReadOnly Property EncoderVersion() As String
    Get
      Return xExtend.EncoderVStr
    End Get
  End Property
  Public ReadOnly Property InfoTagRevision() As String
    Get
      If (xExtend.InfoTagRev And &HF0) \ &H10 = 15 Then
        Return vbNullString
      Else
        Return "r" & Trim(Str((xExtend.InfoTagRev And &HF0) \ &H10))
      End If
    End Get
  End Property
  Public ReadOnly Property VBRMethod() As String
    Get
      Select Case xExtend.InfoTagRev And &HF
        Case 1 : Return "CBR"
        Case 2 : Return "ABR"
        Case 3 : Return "VBR rh"
        Case 4 : Return "VBR mt/rh"
        Case 5 : Return "VBR mt"
        Case 8 : Return "2-Pass CBR"
        Case 9 : Return "2-Pass ABR"
        Case 15 : Return vbNullString
        Case Else : Return "Full VBR Method " & Trim(Str(xExtend.InfoTagRev And &HF))
      End Select
    End Get
  End Property
  Public ReadOnly Property LowpassFilter() As Long
    Get
      Return xExtend.LowpassFliter * 100
    End Get
  End Property
  Public ReadOnly Property PeakSignalAmplitude() As String
    Get
      Return Trim(Str(xExtend.PeakAmplitude / 8388608)) & "%"
    End Get
  End Property
  Public ReadOnly Property RadioReplayGain() As String
    Get
      Dim sName As String
      Dim sOrig As String
      Dim bSign As Boolean
      Dim lGain As Long
      'a: E000
      'b: 1C00
      'c:  200
      'd:  1FF
      'First 3 bits determine Adjustment Name
      'Next 3 bits determine Originator (Who set the adjustment)
      'One bit for +/-
      'Last 9 bits for adjustment (divide by 10 for decimal value)
      Select Case ((xExtend.RadioGain And &HE000) \ 2 ^ 13)
        Case 0 : sName = "Not Set"
        Case 1 : sName = "Radio"
        Case 2 : sName = "Audiophile"
        Case Else : sName = "Unknown"
      End Select
      Select Case ((xExtend.RadioGain And &H1C00) \ 2 ^ 10)
        Case 0 : sOrig = "Not Set"
        Case 1 : sOrig = "Artist"
        Case 2 : sOrig = "User"
        Case 3 : sOrig = "Model"
        Case 4 : sOrig = "RMS Average"
        Case Else : sOrig = "Unknown"
      End Select
      bSign = ((xExtend.RadioGain And &H200) \ 2 ^ 9) = 1
      lGain = (xExtend.RadioGain And &H1FF)
      If sName = "Not Set" And sOrig = "Not Set" And lGain = 0 Then
        Return "Not Set"
      Else
        Return sName & " gain set by " & sOrig & " to " & IIf(bSign, "-", "+") & Trim(Str(lGain / 10))
      End If
    End Get
  End Property
  Public ReadOnly Property AudiophileReplayGain() As String
    Get
      Dim sName As String
      Dim sOrig As String
      Dim bSign As Boolean
      Dim lGain As Long
      Select Case ((xExtend.IdealGain And &HE000) \ 2 ^ 13)
        Case 0 : sName = "Not Set"
        Case 1 : sName = "Radio"
        Case 2 : sName = "Audiophile"
        Case Else : sName = "Unknown"
      End Select
      Select Case ((xExtend.IdealGain And &H1C00) \ 2 ^ 10)
        Case 0 : sOrig = "Not Set"
        Case 1 : sOrig = "Artist"
        Case 2 : sOrig = "User"
        Case 3 : sOrig = "Model"
        Case 4 : sOrig = "RMS Average"
        Case Else : sOrig = "Unknown"
      End Select
      bSign = ((xExtend.IdealGain And &H200) \ 2 ^ 9) = 1
      lGain = CBool(xExtend.IdealGain And &H1FF)
      If sName = "Not Set" And sOrig = "Not Set" And lGain = 0 Then
        Return "Not Set"
      Else
        Return sName & " gain set by " & sOrig & " to " & IIf(bSign, "-", "+") & Trim(Str(lGain / 10))
      End If
    End Get
  End Property
  Public ReadOnly Property EncodingFlags() As String
    Get
      Dim bNSPSYTUNE As Boolean = CBool(xExtend.EncodFlags And &H80)
      Dim bNSSAFEJOINT As Boolean = CBool(xExtend.EncodFlags And &H40)
      Dim bNOGAPtoNEXT As Boolean = CBool(xExtend.EncodFlags And &H20)
      Dim bNOGAPtoLAST As Boolean = CBool(xExtend.EncodFlags And &H10)
      EncodingFlags = Trim(IIf(bNSPSYTUNE, "--nspsytune", vbNullString) & " " & _
                           IIf(bNSSAFEJOINT, "--nssafejoint", vbNullString) & " " & _
                           IIf(bNOGAPtoNEXT, "--nogap (to next)", vbNullString) & " " & _
                           IIf(bNOGAPtoLAST, "--nogap (from last)", vbNullString))
    End Get
  End Property
  Public ReadOnly Property ATHType As Byte
    Get
      Return (xExtend.EncodFlags And &HFF)
    End Get
  End Property
  Public ReadOnly Property MinimalBitrate As Byte
    Get
      Return xExtend.Bitrate
    End Get
  End Property
  Public ReadOnly Property StartEncoderDelay() As Integer
    Get
      Return ((xExtend.EncoderDelay And &HFFF000) \ 2 ^ 12)
    End Get
  End Property
  Public ReadOnly Property EndEncoderDelay() As Integer
    Get
      Return (xExtend.EncoderDelay And &HFFF)
    End Get
  End Property
  Public ReadOnly Property NoiseSharpening As Byte
    Get
      Return ((xExtend.Misc And &HC0) \ 2 ^ 6)
    End Get
  End Property
  Public ReadOnly Property StereoMode() As String
    Get
      Select Case ((xExtend.Misc And &H38) \ 2 ^ 3)
        Case 0 : Return "Mono"
        Case 1 : Return "Stereo"
        Case 2 : Return "Dual"
        Case 3 : Return "Joint"
        Case 4 : Return "Force"
        Case 5 : Return "Auto"
        Case 6 : Return "Intensity"
        Case Else : Return "Different"
      End Select
    End Get
  End Property
  Public ReadOnly Property UnwiseSetting() As Boolean
    Get
      Return CBool(((xExtend.Misc And &H4) \ 2 ^ 2) = 1)
    End Get
  End Property
  Public ReadOnly Property SourceFrequency() As String
    Get
      Select Case (xExtend.Misc And &H3)
        Case 0 : Return "<=32000"
        Case 1 : Return "44100"
        Case 2 : Return "48000"
        Case Else : Return ">48000"
      End Select
    End Get
  End Property
  Public ReadOnly Property MP3Gain() As Single
    Get
      If xExtend.MP3Gain > 128 Then
        Return (128 - xExtend.MP3Gain) * (3 / 2)
      ElseIf xExtend.MP3Gain < 128 Then
        Return xExtend.MP3Gain * (3 / 2)
      Else
        Return 0
      End If
    End Get
  End Property
  Public ReadOnly Property SurroundInfo() As String
    Get
      Select Case ((xExtend.PresetFlags And &H3800) \ 2 ^ 11)
        Case 0 : Return "No Information"
        Case 1 : Return "DPL Encoding"
        Case 2 : Return "DPL2 Encoding"
        Case 3 : Return "Ambisonic Encoding"
        Case 8 : Return "Reserved"
        Case Else : Return "Unknown"
      End Select
    End Get
  End Property
  Public ReadOnly Property PresetUsed() As Integer
    Get
      Return (xExtend.PresetFlags And &H7FF)
    End Get
  End Property
  Public ReadOnly Property MusicLength() As Long
    Get
      Return xExtend.MusicLength
    End Get
  End Property
  Public ReadOnly Property MusicCRC() As Integer
    Get
      Return xExtend.CRC
    End Get
  End Property
  Public ReadOnly Property InfoTagCRC() As Integer
    Get
      Return xExtend.InfoCRC
    End Get
  End Property
End Class
