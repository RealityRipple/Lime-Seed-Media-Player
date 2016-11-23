Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Public Class CDBufferFiller
  Private BufferArray As Byte()
  Private WritePosition As Integer = 0

  Public Sub New(aBuffer As Byte())
    BufferArray = aBuffer
  End Sub
  Public Sub OnCdDataRead(sender As Object, ea As DataReadEventArgs)
    Buffer.BlockCopy(ea.Data, 0, BufferArray, WritePosition, CInt(ea.DataSize))
    WritePosition += CInt(ea.DataSize)
  End Sub

End Class

'Public Class CDDrivesEvents
'  Implements IDisposable
'  Public Event CDExists()
'  Public Event NoCDs()
'  Private Structure DriveData
'    Public info As IO.DriveInfo
'    Public disc As Boolean
'    Public Letter As Char
'    Public Handle As IntPtr
'  End Structure
'  Private cdDrives() As DriveData
'  Private NotWnd As DeviceChangeNotificationWindow = Nothing

'  Public Sub New()
'    Dim cdData = (From Drive In My.Computer.FileSystem.Drives Where Drive.DriveType = IO.DriveType.CDRom).ToArray
'    ReDim cdDrives(cdData.Length - 1)
'    For I As Integer = 0 To cdData.Length - 1
'      cdDrives(I).info = cdData(I)
'      cdDrives(I).disc = cdData(I).IsReady
'      Dim Letter As Char = cdData(I).Name
'      cdDrives(I).Letter = Letter
'      cdDrives(I).Handle = Win32Functions.CreateFile("\\.\" & Letter & ":"c, Win32Functions.GENERIC_READ, Win32Functions.FILE_SHARE_READ, IntPtr.Zero, Win32Functions.OPEN_EXISTING, 0, IntPtr.Zero)
'      If (CInt(cdDrives(I).Handle) <> -1) AndAlso (CInt(cdDrives(I).Handle) <> 0) Then
'        NotWnd = New DeviceChangeNotificationWindow()
'        AddHandler NotWnd.DeviceChange, New DeviceChangeEventHandler(AddressOf NotWnd_DeviceChange)
'      Else
'        Debug.Print("Failed to add handler for Drive " & Letter & "!")
'      End If
'    Next
'  End Sub

'  Public Sub UpdateStates()
'    For I As Integer = 0 To cdDrives.Length - 1
'      If cdDrives(I).disc Then
'        RaiseEvent CDExists()
'        return
'      End If
'    Next
'    RaiseEvent NoCDs()
'  End Sub

'  Private Sub NotWnd_DeviceChange(sender As Object, ea As DeviceChangeEventArgs)
'    For I As Integer = 0 To cdDrives.Length - 1
'      If ea.Drive = cdDrives(I).Letter Then
'        cdDrives(I).disc = ea.ChangeType = DeviceChangeEventType.DeviceInserted
'        Exit For
'      End If
'    Next
'    UpdateStates()
'  End Sub

'#Region "IDisposable Support"
'  Private disposedValue As Boolean ' To detect redundant calls

'  ' IDisposable
'  Protected Overridable Sub Dispose(disposing As Boolean)
'    If Not Me.disposedValue Then
'      If disposing Then
'        ' TODO: dispose managed state (managed objects).
'        For Each CDDrive In cdDrives
'          If (CInt(CDDrive.Handle) <> -1) AndAlso (CInt(CDDrive.Handle) <> 0) Then
'            RemoveHandler NotWnd.DeviceChange, New DeviceChangeEventHandler(AddressOf NotWnd_DeviceChange)
'            Win32Functions.CloseHandle(CDDrive.Handle)
'          End If
'        Next
'        If NotWnd IsNot Nothing Then
'          NotWnd.DestroyHandle()
'          NotWnd = Nothing
'        End If
'      End If

'      ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
'      ' TODO: set large fields to null.
'    End If
'    Me.disposedValue = True
'  End Sub

'  ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
'  'Protected Overrides Sub Finalize()
'  '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
'  '    Dispose(False)
'  '    MyBase.Finalize()
'  'End Sub

'  ' This code added by Visual Basic to correctly implement the disposable pattern.
'  Public Sub Dispose() Implements IDisposable.Dispose
'    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
'    Dispose(True)
'    GC.SuppressFinalize(Me)
'  End Sub
'#End Region

'End Class

