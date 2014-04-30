Option Strict On
Option Explicit On

Imports Microsoft.Win32.SafeHandles
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.data

Imports System.Security

<SuppressUnmanagedCodeSecurity()>
Friend Class NativeMethods

  <DllImport("kernel32", SetLastError:=True)>
  Public Shared Function CreateFile(fileName As String, desiredAccess As UInteger,
            shareMode As UInteger, securityAttributes As IntPtr, creationDisposition As UInteger,
            flagsAndAttributes As UInteger,
            hTemplateFile As IntPtr) As SafeFileHandle
  End Function

  <DllImport("kernel32", SetLastError:=True)>
  Public Shared Function DeviceIoControl(hVol As SafeFileHandle, controlCode As Integer,
            ByVal inbuffer As IntPtr, inBufferSize As Integer,
            ByRef outBuffer As SCSI_ADDRESS, outBufferSize As Integer,
            <Out()> ByRef bytesReturned As Integer, ovelapped As IntPtr) As Boolean
  End Function

  <DllImport("kernel32", SetLastError:=True)>
  Public Shared Function DeviceIoControl(hVol As SafeFileHandle, controlCode As Integer,
            ByRef inbuffer As SCSI_PASS_THROUGH_DIRECT_WITH_BUFFER, inBufferSize As Integer,
            ByRef outBuffer As SCSI_PASS_THROUGH_DIRECT_WITH_BUFFER, outBufferSize As Integer,
            <Out()> ByRef bytesReturned As Integer, ovelapped As IntPtr) As Boolean
  End Function

End Class

Friend Enum CharacterCode
  ISO88591 = 0
  ISO646ASCII = 1
  MSJIS = &H80
  Korean = &H81
  MandarinStandardChinese = &H82
End Enum

Friend Enum CommandDirection As Byte
  SCSI_IOCTL_DATA_OUT = 0
  SCSI_IOCTL_DATA_IN = 1
  SCSI_IOCTL_DATA_UNSPECIFIED = 2
End Enum

Public Enum Language
  ' Language codes, some are in cdrdao, which led me to the rest in
  ' "Standard handbook of audio and radio engineering", google books.
  Unknown = 0
  Albanian
  Breton
  Catalan
  Croatian
  Welsh
  Czech
  Danish
  German
  English
  Spanish
  Esperanto
  Estonian
  Basque
  Faroese
  French
  Frisian
  Irish
  Gaelic
  Galician
  Icelandic
  Italia
  Lappish
  Latin
  Latvian
  Luxembourgian
  Lithuanian
  Hungarian
  Maltese
  Dutch
  Norwegian
  Occitan
  Polish
  Portugese
  Romanian
  Romansh
  Serbian
  Slovak
  Slovenian
  Finnish
  Swedish
  Turkish
  Flemish
  ' gap for reserved ones.
  Zulu = &H45
  Vietnamese
  Uzbek
  Urdu
  Ukrainian
  Thai
  Telugu
  Tatar
  Tamil
  Tadzhik
  Swahili
  SrananTongo
  Somali
  Sinhalese
  Shona
  SerboCroat
  Ruthenian
  Russian
  Quechua
  Pushtu
  Punjabi
  Persian
  Papamiento
  Oriya
  Nepali
  Ndebele
  Marathi
  Moldavian
  Malaysian
  Malagasay
  Macedonian
  Laotian
  Korean
  Khmer
  Kazakh
  Kannada
  Japanese
  Indonesian
  Hindi
  Hebrew
  Hausa
  Gurani
  Gujurati
  Greek
  Georgian
  Fulani
  Dari
  Churash
  Chinese
  Burmese
  Bulgarian
  Bengali
  Bielorussian
  Bambora
  Azerbaijani
  Assamese
  Armenian
  Arabic
  Amharic
End Enum

Friend Enum PackType As Byte
  Title = &H80
  Performer = &H81
  Songwriter = &H82
  Composer = &H83
  Arranger = &H84
  Message = &H85
  DiscInformation = &H86
  Genre = &H87
  ' Toc = &H88    
  ' Toc2 = &H89   
  Code = &H8E
  Size = &H8F
