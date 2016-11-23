'Public Class clsID3v2b
'  Implements IDisposable

'  Public Enum ID3Returns As Byte
'    [Set] = 0
'    Added = 1
'    Removed = 2
'    [ReadOnly] = 3
'    GeneralFailure = 4
'  End Enum
'  Public Enum TextEncoding As Byte
'    NT_ISO = 0
'    UTF_16_LE = 1
'    UTF_16_BE = 2
'    UTF_8 = 3
'  End Enum
'  Public Enum HFLG As Byte
'    FOOTER = &H10
'    EXPERIMENTAL = &H20
'    EXTENDED = &H40
'    UNSYNCH = &H80
'  End Enum
'  Public Enum FFLG As UInteger
'    TAGALTER = &H4000
'    FILEALTER = &H2000
'    [READONLY] = &H1000
'    [GROUP] = &H40
'    COMPRESS = &H8
'    ENCRYPT = &H4
'    UNSYNCH = &H2
'    DATALENGTH = &H1
'  End Enum
'  Public Enum ID3_PIC_TYPE As Byte
'    OTHER = 0
'    PNG_ICON_32x32 = 1
'    FILE_ICON = 2
'    FRONT_COVER = 3
'    BACK_COVER = 4
'    LEAFLET = 5
'    MEDIA_LABEL = 6
'    LEAD_ARTIST = 7
'    ARTIST = 8
'    CONDUCTOR = 9
'    ORCHESTRA_BAND = 10
'    COMPOSER = 11
'    LYRICIST = 12
'    RECORDING_LOCATION = 13
'    DURING_RECORDING = 14
'    DURING_PERFORMANCE = 15
'    SCREENCAP = 16
'    RED_HERRING = 17
'    ILLUSTRATION = 18
'    BAND_LOGO = 19
'    STUDIO_LOGO = 20
'    INVALID = 255
'  End Enum
'  Public Shared ReadOnly Property ID3_PIC_TYPE_DISPLAYORDER As ID3_PIC_TYPE()
'    Get
'      Return {Seed.clsID3v2.ID3_PIC_TYPE.FRONT_COVER,
'              Seed.clsID3v2.ID3_PIC_TYPE.OTHER,
'              Seed.clsID3v2.ID3_PIC_TYPE.SCREENCAP,
'              Seed.clsID3v2.ID3_PIC_TYPE.MEDIA_LABEL,
'              Seed.clsID3v2.ID3_PIC_TYPE.BAND_LOGO,
'              Seed.clsID3v2.ID3_PIC_TYPE.LEAD_ARTIST,
'              Seed.clsID3v2.ID3_PIC_TYPE.ARTIST,
'              Seed.clsID3v2.ID3_PIC_TYPE.ORCHESTRA_BAND,
'              Seed.clsID3v2.ID3_PIC_TYPE.BACK_COVER,
'              Seed.clsID3v2.ID3_PIC_TYPE.DURING_RECORDING,
'              Seed.clsID3v2.ID3_PIC_TYPE.DURING_PERFORMANCE,
'              Seed.clsID3v2.ID3_PIC_TYPE.ILLUSTRATION}
'    End Get
'  End Property
'  Public Enum ID3_PIC_MIME As Byte
'    BMP = 0
'    GIF = 1
'    JPG = 2
'    PNG = 3
'    INVALID = 255
'  End Enum

'  Public Enum FrameType
'    ASPI
'    RBUF
'    PCNT
'    COMM
'    COMR
'    AENC
'    CRM
'    ENCR
'    EQUA
'    EQU2
'    ETCO
'    GEOB
'    GRID
'    IPLS
'    TIPL
'    LINK
'    MCDI
'    MLLT
'    OWNE
'    APIC
'    PRIV
'    POPM
'    POSS
'    RVRB
'    RVAD
'    RVA2
'    SEEK
'    SIGN
'    SYLT
'    SYTC
'    TALB
'    TBPM
'    TCOM
'    TCON
'    TCOP
'    TDAT
'    TDTG
'    TDEN
'    TDLY
'    TENC
'    TFLT
'    TIME
'    TDRL
'    TKEY
'    TLAN
'    TLEN
'    TMCL
'    TMED
'    TMOO
'    TOPE
'    TOFN
'    TOLY
'    TDOR
'    TOAL
'    TOWN
'    TPE1
'    TPE2
'    TPE3
'    TPE4
'    TPOS
'    TPRO
'    TPUB
'    TSRC
'    TDRC
'    TRCK
'    TRSN
'    TRSO
'    TSOA
'    TSOP
'    TSOT
'    TSIZ
'    TSSE
'    TIT1
'    TSST
'    TIT2
'    TIT3
'    TEXT
'    TXXX
'    TYER
'    UFID
'    USER
'    USLT
'    WOAF
'    WOAR
'    WOAS
'    WCOM
'    CWOP
'    WORS
'    WPAY
'    WPUB
'    WXXX
'  End Enum