Public Class CDDrive
  Implements IDisposable
  Private cdHandle As IntPtr
  Private TocValid As Boolean = False
  Private Toc As Win32Functions.CDROM_TOC = Nothing
  Private m_Drive As Char = ControlChars.NullChar
  Private NotWnd As DeviceChangeNotificationWindow = Nothing

  Public Event CDInserted As EventHandler
  Public Event CDRemoved As EventHandler

  Public Sub New()
    Toc = New Win32Functions.CDROM_TOC()
    cdHandle = IntPtr.Zero
  End Sub

  Public Function Open(Drive As Char) As Boolean
    Close()
    If Win32Functions.GetDriveType(Drive & ":\") = Win32Functions.DriveTypes.DRIVE_CDROM Then
      cdHandle = Win32Functions.CreateFile("\\.\" & Drive & ":"c, Win32Functions.GENERIC_READ, Win32Functions.FILE_SHARE_READ Or Win32Functions.FILE_SHARE_WRITE, IntPtr.Zero, Win32Functions.OPEN_EXISTING, 0, _
       IntPtr.Zero)
      If (CInt(cdHandle) <> -1) AndAlso (CInt(cdHandle) <> 0) Then
        m_Drive = Drive
        NotWnd = New DeviceChangeNotificationWindow()
        AddHandler NotWnd.DeviceChange, New DeviceChangeEventHandler(AddressOf NotWnd_DeviceChange)
        Return True
      Else
        Return True
      End If
    Else
      Return False
    End If
  End Function

  Public Sub Close()
    UnLockCD()
    If NotWnd IsNot Nothing Then
      NotWnd.DestroyHandle()
      NotWnd = Nothing
    End If
    If (CInt(cdHandle) <> -1) AndAlso (CInt(cdHandle) <> 0) Then
      Win32Functions.CloseHandle(cdHandle)
    End If
    cdHandle = IntPtr.Zero
    m_Drive = ControlChars.NullChar
    TocValid = False
  End Sub

  Public ReadOnly Property IsOpened() As Boolean
    Get
      Return (CInt(cdHandle) <> -1) AndAlso (CInt(cdHandle) <> 0)
    End Get
  End Property

  Public Sub Dispose() Implements IDisposable.Dispose
    Close()
    GC.SuppressFinalize(Me)
  End Sub

  Protected Overrides Sub Finalize()
    Try
      Dispose()
    Finally
      MyBase.Finalize()
    End Try
  End Sub

  Protected Function ReadTOC() As Boolean
    If (CInt(cdHandle) <> -1) AndAlso (CInt(cdHandle) <> 0) Then
      Dim BytesRead As UInteger = 0
      TocValid = Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_CDROM_READ_TOC, IntPtr.Zero, 0, Toc, CUInt(Marshal.SizeOf(Toc)), _
       BytesRead, IntPtr.Zero) <> 0
    Else
      TocValid = False
    End If
    Return TocValid
  End Function
  Friend Function GetStartSector(track As Integer) As Integer
    If TocValid AndAlso (track >= Toc.FirstTrack) AndAlso (track <= Toc.LastTrack) Then
      Dim td As Win32Functions.TRACK_DATA = Toc.TrackData(track - 1)
      Return (td.Address_1 * 60 * 75 + td.Address_2 * 75 + td.Address_3) - 150
    Else
      Return -1
    End If
  End Function
  Friend Function GetEndSector(track As Integer) As Integer
    If TocValid AndAlso (track >= Toc.FirstTrack) AndAlso (track <= Toc.LastTrack) Then
      Dim td As Win32Functions.TRACK_DATA = Toc.TrackData(track)
      Return (td.Address_1 * 60 * 75 + td.Address_2 * 75 + td.Address_3) - 151
    Else
      Return -1
    End If
  End Function

  Protected Const NSECTORS As Integer = 13
  Protected Const UNDERSAMPLING As Integer = 1
  Protected Const CB_CDDASECTOR As Integer = 2368
  Protected Const CB_QSUBCHANNEL As Integer = 16
  Protected Const CB_CDROMSECTOR As Integer = 2048
  Protected Const CB_AUDIO As Integer = (CB_CDDASECTOR - CB_QSUBCHANNEL)
  ''' <summary>
  ''' Read Audio Sectors
  ''' </summary>
  ''' <param name="sector">The sector where to start to read</param>
  ''' <param name="Buffer">The length must be at least CB_CDDASECTOR*Sectors bytes</param>
  ''' <param name="NumSectors">Number of sectors to read</param>
  ''' <returns>True on success</returns>
  Protected Function ReadSector(sector As Integer, Buffer As Byte(), NumSectors As Integer) As Boolean
    If TocValid AndAlso ((sector + NumSectors) <= GetEndSector(Toc.LastTrack)) AndAlso (Buffer.Length >= CB_AUDIO * NumSectors) Then
      Dim rri As New Win32Functions.RAW_READ_INFO()
      rri.TrackMode = Win32Functions.TRACK_MODE_TYPE.CDDA
      rri.SectorCount = CUInt(NumSectors)
      rri.DiskOffset = sector * CB_CDROMSECTOR

      Dim BytesRead As UInteger = 0
      If Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_CDROM_RAW_READ, rri, CUInt(Marshal.SizeOf(rri)), Buffer, CUInt(NumSectors) * CB_AUDIO, _
       BytesRead, IntPtr.Zero) <> 0 Then
        Return True
      Else
        Return False
      End If
    Else
      Return False
    End If
  End Function
  ''' <summary>
  ''' Lock the CD drive 
  ''' </summary>
  ''' <returns>True on success</returns>
  Public Function LockCD() As Boolean
    If (CInt(cdHandle) <> -1) AndAlso (CInt(cdHandle) <> 0) Then
      Dim Dummy As UInteger = 0
      Dim pmr As New Win32Functions.PREVENT_MEDIA_REMOVAL()
      pmr.PreventMediaRemoval = 1
      Return Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_STORAGE_MEDIA_REMOVAL, pmr, CUInt(Marshal.SizeOf(pmr)), IntPtr.Zero, 0, _
       Dummy, IntPtr.Zero) <> 0
    Else
      Return False
    End If
  End Function
  ''' <summary>
  ''' Unlock CD drive
  ''' </summary>
  ''' <returns>True on success</returns>
  Public Function UnLockCD() As Boolean
    If (CInt(cdHandle) <> -1) AndAlso (CInt(cdHandle) <> 0) Then
      Dim Dummy As UInteger = 0
      Dim pmr As New Win32Functions.PREVENT_MEDIA_REMOVAL()
      pmr.PreventMediaRemoval = 0
      Return Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_STORAGE_MEDIA_REMOVAL, pmr, CUInt(Marshal.SizeOf(pmr)), IntPtr.Zero, 0, _
       Dummy, IntPtr.Zero) <> 0
    Else
      Return False
    End If
  End Function
  ''' <summary>
  ''' Close the CD drive door
  ''' </summary>
  ''' <returns>True on success</returns>
  Public Function LoadCD() As Boolean
    TocValid = False
    If (CInt(cdHandle) <> -1) AndAlso (CInt(cdHandle) <> 0) Then
      Dim Dummy As UInteger = 0
      Return Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_STORAGE_LOAD_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, _
       Dummy, IntPtr.Zero) <> 0
    Else
      Return False
    End If
  End Function
  ''' <summary>
  ''' Open the CD drive door
  ''' </summary>
  ''' <returns>True on success</returns>
  Public Function EjectCD() As Boolean
    TocValid = False
    If (CInt(cdHandle) <> -1) AndAlso (CInt(cdHandle) <> 0) Then
      Dim Dummy As UInteger = 0
      Return Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, _
       Dummy, IntPtr.Zero) <> 0
    Else
      Return False
    End If
  End Function
  ''' <summary>
  ''' Check if there is CD in the drive
  ''' </summary>
  ''' <returns>True on success</returns>
  Public Function IsCDReady() As Boolean
    If (CInt(cdHandle) <> -1) AndAlso (CInt(cdHandle) <> 0) Then
      Dim Dummy As UInteger = 0
      If Win32Functions.DeviceIoControl(cdHandle, Win32Functions.IOCTL_STORAGE_CHECK_VERIFY, IntPtr.Zero, 0, IntPtr.Zero, 0, _
       Dummy, IntPtr.Zero) <> 0 Then
        Return True
      Else
        TocValid = False
        Return False
      End If
    Else
      TocValid = False
      Return False
    End If
  End Function
  ''' <summary>
  ''' If there is a CD in the drive read its TOC
  ''' </summary>
  ''' <returns>True on success</returns>
  Public Function Refresh() As Boolean
    If IsCDReady() Then
      Return ReadTOC()
    Else
      Return False
    End If
  End Function
  ''' <summary>
  ''' Return the number of tracks on the CD
  ''' </summary>
  ''' <returns>-1 on error</returns>
  Public Function GetNumTracks() As Integer
    If TocValid Then
      Return Toc.LastTrack - Toc.FirstTrack + 1
    Else
      Return -1
    End If
  End Function
  ''' <summary>
  ''' Return the number of audio tracks on the CD
  ''' </summary>
  ''' <returns>-1 on error</returns>
  Public Function GetNumAudioTracks() As Integer
    If TocValid Then
      Dim tracks As Integer = 0
      For i As Integer = Toc.FirstTrack - 1 To Toc.LastTrack - 1
        If (Toc.TrackData(i).Control And 4) = 0 Then
          tracks += 1
        End If
      Next
      Return tracks
    Else
      Return -1
    End If

  End Function
  ''' <summary>
  ''' Read the digital data of the track
  ''' </summary>
  ''' <param name="track">Track to read</param>
  ''' <param name="Data">Buffer that will receive the data</param>
  ''' <param name="DataSize">On return the size needed to read the track</param>
  ''' <param name="StartSecond">First second of the track to read, 0 means to start at beginning of the track</param>
  ''' <param name="Seconds2Read">Number of seconds to read, 0 means to read until the end of the track</param>
  ''' <param name="ProgressEvent">Delegate to indicate the reading progress</param>
  ''' <returns>Negative value means an error. On success returns the number of bytes read</returns>
  Public Function ReadTrack(track As Integer, Data As Byte(), ByRef DataSize As UInteger, StartSecond As UInteger, Seconds2Read As UInteger, ProgressEvent As CdReadProgressEventHandler) As Integer
    If TocValid AndAlso (track >= Toc.FirstTrack) AndAlso (track <= Toc.LastTrack) Then
      Dim StartSect As Integer = GetStartSector(track)
      Dim EndSect As Integer = GetEndSector(track)
      If StartSect + CInt(StartSecond) * 75 < EndSect Then StartSecond += CInt(StartSecond) * 75
      'If (StartSect += CInt(StartSecond) * 75) >= EndSect Then
      '  StartSect -= CInt(StartSecond) * 75
      'End If
      If (Seconds2Read > 0) AndAlso (CInt(StartSect + Seconds2Read * 75) < EndSect) Then
        EndSect = StartSect + CInt(Seconds2Read) * 75
      End If
      DataSize = CUInt(EndSect - StartSect) * CB_AUDIO
      If Data IsNot Nothing Then
        If Data.Length >= DataSize Then
          Dim BufferFiller As New CDBufferFiller(Data)
          Return ReadTrack(track, New CdDataReadEventHandler(AddressOf BufferFiller.OnCdDataRead), StartSecond, Seconds2Read, ProgressEvent)
        Else
          Return 0
        End If
      Else
        Return 0
      End If
    Else
      Return -1
    End If
  End Function
  ''' <summary>
  ''' Read the digital data of the track
  ''' </summary>
  ''' <param name="track">Track to read</param>
  ''' <param name="Data">Buffer that will receive the data</param>
  ''' <param name="DataSize">On return the size needed to read the track</param>
  ''' <param name="ProgressEvent">Delegate to indicate the reading progress</param>
  ''' <returns>Negative value means an error. On success returns the number of bytes read</returns>
  Public Function ReadTrack(track As Integer, Data As Byte(), ByRef DataSize As UInteger, ProgressEvent As CdReadProgressEventHandler) As Integer
    Return ReadTrack(track, Data, DataSize, 0, 0, ProgressEvent)
  End Function
  ''' <summary>
  ''' Read the digital data of the track
  ''' </summary>
  ''' <param name="track">Track to read</param>
  ''' <param name="DataReadEvent">Call each time data is read</param>
  ''' <param name="StartSecond">First second of the track to read, 0 means to start at beginning of the track</param>
  ''' <param name="Seconds2Read">Number of seconds to read, 0 means to read until the end of the track</param>
  ''' <param name="ProgressEvent">Delegate to indicate the reading progress</param>
  ''' <returns>Negative value means an error. On success returns the number of bytes read</returns>
  Public Function ReadTrack(track As Integer, DataReadEvent As CdDataReadEventHandler, StartSecond As UInteger, Seconds2Read As UInteger, ProgressEvent As CdReadProgressEventHandler) As Integer
    If Not TocValid Then Refresh()
    If TocValid AndAlso (track >= Toc.FirstTrack) AndAlso (track <= Toc.LastTrack) AndAlso (DataReadEvent IsNot Nothing) Then
      Dim StartSect As Integer = GetStartSector(track)
      Dim EndSect As Integer = GetEndSector(track)
      If StartSect + CInt(StartSecond) * 75 < EndSect Then StartSecond += CInt(StartSecond) * 75
      'If (StartSect += CInt(StartSecond) * 75) >= EndSect Then
      '    StartSect -= CInt(StartSecond) * 75
      '  End If
      If (Seconds2Read > 0) AndAlso (CInt(StartSect + Seconds2Read * 75) < EndSect) Then
        EndSect = StartSect + CInt(Seconds2Read) * 75
      End If
      Dim Bytes2Read As UInteger = CUInt(EndSect - StartSect) * CB_AUDIO
      Dim BytesRead As UInteger = 0
      Dim Data As Byte() = New Byte(CB_AUDIO * NSECTORS - 1) {}
      Dim Cont As Boolean = True
      Dim ReadOk As Boolean = True
      If ProgressEvent IsNot Nothing Then
        Dim rpa As New ReadProgressEventArgs(Bytes2Read, 0)
        ProgressEvent(Me, rpa)
        Cont = Not rpa.CancelRead
      End If
      Dim sector As Integer = StartSect
      While (sector < EndSect) AndAlso (Cont) 'AndAlso (ReadOk)
        Dim Sectors2Read As Integer = If(((sector + NSECTORS) < EndSect), NSECTORS, (EndSect - sector))
        ReadOk = ReadSector(sector, Data, Sectors2Read)
        If ReadOk Then
          Dim dra As New DataReadEventArgs(Data, CUInt(CB_AUDIO * Sectors2Read))
          DataReadEvent(Me, dra)
          BytesRead += CUInt(CB_AUDIO * Sectors2Read)
          If ProgressEvent IsNot Nothing Then
            Dim rpa As New ReadProgressEventArgs(Bytes2Read, BytesRead)
            ProgressEvent(Me, rpa)
            Cont = Not rpa.CancelRead
          End If
        Else
          Debug.Print("Failed to read sector " & sector & "!")
        End If
        sector += NSECTORS
      End While
      If ReadOk Then
        Return CInt(BytesRead)
      Else
        Return -1

      End If
    Else
      Return -1
    End If
  End Function
  ''' <summary>
  ''' Read the digital data of the track
  ''' </summary>
  ''' <param name="track">Track to read</param>
  ''' <param name="DataReadEvent">Call each time data is read</param>
  ''' <param name="ProgressEvent">Delegate to indicate the reading progress</param>
  ''' <returns>Negative value means an error. On success returns the number of bytes read</returns>
  Public Function ReadTrack(track As Integer, DataReadEvent As CdDataReadEventHandler, ProgressEvent As CdReadProgressEventHandler) As Integer
    Return ReadTrack(track, DataReadEvent, 0, 0, ProgressEvent)
  End Function
  ''' <summary>
  ''' Get track size
  ''' </summary>
  ''' <param name="track">Track</param>
  ''' <returns>Size in bytes of track data</returns>
  Public Function TrackSize(track As Integer) As UInteger
    Dim Size As UInteger = 0
    ReadTrack(track, Nothing, Size, Nothing)
    Return Size
  End Function

  Public Function IsAudioTrack(track As Integer) As Boolean
    If (TocValid) AndAlso (track >= Toc.FirstTrack) AndAlso (track <= Toc.LastTrack) Then
      Return (Toc.TrackData(track - 1).Control And 4) = 0
    Else
      Return False
    End If
  End Function

  Public Shared Function GetCDDriveLetters() As Char()
    Dim res As String = ""
    For I As Integer = Asc("A"c) To Asc("Z"c)
      Dim c As Char = Chr(I)
      If Win32Functions.GetDriveType(c & ":") = Win32Functions.DriveTypes.DRIVE_CDROM Then
        res &= c
      End If
    Next
    Return res.ToCharArray()
  End Function

  Private Sub OnCDInserted()
    RaiseEvent CDInserted(Me, EventArgs.Empty)
  End Sub

  Private Sub OnCDRemoved()
    RaiseEvent CDRemoved(Me, EventArgs.Empty)
  End Sub

  Private Sub NotWnd_DeviceChange(sender As Object, ea As DeviceChangeEventArgs)
    If ea.Drive = m_Drive Then
      TocValid = False
      Select Case ea.ChangeType
        Case DeviceChangeEventType.DeviceInserted
          OnCDInserted()
          Exit Select
        Case DeviceChangeEventType.DeviceRemoved
          OnCDRemoved()
          Exit Select
      End Select
    End If
  End Sub