End Enum

<StructLayout(LayoutKind.Sequential)>
Public Structure SCSI_ADDRESS
  Public Length As Integer
  Public PortNumber As Byte
  Public PathId As Byte
  Public TargetId As Byte
  Public Lun As Byte
  Public Shared Function GetSize() As Byte
    Return CType(Marshal.SizeOf(GetType(SCSI_ADDRESS)), Byte)
  End Function
End Structure

<StructLayout(LayoutKind.Sequential)>
Friend Structure SCSI_PASS_THROUGH_DIRECT                           ' x86   x64
  Public Length As Short                                          '  0      0
  Public ScsiStatus As Byte                                       '  2      2
  Public PathId As Byte                                           '  3      3
  Public TargetId As Byte                                         '  4      4
  Public Lun As Byte                                              '  5      5
  Public CdbLength As Byte                                        '  6      6
  Public SenseInfoLength As Byte                                  '  7      7
  Public DataIn As CommandDirection                               '  8      8
  Public DataTransferLength As Integer                            ' 12     12 
  Public TimeOutValue As Integer                                  ' 16     16
  Public DataBuffer As IntPtr                                     ' 20     24
  Public SenseInfoOffset As Integer                               ' 24     32
  <MarshalAs(UnmanagedType.ByValArray, SizeConst:=16)>
  Public cdb() As Byte                                            ' 28     36
  '                                                        Size     44     56
  Public Shared Function GetSize() As Integer
    Return Marshal.SizeOf(GetType(SCSI_PASS_THROUGH_DIRECT))
  End Function

End Structure

<StructLayout(LayoutKind.Sequential)>
Friend Structure SCSI_PASS_THROUGH_DIRECT_WITH_BUFFER
  Public ScsiPassThroughDirect As SCSI_PASS_THROUGH_DIRECT    ' 0    0
  Public Filler As Integer                                    ' 44  56
  <MarshalAs(UnmanagedType.ByValArray, sizeconst:=32)>
  Public SenseBuffer() As Byte                                ' 48  60
  '                                                           = 80  96
End Structure

Friend Class BlockInfo
  Property BlockNumber As Integer
  Property CharCode As Integer
  Property FirstTrack As Integer
  Property LastTrack As Integer
  Public Property PackCounts As IDictionary(Of PackType, Integer)
  Public Property LanguageCode As Integer ' language code of this block
  Sub New(blockNo As Integer)
    Me.BlockNumber = blockNo
    Me.PackCounts = New Dictionary(Of PackType, Integer)
  End Sub
  Private Shared Function GetBlockNo(bytes() As Byte, index As Integer) As Integer
    Return (bytes(index + 3) >> 4) And &H7
  End Function
End Class

Friend Class BlockInfoCollection
  Inherits KeyedCollection(Of Integer, BlockInfo)
  Protected Overrides Function GetKeyForItem(item As BlockInfo) As Integer
    Return item.BlockNumber
  End Function
  Public Function GetExistingOrAddNew(blockNo As Integer) As BlockInfo
    If Me.Contains(blockNo) Then
      Return Me(blockNo)
    Else
      Dim blockInfo As New BlockInfo(blockNo)
      Me.Add(blockInfo)
      Return blockInfo
    End If
  End Function
End Class

Public Class CdText
  Public Property LanguageCodes As Integer()
  Public Property DiscInformation As String
  Public Property GenreCode As Integer
  Public Property Genre As String
  Friend Sub New()
  End Sub
  Property TrackData As DataTable
End Class

