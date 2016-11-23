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
      Dim tN As ParseResponse = ParseFrame(sName, bData)
      Dim tNew As Parsed_TCON = Nothing
      If tN.GetType Is GetType(Parsed_TCON) Then
        tNew = tN
        bEncoding = tNew.Encoding
      End If
      Dim sGenres As String = Nothing
      For I As Integer = FrameCount - 1 To 0 Step -1
        Dim sFrame As ID3v2Frame = ID3Frames(I)
        If sFrame.FrameName = "TCO" Or sFrame.FrameName = "TCON" Then
          Dim tE As ParseResponse = ParseFrame(sFrame.FrameName, sFrame.FrameData)
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
      bEntity = MakeFrameStringData(bEncoding, sGenres, ID3Header.Version(0))
      ID3Frames.Add(New ID3v2Frame(sName, iFlags, bEntity))
      Return ID3Returns.Added
    End If
    ID3Frames.Add(New ID3v2Frame(sName, iFlags, bData))
    Return ID3Returns.Added
  End Function

  Public Function AddTextFrame(sName As String, ByVal Contents As EncodedText, Optional ByVal iFlags As FFLG = 0) As ID3Returns
    Return AddBareFrame(sName, MakeFrameStringData(Contents.Encoding, Contents.Text, ID3Header.Version(0)), iFlags)
  End Function

  Private Function MakeFrameStringData(Encoding As TextEncoding, Text As String, Version As Byte, Optional SkipEncoding As Boolean = False) As Byte()
    If Version = 4 Then
      Dim bData As New List(Of Byte)
      If Not SkipEncoding Then bData.Add(Encoding)
      'Dim sData As String = Nothing
      'sData = Chr(Encoding)
      Select Case Encoding
        Case TextEncoding.UTF_16_LE
          bData.Add(&HFF)
          bData.Add(&HFE)
          'sData &= Chr(&HFF) & Chr(&HFE)
          If Not String.IsNullOrEmpty(Text) Then
            'If Text.Length Mod 2 = 1 Then Text &= vbNullChar
            'sData &= System.Text.Encoding.Unicode.GetString(fileEncoding.GetBytes(Text))
            bData.AddRange(System.Text.Encoding.Unicode.GetBytes(Text))
          End If
        Case TextEncoding.UTF_16_BE
          If Not String.IsNullOrEmpty(Text) Then
            'If Text.Length Mod 2 = 1 Then Text &= vbNullChar
            'sData &= System.Text.Encoding.BigEndianUnicode.GetString(fileEncoding.GetBytes(Text))
            bData.AddRange(System.Text.Encoding.BigEndianUnicode.GetBytes(Text))
          End If
        Case TextEncoding.UTF_8
          'If Not String.IsNullOrEmpty(Text) Then sData &= System.Text.Encoding.UTF8.GetString(fileEncoding.GetBytes(Text))
          If Not String.IsNullOrEmpty(Text) Then bData.AddRange(System.Text.Encoding.UTF8.GetBytes(Text))
        Case Else
          'If Not String.IsNullOrEmpty(Text) Then sData &= Text
          If Not String.IsNullOrEmpty(Text) Then bData.AddRange(fileEncoding.GetBytes(Text))
      End Select
      bData.AddRange(GetNullCharArr(Encoding))
      'sData &= GetNullChar(Encoding)
      'Return sData
      Return bData.ToArray
    Else
      'Dim sData As String = Nothing
      Dim bData As New List(Of Byte)
      Select Case Encoding
        Case TextEncoding.UTF_16_LE, TextEncoding.UTF_8
          If Not SkipEncoding Then bData.Add(TextEncoding.UNICODE)
          bData.Add(&HFF)
          bData.Add(&HFE)
          'sData = Chr(TextEncoding.UNICODE)
          'sData &= Chr(&HFF) & Chr(&HFE)
          If Not String.IsNullOrEmpty(Text) Then
            'If Text.Length Mod 2 = 1 Then Text &= vbNullChar
            'sData &= System.Text.Encoding.Unicode.GetString(fileEncoding.GetBytes(Text))
            bData.AddRange(System.Text.Encoding.Unicode.GetBytes(Text))
          End If
          'sData &= GetNullChar(TextEncoding.UNICODE)
          bData.AddRange(GetNullCharArr(TextEncoding.UNICODE))
        Case TextEncoding.UTF_16_BE
          If Not SkipEncoding Then bData.Add(TextEncoding.UNICODE)
          bData.Add(&HFE)
          bData.Add(&HFF)
          'sData = Chr(TextEncoding.UNICODE)
          'sData &= Chr(&HFE) & Chr(&HFF)
          If Not String.IsNullOrEmpty(Text) Then
            'If Text.Length Mod 2 = 1 Then Text &= vbNullChar
            'sData &= System.Text.Encoding.BigEndianUnicode.GetString(fileEncoding.GetBytes(Text))
            bData.AddRange(System.Text.Encoding.BigEndianUnicode.GetBytes(Text))
          End If
          bData.AddRange(GetNullCharArr(TextEncoding.UTF_16_BE))
          'sData &= GetNullChar(TextEncoding.UTF_16_BE)
        Case Else
          If Not SkipEncoding Then bData.Add(TextEncoding.NT_ISO)
          If Not String.IsNullOrEmpty(Text) Then bData.AddRange(fileEncoding.GetBytes(Text))
          bData.AddRange(GetNullCharArr(TextEncoding.NT_ISO))
          'sData = Chr(TextEncoding.NT_ISO)
          'If Not String.IsNullOrEmpty(Text) Then sData &= Text
          'sData &= GetNullChar(TextEncoding.NT_ISO)
      End Select
      'Return sData
      Return bData.ToArray
    End If
  End Function

  Public Function AddUserLinkFrame(ByVal Description As EncodedText, ByVal URL As String, Optional ByVal iFlags As FFLG = 0) As ID3Returns
    If Description.Text = URL Then Description.Text = Nothing
    Dim bData As New List(Of Byte)
    bData.AddRange(MakeFrameStringData(Description.Encoding, Description.Text, ID3Header.Version(0)))
    bData.AddRange(fileEncoding.GetBytes(URL))
    If ID3v2Ver = "2.2.0" Then
      Return AddBareFrame("WXX", bData.ToArray, iFlags)
    Else
      Return AddBareFrame("WXXX", bData.ToArray, iFlags)
    End If
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
    Debug.Print("Unknown Extension: " & IO.Path.GetExtension(Path).ToLower)
    Return ID3_PIC_MIME.INVALID
  End Function

  Public Shared Function StrToMIME(Format As String, id3v2 As Boolean) As ID3_PIC_MIME
    If id3v2 Then
      Select Case Format.ToUpper
        Case "BMP" : Return ID3_PIC_MIME.BMP
        Case "GIF" : Return ID3_PIC_MIME.GIF
        Case "JPG" : Return ID3_PIC_MIME.JPG
        Case "PNG" : Return ID3_PIC_MIME.PNG
      End Select
      Debug.Print("Unknown Format String: " & Format)
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
      Debug.Print("Unknown MIME String: " & Format)
      Return ID3_PIC_MIME.INVALID
    End If
  End Function

  Public Function AddImageFrame(ByVal pData As Byte(), ByVal [Type] As ID3_PIC_TYPE, ByVal Format As ID3_PIC_MIME, ByVal Description As EncodedText, Optional ByVal iFlags As FFLG = 0) As ID3Returns
    Dim sName As String = Nothing
    Dim bData As New List(Of Byte)
    If Format = ID3_PIC_MIME.INVALID Then Return ID3Returns.GeneralFailure
    If ID3v2Ver = "2.2.0" Then
      sName = "PIC"
      If Type = ID3_PIC_TYPE.INVALID Then Type = ID3_PIC_TYPE.OTHER
      If Description.Encoding = TextEncoding.NT_ISO Then
        bData.Add(TextEncoding.NT_ISO)
      Else
        bData.Add(TextEncoding.UNICODE)
      End If
      If Format = ID3_PIC_MIME.INVALID Then
        'TODO: detect format
      End If
      bData.AddRange(fileEncoding.GetBytes(MIMEtoString(Format, True)))
      bData.Add(Type)
      Select Case Description.Encoding
        Case TextEncoding.UTF_16_LE, TextEncoding.UTF_8
          bData.Add(&HFF)
          bData.Add(&HFE)
          If Not String.IsNullOrEmpty(Description.Text) Then bData.AddRange(System.Text.Encoding.Unicode.GetBytes(Description.Text))
          bData.AddRange(GetNullCharArr(TextEncoding.UNICODE))
        Case TextEncoding.UTF_16_BE
          bData.Add(&HFE)
          bData.Add(&HFF)
          If Not String.IsNullOrEmpty(Description.Text) Then bData.AddRange(System.Text.Encoding.BigEndianUnicode.GetBytes(Description.Text))
          bData.AddRange(GetNullCharArr(Description.Encoding))
        Case Else
          If Not String.IsNullOrEmpty(Description.Text) Then bData.AddRange(fileEncoding.GetBytes(Description.Text))
          bData.AddRange(GetNullCharArr(Description.Encoding))
      End Select
      bData.AddRange(pData)
    Else
      sName = "APIC"
      If Type = ID3_PIC_TYPE.INVALID Then Type = ID3_PIC_TYPE.FRONT_COVER
      bData.Add(Description.Encoding)
      If Format = ID3_PIC_MIME.INVALID Then
        'TODO: detect format
      End If
      bData.AddRange(fileEncoding.GetBytes(MIMEtoString(Format, False)))
      bData.AddRange(GetNullCharArr(TextEncoding.NT_ISO))
      bData.Add(Type)
      If ID3v2Ver = "2.3.0" Then
        Select Case Description.Encoding
          Case TextEncoding.UTF_16_LE, TextEncoding.UTF_8
            bData.Add(&HFF)
            bData.Add(&HFE)
            If Not String.IsNullOrEmpty(Description.Text) Then bData.AddRange(System.Text.Encoding.Unicode.GetBytes(Description.Text))
            bData.AddRange(GetNullCharArr(TextEncoding.UNICODE))
          Case TextEncoding.UTF_16_BE
            bData.Add(&HFE)
            bData.Add(&HFF)
            If Not String.IsNullOrEmpty(Description.Text) Then bData.AddRange(System.Text.Encoding.BigEndianUnicode.GetBytes(Description.Text))
            bData.AddRange(GetNullCharArr(Description.Encoding))
          Case Else
            If Not String.IsNullOrEmpty(Description.Text) Then bData.AddRange(fileEncoding.GetBytes(Description.Text))
            bData.AddRange(GetNullCharArr(Description.Encoding))
        End Select
      Else
        Select Case Description.Encoding
          Case TextEncoding.UTF_16_LE
            If Not String.IsNullOrEmpty(Description.Text) Then bData.AddRange(System.Text.Encoding.Unicode.GetBytes(Description.Text))
          Case TextEncoding.UTF_16_BE
            If Not String.IsNullOrEmpty(Description.Text) Then bData.AddRange(System.Text.Encoding.BigEndianUnicode.GetBytes(Description.Text))
          Case TextEncoding.UTF_8
            If Not String.IsNullOrEmpty(Description.Text) Then bData.AddRange(System.Text.Encoding.UTF8.GetBytes(Description.Text))
          Case Else
            If Not String.IsNullOrEmpty(Description.Text) Then bData.AddRange(fileEncoding.GetBytes(Description.Text))
        End Select
        bData.AddRange(GetNullCharArr(Description.Encoding))
      End If
      bData.AddRange(pData)
    End If
    Return AddBareFrame(sName, bData.ToArray, iFlags)
  End Function

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
        If (sFind = sOldName AndAlso Not IsNothing(ParseFrame(sOldName, ID3Frames(I).FrameData))) Or
           (sFind = sNewName3 AndAlso Not IsNothing(ParseFrame(sNewName3, ID3Frames(I).FrameData))) Or
           (sFind = sNewName4 AndAlso Not IsNothing(ParseFrame(sNewName4, ID3Frames(I).FrameData))) Then frameMatches += 1
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
        If sFind = sOldName AndAlso Not IsNothing(ParseFrame(sOldName, ID3Frames(I).FrameData)) Then
          MatchLists.Add(ParseFrame(sOldName, ID3Frames(I).FrameData))
        ElseIf sFind = sNewName3 AndAlso Not IsNothing(ParseFrame(sNewName3, ID3Frames(I).FrameData)) Then
          MatchLists.Add(ParseFrame(sNewName3, ID3Frames(I).FrameData))
        ElseIf sFind = sNewName4 AndAlso Not IsNothing(ParseFrame(sNewName4, ID3Frames(I).FrameData)) Then
          MatchLists.Add(ParseFrame(sNewName4, ID3Frames(I).FrameData))
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
        If sFind = sOldName AndAlso Not IsNothing(ParseFrame(sOldName, ID3Frames(I).FrameData)) Then
          sMatch &= Join(ParseFrame(sOldName, ID3Frames(I).FrameData).StringData, delimB) & delimA
        ElseIf sFind = sNewName3 AndAlso Not IsNothing(ParseFrame(sNewName3, ID3Frames(I).FrameData)) Then
          sMatch &= Join(ParseFrame(sNewName3, ID3Frames(I).FrameData).StringData, delimB) & delimA
        ElseIf sFind = sNewName4 AndAlso Not IsNothing(ParseFrame(sNewName4, ID3Frames(I).FrameData)) Then
          sMatch &= Join(ParseFrame(sNewName4, ID3Frames(I).FrameData).StringData, delimB) & delimA
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
        ElseIf gData.Contains(vbNullChar) Or gData.Contains(";") Or gData.Contains(",") Or gData.Contains("|") Then
          Dim gSplit As String = gData.Replace(vbNullChar, "|")
          gSplit = gSplit.Replace(";", "|")
          gSplit = gSplit.Replace(",", "|")
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
      If value.Contains(vbNullChar) Or value.Contains(";") Or value.Contains(",") Or value.Contains("|") Then
        Dim gSplit As String = value.Replace(vbNullChar, "|")
        gSplit = gSplit.Replace(";", "|")
        gSplit = gSplit.Replace(",", "|")
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
        AddBareFrame("TCO", MakeFrameStringData(TextEncoding.NT_ISO, gEntity, ID3Header.Version(0)))
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
        Dim bFrameData As Byte() = ioReader.ReadBytes(ID3Frame.FrameSize)
        AddBareFrame(ID3Frame.FrameName, bFrameData, Val("&H" & ID3Frame.FrameFlags(1).ToString("x2") & ID3Frame.FrameFlags(0).ToString("x2")))
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
      Dim lFrameSize As Integer = CLng("&H00" & ID3Frame.FrameSize(0).ToString("x2") & ID3Frame.FrameSize(1).ToString("x2") & ID3Frame.FrameSize(2).ToString("x2"))
      If lFrameSize < ioReader.BaseStream.Length And lFrameSize > 0 Then
        Dim bFrameData As Byte() = ioReader.ReadBytes(lFrameSize)
        AddBareFrame(ID3Frame.FrameName, bFrameData)
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
      Case "TDOR" : Return "TOR"
      Case "TOAL" : Return "TOT"
      Case "TPE1" : Return "TP1"
      Case "TPE2" : Return "TP2"
      Case "TPE3" : Return "TP3"
      Case "TPE4" : Return "TP4"
      Case "TPOS" : Return "TPA"
      Case "TPUB" : Return "TPB"
      Case "TSRC" : Return "TRC"
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
    If Not My.Computer.FileSystem.FileExists(sFile) Then Return
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
    ElseIf sData.Contains(vbNullChar) Or sData.Contains(";") Or sData.Contains(",") Or sData.Contains("|") Then
      Dim gSplit As String = sData.Replace(vbNullChar, "|")
      gSplit = gSplit.Replace(";", "|")
      gSplit = gSplit.Replace(",", "|")
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

  Public Function ParseFrame(sName As String, bData As Byte()) As ParseResponse
    Select Case sName
      Case "UFI", "UFID", "PRIV"
        Dim ioData As New IO.BinaryReader(New IO.MemoryStream(bData))
        Dim sOwner As String = Nothing
        Dim bTmp As Byte
        Do
          bTmp = ioData.ReadByte
          If bTmp > 0 Then sOwner &= ChrW(bTmp)
        Loop Until bTmp = 0
        If String.IsNullOrEmpty(sOwner) Then Return Nothing
        Dim Ident As Byte() = ioData.ReadBytes(bData.Length - (sOwner.Length + 1))
        Return New Parsed_UFID(sName, sOwner, Ident)
      Case "TXX", "TXXX"
        Dim bEncoding As TextEncoding = bData(0)
        Dim bParts As Byte()() = SplitByteArrayStrings(bData, 1, bEncoding, True)
        Dim sDesc As String = ParseString(bEncoding, bParts(0), 0, False)
        Dim sValue As String = ParseString(bEncoding, bParts(1), 0, True)
        If String.IsNullOrEmpty(sDesc) Or sDesc = ChrW(&HFEFF) Then Return New Parsed_TXXX(sName, bEncoding, sValue)
        Return New Parsed_TXXX(sName, bEncoding, sDesc, sValue)
      Case "TCO", "TCON"
        Dim bEncoding As TextEncoding = TextEncoding.NT_ISO
        Dim sData As String() = ParseGenreFrame(bData, bENcoding)
        If sData Is Nothing Then Return New Parse_Failure(sName, "Empty Genre Field!")
        If sData.Length = 1 Then Return New Parsed_TCON(sName, bEncoding, sData(0))
        Return New Parsed_TCON(sName, bEncoding, sData)
      Case "MCI", "MCDI" : Return New Parsed_MCDI(sName, bData)
      Case "COM", "COMM"
        If bData.Length < 5 Then Return New Parse_Failure(sName, "Less than five bytes to read!")
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
          Return New Parse_Failure(sName, "Unknown Comment Data!")
        End If
      Case "PIC"
        If bData.Length < 6 Then Return New Parse_Failure(sName, "Less than six bytes to read!")
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
          Return New Parse_Failure(sName, "Unknown Picture Data!")
        End If
      Case "APIC"
        If bData.Length < 3 Then Return New Parse_Failure(sName, "Less than three bytes to read!")
        Dim bEnc As TextEncoding = bData(0)
        Dim bBlobs As Byte()() = SplitByteArrayStrings(bData, 1, TextEncoding.NT_ISO, False)
        If Not bBlobs.Length = 2 Then
          Return New Parse_Failure(sName, "Missing Null Terminator")
        End If
        Dim bPicMIME As ID3_PIC_MIME = StrToMIME(fileEncoding.GetString(bBlobs(0)), False)
        If bBlobs(1).Length = 0 Then
          Return New Parse_Failure(sName, "Ran out of data after MIME")
        End If
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
            Debug.Print("Unknown Character: " & AscW(sDesc(I)) & """" & sDesc(I) & """")
            sDesc = Nothing
            Exit For
          Next
        End If
        Return New Parsed_APIC(sName, bEnc, bPicMIME, sDesc, bPicType, bBlobs2(1))
      Case "ULT", "USLT"
        If bData.Length < 5 Then Return New Parse_Failure(sName, "Less than five bytes to read!")
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
          Return New Parse_Failure(sName, "Unknown Lyric Data!")
        End If
      Case "WXX", "WXXX"
        If bData.Length < 5 Then Return New Parse_Failure(sName, "Less than five bytes to read!")
        Dim bEncoding As TextEncoding = bData(0)
        Dim bBlobs As Byte()() = SplitByteArrayStrings(bData, 1, bEncoding, True)
        If bBlobs.Length = 1 Then
          Dim sDec As String = ParseString(bEncoding, bBlobs(0), 0, True)
          Return New Parsed_TXXX(sName, bEncoding, sDec)
        ElseIf bBlobs.Length = 2 Then
          Dim sDesc As String = ParseString(bEncoding, bBlobs(0), 0, True)
          Dim sComm As String = ParseString(TextEncoding.NT_ISO, bBlobs(1), 0, False)
          Return New Parsed_TXXX(sName, bEncoding, sDesc, sComm)
        Else
          Return New Parse_Failure(sName, "Unknown Link Data!")
        End If
      Case "GEO", "GEOB"
        Dim bEncoding As TextEncoding = bData(0)
        Dim bBlob1 As Byte()() = SplitByteArrayStrings(bData, 1, TextEncoding.NT_ISO, False)
        If Not bBlob1.Length = 2 Then
          Return New Parse_Failure(sName, "Unknown Encapsulated Object Data!")
        End If
        Dim sMIME As String = fileEncoding.GetString(bBlob1(0))
        Dim bBlob2 As Byte()() = SplitByteArrayStrings(bBlob1(1), 0, bEncoding, True)
        If Not bBlob2.Length = 2 Then
          Return New Parse_Failure(sName, "Unknown Encapsulated Object Data!")
        End If
        Dim sFile As String = ParseString(bEncoding, bBlob2(0), 0, False)
        Dim bBlob3 As Byte()() = SplitByteArrayStrings(bBlob2(1), 0, bEncoding, True)
        If Not bBlob3.Length = 2 Then
          Return New Parsed_GEOB(sName, bEncoding, sMIME, sFile, Nothing, bBlob2(1))
        End If
        Dim sDesc As String = ParseString(bEncoding, bBlob3(0), 0, True)
        Return New Parsed_GEOB(sName, bEncoding, sMIME, sFile, sDesc, bBlob3(1))
      Case Else
        If sName.StartsWith("T") Or sName.StartsWith("W") Then
          If bData(0) < 4 Then
            Dim bEncoding As TextEncoding = bData(0)
            Dim sBlob As String = ParseString(bEncoding, bData, 1, True)
            Return New Parsed_TZZZ(sName, bEncoding, sBlob)
          Else
            Dim bEncoding As TextEncoding = TextEncoding.NT_ISO
            Dim sText As String = Nothing
            If bData.Length > 1 Then
              If bData(0) = &HFF And bData(1) = &HFE Then
                Debug.Print("LittleEndian Byte Order Mark")
                bEncoding = TextEncoding.UTF_16_LE
                If bData.Length = 3 AndAlso bData(2) = 0 Then
                  sText = Nothing
                Else
                  sText = ParseString(bEncoding, bData, 2, False)
                End If
              ElseIf bData(0) = &HFE And bData(1) = &HFF Then
                Debug.Print("BigEndian Byte Order Mark")
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
        Else
          If bData(0) < 4 Then
            Dim bEncoding As TextEncoding = bData(0)
            Dim sDec As String = ParseString(bEncoding, bData, 1, True)
            Dim isANPWS As Boolean = True
            If String.IsNullOrEmpty(sDec) Then
              isANPWS = False
            ElseIf sDec.Contains("[NULL]") Then
              isANPWS = False
            Else
              If bEncoding = TextEncoding.NT_ISO Or bEncoding = TextEncoding.UTF_8 Then
                If sDec.Length < bData.Length - 4 Then isANPWS = False
              ElseIf bEncoding = TextEncoding.UTF_16_LE Or bEncoding = TextEncoding.UTF_16_BE Then
                If sDec.Length < Math.Floor((bData.Length - 4) / 2) Then isANPWS = False
              End If
              For C As Integer = 0 To sDec.Length - 1
                If Not IsSimpleChar(sDec, C) Then
                  isANPWS = False
                  Exit For
                End If
              Next
            End If
            If isANPWS Then Return New Parsed_TZZZ(sName, bEncoding, sDec)
            Return New Parse_Unparsed(sName, bData)
          Else
            Return New Parse_Unparsed(sName, bData)
          End If
        End If
    End Select
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
            Debug.Print("LittleEndian Byte Order Mark in what should be an NT_ISO string!")
            If AllowChange Then Encoding = TextEncoding.UTF_16_LE
            If bBlob.Count = 1 AndAlso bBlob(0) = 0 Then
              sText = Nothing
            Else
              sText = System.Text.Encoding.Unicode.GetString(bBlob.ToArray)
            End If
          ElseIf bBlob(0) = &HFE And bBlob(1) = &HFF Then
            bBlob.RemoveRange(0, 2)
            Debug.Print("BigEndian Byte Order Mark in what should be an NT_ISO string!")
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
            Debug.Print("LittleEndian Byte Order Mark")
            If bBlob.Count = 1 AndAlso bBlob(0) = 0 Then
              sText = Nothing
            Else
              sText = System.Text.Encoding.Unicode.GetString(bBlob.ToArray)
            End If
          ElseIf bBlob(0) = &HFE And bBlob(1) = &HFF Then
            bBlob.RemoveRange(0, 2)
            Debug.Print("BigEndian Byte Order Mark")
            If AllowChange Then Encoding = TextEncoding.UTF_16_BE
            If bBlob.Count = 1 AndAlso bBlob(0) = 0 Then
              sText = Nothing
            Else
              sText = System.Text.Encoding.BigEndianUnicode.GetString(bBlob.ToArray)
            End If
          Else
            Debug.Print("Assuming LittleEndian Byte Order Mark")
            sText = System.Text.Encoding.Unicode.GetString(bBlob.ToArray)
          End If
        Else
          Debug.Print("Assuming LittleEndian Byte Order Mark")
          sText = System.Text.Encoding.Unicode.GetString(bBlob.ToArray)
        End If
      Case TextEncoding.UTF_16_BE
        If bBlob.Count > 1 Then
          If bBlob(0) = &HFE And bBlob(1) = &HFF Then
            Debug.Print("Byte Order Mark in UTF-16-BigEndian! This shouldn't be here!")
            bBlob.RemoveRange(0, 2)
          End If
        End If
        If bBlob.Count = 1 AndAlso bBlob(0) = 0 Then
          sText = Nothing
        Else
          sText = System.Text.Encoding.BigEndianUnicode.GetString(bBlob.ToArray)
        End If
      Case TextEncoding.UTF_8
        If bBlob.Count > 1 Then
          If (bBlob(0) = &HFF And bBlob(1) = &HFE) Or (bBlob(0) = &HFE And bBlob(1) = &HFF) Then
            Debug.Print("Byte Order Mark in UTF-8! MADNESS!!!!")
            bBlob.RemoveRange(0, 2)
          End If
        End If
        If bBlob.Count = 1 AndAlso bBlob(0) = 0 Then
          sText = Nothing
        Else
          sText = System.Text.Encoding.UTF8.GetString(bBlob.ToArray)
        End If
      Case Else
        'Probably not encoded...
        'Debug.Print("Unknown Content encoding: " & Encoding & " [" & Text & "]!")
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
      If Char.IsLetterOrDigit(sText, I) Or Char.IsWhiteSpace(sText, I) Or Char.IsPunctuation(sText, I) Or Char.IsSymbol(sText, I) Then
        sNewText &= sText(I)
      Else
        'Debug.Print("Unknown Character: " & AscW(sText(I)) & """" & sText(I) & """")
      End If
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
    Using ID3Data As New IO.FileStream(SaveAs, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.Read)
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
              ElseIf ID3Header.Version(0) = 3 Then
                sName = GetNewName(sName, 3)
              ElseIf ID3Header.Version(0) = 4 Then
                sName = GetNewName(sName, 4)
              End If
              If (FrameFlags(I) And FFLG.TAGALTER) = FFLG.TAGALTER Or (FrameFlags(I) And FFLG.FILEALTER) = FFLG.FILEALTER Then Continue For
              Dim fData As ParseResponse = ParseFrame(sName, FrameData(I))
              If fData Is Nothing OrElse fData.GetType = GetType(Parse_Failure) Then
                Debug.Print("Unhandled Frame:")
                Debug.Print("Frame Name: " & sName)
                Debug.Print("Frame Data: " & fileEncoding.GetString(FrameData(I)))
                Debug.Print("Frame Data: " & BitConverter.ToString(FrameData(I)))
                Return False
              End If
              Select Case sName
                Case "WXXX"
                  Dim mData As Parsed_TXXX = fData
                  Dim bData As New List(Of Byte)
                  bData.AddRange(MakeFrameStringData(mData.Encoding, mData.Description, ID3Header.Version(0)))
                  bData.AddRange(fileEncoding.GetBytes(mData.Value)) 'no null
                  WriteFrame(ID3ContentWriter, sName, FrameFlags(I), bData.ToArray)
                Case Else
                  If sName.StartsWith("W") Then
                    Dim mData As Parsed_TZZZ = fData
                    Dim bData As New List(Of Byte)
                    bData.AddRange(MakeFrameStringData(mData.Encoding, mData.Value, ID3Header.Version(0)))
                    WriteFrame(ID3ContentWriter, sName, FrameFlags(I), bData.ToArray)
                  Else
                    Select Case fData.GetType
                      Case GetType(Parsed_APIC)
                        Dim mData As Parsed_APIC = fData
                        If sName = "PIC" Then
                          Dim bData As New List(Of Byte)
                          bData.Add(mData.Encoding)
                          bData.AddRange(fileEncoding.GetBytes(MIMEtoString(mData.MIME, True))) 'no null
                          bData.Add(mData.Type)
                          bData.AddRange(MakeFrameStringData(mData.Encoding, mData.Description, ID3Header.Version(0), True))
                          bData.AddRange(mData.Image)
                          WriteFrame(ID3ContentWriter, sName, FrameFlags(I), bData.ToArray)
                        ElseIf sName = "APIC" Then
                          Dim bData As New List(Of Byte)
                          bData.Add(mData.Encoding)
                          bData.AddRange(fileEncoding.GetBytes(MIMEtoString(mData.MIME, False)))
                          bData.Add(0)
                          bData.Add(mData.Type)
                          bData.AddRange(MakeFrameStringData(mData.Encoding, mData.Description, ID3Header.Version(0), True))
                          bData.AddRange(mData.Image)
                          WriteFrame(ID3ContentWriter, sName, FrameFlags(I), bData.ToArray)
                        Else
                          Debug.Print("Unhandled Frame:")
                          Debug.Print("Frame Name: " & sName)
                          Debug.Print("Frame Data: " & fileEncoding.GetString(FrameData(I)))
                          Debug.Print("Frame Data: " & BitConverter.ToString(FrameData(I)))
                          Return False
                        End If
                      Case GetType(Parsed_COMM)
                        Dim mData As Parsed_COMM = fData
                        Dim bData As New List(Of Byte)
                        bData.Add(mData.Encoding)
                        bData.AddRange(fileEncoding.GetBytes(mData.Language)) 'no null
                        bData.AddRange(MakeFrameStringData(mData.Encoding, mData.Description, ID3Header.Version(0), True))
                        bData.AddRange(MakeFrameStringData(mData.Encoding, mData.Comment, ID3Header.Version(0), True))
                        WriteFrame(ID3ContentWriter, sName, FrameFlags(I), bData.ToArray)
                      Case GetType(Parsed_GEOB)
                        Dim mData As Parsed_GEOB = fData
                        Dim bData As New List(Of Byte)
                        bData.Add(mData.Encoding)
                        bData.AddRange(fileEncoding.GetBytes(mData.MIME))
                        bData.Add(0)
                        bData.AddRange(MakeFrameStringData(mData.Encoding, mData.Filename, ID3Header.Version(0), True))
                        bData.AddRange(MakeFrameStringData(mData.Encoding, mData.Description, ID3Header.Version(0), True))
                        bData.AddRange(mData.Content)
                        WriteFrame(ID3ContentWriter, sName, FrameFlags(I), bData.ToArray)
                      Case GetType(Parsed_MCDI)
                        Dim mData As Parsed_MCDI = fData
                        WriteFrame(ID3ContentWriter, sName, FrameFlags(I), mData.TOC)
                      Case GetType(Parsed_TCON)
                        Dim mData As Parsed_TCON = fData
                        For Each gEntity As String In mData.GenreList
                          Dim gID As String = gEntity
                          For g As Integer = &H0 To &HBF
                            If StrComp(gEntity, clsID3v1.GenreName(g), CompareMethod.Text) = 0 Then
                              gID = g
                              Exit For
                            End If
                          Next
                          Dim bEntity As Byte() = Nothing
                          If Not ID3Header.Version(0) = 4 Then
                            If IsNumeric(gID) Then gID = "(" & gID & ")"
                            bEntity = MakeFrameStringData(mData.Encoding, gID, ID3Header.Version(0))
                          Else
                            bEntity = MakeFrameStringData(mData.Encoding, gEntity, ID3Header.Version(0))
                          End If
                          WriteFrame(ID3ContentWriter, sName, FrameFlags(I), bEntity)
                        Next
                      Case GetType(Parsed_TXXX)
                        Dim mData As Parsed_TXXX = fData
                        Dim bData As New List(Of Byte)
                        bData.AddRange(MakeFrameStringData(mData.Encoding, mData.Description, ID3Header.Version(0)))
                        bData.AddRange(MakeFrameStringData(mData.Encoding, mData.Value, ID3Header.Version(0), True))
                        WriteFrame(ID3ContentWriter, sName, FrameFlags(I), bData.ToArray)
                      Case GetType(Parsed_TZZZ)
                        Dim mData As Parsed_TZZZ = fData
                        Dim bData As New List(Of Byte)
                        bData.AddRange(MakeFrameStringData(mData.Encoding, mData.Value, ID3Header.Version(0)))
                        WriteFrame(ID3ContentWriter, sName, FrameFlags(I), bData.ToArray)
                      Case GetType(Parsed_UFID)
                        Dim mData As Parsed_UFID = fData
                        Dim bData As New List(Of Byte)
                        bData.AddRange(fileEncoding.GetBytes(mData.Owner))
                        bData.Add(0)
                        bData.AddRange(mData.Ident)
                        WriteFrame(ID3ContentWriter, sName, FrameFlags(I), bData.ToArray)
                      Case GetType(Parse_Unparsed)
                        Dim mData As Parse_Unparsed = fData
                        WriteFrame(ID3ContentWriter, sName, FrameFlags(I), mData.Data)
                      Case Else
                        Debug.Print("Unhandled Frame:")
                        Debug.Print("Frame Name: " & sName)
                        Debug.Print("Frame Data: " & fileEncoding.GetString(FrameData(I)))
                        Debug.Print("Frame Data: " & BitConverter.ToString(FrameData(I)))
                        Return False
                    End Select
                  End If
              End Select
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
    Return True
  End Function

  Private Function TrimID3v2(Path As String) As Byte()
    If String.IsNullOrEmpty(Path) Then Return Nothing
    If Not My.Computer.FileSystem.FileExists(Path) Then Return Nothing
    If My.Computer.FileSystem.GetFileInfo(Path).Length >= 1024L * 1024L * 1024L * 4L Then Return Nothing
    Dim bFile As Byte() = My.Computer.FileSystem.ReadAllBytes(Path)
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