'  Public MustInherit Class Frame_Generic
'    Private m_Name As FrameType
'    Private m_Flags As FFLG
'    Public Property [Name] As FrameType
'      Get
'        Return m_Name
'      End Get
'      Set(value As FrameType)
'        m_Name = value
'      End Set
'    End Property
'    Public Property Flags As FFLG
'      Get
'        Return m_Flags
'      End Get
'      Set(value As FFLG)
'        m_Flags = value
'      End Set
'    End Property
'    Public Overrides Function ToString() As String
'      Return GetFrameName(m_Name)
'    End Function
'  End Class
'  Public Class Frame_UFID
'    Inherits Frame_Generic
'    Private m_OwnerID As String
'    Private m_UFID As Byte()
'    Public Property OwnerID As String
'      Get
'        Return m_OwnerID
'      End Get
'      Set(value As String)
'        m_OwnerID = value
'      End Set
'    End Property
'    Public Property UFID As Byte()
'      Get
'        Return m_UFID
'      End Get
'      Set(value As Byte())
'        If value.Length > 64 Then ReDim Preserve value(63)
'        m_UFID = value
'      End Set
'    End Property
'  End Class
'  Public Class Frame_Text
'    Inherits Frame_Generic
'    Private m_Encoding As TextEncoding
'    Private m_Information As String
'    Public Property Encoding As TextEncoding
'      Get
'        Return m_Encoding
'      End Get
'      Set(value As TextEncoding)
'        m_Encoding = value
'      End Set
'    End Property
'    Public Overridable Property Information As String
'      Get
'        Return m_Information
'      End Get
'      Set(value As String)
'        m_Information = value
'      End Set
'    End Property
'  End Class
'  Public Class Frame_Text_Numerical
'    Inherits Frame_Text
'    Public Overrides Property Information As String
'      Get
'        Return MyBase.Information
'      End Get
'      Set(value As String)
'        MyBase.Information = Val(value)
'      End Set
'    End Property
'  End Class
'  Public Class Frame_Text_Timestamp
'    Inherits Frame_Text
'    Public Property Timestamp As Date
'      Get
'        Dim dTest As Date
'        If Date.TryParseExact(MyBase.Information, "yyyy-MM-dd\THH:mm:ss", Nothing, Globalization.DateTimeStyles.AssumeUniversal, dTest) Then
'          Return dTest
'        Else
'          Return Nothing
'        End If
'      End Get
'      Set(value As Date)
'        MyBase.Information = value.ToUniversalTime.ToString("yyyy-MM-dd\THH:mm:ss")
'      End Set
'    End Property
'  End Class
'  Public Class Frame_TRCK
'    Inherits Frame_Text
'    Public Property Track As Integer
'      Get
'        If MyBase.Information.Contains("/") Then
'          Return Val(MyBase.Information.Substring(0, MyBase.Information.IndexOf("/")))
'        Else
'          Return Val(MyBase.Information)
'        End If
'      End Get
'      Set(value As Integer)
'        If MyBase.Information.Contains("/") Then
'          MyBase.Information = value & "/" & Val(MyBase.Information.Substring(MyBase.Information.IndexOf("/") + 1))
'        Else
'          MyBase.Information = value
'        End If
'      End Set
'    End Property
'    Public Property Total As Integer
'      Get
'        If MyBase.Information.Contains("/") Then
'          Return Val(MyBase.Information.Substring(MyBase.Information.IndexOf("/") + 1))
'        Else
'          Return -1
'        End If
'      End Get
'      Set(value As Integer)
'        If MyBase.Information.Contains("/") Then
'          MyBase.Information = Val(MyBase.Information.Substring(0, MyBase.Information.IndexOf("/"))) & "/" & value
'        Else
'          MyBase.Information = Val(MyBase.Information) & "/" & value
'        End If
'      End Set
'    End Property
'  End Class
'  Public Class Frame_TPOS
'    Inherits Frame_Text
'    Public Property Disc As Integer
'      Get
'        If MyBase.Information.Contains("/") Then
'          Return Val(MyBase.Information.Substring(0, MyBase.Information.IndexOf("/")))
'        Else
'          Return Val(MyBase.Information)
'        End If
'      End Get
'      Set(value As Integer)
'        If MyBase.Information.Contains("/") Then
'          MyBase.Information = value & "/" & Val(MyBase.Information.Substring(MyBase.Information.IndexOf("/") + 1))
'        Else
'          MyBase.Information = value
'        End If
'      End Set
'    End Property
'    Public Property Total As Integer
'      Get
'        If MyBase.Information.Contains("/") Then
'          Return Val(MyBase.Information.Substring(MyBase.Information.IndexOf("/") + 1))
'        Else
'          Return -1
'        End If
'      End Get
'      Set(value As Integer)
'        If MyBase.Information.Contains("/") Then
'          MyBase.Information = Val(MyBase.Information.Substring(0, MyBase.Information.IndexOf("/"))) & "/" & value
'        Else
'          MyBase.Information = Val(MyBase.Information) & "/" & value
'        End If
'      End Set
'    End Property
'  End Class
'  Public Class Frame_TSRC
'    Inherits Frame_Text
'    Public Overrides Property Information As String
'      Get
'        Return MyBase.Information
'      End Get
'      Set(value As String)
'        If value.Length > 12 Then value = value.Substring(0, 12)
'        MyBase.Information = value
'      End Set
'    End Property
'  End Class
'  Public Class Frame_TMCL
'    Inherits Frame_Text
'    Public Structure Musician
'      Public [Name] As String
'      Public Instrument As String
'      Public Sub New(sArtist As String, sInstrument As String)
'        Name = sArtist
'        Instrument = sInstrument
'      End Sub
'    End Structure
'    Public Property Musicians As List(Of Musician)
'      Get
'        If String.IsNullOrEmpty(MyBase.Information) Then Return Nothing
'        Dim musicList As New List(Of Musician)
'        If MyBase.Information.Contains(",") Then
'          Dim myInfo() As String = Split(MyBase.Information, ",")
'          For I As Integer = 0 To myInfo.Length - 1 Step 2
'            musicList.Add(New Musician(myInfo(I), myInfo(I + 1)))
'          Next
'        Else
'          musicList.Add(New Musician(MyBase.Information, Nothing))
'        End If
'        Return musicList
'      End Get
'      Set(value As List(Of Musician))
'        Dim sList As String = Nothing
'        For Each artist In value
'          sList &= artist.Name & "," & artist.Instrument & ","
'        Next
'        If String.IsNullOrEmpty(sList) Then
'          MyBase.Information = Nothing
'        Else
'          Do While sList.EndsWith(",")
'            sList = sList.Substring(0, sList.Length - 1)
'            If sList.Length = 0 Then Exit Do
'          Loop
'          If String.IsNullOrEmpty(sList) Then
'            MyBase.Information = Nothing
'          Else
'            MyBase.Information = sList
'          End If
'        End If
'      End Set
'    End Property
'  End Class
'  Public Class Frame_TIPL
'    Inherits Frame_Text
'    Public Structure InvolvedPerson
'      Public [Name] As String
'      Public Job As String
'      Public Sub New(sName As String, sJob As String)
'        Name = sName
'        Job = sJob
'      End Sub
'    End Structure
'    Public Property InvolvedPeople As List(Of InvolvedPerson)
'      Get
'        If String.IsNullOrEmpty(MyBase.Information) Then Return Nothing
'        Dim musicList As New List(Of InvolvedPerson)
'        If MyBase.Information.Contains(",") Then
'          Dim myInfo() As String = Split(MyBase.Information, ",")
'          For I As Integer = 0 To myInfo.Length - 1 Step 2
'            musicList.Add(New InvolvedPerson(myInfo(I), myInfo(I + 1)))
'          Next
'        Else
'          musicList.Add(New InvolvedPerson(MyBase.Information, Nothing))
'        End If
'        Return musicList
'      End Get
'      Set(value As List(Of InvolvedPerson))
'        Dim sList As String = Nothing
'        For Each artist In value
'          sList &= artist.Name & "," & artist.Job & ","
'        Next
'        If String.IsNullOrEmpty(sList) Then
'          MyBase.Information = Nothing
'        Else
'          Do While sList.EndsWith(",")
'            sList = sList.Substring(0, sList.Length - 1)
'            If sList.Length = 0 Then Exit Do
'          Loop
'          If String.IsNullOrEmpty(sList) Then
'            MyBase.Information = Nothing
'          Else
'            MyBase.Information = sList
'          End If
'        End If
'      End Set
'    End Property
'  End Class
'  Public Class Frame_TKEY
'    Inherits Frame_Text
'    Public Overrides Property Information As String
'      Get
'        Return MyBase.Information
'      End Get
'      Set(value As String)
'        Dim sInfo As String = Nothing
'        For I As Integer = 0 To value.Length - 1
'          Select Case value(I)
'            Case "A", "B", "C", "D", "E", "F", "G", "b", "#", "m", "o"
'              sInfo &= value(I)
'          End Select
'        Next
'        MyBase.Information = sInfo
'      End Set
'    End Property
'  End Class
'  Public Class Frame_TLNG
'    Inherits Frame_Text
'    Public Property Languages As List(Of String)
'      Get
'        If String.IsNullOrEmpty(MyBase.Information) Then Return Nothing
'        Dim langList As New List(Of String)
'        If MyBase.Information.Contains(vbNullChar) Then
'          langList.AddRange(Split(MyBase.Information, vbNullChar))
'        Else
'          langList.Add(MyBase.Information)
'        End If
'        Return langList
'      End Get
'      Set(value As List(Of String))
'        MyBase.Information = Join(value.ToArray, vbNullChar)
'      End Set
'    End Property
'  End Class
'  Public Class Frame_TCON
'    Inherits Frame_Text
'    Public Property Genres As List(Of String)
'      'TODO: make this right
'      Get
'        If String.IsNullOrEmpty(MyBase.Information) Then Return Nothing
'        Dim genreList As New List(Of String)
'        If MyBase.Information.Contains(vbNullChar) Then
'          genreList.AddRange(Split(MyBase.Information, vbNullChar))
'        Else
'          genreList.Add(MyBase.Information)
'        End If
'        Return genreList
'      End Get
'      Set(value As List(Of String))
'        MyBase.Information = Join(value.ToArray, vbNullChar)
'      End Set
'    End Property
'  End Class
'  Public Class Frame_TXXX
'    Inherits Frame_Text
'    Private m_Value As String
'    Public Property Value As String
'      Get
'        Return m_Value
'      End Get
'      Set(value As String)
'        m_Value = value
'      End Set
'    End Property
'  End Class
'  Public Class Frame_URL
'    Inherits Frame_Generic
'    Private m_URL As String
'    Public Overridable Property URL As String
'      Get
'        Return m_URL
'      End Get
'      Set(value As String)
'        m_URL = value
'      End Set
'    End Property
'  End Class
'  Public Class Frame_WXXX
'    Inherits Frame_Text
'    Private m_URL As String
'    Public Property URL As String
'      Get
'        Return m_URL
'      End Get
'      Set(value As String)
'        m_URL = value
'      End Set
'    End Property
'  End Class
'  Public Class Frame_MCDI
'    Inherits Frame_Generic
'    Private m_TOC As Byte()
'    Public Property TOC As Byte()
'      Get
'        Return m_TOC
'      End Get
'      Set(value As Byte())
'        If value.Length > 804 Then ReDim Preserve value(803)
'        m_TOC = value
'      End Set
'    End Property
'  End Class
'  Public Class Frame_ETCO
'    Inherits Frame_Generic
'    Public Enum EventType
'      Padding = &H0
'      End_of_Initial_Silence = &H1
'      Intro_Start = &H2
'      Main_Part_Start = &H3
'      Outro_Start = &H4
'      Outro_End = &H5
'      Verse_Start = &H6
'      Refrain_Start = &H7
'      Interlude_Start = &H8
'      Theme_Start = &H9
'      Variation_Start = &HA
'      Key_Change = &HB
'      Time_Change = &HC
'      Momentary_Unwanted_Noise = &HD
'      Sustained_Noise = &HE
'      Sustained_Noise_End = &HF
'      Intro_End = &H10
'      Main_Part_End = &H11
'      Verse_End = &H12
'      Refrain_End = &H13
'      Theme_End = &H14
'      Profanity = &H15
'      Profanity_End = &H16
'      Audio_End = &HFD
'      Audio_File_End = &HFE
'    End Enum
'    Public Structure [Event]
'      Public [Type] As EventType
'      Public TimeStamp As UInt32
'      Public Sub New(eType As EventType, uTimeStamp As UInt32)
'        [Type] = eType
'        TimeStamp = uTimeStamp
'      End Sub
'    End Structure
'    Private m_Format As Byte
'    Private m_Events As New List(Of [Event])
'    Public Property TimeStampFormat As Byte
'      Get
'        Return m_Format
'      End Get
'      Set(value As Byte)
'        m_Format = value
'      End Set
'    End Property
'    Public Property EventList As List(Of [Event])
'      Get
'        Return m_Events
'      End Get
'      Set(value As List(Of [Event]))
'        m_Events = value
'      End Set
'    End Property
'  End Class
'  Public Class Frame_MLLT
'    Inherits Frame_Generic
'    Private m_FramesBetweenReference As UInt16
'    Private m_BytesBetweenReference As UInt32
'    Private m_MSBetweenReference As UInt32
'    Private m_BitsForBytesDeviation As Byte
'    Private m_BitsForMSDeviation As Byte
'    Private m_ByteDeviation As Byte()
'    Private m_MSDeviation As Byte()
'    Public Property FramesBetweenReference As UInt16
'      Get
'        Return m_FramesBetweenReference
'      End Get
'      Set(value As UInt16)
'        m_FramesBetweenReference = value
'      End Set
'    End Property
'    Public Property BytesBetweenReference As UInt32
'      Get
'        Return m_BytesBetweenReference
'      End Get
'      Set(value As UInt32)
'        m_BytesBetweenReference = value And &HFFFFFF
'      End Set
'    End Property
'    Public Property MSBetweenReference As UInt32
'      Get
'        Return m_MSBetweenReference
'      End Get
'      Set(value As UInt32)
'        m_MSBetweenReference = value And &HFFFFFF
'      End Set
'    End Property
'    Public Property BitsForBytesDeviation As Byte
'      Get
'        Return m_BitsForBytesDeviation
'      End Get
'      Set(value As Byte)
'        m_BitsForBytesDeviation = value
'      End Set
'    End Property
'    Public Property BitsForMSDeviation As Byte
'      Get
'        Return m_BitsForMSDeviation
'      End Get
'      Set(value As Byte)
'        m_BitsForMSDeviation = value
'      End Set
'    End Property
'    Public Property DeviationInBytes As Byte()
'      Get
'        Return m_ByteDeviation
'      End Get
'      Set(value As Byte())
'        m_ByteDeviation = value
'      End Set
'    End Property
'    Public Property DeviationInMS As Byte()
'      Get
'        Return m_MSDeviation
'      End Get
'      Set(value As Byte())
'        m_MSDeviation = value
'      End Set
'    End Property
'  End Class

