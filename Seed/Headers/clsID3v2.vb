Public Class clsID3v2
  Implements IDisposable

  '0 = Set
  '1 = Added
  '2 = Removed
  '3 = Read only
  '4 = General Failure

  Public Class CONSTS
    Public Shared HFLG_UNSYNCH As Byte = &H80
    Public Shared HFLG_EXTENDED As Byte = &H40
    Public Shared HFLG_EXPERIMENTAL As Byte = &H20
    Public Shared HFLG_FOOTER As Byte = &H10

    Public Shared FFLG_TAGALTER As Integer = &H4000
    Public Shared FFLG_FILEALTER As Integer = &H2000
    Public Shared FFLG_READONLY As Integer = &H1000
    Public Shared FFLG_GROUP As Integer = &H40
    Public Shared FFLG_COMPRESS As Integer = &H8
    Public Shared FFLG_ENCRYPT As Integer = &H4
    Public Shared FFLG_UNSYNCH As Integer = &H2
    Public Shared FFLG_DATALENGTH As Integer = &H1
  End Class



  Private Structure ID3v2Header
    Public Identifier As String '* 3
    Public Version() As Byte ' 0 to 1
    Public Flags As Byte
    Public Size As UInteger
  End Structure
  Private Structure ID3v2Frame2
    Public FrameName As String ' * 3
    Public FrameSize() As Byte ' 0 to 2
  End Structure
  Private Structure ID3v2Frame1
    Public FrameName As String '* 4
    Public FrameSize As UInteger
    Public FrameFlags() As Byte '0 to 1
  End Structure
  Private Structure ID3v2Frame
    Public FrameName As String
    Public FrameData As String
    Public FrameFlags As UInteger
  End Structure

  Private m_sMp3File As String
  Private ID3Header As ID3v2Header
  Private HasID3v2 As Boolean
  Private ID3Frames As ArrayList
  Private lID3Len As Integer

  Public Function AddFrame(sName As String, ByVal sData As String, Optional ByVal iFlags As Integer = 0) As Byte
    ID3Frames.Add(New ID3v2Frame With {.FrameName = sName, .FrameData = sData, .FrameFlags = iFlags})
    Return 1
  End Function

  Public Sub PutFrame(sName As String, value As String)
    If value.Contains(vbCr) Then
      Do
        value = value.Replace(vbNewLine, vbCr).Replace(vbLf, vbCr).Replace(vbCr & vbCr, vbCr)
      Loop While value.Contains(vbCr & vbCr) Or value.Contains(vbLf) Or value.Contains(vbNewLine)
      RemoveFrame(sName)
      For Each sVal As String In value.Split(vbCr)
        AddFrame(sName, vbNullChar & sVal)
      Next
    Else
      SetFrame(sName, vbNullChar & value)
    End If
  End Sub

  Private Function CheckID3v2(ByRef ioReader As IO.BinaryReader) As Integer
    Try
      ioReader.BaseStream.Position = 0
      Do Until ioReader.BaseStream.Position >= ioReader.BaseStream.Length - 1
        Dim sFile As String = ioReader.ReadChars(4096)
        If sFile.Contains("ID3") Then
          Return ioReader.BaseStream.Position - 4096 + sFile.IndexOf("ID3")
        End If
      Loop
      Return -1
    Catch ex As Exception
      Return -1
    End Try
  End Function

  Public Function ReadFrame(sName As String) As Byte()()
    Try
      Dim sNewName3 As String = GetNewName(sName, 3)
      Dim sNewName4 As String = GetNewName(sName, 4)
      Return (From id3frame As ID3v2Frame In ID3Frames Where id3frame.FrameName = sName Or id3frame.FrameName = sNewName3 Or id3frame.FrameName = sNewName4 Select fileEncoding.GetBytes(id3frame.FrameData)).ToArray
    Catch ex As Exception
      Return Nothing
    End Try
  End Function

  Public Function FindFrame(sName As String) As String
    Try
      Dim sNewName3 As String = GetNewName(sName, 3)
      Dim sNewName4 As String = GetNewName(sName, 4)
      Return Join((From id3frame As ID3v2Frame In ID3Frames Where (id3frame.FrameName = sName Or id3frame.FrameName = sNewName3 Or id3frame.FrameName = sNewName4) And Not IsNothing(ParseFrame(sName, id3frame.FrameData)) Select Join(ParseFrame(sName, id3frame.FrameData), vbNewLine)).ToArray, vbNewLine)
    Catch ex As Exception
      Return Nothing
    End Try
  End Function

  Public ReadOnly Property FrameCount() As Integer
    Get
      Return ID3Frames.Count
    End Get
  End Property

  Public ReadOnly Property FrameData(Index As Integer) As String
    Get
      Return CType(ID3Frames(Index), ID3v2Frame).FrameData
    End Get
  End Property

  Public ReadOnly Property FrameFlags(Index As Integer) As Integer
    Get
      Return CType(ID3Frames(Index), ID3v2Frame).FrameFlags
    End Get
  End Property

  Public ReadOnly Property FrameName(Index As Integer) As String
    Get
      Return CType(ID3Frames(Index), ID3v2Frame).FrameName
    End Get
  End Property

  Public Property Genre() As String
    Get
      Try
        Dim gData As String = FindFrame("TCO")
        gData = gData.Replace("("c, vbNullString).Replace(")"c, vbNullString)

        Dim gList() As String = gData.Split(vbNullChar)
        For I As Integer = 0 To gList.Length - 1
          If Not String.IsNullOrEmpty(gList(I)) AndAlso IsNumeric(gList(I)) Then gList(I) = clsID3v1.GenreName(gList(I))
        Next I
        Dim sGenre As String = Join(gList, vbNewLine)
        Do While sGenre.StartsWith(vbNewLine)
          sGenre = sGenre.Substring(2)
        Loop
        Do While sGenre.EndsWith(vbNewLine)
          sGenre = sGenre.Substring(0, sGenre.Length - 2)
        Loop
        Do While sGenre.Contains(vbNewLine & vbNewLine)
          sGenre = sGenre.Replace(vbNewLine & vbNewLine, vbNewLine)
        Loop
        Return sGenre
      Catch ex As Exception
        Genre = vbNullString
      End Try
    End Get
    Set(value As String)
      PutFrame("TCO", value)
    End Set
  End Property

  Private Sub GetExtendedHeader(ByRef ioReader As IO.BinaryReader, ByVal StartLoc As Integer)
    Try
      Dim lLen As Integer
      If StartLoc > 0 Then
        ioReader.BaseStream.Position = StartLoc + 10
        lLen = ioReader.ReadUInt32
        lLen = GetSize(lLen)
        ioReader.BaseStream.Position = StartLoc + 14 + lLen
      End If
    Catch ex As Exception
      Debug.Print("ID3v2 Extended Header Read error: " & ex.Message)
    End Try
  End Sub

  Private Sub GetFrame(ByRef ioReader As IO.BinaryReader)
    Try
      Dim ID3Frame As ID3v2Frame1
      ID3Frame.FrameName = ioReader.ReadChars(4)
      Dim sSize As String = ioReader.ReadChars(4)
      ID3Frame.FrameSize = GetDWORD(sSize)
      If ID3Header.Version(0) = 4 Then ID3Frame.FrameSize = UnSynchSafe(ID3Frame.FrameSize)
      If ID3Frame.FrameSize < ioReader.BaseStream.Length And ID3Frame.FrameSize > 0 Then
        ReDim ID3Frame.FrameFlags(1)
        ID3Frame.FrameFlags = ioReader.ReadBytes(2)
        Dim sFrameData As String = ioReader.ReadChars(ID3Frame.FrameSize)
        AddFrame(ID3Frame.FrameName, sFrameData, Val("&H" & Hex$(ID3Frame.FrameFlags(1)) & Hex$(ID3Frame.FrameFlags(0))))
      End If
    Catch ex As Exception
      Debug.Print("ID3v2 Frame v3/4 Read error: " & ex.Message)
    End Try
  End Sub

  Private Sub GetFrame2(ByRef ioReader As IO.BinaryReader)
    Try
      Dim ID3Frame As ID3v2Frame2
      ID3Frame.FrameName = ioReader.ReadChars(3)
      ReDim ID3Frame.FrameSize(2)
      ID3Frame.FrameSize = ioReader.ReadBytes(3)
      Dim lFrameSize As Integer = CLng("&H00" & BufferHex(ID3Frame.FrameSize(0)) & BufferHex(ID3Frame.FrameSize(1)) & BufferHex(ID3Frame.FrameSize(2)))
      If lFrameSize < ioReader.BaseStream.Length And lFrameSize > 0 Then
        Dim sFrameData As String = ioReader.ReadChars(lFrameSize)
        AddFrame(ID3Frame.FrameName, sFrameData)
      End If
    Catch ex As Exception
      Debug.Print("ID3v2 Frame v2 Read error: " & ex.Message)
    End Try
  End Sub

  Private Sub GetHeader(ByRef ioReader As IO.BinaryReader, ByVal StartLoc As Integer)
    Try
      HasID3v2 = False
      If StartLoc > -1 Then
        ioReader.BaseStream.Position = StartLoc
        ID3Header.Identifier = ioReader.ReadChars(3)
        If ID3Header.Identifier = "ID3" Then
          ReDim ID3Header.Version(1)
          ID3Header.Version = ioReader.ReadBytes(2)
          ID3Header.Flags = ioReader.ReadByte
          ID3Header.Size = ioReader.ReadUInt32
          ID3Header.Size = UnSynchSafe(GetSize(ID3Header.Size))
          Select Case ID3Header.Version(0)
            Case 2, 3, 4
              Select Case ID3Header.Version(1)
                Case 0
                  If (ID3Header.Flags And &HE0) = ID3Header.Flags Then HasID3v2 = True
              End Select
            Case Else
              'Debug.Print("Unknown ID3 Version: 2." & ID3Header.Version(0) & "." & ID3Header.Version(1))
              HasID3v2 = False
          End Select
        End If
      End If
    Catch ex As Exception
      HasID3v2 = False
    End Try
  End Sub

  Private Function BufferHex(lIn As UInteger, Optional lLen As UShort = 2) As String
    Dim sVal As String = Hex(lIn)
    Do While sVal.Length < lLen : sVal = "0" & sVal : Loop
    Return sVal
  End Function

  Private Function GetNewName(sNewName As String, rev As Integer) As String
    Select Case sNewName
      Case "BUF" : Return "RBUF"
      Case "CNT" : Return "PCNT"
      Case "COM" : Return "COMM"
      Case "CRA" : Return "AENC"
      Case "CRM" : Return sNewName
      Case "ETC" : Return "ETCO"
      Case "EQU" : Return IIf(rev = 3, "EQUA", "EQU2")
      Case "GEO" : Return "GEOB"
      Case "IPL" : Return IIf(rev = 3, "IPLS", "TIPL")
      Case "LNK" : Return "LINK"
      Case "MCI" : Return "MCDI"
      Case "MLL" : Return "MLLT"
      Case "PIC" : Return "APIC"
      Case "POP" : Return "POPM"
      Case "REV" : Return "RVRB"
      Case "RVA" : Return IIf(rev = 3, "RVAD", "RVA2")
      Case "SLT" : Return "SYLT"
      Case "STC" : Return "SYTC"
      Case "TAL" : Return "TALB"
      Case "TBP" : Return "TBPM"
      Case "TCM" : Return "TCOM"
      Case "TCO" : Return "TCON"
      Case "TCR" : Return "TCOP"
      Case "TDA" : Return sNewName
      Case "TDY" : Return "TDLY"
      Case "TEN" : Return "TENC"
      Case "TFT" : Return "TFLT"
      Case "TIM" : Return sNewName
      Case "TKE" : Return "TKEY"
      Case "TLA" : Return "TLAN"
      Case "TLE" : Return "TLEN"
      Case "TMT" : Return "TMED"
      Case "TOA" : Return "TOPE"
      Case "TOF" : Return "TOFN"
      Case "TOL" : Return "TOLY"
      Case "TOR" : Return "TDOR"
      Case "TOT" : Return "TOAL"
      Case "TP1" : Return "TPE1"
      Case "TP2" : Return "TPE2"
      Case "TP3" : Return "TPE3"
      Case "TP4" : Return "TPE4"
      Case "TPA" : Return "TPOS"
      Case "TPB" : Return "TPUB"
      Case "TRC" : Return "TSRC"
      Case "TRD" : Return IIf(rev = 3, "TDAT", "TDRC")
      Case "TRK" : Return "TRCK"
      Case "TSI" : Return "TSIZ"
      Case "TSS" : Return "TSSE"
      Case "TT1" : Return "TIT1"
      Case "TT2" : Return "TIT2"
      Case "TT3" : Return "TIT3"
      Case "TXT" : Return "TEXT"
      Case "TXX" : Return "TXXX"
      Case "TYE" : Return "TYER"
      Case "UFI" : Return "UFID"
      Case "ULT" : Return "USLT"
      Case "WAF" : Return "WOAF"
      Case "WAR" : Return "WOAR"
      Case "WAS" : Return "WOAS"
      Case "WCM" : Return "WCOM"
      Case "WCP" : Return "WCOP"
      Case "WPB" : Return "WPUB"
      Case "WXX" : Return "WXXX"
      Case Else : Return sNewName
    End Select
  End Function

  Public Shared Function GetFrameName(sName As String) As String
    Select Case sName
      Case "ASPI" : Return "Audio Seek Point Index"
      Case "BUF", "RBUF" : Return "Recommended Buffer Size"
      Case "CNT", "PCNT" : Return "Play Counter"
      Case "COM", "COMM" : Return "Comments"
      Case "COMR" : Return "Commercial"
      Case "CRA", "AENC" : Return "Audio Encryption"
      Case "CRM" : Return "Encrypted META Frame"
      Case "ENCR" : Return "Encryption Method Registration"
      Case "EQU", "EQUA", "EQU2" : Return "Equalization"
      Case "ETC", "ETCO" : Return "Event Timing Codes"
      Case "GEO", "GEOB" : Return "General Encapsulated Object"
      Case "GRID" : Return "Group ID Registration"
      Case "IPL", "IPLS", "TIPL" : Return "Involved People List"
      Case "LNK", "LINK" : Return "Linked Information"
      Case "MCI", "MCDI" : Return "Music CD Identifier"
      Case "MLL", "MLLT" : Return "MPEG Location Lookup Table"
      Case "OWNE" : Return "Ownership"
      Case "PIC", "APIC" : Return "Picture"
      Case "PRIV" : Return "Private"
      Case "POP", "POPM" : Return "Popularimeter"
      Case "POSS" : Return "Position Synchronization"
      Case "REV", "RVRB" : Return "Reverb"
      Case "RVA", "RVAD", "RVA2" : Return "Relative Volume Advustment"
      Case "SEEK" : Return "Seek"
      Case "SIGN" : Return "Signature"
      Case "SLT", "SYLT" : Return "Synced Lyrics"
      Case "STC", "SYTC" : Return "Synced Tempo Codes"
      Case "TAL", "TALB" : Return "Album"
      Case "TBB", "TBPM" : Return "Beats per Minute"
      Case "TCM", "TCOM" : Return "Composer"
      Case "TCO", "TCON" : Return "Genre"
      Case "TCR", "TCOP" : Return "Copyright Message"
      Case "TDA", "TDAT" : Return "Date"
      Case "TDTG" : Return "Tagging Time"
      Case "TDEN" : Return "Encoding Time"
      Case "TDY", "TDLY" : Return "Playlist Delay"
      Case "TEN", "TENC" : Return "Encoded By"
      Case "TFT", "TFLT" : Return "File Type"
      Case "TIM", "TIME", "TDRL" : Return "Time"
      Case "TKE", "TKEY" : Return "Initial Key"
      Case "TLA", "TLAN" : Return "Language"
      Case "TLE", "TLEN" : Return "Length"
      Case "TMCL" : Return "Musician Credits"
      Case "TMT", "TMED" : Return "Media Type"
      Case "TMOO" : Return "Mood"
      Case "TOA", "TOPE" : Return "Original Artist"
      Case "TOF", "TOFN" : Return "Original Filename"
      Case "TOL", "TOLY" : Return "Original Lyricist"
      Case "TOR", "TDOR" : Return "Original Release Year"
      Case "TOT", "TOAL" : Return "Original Album"
      Case "TOWN" : Return "File Owner"
      Case "TP1", "TPE1" : Return "Artist"
      Case "TP2", "TPE2" : Return "Band"
      Case "TP3", "TPE3" : Return "Conductor"
      Case "TP4", "TPE4" : Return "Mix By"
      Case "TPA", "TPOS" : Return "Disc Number"
      Case "TPRO" : Return "Produced"
      Case "TPB", "TPUB" : Return "Publisher"
      Case "TRC", "TSRC" : Return "ISRC"
      Case "TRD", "TDRC" : Return "Recording Date"
      Case "TRK", "TRCK" : Return "Track"
      Case "TRSN" : Return "Internet Radio Station Name"
      Case "TRSO" : Return "Internet Ratio Station Owner"
      Case "TSOA" : Return "Album Sort Order"
      Case "TSOP" : Return "Performer Sort Order"
      Case "TSOT" : Return "Title Sort Order"
      Case "TSI", "TSIZ" : Return "Size"
      Case "TSS", "TSSE" : Return "Encoding Settings"
      Case "TT1", "TIT1" : Return "Set Title"
      Case "TSST" : Return "Set Subtitle"
      Case "TT2", "TIT2" : Return "Title"
      Case "TT3", "TIT3" : Return "Subtitle"
      Case "TXT", "TEXT" : Return "Lyricist"
      Case "TXX", "TXXX" : Return "Custom Text"
      Case "TYE", "TYER" : Return "Year"
      Case "UFI", "UFID" : Return "Unique File ID"
      Case "USER" : Return "Terms of Use"
      Case "ULT", "USLT" : Return "Lyrics"
      Case "WAF", "WOAF" : Return "Song Webpage"
      Case "WAR", "WOAR" : Return "Artist Webpage"
      Case "WAS", "WOAS" : Return "Source Webpage"
      Case "WCM", "WCOM" : Return "Commercial Information"
      Case "WCP", "CWOP" : Return "Copyright Information"
      Case "WORS" : Return "Interent Radio Station Webpage"
      Case "WPAY" : Return "Payment"
      Case "WPB", "WPUB" : Return "Publisher Webpage"
      Case "WXX", "WXXX" : Return "Custom Webpage"
      Case Else : Return sName
    End Select
  End Function

  Private Function GetSize(lFromFile As UInteger) As UInteger
    Dim bFF() As Byte = BitConverter.GetBytes(lFromFile)
    Array.Reverse(bFF)
    Return BitConverter.ToUInt32(bFF, 0)
  End Function

  Private Function UnSynchSafe(lIn As UInteger) As UInteger
    Dim lOut As UInteger = 0
    lOut = (lOut Or lIn And &H7F000000) \ 2
    lOut = (lOut Or lIn And &H7F0000) \ 2
    lOut = (lOut Or lIn And &H7F00) \ 2
    lOut = lOut Or lIn And &H7F
    Return lOut
  End Function

  Private Function SynchSafe(lIn As UInteger) As UInteger
    Return lIn * 2 ^ 3 And &H7F000000 Or (lIn * 2 ^ 2) And &H7F0000 Or (lIn * 2 ^ 1) And &H7F00 Or lIn And &H7F
  End Function

  Public ReadOnly Property HasID3v2Tag() As Boolean
    Get
      Return HasID3v2
    End Get
  End Property

  Public Property ID3v2Ver() As String
    Get
      If ID3Header.Version Is Nothing Then ReDim ID3Header.Version(1)
      Return "2." & ID3Header.Version(0) & "." & ID3Header.Version(1)
    End Get
    Set(value As String)
      If ID3Header.Version Is Nothing Then ReDim ID3Header.Version(1)
      If value.Contains(".") Then
        Dim Parts() As String = Split(value, ".")
        If Parts.Length = 3 AndAlso (IsNumeric(Parts(0)) And IsNumeric(Parts(1)) And IsNumeric(Parts(2))) Then
          If Parts(0) = "2" Then
            Try
              ID3Header.Version(0) = CByte(Parts(1))
              ID3Header.Version(1) = CByte(Parts(2))
            Catch ex As Exception

            End Try
          End If
        End If
      End If
    End Set
  End Property

  Public ReadOnly Property ID3v2Len() As Integer
    Get
      Return lID3Len
    End Get
  End Property

  Private Sub LoadFile(sFile As String)
    If Not My.Computer.FileSystem.FileExists(sFile) Then Exit Sub
    Dim lStart As Integer
    Dim bTest As Byte
    Dim lLast As Integer
    Try
      ID3Frames.Clear()

      Using bR As New IO.BinaryReader(New IO.FileStream(sFile, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read), fileEncoding)
        lStart = CheckID3v2(bR)
        If lStart = -1 Then Exit Sub
        GetHeader(bR, lStart)
        If HasID3v2 Then
          If ID3Header.Flags And CONSTS.HFLG_EXTENDED Then GetExtendedHeader(bR, lStart)
          Do While bR.BaseStream.Position < bR.BaseStream.Length - 1
            If ID3Header.Version(0) = 2 Then
              GetFrame2(bR)
            Else
              GetFrame(bR)
            End If
            lLast = bR.BaseStream.Position
            bTest = bR.ReadByte
            If (bTest = 0) Or (bTest = &HFF) Then Exit Do
            bR.BaseStream.Position = lLast
          Loop
          If lLast < bR.BaseStream.Length - 1 Then
            lID3Len = lLast
          Else
            lID3Len = (ID3Header.Size + IIf(ID3Header.Flags And CONSTS.HFLG_FOOTER, &H15, &HB))
            Exit Sub
          End If
        Else
          lID3Len = 1
        End If
        bR.Close()
      End Using
    Catch ex As Exception
      Debug.Print("ID3v2 Read error: " & ex.Message)
      lID3Len = 1
    End Try
  End Sub

  Private Function MakeDWORD(lIn As UInt32) As String
    Return StrReverse(fileEncoding.GetString(BitConverter.GetBytes(lIn)))
  End Function

  Private Function MakeWORD(iIn As UInt16) As String
    Return StrReverse(fileEncoding.GetString(BitConverter.GetBytes(iIn)))
  End Function

  Private Function GetDWORD(sIn As String) As UInt32
    If sIn.Length > 4 Then sIn = sIn.Substring(0, 4)
    Return BitConverter.ToUInt32(fileEncoding.GetBytes(StrReverse(sIn)), 0)
  End Function

  Private Function GetWORD(sIn As String) As UInt16
    If sIn.Length > 2 Then sIn = sIn.Substring(0, 2)
    Return BitConverter.ToUInt32(fileEncoding.GetBytes(StrReverse(sIn)), 0)
  End Function

  Public Sub New(File As String)
    ID3Frames = New ArrayList
    m_sMp3File = File
    LoadFile(File)
  End Sub

  Public Property MP3File() As String
    Get
      Return m_sMp3File
    End Get
    Set(value As String)
      m_sMp3File = value
      LoadFile(value)
    End Set
  End Property

  Private Function ConvertGenre(sIn As String) As String
    If IsNumeric(sIn) AndAlso (sIn > -1 And sIn < 256) Then
      Return clsID3v1.GenreName(sIn)
    ElseIf sIn = "RX" Then
      Return "Remix"
    ElseIf sIn = "CR" Then
      Return "Cover"
    Else
      Return sIn
    End If
  End Function

  Public Function ParseFrame(sName As String, ByVal sData As String) As Object()
    Dim ioData As New IO.BinaryReader(New IO.MemoryStream(fileEncoding.GetBytes(sData)))
    Select Case sName
      Case "UFI", "UFID"
        Dim sOwner As String = Nothing
        Dim bTmp As Byte
        Do
          bTmp = ioData.ReadByte
          If bTmp > 0 Then sOwner &= ChrW(bTmp)
        Loop Until bTmp = 0
        If String.IsNullOrEmpty(sOwner) Then Return Nothing
        Dim Ident() As Byte = ioData.ReadBytes(sData.Length - (sOwner.Length + 1))
        Return {sOwner, BitConverter.ToString(Ident)}
      Case "TXX", "TXXX"
        Dim bEncoding As Byte = Asc(sData.Substring(0, 1))
        Dim sFind As String = vbNullChar
        If bEncoding = 1 Or bEncoding = 2 Then sFind &= vbNullChar
        Dim sDesc As String = sData.Substring(1, sData.LastIndexOf(sFind) + (sFind.Length - 1))
        Dim sValue As String = sData.Substring(sDesc.Length + 1)
        sDesc = ParseString(bEncoding, sDesc)
        sValue = ParseString(bEncoding, sValue)
        If String.IsNullOrEmpty(sDesc) Or sDesc = ChrW(&HFEFF) Then Return {sValue}
        Return {sDesc, sValue}
      Case "TCO", "TCON"
        If ID3Header.Version(0) = "4" Then
          Dim Genres As New ArrayList
          Do
            Dim sGenre As String = Nothing
            Dim bTmp As Byte
            Do
              bTmp = ioData.ReadByte
              If bTmp > 0 Then sGenre &= ChrW(bTmp)
            Loop Until bTmp = 0 Or ioData.BaseStream.Position >= ioData.BaseStream.Length - 1
            Genres.Add(ConvertGenre(sGenre))
          Loop Until ioData.BaseStream.Position >= ioData.BaseStream.Length - 1
          Return {Join(Genres.ToArray, "/")}
        Else
          Dim sBak As String = sData
          Dim bEncoding As Byte = Asc(sData.Substring(0, 1))
          sData = ParseString(bEncoding, sData.Substring(1))
          If String.IsNullOrEmpty(sData) Then Return Nothing
          Dim sTmp As String = String.Empty
          Dim lPos As Integer = 0
          Dim startVal As Boolean = False
          Dim Genres As New ArrayList
          Do
            Dim sI As String = sData.Substring(lPos, 1)
            If sI = "(" And Not startVal Then
              If Not String.IsNullOrEmpty(sTmp) Then
                Genres.Add(ConvertGenre(sTmp))
                sTmp = String.Empty
              End If
              startVal = True
            ElseIf sI = ")" And startVal Then
              startVal = False
              If Not String.IsNullOrEmpty(sTmp) Then
                Genres.Add(ConvertGenre(sTmp))
                sTmp = String.Empty
              End If
            Else
              sTmp &= sI
            End If
            lPos += 1
          Loop While lPos < sData.Length
          If Not String.IsNullOrEmpty(sTmp) Then
            Genres.Add(ConvertGenre(sTmp))
            sTmp = String.Empty
          End If
          Return {Join(Genres.ToArray, "/")}
        End If
      Case "MCI", "MCDI" : Return {BitConverter.ToString(fileEncoding.GetBytes(sData))}
      Case "COM", "COMM" : Return {ParseString(Asc(sData.Substring(0, 1)), sData.Substring(4))}
      Case "PIC"
        Dim bEnc As Byte = Asc(sData.Substring(0, 1))
        Dim sFormat As String = sData.Substring(1, 3)
        Dim bPicType As Byte = Asc(sData.Substring(4, 1))
        Dim sDesc As String = sData.Substring(5)
        Dim sFind As String = vbNullChar
        If bEnc = 1 Or bEnc = 2 Then sFind &= vbNullChar
        If sDesc.Contains(sFind) Then
          Dim sPicture As String = sDesc.Substring(sDesc.IndexOf(sFind) + sFind.Length)
          sDesc = sDesc.Substring(0, sDesc.IndexOf(sFind))
          sDesc = ParseString(bEnc, sDesc)
          Return {sFormat, sDesc, bPicType, sPicture}
        Else
          Return Nothing
        End If
      Case "APIC"
        Dim bEnc As Byte = Asc(sData.Substring(0, 1))
        Dim sMIME As String = sData.Substring(1)
        If sMIME.Contains(vbNullChar) Then
          sMIME = sMIME.Substring(0, sMIME.IndexOf(vbNullChar))
          If sData.Length > sMIME.Length + 2 Then
            Dim bPicType As Byte = Asc(sData.Substring(sMIME.Length + 2, 1))
            Dim sDesc As String = sData.Substring(sMIME.Length + 3)
            Dim sFind As String = vbNullChar
            If bEnc = 1 Or bEnc = 2 Then sFind &= vbNullChar
            If sDesc.Contains(sFind) Then
              Dim sPicture As String = sDesc.Substring(sDesc.IndexOf(sFind) + sFind.Length)
              sDesc = sDesc.Substring(0, sDesc.IndexOf(sFind))
              sDesc = ParseString(bEnc, sDesc)
              Return {sMIME, sDesc, bPicType, sPicture}
            Else
              Return Nothing
            End If
          Else
            Return Nothing
          End If
        Else
          Return Nothing
        End If
      Case "ULT", "USLT"
        If Asc(sData.Substring(0, 1)) < 4 Then
          If sData.Substring(4, 1) = vbNullChar Then
            Return {sData.Substring(1, 3), ParseString(Asc(sData.Substring(0, 1)), sData.Substring(5))}
          Else
            Return {ParseString(Asc(sData.Substring(0, 1)), sData.Substring(1))}
          End If
        Else
          Return {sData}
        End If
      Case "WXX", "WXXX"
        Dim bEncoding As Byte = Asc(sData.Substring(0, 1))
        Dim sFind As String = vbNullChar
        If bEncoding = 1 Or bEncoding = 2 Then sFind &= vbNullChar
        Dim sDesc As String = sData.Substring(1, sData.LastIndexOf(sFind) + (sFind.Length - 1))
        Dim sValue As String = sData.Substring(sDesc.Length + 1)
        sDesc = ParseString(bEncoding, sDesc)
        sValue = ParseString(0, sValue)
        If String.IsNullOrWhiteSpace(sDesc) Or sDesc = ChrW(&HFEFF) Then Return {sValue}
        Return {sDesc, sValue}
      Case "GEO", "GEOB"
        Dim bEncoding As Byte = Asc(sData.Substring(0, 1))
        Dim sMIME As String = sData.Substring(1, sData.IndexOf(vbNullChar, 1) - 1)
        Dim sPos As Integer = sMIME.Length + 2
        Dim sFind As String = vbNullChar
        If bEncoding = 1 Or bEncoding = 2 Then sFind &= vbNullChar
        Dim sFile As String = sData.Substring(sPos, sData.IndexOf(sFind, sPos) + (sFind.Length - 1) - sPos)
        sPos += sFile.Length + 1
        Dim sDesc As String = sData.Substring(sPos, sData.IndexOf(sFind, sPos) + (sFind.Length - 1) - sPos)
        sPos += sDesc.Length + 1
        Dim sContent As String = sData.Substring(sPos)
        sFile = ParseString(bEncoding, sFile)
        sDesc = ParseString(bEncoding, sDesc)
        Return {sMIME, sFile, sDesc, sContent}
      Case Else
        If sName.StartsWith("T") Then
          Return {ParseString(Asc(sData.Substring(0, 1)), sData.Substring(1))}
        Else
          'If sName.StartsWith("T") Or sName.StartsWith("W") T  hen
          If Asc(sData.Substring(0, 1)) < 4 Then
            Return {ParseString(Asc(sData.Substring(0, 1)), sData.Substring(1))}
          Else
            Return {sData}
          End If
          'Return ParseString(Asc(Left$(sData, 1)), Mid$(sData, 2))
          'Else
          'Return sData
          'End If
        End If

    End Select
  End Function

  Public Shared Function ImageID(ID As Byte) As String
    Select Case ID
      Case &H0 : Return "Other"
      Case &H1 : Return "32x32 Icon"
      Case &H2 : Return "Icon"
      Case &H3 : Return "Front Cover"
      Case &H4 : Return "Back Cover"
      Case &H5 : Return "Leaflet"
      Case &H6 : Return "Media"
      Case &H7 : Return "Lead Artist"
      Case &H8 : Return "Artist"
      Case &H9 : Return "Conductor"
      Case &HA : Return "Band"
      Case &HB : Return "Composer"
      Case &HC : Return "Lyricist"
      Case &HD : Return "Recording Location"
      Case &HE : Return "During Recording"
      Case &HF : Return "During Performance"
      Case &H10 : Return "Video Screen Capture"
      Case &H11 : Return "Red Herring"
      Case &H12 : Return "Illustration"
      Case &H13 : Return "Band Logo"
      Case &H14 : Return "Publisher Logo"
      Case Else : Return "Unknown (" & Hex(ID) & ")"
    End Select
  End Function

  Private Function ParseString(Encoding As Byte, ByVal Text As String) As String
    Select Case Encoding
      Case 0
        'ISO (NT)
      Case 1
        'UTF 16
        Text = System.Text.Encoding.Unicode.GetString(fileEncoding.GetBytes(Text))
      Case 2
        'UTF 16 BE
        Text = System.Text.Encoding.BigEndianUnicode.GetString(fileEncoding.GetBytes(Text))
      Case 3
        'UTF 8
        Text = System.Text.Encoding.UTF8.GetString(fileEncoding.GetBytes(Text))
      Case Else
        'Probably not encoded...
        'Debug.Print("Unknown Content encoding: " & Encoding & " [" & Text & "]!")
    End Select
    If Text.Contains(vbNullChar) Then Text = Text.Substring(0, Text.IndexOf(vbNullChar))
    Do While Text.Contains(vbNullChar)
      Text = Text.Replace(vbNullChar, "[NULL]")
    Loop
    Do While Text.Contains(vbLf)
      Text = Text.Replace(vbLf, "[LF]")
    Loop
    'Text = Text.Replace(vbCr, vbNewLine)
    Do While Text.StartsWith(vbCr)
      Text = Text.Substring(1)
    Loop
    Do While Text.EndsWith(vbCr)
      Text = Text.Substring(0, Text.Length - 1)
    Loop
    Return Text
  End Function

  Public Function RemoveAll() As Byte
    ID3Frames.Clear()
    RemoveAll = 2
  End Function

  Public Function RemoveFrame(sName As String) As Byte
    For I As Integer = 0 To ID3Frames.Count - 1
      If ID3Frames(I).FrameName = sName Or ID3Frames(I).FrameName = GetNewName(sName, 3) Or ID3Frames(I).FrameName = GetNewName(sName, 4) Then
        If ID3Frames(I).FrameFlags And CONSTS.FFLG_READONLY Then
          Return 3
        Else
          ID3Frames.RemoveAt(I)
          Return 2
        End If
      End If
    Next I
    Return 4
  End Function

  Public Sub Save(Optional SaveAs As String = Nothing)
    If String.IsNullOrEmpty(SaveAs) Then SaveAs = m_sMp3File
    Dim FileData() As Byte = TrimID3v2(m_sMp3File)
    Using ID3Data As New IO.FileStream(SaveAs, IO.FileMode.OpenOrCreate, IO.FileAccess.Write, IO.FileShare.None)
      Using ID3Writer As New IO.BinaryWriter(ID3Data, fileEncoding)
        ID3Header.Identifier = "ID3"
        ID3Header.Flags = 0

        ID3Writer.Write(fileEncoding.GetBytes(ID3Header.Identifier))
        ID3Writer.Write(ID3Header.Version(0))
        ID3Writer.Write(ID3Header.Version(1))
        ID3Writer.Write(ID3Header.Flags)
        Dim bContent() As Byte
        Using ID3Content As New IO.MemoryStream
          Using ID3ContentWriter As New IO.BinaryWriter(ID3Content, fileEncoding)

            For I = 0 To FrameCount - 1
              If Not FrameData(I) = vbNullChar Then
                Dim sName As String = FrameName(I)
                If ID3Header.Version(0) = 3 Then
                  sName = GetNewName(sName, 3)
                ElseIf ID3Header.Version(0) = 4 Then
                  sName = GetNewName(sName, 4)
                End If
                If Not ((FrameFlags(I) And CONSTS.FFLG_TAGALTER) Or (FrameFlags(I) And CONSTS.FFLG_FILEALTER)) Then
                  Dim bData() As Byte = fileEncoding.GetBytes(FrameData(I))
                  'Name
                  ID3ContentWriter.Write(fileEncoding.GetBytes(sName))
                  'DWORD Size
                  If ID3Header.Version(0) = 4 Then
                    ID3ContentWriter.Write(fileEncoding.GetBytes(MakeDWORD(SynchSafe(bData.Length))))
                  Else
                    ID3ContentWriter.Write(fileEncoding.GetBytes(MakeDWORD(bData.Length)))
                  End If
                  'WORD Flags
                  ID3ContentWriter.Write(fileEncoding.GetBytes(MakeWORD(FrameFlags(I))))
                  'Data
                  ID3ContentWriter.Write(bData)
                End If
              End If
            Next I
            Dim Spaces As Integer = &H200
            Do Until ID3Content.Length + &HA < Spaces
              Spaces += &H200
            Loop
            Spaces = Spaces - (ID3Content.Length + &HA)
            ID3ContentWriter.Write(fileEncoding.GetBytes(StrDup(Spaces, vbNullChar)))
          End Using
          bContent = ID3Content.ToArray
        End Using

        ID3Header.Size = bContent.Length
        ID3Writer.Write(fileEncoding.GetBytes(MakeDWORD(SynchSafe(ID3Header.Size))))
        ID3Writer.Write(bContent)

        ID3Writer.Write(FileData)
        ID3Writer.Flush()
      End Using
      ID3Data.Close()
    End Using

  End Sub

  Private Function TrimID3v2(Path As String) As Byte()
    If String.IsNullOrEmpty(Path) Then Return Nothing
    If Not My.Computer.FileSystem.FileExists(Path) Then Return Nothing
    If My.Computer.FileSystem.GetFileInfo(Path).Length >= 1024L * 1024L * 1024L * 4L Then Return Nothing
    Dim bFile() As Byte = My.Computer.FileSystem.ReadAllBytes(Path)
    Dim lFrameStart As Integer = 0
    Do
      lFrameStart = GetBytePos(bFile, &HFF, lFrameStart)
      If lFrameStart >= 0 AndAlso CheckMPEG(bFile, lFrameStart) Then Exit Do
      lFrameStart += 1
    Loop
    If lFrameStart = 0 Then Return bFile
    Dim lFrameLen As Integer = bFile.Length - lFrameStart
    Dim bReturn(lFrameLen - 1) As Byte
    Array.ConstrainedCopy(bFile, lFrameStart, bReturn, 0, lFrameLen)
    Return bReturn
  End Function

  Public Function SetFrame(sName As String, ByVal sData As String, Optional ByVal iFlags As Integer = 0) As Byte
    Dim B As Boolean = False
    If Not String.IsNullOrEmpty(sData) Then
      For I As Integer = 0 To ID3Frames.Count - 1
        If ID3Frames(I).FrameName = sName Or ID3Frames(I).FrameName = GetNewName(sName, 3) Or ID3Frames(I).FrameName = GetNewName(sName, 4) Then
          If ID3Frames(I).FrameFlags And CONSTS.FFLG_READONLY Then
            Return 3
          Else
            B = True
            ID3Frames(I).FrameData = sData
            ID3Frames(I).FrameFlags = iFlags
            Return 0
          End If
        End If
      Next I
      If Not B Then
        Dim sNewName As String = ""
        If ID3Header.Version(0) = 2 Then
          sNewName = sName
        ElseIf ID3Header.Version(0) = 3 Then
          sNewName = GetNewName(sName, 3)
        ElseIf ID3Header.Version(0) = 4 Then
          sNewName = GetNewName(sName, 4)
        Else
          Return 4
        End If
        If String.IsNullOrEmpty(sNewName) Then Return 4
        Return AddFrame(sNewName, sData, iFlags)
      End If
      Return 4
    Else
      Return RemoveFrame(sName)
    End If
  End Function

#Region "IDisposable Support"
  Private disposedValue As Boolean ' To detect redundant calls

  ' IDisposable
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        ' TODO: dispose managed state (managed objects).
      End If

      ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
      ' TODO: set large fields to null.
    End If
    Me.disposedValue = True
  End Sub

  ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
  'Protected Overrides Sub Finalize()
  '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
  '    Dispose(False)
  '    MyBase.Finalize()
  'End Sub

  ' This code added by Visual Basic to correctly implement the disposable pattern.
  Public Sub Dispose() Implements IDisposable.Dispose
    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region
End Class