End Class

Public Class DataReadEventArgs
  Inherits EventArgs
  Private m_Data As Byte()
  Private m_DataSize As UInteger
  Public Sub New(data As Byte(), size As UInteger)
    m_Data = data
    m_DataSize = size
  End Sub
  Public ReadOnly Property Data As Byte()
    Get
      Return m_Data
    End Get
  End Property
  Public ReadOnly Property DataSize() As UInteger
    Get
      Return m_DataSize
    End Get
  End Property
End Class

Public Class ReadProgressEventArgs
  Inherits EventArgs
  Private m_Bytes2Read As UInteger
  Private m_BytesRead As UInteger
  Private m_CancelRead As Boolean = False
  Public Sub New(bytes2read As UInteger, bytesread As UInteger)
    m_Bytes2Read = bytes2read
    m_BytesRead = bytesread
  End Sub
  Public ReadOnly Property Bytes2Read() As UInteger
    Get
      Return m_Bytes2Read
    End Get
  End Property
  Public ReadOnly Property BytesRead() As UInteger
    Get
      Return m_BytesRead
    End Get
  End Property
  Public Property CancelRead() As Boolean
    Get
      Return m_CancelRead
    End Get
    Set(value As Boolean)
      m_CancelRead = value
    End Set
  End Property
End Class

