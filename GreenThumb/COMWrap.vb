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

<ComVisible(True), Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(cominterfacetype.InterfaceIsIUnknown)>
Public Interface IShellItem
  Sub BindToHandler(ByVal pbc As IntPtr, <MarshalAs(UnmanagedType.LPStruct)> ByVal bhid As Guid, <MarshalAs(UnmanagedType.LPStruct)> ByVal riid As Guid, ByRef ppv As IntPtr)
  Sub GetParent(ByRef ppsi As IShellItem)
  Sub GetDisplayName(ByVal sigdnName As SIGDN, ByRef ppszName As IntPtr)
  Sub GetAttributes(ByVal sfgaoMask As UInt32, ByRef psfgaoAttribs As UInt32)
  Sub Compare(ByVal psi As IShellItem, ByVal hint As UInt32, ByRef piOrder As Integer)
End Interface