Public Class CdTextRetriever
  Public Shared Function GetCdText(driveInfo As DriveInfo) As CdText
    Dim devicePath As String = String.Format("\\.\{0}:", driveInfo.Name(0))
    Const FILE_ATTRIBUTE_NORMAL As UInteger = &H80
    Const GENERIC_READ As UInteger = &H80000000UI
    Const GENERIC_WRITE As UInteger = &H40000000
    Const FILE_SHARE_READ As UInteger = 1
    Const FILE_SHARE_WRITE As UInteger = 2
    Const OPEN_EXISTING As UInteger = 3
    Using hDevice As SafeFileHandle = NativeMethods.CreateFile(devicePath, GENERIC_READ Or GENERIC_WRITE, FILE_SHARE_READ Or FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero)
      If hDevice.IsInvalid OrElse hDevice.IsClosed Then Throw New Win32Exception
      Return ReadCdText(hDevice)
    End Using
  End Function

  Private Shared Function ReadCdText(hDevice As SafeFileHandle) As CdText
    Dim address As SCSI_ADDRESS = GetScsiAddress(hDevice)
    Dim bytes(1) As Byte
    GetCdTextBytes(hDevice, address, bytes)
    bytes = New Byte((CType(bytes(0), Integer) << 8) Or bytes(1) + 1) {}
    GetCdTextBytes(hDevice, address, bytes)
    Dim parser As New DataPacketParser
    Return parser.ReadCtText(bytes)
  End Function

  Private Shared Function GetScsiAddress(hDevice As SafeFileHandle) As SCSI_ADDRESS
    Dim address As New SCSI_ADDRESS
    Dim bytesReturned As Integer
    Const IOCTL_SCSI_GET_ADDRESS As Integer = &H41018
    Dim result As Boolean = NativeMethods.DeviceIoControl(hDevice, IOCTL_SCSI_GET_ADDRESS, IntPtr.Zero, 0, address, SCSI_ADDRESS.GetSize, bytesReturned, IntPtr.Zero)
    If result = False Then Throw New Win32Exception
    Return address
  End Function

  Private Shared Sub GetCdTextBytes(hDevice As SafeFileHandle, address As SCSI_ADDRESS, bytes() As Byte)
    Dim bytesReturned As Integer
    Dim sptdwb As New SCSI_PASS_THROUGH_DIRECT_WITH_BUFFER
    Dim pinnedBytesHandle As GCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned)
    sptdwb.ScsiPassThroughDirect.cdb = New Byte(15) {}
    sptdwb.SenseBuffer = New Byte(31) {}
    sptdwb.ScsiPassThroughDirect.Length = CShort(SCSI_PASS_THROUGH_DIRECT.GetSize)
    sptdwb.ScsiPassThroughDirect.CdbLength = 10
    sptdwb.ScsiPassThroughDirect.DataIn = CommandDirection.SCSI_IOCTL_DATA_IN
    sptdwb.ScsiPassThroughDirect.DataTransferLength = bytes.Length
    sptdwb.ScsiPassThroughDirect.DataBuffer = pinnedBytesHandle.AddrOfPinnedObject
    sptdwb.ScsiPassThroughDirect.TimeOutValue = 10
    sptdwb.ScsiPassThroughDirect.SenseInfoOffset = Marshal.OffsetOf(GetType(SCSI_PASS_THROUGH_DIRECT_WITH_BUFFER), "SenseBuffer").ToInt32
    sptdwb.ScsiPassThroughDirect.SenseInfoLength = 24
    sptdwb.ScsiPassThroughDirect.PathId = address.PathId
    sptdwb.ScsiPassThroughDirect.TargetId = address.TargetId
    sptdwb.ScsiPassThroughDirect.Lun = address.Lun
    sptdwb.ScsiPassThroughDirect.cdb(0) = &H43
    sptdwb.ScsiPassThroughDirect.cdb(2) = 5
    sptdwb.ScsiPassThroughDirect.cdb(7) = CByte((bytes.Length >> 8) And &HFF)
    sptdwb.ScsiPassThroughDirect.cdb(8) = CByte(bytes.Length And &HFF)
    Try
      Const IOCTL_SCSI_PASS_THROUGH_DIRECT As Integer = &H4D014
      Dim result As Boolean = NativeMethods.DeviceIoControl(hDevice, IOCTL_SCSI_PASS_THROUGH_DIRECT, sptdwb, Marshal.SizeOf(sptdwb), sptdwb, Marshal.SizeOf(sptdwb), bytesReturned, IntPtr.Zero)
      If result = False Then Throw New Win32Exception
      Dim error1 As IoctlResult = IoctlResult.FromSenseBuffer(sptdwb.SenseBuffer, sptdwb.ScsiPassThroughDirect.ScsiStatus)
      If Not (error1.SenseKey = 0 AndAlso error1.AdditionalSenseCode = 0 AndAlso error1.AdditionalSenseCodeQualifier = 0) Then
        Debug.Print("unable to get CD Text: " & error1.ErrorMessage)
        'Throw New IO.IOException(error1.ErrorMessage)
      End If
    Finally
      pinnedBytesHandle.Free()
    End Try
  End Sub
