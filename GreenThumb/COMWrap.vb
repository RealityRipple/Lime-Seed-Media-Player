Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.ComTypes
Public Enum WTS_ALPHATYPE
  WTSAT_UNKNOWN = 0
  WTSAT_RGB = 1
  WTSAT_ARGB = 2
End Enum

Public Enum SIGDN As UInteger
  NORMALDISPLAY = 0
  PARENTRELATIVEPARSING = &H80018001UI
  PARENTRELATIVEFORADDRESSBAR = &H8001C001UI
  DESKTOPABSOLUTEPARSING = &H80028000UI
  PARENTRELATIVEEDITING = &H80031001UI
  DESKTOPABSOLUTEEDITING = &H8004C000UI
  FILESYSPATH = &H80058000UI
  URL = &H80068000UI
End Enum

Public Enum typedef As Short
  VT_EMPTY = 0
  VT_NULL = 1
  VT_I2 = 2
  VT_I4 = 3
  VT_BOOL = 11
  VT_VARIANT = 12
  VT_I1 = 16
  VT_UI1 = 17
  VT_UI2 = 18
  VT_UI4 = 19
  VT_I8 = 20
  VT_UI8 = 21
  VT_LPWSTR = 31
  VT_BLOB = 65
  VT_CLSID = 72
  VT_VECTOR = &H1000
End Enum

<StructLayout(LayoutKind.Sequential)>
Public Structure PROPERTYKEY
  Public fmtid As Guid
  Public pid As UIntPtr
  Public Sub New(fmt As Guid, p As UIntPtr)
    fmtid = fmt
    pid = p
  End Sub
  Public Overrides Function ToString() As String
    Dim sFMT As String = fmtid.ToString("B").ToUpper
    Dim sPID As String = pid.ToString
    Select Case sFMT
      Case "{F29F85E0-4FF9-1068-AB91-08002B27B3D9}"
        sFMT = "Document"
        Select Case pid.ToUInt32
          Case 2 : sPID = "Title"
          Case 6 : sPID = "Comment"
        End Select
      Case "{56A3372E-CE9C-11D2-9F0E-006097C686F6}"
        sFMT = "Music"
        Select Case pid.ToUInt32
          Case 2 : sPID = "Artist"
          Case 4 : sPID = "Album"
          Case 5 : sPID = "Year"
          Case 7 : sPID = "Track"
          Case &HB : sPID = "Genre"
        End Select
      Case "{64440490-4C8B-11D1-8B70-080036B11A03}"
        sFMT = "Audio"
        Select Case pid.ToUInt32
          Case 2 : sPID = "Format"
          Case 3 : sPID = "Duration"
          Case 4 : sPID = "Bitrate"
          Case 5 : sPID = "Samplerate"
          Case 7 : sPID = "Channels"
        End Select
      Case "{64440491-4C8B-11D1-8B70-080036B11A03}"
        sFMT = "Video"
        Select Case pid.ToUInt32
          Case 2 : sPID = "Title"
          Case 3 : sPID = "Width"
          Case 4 : sPID = "Height"
          Case 5 : sPID = "Duration"
          Case 6 : sPID = "FrameRate"
          Case 8 : sPID = "Bitrate"
          Case &HA : sPID = "Format"
        End Select
      Case "{AEAC19E4-89AE-4508-B9B7-BB867ABEE2ED}"
        sFMT = "DRM"
        Select Case pid.ToUInt32
          Case 2 : sPID = "IsProtected"
        End Select
    End Select
    Return sFMT & " " & sPID
  End Function
End Structure

<StructLayout(LayoutKind.Sequential)>
Public Structure PropVariant2
  Public variantType As typedef
  Public Reserved1, Reserved2, Reserved3 As Short
  Public pointerValue As IntPtr
End Structure