Friend Enum DeviceChangeEventType
  DeviceInserted
  DeviceRemoved
End Enum
Friend Class DeviceChangeEventArgs
  Inherits EventArgs
  Private m_Type As DeviceChangeEventType
  Private m_Drive As Char
  Public Sub New(drive As Char, type As DeviceChangeEventType)
    m_Drive = drive
    m_Type = type
  End Sub
  Public ReadOnly Property Drive() As Char
    Get
      Return m_Drive
    End Get
  End Property
  Public ReadOnly Property ChangeType() As DeviceChangeEventType
    Get
      Return m_Type
    End Get
  End Property
End Class
Public Delegate Sub CdDataReadEventHandler(sender As Object, ea As DataReadEventArgs)
Public Delegate Sub CdReadProgressEventHandler(sender As Object, ea As ReadProgressEventArgs)
Friend Delegate Sub DeviceChangeEventHandler(sender As Object, ea As DeviceChangeEventArgs)

Friend Enum DeviceType As UInteger
  DBT_DEVTYP_OEM = &H0
  ' oem-defined device type
  DBT_DEVTYP_DEVNODE = &H1
  ' devnode number
  DBT_DEVTYP_VOLUME = &H2
  ' logical volume
  DBT_DEVTYP_PORT = &H3
  ' serial, parallel
  DBT_DEVTYP_NET = &H4
  ' network resource
