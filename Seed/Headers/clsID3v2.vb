Public Class clsID3v2
  Implements IDisposable

  Public Enum ID3Returns As Byte
    [Set] = 0
    Added = 1
    Removed = 2
    [ReadOnly] = 3
    GeneralFailure = 4
  End Enum
  Public Enum TextEncoding As Byte
    NT_ISO = 0
    UTF_16_LE = 1
    UNICODE = 1
    UTF_16_BE = 2
    UTF_8 = 3
  End Enum
  Public Enum HFLG As Byte
    FOOTER = &H10
    EXPERIMENTAL = &H20
    EXTENDED = &H40
    UNSYNCH = &H80
  End Enum
  Public Enum FFLG As UInteger
    TAGALTER = &H4000
    FILEALTER = &H2000
    [READONLY] = &H1000
    [GROUP] = &H40
    COMPRESS = &H8
    ENCRYPT = &H4
    UNSYNCH = &H2
    DATALENGTH = &H1
  End Enum
  Public Enum ID3_PIC_TYPE As Byte
    OTHER = 0
    PNG_ICON_32x32 = 1
    FILE_ICON = 2
    FRONT_COVER = 3
    BACK_COVER = 4
    LEAFLET = 5
    MEDIA_LABEL = 6
    LEAD_ARTIST = 7
    ARTIST = 8
    CONDUCTOR = 9
    ORCHESTRA_BAND = 10
    COMPOSER = 11
    LYRICIST = 12
    RECORDING_LOCATION = 13
    DURING_RECORDING = 14
    DURING_PERFORMANCE = 15
    SCREENCAP = 16
    RED_HERRING = 17
    ILLUSTRATION = 18
    BAND_LOGO = 19
    STUDIO_LOGO = 20
    INVALID = 255
  End Enum
  Public Shared ReadOnly Property ID3_PIC_TYPE_DISPLAYORDER As ID3_PIC_TYPE()
    Get
      Return {ID3_PIC_TYPE.FRONT_COVER,
              ID3_PIC_TYPE.OTHER,
              ID3_PIC_TYPE.SCREENCAP,
              ID3_PIC_TYPE.MEDIA_LABEL,
              ID3_PIC_TYPE.BAND_LOGO,
              ID3_PIC_TYPE.LEAD_ARTIST,
              ID3_PIC_TYPE.ARTIST,
              ID3_PIC_TYPE.ORCHESTRA_BAND,
              ID3_PIC_TYPE.BACK_COVER,
              ID3_PIC_TYPE.DURING_RECORDING,
              ID3_PIC_TYPE.DURING_PERFORMANCE,
              ID3_PIC_TYPE.ILLUSTRATION}
    End Get
  End Property
  Public Enum ID3_PIC_MIME As Byte
    BMP = 0
    GIF = 1
    JPG = 2
    PNG = 3
    INVALID = 255
  End Enum

  Private Structure ID3v2Header
    Public Identifier As String '* 3
    Public Version As Byte() ' 0 to 1
    Public Flags As HFLG
    Public Size As UInteger
  End Structure
  Private Structure ID3v2Frame2
    Public FrameName As String ' * 3
    Public FrameSize As Byte() ' 0 to 2
  End Structure
  Private Structure ID3v2Frame1
    Public FrameName As String '* 4
    Public FrameSize As UInteger
    Public FrameFlags As Byte() '0 to 1
  End Structure
  Private Structure ID3v2Frame
    Public FrameName As String
    Public FrameData As Byte()
    Public FrameFlags As FFLG
    Public Sub New(sName As String, uFlags As FFLG, bData As Byte())
      FrameName = sName
      FrameFlags = uFlags
      FrameData = bData
    End Sub
  End Structure

  Private m_sMp3File As String
  Private ID3Header As ID3v2Header
  Private HasID3v2 As Boolean
  Private ID3Frames As List(Of ID3v2Frame)
  Private lID3Len As Integer

  Public Shared Function GetNullChar(Encoding As TextEncoding) As String
    Select Case Encoding
      Case TextEncoding.NT_ISO
        Return vbNullChar
      Case TextEncoding.UTF_8
        Return vbNullChar
      Case TextEncoding.UTF_16_LE, TextEncoding.UTF_16_BE, TextEncoding.UNICODE
        Return vbNullChar & vbNullChar
      Case Else
        Return vbNullChar
    End Select
  End Function
  Public Shared Function GetNullCharArr(Encoding As TextEncoding) As Byte()
    Select Case Encoding
      Case TextEncoding.NT_ISO
        Return {CByte(0)}
      Case TextEncoding.UTF_8
        Return {CByte(0)}
      Case TextEncoding.UTF_16_LE, TextEncoding.UTF_16_BE, TextEncoding.UNICODE
        Return {CByte(0), CByte(0)}
      Case Else
        Return {CByte(0)}
    End Select
  End Function
  Private Shared Function TrimNull(inStr As String) As String
    inStr = inStr.Trim
    If inStr.Contains(vbNullChar) Then Return inStr.Substring(0, inStr.IndexOf(vbNullChar))
    Return inStr
  End Function
  Private Shared Function IsSimpleChar(inStr As String, index As Integer) As Boolean
    Return IsSimpleChar(inStr(index))
  End Function
  Private Shared Function IsSimpleChar(inChar As Char) As Boolean
    Dim iChar As Integer = AscW(inChar)
    If iChar = &H9 Then Return True
    If iChar = &HA Or iChar = &HD Then Return True
    If iChar >= &H20 And iChar <= &H7E Then Return True
    Return False
  End Function

  Public Class EncodedText
    Public Encoding As TextEncoding
    Public Text As String
    Public Sub New(sText As String)
      Text = sText
      Encoding = TextEncoding.NT_ISO
    End Sub
    Public Sub New(sText As String, tEncoding As TextEncoding)
      Text = sText
      Encoding = tEncoding
    End Sub
    Public Shared ReadOnly Property Empty As EncodedText
      Get
        Return New EncodedText(Nothing, TextEncoding.NT_ISO)
      End Get
    End Property
    Public ReadOnly Property IsEmpty As Boolean
      Get
        If String.IsNullOrEmpty(Text) And Encoding = TextEncoding.NT_ISO Then Return True
        Return False
      End Get
    End Property
  End Class

  Public Function AddBareFrame(sName As String, bData As Byte(), Optional ByVal iFlags As FFLG = 0) As ID3Returns
    If sName = "TCON" Or sName = "TCO" Then
      Dim bEncoding As TextEncoding = TextEncoding.NT_ISO
      Dim tN As ParseResponse = ParseResponse.FromByteArray(sName, bData)
      Dim tNew As Parsed_TCON = Nothing
      If tN.GetType Is GetType(Parsed_TCON) Then
        tNew = tN
        bEncoding = tNew.Encoding
      End If
      Dim sGenres As String = Nothing
      For I As Integer = FrameCount - 1 To 0 Step -1
        Dim sFrame As ID3v2Frame = ID3Frames(I)
        If sFrame.FrameName = "TCO" Or sFrame.FrameName = "TCON" Then
          Dim tE As ParseResponse = ParseResponse.FromByteArray(sFrame.FrameName, sFrame.FrameData)
          Dim tExisting As Parsed_TCON = Nothing
          If tE.GetType Is GetType(Parsed_TCON) Then tExisting = tE
          If tExisting IsNot Nothing AndAlso tExisting.GenreList.Length > 0 Then
            If tNew Is Nothing AndAlso tExisting IsNot Nothing Then bEncoding = tExisting.Encoding
            For Each gEntity As String In tExisting.GenreList
              If String.IsNullOrEmpty(gEntity) Then Continue For
              Dim gID As String = gEntity
              For g As Integer = &H0 To &HBF
                If StrComp(gEntity, clsID3v1.GenreName(g), CompareMethod.Text) = 0 Then
                  gID = g
                  Exit For
                End If
              Next
              If Not ID3Header.Version(0) = 4 AndAlso IsNumeric(gID) Then gEntity = "(" & gID & ")"
              If String.IsNullOrEmpty(sGenres) Then
                sGenres = gEntity
              Else
                sGenres &= "; " & gEntity
              End If
            Next
          End If
          ID3Frames.RemoveAt(I)
        End If
      Next

      If tNew IsNot Nothing AndAlso tNew.GenreList.Length > 0 Then
        For Each gEntity As String In tNew.GenreList
          If String.IsNullOrEmpty(gEntity) Then Continue For
          Dim gID As String = gEntity
          For g As Integer = &H0 To &HBF
            If StrComp(gEntity, clsID3v1.GenreName(g), CompareMethod.Text) = 0 Then
              gID = g
              Exit For
            End If
          Next
          If Not ID3Header.Version(0) = 4 AndAlso IsNumeric(gID) Then gEntity = "(" & gID & ")"
          If String.IsNullOrEmpty(sGenres) Then
            sGenres = gEntity
          Else
            sGenres &= "; " & gEntity
          End If
        Next
      End If
      Dim bEntity As Byte() = Nothing
      bEntity = MakeFrameStringData(bEncoding, sGenres, ID3Header.Version)
      ID3Frames.Add(New ID3v2Frame(sName, iFlags, bEntity))
      Return ID3Returns.Added
    End If
    ID3Frames.Add(New ID3v2Frame(sName, iFlags, bData))
    Return ID3Returns.Added
  End Function

  Public Function AddTextFrame(sName As String, ByVal Contents As EncodedText, Optional ByVal iFlags As FFLG = 0) As ID3Returns
    Return AddBareFrame(sName, MakeFrameStringData(Contents.Encoding, Contents.Text, ID3Header.Version), iFlags)
  End Function

  Private Shared Function MakeFrameStringData(Encoding As TextEncoding, Text As String, Version As Byte(), Optional SkipEncoding As Boolean = False) As Byte()
    If Version(0) = 4 Then
      Dim bData As New List(Of Byte)
      If Not SkipEncoding Then bData.Add(Encoding)
      Select Case Encoding
        Case TextEncoding.UTF_16_LE
          bData.Add(&HFF)
          bData.Add(&HFE)
          If Not String.IsNullOrEmpty(Text) Then bData.AddRange(System.Text.Encoding.Unicode.GetBytes(Text))
        Case TextEncoding.UTF_16_BE
          If Not String.IsNullOrEmpty(Text) Then bData.AddRange(System.Text.Encoding.BigEndianUnicode.GetBytes(Text))
        Case TextEncoding.UTF_8
          If Not String.IsNullOrEmpty(Text) Then bData.AddRange(System.Text.Encoding.UTF8.GetBytes(Text))
        Case Else
          If Not String.IsNullOrEmpty(Text) Then bData.AddRange(fileEncoding.GetBytes(Text))
      End Select
      bData.AddRange(GetNullCharArr(Encoding))
      Return bData.ToArray
    Else
      Dim bData As New List(Of Byte)
      Select Case Encoding
        Case TextEncoding.UTF_16_LE, TextEncoding.UTF_8
          If Not SkipEncoding Then bData.Add(TextEncoding.UNICODE)
          bData.Add(&HFF)
          bData.Add(&HFE)
          If Not String.IsNullOrEmpty(Text) Then bData.AddRange(System.Text.Encoding.Unicode.GetBytes(Text))
          bData.AddRange(GetNullCharArr(TextEncoding.UNICODE))
        Case TextEncoding.UTF_16_BE
          If Not SkipEncoding Then bData.Add(TextEncoding.UNICODE)
          bData.Add(&HFE)
          bData.Add(&HFF)
          If Not String.IsNullOrEmpty(Text) Then bData.AddRange(System.Text.Encoding.BigEndianUnicode.GetBytes(Text))
          bData.AddRange(GetNullCharArr(TextEncoding.UTF_16_BE))
        Case Else
          If Not SkipEncoding Then bData.Add(TextEncoding.NT_ISO)
          If Not String.IsNullOrEmpty(Text) Then bData.AddRange(fileEncoding.GetBytes(Text))
          bData.AddRange(GetNullCharArr(TextEncoding.NT_ISO))
      End Select
      Return bData.ToArray
    End If
  End Function

  Private Shared Function MakeFrameStringData(Text As String, NullTerminator As Boolean) As Byte()
    Dim bData As New List(Of Byte)
    If Not String.IsNullOrEmpty(Text) Then bData.AddRange(fileEncoding.GetBytes(Text))
    If NullTerminator Then bData.Add(0)
    Return bData.ToArray 
  End Function

  Public Shared Function MIMEtoString(Format As ID3_PIC_MIME, id3v2 As Boolean) As String
    If id3v2 Then
      Select Case Format
        Case ID3_PIC_MIME.BMP : Return "BMP"
        Case ID3_PIC_MIME.GIF : Return "GIF"
        Case ID3_PIC_MIME.JPG : Return "JPG"
        Case ID3_PIC_MIME.PNG : Return "PNG"
      End Select
      Return "BMP"
    Else
      Select Case Format
        Case ID3_PIC_MIME.BMP : Return "image/bmp"
        Case ID3_PIC_MIME.GIF : Return "image/gif"
        Case ID3_PIC_MIME.JPG : Return "image/jpeg"
        Case ID3_PIC_MIME.PNG : Return "image/png"
      End Select
      Return "image/bmp"
    End If
  End Function

  Public Shared Function ExtToMIME(Path As String) As ID3_PIC_MIME
    Select Case IO.Path.GetExtension(Path).ToLower
      Case ".jpg", ".jpeg", ".jpe" : Return ID3_PIC_MIME.JPG
      Case ".gif" : Return ID3_PIC_MIME.GIF
      Case ".png" : Return ID3_PIC_MIME.PNG
      Case ".bmp" : Return ID3_PIC_MIME.BMP
    End Select
    MsgBox("Unknown Image Extension: " & IO.Path.GetExtension(Path).ToLower & vbNewLine & "ExtToMIME function may need updating.", MsgBoxStyle.Exclamation, My.Application.Info.Title & " ID3v2")
    Return ID3_PIC_MIME.INVALID
  End Function

  Public Shared Function StrToMIME(Format As String, id3v2_2 As Boolean) As ID3_PIC_MIME
    If id3v2_2 Then
      Select Case Format.ToUpper
        Case "BMP" : Return ID3_PIC_MIME.BMP
        Case "GIF" : Return ID3_PIC_MIME.GIF
        Case "JPG" : Return ID3_PIC_MIME.JPG
        Case "PNG" : Return ID3_PIC_MIME.PNG
      End Select
      MsgBox("Unknown Image Type: " & Format & vbNewLine & "StrToMIME function may need updating.", MsgBoxStyle.Exclamation, My.Application.Info.Title & " ID3v2")
      Return ID3_PIC_MIME.INVALID
    Else
      Select Case Format.ToLower
        Case "image/bmp" : Return ID3_PIC_MIME.BMP
        Case "image/gif" : Return ID3_PIC_MIME.GIF
        Case "image/jpeg", "image/jpg" : Return ID3_PIC_MIME.JPG
        Case "image/png" : Return ID3_PIC_MIME.PNG
      End Select
      Select Case Format.ToUpper
        Case "BMP" : Return ID3_PIC_MIME.BMP
        Case "GIF" : Return ID3_PIC_MIME.GIF
        Case "JPG" : Return ID3_PIC_MIME.JPG
        Case "PNG" : Return ID3_PIC_MIME.PNG
      End Select
      MsgBox("Unknown MIME Type: " & Format & vbNewLine & "StrToMIME function may need updating.", MsgBoxStyle.Exclamation, My.Application.Info.Title & " ID3v2")
      Return ID3_PIC_MIME.INVALID
    End If
  End Function

  Public Function AddAPICFrame(ByVal pData As Byte(), ByVal [Type] As ID3_PIC_TYPE, ByVal Format As ID3_PIC_MIME, ByVal Description As EncodedText, Optional ByVal iFlags As FFLG = 0) As ID3Returns
    Dim sName As String = Nothing
    If ID3Header.Version(0) = 2 Then
      sName = "PIC"
    Else
      sName = "APIC"
    End If
    Return AddBareFrame(sName, (New Parsed_APIC(sName, Description.Encoding, Format, Description.Text, Type, pData)).ToByteArray(ID3Header.Version), iFlags)
  End Function

  Private Function CheckID3v2(ByRef ioReader As IO.BinaryReader) As Integer
    Try
      ioReader.BaseStream.Position = 0
      Do Until ioReader.BaseStream.Position >= ioReader.BaseStream.Length - 1
        Dim readNum As Integer = 4096
        If ioReader.BaseStream.Length - 4096 < ioReader.BaseStream.Position Then readNum = ioReader.BaseStream.Length - ioReader.BaseStream.Position
        Dim sFile As String = ioReader.ReadChars(readNum)
        If sFile.Contains("ID3") Then
          Return ioReader.BaseStream.Position - readNum + sFile.IndexOf("ID3")
        End If
      Loop
      Return -1
    Catch ex As Exception
      Return -1
    End Try
  End Function

  Public Function FindFrame(sName As String) As Boolean
    Return FindFrameMatchCount(sName) > 0
  End Function

  Public Function FindFrameMatchCount(sName As String) As Integer
    Dim frameMatches As Integer = 0
    Try
      Dim sOldName As String = GetOldName(sName)
      Dim sNewName3 As String = GetNewName(sName, 3)
      Dim sNewName4 As String = GetNewName(sName, 4)
      For I As Integer = 0 To ID3Frames.Count - 1
        Dim sFind As String = TrimNull(ID3Frames(I).FrameName)
        If (sFind = sOldName AndAlso Not IsNothing(ParseResponse.FromByteArray(sOldName, ID3Frames(I).FrameData))) Or
           (sFind = sNewName3 AndAlso Not IsNothing(ParseResponse.FromByteArray(sNewName3, ID3Frames(I).FrameData))) Or
           (sFind = sNewName4 AndAlso Not IsNothing(ParseResponse.FromByteArray(sNewName4, ID3Frames(I).FrameData))) Then frameMatches += 1
      Next
    Catch ex As Exception
    End Try
    Return frameMatches
  End Function

  Public Function FindFrameMatches(sName As String) As ParseResponse()
    Dim MatchLists As New List(Of ParseResponse)
    Try
      Dim sOldName As String = GetOldName(sName)
      Dim sNewName3 As String = GetNewName(sName, 3)
      Dim sNewName4 As String = GetNewName(sName, 4)
      For I As Integer = 0 To ID3Frames.Count - 1
        Dim sFind As String = TrimNull(ID3Frames(I).FrameName)
        If sFind = sOldName AndAlso Not IsNothing(ParseResponse.FromByteArray(sOldName, ID3Frames(I).FrameData)) Then
          MatchLists.Add(ParseResponse.FromByteArray(sOldName, ID3Frames(I).FrameData))
        ElseIf sFind = sNewName3 AndAlso Not IsNothing(ParseResponse.FromByteArray(sNewName3, ID3Frames(I).FrameData)) Then
          MatchLists.Add(ParseResponse.FromByteArray(sNewName3, ID3Frames(I).FrameData))
        ElseIf sFind = sNewName4 AndAlso Not IsNothing(ParseResponse.FromByteArray(sNewName4, ID3Frames(I).FrameData)) Then
          MatchLists.Add(ParseResponse.FromByteArray(sNewName4, ID3Frames(I).FrameData))
        End If
      Next
      If MatchLists.Count = 0 Then Return Nothing
      Return MatchLists.ToArray
    Catch ex As Exception
      Return Nothing
    End Try
  End Function

  Public Function FindFrameMatchString(sName As String, Optional delimA As String = "; ", Optional delimB As String = "; ") As String
    Dim sMatch As String = Nothing
    Try
      Dim sOldName As String = GetOldName(sName)
      Dim sNewName3 As String = GetNewName(sName, 3)
      Dim sNewName4 As String = GetNewName(sName, 4)
      For I As Integer = 0 To ID3Frames.Count - 1
        Dim sFind As String = TrimNull(ID3Frames(I).FrameName)
        If sFind = sOldName AndAlso Not IsNothing(ParseResponse.FromByteArray(sOldName, ID3Frames(I).FrameData)) Then
          sMatch &= Join(ParseResponse.FromByteArray(sOldName, ID3Frames(I).FrameData).StringData, delimB) & delimA
        ElseIf sFind = sNewName3 AndAlso Not IsNothing(ParseResponse.FromByteArray(sNewName3, ID3Frames(I).FrameData)) Then
          sMatch &= Join(ParseResponse.FromByteArray(sNewName3, ID3Frames(I).FrameData).StringData, delimB) & delimA
        ElseIf sFind = sNewName4 AndAlso Not IsNothing(ParseResponse.FromByteArray(sNewName4, ID3Frames(I).FrameData)) Then
          sMatch &= Join(ParseResponse.FromByteArray(sNewName4, ID3Frames(I).FrameData).StringData, delimB) & delimA
        End If
      Next
      If Not String.IsNullOrEmpty(sMatch) AndAlso sMatch.EndsWith(delimA) Then sMatch = sMatch.Substring(0, sMatch.Length - delimA.Length)
      Return sMatch
    Catch ex As Exception
      Return Nothing
    End Try
  End Function

  Public ReadOnly Property FrameCount() As Integer
    Get
      Return ID3Frames.Count
    End Get
  End Property

  Public ReadOnly Property FrameData(Index As Integer) As Byte()
    Get
      Return ID3Frames(Index).FrameData
    End Get
  End Property

  Public ReadOnly Property FrameFlags(Index As Integer) As Integer
    Get
      Return ID3Frames(Index).FrameFlags
    End Get
  End Property

  Public ReadOnly Property FrameName(Index As Integer) As String
    Get
      Return TrimNull(ID3Frames(Index).FrameName)
    End Get
  End Property

  Public Property Genre() As String
    Get
      Try
        Dim gData As String = FindFrameMatchString("TCO", vbNullChar, vbNullChar)
        Dim gList As New List(Of String)
        If gData.Contains("(") Then
          Dim gTmp As String = Nothing
          For I As Integer = 0 To gData.Length - 1
            If gData(I) = "(" Then
              If Not String.IsNullOrEmpty(gTmp) Then
                gList.Add(gTmp)
                gTmp = Nothing
              End If
              If I < gData.Length - 1 AndAlso gData(I + 1) = "(" Then
                gTmp = "("
                I += 1
              End If
            ElseIf gData(I) = ")" Then
              If gTmp.StartsWith("(") And Not gTmp.Contains(")") Then
                gTmp &= ")"
              Else
                If Not String.IsNullOrEmpty(gTmp) Then
                  gList.Add(gTmp)
                  gTmp = Nothing
                End If
              End If
            Else
              gTmp &= gData(I)
            End If
          Next
          If Not String.IsNullOrEmpty(gTmp) Then
            gList.Add(gTmp)
            gTmp = Nothing
          End If
        ElseIf gData.Contains(vbNullChar) Or gData.Contains(";") Or gData.Contains(",") Or gData.Contains("/") Or gData.Contains("|") Then
          Dim gSplit As String = gData.Replace(vbNullChar, "|")
          gSplit = gSplit.Replace(";", "|")
          gSplit = gSplit.Replace(",", "|")
          gSplit = gSplit.Replace("/", "|")
          Do While gSplit.Contains("||")
            gSplit = gSplit.Replace("||", "|")
          Loop
          gList.AddRange(Split(gSplit, "|"))
        Else
          gList.Add(gData)
        End If
        For I As Integer = 0 To gList.Count - 1
          If Not String.IsNullOrEmpty(gList(I)) AndAlso IsNumeric(gList(I)) Then gList(I) = clsID3v1.GenreName(gList(I))
        Next I
        Dim sGenre As String = Join(gList.ToArray, vbNewLine).Trim
        Do While sGenre.Contains(vbNewLine & vbNewLine)
          sGenre = sGenre.Replace(vbNewLine & vbNewLine, vbNewLine)
        Loop
        Return sGenre
      Catch ex As Exception
        Genre = vbNullString
      End Try
    End Get
    Set(value As String)
      If FindFrame("TCO") Then RemoveFrame("TCO")
      Dim gList As New List(Of String)
      If value.Contains(vbNullChar) Or value.Contains(";") Or value.Contains(",") Or value.Contains("/") Or value.Contains("|") Then
        Dim gSplit As String = value.Replace(vbNullChar, "|")
        gSplit = gSplit.Replace(";", "|")
        gSplit = gSplit.Replace(",", "|")
        gSplit = gSplit.Replace("/", "|")
        Do While gSplit.Contains("||")
          gSplit = gSplit.Replace("||", "|")
        Loop
        gList.AddRange(Split(gSplit, "|"))
      Else
        gList.Add(value)
      End If
      If gList.Count = 0 Then Return
      For Each gEntity In gList
        Dim gID As String = gEntity
        For g As Integer = &H0 To &HBF
          If StrComp(gEntity, clsID3v1.GenreName(g), CompareMethod.Text) = 0 Then
            gID = g
            Exit For
          End If
        Next
        If Not ID3Header.Version(0) = 4 AndAlso IsNumeric(gID) Then gEntity = "(" & gID & ")"
        AddBareFrame("TCO", MakeFrameStringData(TextEncoding.NT_ISO, gEntity, ID3Header.Version))
      Next
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
      MsgBox("Extended Header Read Error" & vbNewLine & ex.ToString, MsgBoxStyle.Exclamation, My.Application.Info.Title & " ID3v2")
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
        Dim bFrameData As Byte() = ioReader.ReadBytes(ID3Frame.FrameSize)
        AddBareFrame(ID3Frame.FrameName, bFrameData, Val("&H" & ID3Frame.FrameFlags(1).ToString("x2") & ID3Frame.FrameFlags(0).ToString("x2")))
      End If
    Catch ex As Exception
      MsgBox("Frame Read Error (v2.3+)" & vbNewLine & ex.ToString, MsgBoxStyle.Exclamation, My.Application.Info.Title & " ID3v2")
    End Try
  End Sub

  Private Sub GetFrame2(ByRef ioReader As IO.BinaryReader)
    Try
      Dim ID3Frame As ID3v2Frame2
      ID3Frame.FrameName = ioReader.ReadChars(3)
      ReDim ID3Frame.FrameSize(2)
      ID3Frame.FrameSize = ioReader.ReadBytes(3)
      Dim lFrameSize As Integer = CLng("&H00" & ID3Frame.FrameSize(0).ToString("x2") & ID3Frame.FrameSize(1).ToString("x2") & ID3Frame.FrameSize(2).ToString("x2"))
      If lFrameSize < ioReader.BaseStream.Length And lFrameSize > 0 Then
        Dim bFrameData As Byte() = ioReader.ReadBytes(lFrameSize)
        AddBareFrame(ID3Frame.FrameName, bFrameData)
      End If
    Catch ex As Exception
      MsgBox("Frame Read Error (v2.2)" & vbNewLine & ex.ToString, MsgBoxStyle.Exclamation, My.Application.Info.Title & " ID3v2")
    End Try
  End Sub

  Private Sub GetHeader(ByRef ioReader As IO.BinaryReader, ByVal StartLoc As Integer)
    Try
      HasID3v2 = False
      If StartLoc < 0 Then Return
      ioReader.BaseStream.Position = StartLoc
      ID3Header.Identifier = ioReader.ReadChars(3)
      If Not ID3Header.Identifier = "ID3" Then Return
      ReDim ID3Header.Version(1)
      ID3Header.Version = ioReader.ReadBytes(2)
      ID3Header.Flags = ioReader.ReadByte
      If Not (ID3Header.Flags And &HF0) = ID3Header.Flags Then Return
      ID3Header.Size = ioReader.ReadUInt32
      ID3Header.Size = UnSynchSafe(GetSize(ID3Header.Size))
      Select Case ID3Header.Version(0)
        Case 2, 3, 4
          Select Case ID3Header.Version(1)
            Case 0
              HasID3v2 = True
              Return
            Case Else
              If MsgBox("Unknown ID3v2 Revision: v2." & ID3Header.Version(0) & "." & ID3Header.Version(1) & "." & vbNewLine & "Try to parse as v2." & ID3Header.Version(0) & ".0?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                HasID3v2 = True
                Return
              Else
                Return
              End If
          End Select
        Case Else
          Dim NearestV As Integer = 0
          If ID3Header.Version(0) < 2 Then
            NearestV = 2
          ElseIf ID3Header.Version(0) > 4 Then
            NearestV = 4
          Else
            NearestV = 3
          End If
          If MsgBox("Unknown ID3v2 Version: v2." & ID3Header.Version(0) & "." & ID3Header.Version(1) & "." & vbNewLine & "Try to parse as v2." & NearestV & ".0?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            ID3Header.Version(0) = NearestV
            ID3Header.Version(1) = 0
            HasID3v2 = True
            Return
          Else
            Return
          End If
      End Select
    Catch ex As Exception
      HasID3v2 = False
    End Try
  End Sub

  Private Function BufferHex(lIn As UInteger, Optional lLen As UShort = 2) As String
    Dim sVal As String = Hex(lIn)
    Do While sVal.Length < lLen : sVal = "0" & sVal : Loop
    Return sVal
  End Function

  Private Function GetNewName(sOldName As String, rev As Integer) As String
    Select Case TrimNull(sOldName).ToUpper
      Case "BUF" : Return "RBUF"
      Case "CNT" : Return "PCNT"
      Case "COM" : Return "COMM"
      Case "CRA" : Return "AENC"
      Case "CRM" : Return sOldName
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
      Case "TCP" : Return "TCMP"
      Case "TCR" : Return "TCOP"
      Case "TDA" : Return sOldName
      Case "TDY" : Return "TDLY"
      Case "TEN" : Return "TENC"
      Case "TFT" : Return "TFLT"
      Case "TIM" : Return sOldName
      Case "TKE" : Return "TKEY"
      Case "TLA" : Return "TLAN"
      Case "TLE" : Return "TLEN"
      Case "TMT" : Return "TMED"
      Case "TOA" : Return "TOPE"
      Case "TOF" : Return "TOFN"
      Case "TOL" : Return "TOLY"
      Case "TOR" : Return If(rev = 3, "TORY", "TDOR")
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
      Case "TST" : Return "TSOT"
      Case "TSA" : Return "TSOA"
      Case "TSP" : Return "TSOP"
      Case "TS2" : Return "TSO2"
      Case "TSC" : Return "TSOC"
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
      Case Else : Return sOldName
    End Select
  End Function

  Private Shared Function GetOldName(sNewName As String) As String
    Select Case TrimNull(sNewName).ToUpper
      Case "RBUF" : Return "BUF"
      Case "PCNT" : Return "CNT"
      Case "COMM" : Return "COM"
      Case "AENC" : Return "CRA"
      Case "CRM" : Return sNewName
      Case "ETCO" : Return "ETC"
      Case "EQUA", "EQU2" : Return "EQU"
      Case "GEOB" : Return "GEO"
      Case "IPLS", "TIPL" : Return "IPL"
      Case "LINK" : Return "LNK"
      Case "MCDI" : Return "MCI"
      Case "MLLT" : Return "MLL"
      Case "APIC" : Return "PIC"
      Case "POPM" : Return "POP"
      Case "RVRB" : Return "REV"
      Case "RVAD", "RVA2" : Return "RVA"
      Case "SYLT" : Return "SLT"
      Case "SYTC" : Return "STC"
      Case "TALB" : Return "TAL"
      Case "TBPM" : Return "TBP"
      Case "TCMP" : Return "TCP"
      Case "TCOM" : Return "TCM"
      Case "TCON" : Return "TCO"
      Case "TCOP" : Return "TCR"
      Case "TDA" : Return sNewName
      Case "TDLY" : Return "TDY"
      Case "TENC" : Return "TEN"
      Case "TFLT" : Return "TFT"
      Case "TIM" : Return sNewName
      Case "TKEY" : Return "TKE"
      Case "TLAN" : Return "TLA"
      Case "TLEN" : Return "TLE"
      Case "TMED" : Return "TMT"
      Case "TOPE" : Return "TOA"
      Case "TOFN" : Return "TOF"
      Case "TOLY" : Return "TOL"
      Case "TDOR", "TORY" : Return "TOR"
      Case "TOAL" : Return "TOT"
      Case "TPE1" : Return "TP1"
      Case "TPE2" : Return "TP2"
      Case "TPE3" : Return "TP3"
      Case "TPE4" : Return "TP4"
      Case "TPOS" : Return "TPA"
      Case "TPUB" : Return "TPB"
      Case "TSRC" : Return "TRC"
      Case "TSOT" : Return "TST"
      Case "TSOA" : Return "TSA"
      Case "TSOP" : Return "TSP"
      Case "TSO2" : Return "TS2"
      Case "TSOC" : Return "TSC"
      Case "TDAT", "TDRC" : Return "TRD"
      Case "TRCK" : Return "TRK"
      Case "TSIZ" : Return "TSI"
      Case "TSSE" : Return "TSS"
      Case "TIT1" : Return "TT1"
      Case "TIT2" : Return "TT2"
      Case "TIT3" : Return "TT3"
      Case "TEXT" : Return "TXT"
      Case "TXXX" : Return "TXX"
      Case "TYER" : Return "TYE"
      Case "UFID" : Return "UFI"
      Case "ULST" : Return "ULT"
      Case "WOAF" : Return "WAF"
      Case "WOAR" : Return "WAR"
      Case "WOAS" : Return "WAS"
      Case "WCOM" : Return "WCM"
      Case "WCOP" : Return "WCP"
      Case "WPUB" : Return "WPB"
      Case "WXXX" : Return "WXX"
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
      Case "TCP", "TCMP" : Return "Compilation"
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
      Case "TOR", "TORY", "TDOR" : Return "Original Release Year"
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
      Case "TSA", "TSOA" : Return "Artist Sort Order"
      Case "TSP", "TSOP" : Return "Band Sort Order"
      Case "TST", "TSOT" : Return "Title Sort Order"
      Case "TSC", "TSOC" : Return "Composer Sort Order"
      Case "TS2", "TSO2" : Return "Album Sort Order"
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
    Dim bFF As Byte() = BitConverter.GetBytes(lFromFile)
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
    If Not io.file.exists(sFile) Then Return
    Dim lStart As Integer
    Dim bTest As Byte
    Dim lLast As Integer
    Try
      ID3Frames.Clear()

      Using bR As New IO.BinaryReader(New IO.FileStream(sFile, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read), fileEncoding)
        lStart = CheckID3v2(bR)
        If lStart = -1 Then Return
        GetHeader(bR, lStart)
        If HasID3v2 Then
          If (ID3Header.Flags And HFLG.EXTENDED) = HFLG.EXTENDED Then GetExtendedHeader(bR, lStart)
          Do While bR.BaseStream.Position < bR.BaseStream.Length - 1
            If ID3Header.Version(0) = 2 Then
              GetFrame2(bR)
            Else
              GetFrame(bR)
            End If
            lLast = bR.BaseStream.Position
            If bR.BaseStream.Position >= bR.BaseStream.Length - 1 Then Exit Do
            bTest = bR.ReadByte
            If (bTest = 0) Or (bTest = &HFF) Then Exit Do
            bR.BaseStream.Position = lLast
          Loop
          If lLast < bR.BaseStream.Length - 1 Then
            lID3Len = lLast
          Else
            lID3Len = (ID3Header.Size + IIf((ID3Header.Flags And HFLG.FOOTER) = HFLG.FOOTER, &H15, &HB))
            Return
          End If
        Else
          lID3Len = 1
        End If
        bR.Close()
      End Using
    Catch ex As Exception
      MsgBox("Error Reading " & IO.Path.GetFileName(sFile) & vbNewLine & ex.ToString, MsgBoxStyle.Exclamation, My.Application.Info.Title & " ID3v2")
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
    ID3Frames = New List(Of ID3v2Frame)
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

  Public Shared Function TryToDecode(bIn() As Byte, ByRef sOut As String) As Boolean
    For Each iEncoding As TextEncoding In {TextEncoding.NT_ISO, TextEncoding.UTF_8, TextEncoding.UTF_16_LE, TextEncoding.UTF_16_BE}
      Dim sDec As String = ParseString(iEncoding, bIn, 0, False)
      Dim isANPWS As Boolean = True
      If String.IsNullOrEmpty(sDec) Then
        isANPWS = False
      ElseIf sDec.Contains("[NULL]") Then
        isANPWS = False
      Else
        If iEncoding = TextEncoding.NT_ISO Or iEncoding = TextEncoding.UTF_8 Then
          If sDec.Length < bIn.Length - 3 Then isANPWS = False
        ElseIf iEncoding = TextEncoding.UTF_16_LE Or iEncoding = TextEncoding.UTF_16_BE Then
          If sDec.Length < Math.Floor((bIn.Length - 3) / 2) Then isANPWS = False
        End If
        For C As Integer = 0 To sDec.Length - 1
          If Not IsSimpleChar(sDec, C) Then
            isANPWS = False
            Exit For
          End If
        Next
      End If
      If isANPWS Then
        sOut = sDec
        Return True
      End If
    Next
    Return False
  End Function

  Public MustInherit Class ParseResponse
    Private m_Name As String
    Public MustOverride ReadOnly Property StringData As String()
    Public ReadOnly Property [Name] As String
      Get
        Return m_Name
      End Get
    End Property
    Public Sub New(sName As String)
      m_Name = sName
    End Sub
    Public Shared Function StringIn(sIn As String) As String
      If String.IsNullOrEmpty(sIn) Then Return Nothing
      Return sIn.Replace(vbNewLine, vbCr)
    End Function
    Public Shared Function StringOut(sOut As String) As String
      If String.IsNullOrEmpty(sOut) Then Return Nothing
      Return sOut.Replace(vbCr, vbNewLine)
    End Function
    Public MustOverride Function ToByteArray(Version As Byte()) As Byte()
    Public Shared Function FromByteArray(sName As String, bData As Byte()) As ParseResponse
      Select Case sName
        Case "TXX", "TXXX" : Return Parsed_TXXX.FromByteArray(sName, bData)
        Case "WXX", "WXXX" : Return Parsed_WXXX.FromByteArray(sName, bData)
        Case "UFI", "UFID" : Return Parsed_UFID.FromByteArray(sName, bData)
        Case "PRIV" : Return Parsed_PRIV.FromByteArray(sName, bData)
        Case "TCO", "TCON" : Return Parsed_TCON.FromByteArray(sName, bData)
        Case "MCI", "MCDI" : Return Parsed_MCDI.FromByteArray(sName, bData)
        Case "COM", "COMM" : Return Parsed_COMM.FromByteArray(sName, bData)
        Case "PIC", "APIC" : Return Parsed_APIC.FromByteArray(sName, bData)
        Case "ULT", "USLT" : Return Parsed_USLT.FromByteArray(sName, bData)
        Case "GEO", "GEOB" : Return Parsed_GEOB.FromByteArray(sName, bData)
        Case "CNT", "PCNT" : Return Parsed_PCNT.FromByteArray(sName, bData)
        Case "POP", "POPM" : Return Parsed_POPM.FromByteArray(sName, bData)
      End Select
      If sName.StartsWith("T") Then Return Parsed_TZZZ.FromByteArray(sName, bData)
      If sName.StartsWith("W") Then Return Parsed_WZZZ.FromByteArray(sName, bData)
      Return New Parse_Failure(sName, "Unable to handle the " & sName & " Frame type!")
    End Function
  End Class

  Public Class Parsed_UFID
    Inherits ParseResponse
    Private m_Owner As String
    Private m_Ident As Byte()
    Public Property Owner As String
      Get
        Return StringOut(m_Owner)
      End Get
      Set(value As String)
        m_Owner = StringIn(value)
      End Set
    End Property
    Public ReadOnly Property IdentString As String
      Get
        Dim sDec As String = Nothing
        If TryToDecode(m_Ident, sDec) Then Return sDec
        Return BitConverter.ToString(m_Ident)
      End Get
    End Property
    Public Property Ident As Byte()
      Get
        Return m_Ident
      End Get
      Set(value As Byte())
        m_Ident = value
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {Owner, IdentString}
      End Get
    End Property
    Public Sub New(sName As String, sOwner As String, bIdent As Byte())
      MyBase.New(sName)
      m_Owner = sOwner
      m_Ident = bIdent
    End Sub
    Public Overrides Function ToByteArray(Version As Byte()) As Byte()
      Dim bData As New List(Of Byte)
      bData.AddRange(MakeFrameStringData(m_Owner, True))
      bData.AddRange(m_Ident)
      Return bData.ToArray
    End Function
    Public Overloads Shared Function FromByteArray(sName As String, bData As Byte()) As Parsed_UFID
      Dim bBlobs As Byte()() = SplitByteArrays(bData, 0, {0}, False, 2)
      If bBlobs.Length = 2 Then
        Dim sOwner As String = ParseString(TextEncoding.NT_ISO, bBlobs(0), 0, False)
        Return New Parsed_UFID(sName, sOwner, bBlobs(1))
      Else
        Return New Parsed_UFID(sName, Nothing, bData)
      End If
    End Function
  End Class
  Public Class Parsed_PRIV
    Inherits ParseResponse
    Private m_Owner As String
    Private m_Data As Byte()
    Public Property Owner As String
      Get
        Return StringOut(m_Owner)
      End Get
      Set(value As String)
        m_Owner = StringIn(value)
      End Set
    End Property
    Private Function GetDWORD(bIn As Byte(), Optional ByVal lStart As Long = 0) As UInt32
      Dim bTmp(3) As Byte
      If lStart + 3 >= bIn.Length Then
        bTmp(0) = 0
      Else
        bTmp(0) = bIn(lStart + 3)
      End If
      If lStart + 2 >= bIn.Length Then
        bTmp(1) = 0
      Else
        bTmp(1) = bIn(lStart + 2)
      End If
      If lStart + 1 >= bIn.Length Then
        bTmp(2) = 0
      Else
        bTmp(2) = bIn(lStart + 1)
      End If
      If lStart >= bIn.Length Then
        bTmp(3) = 0
      Else
        bTmp(3) = bIn(lStart)
      End If
      Return BitConverter.ToUInt32(bTmp, 0)
    End Function
    Public ReadOnly Property DataString As String
      Get
        Dim sDec As String = Nothing
        If m_Data.Length = 16 Then
          Return "{" & New Guid(m_Data).ToString & "}"
        ElseIf m_Data.Length = 4 Then
          If m_Data(2) = 0 And m_Data(3) = 0 Then
            Dim uiVal As UInt16 = Val("&H" & m_Data(1).ToString("x2") & m_Data(0).ToString("x2"))
            If uiVal <= Int16.MaxValue Then
              Return FormatPercent(uiVal / Int16.MaxValue, 2, TriState.True, TriState.False, TriState.True)
            Else
              Return FormatPercent(uiVal / UInt16.MaxValue, 2, TriState.True, TriState.False, TriState.True) & " (" & BitConverter.ToString(m_Data) & ")"
            End If
          Else
            Return GetDWORD(m_Data)
          End If
        End If
        If TryToDecode(m_Data, sDec) Then Return sDec
        Return BitConverter.ToString(m_Data)
      End Get
    End Property
    Public Property Data As Byte()
      Get
        Return m_Data
      End Get
      Set(value As Byte())
        m_Data = value
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {Owner, DataString}
      End Get
    End Property
    Public Sub New(sName As String, sOwner As String, bData As Byte())
      MyBase.New(sName)
      m_Owner = sOwner
      m_Data = bData
    End Sub
    Public Overrides Function ToByteArray(Version As Byte()) As Byte()
      Dim bData As New List(Of Byte)
      bData.AddRange(MakeFrameStringData(m_Owner, True))
      bData.AddRange(m_Data)
      Return bData.ToArray
    End Function
    Public Overloads Shared Function FromByteArray(sName As String, bData As Byte()) As Parsed_PRIV
      Dim bBlobs As Byte()() = SplitByteArrays(bData, 0, {0}, False, 2)
      If bBlobs.Length = 2 Then
        Dim sOwner As String = ParseString(TextEncoding.NT_ISO, bBlobs(0), 0, False)
        Return New Parsed_PRIV(sName, sOwner, bBlobs(1))
      Else
        Return New Parsed_PRIV(sName, Nothing, bData)
      End If
    End Function
  End Class
  Public Class Parsed_TXXX
    Inherits ParseResponse
    Private m_Encoding As TextEncoding
    Private m_Description As String
    Private m_Value As String
    Public Property Encoding As TextEncoding
      Get
        Return m_Encoding
      End Get
      Set(value As TextEncoding)
        m_Encoding = value
      End Set
    End Property
    Public Property Description As String
      Get
        Return StringOut(m_Description)
      End Get
      Set(value As String)
        m_Description = StringIn(value)
      End Set
    End Property
    Public Property Value As String
      Get
        Return StringOut(m_Value)
      End Get
      Set(value As String)
        m_Value = StringIn(value)
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {Description, Value}
      End Get
    End Property
    Public Sub New(sName As String, bEncoding As TextEncoding, sValue As String)
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_Description = Nothing
      m_Value = sValue
    End Sub
    Public Sub New(sName As String, bEncoding As TextEncoding, sDescription As String, sValue As String)
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_Description = sDescription
      m_Value = sValue
    End Sub
    Public Overrides Function ToByteArray(Version As Byte()) As Byte()
      Dim bData As New List(Of Byte)
      bData.AddRange(MakeFrameStringData(m_Encoding, m_Description, Version))
      bData.AddRange(MakeFrameStringData(m_Encoding, m_Value, Version, True))
      Return bData.ToArray
    End Function
    Public Overloads Shared Function FromByteArray(sName As String, bData As Byte()) As Parsed_TXXX
      Dim bEncoding As TextEncoding = bData(0)
      Dim bParts As Byte()() = SplitByteArrayStrings(bData, 1, bEncoding, True)
      Dim sDesc As String = ParseString(bEncoding, bParts(0), 0, False)
      Dim sValue As String = ParseString(bEncoding, bParts(1), 0, True)
      If String.IsNullOrEmpty(sDesc) Or sDesc = ChrW(&HFEFF) Then Return New Parsed_TXXX(sName, bEncoding, sValue)
      Return New Parsed_TXXX(sName, bEncoding, sDesc, sValue)
    End Function
  End Class
  Public Class Parsed_WXXX
    Inherits ParseResponse
    Private m_Encoding As TextEncoding
    Private m_Description As String
    Private m_URL As String
    Public Property Encoding As TextEncoding
      Get
        Return m_Encoding
      End Get
      Set(value As TextEncoding)
        m_Encoding = value
      End Set
    End Property
    Public Property Description As String
      Get
        Return StringOut(m_Description)
      End Get
      Set(value As String)
        m_Description = StringIn(value)
      End Set
    End Property
    Public Property URL As String
      Get
        Return StringOut(m_URL)
      End Get
      Set(value As String)
        m_URL = StringIn(value)
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {Description, URL}
      End Get
    End Property
    Public Sub New(sName As String, bEncoding As TextEncoding, sURL As String)
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_Description = Nothing
      m_URL = sURL
    End Sub
    Public Sub New(sName As String, bEncoding As TextEncoding, sDescription As String, sURL As String)
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_Description = sDescription
      m_URL = sURL
    End Sub
    Public Overrides Function ToByteArray(Version As Byte()) As Byte()
      Dim bData As New List(Of Byte)
      bData.AddRange(MakeFrameStringData(m_Encoding, m_Description, Version))
      bData.AddRange(MakeFrameStringData(m_URL, False))
      Return bData.ToArray
    End Function
    Public Overloads Shared Function FromByteArray(sName As String, bData As Byte()) As Parsed_WXXX
      If bData.Length < 5 Then Return Nothing
      Dim bEncoding As TextEncoding = bData(0)
      Dim bBlobs As Byte()() = SplitByteArrayStrings(bData, 1, bEncoding, True)
      If bBlobs.Length = 1 Then
        Dim sDec As String = ParseString(bEncoding, bBlobs(0), 0, True)
        Return New Parsed_WXXX(sName, bEncoding, sDec)
      ElseIf bBlobs.Length = 2 Then
        Dim sDesc As String = ParseString(bEncoding, bBlobs(0), 0, True)
        Dim sComm As String = ParseString(TextEncoding.NT_ISO, bBlobs(1), 0, False)
        Return New Parsed_WXXX(sName, bEncoding, sDesc, sComm)
      Else
        Return Nothing
      End If
    End Function
  End Class
  Public Class Parsed_TZZZ
    Inherits ParseResponse
    Private m_Encoding As TextEncoding
    Private m_Value As String
    Public Property Encoding As TextEncoding
      Get
        Return m_Encoding
      End Get
      Set(value As TextEncoding)
        m_Encoding = value
      End Set
    End Property
    Public Property Value As String
      Get
        Return StringOut(m_Value)
      End Get
      Set(value As String)
        m_Value = StringIn(value)
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {Value}
      End Get
    End Property
    Public Sub New(sName As String, bEncoding As TextEncoding, sValue As String)
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_Value = sValue
    End Sub
    Public Overrides Function ToByteArray(Version() As Byte) As Byte()
      Dim bData As New List(Of Byte)
      bData.AddRange(MakeFrameStringData(m_Encoding, m_Value, Version))
      Return bData.ToArray
    End Function
    Public Shared Shadows Function FromByteArray(sName As String, bData As Byte()) As Parsed_TZZZ
      If bData(0) < 4 Then
        Dim bEncoding As TextEncoding = bData(0)
        Dim sBlob As String = ParseString(bEncoding, bData, 1, True)
        Return New Parsed_TZZZ(sName, bEncoding, sBlob)
      Else
        Dim bEncoding As TextEncoding = TextEncoding.NT_ISO
        Dim sText As String = Nothing
        If bData.Length > 1 Then
          If bData(0) = &HFF And bData(1) = &HFE Then
            bEncoding = TextEncoding.UTF_16_LE
            If bData.Length = 3 AndAlso bData(2) = 0 Then
              sText = Nothing
            Else
              sText = ParseString(bEncoding, bData, 2, False)
            End If
          ElseIf bData(0) = &HFE And bData(1) = &HFF Then
            bEncoding = TextEncoding.UTF_16_BE
            If bData.Length = 3 AndAlso bData(2) = 0 Then
              sText = Nothing
            Else
              sText = ParseString(bEncoding, bData, 2, False)
            End If
          Else
            sText = ParseString(bEncoding, bData, 0, False)
          End If
        Else
          sText = ParseString(bEncoding, bData, 0, False)
        End If
        Return New Parsed_TZZZ(sName, bEncoding, sText)
      End If
    End Function
  End Class
  Public Class Parsed_WZZZ
    Inherits ParseResponse
    Private m_Encoding As TextEncoding
    Private m_URL As String
    Public Property Encoding As TextEncoding
      Get
        Return m_Encoding
      End Get
      Set(value As TextEncoding)
        m_Encoding = value
      End Set
    End Property
    Public Property URL As String
      Get
        Return StringOut(m_URL)
      End Get
      Set(value As String)
        m_URL = StringIn(value)
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {URL}
      End Get
    End Property
    Public Sub New(sName As String, bEncoding As TextEncoding, sURL As String)
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_URL = sURL
    End Sub
    Public Overrides Function ToByteArray(Version() As Byte) As Byte()
      Dim bData As New List(Of Byte)
      bData.AddRange(MakeFrameStringData(m_Encoding, m_URL, Version))
      Return bData.ToArray
    End Function
    Public Shared Shadows Function FromByteArray(sName As String, bData As Byte()) As Parsed_WZZZ
      If bData(0) < 4 Then
        Dim bEncoding As TextEncoding = bData(0)
        Dim sBlob As String = ParseString(bEncoding, bData, 1, True)
        Return New Parsed_WZZZ(sName, bEncoding, sBlob)
      Else
        Dim bEncoding As TextEncoding = TextEncoding.NT_ISO
        Dim sText As String = Nothing
        If bData.Length > 1 Then
          If bData(0) = &HFF And bData(1) = &HFE Then
            bEncoding = TextEncoding.UTF_16_LE
            If bData.Length = 3 AndAlso bData(2) = 0 Then
              sText = Nothing
            Else
              sText = ParseString(bEncoding, bData, 2, False)
            End If
          ElseIf bData(0) = &HFE And bData(1) = &HFF Then
            bEncoding = TextEncoding.UTF_16_BE
            If bData.Length = 3 AndAlso bData(2) = 0 Then
              sText = Nothing
            Else
              sText = ParseString(bEncoding, bData, 2, False)
            End If
          Else
            sText = ParseString(bEncoding, bData, 0, False)
          End If
        Else
          sText = ParseString(bEncoding, bData, 0, False)
        End If
        Return New Parsed_WZZZ(sName, bEncoding, sText)
      End If
    End Function
  End Class
  Public Class Parsed_TCON
    Inherits ParseResponse
    Private m_Encoding As TextEncoding
    Private m_GenreList As String()
    Public Property Encoding As TextEncoding
      Get
        Return m_Encoding
      End Get
      Set(value As TextEncoding)
        m_Encoding = value
      End Set
    End Property
    Public Property GenreList As String()
      Get
        Return m_GenreList
      End Get
      Set(value As String())
        m_GenreList = value
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return GenreList
      End Get
    End Property
    Public Sub New(sName As String, bEncoding As TextEncoding, sGenre As String)
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_GenreList = {sGenre}
    End Sub
    Public Sub New(sName As String, bEncoding As TextEncoding, sGenreList As String())
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_GenreList = sGenreList
    End Sub
    Public Overrides Function ToByteArray(Version() As Byte) As Byte()
      Dim sGenres As String = Nothing
      For Each gEntity As String In m_GenreList
        Dim gID As String = gEntity
        For g As Integer = &H0 To &H93
          If StrComp(gEntity, clsID3v1.GenreName(g), CompareMethod.Text) = 0 Then
            gID = g
            Exit For
          End If
        Next
        If Not Version(0) = 4 Then
          If IsNumeric(gID) Then gID = "(" & gID & ")"
          If String.IsNullOrEmpty(sGenres) Then
            sGenres = gID
          ElseIf sGenres.EndsWith(")") AndAlso gID.StartsWith("(") Then
            sGenres &= gID
          Else
            sGenres &= "; " & gID
          End If
        Else
          If String.IsNullOrEmpty(sGenres) Then
            sGenres = gID
          Else
            sGenres &= vbNullChar & gID
          End If
        End If
      Next
      Return MakeFrameStringData(m_Encoding, sGenres, Version)
    End Function
    Public Shared Shadows Function FromByteArray(sName As String, bData As Byte()) As Parsed_TCON
      Dim bEncoding As TextEncoding = TextEncoding.NT_ISO
      Dim sData As String() = ParseGenreFrame(bData, bEncoding)
      If sData Is Nothing Then Return Nothing
      If sData.Length = 1 Then Return New Parsed_TCON(sName, bEncoding, sData(0))
      Return New Parsed_TCON(sName, bEncoding, sData)
    End Function
    Public Shared Shadows Function FromByteArrayEx(sName As String, bData As Byte()) As Parsed_TCON()
      Dim bEncoding As TextEncoding = TextEncoding.NT_ISO
      Dim sData As String() = ParseGenreFrame(bData, bEncoding)
      If sData Is Nothing Then Return Nothing
      If sData.Length = 1 Then Return {New Parsed_TCON(sName, bEncoding, sData(0))}
      Dim tParses As New List(Of Parsed_TCON)
      For I As Integer = 0 To sData.Length - 1
        tParses.Add(New Parsed_TCON(sName, bEncoding, sData(I)))
      Next
      Return tParses.ToArray
    End Function
  End Class
  Public Class Parsed_MCDI
    Inherits ParseResponse
    Private m_TOC As Byte()
    Public ReadOnly Property TOCString As String
      Get
        Dim sDec As String = Nothing
        If TryToDecode(m_TOC, sDec) Then Return sDec
        Return BitConverter.ToString(m_TOC)
      End Get
    End Property
    Public Property TOC As Byte()
      Get
        Return m_TOC
      End Get
      Set(value As Byte())
        m_TOC = value
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {BitConverter.ToString(TOC)}
      End Get
    End Property
    Public Sub New(sName As String, bTOC As Byte())
      MyBase.new(sName)
      m_TOC = bTOC
    End Sub
    Public Overrides Function ToByteArray(Version() As Byte) As Byte()
      Return m_TOC
    End Function
    Public Shared Shadows Function FromByteArray(sName As String, bData As Byte()) As Parsed_MCDI
      Return New Parsed_MCDI(sName, bData)
    End Function
  End Class
  Public Class Parsed_COMM
    Inherits ParseResponse
    Private m_Encoding As TextEncoding
    Private m_Language As String
    Private m_Description As String
    Private m_Comment As String
    Public Property Encoding As TextEncoding
      Get
        Return m_Encoding
      End Get
      Set(value As TextEncoding)
        m_Encoding = value
      End Set
    End Property
    Public Property Language As String
      Get
        Return m_Language
      End Get
      Set(value As String)
        If Not String.IsNullOrEmpty(value) AndAlso value.Length > 3 Then value = value.Substring(0, 3)
        m_Language = value
      End Set
    End Property
    Public Property Description As String
      Get
        Return StringOut(m_Description)
      End Get
      Set(value As String)
        m_Description = StringIn(value)
      End Set
    End Property
    Public Property Comment As String
      Get
        Return StringOut(m_Comment)
      End Get
      Set(value As String)
        m_Comment = StringIn(value)
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {Language, Description, Comment}
      End Get
    End Property
    Public Sub New(sName As String, bEncoding As TextEncoding, sLanguage As String, sComment As String)
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_Language = sLanguage
      m_Description = Nothing
      m_Comment = sComment
    End Sub
    Public Sub New(sName As String, bEncoding As TextEncoding, sLanguage As String, sDescription As String, sComment As String)
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_Language = sLanguage
      m_Description = sDescription
      m_Comment = sComment
    End Sub
    Public Overrides Function ToByteArray(Version() As Byte) As Byte()
      Dim bData As New List(Of Byte)
      bData.Add(m_Encoding)
      If String.IsNullOrEmpty(m_Language) Then
        bData.AddRange(MakeFrameStringData("eng", False))
      Else
        bData.AddRange(MakeFrameStringData(m_Language, False))
      End If
      bData.AddRange(MakeFrameStringData(m_Encoding, m_Description, Version, True))
      bData.AddRange(MakeFrameStringData(m_Encoding, m_Comment, Version, True))
      Return bData.ToArray
    End Function
    Public Shared Shadows Function FromByteArray(sName As String, bData As Byte()) As Parsed_COMM
      If bData.Length < 5 Then Return Nothing
      Dim bEncoding As TextEncoding = bData(0)
      Dim sLang As String = fileEncoding.GetString(bData, 1, 3)
      Dim bBlobs As Byte()() = SplitByteArrayStrings(bData, 4, bEncoding, True)
      If bBlobs.Length = 1 Then
        Dim sDec As String = ParseString(bEncoding, bBlobs(0), 0, True)
        Return New Parsed_COMM(sName, bEncoding, sLang, sDec)
      ElseIf bBlobs.Length = 2 Then
        Dim sDesc As String = ParseString(bEncoding, bBlobs(0), 0, False)
        Dim sComm As String = ParseString(bEncoding, bBlobs(1), 0, True)
        Return New Parsed_COMM(sName, bEncoding, sLang, sDesc, sComm)
      Else
        Return Nothing
      End If
    End Function
  End Class
  Public Class Parsed_USLT
    Inherits ParseResponse
    Private m_Encoding As TextEncoding
    Private m_Language As String
    Private m_Description As String
    Private m_Lyrics As String
    Public Property Encoding As TextEncoding
      Get
        Return m_Encoding
      End Get
      Set(value As TextEncoding)
        m_Encoding = value
      End Set
    End Property
    Public Property Language As String
      Get
        Return m_Language
      End Get
      Set(value As String)
        If Not String.IsNullOrEmpty(value) AndAlso value.Length > 3 Then value = value.Substring(0, 3)
        m_Language = value
      End Set
    End Property
    Public Property Description As String
      Get
        Return StringOut(m_Description)
      End Get
      Set(value As String)
        m_Description = StringIn(value)
      End Set
    End Property
    Public Property Lyrics As String
      Get
        Return StringOut(m_Lyrics)
      End Get
      Set(value As String)
        m_Lyrics = StringIn(value)
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {Language, Description, Lyrics}
      End Get
    End Property
    Public Sub New(sName As String, bEncoding As TextEncoding, sLanguage As String, sLyrics As String)
      MyBase.New(sName)
      If String.IsNullOrEmpty(sLyrics) Then Stop
      m_Encoding = bEncoding
      m_Language = sLanguage
      m_Description = Nothing
      If Not String.IsNullOrEmpty(sLyrics) AndAlso sLyrics.Contains("[LF]") Then sLyrics = sLyrics.Replace("[LF]", "")
      m_Lyrics = sLyrics
    End Sub
    Public Sub New(sName As String, bEncoding As TextEncoding, sLanguage As String, sDescription As String, sLyrics As String)
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_Language = sLanguage
      m_Description = sDescription
      If Not String.IsNullOrEmpty(sLyrics) AndAlso sLyrics.Contains("[LF]") Then sLyrics = sLyrics.Replace("[LF]", "")
      m_Lyrics = sLyrics
    End Sub
    Public Overrides Function ToByteArray(Version() As Byte) As Byte()
      Dim bData As New List(Of Byte)
      bData.Add(m_Encoding)
      bData.AddRange(MakeFrameStringData(m_Language, False))
      bData.AddRange(MakeFrameStringData(m_Encoding, m_Description, Version, True))
      bData.AddRange(MakeFrameStringData(m_Encoding, m_Lyrics, Version, True))
      Return bData.ToArray
    End Function
    Public Shared Shadows Function FromByteArray(sName As String, bData As Byte()) As Parsed_USLT
      If bData.Length < 5 Then Return Nothing
      Dim bEncoding As TextEncoding = bData(0)
      Dim sLang As String = fileEncoding.GetString(bData, 1, 3)
      Dim bBlobs As Byte()() = SplitByteArrayStrings(bData, 4, bEncoding, True)
      If bBlobs.Length = 1 Then
        Dim sDec As String = ParseString(bEncoding, bBlobs(0), 0, True)
        Return New Parsed_USLT(sName, bEncoding, sLang, sDec)
      ElseIf bBlobs.Length = 2 Then
        Dim sDesc As String = ParseString(bEncoding, bBlobs(0), 0, False)
        Dim sLyr As String = ParseString(bEncoding, bBlobs(1), 0, True)
        Return New Parsed_USLT(sName, bEncoding, sLang, sDesc, sLyr)
      Else
        Return Nothing
      End If
    End Function
  End Class
  Public Class Parsed_APIC
    Inherits ParseResponse
    Private m_Encoding As TextEncoding
    Private m_MIME As ID3_PIC_MIME
    Private m_Description As String
    Private m_Type As ID3_PIC_TYPE
    Private m_Image As Byte()
    Public Property Encoding As TextEncoding
      Get
        Return m_Encoding
      End Get
      Set(value As TextEncoding)
        m_Encoding = value
      End Set
    End Property
    Public Property MIME As ID3_PIC_MIME
      Get
        Return m_MIME
      End Get
      Set(value As ID3_PIC_MIME)
        m_MIME = value
      End Set
    End Property
    Public Property Description As String
      Get
        Return StringOut(m_Description)
      End Get
      Set(value As String)
        m_Description = StringIn(value)
      End Set
    End Property
    Public Property [Type] As ID3_PIC_TYPE
      Get
        Return m_Type
      End Get
      Set(value As ID3_PIC_TYPE)
        m_Type = value
      End Set
    End Property
    Public Property Image As Byte()
      Get
        Return m_Image
      End Get
      Set(value As Byte())
        m_Image = CleanImage(value)
      End Set
    End Property
    Public ReadOnly Property Picture As Drawing.Image
      Get
        Try
          Using sData As New IO.MemoryStream(m_Image)
            Return Drawing.Image.FromStream(sData)
          End Using
        Catch ex As Exception
          Return Nothing
        End Try
      End Get
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {MIME.ToString, Description, Type.ToString}
      End Get
    End Property
    Public Sub New(sName As String, bEncoding As TextEncoding, bMIME As ID3_PIC_MIME, sDescription As String, bType As ID3_PIC_TYPE, bImage As Byte())
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_MIME = bMIME
      m_Description = sDescription
      m_Type = bType
      m_Image = CleanImage(bImage)
    End Sub
    Private Function CleanImage(bIn As Byte()) As Byte()
      If bIn Is Nothing OrElse bIn.Length = 0 Then Return Nothing
      Dim startFrom As Integer = 0
      If bIn.Length > 0 Then
        For I As Integer = 0 To bIn.Length - 1
          If Not bIn(I) = 0 Then
            startFrom = I
            Exit For
          End If
        Next
      End If
      If startFrom = 0 Then
        Return bIn
      Else
        If startFrom > 1 Then Debug.Print("Cutting off " & startFrom & " null bytes!")
        Dim iRemaining As Integer = bIn.Length - startFrom
        Dim bImage(iRemaining - 1) As Byte
        Array.ConstrainedCopy(bIn, startFrom, bImage, 0, iRemaining)
        Return bImage
      End If
    End Function
    Public Overrides Function ToByteArray(Version As Byte()) As Byte()
      If Version(0) = 2 Then
        Dim bData As New List(Of Byte)
        bData.Add(m_Encoding)
        bData.AddRange(MakeFrameStringData(MIMEtoString(m_MIME, True), False))
        bData.Add(m_Type)
        bData.AddRange(MakeFrameStringData(m_Encoding, m_Description, Version, True))
        bData.AddRange(m_Image)
        Return bData.ToArray
      Else
        Dim bData As New List(Of Byte)
        bData.Add(m_Encoding)
        bData.AddRange(MakeFrameStringData(MIMEtoString(m_MIME, False), False))
        bData.Add(0)
        bData.Add(m_Type)
        bData.AddRange(MakeFrameStringData(m_Encoding, m_Description, Version, True))
        bData.AddRange(m_Image)
        Return bData.ToArray
      End If
    End Function
    Public Overloads Shared Function FromByteArray(sName As String, bData As Byte()) As Parsed_APIC
      If sName = "PIC" Then
        If bData.Length < 6 Then Return Nothing
        Dim bEnc As TextEncoding = bData(0)
        Dim sFormat As String = fileEncoding.GetString(bData, 1, 3)
        Dim bPicMIME As ID3_PIC_MIME = StrToMIME(sFormat, True)
        Dim bPicType As ID3_PIC_TYPE = bData(4)
        If bData(4) > ID3_PIC_TYPE.STUDIO_LOGO Then bPicType = ID3_PIC_TYPE.INVALID
        Dim bBlobs As Byte()() = SplitByteArrayStrings(bData, 5, bEnc, True)
        If bBlobs.Length = 1 Then
          Return New Parsed_APIC(sName, bEnc, bPicMIME, Nothing, bPicType, bBlobs(0))
        ElseIf bBlobs.Length = 2 Then
          Dim sDesc As String = ParseString(bEnc, bBlobs(0), 0, True)
          Return New Parsed_APIC(sName, bEnc, bPicMIME, sDesc, bPicType, bBlobs(1))
        Else
          Return Nothing
        End If
      Else
        If bData.Length < 3 Then Return Nothing
        Dim bEnc As TextEncoding = bData(0)
        Dim bBlobs As Byte()() = SplitByteArrayStrings(bData, 1, TextEncoding.NT_ISO, False)
        If Not bBlobs.Length = 2 Then Return Nothing
        Dim bPicMIME As ID3_PIC_MIME = StrToMIME(fileEncoding.GetString(bBlobs(0)), False)
        If bBlobs(1).Length = 0 Then Return Nothing
        Dim bPicType As ID3_PIC_TYPE = bBlobs(1)(0)
        If bBlobs(1)(0) > ID3_PIC_TYPE.STUDIO_LOGO Then bPicType = ID3_PIC_TYPE.INVALID
        Dim bBlobs2 As Byte()() = SplitByteArrayStrings(bBlobs(1), 1, bEnc, True)
        If bBlobs2.Length = 1 Then
          Return New Parsed_APIC(sName, bEnc, bPicMIME, Nothing, bPicType, bBlobs2(0))
        End If
        Dim sDesc As String = ParseString(bEnc, bBlobs2(0), 0, True)
        If Not String.IsNullOrEmpty(sDesc) Then
          For I As Integer = 0 To sDesc.Length - 1
            If IsSimpleChar(sDesc, I) Then Continue For
            sDesc = Nothing
            Exit For
          Next
        End If
        Return New Parsed_APIC(sName, bEnc, bPicMIME, sDesc, bPicType, bBlobs2(1))
      End If
    End Function
  End Class
  Public Class Parsed_GEOB
    Inherits ParseResponse
    Private m_Encoding As TextEncoding
    Private m_MIME As String
    Private m_Filename As String
    Private m_Description As String
    Private m_Content As Byte()
    Public Property Encoding As TextEncoding
      Get
        Return m_Encoding
      End Get
      Set(value As TextEncoding)
        m_Encoding = value
      End Set
    End Property
    Public Property MIME As String
      Get
        Return StringOut(m_MIME)
      End Get
      Set(value As String)
        m_MIME = StringIn(value)
      End Set
    End Property
    Public Property Filename As String
      Get
        Return StringOut(m_Filename)
      End Get
      Set(value As String)
        m_Filename = StringIn(value)
      End Set
    End Property
    Public Property Description As String
      Get
        Return StringOut(m_Description)
      End Get
      Set(value As String)
        m_Description = StringIn(value)
      End Set
    End Property
    Public ReadOnly Property ContentString As String
      Get
        Dim sDec As String = Nothing
        If m_Content.Length = 16 Then
          Try
            Return "{" & New Guid(m_Content).ToString.ToUpper & "}"
          Catch ex As Exception
          End Try
        End If
        If TryToDecode(m_Content, sDec) Then Return sDec
        Return BitConverter.ToString(m_Content)
      End Get
    End Property
    Public Property Content As Byte()
      Get
        Return m_Content
      End Get
      Set(value As Byte())
        m_Content = value
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {MIME, Filename, Description, ContentString}
      End Get
    End Property
    Public Sub New(sName As String, bEncoding As TextEncoding, sMIME As String, sFilename As String, sDescription As String, bContent As Byte())
      MyBase.New(sName)
      m_Encoding = bEncoding
      m_MIME = sMIME
      m_Filename = sFilename
      m_Description = sDescription
      m_Content = bContent
    End Sub
    Public Overrides Function ToByteArray(Version() As Byte) As Byte()
      Dim bData As New List(Of Byte)
      bData.Add(m_Encoding)
      bData.AddRange(MakeFrameStringData(m_MIME, True))
      bData.AddRange(MakeFrameStringData(m_Encoding, m_Filename, Version, True))
      bData.AddRange(MakeFrameStringData(m_Encoding, m_Description, Version, True))
      bData.AddRange(m_Content)
      Return bData.ToArray
    End Function
    Public Shared Shadows Function FromByteArray(sName As String, bData As Byte()) As Parsed_GEOB
      Dim bEncoding As TextEncoding = bData(0)
      Dim bBlob1 As Byte()() = SplitByteArrayStrings(bData, 1, TextEncoding.NT_ISO, False)
      If Not bBlob1.Length = 2 Then Return Nothing
      Dim sMIME As String = fileEncoding.GetString(bBlob1(0))
      Dim bBlob2 As Byte()() = SplitByteArrayStrings(bBlob1(1), 0, bEncoding, True)
      If Not bBlob2.Length = 2 Then Return Nothing
      Dim sFile As String = ParseString(bEncoding, bBlob2(0), 0, False)
      Dim bBlob3 As Byte()() = SplitByteArrayStrings(bBlob2(1), 0, bEncoding, True)
      If Not bBlob3.Length = 2 Then Return New Parsed_GEOB(sName, bEncoding, sMIME, sFile, Nothing, bBlob2(1))
      Dim sDesc As String = ParseString(bEncoding, bBlob3(0), 0, True)
      Return New Parsed_GEOB(sName, bEncoding, sMIME, sFile, sDesc, bBlob3(1))
    End Function
  End Class
  Public Class Parsed_PCNT
    Inherits ParseResponse
    Private m_Counter As UInt64
    Public Property Counter As UInt64
      Get
        Return m_Counter
      End Get
      Set(value As UInt64)
        m_Counter = value
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {Counter.ToString}
      End Get
    End Property
    Public Sub New(sName As String, iCount As UInt64)
      MyBase.New(sName)
      m_Counter = iCount
    End Sub
    Public Overrides Function ToByteArray(Version As Byte()) As Byte()
      Dim bData As New List(Of Byte)
      If m_Counter <= &HFFUL Then
        bData.Add(0)
        bData.Add(0)
        bData.Add(0)
        bData.Add(m_Counter)
      ElseIf m_Counter <= &HFFFFUL Then
        bData.Add(0)
        bData.Add(0)
        bData.Add((m_Counter And &HFF00UL) >> 8)
        bData.Add((m_Counter And &HFFUL))
      ElseIf m_Counter <= &HFFFFFFUL Then
        bData.Add(0)
        bData.Add((m_Counter And &HFF0000UL) >> 16)
        bData.Add((m_Counter And &HFF00UL) >> 8)
        bData.Add((m_Counter And &HFFUL))
      ElseIf m_Counter <= &HFFFFFFFFUL Then
        bData.Add((m_Counter And &HFF000000UL) >> 24)
        bData.Add((m_Counter And &HFF0000UL) >> 16)
        bData.Add((m_Counter And &HFF00UL) >> 8)
        bData.Add((m_Counter And &HFFUL))
      ElseIf m_Counter <= &HFFFFFFFFFFUL Then
        bData.Add((m_Counter And &HFF00000000UL) >> 32)
        bData.Add((m_Counter And &HFF000000UL) >> 24)
        bData.Add((m_Counter And &HFF0000UL) >> 16)
        bData.Add((m_Counter And &HFF00UL) >> 8)
        bData.Add((m_Counter And &HFFUL))
      ElseIf m_Counter <= &HFFFFFFFFFFFFUL Then
        bData.Add((m_Counter And &HFF0000000000UL) >> 40)
        bData.Add((m_Counter And &HFF00000000UL) >> 32)
        bData.Add((m_Counter And &HFF000000UL) >> 24)
        bData.Add((m_Counter And &HFF0000UL) >> 16)
        bData.Add((m_Counter And &HFF00UL) >> 8)
        bData.Add((m_Counter And &HFFUL))
      ElseIf m_Counter <= &HFFFFFFFFFFFFFFUL Then
        bData.Add((m_Counter And &HFF000000000000UL) >> 48)
        bData.Add((m_Counter And &HFF0000000000UL) >> 40)
        bData.Add((m_Counter And &HFF00000000UL) >> 32)
        bData.Add((m_Counter And &HFF000000UL) >> 24)
        bData.Add((m_Counter And &HFF0000UL) >> 16)
        bData.Add((m_Counter And &HFF00UL) >> 8)
        bData.Add((m_Counter And &HFFUL))
      Else
        bData.Add((m_Counter And &HFF00000000000000UL) >> 56)
        bData.Add((m_Counter And &HFF000000000000UL) >> 48)
        bData.Add((m_Counter And &HFF0000000000UL) >> 40)
        bData.Add((m_Counter And &HFF00000000UL) >> 32)
        bData.Add((m_Counter And &HFF000000UL) >> 24)
        bData.Add((m_Counter And &HFF0000UL) >> 16)
        bData.Add((m_Counter And &HFF00UL) >> 8)
        bData.Add((m_Counter And &HFFUL))
      End If
      Return bData.ToArray
    End Function
    Public Overloads Shared Function FromByteArray(sName As String, bData As Byte()) As Parsed_PCNT
      Dim uCount As UInt64 = 0
      Select Case bData.Length
        Case Is < 4
          Try
            Dim bEncoding As TextEncoding = bData(0)
            Dim sCount As String = ParseString(bEncoding, bData, 1, True)
            If IsNumeric(sCount) Then uCount = CULng(sCount)
          Catch ex As Exception
            MsgBox("Invalid Length reading " & sName & ": " & bData.Length & " is less than a 32-bit integer!")
          End Try
        Case 4 : uCount = bData(3) + (bData(2) << 8) + (bData(1) << 16) + (bData(0) << 24)
        Case 5 : uCount = bData(4) + (bData(3) << 8) + (bData(2) << 16) + (bData(1) << 24) + (bData(0) << 32)
        Case 6 : uCount = bData(5) + (bData(4) << 8) + (bData(3) << 16) + (bData(2) << 24) + (bData(1) << 32) + (bData(0) << 40)
        Case 7 : uCount = bData(6) + (bData(5) << 8) + (bData(4) << 16) + (bData(3) << 24) + (bData(2) << 32) + (bData(1) << 40) + (bData(0) << 48)
        Case 8 : uCount = bData(7) + (bData(6) << 8) + (bData(5) << 16) + (bData(4) << 24) + (bData(3) << 32) + (bData(2) << 40) + (bData(1) << 48) + (bData(0) << 56)
        Case Is > 8
          MsgBox("Invalid Length reading " & sName & ": " & bData.Length & " is greater than a 64-bit integer!")
        Case Else
          MsgBox("Invalid Length reading " & sName & ": " & bData.Length & " is not a normal number!?!")
      End Select
      Return New Parsed_PCNT(sName, uCount)
    End Function
  End Class
  Public Class Parsed_POPM
    Inherits ParseResponse
    Private m_Owner As String
    Private m_Rating As Byte
    Private m_Counter As UInt64
    Public Property Owner As String
      Get
        Return m_Owner
      End Get
      Set(value As String)
        m_Owner = value
      End Set
    End Property
    Public Property Rating As Byte
      Get
        Return m_Rating
      End Get
      Set(value As Byte)
        m_Rating = value
      End Set
    End Property
    Public Property Counter As UInt64
      Get
        Return m_Counter
      End Get
      Set(value As UInt64)
        m_Counter = value
      End Set
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {Owner, Rating.ToString, Counter.ToString}
      End Get
    End Property
    Public Sub New(sName As String, sOwner As String, bRating As Byte, iCount As UInt64)
      MyBase.New(sName)
      m_Owner = sOwner
      m_Rating = bRating
      m_Counter = iCount
    End Sub
    Public Overrides Function ToByteArray(Version As Byte()) As Byte()
      Dim bData As New List(Of Byte)
      bData.AddRange(MakeFrameStringData(m_Owner, True))
      bData.Add(m_Rating)
      If m_Counter > 0 Then
        If m_Counter <= &HFFUL Then
          bData.Add(0)
          bData.Add(0)
          bData.Add(0)
          bData.Add(m_Counter)
        ElseIf m_Counter <= &HFFFFUL Then
          bData.Add(0)
          bData.Add(0)
          bData.Add((m_Counter And &HFF00UL) >> 8)
          bData.Add((m_Counter And &HFFUL))
        ElseIf m_Counter <= &HFFFFFFUL Then
          bData.Add(0)
          bData.Add((m_Counter And &HFF0000UL) >> 16)
          bData.Add((m_Counter And &HFF00UL) >> 8)
          bData.Add((m_Counter And &HFFUL))
        ElseIf m_Counter <= &HFFFFFFFFUL Then
          bData.Add((m_Counter And &HFF000000UL) >> 24)
          bData.Add((m_Counter And &HFF0000UL) >> 16)
          bData.Add((m_Counter And &HFF00UL) >> 8)
          bData.Add((m_Counter And &HFFUL))
        ElseIf m_Counter <= &HFFFFFFFFFFUL Then
          bData.Add((m_Counter And &HFF00000000UL) >> 32)
          bData.Add((m_Counter And &HFF000000UL) >> 24)
          bData.Add((m_Counter And &HFF0000UL) >> 16)
          bData.Add((m_Counter And &HFF00UL) >> 8)
          bData.Add((m_Counter And &HFFUL))
        ElseIf m_Counter <= &HFFFFFFFFFFFFUL Then
          bData.Add((m_Counter And &HFF0000000000UL) >> 40)
          bData.Add((m_Counter And &HFF00000000UL) >> 32)
          bData.Add((m_Counter And &HFF000000UL) >> 24)
          bData.Add((m_Counter And &HFF0000UL) >> 16)
          bData.Add((m_Counter And &HFF00UL) >> 8)
          bData.Add((m_Counter And &HFFUL))
        ElseIf m_Counter <= &HFFFFFFFFFFFFFFUL Then
          bData.Add((m_Counter And &HFF000000000000UL) >> 48)
          bData.Add((m_Counter And &HFF0000000000UL) >> 40)
          bData.Add((m_Counter And &HFF00000000UL) >> 32)
          bData.Add((m_Counter And &HFF000000UL) >> 24)
          bData.Add((m_Counter And &HFF0000UL) >> 16)
          bData.Add((m_Counter And &HFF00UL) >> 8)
          bData.Add((m_Counter And &HFFUL))
        Else
          bData.Add((m_Counter And &HFF00000000000000UL) >> 56)
          bData.Add((m_Counter And &HFF000000000000UL) >> 48)
          bData.Add((m_Counter And &HFF0000000000UL) >> 40)
          bData.Add((m_Counter And &HFF00000000UL) >> 32)
          bData.Add((m_Counter And &HFF000000UL) >> 24)
          bData.Add((m_Counter And &HFF0000UL) >> 16)
          bData.Add((m_Counter And &HFF00UL) >> 8)
          bData.Add((m_Counter And &HFFUL))
        End If
      End If
      Return bData.ToArray
    End Function
    Public Overloads Shared Function FromByteArray(sName As String, bData As Byte()) As Parsed_POPM
      Dim bBlobs As Byte()() = SplitByteArrays(bData, 0, {0}, False, 2)
      If Not bBlobs.Length = 2 Then Return Nothing
      Dim sOwner As String = ParseString(TextEncoding.NT_ISO, bBlobs(0), 0, False)
      Dim bRating As Byte = bBlobs(1)(0)
      Dim uCount As UInt64 = 0
      Select Case bBlobs(1).Length
        Case 1
          uCount = 0
        Case Is < 5
          Try
            Dim bEncoding As TextEncoding = bBlobs(1)(1)
            Dim sCount As String = ParseString(bEncoding, bBlobs(1), 2, True)
            If IsNumeric(sCount) Then uCount = CULng(sCount)
          Catch ex As Exception
            MsgBox("Invalid Length reading " & sName & ": " & (bBlobs(1).Length - 1) & " is less than a 32-bit integer!")
          End Try
        Case 5 : uCount = bBlobs(1)(4) + (bBlobs(1)(3) << 8) + (bBlobs(1)(2) << 16) + (bBlobs(1)(1) << 24)
        Case 6 : uCount = bBlobs(1)(5) + (bBlobs(1)(4) << 8) + (bBlobs(1)(3) << 16) + (bBlobs(1)(2) << 24) + (bBlobs(1)(1) << 32)
        Case 7 : uCount = bBlobs(1)(6) + (bBlobs(1)(5) << 8) + (bBlobs(1)(4) << 16) + (bBlobs(1)(3) << 24) + (bBlobs(1)(2) << 32) + (bBlobs(1)(1) << 40)
        Case 8 : uCount = bBlobs(1)(7) + (bBlobs(1)(6) << 8) + (bBlobs(1)(5) << 16) + (bBlobs(1)(4) << 24) + (bBlobs(1)(3) << 32) + (bBlobs(1)(2) << 40) + (bBlobs(1)(1) << 48)
        Case 9 : uCount = bBlobs(1)(8) + (bBlobs(1)(7) << 8) + (bBlobs(1)(6) << 16) + (bBlobs(1)(5) << 24) + (bBlobs(1)(4) << 32) + (bBlobs(1)(3) << 40) + (bBlobs(1)(2) << 48) + (bBlobs(1)(1) << 56)
        Case Is > 9
          MsgBox("Invalid Length reading " & sName & ": " & (bBlobs(1).Length - 1) & " is greater than a 64-bit integer!")
        Case Else
          MsgBox("Invalid Length reading " & sName & ": " & (bBlobs(1).Length) & " is not a normal number!?!")
      End Select
      Return New Parsed_POPM(sName, sOwner, bRating, uCount)
    End Function
  End Class
  Public Class Parse_Failure
    Inherits ParseResponse
    Private m_Error As String
    Public ReadOnly Property [Error] As String
      Get
        Return m_Error
      End Get
    End Property
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {[Error]}
      End Get
    End Property
    Public Sub New(sName As String, sError As String)
      MyBase.New(sName)
      m_Error = sError
    End Sub
    Public Overrides Function ToByteArray(Version() As Byte) As Byte()
      Return Nothing
    End Function
  End Class
  Public Class Parse_Unparsed
    Inherits ParseResponse
    Private m_Data As Byte()
    Public Property Data As Byte()
      Get
        Return m_Data
      End Get
      Set(value As Byte())
        m_Data = value
      End Set
    End Property
    Public ReadOnly Property DataString As String
      Get
        Return BitConverter.ToString(Data)
      End Get
    End Property
    Public Sub New(sName As String, bData As Byte())
      MyBase.New(sName)
      m_Data = bData
    End Sub
    Public Overrides ReadOnly Property StringData As String()
      Get
        Return {DataString}
      End Get
    End Property
    Public Overrides Function ToByteArray(Version() As Byte) As Byte()
      Return m_Data
    End Function
  End Class

  Public Shared Function ParseGenreFrame(bData As Byte(), ByRef bEncoding As TextEncoding) As String()
    Dim sData As String = Nothing
    bEncoding = TextEncoding.NT_ISO
    If bData(0) < 4 Then
      bEncoding = bData(0)
      sData = ParseString(bEncoding, bData, 1, False)
    Else
      sData = ParseString(bEncoding, bData, 0, False)
    End If
    If String.IsNullOrEmpty(sData) Then Return Nothing
    Dim gList As New List(Of String)
    If sData.Contains("(") Then
      Dim gTmp As String = Nothing
      For I As Integer = 0 To sData.Length - 1
        If sData(I) = "(" Then
          If Not String.IsNullOrEmpty(gTmp) Then
            If gTmp = GetNullChar(bEncoding) Then
              gTmp = Nothing
            ElseIf gTmp.Trim = ";" Then
              gTmp = Nothing
            ElseIf gTmp.Trim = "," Then
              gTmp = Nothing
            ElseIf gTmp.Trim = "|" Then
              gTmp = Nothing
            ElseIf gTmp.Trim = "/" Then
              gTmp = Nothing
            Else
              gList.Add(gTmp)
              gTmp = Nothing
            End If
          End If
          If I < sData.Length - 1 AndAlso sData(I + 1) = "(" Then
            gTmp = "("
            I += 1
          End If
        ElseIf sData(I) = ")" Then
          If gTmp.StartsWith("(") And Not gTmp.Contains(")") Then
            gTmp &= ")"
          Else
            If Not String.IsNullOrEmpty(gTmp) Then
              gList.Add(gTmp)
              gTmp = Nothing
            End If
          End If
        ElseIf sData(I) = ";" Or sData(I) = "," Or sData(I) = "/" Or sData(I) = "|" Then
          If Not String.IsNullOrEmpty(gTmp) Then
            gList.Add(gTmp)
            gTmp = Nothing
          End If
        Else
          gTmp &= sData(I)
        End If
      Next
      If Not String.IsNullOrEmpty(gTmp) Then
        gList.Add(gTmp)
        gTmp = Nothing
      End If
    ElseIf sData.Contains(vbNullChar) Or sData.Contains(";") Or sData.Contains(",") Or sData.Contains("/") Or sData.Contains("|") Then
      Dim gSplit As String = sData.Replace(vbNullChar, "|")
      gSplit = gSplit.Replace(";", "|")
      gSplit = gSplit.Replace(",", "|")
      gSplit = gSplit.Replace("/", "|")
      Do While gSplit.Contains("||")
        gSplit = gSplit.Replace("||", "|")
      Loop
      gList.AddRange(Split(gSplit, "|"))
    Else
      gList.Add(sData)
    End If
    For I As Integer = 0 To gList.Count - 1
      gList(I) = gList(I).Trim
      If Not String.IsNullOrEmpty(gList(I)) AndAlso IsNumeric(gList(I)) Then gList(I) = clsID3v1.GenreName(gList(I))
    Next I
    Dim sGenre As String = Join(gList.ToArray, vbNewLine).Trim
    Do While sGenre.Contains(vbNewLine & vbNewLine)
      sGenre = sGenre.Replace(vbNewLine & vbNewLine, vbNewLine)
    Loop
    If sGenre.Contains(vbNewLine) Then Return Split(sGenre, vbNewLine)
    Return {sGenre}
  End Function
  Public Shared Function ParseGenreFrame(sData As String, bEncoding As TextEncoding) As String()
    If String.IsNullOrEmpty(sData) Then Return Nothing
    Dim gList As New List(Of String)
    If sData.Contains("(") Then
      Dim gTmp As String = Nothing
      For I As Integer = 0 To sData.Length - 1
        If sData(I) = "(" Then
          If Not String.IsNullOrEmpty(gTmp) Then
            If gTmp = GetNullChar(bEncoding) Then
              gTmp = Nothing
            ElseIf gTmp.Trim = ";" Then
              gTmp = Nothing
            ElseIf gTmp.Trim = "," Then
              gTmp = Nothing
            ElseIf gTmp.Trim = "/" Then
              gTmp = Nothing
            ElseIf gTmp.Trim = "|" Then
              gTmp = Nothing
            Else
              gList.Add(gTmp)
              gTmp = Nothing
            End If
          End If
          If I < sData.Length - 1 AndAlso sData(I + 1) = "(" Then
            gTmp = "("
            I += 1
          End If
        ElseIf sData(I) = ")" Then
          If gTmp.StartsWith("(") And Not gTmp.Contains(")") Then
            gTmp &= ")"
          Else
            If Not String.IsNullOrEmpty(gTmp) Then
              gList.Add(gTmp)
              gTmp = Nothing
            End If
          End If
        Else
          gTmp &= sData(I)
        End If
      Next
      If Not String.IsNullOrEmpty(gTmp) Then
        gList.Add(gTmp)
        gTmp = Nothing
      End If
    ElseIf sData.Contains(vbNullChar) Or sData.Contains(";") Or sData.Contains(",") Or sData.Contains("/") Or sData.Contains("|") Then
      Dim gSplit As String = sData.Replace(vbNullChar, "|")
      gSplit = gSplit.Replace(";", "|")
      gSplit = gSplit.Replace(",", "|")
      gSplit = gSplit.Replace("/", "|")
      Do While gSplit.Contains("||")
        gSplit = gSplit.Replace("||", "|")
      Loop
      gList.AddRange(Split(gSplit, "|"))
    Else
      gList.Add(sData)
    End If
    For I As Integer = 0 To gList.Count - 1
      gList(I) = gList(I).Trim
      If Not String.IsNullOrEmpty(gList(I)) AndAlso IsNumeric(gList(I)) Then gList(I) = clsID3v1.GenreName(gList(I))
    Next I
    Dim sGenre As String = Join(gList.ToArray, vbNewLine).Trim
    Do While sGenre.Contains(vbNewLine & vbNewLine)
      sGenre = sGenre.Replace(vbNewLine & vbNewLine, vbNewLine)
    Loop
    If sGenre.Contains(vbNewLine) Then Return Split(sGenre, vbNewLine)
    Return {sGenre}
  End Function

  Public Shared Function ImageID(ID As ID3_PIC_TYPE) As String
    Select Case ID
      Case ID3_PIC_TYPE.OTHER : Return "Other"
      Case ID3_PIC_TYPE.PNG_ICON_32x32 : Return "32x32 Icon"
      Case ID3_PIC_TYPE.FILE_ICON : Return "Icon"
      Case ID3_PIC_TYPE.FRONT_COVER : Return "Front Cover"
      Case ID3_PIC_TYPE.BACK_COVER : Return "Back Cover"
      Case ID3_PIC_TYPE.LEAFLET : Return "Leaflet"
      Case ID3_PIC_TYPE.MEDIA_LABEL : Return "Media"
      Case ID3_PIC_TYPE.LEAD_ARTIST : Return "Lead Artist"
      Case ID3_PIC_TYPE.ARTIST : Return "Artist"
      Case ID3_PIC_TYPE.CONDUCTOR : Return "Conductor"
      Case ID3_PIC_TYPE.ORCHESTRA_BAND : Return "OBand"
      Case ID3_PIC_TYPE.COMPOSER : Return "Composer"
      Case ID3_PIC_TYPE.LYRICIST : Return "Lyricist"
      Case ID3_PIC_TYPE.RECORDING_LOCATION : Return "Recording Location"
      Case ID3_PIC_TYPE.DURING_RECORDING : Return "During Recording"
      Case ID3_PIC_TYPE.DURING_PERFORMANCE : Return "During Performance"
      Case ID3_PIC_TYPE.SCREENCAP : Return "Video Screen Capture"
      Case ID3_PIC_TYPE.RED_HERRING : Return "Red Herring"
      Case ID3_PIC_TYPE.ILLUSTRATION : Return "Illustration"
      Case ID3_PIC_TYPE.BAND_LOGO : Return "Band Logo"
      Case ID3_PIC_TYPE.STUDIO_LOGO : Return "Studio Logo"
      Case ID3_PIC_TYPE.INVALID : Return "Invalid Type"
      Case Else : Return "Unknown (" & Hex(ID) & ")"
    End Select
  End Function

  Public Shared Function SplitByteArrayStrings(ByteArray As Byte(), StartIndex As Integer, Encoding As TextEncoding, IncludeSeparator As Boolean) As Byte()()
    Select Case Encoding
      Case TextEncoding.NT_ISO, TextEncoding.UTF_8
        Return SplitByteArrays(ByteArray, StartIndex, GetNullCharArr(Encoding), IncludeSeparator, 2)
      Case TextEncoding.UTF_16_LE, TextEncoding.UTF_16_BE
        Dim TryExtra As Byte()() = SplitByteArrays(ByteArray, StartIndex, {0, 0, 0}, IncludeSeparator, 2)
        Dim TryReal As Byte()() = SplitByteArrays(ByteArray, StartIndex, GetNullCharArr(Encoding), IncludeSeparator, 2)
        If TryExtra.Length = 0 Then Return TryReal
        If Math.Abs(TryExtra(0).Length - TryReal(0).Length) > 1 Then Return TryReal
        Return TryExtra
      Case Else
        Return SplitByteArrays(ByteArray, StartIndex, GetNullCharArr(Encoding), IncludeSeparator, 2)
    End Select
  End Function

  Public Shared Function SplitByteArrays(ByteArray As Byte(), StartIndex As Integer, Separator As Byte(), IncludeSeparator As Boolean, Optional Limit As Integer = -1) As Byte()()
    Dim bArrList As New List(Of Byte())
    Dim bArrTmp As New List(Of Byte)
    Dim bMatches(Separator.Length - 1) As Boolean
    Dim iMatches As Integer = 0
    For I As Integer = StartIndex To ByteArray.Length - 1
      If ByteArray(I) = Separator(iMatches) Then
        bMatches(iMatches) = True
        iMatches += 1
        If IncludeSeparator Then bArrTmp.Add(ByteArray(I))
      Else
        If iMatches > 0 Then
          For M As Integer = 0 To bMatches.Length - 1
            bMatches(M) = False
          Next
          iMatches = 0
        End If
        bArrTmp.Add(ByteArray(I))
      End If
      If iMatches >= Separator.Length Then
        If Limit > 0 Then
          If bArrList.Count = Limit - 1 And I + 1 <= ByteArray.Length Then
            If Not IncludeSeparator Then bArrTmp.Add(ByteArray(I))
            For B As Integer = I + 1 To ByteArray.Length - 1
              bArrTmp.Add(ByteArray(B))
            Next
            Exit For
          End If
        End If
        bArrList.Add(bArrTmp.ToArray)
        bArrTmp.Clear()
        For M As Integer = 0 To bMatches.Length - 1
          bMatches(M) = False
        Next
        iMatches = 0
      End If
    Next
    If bArrTmp.Count > 0 Then
      bArrList.Add(bArrTmp.ToArray)
      bArrTmp.Clear()
    End If
    Return bArrList.ToArray

  End Function

  Private Shared Function ParseString(ByRef Encoding As TextEncoding, bData As Byte(), StartIndex As Integer, AllowChange As Boolean) As String
    If bData Is Nothing Then Return Nothing
    If bData.Length = 0 Then Return Nothing
    If bData.Length <= StartIndex Then Return Nothing
    Dim bBlobs As Byte()() = SplitByteArrayStrings(bData, StartIndex, Encoding, True)
    Dim bBlob As New List(Of Byte)
    If bBlobs(0).Length = 0 Then
      bBlob.AddRange(bData)
    Else
      bBlob.AddRange(bBlobs(0))
    End If
    Dim sText As String = Nothing
    Select Case Encoding
      Case TextEncoding.NT_ISO
        If bBlob.Count > 1 Then
          If bBlob(0) = &HFF And bBlob(1) = &HFE Then
            bBlob.RemoveRange(0, 2)
            If AllowChange Then Encoding = TextEncoding.UTF_16_LE
            If bBlob.Count = 1 AndAlso bBlob(0) = 0 Then
              sText = Nothing
            Else
              sText = System.Text.Encoding.Unicode.GetString(bBlob.ToArray)
            End If
          ElseIf bBlob(0) = &HFE And bBlob(1) = &HFF Then
            bBlob.RemoveRange(0, 2)
            If AllowChange Then Encoding = TextEncoding.UTF_16_BE
            If bBlob.Count = 1 AndAlso bBlob(0) = 0 Then
              sText = Nothing
            Else
              sText = System.Text.Encoding.BigEndianUnicode.GetString(bBlob.ToArray)
            End If
          Else
            sText = fileEncoding.GetString(bBlob.ToArray)
          End If
        Else
          sText = fileEncoding.GetString(bBlob.ToArray)
        End If
      Case TextEncoding.UTF_16_LE
        If bBlob.Count > 1 Then
          If bBlob(0) = &HFF And bBlob(1) = &HFE Then
            bBlob.RemoveRange(0, 2)
            If bBlob.Count = 1 AndAlso bBlob(0) = 0 Then
              sText = Nothing
            Else
              sText = System.Text.Encoding.Unicode.GetString(bBlob.ToArray)
            End If
          ElseIf bBlob(0) = &HFE And bBlob(1) = &HFF Then
            bBlob.RemoveRange(0, 2)
            If AllowChange Then Encoding = TextEncoding.UTF_16_BE
            If bBlob.Count = 1 AndAlso bBlob(0) = 0 Then
              sText = Nothing
            Else
              sText = System.Text.Encoding.BigEndianUnicode.GetString(bBlob.ToArray)
            End If
          Else
            sText = System.Text.Encoding.Unicode.GetString(bBlob.ToArray)
          End If
        Else
          sText = System.Text.Encoding.Unicode.GetString(bBlob.ToArray)
        End If
      Case TextEncoding.UTF_16_BE
        If bBlob.Count > 1 AndAlso (bBlob(0) = &HFE And bBlob(1) = &HFF) Then bBlob.RemoveRange(0, 2)
        If bBlob.Count = 1 AndAlso bBlob(0) = 0 Then
          sText = Nothing
        Else
          sText = System.Text.Encoding.BigEndianUnicode.GetString(bBlob.ToArray)
        End If
      Case TextEncoding.UTF_8
        If bBlob.Count > 1 Then
          If (bBlob(0) = &HFF And bBlob(1) = &HFE) Or (bBlob(0) = &HFE And bBlob(1) = &HFF) Then bBlob.RemoveRange(0, 2)
        End If
        If bBlob.Count = 1 AndAlso bBlob(0) = 0 Then
          sText = Nothing
        Else
          sText = System.Text.Encoding.UTF8.GetString(bBlob.ToArray)
        End If
      Case Else
        'Probably not encoded...
    End Select
    If String.IsNullOrEmpty(sText) Then Return Nothing
    Do While sText.Contains(vbNullChar)
      sText = sText.Replace(vbNullChar, "[NULL]")
    Loop
    Do While sText.EndsWith("[NULL]")
      sText = sText.Substring(0, sText.Length - 6)
    Loop
    Do While sText.Contains(vbLf)
      sText = sText.Replace(vbLf, "[LF]")
    Loop
    sText = sText.Trim
    Dim sNewText As String = Nothing
    For I As Integer = 0 To sText.Length - 1
      If Char.IsLetterOrDigit(sText, I) Or Char.IsWhiteSpace(sText, I) Or Char.IsPunctuation(sText, I) Or Char.IsSymbol(sText, I) Then sNewText &= sText(I)
    Next
    Return sNewText
  End Function

  Public Function RemoveAll() As ID3Returns
    ID3Frames.Clear()
    RemoveAll = ID3Returns.Removed
  End Function

  Public Function RemoveFrame(sName As String, Optional Index As Integer = -1) As ID3Returns
    Dim framesFound As New List(Of Integer)
    For I As Integer = 0 To ID3Frames.Count - 1
      Dim sFind As String = TrimNull(ID3Frames(I).FrameName)
      If sFind = GetOldName(sName) Or sFind = GetNewName(sName, 3) Or sFind = GetNewName(sName, 4) Then
        If (ID3Frames(I).FrameFlags And FFLG.READONLY) = FFLG.READONLY Then
          Return ID3Returns.ReadOnly
        Else
          framesFound.Add(I)
        End If
      End If
    Next I
    If framesFound.Count = 0 Then Return ID3Returns.GeneralFailure
    If Index = -1 Then
      For Each frameIdx In framesFound
        ID3Frames.RemoveAt(frameIdx)
      Next
      Return ID3Returns.Removed
    Else
      If Index >= framesFound.Count Then Return ID3Returns.GeneralFailure
      ID3Frames.RemoveAt(framesFound(Index))
      Return ID3Returns.Removed
    End If
  End Function

  Private Class frameSort
    Implements IComparer(Of ID3v2Frame)
    Public Function Compare(x As ID3v2Frame, y As ID3v2Frame) As Integer Implements IComparer(Of Seed.clsID3v2.ID3v2Frame).Compare
      Dim oldX As String = GetOldName(x.FrameName)
      Dim oldY As String = GetOldName(y.FrameName)
      If oldX = oldY Then Return 0
      If oldX = "PIC" Or oldX = "APIC" Then Return 1
      If oldY = "PIC" Or oldY = "APIC" Then Return -1
      If oldX = "PRIV" Then Return 1
      If oldY = "PRIV" Then Return -1
      If oldX = "TRK" Or oldX = "TRCK" Then Return -1
      If oldY = "TRK" Or oldY = "TRCK" Then Return 1
      If oldX = "TT2" Or oldX = "TIT2" Then Return -1
      If oldY = "TT2" Or oldY = "TIT2" Then Return 1
      If oldX = "TP1" Or oldX = "TPE1" Then Return -1
      If oldY = "TP1" Or oldY = "TPE1" Then Return 1
      If oldX = "TP2" Or oldX = "TPE2" Then Return -1
      If oldY = "TP2" Or oldY = "TPE2" Then Return 1
      If oldX = "TAL" Or oldX = "TALB" Then Return -1
      If oldY = "TAL" Or oldY = "TALB" Then Return 1
      If oldX = "TCO" Or oldX = "TCON" Then Return -1
      If oldY = "TCO" Or oldY = "TCON" Then Return 1
      If oldX = "TRD" Or oldX = "TDRC" Then Return -1
      If oldY = "TRD" Or oldY = "TDRC" Then Return 1
      If oldX = "TYE" Or oldX = "TYER" Then Return -1
      If oldY = "TYE" Or oldY = "TYER" Then Return 1
      Return (New CaseInsensitiveComparer).Compare(GetFrameName(oldX), GetFrameName(oldY))
    End Function
  End Class

  Private Sub WriteFrame(ByRef cWriter As IO.BinaryWriter, sName As String, uFlags As FFLG, bData As Byte())
    'Name
    cWriter.Write(fileEncoding.GetBytes(sName))
    'DWORD Size
    If ID3Header.Version(0) = 4 Then
      cWriter.Write(fileEncoding.GetBytes(MakeDWORD(SynchSafe(bData.Length))))
    ElseIf ID3Header.Version(0) = 3 Then
      cWriter.Write(fileEncoding.GetBytes(MakeDWORD(bData.Length)))
    ElseIf ID3Header.Version(0) = 2 Then
      cWriter.Write(fileEncoding.GetBytes(MakeDWORD(bData.Length).Substring(1)))
    Else
      cWriter.Write(fileEncoding.GetBytes(MakeDWORD(bData.Length)))
    End If
    'WORD Flags
    If Not ID3Header.Version(0) = 2 Then cWriter.Write(fileEncoding.GetBytes(MakeWORD(uFlags)))
    'Data
    cWriter.Write(bData)
  End Sub

  Public Function Save(Optional SaveAs As String = Nothing) As Boolean
    If String.IsNullOrEmpty(SaveAs) Then SaveAs = m_sMp3File
    Dim FileData As Byte() = TrimID3v2(m_sMp3File)
    ID3Frames.Sort(New frameSort)
    Dim newSave As String = SaveAs & ".new_id" & (New Random).Next(0, 255).ToString("x2")
    Dim oldSave As String = SaveAs & ".old_id" & (New Random).Next(0, 255).ToString("x2")
    Try
      Using ID3Data As New IO.FileStream(newSave, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.Read)
        Using ID3Writer As New IO.BinaryWriter(ID3Data, fileEncoding)
          ID3Header.Identifier = "ID3"
          ID3Header.Flags = 0
          Dim bContent As Byte()
          Using ID3Content As New IO.MemoryStream
            Using ID3ContentWriter As New IO.BinaryWriter(ID3Content, fileEncoding)
              For I = 0 To FrameCount - 1
                If FrameData(I) Is Nothing Then Continue For
                If FrameData(I).Length = 0 Then Continue For
                If FrameData(I).Length = 1 AndAlso FrameData(I)(0) = 0 Then Continue For
                Dim sName As String = FrameName(I)
                If ID3Header.Version(0) = 2 Then
                  sName = GetOldName(sName)
                  If Not sName.Length = 3 Then
                    Debug.Print("Unhandled Frame: " & FrameName(I) & ": " & BitConverter.ToString(FrameData(I)) & " (" & Replace(fileEncoding.GetString(FrameData(I)), vbNullChar, "[NULL]") & ")")
                    Return False
                  End If
                ElseIf ID3Header.Version(0) = 3 Then
                  sName = GetNewName(sName, 3)
                  If Not sName.Length = 4 Then
                    Debug.Print("Unhandled Frame: " & FrameName(I) & ": " & BitConverter.ToString(FrameData(I)) & " (" & Replace(fileEncoding.GetString(FrameData(I)), vbNullChar, "[NULL]") & ")")
                    Return False
                  End If
                ElseIf ID3Header.Version(0) = 4 Then
                  sName = GetNewName(sName, 4)
                  If Not sName.Length = 4 Then
                    Debug.Print("Unhandled Frame: " & FrameName(I) & ": " & BitConverter.ToString(FrameData(I)) & " (" & Replace(fileEncoding.GetString(FrameData(I)), vbNullChar, "[NULL]") & ")")
                    Return False
                  End If
                End If
                If (FrameFlags(I) And FFLG.TAGALTER) = FFLG.TAGALTER Or (FrameFlags(I) And FFLG.FILEALTER) = FFLG.FILEALTER Then Continue For
                If FrameName(I) = "TCON" Or FrameName(I) = "TCO" Then
                  Dim fDataList As ParseResponse() = Parsed_TCON.FromByteArrayEx(FrameName(I), FrameData(I))
                  If fDataList Is Nothing OrElse fDataList.Length < 1 Then
                    Debug.Print("Unhandled Frame: " & FrameName(I) & ": " & BitConverter.ToString(FrameData(I)) & " (" & Replace(fileEncoding.GetString(FrameData(I)), vbNullChar, "[NULL]") & ")")
                    Return False
                  End If
                  For Each fData As ParseResponse In fDataList
                    If fData Is Nothing OrElse fData.GetType = GetType(Parse_Failure) Then
                      Dim isNulls As Boolean = True
                      For J As Integer = 0 To FrameData(I).Length - 1
                        If Not FrameData(I)(J) = 0 Then isNulls = False
                      Next
                      If isNulls Then Continue For
                      Debug.Print("Unhandled Frame: " & FrameName(I) & ": " & BitConverter.ToString(FrameData(I)) & " (" & Replace(fileEncoding.GetString(FrameData(I)), vbNullChar, "[NULL]") & ")")
                      Return False
                    End If
                    WriteFrame(ID3ContentWriter, sName, FrameFlags(I), fData.ToByteArray(ID3Header.Version))
                  Next
                Else
                  Dim fData As ParseResponse = ParseResponse.FromByteArray(FrameName(I), FrameData(I))
                  If fData Is Nothing OrElse fData.GetType = GetType(Parse_Failure) Then
                    Dim isNulls As Boolean = True
                    For J As Integer = 0 To FrameData(I).Length - 1
                      If Not FrameData(I)(J) = 0 Then isNulls = False
                    Next
                    If isNulls Then Continue For
                    Debug.Print("Unhandled Frame: " & FrameName(I) & ": " & BitConverter.ToString(FrameData(I)) & " (" & Replace(fileEncoding.GetString(FrameData(I)), vbNullChar, "[NULL]") & ")")
                    Return False
                  End If
                  WriteFrame(ID3ContentWriter, sName, FrameFlags(I), fData.ToByteArray(ID3Header.Version))
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

          ID3Writer.Write(fileEncoding.GetBytes(ID3Header.Identifier))
          ID3Writer.Write(ID3Header.Version(0))
          ID3Writer.Write(ID3Header.Version(1))
          ID3Writer.Write(ID3Header.Flags)

          ID3Writer.Write(fileEncoding.GetBytes(MakeDWORD(SynchSafe(ID3Header.Size))))

          ID3Writer.Write(bContent)

          ID3Writer.Write(FileData)
          ID3Writer.Flush()
        End Using
        ID3Data.Close()
      End Using
      If CompareFiles(SaveAs, newSave) Then Return True
      If IO.File.Exists(SaveAs) Then
        Try
          IO.File.Move(SaveAs, oldSave)
        Catch ex As Exception
          Return False
        End Try
      End If
      Try
        IO.File.Move(newSave, SaveAs)
      Catch ex As Exception
        Return False
      End Try
      Try
        IO.File.Delete(oldSave)
      Catch ex As Exception
      End Try
      Return True
    Catch ex As Exception
      Return False
    Finally
      If IO.File.Exists(newSave) Then IO.File.Delete(newSave)
      If IO.File.Exists(oldSave) Then
        If IO.File.Exists(SaveAs) Then
          IO.File.Delete(oldSave)
        Else
          IO.File.Move(oldSave, SaveAs)
        End If
      End If
    End Try

  End Function

  Private Function CompareFiles(oldFile As String, newFile As String) As Boolean
    Dim bOld As Byte() = IO.File.ReadAllBytes(oldFile)
    Dim bNew As Byte() = IO.File.ReadAllBytes(newFile)
    If Not bOld.Length = bNew.Length Then Return False
    For I As Integer = 0 To bOld.Length - 1
      If Not bOld(I) = bNew(I) Then Return False
    Next
    Return True
  End Function

  Private Function TrimID3v2(Path As String) As Byte()
    If String.IsNullOrEmpty(Path) Then Return Nothing
    If Not io.file.exists(Path) Then Return Nothing
    If (New IO.FileInfo(Path)).Length >= 1024L * 1024L * 1024L * 4L Then Return Nothing
    Dim bFile As Byte() = IO.File.ReadAllBytes(Path)
    Dim lFrameStart As Integer = -1
    Do
      lFrameStart = Array.IndexOf(Of Byte)(bFile, &HFF, lFrameStart + 1)
      If lFrameStart = -1 Then Return bFile
    Loop Until CheckMPEG(bFile, lFrameStart)
    If lFrameStart = -1 Then Return bFile
    Dim lFrameLen As Integer = bFile.Length - lFrameStart
    Dim bReturn(lFrameLen - 1) As Byte
    Array.ConstrainedCopy(bFile, lFrameStart, bReturn, 0, lFrameLen)
    Return bReturn
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