<StructLayout(LayoutKind.Explicit, Size:=16)>
Public Structure PropVariant
  <FieldOffset(0)>
  <MarshalAs(UnmanagedType.U4)>
  Public type As VarEnum
  <FieldOffset(8)>
  Friend union As PropVariantUnion
End Structure

<StructLayout(LayoutKind.Explicit)> _
Public Structure PropVariantUnion
  <FieldOffset(0)> _
  Public ptr As IntPtr

  <FieldOffset(0)> _
  Public i1Value As SByte
  <FieldOffset(0)> _
  Public i2Value As Int16
  <FieldOffset(0)> _
  Public i4Value As Int32
  <FieldOffset(0)> _
  Public i8Value As Int64

  <FieldOffset(0)> _
  Public ui1Value As Byte
  <FieldOffset(0)> _
  Public ui2Value As UInt16
  <FieldOffset(0)> _
  Public ui4Value As UInt32
  <FieldOffset(0)> _
  Public ui8Value As UInt64

  <FieldOffset(0)> _
  Public boolValue As VARIANT_BOOL

  <FieldOffset(0)> _
  Public lpwstrValue As IntPtr

  <FieldOffset(0)> _
  Public filetimeValue As System.Runtime.InteropServices.ComTypes.FILETIME

End Structure

Public Enum VARIANT_BOOL As Int16
  VARIANT_TRUE = -1S
  VARIANT_FALSE = 0S
End Enum

<ComVisible(True), Guid("E357FCCD-A995-4576-B01F-234630154E96"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
Public Interface IThumbnailProvider
  Sub GetThumbnail(ByVal cx As UInteger, ByRef hBitmap As IntPtr, ByRef bitmapType As WTS_ALPHATYPE)
End Interface

<ComVisible(True), Guid("B7D14566-0509-4CCE-A71F-0A554233BD9B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
Public Interface IInitializeWithFile
  Sub Initialize(<MarshalAs(UnmanagedType.LPWStr)> ByVal pszFilePath As String, ByVal grfMode As UInt32)
End Interface

<ComVisible(True), Guid("7F73BE3F-FB79-493C-A6C7-7EE14E245841"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
Public Interface IInitializeWithItem
  Sub Initialize(ByVal psi As IShellItem, ByVal grfMode As Integer)
End Interface

<ComVisible(True), Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
Public Interface IShellItem
  Sub BindToHandler(ByVal pbc As IntPtr, <MarshalAs(UnmanagedType.LPStruct)> ByVal bhid As Guid, <MarshalAs(UnmanagedType.LPStruct)> ByVal riid As Guid, ByRef ppv As IntPtr)
  Sub GetParent(ByRef ppsi As IShellItem)
  Sub GetDisplayName(ByVal sigdnName As SIGDN, ByRef ppszName As IntPtr)
  Sub GetAttributes(ByVal sfgaoMask As UInt32, ByRef psfgaoAttribs As UInt32)
  Sub Compare(ByVal psi As IShellItem, ByVal hint As UInt32, ByRef piOrder As Integer)
End Interface

<ComImport(), Guid("c8e2d566-186e-4d49-bf41-6909ead56acc"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
Public Interface IPropertyStoreCapabilities
  <PreserveSig()>
  Function IsPropertyWritable(<[In]()> ByRef key As PROPERTYKEY) As Integer
End Interface

<ComImport(), Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
Public Interface IPropertyStore
  <PreserveSig()>
  Function GetCount(<Out()> ByRef cProps As UInteger) As Integer
  <PreserveSig()>
  Function GetAt(<[In]()> iProp As UInteger, <Out()> ByRef pkey As PROPERTYKEY) As Integer
  <PreserveSig()>
  Function GetValue(<[In]()> key As PROPERTYKEY, <Out()> ByRef pv As PropVariant) As Integer
  <PreserveSig()>
  Function SetValue(<[In]()> key As PROPERTYKEY, <[In]()> ByRef pv As Object) As Integer
  <PreserveSig()>
  Function Commit() As Integer
End Interface