End Class

Friend Class Crc
  Private Shared table() As UShort

  Shared Sub New()
    table = New UShort(255) {}
    For i As UShort = 0 To 255
      Dim temp As UShort = 0
      Dim a As UShort = i << 8
      For j As Integer = 0 To 7
        If (((temp Xor a) And &H8000US) <> 0US) Then
          temp = (temp << 1) Xor 4129US
        Else
          temp <<= 1
        End If
        a <<= 1
      Next
      table(i) = temp
    Next
  End Sub

  Private Shared Function GetCrc(bytes() As Byte, index As Integer) As UShort
    Dim crc As UShort = 0
    For i As Integer = index To index + 15
      crc = (crc << 8) Xor table((crc >> 8) Xor bytes(i))
    Next
    Return Not crc
  End Function

  Private Shared Function GetCrcFromPackData(bytes() As Byte, index As Integer) As UShort
    Return (CUShort(bytes(index + 16)) << 8) Or bytes(index + 17)
  End Function

  Public Shared Function CrcIsOk(bytes() As Byte, index As Integer) As Boolean
    Dim crcCalculated As UShort = Crc.GetCrc(bytes, index)
    Dim crcStored As UShort = GetCrcFromPackData(bytes, index)
    Return crcCalculated = crcStored
  End Function
End Class