End Enum

Friend Enum VolumeChangeFlags As UShort
  DBTF_MEDIA = &H1
  ' media comings and goings
  DBTF_NET = &H2
  ' network volume
End Enum

<StructLayout(LayoutKind.Sequential)> _
Friend Structure DEV_BROADCAST_HDR
  Public dbch_size As UInteger
  Public dbch_devicetype As DeviceType
  Private dbch_reserved As UInteger
End Structure

<StructLayout(LayoutKind.Sequential)> _
Friend Structure DEV_BROADCAST_VOLUME
  Public dbcv_size As UInteger
  Public dbcv_devicetype As DeviceType
  Private dbcv_reserved As UInteger
  Private dbcv_unitmask As UInteger
  Public ReadOnly Property Drives() As Char()
    Get
      Dim drvs As String = ""
      For I As Integer = Asc("A"c) To Asc("Z"c)
        If (dbcv_unitmask And (1 << (I - Asc("A"c)))) <> 0 Then
          drvs &= Chr(I)
        End If
      Next
      Return drvs.ToCharArray()
    End Get
  End Property
  Public dbcv_flags As VolumeChangeFlags
End Structure

Friend Class DeviceChangeNotificationWindow
  Inherits NativeWindow
  Public Event DeviceChange As DeviceChangeEventHandler

  Const WS_EX_TOOLWINDOW As Integer = &H80
  Const WS_POPUP As Integer = -2147483648

  Const WM_DEVICECHANGE As Integer = &H219

  Const DBT_APPYBEGIN As Integer = &H0
  Const DBT_APPYEND As Integer = &H1
  Const DBT_DEVNODES_CHANGED As Integer = &H7
  Const DBT_QUERYCHANGECONFIG As Integer = &H17
  Const DBT_CONFIGCHANGED As Integer = &H18
  Const DBT_CONFIGCHANGECANCELED As Integer = &H19
  Const DBT_MONITORCHANGE As Integer = &H1B
  Const DBT_SHELLLOGGEDON As Integer = &H20
  Const DBT_CONFIGMGAPI32 As Integer = &H22
  Const DBT_VXDINITCOMPLETE As Integer = &H23
  Const DBT_VOLLOCKQUERYLOCK As Integer = &H8041
  Const DBT_VOLLOCKLOCKTAKEN As Integer = &H8042
  Const DBT_VOLLOCKLOCKFAILED As Integer = &H8043
  Const DBT_VOLLOCKQUERYUNLOCK As Integer = &H8044
  Const DBT_VOLLOCKLOCKRELEASED As Integer = &H8045
  Const DBT_VOLLOCKUNLOCKFAILED As Integer = &H8046
  Const DBT_DEVICEARRIVAL As Integer = &H8000
  Const DBT_DEVICEQUERYREMOVE As Integer = &H8001
  Const DBT_DEVICEQUERYREMOVEFAILED As Integer = &H8002
  Const DBT_DEVICEREMOVEPENDING As Integer = &H8003
  Const DBT_DEVICEREMOVECOMPLETE As Integer = &H8004
  Const DBT_DEVICETYPESPECIFIC As Integer = &H8005

  Public Sub New()
    Dim Params As New CreateParams()
    Params.ExStyle = WS_EX_TOOLWINDOW
    Params.Style = WS_POPUP
    CreateHandle(Params)
  End Sub

  Private Sub OnCDChange(ea As DeviceChangeEventArgs)
    RaiseEvent DeviceChange(Me, ea)
  End Sub
  Private Sub OnDeviceChange(DevDesc As DEV_BROADCAST_VOLUME, EventType As DeviceChangeEventType)
    'If DeviceChange IsNot Nothing Then
    For Each ch As Char In DevDesc.Drives
      Dim a As New DeviceChangeEventArgs(ch, EventType)
      RaiseEvent DeviceChange(Me, a)
    Next
    'End If
  End Sub

  Protected Overrides Sub WndProc(ByRef m As Message)
    If m.Msg = WM_DEVICECHANGE Then
      Dim head As DEV_BROADCAST_HDR
      Select Case m.WParam.ToInt32()
        'case DBT_DEVNODES_CHANGED :
        '            break;
        '          case DBT_CONFIGCHANGED :
        '            break;

        Case DBT_DEVICEARRIVAL
          head = CType(Marshal.PtrToStructure(m.LParam, GetType(DEV_BROADCAST_HDR)), DEV_BROADCAST_HDR)
          If head.dbch_devicetype = DeviceType.DBT_DEVTYP_VOLUME Then
            Dim DevDesc As DEV_BROADCAST_VOLUME = CType(Marshal.PtrToStructure(m.LParam, GetType(DEV_BROADCAST_VOLUME)), DEV_BROADCAST_VOLUME)
            If DevDesc.dbcv_flags = VolumeChangeFlags.DBTF_MEDIA Then
              OnDeviceChange(DevDesc, DeviceChangeEventType.DeviceInserted)
            End If
          End If
          Exit Select
          'case DBT_DEVICEQUERYREMOVE :
          '            break;
          '          case DBT_DEVICEQUERYREMOVEFAILED :
          '            break;
          '          case DBT_DEVICEREMOVEPENDING :
          '            break;

        Case DBT_DEVICEREMOVECOMPLETE
          head = CType(Marshal.PtrToStructure(m.LParam, GetType(DEV_BROADCAST_HDR)), DEV_BROADCAST_HDR)
          If head.dbch_devicetype = DeviceType.DBT_DEVTYP_VOLUME Then
            Dim DevDesc As DEV_BROADCAST_VOLUME = CType(Marshal.PtrToStructure(m.LParam, GetType(DEV_BROADCAST_VOLUME)), DEV_BROADCAST_VOLUME)
            If DevDesc.dbcv_flags = VolumeChangeFlags.DBTF_MEDIA Then
              OnDeviceChange(DevDesc, DeviceChangeEventType.DeviceRemoved)
            End If
          End If
          Exit Select
          'case DBT_DEVICETYPESPECIFIC :
          '            break;

      End Select
    End If
    MyBase.WndProc(m)
  End Sub
