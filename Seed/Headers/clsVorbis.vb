Public Class clsVorbis
  Implements IDisposable

  Private Class BitReader
    Implements IDisposable
    Private bIn() As Byte
    Private lPos As ULong
    Private bitData As BitArray

    Public Property Position As ULong
      Get
        Return lPos
      End Get
      Set(value As ULong)
        lPos = value
      End Set
    End Property

    Public ReadOnly Property GetAllData() As Byte()
      Get
        Return bIn
      End Get
    End Property

    Public Sub New(array As Byte())
      bIn = array
      bitData = FlipBits(bIn)
      lPos = 0
    End Sub

    Private Function FlipBits(Bytes() As Byte) As BitArray
      Dim bTmp(Bytes.Length - 1) As Byte
      For I As Integer = 0 To Bytes.Length - 1
        For J As Integer = 0 To 7
          If ((Bytes(I) And CByte(1 << J)) <> 0) Then bTmp(I) += CByte(1 << (7 - J))
        Next
      Next
      Return New BitArray(bTmp)
    End Function

    Public Sub Append(array As Byte())
      If bIn Is Nothing Then
        bIn = array
      Else
        Dim oldLen As Long = bIn.LongLength
        ReDim Preserve bIn(oldLen + array.Length - 1)
        For I As Integer = 0 To array.Length - 1
          bIn(oldLen + I) = array(I)
        Next
      End If
      bitData = FlipBits(bIn)
    End Sub

    Public Function ReadNumber(Bits As UInt64) As ULong
      If Bits > 64 Then
        Throw New ArgumentException("Can not read over 64 bits", "Bits")
        Return 0
      End If
      Dim lLoc As UInt64 = lPos + Bits
      If lLoc > bitData.Length Then
        Throw New ArgumentException("Out of buffered data.")
        Return 0
      End If
      Dim uRet As ULong
      For I As Integer = 0 To Bits - 1
        If bitData(lPos + I) Then uRet = uRet Or (1UL << (Bits - (I + 1)))
      Next
      lPos += Bits
      Return uRet
    End Function

    Public Function ReadBytes(Bytes As UInt64) As Byte()
      Dim lLoc As UInt64 = lPos + (Bytes * 8)
      If lLoc > bitData.Length Then
        Throw New ArgumentException("Out of buffered data.")
        Return Nothing
      End If
      Dim bTmp(Bytes - 1) As Byte
      Array.ConstrainedCopy(bIn, Math.Floor(lPos / 8), bTmp, 0, Bytes)
      lPos = lLoc
      Return bTmp
    End Function

    Public Function ReadChars(Chars As Integer) As String
      Dim bTmp() As Byte = ReadBytes(Chars)
      Return System.Text.Encoding.ASCII.GetString(bTmp)
    End Function

    Public Function ReadChars(Chars As Integer, CodePage As Integer) As String
      Dim bTmp() As Byte = ReadBytes(Chars)
      Return System.Text.Encoding.GetEncoding(CodePage).GetString(bTmp)
    End Function

    Public Function ReadDWord() As String
      'Dim val As ULong = ReadNumber(32)
      Dim sTmp As String = System.Text.Encoding.ASCII.GetString(ReadBytes(4))
      sTmp = sTmp.Substring(sTmp.LastIndexOf(vbNullChar) + 1)
      Return sTmp
    End Function

    Public Shared Function ToByteArray(bits As BitArray) As Byte()
      Dim numBytes As Integer = bits.Count / 8
      If bits.Count Mod 8 <> 0 Then numBytes += 1
      Dim bytes As Byte() = New Byte(numBytes - 1) {}
      Dim byteIndex As Integer = 0, bitIndex As Integer = 0
      For i As Integer = 0 To bits.Count - 1
        If bits(i) Then bytes(byteIndex) = bytes(byteIndex) Or CByte(1 << (7 - bitIndex))
        bitIndex += 1
        If bitIndex = 8 Then
          bitIndex = 0
          byteIndex += 1
        End If
      Next
      Return bytes
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

  Private crcLookup() As UInt32 = {&H0UI, &H4C11DB7UI, &H9823B6EUI, &HD4326D9UI, &H130476DCUI, &H17C56B6BUI, &H1A864DB2UI, &H1E475005UI, &H2608EDB8UI, &H22C9F00FUI, &H2F8AD6D6UI, &H2B4BCB61UI, &H350C9B64UI, &H31CD86D3UI, &H3C8EA00AUI, &H384FBDBDUI, &H4C11DB70UI, &H48D0C6C7UI, &H4593E01EUI, &H4152FDA9UI, &H5F15ADACUI, &H5BD4B01BUI, &H569796C2UI, &H52568B75UI, &H6A1936C8UI, &H6ED82B7FUI, &H639B0DA6UI, &H675A1011UI, &H791D4014UI, &H7DDC5DA3UI, &H709F7B7AUI, &H745E66CDUI, &H9823B6E0UI, &H9CE2AB57UI, &H91A18D8EUI, &H95609039UI, &H8B27C03CUI, &H8FE6DD8BUI, &H82A5FB52UI, &H8664E6E5UI, &HBE2B5B58UI, &HBAEA46EFUI, &HB7A96036UI, &HB3687D81UI, &HAD2F2D84UI, &HA9EE3033UI, &HA4AD16EAUI, &HA06C0B5DUI, &HD4326D90UI, &HD0F37027UI, &HDDB056FEUI, &HD9714B49UI, &HC7361B4CUI, &HC3F706FBUI, &HCEB42022UI, &HCA753D95UI, &HF23A8028UI, &HF6FB9D9FUI, &HFBB8BB46UI, &HFF79A6F1UI, &HE13EF6F4UI, &HE5FFEB43UI, &HE8BCCD9AUI, &HEC7DD02DUI, &H34867077UI, &H30476DC0UI, &H3D044B19UI, &H39C556AEUI, &H278206ABUI, &H23431B1CUI, &H2E003DC5UI, &H2AC12072UI, &H128E9DCFUI, &H164F8078UI, &H1B0CA6A1UI, &H1FCDBB16UI, &H18AEB13UI, &H54BF6A4UI, &H808D07DUI, &HCC9CDCAUI, &H7897AB07UI, &H7C56B6B0UI, &H71159069UI, &H75D48DDEUI, &H6B93DDDBUI, &H6F52C06CUI, &H6211E6B5UI, &H66D0FB02UI, &H5E9F46BFUI, &H5A5E5B08UI, &H571D7DD1UI, &H53DC6066UI, &H4D9B3063UI, &H495A2DD4UI, &H44190B0DUI, &H40D816BAUI, &HACA5C697UI, &HA864DB20UI, &HA527FDF9UI, &HA1E6E04EUI, &HBFA1B04BUI, &HBB60ADFCUI, &HB6238B25UI, &HB2E29692UI, &H8AAD2B2FUI, &H8E6C3698UI, &H832F1041UI, &H87EE0DF6UI, &H99A95DF3UI, &H9D684044UI, &H902B669DUI, &H94EA7B2AUI, &HE0B41DE7UI, &HE4750050UI, &HE9362689UI, &HEDF73B3EUI, &HF3B06B3BUI, &HF771768CUI, &HFA325055UI, &HFEF34DE2UI, &HC6BCF05FUI, &HC27DEDE8UI, &HCF3ECB31UI, &HCBFFD686UI, &HD5B88683UI, &HD1799B34UI, &HDC3ABDEDUI, &HD8FBA05AUI, &H690CE0EEUI, &H6DCDFD59UI, &H608EDB80UI, &H644FC637UI, &H7A089632UI, &H7EC98B85UI, &H738AAD5CUI, &H774BB0EBUI, &H4F040D56UI, &H4BC510E1UI, &H46863638UI, &H42472B8FUI, &H5C007B8AUI, &H58C1663DUI, &H558240E4UI, &H51435D53UI, &H251D3B9EUI, &H21DC2629UI, &H2C9F00F0UI, &H285E1D47UI, &H36194D42UI, &H32D850F5UI, &H3F9B762CUI, &H3B5A6B9BUI, &H315D626UI, &H7D4CB91UI, &HA97ED48UI, &HE56F0FFUI, &H1011A0FAUI, &H14D0BD4DUI, &H19939B94UI, &H1D528623UI, &HF12F560EUI, &HF5EE4BB9UI, &HF8AD6D60UI, &HFC6C70D7UI, &HE22B20D2UI, &HE6EA3D65UI, &HEBA91BBCUI, &HEF68060BUI, &HD727BBB6UI, &HD3E6A601UI, &HDEA580D8UI, &HDA649D6FUI, &HC423CD6AUI, &HC0E2D0DDUI, &HCDA1F604UI, &HC960EBB3UI, &HBD3E8D7EUI, &HB9FF90C9UI, &HB4BCB610UI, &HB07DABA7UI, &HAE3AFBA2UI, &HAAFBE615UI, &HA7B8C0CCUI, &HA379DD7BUI, &H9B3660C6UI, &H9FF77D71UI, &H92B45BA8UI, &H9675461FUI, &H8832161AUI, &H8CF30BADUI, &H81B02D74UI, &H857130C3UI, &H5D8A9099UI, &H594B8D2EUI, &H5408ABF7UI, &H50C9B640UI, &H4E8EE645UI, &H4A4FFBF2UI, &H470CDD2BUI, &H43CDC09CUI, &H7B827D21UI, &H7F436096UI, &H7200464FUI, &H76C15BF8UI, &H68860BFDUI, &H6C47164AUI, &H61043093UI, &H65C52D24UI, &H119B4BE9UI, &H155A565EUI, &H18197087UI, &H1CD86D30UI, &H29F3D35UI, &H65E2082UI, &HB1D065BUI, &HFDC1BECUI, &H3793A651UI, &H3352BBE6UI, &H3E119D3FUI, &H3AD08088UI, &H2497D08DUI, &H2056CD3AUI, &H2D15EBE3UI, &H29D4F654UI, &HC5A92679UI, &HC1683BCEUI, &HCC2B1D17UI, &HC8EA00A0UI, &HD6AD50A5UI, &HD26C4D12UI, &HDF2F6BCBUI, &HDBEE767CUI, &HE3A1CBC1UI, &HE760D676UI, &HEA23F0AFUI, &HEEE2ED18UI, &HF0A5BD1DUI, &HF464A0AAUI, &HF9278673UI, &HFDE69BC4UI, &H89B8FD09UI, &H8D79E0BEUI, &H803AC667UI, &H84FBDBD0UI, &H9ABC8BD5UI, &H9E7D9662UI, &H933EB0BBUI, &H97FFAD0CUI, &HAFB010B1UI, &HAB710D06UI, &HA6322BDFUI, &HA2F33668UI, &HBCB4666DUI, &HB8757BDAUI, &HB5365D03UI, &HB1F740B4UI}

  Private Function CRC32(inData() As Byte) As UInt32
    Dim crc_reg As UInt32 = 0
    For I As Integer = 0 To inData.Length - 1
      Dim b As Byte = inData(I)
      crc_reg = (crc_reg << 8) Xor crcLookup(((crc_reg >> 24) And &HFF) Xor b)
    Next
    Return crc_reg
  End Function

  Private sFile As String
  Private sTitle As String
  Private sVersion As String
  Private sAlbum As String
  Private sTrack As String
  Private sArtist As String
  Private sPerformer As String
  Private sCopyright As String
  Private sLicense As String
  Private sLabel As String
  Private sDescription As String
  Private sGenre As String
  Private sDate As String
  Private sLocation As String
  Private sContact As String
  Private sISRC As String
  Private sOther() As String
  Private lVorbDuration As Double
  Private lVorbVer As Long
  Private bVorbChannels As Byte
  Private lVorbRate As Long
  Private lVorbQuality, lVorbQMin, lVorbQMax As Integer
  Private sVendor As String
  Private bolHasVorb As Boolean
  Private pctImages() As FLAC_PIC
  Private lHeaderLength As Long

  Public Structure FLAC_PIC
    Public MIME As String
    Public Descr As String
    Public PicType As Byte
    Public Size As Drawing.Size
    Public ColorDepth As UInt32
    Public ColorIndexes As UInt32
    Public Image As Drawing.Image
  End Structure

  Private Structure IDHeader
    Public Version As UInt32
    Public Channels As Byte
    Public SampleRate As UInt32
    Public MaxRate As Int32
    Public NomRate As Int32
    Public MinRate As Int32
    Public Block As Integer
    Public FrameFlag As Byte
  End Structure

  Private Enum Header_Type_Flags
    Continued = &H1
    FirstPage = &H2
    LastPage = &H4
  End Enum

  Private Structure Page
    Public Header As Long
    Public Version As Byte
    Public TypeFlag As Header_Type_Flags
    Public GranulePosition As ULong
    Public Serial As Long
    Public Sequence As Long
    Public Checksum As Long
    Public Segments As Byte
    Public Segs() As Byte
    Public HeadLen As Long
    Public Length As Long
  End Structure

  Public ReadOnly Property Album() As String
    Get
      Return sAlbum
    End Get
  End Property

  Public ReadOnly Property Artist() As String
    Get
      Return sArtist
    End Get
  End Property

  Public ReadOnly Property Contact() As String
    Get
      Return sContact
    End Get
  End Property

  Public ReadOnly Property Copyright() As String
    Get
      Return sCopyright
    End Get
  End Property

  Public ReadOnly Property Description() As String
    Get
      Return sDescription
    End Get
  End Property

  Public ReadOnly Property File_Channels() As Byte
    Get
      Return bVorbChannels
    End Get
  End Property

  Public ReadOnly Property File_Rate() As Long
    Get
      Return lVorbRate
    End Get
  End Property

  Public ReadOnly Property File_Quality As Integer
    Get
      Return lVorbQuality
    End Get
  End Property

  Public ReadOnly Property File_MinQuality As Integer
    Get
      Return lVorbQMin
    End Get
  End Property

  Public ReadOnly Property File_MaxQuality As Integer
    Get
      Return lVorbQMax
    End Get
  End Property

  Public ReadOnly Property File_Vendor() As String
    Get
      Return sVendor
    End Get
  End Property

  Public ReadOnly Property File_Version() As Long
    Get
      Return lVorbVer
    End Get
  End Property

  Public ReadOnly Property File_Duration() As Double
    Get
      Return lVorbDuration
    End Get
  End Property

  Private Sub FillInData(Raw As String)
    Dim Field As String
    Dim Data As String
    Dim I As Long
    Dim bDid As Boolean
    Field = Split(Raw, "=", 2)(0)
    Data = Split(Raw, "=", 2)(1) ' UTF8ToANSI(Mid$(Raw, Len(Field) + 2))
    Select Case UCase$(Field)
      Case "TITLE"
        If Not String.IsNullOrEmpty(sTitle) Then
          sTitle = sTitle & ", " & Data
        Else
          sTitle = Data
        End If
      Case "VERSION"
        If Not String.IsNullOrEmpty(sVersion) Then
          sVersion = sVersion & ", " & Data
        Else
          sVersion = Data
        End If
      Case "ALBUM"
        If Not String.IsNullOrEmpty(sAlbum) Then
          sAlbum = sAlbum & ", " & Data
        Else
          sAlbum = Data
        End If
      Case "TRACKNUMBER"
        If Not String.IsNullOrEmpty(sTrack) Then
          sTrack = sTrack & ", " & Data
        Else
          sTrack = Data
        End If
      Case "ARTIST"
        If Not String.IsNullOrEmpty(sArtist) Then
          sArtist = sArtist & ", " & Data
        Else
          sArtist = Data
        End If
      Case "PERFORMER"
        If Not String.IsNullOrEmpty(sPerformer) Then
          sPerformer = sPerformer & ", " & Data
        Else
          sPerformer = Data
        End If
      Case "COPYRIGHT"
        If Not String.IsNullOrEmpty(sCopyright) Then
          sCopyright = sCopyright & ", " & Data
        Else
          sCopyright = Data
        End If
      Case "LICENSE"
        If Not String.IsNullOrEmpty(sLicense) Then
          sLicense = sLicense & ", " & Data
        Else
          sLicense = Data
        End If
      Case "ORGANIZATION"
        If Not String.IsNullOrEmpty(sLabel) Then
          sLabel = sLabel & ", " & Data
        Else
          sLabel = Data
        End If
      Case "DESCRIPTION"
        If Not String.IsNullOrEmpty(sDescription) Then
          sDescription = sDescription & ", " & Data
        Else
          sDescription = Data
        End If
      Case "GENRE"
        If Not String.IsNullOrEmpty(sGenre) Then
          sGenre = sGenre & ", " & Data
        Else
          sGenre = Data
        End If
      Case "DATE"
        If Not String.IsNullOrEmpty(sDate) Then
          sDate = sDate & ", " & Data
        Else
          sDate = Data
        End If
      Case "LOCATION"
        If Not String.IsNullOrEmpty(sLocation) Then
          sLocation = sLocation & ", " & Data
        Else
          sLocation = Data
        End If
      Case "CONTACT"
        If Not String.IsNullOrEmpty(sContact) Then
          sContact = sContact & ", " & Data
        Else
          sContact = Data
        End If
      Case "ISRC"
        If Not String.IsNullOrEmpty(sISRC) Then
          sISRC = sISRC & ", " & Data
        Else
          sISRC = Data
        End If
      Case Else
        bDid = False
        If sOther IsNot Nothing Then
          For I = 0 To UBound(sOther)
            If UCase(Left$(sOther(I), InStr(sOther(I), ":") - 1)) = UCase$(Field) Then
              sOther(I) = sOther(I) & ", " & Data
              bDid = True
              Exit For
            End If
          Next I
          If Not bDid Then
            ReDim Preserve sOther(UBound(sOther) + 1)
            sOther(UBound(sOther)) = Field & ": " & Data
          End If
        Else
          ReDim sOther(0)
          sOther(0) = Field & ": " & Data
        End If
        'Debug.Print "Unknown Field type: " & Field & "=" & Data
    End Select
  End Sub

  Public ReadOnly Property Genre() As String
    Get
      Return sGenre
    End Get
  End Property

  Public ReadOnly Property HasVorbis() As Boolean
    Get
      Return bolHasVorb
    End Get
  End Property

  Public ReadOnly Property ISRC() As String
    Get
      Return sISRC
    End Get
  End Property

  Public ReadOnly Property Label() As String
    Get
      Return sLabel
    End Get
  End Property

  Public ReadOnly Property License() As String
    Get
      Return sLicense
    End Get
  End Property

  Public ReadOnly Property Pictures() As FLAC_PIC()
    Get
      Return pctImages
    End Get
  End Property

  Private Sub LoadVorbis()
    Const H_OggS As UInt32 = &H4F676753
    Const H_fLaC As UInt32 = &H664C6143
    Dim ioFile As IO.FileStream = IO.File.Open(sFile, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
    Dim lStartPos As Long = -1
    If IO.Path.GetExtension(sFile).ToLower = ".flac" Then
      lStartPos = FindLONG(ioFile, H_fLaC)
      If lStartPos = -1 Then lStartPos = FindLONG(ioFile, H_OggS)
    Else
      lStartPos = FindLONG(ioFile, H_OggS)
      If lStartPos = -1 Then lStartPos = FindLONG(ioFile, H_fLaC)
    End If
    If lStartPos > -1 Then
      ioFile.Position = lStartPos
      Dim dHeader As UInt32 = ReadLONG(ioFile)
      Select Case dHeader
        Case H_OggS
          ioFile.Position -= 4
          Do
            dHeader = ReadLONG(ioFile)
            ioFile.Position -= 4
            If dHeader = H_OggS Then
              Dim PageChunk As Page = ReadPage(ioFile)
              Dim bPage(PageChunk.Length - 1) As Byte
              ioFile.Position -= PageChunk.HeadLen
              ioFile.Read(bPage, 0, PageChunk.Length)
              ioFile.Position -= PageChunk.Length - PageChunk.HeadLen
              bPage(22) = 0
              bPage(23) = 0
              bPage(24) = 0
              bPage(25) = 0
              Dim Checksum As UInt32 = CRC32(bPage)
              If Not PageChunk.Checksum = Checksum Then
                Debug.Print("Checksum Mismatch!")
                Stop
              End If
            End If
            Dim bChunkType As Byte = ioFile.ReadByte
            Select Case bChunkType
              Case 1 'Type
                Dim sChunkID As String = ReadString(ioFile, 6)
                Select Case sChunkID
                  Case "vorbis"
                    Dim ID As IDHeader
                    ID.Version = ReadDWORD(ioFile)
                    ID.Channels = ioFile.ReadByte
                    ID.SampleRate = ReadDWORD(ioFile)
                    ID.MaxRate = ReadINT(ioFile)
                    ID.NomRate = ReadINT(ioFile)
                    ID.MinRate = ReadINT(ioFile)
                    ID.Block = ioFile.ReadByte
                    ID.FrameFlag = ioFile.ReadByte
                    lVorbVer = ID.Version
                    lVorbRate = ID.SampleRate
                    bVorbChannels = ID.Channels
                    'Debug.Print(ID.MaxRate & " > " & ID.NomRate & " > " & ID.MinRate)
                    lVorbQuality = ID.NomRate
                    lVorbQMin = ID.MinRate
                    lVorbQMax = ID.MaxRate
                  Case "video" & vbNullChar
                    Dim bTmp As Byte = ioFile.ReadByte
                    Dim bTmp2 As Byte = ioFile.ReadByte
                    Dim sCodec As String = ReadString(ioFile, 8)
                    Dim Unknown1 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown2 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown3 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown4 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown5 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown6 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown7 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown8 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown9 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown10 As UInt32 = ReadDWORD(ioFile)
                  Case "text" & vbNullChar & vbNullChar
                    Dim bTmp As Byte = ioFile.ReadByte
                    Dim bTmp2 As Byte = ioFile.ReadByte
                    Dim sCodec As String = ReadString(ioFile, 8)
                    Dim Unknown1 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown2 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown3 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown4 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown5 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown6 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown7 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown8 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown9 As UInt32 = ReadDWORD(ioFile)
                    Dim Unknown10 As UInt32 = ReadDWORD(ioFile)
                  Case Else
                    Debug.Print("Unknown Type Chunk ID: " & sChunkID)
                    Exit Do
                End Select
              Case 3 'Comment
                Dim sChunkID As String = ReadString(ioFile, 6)
                Select Case sChunkID
                  Case "vorbis"
                    bolHasVorb = True
                    Dim venLen As Long = ReadDWORD(ioFile)
                    sVendor = ReadString(ioFile, venLen)
                    Dim lCom As UInt32 = ReadDWORD(ioFile)
                    For I As Integer = 1 To lCom
                      Dim dataLen As Long = ReadDWORD(ioFile)
                      Dim sData As String = ReadString(ioFile, dataLen, System.Text.Encoding.UTF8.CodePage)
                      FillInData(sData)
                    Next I
                    Dim endByte As Byte = ioFile.ReadByte
                    If endByte <> 1 Then Debug.Print("End byte: " & Hex(endByte))
                    'Exit Do
                  Case Else
                    Debug.Print("Unknown Comment Chunk ID: " & sChunkID)
                    Exit Do
                End Select
              Case 5 'Setup 
                'search for H_OggS and find the last instance of it
                If ioFile.Length > 65535 Then ioFile.Seek(-65535, IO.SeekOrigin.End)

                Dim lastPage As Long = ioFile.Position
                Do
                  Dim thisPage As Long = FindLONG(ioFile, H_OggS, lastPage)
                  ioFile.Position = thisPage
                  Dim thisData As Page = ReadPage(ioFile)
                  If FindLONG(ioFile, H_OggS, thisPage + 4) > 0 Then
                    lastPage = thisPage + 4
                    Continue Do
                  End If
                  If (thisData.TypeFlag And Header_Type_Flags.LastPage) = Header_Type_Flags.LastPage Then
                    If thisData.GranulePosition = &HFFFFFFFFFFFFFFFFUL Then
                      'No finish on last page?
                      lVorbDuration = 0
                      Stop
                      Exit Do
                    Else
                      Dim TotalSamples As UInt64 = thisData.GranulePosition
                      lVorbDuration = TotalSamples / lVorbRate
                      Exit Do
                    End If
                  Else
                    'last page is not last page?
                    lVorbDuration = 0
                    Stop
                    Exit Do
                  End If
                Loop Until ioFile.Position >= ioFile.Length
                Exit Do
              Case 0 'Padding
                Continue Do
              Case Else
                Debug.Print("Unknown Chunk Type: " & bChunkType)
                Exit Do
            End Select
          Loop While ioFile.Position < ioFile.Length
        Case H_fLaC
          Do
            Dim bTmp(3) As Byte
            ioFile.Read(bTmp, 0, 4)
            Using bRead As New BitReader(bTmp)
              Dim LastBlock As Boolean = bRead.ReadNumber(1) = 1
              Dim BlockType As Byte = bRead.ReadNumber(7)
              Dim BlockLen As UInt32 = bRead.ReadNumber(24)
              Dim bBlock(BlockLen - 1) As Byte
              ioFile.Read(bBlock, 0, BlockLen)
              bRead.Append(bBlock)
              Select Case BlockType
                Case 0 'streaminfo
                  Dim MinSamples As UInt16 = bRead.ReadNumber(16)
                  Dim MaxSamples As UInt16 = bRead.ReadNumber(16)
                  Dim MinFrames As UInt32 = bRead.ReadNumber(24)
                  Dim MaxFrames As UInt32 = bRead.ReadNumber(24)
                  Dim SampleRate As UInt32 = bRead.ReadNumber(20)
                  lVorbRate = SampleRate
                  Dim Channels As Byte = bRead.ReadNumber(3) + 1
                  bVorbChannels = Channels
                  Dim BitsPerSample As Byte = bRead.ReadNumber(5) + 1
                  Dim SamplesInStream As UInt64 = bRead.ReadNumber(36)
                  Dim MD5Sig() As Byte = bRead.ReadBytes(16)
                Case 1 'padding
                  Debug.Print(BitConverter.ToString(bRead.GetAllData))
                  Stop
                Case 2 'app
                  Dim AppID As UInt32 = bRead.ReadNumber(32)
                  Debug.Print(BitConverter.ToString(bRead.GetAllData))
                  Stop
                Case 3 'seektable
                  Dim SeekPoints As UInt32 = BlockLen / 18
                  For I As Integer = 0 To SeekPoints - 1
                    Dim FirstSample As UInt64 = bRead.ReadNumber(64)
                    Dim Offset As UInt64 = bRead.ReadNumber(64)
                    Dim SamplesInFrame As UInt16 = bRead.ReadNumber(16)
                  Next
                Case 4 'comment
                  bolHasVorb = True
                  Dim venLen As UInt32 = SwapEndian32(bRead.ReadNumber(32))
                  sVendor = bRead.ReadChars(venLen) ' ReadString(ioFile, venLen)
                  Dim lCom As UInt32 = SwapEndian32(bRead.ReadNumber(32))
                  For I As Integer = 1 To lCom
                    Dim dataLen As UInt32 = SwapEndian32(bRead.ReadNumber(32))
                    Dim sData As String = bRead.ReadChars(dataLen, System.Text.Encoding.UTF8.CodePage)
                    FillInData(sData)
                  Next I
                Case 5 'cuesheet
                  Debug.Print(BitConverter.ToString(bRead.GetAllData))
                  Stop
                Case 6 'picture
                  Dim PicType As UInt32 = bRead.ReadNumber(32)
                  Dim MIMELen As UInt32 = bRead.ReadNumber(32)
                  Dim MIMEType As String = bRead.ReadChars(MIMELen)
                  Dim DescLen As UInt32 = bRead.ReadNumber(32)
                  Dim sDesc As String = bRead.ReadChars(DescLen, System.Text.Encoding.UTF8.CodePage)
                  Dim Width As UInt32 = bRead.ReadNumber(32)
                  Dim Height As UInt32 = bRead.ReadNumber(32)
                  Dim Depth As UInt32 = bRead.ReadNumber(32)
                  Dim ColorIndex As UInt32 = bRead.ReadNumber(32)
                  Dim PicLen As UInt32 = bRead.ReadNumber(32)
                  Dim bPic() As Byte = bRead.ReadBytes(PicLen)
                  Dim imgPic As Drawing.Image = Nothing
                  Dim sTmpPath As String = IO.Path.GetTempFileName
                  My.Computer.FileSystem.WriteAllBytes(sTmpPath, bPic, False)
                  Try
                    imgPic = PathToImg(sTmpPath)
                  Catch ex As Exception
                    Debug.Print("Image Error: " & ex.Message)
                    imgPic = Nothing
                  Finally
                    My.Computer.FileSystem.DeleteFile(sTmpPath)
                  End Try
                  If imgPic.Width <> Width Or imgPic.Height <> Height Then
                    Debug.Print("Size Mismatch!")
                    Stop
                  End If
                  If imgPic IsNot Nothing Then
                    Dim pctImgCount As Integer
                    If pctImages Is Nothing Then
                      ReDim pctImages(0)
                      pctImgCount = 0
                    Else
                      pctImgCount = pctImages.Length
                      ReDim Preserve pctImages(pctImgCount)
                    End If
                    With pctImages(pctImgCount)
                      .PicType = PicType
                      .MIME = MIMEType
                      .Descr = sDesc
                      .Size = New Drawing.Size(Width, Height)
                      .ColorDepth = Depth
                      .ColorIndexes = ColorIndex
                      .Image = imgPic
                    End With
                  End If
                  'Stop
                Case Else
                  Debug.Print("Unknown block type " & BlockType)
                  Debug.Print(BitConverter.ToString(bRead.GetAllData))
                  Stop
              End Select
              If LastBlock Or bolHasVorb Then Exit Do
            End Using
          Loop
        Case Else
          bolHasVorb = False
          Debug.Print("Invalid format: " & Hex(dHeader))
      End Select
      lHeaderLength = ioFile.Position
    End If
    ioFile.Close()
  End Sub

  Private Function ReadPage(ByRef ioFile As IO.FileStream) As Page
    Dim pg As Page
    pg.Header = ReadLONG(ioFile)
    pg.Version = ioFile.ReadByte
    pg.TypeFlag = ioFile.ReadByte
    pg.GranulePosition = ReadQWORD(ioFile)
    pg.Serial = ReadDWORD(ioFile)
    pg.Sequence = ReadDWORD(ioFile)
    pg.Checksum = ReadDWORD(ioFile)
    pg.Segments = ioFile.ReadByte
    pg.HeadLen = 27 + pg.Segments
    pg.Length = pg.HeadLen
    ReDim pg.Segs(pg.Segments)
    For I = 0 To pg.Segments - 1
      pg.Segs(I) = ioFile.ReadByte
      pg.Length = pg.Length + pg.Segs(I)
    Next I
    Return pg
  End Function

  Public ReadOnly Property Location() As String
    Get
      Return sLocation
    End Get
  End Property

  Public ReadOnly Property File() As String
    Get
      Return sFile
    End Get
  End Property

  Public Sub New(File As String)
    sFile = File
    ResetData()
    LoadVorbis()
  End Sub

  Public ReadOnly Property Other() As String
    Get
      Return Join(sOther, vbNewLine)
    End Get
  End Property

  Public ReadOnly Property Performer() As String
    Get
      Return sPerformer
    End Get
  End Property

  Public ReadOnly Property RecordDate() As String
    Get
      Return sDate
    End Get
  End Property

  Private Sub ResetData()
    sTitle = vbNullString
    sVersion = vbNullString
    sAlbum = vbNullString
    sTrack = vbNullString
    sArtist = vbNullString
    sPerformer = vbNullString
    sCopyright = vbNullString
    sLicense = vbNullString
    sLabel = vbNullString
    sDescription = vbNullString
    sGenre = vbNullString
    sDate = vbNullString
    sLocation = vbNullString
    sContact = vbNullString
    sISRC = vbNullString
    lVorbVer = 0
    bVorbChannels = 0
    lVorbRate = 0
    lVorbDuration = 0.0
    sVendor = vbNullString
  End Sub

  Public ReadOnly Property Title() As String
    Get
      Return sTitle
    End Get
  End Property

  Public ReadOnly Property Track() As String
    Get
      Return sTrack
    End Get
  End Property

  Public ReadOnly Property Version() As String
    Get
      Return sVersion
    End Get
  End Property

  Public ReadOnly Property HeaderLength As Long
    Get
      Return lHeaderLength
    End Get
  End Property

  Private Function FindLONG(ioFile As IO.FileStream, toFind As UInt32) As Int64
    Const ChunkSize As Integer = 256
    Const FindSize As Integer = 4
    Dim ioPos As Long = ioFile.Position
    Dim ioFound As Int64 = -1
    Dim bFind As Byte = (toFind And &HFF000000UL) >> 24
    For I As Long = 0 To ioFile.Length - 1 Step ChunkSize
      ioFile.Seek(I, IO.SeekOrigin.Begin)
      Dim bChunk(ChunkSize - 1) As Byte
      ioFile.Read(bChunk, 0, ChunkSize)
      If bChunk.Contains(bFind) Then
        Dim iTryLoc As Integer = 0
        Do Until iTryLoc > ChunkSize
          Dim newLoc As Integer = Array.IndexOf(bChunk, bFind, iTryLoc)
          If newLoc = -1 Then Exit Do
          ioFile.Seek(I + newLoc, IO.SeekOrigin.Begin)
          Dim bData(FindSize - 1) As Byte
          If ioFile.Read(bData, 0, FindSize) <> 0 Then
            If GetDWORD(bData, 0) = toFind Then
              ioFound = ioFile.Position - FindSize
              Exit For
            End If
          End If
          iTryLoc = newLoc + 1
        Loop
      End If
    Next
    ioFile.Position = ioPos
    Return ioFound
  End Function

  Private Function FindLONG(ioFile As IO.FileStream, toFind As UInt32, startingPoint As Long) As Int64
    Const ChunkSize As Integer = 256
    Const FindSize As Integer = 4
    Dim ioPos As Long = ioFile.Position
    Dim ioFound As Int64 = -1
    Dim bFind As Byte = (toFind And &HFF000000UL) >> 24
    For I As Long = startingPoint To ioFile.Length - 1 Step ChunkSize
      ioFile.Seek(I, IO.SeekOrigin.Begin)
      Dim bChunk(ChunkSize - 1) As Byte
      ioFile.Read(bChunk, 0, ChunkSize)
      If bChunk.Contains(bFind) Then
        Dim iTryLoc As Integer = 0
        Do Until iTryLoc > ChunkSize
          Dim newLoc As Integer = Array.IndexOf(bChunk, bFind, iTryLoc)
          If newLoc = -1 Then Exit Do
          ioFile.Seek(I + newLoc, IO.SeekOrigin.Begin)
          Dim bData(FindSize - 1) As Byte
          If ioFile.Read(bData, 0, FindSize) <> 0 Then
            If GetDWORD(bData, 0) = toFind Then
              ioFound = ioFile.Position - FindSize
              Exit For
            End If
          End If
          iTryLoc = newLoc + 1
        Loop
      End If
    Next
    ioFile.Position = ioPos
    Return ioFound
  End Function

  Private Function ReadLONG(ioFile As IO.FileStream) As UInt32
    Dim bData(3) As Byte
    If ioFile.Read(bData, 0, 4) <> 0 Then
      Return GetDWORD(bData, 0)
    Else
      Return 0
    End If
  End Function

  Private Function ReadINT(ioFile As IO.FileStream) As Int32
    Dim bData(3) As Byte
    If ioFile.Read(bData, 0, 4) <> 0 Then
      Return BitConverter.ToInt32(bData, 0)
    Else
      Return 0
    End If
  End Function

  Private Function ReadUSHORT(ioFile As IO.FileStream) As UInt16
    Dim bData(1) As Byte
    If ioFile.Read(bData, 0, 2) <> 0 Then
      Return BitConverter.ToUInt16(bData, 0)
    Else
      Return 0
    End If
  End Function

  Private Function ReadDWORD(ioFile As IO.FileStream) As UInt32
    Dim bData(3) As Byte
    If ioFile.Read(bData, 0, 4) <> 0 Then
      Return BitConverter.ToUInt32(bData, 0)
    Else
      Return 0
    End If
  End Function

  Private Function ReadQWORD(ioFile As IO.FileStream) As UInt64
    Dim bData(7) As Byte
    If ioFile.Read(bData, 0, 8) <> 0 Then
      Return BitConverter.ToUInt64(bData, 0)
    Else
      Return 0
    End If
  End Function

  Private Function ReadString(ioFile As IO.FileStream, Length As UInt32, Optional codepage As Integer = LATIN_1) As String
    Dim bData(Length - 1) As Byte
    If ioFile.Read(bData, 0, Length) <> 0 Then
      Return System.Text.Encoding.GetEncoding(codepage).GetString(bData)
    Else
      Return 0
    End If
  End Function

  Private Function PathToImg(Path As String) As Drawing.Image
    If My.Computer.FileSystem.FileExists(Path) Then
      Using iStream As New IO.FileStream(Path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
        Return Drawing.Image.FromStream(iStream, True, True)
      End Using
    Else
      Return Nothing
    End If
  End Function

#Region "IDisposable Support"
  Private disposedValue As Boolean ' To detect redundant calls

  ' IDisposable
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        ' TODO: dispose managed state (managed objects).
        ResetData()
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