Public Class DataPacketParser
  Private blockInfos As BlockInfoCollection
  Private cdText As CdText

  Public Function ReadCtText(bytes() As Byte) As CdText
    blockInfos = New BlockInfoCollection
    cdText = New CdText
    Pass1(bytes)
    Pass2(bytes)
    Return cdText
  End Function

  Private Sub Pass1(bytes() As Byte)
    Dim languageCodes(7) As Language
    Dim discInfoBuilder As New StringBuilder
    Dim genreBuilder As New StringBuilder
    For index As Integer = 4 To bytes.Length - 19 Step 18
      If Not Crc.CrcIsOk(bytes, index) Then
        ' maybe you could be more lenient if it's just some one or two corrupt packets.
        Throw New IO.InvalidDataException("Crc check failed.")
      End If
      Dim packTypeRaw As Byte = bytes(index)
      Select Case packTypeRaw
        Case PackType.Size ' &H8F
          Dim blockInfo As BlockInfo = blockInfos.GetExistingOrAddNew(GetBlockNo(bytes, index))
          Select Case bytes(index + 1)
            Case 0
              blockInfo.CharCode = bytes(index + 4)
              blockInfo.FirstTrack = bytes(index + 5)
              blockInfo.LastTrack = bytes(index + 6)
              blockInfo.PackCounts.Add(PackType.Title, bytes(index + 8))
              blockInfo.PackCounts.Add(PackType.Performer, bytes(index + 9))
              blockInfo.PackCounts.Add(PackType.Songwriter, bytes(index + 10))
              blockInfo.PackCounts.Add(PackType.Composer, bytes(index + 11))
              blockInfo.PackCounts.Add(PackType.Arranger, bytes(index + 12))
              blockInfo.PackCounts.Add(PackType.Message, bytes(index + 13))
              blockInfo.PackCounts.Add(PackType.DiscInformation, bytes(index + 14))
              blockInfo.PackCounts.Add(PackType.Genre, bytes(index + 15))
            Case 1
              blockInfo.PackCounts.Add(PackType.Code, bytes(index + 10))
            Case 2
              For j As Integer = 8 To 15
                languageCodes(j - 8) = CType(bytes(index + j), Language)
              Next
              blockInfo.LanguageCode = languageCodes(blockInfo.BlockNumber)
          End Select
        Case PackType.DiscInformation ' &H86
          discInfoBuilder.Append(EncodingManager.Iso88591.GetString(bytes, index + 4, 12))
        Case PackType.Genre
          If bytes(index + 3) = 0 Then
            cdText.GenreCode = (CInt(bytes(index + 4)) << 8) Or bytes(index + 5)
            genreBuilder.Append(EncodingManager.Iso88591.GetString(bytes, index + 6, 10))
          Else
            genreBuilder.Append(EncodingManager.Iso88591.GetString(bytes, index + 4, 12))
          End If
      End Select
    Next
    cdText.LanguageCodes = languageCodes
    cdText.DiscInformation = discInfoBuilder.ToString
    cdText.Genre = genreBuilder.ToString
  End Sub

  Private Sub Pass2(bytes() As Byte)
    Dim dt As DataTable = GetDataTable()
    For index As Integer = 4 To bytes.Length - 19 Step 18
      Dim packType As PackType = CType(bytes(index), PackType)
      If (packType >= packType.Title AndAlso packType <= packType.Message) OrElse packType = packType.Code Then
        Dim blockNo As Integer = GetBlockNo(bytes, index)
        Dim blockInfo As BlockInfo = blockInfos(blockNo)
        Dim languageCode As Integer = blockInfo.LanguageCode
        Dim encoding As Encoding = EncodingManager.GetEncoding(blockInfo.CharCode)
        Dim trackNo As Integer = GetTrackNo(bytes, index)
        Dim maxTrackNo As Integer = blockInfo.LastTrack
        Dim chars() As Char = encoding.GetChars(bytes, index + 4, 12)
        Dim dr As DataRow = GetDataRow(dt, languageCode, trackNo)
        Dim sb As New StringBuilder
        Dim colIndex As Integer = ColumnIndexFromPackType(packType)
        For j As Integer = 0 To chars.Length - 1
          If chars(j) = Char.MinValue Then
            Dim s As String = If(dr.Field(Of String)(colIndex), "")
            sb.Insert(0, s)
            dr(colIndex) = sb.ToString
            trackNo += 1
            If trackNo > maxTrackNo Then Exit For
            sb.Clear()
            dr = GetDataRow(dt, languageCode, trackNo)
          Else
            sb.Append(chars(j))
            If j = chars.Length - 1 Then
              Dim s As String = If(dr.Field(Of String)(colIndex), "")
              sb.Insert(0, s)
              dr(colIndex) = sb.ToString
            End If
          End If
        Next
      End If
    Next
    cdText.TrackData = dt
  End Sub

  Private Shared Function GetDataRow(dt As DataTable, language As Integer, trackNo As Integer) As DataRow
    Dim dr As DataRow = (From dr2 As DataRow In dt.AsEnumerable Where dr2.Field(Of Integer)(0) = language AndAlso dr2.Field(Of Integer)(1) = trackNo Select dr2).FirstOrDefault
    If dr Is Nothing Then
      dr = dt.NewRow
      dr(0) = language
      dr(1) = trackNo
      dt.Rows.Add(dr)
    End If
    Return dr
  End Function

  Private Shared Function ColumnIndexFromPackType(packType As PackType) As Integer
    Select Case packType
      Case packType.Title
        Return 2
      Case packType.Performer
        Return 3
      Case packType.Songwriter
        Return 4
      Case packType.Composer
        Return 5
      Case packType.Arranger
        Return 6
      Case packType.Message
        Return 7
      Case packType.Code
        Return 8
      Case Else
        Throw New ArgumentException("Invalid pack type")
    End Select
  End Function

  Private Shared Function GetDataTable() As DataTable
    Dim dt As New DataTable
    dt.Columns.Add("Language", GetType(Integer))
    dt.Columns.Add("Track No", GetType(Integer))
    dt.Columns.Add("Title", GetType(String))
    dt.Columns.Add("Performer", GetType(String))
    dt.Columns.Add("Songwriter", GetType(String))
    dt.Columns.Add("Composer", GetType(String))
    dt.Columns.Add("Arranger", GetType(String))
    dt.Columns.Add("Messages", GetType(String))
    dt.Columns.Add("Code", GetType(String))
    dt.PrimaryKey = New DataColumn() {dt.Columns(0), dt.Columns(1)}
    Return dt
  End Function

  Private Shared Function GetBlockNo(bytes() As Byte, index As Integer) As Integer
    Return (bytes(index + 3) >> 4) And &H7
  End Function

  Private Shared Function GetTrackNo(bytes() As Byte, index As Integer) As Integer
    Return bytes(index + 1) And &H7F
  End Function

  Private Shared Function GetIsDoubleByte(bytes() As Byte, index As Integer) As Boolean
    Return (bytes(index + 3) And &H80) = &H80
  End Function