End Class

''' <summary>
''' Wrapper class for Win32 functions and structures needed to handle CD.
''' </summary>
Friend Class Win32Functions
  Public Enum DriveTypes As UInteger
    DRIVE_UNKNOWN = 0
    DRIVE_NO_ROOT_DIR
    DRIVE_REMOVABLE
    DRIVE_FIXED
    DRIVE_REMOTE
    DRIVE_CDROM
    DRIVE_RAMDISK
  End Enum

  <System.Runtime.InteropServices.DllImport("Kernel32.dll")>
  Public Shared Function GetDriveType(drive As String) As DriveTypes
  End Function

  'DesiredAccess values
  Public Const GENERIC_READ As UInteger = &H80000000UI
  Public Const GENERIC_WRITE As UInteger = &H40000000
  Public Const GENERIC_EXECUTE As UInteger = &H20000000
  Public Const GENERIC_ALL As UInteger = &H10000000

  'Share constants
  Public Const FILE_SHARE_READ As UInteger = &H1
  Public Const FILE_SHARE_WRITE As UInteger = &H2
  Public Const FILE_SHARE_DELETE As UInteger = &H4

  'CreationDisposition constants
  Public Const CREATE_NEW As UInteger = 1
  Public Const CREATE_ALWAYS As UInteger = 2
  Public Const OPEN_EXISTING As UInteger = 3
  Public Const OPEN_ALWAYS As UInteger = 4
  Public Const TRUNCATE_EXISTING As UInteger = 5

  ''' <summary>
  ''' Win32 CreateFile function, look for complete information at Platform SDK
  ''' </summary>
  ''' <param name="FileName">In order to read CD data FileName must be "\\.\\D:" where D is the CDROM drive letter</param>
  ''' <param name="DesiredAccess">Must be GENERIC_READ for CDROMs others access flags are not important in this case</param>
  ''' <param name="ShareMode">O means exlusive access, FILE_SHARE_READ allow open the CDROM</param>
  ''' <param name="lpSecurityAttributes">See Platform SDK documentation for details. NULL pointer could be enough</param>
  ''' <param name="CreationDisposition">Must be OPEN_EXISTING for CDROM drives</param>
  ''' <param name="dwFlagsAndAttributes">0 in fine for this case</param>
  ''' <param name="hTemplateFile">NULL handle in this case</param>
  ''' <returns>INVALID_HANDLE_VALUE on error or the handle to file if success</returns>
  <System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError:=True)> _
  Public Shared Function CreateFile(FileName As String, DesiredAccess As UInteger, ShareMode As UInteger, lpSecurityAttributes As IntPtr, CreationDisposition As UInteger, dwFlagsAndAttributes As UInteger, _
   hTemplateFile As IntPtr) As IntPtr
  End Function

  ''' <summary>
  ''' The CloseHandle function closes an open object handle.
  ''' </summary>
  ''' <param name="hObject">Handle to an open object.</param>
  ''' <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
  <System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError:=True)> _
  Public Shared Function CloseHandle(hObject As IntPtr) As Integer
  End Function

  Public Const IOCTL_CDROM_READ_TOC As UInteger = &H24000
  Public Const IOCTL_STORAGE_CHECK_VERIFY As UInteger = &H2D4800
  Public Const IOCTL_CDROM_RAW_READ As UInteger = &H2403E
  Public Const IOCTL_STORAGE_MEDIA_REMOVAL As UInteger = &H2D4804
  Public Const IOCTL_STORAGE_EJECT_MEDIA As UInteger = &H2D4808
  Public Const IOCTL_STORAGE_LOAD_MEDIA As UInteger = &H2D480C

  ''' <summary>
  ''' Most general form of DeviceIoControl Win32 function
  ''' </summary>
  ''' <param name="hDevice">Handle of device opened with CreateFile, <see cref="Win32Functions.CreateFile"/></param>
  ''' <param name="IoControlCode">Code of DeviceIoControl operation</param>
  ''' <param name="lpInBuffer">Pointer to a buffer that contains the data required to perform the operation.</param>
  ''' <param name="InBufferSize">Size of the buffer pointed to by lpInBuffer, in bytes.</param>
  ''' <param name="lpOutBuffer">Pointer to a buffer that receives the operation's output data.</param>
  ''' <param name="nOutBufferSize">Size of the buffer pointed to by lpOutBuffer, in bytes.</param>
  ''' <param name="lpBytesReturned">Receives the size, in bytes, of the data stored into the buffer pointed to by lpOutBuffer. </param>
  ''' <param name="lpOverlapped">Pointer to an OVERLAPPED structure. Discarded for this case</param>
  ''' <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
  <System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError:=True)> _
  Public Shared Function DeviceIoControl(hDevice As IntPtr, IoControlCode As UInteger, lpInBuffer As IntPtr, InBufferSize As UInteger, lpOutBuffer As IntPtr, nOutBufferSize As UInteger, _
   ByRef lpBytesReturned As UInteger, lpOverlapped As IntPtr) As Integer
  End Function

  <StructLayout(LayoutKind.Sequential)> _
  Public Structure TRACK_DATA
    Public Reserved As Byte
    Private BitMapped As Byte
    Public Property Control As Byte
      Get
        Return CByte(BitMapped And &HF)
      End Get
      Set(value As Byte)
        BitMapped = CByte((BitMapped And &HF0) Or (value And CByte(&HF)))
      End Set
    End Property
    Public Property Adr As Byte
      Get
        Return CByte((BitMapped And CByte(&HF0)) >> 4)
      End Get
      Set(value As Byte)
        BitMapped = CByte((BitMapped And CByte(&HF)) Or (value << 4))
      End Set
    End Property
    Public TrackNumber As Byte
    Public Reserved1 As Byte
    ''' <summary>
    ''' Don't use array to avoid array creation
    ''' </summary>
    Public Address_0 As Byte
    Public Address_1 As Byte
    Public Address_2 As Byte
    Public Address_3 As Byte
  End Structure

  Public Const MAXIMUM_NUMBER_TRACKS As Integer = 100

  <StructLayout(LayoutKind.Sequential)> _
  Public Class TrackDataList
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=MAXIMUM_NUMBER_TRACKS * 8)> _
    Private Data As Byte()
    Default Public ReadOnly Property Item(Index As Integer) As TRACK_DATA
      Get
        If (Index < 0) Or (Index >= MAXIMUM_NUMBER_TRACKS) Then
          Throw New IndexOutOfRangeException()
        End If
        Dim res As TRACK_DATA
        Dim handle As GCHandle = GCHandle.Alloc(Data, GCHandleType.Pinned)
        Try
          Dim buffer As IntPtr = handle.AddrOfPinnedObject()
          buffer = CType(buffer.ToInt32() + (Index * Marshal.SizeOf(GetType(TRACK_DATA))), IntPtr)
          res = CType(Marshal.PtrToStructure(buffer, GetType(TRACK_DATA)), TRACK_DATA)
        Finally
          handle.Free()
        End Try
        Return res
      End Get
    End Property
    Public Sub New()
      Data = New Byte(MAXIMUM_NUMBER_TRACKS * Marshal.SizeOf(GetType(TRACK_DATA)) - 1) {}
    End Sub
  End Class

  <StructLayout(LayoutKind.Sequential)> _
  Public Class CDROM_TOC
    Public Length As UShort
    Public FirstTrack As Byte = 0
    Public LastTrack As Byte = 0

    Public TrackData As TrackDataList

    Public Sub New()
      TrackData = New TrackDataList()
      Length = CUShort(Marshal.SizeOf(Me))
    End Sub
  End Class

  <StructLayout(LayoutKind.Sequential)> _
  Public Class PREVENT_MEDIA_REMOVAL
    Public PreventMediaRemoval As Byte = 0
  End Class

  Public Enum TRACK_MODE_TYPE
    YellowMode2
    XAForm2
    CDDA
  End Enum
  <StructLayout(LayoutKind.Sequential)> _
  Public Class RAW_READ_INFO
    Public DiskOffset As Long = 0
    Public SectorCount As UInteger = 0
    Public TrackMode As TRACK_MODE_TYPE = TRACK_MODE_TYPE.CDDA
  End Class

  ''' <summary>
  ''' Overload version of DeviceIOControl to read the TOC (Table of contents)
  ''' </summary>
  ''' <param name="hDevice">Handle of device opened with CreateFile, <see cref="Win32Functions.CreateFile"/></param>
  ''' <param name="IoControlCode">Must be IOCTL_CDROM_READ_TOC for this overload version</param>
  ''' <param name="InBuffer">Must be <code>IntPtr.Zero</code> for this overload version </param>
  ''' <param name="InBufferSize">Must be 0 for this overload version</param>
  ''' <param name="OutTOC">TOC object that receive the CDROM TOC</param>
  ''' <param name="OutBufferSize">Must be <code>(UInt32)Marshal.SizeOf(CDROM_TOC)</code> for this overload version</param>
  ''' <param name="BytesReturned">Receives the size, in bytes, of the data stored into OutTOC</param>
  ''' <param name="Overlapped">Pointer to an OVERLAPPED structure. Discarded for this case</param>
  ''' <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
  <System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError:=True)> _
  Public Shared Function DeviceIoControl(hDevice As IntPtr, IoControlCode As UInteger, InBuffer As IntPtr, InBufferSize As UInteger, <Out()> OutTOC As CDROM_TOC, OutBufferSize As UInteger, _
   ByRef BytesReturned As UInteger, Overlapped As IntPtr) As Integer
  End Function

  ''' <summary>
  ''' Overload version of DeviceIOControl to lock/unlock the CD
  ''' </summary>
  ''' <param name="hDevice">Handle of device opened with CreateFile, <see cref="Win32Functions.CreateFile"/></param>
  ''' <param name="IoControlCode">Must be IOCTL_STORAGE_MEDIA_REMOVAL for this overload version</param>
  ''' <param name="InMediaRemoval">Set the lock/unlock state</param>
  ''' <param name="InBufferSize">Must be <code>(UInt32)Marshal.SizeOf(PREVENT_MEDIA_REMOVAL)</code> for this overload version</param>
  ''' <param name="OutBuffer">Must be <code>IntPtr.Zero</code> for this overload version </param>
  ''' <param name="OutBufferSize">Must be 0 for this overload version</param>
  ''' <param name="BytesReturned">A "dummy" varible in this case</param>
  ''' <param name="Overlapped">Pointer to an OVERLAPPED structure. Discarded for this case</param>
  ''' <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
  <System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError:=True)> _
  Public Shared Function DeviceIoControl(hDevice As IntPtr, IoControlCode As UInteger, <[In]()> InMediaRemoval As PREVENT_MEDIA_REMOVAL, InBufferSize As UInteger, OutBuffer As IntPtr, OutBufferSize As UInteger, _
   ByRef BytesReturned As UInteger, Overlapped As IntPtr) As Integer
  End Function

  ''' <summary>
  ''' Overload version of DeviceIOControl to read digital data
  ''' </summary>
  ''' <param name="hDevice">Handle of device opened with CreateFile, <see cref="Win32Functions.CreateFile"/></param>
  ''' <param name="IoControlCode">Must be IOCTL_CDROM_RAW_READ for this overload version</param>
  ''' <param name="rri">RAW_READ_INFO structure</param>
  ''' <param name="InBufferSize">Size of RAW_READ_INFO structure</param>
  ''' <param name="OutBuffer">Buffer that will receive the data to be read</param>
  ''' <param name="OutBufferSize">Size of the buffer</param>
  ''' <param name="BytesReturned">Receives the size, in bytes, of the data stored into OutBuffer</param>
  ''' <param name="Overlapped">Pointer to an OVERLAPPED structure. Discarded for this case</param>
  ''' <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
  <System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError:=True)> _
  Public Shared Function DeviceIoControl(hDevice As IntPtr, IoControlCode As UInteger, <[In]()> rri As RAW_READ_INFO, InBufferSize As UInteger, <[In](), Out()> OutBuffer As Byte(), OutBufferSize As UInteger, _
   ByRef BytesReturned As UInteger, Overlapped As IntPtr) As Integer
  End Function
End Class