'  Private Structure ID3v2Header
'    Public Identifier As String '* 3
'    Public Version As Byte() ' 0 to 1
'    Public Flags As HFLG
'    Public Size As UInteger
'  End Structure
'  Private Structure ID3v2Frame2
'    Public FrameName As String ' * 3
'    Public FrameSize As Byte() ' 0 to 2
'  End Structure
'  Private Structure ID3v2Frame1
'    Public FrameName As String '* 4
'    Public FrameSize As UInteger
'    Public FrameFlags As Byte() '0 to 1
'  End Structure
'  Private Structure ID3v2Frame
'    Public FrameName As String
'    Public FrameData As String
'    Public FrameFlags As FFLG
'  End Structure

'  Private m_sMp3File As String
'  Private ID3Header As ID3v2Header
'  Private HasID3v2 As Boolean
'  Private ID3Frames As ArrayList
'  Private lID3Len As Integer

'  Public Class EncodedText
'    Public Encoding As TextEncoding
'    Public Text As String
'    Public Sub New(sText As String)
'      Text = sText
'      Encoding = TextEncoding.NT_ISO
'    End Sub
'    Public Sub New(sText As String, tEncoding As TextEncoding)
'      Text = sText
'      Encoding = tEncoding
'    End Sub
'    Public Shared ReadOnly Property Empty As EncodedText
'      Get
'        Return New EncodedText(Nothing, TextEncoding.NT_ISO)
'      End Get
'    End Property
'    Public ReadOnly Property IsEmpty As Boolean
'      Get
'        If String.IsNullOrEmpty(Text) And Encoding = TextEncoding.NT_ISO Then Return True
'        Return False
'      End Get
'    End Property
'  End Class

'#Region "IDisposable Support"
'  Private disposedValue As Boolean
'  Protected Overridable Sub Dispose(disposing As Boolean)
'    If Not Me.disposedValue Then
'      If disposing Then
'      End If
'    End If
'    Me.disposedValue = True
'  End Sub
'  Public Sub Dispose() Implements IDisposable.Dispose
'    Dispose(True)
'    GC.SuppressFinalize(Me)
'  End Sub
'#End Region
'End Class