End Class

Friend Class EncodingManager

  Private Shared s_iso646Ascii As Encoding
  Private Shared ReadOnly Property Iso646Ascii As Encoding
    Get
      If s_iso646Ascii Is Nothing Then
        s_iso646Ascii = Encoding.GetEncoding(20127)
      End If
      Return s_iso646Ascii
    End Get
  End Property

  Private Shared s_iso88591 As Encoding
  Public Shared ReadOnly Property Iso88591 As Encoding
    Get
      If s_iso88591 Is Nothing Then
        s_iso88591 = Encoding.GetEncoding("ISO-8859-1")
      End If
      Return s_iso88591
    End Get
  End Property

  Private Shared s_mandarinStandardChinese As Encoding
  Private Shared ReadOnly Property MandarinStandardChinese As Encoding
    Get
      Return s_mandarinStandardChinese
    End Get
  End Property

  Public Shared Function GetEncoding(code As Integer) As Encoding
    Select Case code
      Case CharacterCode.ISO646ASCII
        Return EncodingManager.Iso646Ascii
      Case CharacterCode.ISO88591
        Return EncodingManager.Iso88591
      Case CharacterCode.Korean
        Throw New NotImplementedException("Don't know how to decode korean")
      Case CharacterCode.MandarinStandardChinese
        Return EncodingManager.MandarinStandardChinese
      Case CharacterCode.MSJIS
        Throw New NotImplementedException("Don't know how to decode MSJIS")
      Case Else
        Throw New NotImplementedException("Don't know how to decode character code: " & code)
    End Select
  End Function

End Class

