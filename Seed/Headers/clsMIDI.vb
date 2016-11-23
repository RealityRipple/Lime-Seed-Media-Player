Public Class clsMIDI
  Implements IDisposable

  Private Structure TrackHeader
    Public Format As TrackType
    Public TrackCount As UInt16
    Public TrackTime As UInt16
  End Structure
  Public Enum TrackType
    SingleTrack = 0
    MultiSynch = 1
    MultiAsynch = 2
    Unknown
  End Enum
  Public Structure TrackChunk
    Public Structure TimeSignature
      Public Numerator As Byte
      Public Denominator As Byte
      Public TicksPerClick As Byte
      Public ThirtySecondsPerQuarter As Byte
    End Structure
    Public Structure KeySignature
      Public SharpsFlats As Byte
      Public MajorMinor As Byte
    End Structure
    Public SequenceNumber As UInt16
    Public TextEvent As String
    Public Copyright As String
    Public TrackName As String
    Public TrackInstrument As String
    Public TrackLyric As String
    Public TrackMarker As String
    Public TrackCuePoint As String
    Public ChannelPrefix As Byte
    Public PortPrefix As Byte
    Public TrackTempo As UInt32
    Public SMPTEOffset As UInt64
    Public TrackTime As TimeSignature
    Public TrackSignature As KeySignature
    Public SequencerInfo As Byte()
    'Audio track data...
  End Structure
  Private bValid As Boolean
  Private mHeader As TrackHeader
  Private mChunks() As TrackChunk
  Private Const LOCALE As Integer = LATIN_1

  Public ReadOnly Property IsMIDI As Boolean
    Get
      Return bValid
    End Get
  End Property

  Public ReadOnly Property TrackFormat As TrackType
    Get
      Return mHeader.Format
    End Get
  End Property

  Public ReadOnly Property TrackCount As UInt16
    Get
      Return mHeader.TrackCount
    End Get
  End Property

  Public ReadOnly Property TrackTime As UInt16
    Get
      Return mHeader.TrackTime
    End Get
  End Property

  Public ReadOnly Property Tracks As TrackChunk()
    Get
      Return mChunks
    End Get
  End Property

  Public Sub New(FilePath As String)
    bValid = False
    If String.IsNullOrEmpty(FilePath) Then Return
    If Not My.Computer.FileSystem.FileExists(FilePath) Then Return
    If My.Computer.FileSystem.GetFileInfo(FilePath).Length >= 1024L * 1024L * 1024L * 2L Then Return
    Dim bFile As Byte() = My.Computer.FileSystem.ReadAllBytes(FilePath)
    Dim lPos As Long = 0
    ReadHeader(bFile, lPos)
    ReDim mChunks(mHeader.TrackCount - 1)
    For I As Integer = 0 To mHeader.TrackCount - 1
      mChunks(I) = ReadTrack(bFile, lPos)
      If Not bValid Then Exit For
    Next
  End Sub

  Private Sub ReadHeader(ByRef bFile As Byte(), ByRef lPos As Long)
    Select Case GetDWORD(bFile, lPos)
      Case &H4D546864
        lPos += 4
        bValid = True
        Dim uLen As UInt32 = GetDWORD(bFile, lPos)
        lPos += 4
        If uLen = 6 Then
          Dim iTrackFormat As UInt16 = GetWORD(bFile, lPos)
          If iTrackFormat < 3 Then
            mHeader.Format = iTrackFormat
          Else
            mHeader.Format = TrackType.Unknown
          End If
          lPos += 2
          mHeader.TrackCount = GetWORD(bFile, lPos)
          lPos += 2
          mHeader.TrackTime = GetWORD(bFile, lPos)
          lPos += 2
        Else
          bValid = False
          Return
        End If
      Case &H52494646
        Debug.Print("RMI")
        lPos += 20
        ReadHeader(bFile, lPos)
      Case Else
        lPos += 4
        bValid = False
        Return
    End Select
  End Sub

  Private Function ReadTrack(ByRef bFile As Byte(), ByRef lPos As Long) As TrackChunk
    If GetDWORD(bFile, lPos) = &H4D54726B Then
      lPos += 4
      Dim newChunk As New TrackChunk
      Dim uLen As UInt32 = GetDWORD(bFile, lPos)
      lPos += 4
      'Debug.Print("Track Length: " & uLen)
      Dim lLen As UInt32 = lPos + uLen
      Do
        Dim vTime As UInt64 = ReadVar(bFile, lPos) '= bFile(lPos)
        'lPos += 1
        'Debug.Print(" Time Delta: " & vTime)
        Dim CommandByte As Byte = bFile(lPos)
        lPos += 1
        'Debug.Print(" CommandByte: " & Hex(CommandByte))
        If CommandByte = &HFF Then
          Dim MetaEvent As Byte = bFile(lPos)
          lPos += 1
          Dim MetaLen As UInt64 = ReadVar(bFile, lPos)
          'lPos += 1
          'Debug.Print("  Meta Event " & Hex(MetaEvent))
          Select Case MetaEvent
            Case &H0
              If MetaLen = 2 Then
                newChunk.SequenceNumber = GetWORD(bFile, lPos)
                lPos += 4
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H1
              If String.IsNullOrEmpty(newChunk.TextEvent) Then
                newChunk.TextEvent = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TextEvent &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H2
              If String.IsNullOrEmpty(newChunk.Copyright) Then
                newChunk.Copyright = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.Copyright &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H3
              If String.IsNullOrEmpty(newChunk.TrackName) Then
                newChunk.TrackName = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TrackName &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H4
              If String.IsNullOrEmpty(newChunk.TrackInstrument) Then
                newChunk.TrackInstrument = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TrackInstrument &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H5
              If String.IsNullOrEmpty(newChunk.TrackLyric) Then
                newChunk.TrackLyric = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TrackLyric &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H6
              If String.IsNullOrEmpty(newChunk.TrackMarker) Then
                newChunk.TrackMarker = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TrackMarker &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H7
              If String.IsNullOrEmpty(newChunk.TrackCuePoint) Then
                newChunk.TrackCuePoint = System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              Else
                newChunk.TrackCuePoint &= ", " & System.Text.Encoding.GetEncoding(LOCALE).GetString(bFile, lPos, MetaLen)
                lPos += MetaLen
              End If
            Case &H20
              If MetaLen = 1 Then
                newChunk.ChannelPrefix = bFile(lPos)
                lPos += 1
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H21
              If MetaLen = 1 Then
                newChunk.PortPrefix = bFile(lPos)
                lPos += 1
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H2F
              If MetaLen = 0 Then
                Exit Do
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H51
              If MetaLen = 3 Then
                newChunk.TrackTempo = GetWORDandBYTE(bFile, lPos)
                lPos += 3
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H54
              Dim Unknown(7) As Byte
              Array.ConstrainedCopy(bFile, lPos, Unknown, 7 - MetaLen, MetaLen)
              lPos += MetaLen
              newChunk.SMPTEOffset = BitConverter.ToUInt64(Unknown, 0)
              'Debug.Print("Unknown Event: " & Hex(MetaEvent) & ": " & BitConverter.ToString(Unknown))
            Case &H58
              If MetaLen = 4 Then
                newChunk.TrackTime.Numerator = bFile(lPos)
                lPos += 1
                newChunk.TrackTime.Denominator = bFile(lPos)
                lPos += 1
                newChunk.TrackTime.TicksPerClick = bFile(lPos)
                lPos += 1
                newChunk.TrackTime.ThirtySecondsPerQuarter = bFile(lPos)
                lPos += 1
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H59
              If MetaLen = 2 Then
                newChunk.TrackSignature.SharpsFlats = bFile(lPos)
                lPos += 1
                newChunk.TrackSignature.MajorMinor = bFile(lPos)
                lPos += 1
              Else
                lPos += MetaLen
                bValid = False
                Return newChunk
              End If
            Case &H7F
              ReDim newChunk.SequencerInfo(MetaLen - 1)
              Array.ConstrainedCopy(bFile, lPos, newChunk.SequencerInfo, 0, MetaLen)
              lPos += MetaLen
            Case Else
              Dim Unknown(MetaLen - 1) As Byte
              Array.ConstrainedCopy(bFile, lPos, Unknown, 0, MetaLen)
              lPos += MetaLen
              'Debug.Print("    Unknown Event: " & Hex(MetaEvent) & ": " & BitConverter.ToString(Unknown))
          End Select
        Else
          Dim Command As Byte = (&HF0 And CommandByte) >> 4
          Dim Channel As Byte = (&HF And CommandByte) '<< 4
          'Debug.Print("   Command: " & Hex(Command) & ", Channel: " & Channel)
          Select Case Command
            Case &H8
              Dim Note As Byte = bFile(lPos)
              lPos += 1
              Dim Velocity As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " note " & GetNote(Note) & " off [" & Velocity & "]")
            Case &H9
              Dim Note As Byte = bFile(lPos)
              lPos += 1
              Dim Velocity As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " note " & GetNote(Note) & " on [" & Velocity & "]")
            Case &HA
              Dim Key As Byte = bFile(lPos)
              lPos += 1
              Dim Velocity As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " note " & GetNote(Key) & " after-touch [" & Velocity & "]")
            Case &HB
              Dim Controller As Byte = bFile(lPos)
              lPos += 1
              Dim Velocity As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " control " & Hex(Controller) & " change to " & Velocity)
            Case &HC
              Dim Program As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " program " & Hex(Program) & " change")
            Case &HD
              Dim chanNum As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " chan " & Hex(chanNum) & " after-touch")
            Case &HE
              Dim Bottom As Byte = bFile(lPos)
              lPos += 1
              Dim Top As Byte = bFile(lPos)
              lPos += 1
              'Debug.Print("    Channel " & Channel & " pitch wheel change " & Hex(Bottom) & " " & Hex(Top))
            Case Else
              'Debug.Print("    Unknown: " & Hex(Command) & " " & Channel & " " & Hex(CommandByte) & ", skipping ahead...")
              'Debug.Print("Error at " & lPos)
              Do
                If lPos >= lLen Then
                  lPos = lLen
                  Exit Do
                End If
                If bFile(lPos) And &H80 Then
                  lPos -= 1
                  Exit Do
                Else
                  lPos += 1
                End If
              Loop

              'bValid = False
              'Return newChunk
          End Select
        End If

      Loop Until lPos >= lLen
      Debug.Print(lPos & " " & lLen)
      lPos = lLen
      Return newChunk
    Else
      lPos += 4
      bValid = False
      Return Nothing
    End If
  End Function

  Private Function ReadVar(ByRef bFile As Byte(), ByRef lPos As Long) As UInt64
    Dim val As UInt32
    Dim c As Byte
    val = bFile(lPos)
    lPos += 1
    If val And &H80 Then
      val = val And &H7F
      Do
        c = bFile(lPos)
        lPos += 1
        val = (val << 7) + (c And &H7F)
      Loop While (c And &H80)
    End If
    Return val
  End Function

  Private Function GetWORDandBYTE(bIn As Byte(), Optional ByVal lStart As Long = 0) As UInt32
    Dim bTmp(3) As Byte

    If lStart + 2 >= bIn.Length Then
      bTmp(0) = 0
    Else
      bTmp(0) = bIn(lStart + 2)
    End If

    If lStart + 1 >= bIn.Length Then
      bTmp(1) = 0
    Else
      bTmp(1) = bIn(lStart + 1)
    End If
    If lStart >= bIn.Length Then
      bTmp(2) = 0
    Else
      bTmp(2) = bIn(lStart)
    End If
    Return BitConverter.ToUInt32(bTmp, 0)
  End Function

  Private Function GetNote(Number As Byte) As String
    Dim Row As Byte = Number \ 12
    Dim Col As Byte = Number Mod 12
    Select Case Col
      Case 0 : Return "C (Octave " & Row & ")"
      Case 1 : Return "C# (Octave " & Row & ")"
      Case 2 : Return "D (Octave " & Row & ")"
      Case 3 : Return "D# (Octave " & Row & ")"
      Case 4 : Return "E (Octave " & Row & ")"
      Case 5 : Return "F (Octave " & Row & ")"
      Case 6 : Return "F# (Octave " & Row & ")"
      Case 7 : Return "G (Octave " & Row & ")"
      Case 8 : Return "G# (Octave " & Row & ")"
      Case 9 : Return "A (Octave " & Row & ")"
      Case 10 : Return "A (Octave " & Row & ")"
      Case 11 : Return "B (Octave " & Row & ")"
      Case Else : Return "Unknown " & Col & " (Octave " & Row & ")"
    End Select
  End Function

#Region "IDisposable Support"
  Private disposedValue As Boolean 
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
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