Public Class IoctlResult
  Private Shared SenseRepository As IDictionary(Of Integer, IoctlResult)
  Private m_sk As Integer
  Public ReadOnly Property SenseKey() As Integer
    Get
      Return m_sk
    End Get
  End Property

  Private m_asc As Integer
  Public ReadOnly Property AdditionalSenseCode() As Integer
    Get
      Return m_asc
    End Get
  End Property

  Private m_ascq As Integer
  Public ReadOnly Property AdditionalSenseCodeQualifier() As Integer
    Get
      Return m_ascq
    End Get
  End Property

  Private m_scsiStatus As Integer
  Public ReadOnly Property ScsiStatus As Integer
    Get
      Return m_scsiStatus
    End Get
  End Property

  Private m_error As String
  Public Property ErrorMessage() As String
    Get
      Return String.Format("Error: {0} sk{1} asc{2} ascq{3}", m_error, m_sk, m_asc, m_ascq)
    End Get
    Set(ByVal value As String)
      m_error = value
    End Set
  End Property

  Private Sub New(sk As Integer, asc As Integer, ascq As Integer, errorMessage As String, scsiStatus As Integer)
    m_sk = sk
    m_asc = asc
    m_ascq = ascq
    m_error = errorMessage
    m_scsiStatus = scsiStatus
  End Sub

  Public Shared Function FromSenseBuffer(sense() As Byte, scsiStatus As Integer) As IoctlResult
    Dim sk As Integer = sense(2)
    Dim asc As Integer = sense(12)
    Dim ascq As Integer = sense(13)
    Dim key As Integer = HashCode(sk, asc, ascq)
    If IoctlResult.SenseRepository Is Nothing Then BuildRepository()
    Dim error1 As IoctlResult = Nothing
    If IoctlResult.SenseRepository.TryGetValue(key, error1) Then
      error1.m_scsiStatus = scsiStatus
    Else
      error1 = New IoctlResult(sk And &HF, asc, ascq, "Unknown!!", scsiStatus)
    End If
    Return error1
  End Function

  Private Shared Sub BuildRepository()
    Dim textReader As New IO.StringReader(My.Resources.SkAscAscq)
    IoctlResult.SenseRepository = New Dictionary(Of Integer, IoctlResult)
    While textReader.Peek() > -1
      Dim line As String = textReader.ReadLine
      Dim sk As Integer = 0
      Dim asc As Integer = 0
      Dim ascq As Integer = 0
      Dim spaceCount As Integer = 0
      Dim i As Integer = 0
      Dim val As Byte
      Do
        Dim c As Char = line(i)
        If c = " "c Then
          spaceCount += 1
          val = 0
        Else
          If line(i + 1) <> " "c Then
            val = (Byte.Parse(c, Globalization.NumberStyles.HexNumber) << 4)
          Else
            val = val Or (Byte.Parse(c, Globalization.NumberStyles.HexNumber))
            Select Case spaceCount
              Case 0
                sk = val
              Case 1
                asc = val
              Case 2
                ascq = val
            End Select
          End If
        End If
        i += 1
      Loop Until spaceCount = 3
      Dim key As Integer = HashCode(sk, asc, ascq)
      SenseRepository.Add(key, New IoctlResult(sk, asc, ascq, line.Substring(i), -1))
    End While
  End Sub
  Private Shared Function HashCode(sk As Integer, asc As Integer, ascq As Integer) As Integer
    If sk = 4 AndAlso asc = &H40 Then Return &HFF4004
    If sk = &HB AndAlso asc = &H4D Then Return &HFF4D0B
    If asc = &H34 AndAlso ascq = 0 Then Return &H34FF
    If asc = &H35 Then
      Select Case ascq
        Case 0
          Return &H35FF
        Case 1
          Return &H135FF
        Case 2
          Return &H235FF
        Case 3
          Return &H335FF
        Case 4
          Return &H435FF
      End Select
    End If
    Return (sk And &HF) Or ((asc << 8) And &HFF00) Or ((ascq << 16) And &HFF0000)
  End Function
  Public Overrides Function GetHashCode() As Integer
    Return HashCode(m_sk, m_asc, m_ascq)
  End Function
  Public Overrides Function ToString() As String
    Return String.Format("SK:{0:x2}/ASC:{1:x2}/ASCQ:{2:x2} ScsiStatus:{3} {4}", Me.SenseKey, Me.AdditionalSenseCode, Me.AdditionalSenseCodeQualifier, Me.ScsiStatus, Me.ErrorMessage)
  End Function
End Class

Public Class ReadCdTextWorkerResult

  Public Property Success() As Boolean

  Public Property ErrorMessage() As String

  Public Property CDText As CdText

End Class

Friend Class ReadCdTextWorker
  Inherits System.ComponentModel.BackgroundWorker

  Protected Overrides Sub OnDoWork(e As System.ComponentModel.DoWorkEventArgs)
    MyBase.OnDoWork(e)
    Dim result As New ReadCdTextWorkerResult
    Dim driveInfo As System.IO.DriveInfo = DirectCast(e.Argument, System.IO.DriveInfo)
    ' IsReady has to be in a non-ui thread as it can block for a long time when the CD drive
    ' is spinning up.
    If driveInfo.IsReady Then
      Try
        result.CDText = CdTextRetriever.GetCdText(driveInfo)
        result.Success = True
      Catch ex As Exception
        result.Success = False
      End Try
    Else
      Const fmt As String = "Drive {0} is not ready. Check a CD is inserted"
      result.ErrorMessage = String.Format(fmt, driveInfo)
    End If
    e.Result = result
  End Sub

End Class
