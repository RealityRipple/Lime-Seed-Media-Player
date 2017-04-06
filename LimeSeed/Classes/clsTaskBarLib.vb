Imports System.Runtime.InteropServices
Imports Microsoft.WindowsAPICodePack.Taskbar
Namespace TaskbarLib
  <ComImport(), Guid("56FDF344-FD6D-11D0-958A-006097C9A090"), ClassInterface(CShort(0)), TypeLibType(CShort(2))>
  Public Class TaskbarListClass
    Implements ITaskbarList3, ITaskbarList2, ITaskbarList
    Public Sub New()
    End Sub
    Public Overridable Sub HrInit() Implements ITaskbarList3.HrInit, ITaskbarList2.HrInit, ITaskbarList.HrInit
    End Sub
    Public Overridable Sub ActivateTab(<[In]()> ByVal hwnd As Integer) Implements ITaskbarList3.ActivateTab, ITaskbarList2.ActivateTab, ITaskbarList.ActivateTab
    End Sub
    Public Overridable Sub AddTab(<[In]()> ByVal hwnd As Integer) Implements ITaskbarList3.AddTab, ITaskbarList2.AddTab, ITaskbarList.AddTab
    End Sub
    Public Overridable Sub DeleteTab(<[In]()> ByVal hwnd As Integer) Implements ITaskbarList3.DeleteTab, ITaskbarList2.DeleteTab, ITaskbarList.DeleteTab
    End Sub
    Public Overridable Sub MarkFullscreenWindow(<[In]()> ByVal hwnd As Integer, <[In]()> ByVal fFullscreen As Integer) Implements ITaskbarList3.MarkFullscreenWindow, ITaskbarList2.MarkFullscreenWindow
    End Sub
    Public Overridable Sub RegisterTab(<[In]()> ByVal hwndTab As Integer, <[In](), ComAliasName("TaskbarLib.wireHWND")> ByRef hwndMDI As RemotableHandle) Implements ITaskbarList3.RegisterTab
    End Sub
    Public Overridable Sub SetActivateAlt(<[In]()> ByVal hwnd As Integer) Implements ITaskbarList3.SetActivateAlt, ITaskbarList2.SetActivateAlt, ITaskbarList.SetActivateAlt
    End Sub
    Public Overridable Sub SetOverlayIcon(<[In]()> ByVal hwnd As Integer, <[In](), MarshalAs(UnmanagedType.IUnknown)> ByVal hIcon As Object, <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pszDescription As String) Implements ITaskbarList3.SetOverlayIcon
    End Sub
    Public Overridable Sub SetProgressState(<[In]()> ByVal hwnd As IntPtr, <[In]()> ByVal tbpFlags As TBPFLAG) Implements ITaskbarList3.SetProgressState
    End Sub
    Public Overridable Sub SetProgressValue(<[In]()> ByVal hwnd As IntPtr, <[In]()> ByVal ullCompleted As UInt64, <[In]()> ByVal ullTotal As UInt64) Implements ITaskbarList3.SetProgressValue
    End Sub
    Public Overridable Sub SetTabActive(<[In]()> ByVal hwndTab As Integer, <[In]()> ByVal hwndMDI As Integer, <[In]()> ByVal tbatFlags As TBATFLAG) Implements ITaskbarList3.SetTabActive
    End Sub
    Public Overridable Sub SetTabOrder(<[In]()> ByVal hwndTab As Integer, <[In]()> ByVal hwndInsertBefore As Integer) Implements ITaskbarList3.SetTabOrder
    End Sub
    Public Overridable Sub SetThumbnailClip(<[In]()> ByVal hwnd As Integer, <[In]()> ByRef prcClip As tagRECT) Implements ITaskbarList3.SetThumbnailClip
    End Sub
    Public Overridable Sub SetThumbnailTooltip(<[In]()> ByVal hwnd As Integer, <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pszTip As String) Implements ITaskbarList3.SetThumbnailTooltip
    End Sub
    Public Overridable Sub ThumbBarAddButtons(<[In]()> ByVal hwnd As Integer, <[In]()> ByVal cButtons As UInt32, <[In]()> ByRef pButton As tagTHUMBBUTTON) Implements ITaskbarList3.ThumbBarAddButtons
    End Sub
    Public Overridable Sub ThumbBarSetImageList(<[In]()> ByVal hwnd As Integer, <[In](), MarshalAs(UnmanagedType.IUnknown)> ByVal himl As Object) Implements ITaskbarList3.ThumbBarSetImageList
    End Sub
    Public Overridable Sub ThumbBarUpdateButtons(<[In]()> ByVal hwnd As Integer, <[In]()> ByVal cButtons As UInt32, <[In]()> ByRef pButton As tagTHUMBBUTTON) Implements ITaskbarList3.ThumbBarUpdateButtons
    End Sub
    Public Overridable Sub UnregisterTab(<[In]()> ByVal hwndTab As Integer) Implements ITaskbarList3.UnregisterTab
    End Sub
  End Class

  <ComImport(), InterfaceType(CShort(1)), Guid("56FDF342-FD6D-11D0-958A-006097C9A090")> Public Interface ITaskbarList
    Sub HrInit()
    Sub AddTab(<[In]()> ByVal hwnd As Integer)
    Sub DeleteTab(<[In]()> ByVal hwnd As Integer)
    Sub ActivateTab(<[In]()> ByVal hwnd As Integer)
    Sub SetActivateAlt(<[In]()> ByVal hwnd As Integer)
  End Interface

  <ComImport(), Guid("602D4995-B13A-429B-A66E-1935E44F4317"), InterfaceType(CShort(1))> Public Interface ITaskbarList2
    Inherits ITaskbarList
    Overloads Sub HrInit()
    Overloads Sub AddTab(<[In]()> ByVal hwnd As Integer)
    Overloads Sub DeleteTab(<[In]()> ByVal hwnd As Integer)
    Overloads Sub ActivateTab(<[In]()> ByVal hwnd As Integer)
    Overloads Sub SetActivateAlt(<[In]()> ByVal hwnd As Integer)
    Sub MarkFullscreenWindow(<[In]()> ByVal hwnd As Integer, <[In]()> ByVal fFullscreen As Integer)
  End Interface

  <ComImport(), InterfaceType(CShort(1)), Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF")> Public Interface ITaskbarList3
    Inherits ITaskbarList2
    Overloads Sub HrInit()
    Overloads Sub AddTab(<[In]()> ByVal hwnd As Integer)
    Overloads Sub DeleteTab(<[In]()> ByVal hwnd As Integer)
    Overloads Sub ActivateTab(<[In]()> ByVal hwnd As Integer)
    Overloads Sub SetActivateAlt(<[In]()> ByVal hwnd As Integer)
    Overloads Sub MarkFullscreenWindow(<[In]()> ByVal hwnd As Integer, <[In]()> ByVal fFullscreen As Integer)
    Sub SetProgressValue(<[In]()> ByVal hwnd As IntPtr, <[In]()> ByVal ullCompleted As UInt64, <[In]()> ByVal ullTotal As UInt64) 'UInt64
    Sub SetProgressState(<[In]()> ByVal hwnd As IntPtr, <[In]()> ByVal tbpFlags As TBPFLAG)
    Sub RegisterTab(<[In]()> ByVal hwndTab As Integer, <[In](), ComAliasName("TaskbarLib.wireHWND")> ByRef hwndMDI As RemotableHandle)
    Sub UnregisterTab(<[In]()> ByVal hwndTab As Integer)
    Sub SetTabOrder(<[In]()> ByVal hwndTab As Integer, <[In]()> ByVal hwndInsertBefore As Integer)
    Sub SetTabActive(<[In]()> ByVal hwndTab As Integer, <[In]()> ByVal hwndMDI As Integer, <[In]()> ByVal tbatFlags As TBATFLAG)
    Sub ThumbBarAddButtons(<[In]()> ByVal hwnd As Integer, <[In]()> ByVal cButtons As UInt32, <[In]()> ByRef pButton As tagTHUMBBUTTON)
    Sub ThumbBarUpdateButtons(<[In]()> ByVal hwnd As Integer, <[In]()> ByVal cButtons As UInt32, <[In]()> ByRef pButton As tagTHUMBBUTTON)
    Sub ThumbBarSetImageList(<[In]()> ByVal hwnd As Integer, <[In](), MarshalAs(UnmanagedType.IUnknown)> ByVal himl As Object)
    Sub SetOverlayIcon(<[In]()> ByVal hwnd As Integer, <[In](), MarshalAs(UnmanagedType.IUnknown)> ByVal hIcon As Object, <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pszDescription As String)
    Sub SetThumbnailTooltip(<[In]()> ByVal hwnd As Integer, <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pszTip As String)
    Sub SetThumbnailClip(<[In]()> ByVal hwnd As Integer, <[In]()> ByRef prcClip As tagRECT)
  End Interface

  <ComImport(), CoClass(GetType(TaskbarListClass)), Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF")> Public Interface TaskbarList
    Inherits ITaskbarList3
  End Interface

  <StructLayout(LayoutKind.Sequential, Pack:=4)> Public Structure tagRECT
    Public left As Integer
    Public top As Integer
    Public right As Integer
    Public bottom As Integer
  End Structure

  <StructLayout(LayoutKind.Sequential, Pack:=4)> Public Structure RemotableHandle
    Public fContext As Integer
    Public u As IWinTypes
  End Structure

  <StructLayout(LayoutKind.Explicit, Pack:=4)> Public Structure IWinTypes
    <FieldOffset(0)> Public hInproc As Integer
    <FieldOffset(0)> Public hRemote As Integer
  End Structure
  <StructLayout(LayoutKind.Sequential, Pack:=4)> Public Structure tagTHUMBBUTTON
    Public dwMask As UInt32
    Public iId As UInt32
    Public iBitmap As UInt32
    <MarshalAs(UnmanagedType.IUnknown)> Public hIcon As Object
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)> Public szTip As UInt16()
    Public dwFlags As UInt32
  End Structure

  <Flags()> Public Enum TBATFLAG
    TBATF_USEMDILIVEPREVIEW = 2
    TBATF_USEMDITHUMBNAIL = 1
  End Enum

  <Flags()> Public Enum TBPFLAG
    TBPF_ERROR = 4
    TBPF_INDETERMINATE = 1
    TBPF_NOPROGRESS = 0
    TBPF_NORMAL = 2
    TBPF_PAUSED = 8
  End Enum
End Namespace

Public Structure ThumbnailToolBarButtons
  Public Structure ButtonAndToolTip
    Public Icon As Drawing.Icon
    Public ToolTipText As String
    Public Sub New(image As Drawing.Icon, tt As String)
      Icon = image.Clone
      ToolTipText = tt.Clone
    End Sub
  End Structure
  Public Back As ButtonAndToolTip
  Public Play As ButtonAndToolTip
  Public Pause As ButtonAndToolTip
  Public [Stop] As ButtonAndToolTip
  Public [Next] As ButtonAndToolTip
  Public Sub New(btBack As ButtonAndToolTip, btPlay As ButtonAndToolTip, btPause As ButtonAndToolTip, btStop As ButtonAndToolTip, btNext As ButtonAndToolTip)
    Back = btBack
    Play = btPlay
    Pause = btPause
    [Stop] = btStop
    [Next] = btNext
  End Sub
End Structure
Public Class TabbedThumbnailAppEventArgs
  Inherits TabbedThumbnailEventArgs
  Public Enum Events
    Activated
    Closed
    Maximized
    Minimized
  End Enum
  Public EventType As Events
  Public Sub New(Handle As IntPtr, Type As Events)
    MyBase.New(Handle)
    EventType = Type
  End Sub
End Class

Public Class TaskbarController_Image
  Implements IDisposable
  Private WithEvents ttbBack, ttbPlayPause, ttbStop, ttbNext As ThumbnailToolBarButton
  Private c_MainHandle As IntPtr
  Private tmpObj As Control
  Private WithEvents taskThumb As TabbedThumbnail
  Private c_Buttons As ThumbnailToolBarButtons
  Private c_Icon As Drawing.Icon
  Private c_Title, c_Tooltip As String

  Public Event AppEvent(sender As Object, e As TabbedThumbnailAppEventArgs)
  Public Event Button_Click(sender As Object, e As ThumbnailButtonClickedEventArgs)
  Private WithEvents tmrEvent As Timer

  Private bIsPause As Boolean
  'Private syncTask As New Object
  'Private syncThumb As New Object

  Public Sub New(hWnd As IntPtr, Preview As Drawing.Image, Buttons As ThumbnailToolBarButtons, Icon As Drawing.Icon, Title As String, Tooltip As String)
    If hWnd = IntPtr.Zero Then Return
    If Preview Is Nothing Then Return
    If Not clsGlass.IsCompositionEnabled Then Return
    If Not TaskbarManager.IsPlatformSupported Then Return
    'SyncLock syncThumb
    'SyncLock syncTask
    c_MainHandle = hWnd
    c_Buttons = Buttons
    c_Icon = Icon.Clone
    c_Title = Title.Clone
    c_Tooltip = Tooltip.Clone
    tmpObj = New Control
    ttbBack = New ThumbnailToolBarButton(c_Buttons.Back.Icon.Clone, c_Buttons.Back.ToolTipText.Clone) With {.Enabled = True, .Name = "ttbBack"}
    ttbPlayPause = New ThumbnailToolBarButton(c_Buttons.Play.Icon.Clone, c_Buttons.Play.ToolTipText.Clone) With {.Enabled = True, .Name = "ttbPlayPause"}
    ttbStop = New ThumbnailToolBarButton(c_Buttons.Stop.Icon.Clone, c_Buttons.Stop.ToolTipText.Clone) With {.Enabled = True, .Name = "ttbStop"}
    ttbNext = New ThumbnailToolBarButton(c_Buttons.Next.Icon.Clone, c_Buttons.Next.ToolTipText.Clone) With {.Enabled = True, .Name = "ttbNext"}
    TaskbarManager.Instance.ThumbnailToolBars.AddButtons(tmpObj.Handle, ttbBack, ttbPlayPause, ttbStop, ttbNext)
    taskThumb = New TabbedThumbnail(c_MainHandle, tmpObj)
    TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(taskThumb)
    Dim tThumb As TabbedThumbnail = TaskbarManager.Instance.TabbedThumbnail.GetThumbnailPreview(tmpObj)
    Using bmpPreview As New Drawing.Bitmap(Preview)
      If tThumb IsNot Nothing Then tThumb.SetImage(bmpPreview)
    End Using
    taskThumb.Title = c_Title.Clone
    taskThumb.Tooltip = c_Tooltip.Clone
    If c_Icon IsNot Nothing Then taskThumb.SetWindowIcon(c_Icon.Clone)
    'End SyncLock
    'End SyncLock
  End Sub

  Public Property Title As String
    Get
      'SyncLock syncTask
      Return c_Title.Clone
      'End SyncLock
    End Get
    Set(value As String)
      'SyncLock syncTask
      c_Title = value.Clone
      taskThumb.Title = c_Title.Clone
      'End SyncLock
    End Set
  End Property

  Public Property Tooltip As String
    Get
      'SyncLock syncTask
      Return c_Tooltip.Clone
      'End SyncLock
    End Get
    Set(value As String)
      'SyncLock syncTask
      c_Tooltip = value.Clone
      taskThumb.Tooltip = c_Tooltip.Clone
      'End SyncLock
    End Set
  End Property

  Public Property Icon As Drawing.Icon
    Get
      'SyncLock syncTask
      Return c_Icon.Clone
      'End SyncLock
    End Get
    Set(value As Drawing.Icon)
      'SyncLock syncTask
      c_Icon = value.Clone
      taskThumb.SetWindowIcon(c_Icon.Clone)
      'End SyncLock
    End Set
  End Property

  Public Property BackEnabled As Boolean
    Get
      'SyncLock syncTask
      Return ttbBack.Enabled
      'End SyncLock
    End Get
    Set(value As Boolean)
      'SyncLock syncTask
      ttbBack.Enabled = value
      'End SyncLock
    End Set
  End Property

  Public Property PlayPauseEnabled As Boolean
    Get
      'SyncLock syncTask
      Return ttbPlayPause.Enabled
      'End SyncLock
    End Get
    Set(value As Boolean)
      'SyncLock syncTask
      ttbPlayPause.Enabled = value
      'End SyncLock
    End Set
  End Property

  Public Property StopEnabled As Boolean
    Get
      'SyncLock syncTask
      Return ttbStop.Enabled
      'End SyncLock
    End Get
    Set(value As Boolean)
      'SyncLock syncTask
      ttbStop.Enabled = value
      'End SyncLock
    End Set
  End Property

  Public Property NextEnabled As Boolean
    Get
      'SyncLock syncTask
      Return ttbStop.Enabled
      'End SyncLock
    End Get
    Set(value As Boolean)
      'SyncLock syncTask
      ttbStop.Enabled = value
      'End SyncLock
    End Set
  End Property

  Public Property IsPause As Boolean
    Get
      'SyncLock syncTask
      Return bIsPause
      'End SyncLock
    End Get
    Set(value As Boolean)
      'SyncLock syncTask
      bIsPause = value
      If bIsPause Then
        ttbPlayPause.Icon = c_Buttons.Pause.Icon.Clone
        ttbPlayPause.Tooltip = c_Buttons.Pause.ToolTipText.Clone
      Else
        ttbPlayPause.Icon = c_Buttons.Play.Icon.Clone
        ttbPlayPause.Tooltip = c_Buttons.Play.ToolTipText.Clone
      End If
      'End SyncLock
    End Set
  End Property

  Private Sub ttbBack_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbBack.Click
    RunEvent(sender, e)
  End Sub

  Private Sub ttbNext_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbNext.Click
    RunEvent(sender, e)
  End Sub

  Private Sub ttbPlayPause_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbPlayPause.Click
    RunEvent(sender, e)
  End Sub

  Private Sub ttbStop_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbStop.Click
    RunEvent(sender, e)
  End Sub

  Private Sub RunEvent(sender As Object, e As EventArgs)
    tmrEvent = Nothing
    tmrEvent = New Timer
    tmrEvent.Tag = {sender, e}
    tmrEvent.Interval = 1
    tmrEvent.Start()
  End Sub

  Private Sub tmrEvent_Tick(sender As Object, e As System.EventArgs) Handles tmrEvent.Tick
    tmrEvent.Stop()
    Dim eSender As Object = tmrEvent.Tag(0)
    Dim eE As EventArgs = tmrEvent.Tag(1)
    If eSender.GetType Is GetType(TabbedThumbnail) Then
      RaiseEvent AppEvent(eSender, eE)
    Else
      RaiseEvent Button_Click(eSender, eE)
    End If
    tmrEvent = Nothing
  End Sub

  Private Sub taskThumb_TabbedThumbnailActivated(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailActivated
    RunEvent(sender, New TabbedThumbnailAppEventArgs(e.WindowHandle, TabbedThumbnailAppEventArgs.Events.Activated))
    'Stop
  End Sub

  Private Sub taskThumb_TabbedThumbnailClosed(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailClosed
    RunEvent(sender, New TabbedThumbnailAppEventArgs(e.WindowHandle, TabbedThumbnailAppEventArgs.Events.Closed))
    'Stop
  End Sub

  Private Sub taskThumb_TabbedThumbnailMaximized(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailMaximized
    RunEvent(sender, New TabbedThumbnailAppEventArgs(e.WindowHandle, TabbedThumbnailAppEventArgs.Events.Maximized))
    'Stop
  End Sub

  Private Sub taskThumb_TabbedThumbnailMinimized(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailMinimized
    RunEvent(sender, New TabbedThumbnailAppEventArgs(e.WindowHandle, TabbedThumbnailAppEventArgs.Events.Minimized))
    'Stop
  End Sub

#Region "IDisposable Support"
  Private disposedValue As Boolean 
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        'SyncLock syncThumb
        'SyncLock syncTask
        If tmpObj IsNot Nothing Then
          If TaskbarManager.Instance.TabbedThumbnail.IsThumbnailPreviewAdded(tmpObj) Then
            Try
              TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(tmpObj)
            Catch ex As Exception

            End Try
          End If
          tmpObj.Dispose()
          tmpObj = Nothing
        End If
        If taskThumb IsNot Nothing Then
          If TaskbarManager.Instance.TabbedThumbnail.IsThumbnailPreviewAdded(taskThumb) Then
            Try
              TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(taskThumb)
            Catch ex As Exception

            End Try
          End If
          taskThumb.Dispose()
          taskThumb = Nothing
        End If
        TabbedThumbnailManager.ClearThumbnailClip(c_MainHandle)
        If ttbBack IsNot Nothing Then
          ttbBack.Dispose()
          ttbBack = Nothing
        End If
        If ttbPlayPause IsNot Nothing Then
          ttbPlayPause.Dispose()
          ttbPlayPause = Nothing
        End If
        If ttbStop IsNot Nothing Then
          ttbStop.Dispose()
          ttbStop = Nothing
        End If
        If ttbNext IsNot Nothing Then
          ttbNext.Dispose()
          ttbNext = Nothing
        End If
        'End SyncLock
        'End SyncLock
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

Public Class TaskbarController_Clip
  Implements IDisposable
  Private WithEvents ttbBack, ttbPlayPause, ttbStop, ttbNext As ThumbnailToolBarButton
  Private c_MainHandle As IntPtr
  Private WithEvents taskThumb As TabbedThumbnail
  Private c_Buttons As ThumbnailToolBarButtons
  Private c_Icon As Drawing.Icon
  Private c_Title, c_Tooltip As String

  Public Event AppEvent(sender As Object, e As TabbedThumbnailAppEventArgs)
  Public Event Button_Click(sender As Object, e As ThumbnailButtonClickedEventArgs)
  Private WithEvents tmrEvent As Timer

  Private bIsPause As Boolean
  Private syncTask As New Object
  Private syncThumb As New Object

  Public Sub New(hWnd As IntPtr, Preview As Object, Buttons As ThumbnailToolBarButtons, Icon As Drawing.Icon, Title As String, Tooltip As String)
    If hWnd = IntPtr.Zero Then Return
    If Preview Is Nothing Then Return
    If Not clsGlass.IsCompositionEnabled Then Return
    If Not TaskbarManager.IsPlatformSupported Then Return
    'SyncLock syncThumb
    'SyncLock syncTask
    c_MainHandle = hWnd
    c_Buttons = Buttons
    c_Icon = Icon.Clone
    c_Title = Title.Clone
    c_Tooltip = Tooltip.Clone
    ttbBack = New ThumbnailToolBarButton(c_Buttons.Back.Icon.Clone, c_Buttons.Back.ToolTipText.Clone) With {.Enabled = True, .Name = "ttbBack"}
    ttbPlayPause = New ThumbnailToolBarButton(c_Buttons.Play.Icon.Clone, c_Buttons.Play.ToolTipText.Clone) With {.Enabled = True, .Name = "ttbPlayPause"}
    ttbStop = New ThumbnailToolBarButton(c_Buttons.Stop.Icon.Clone, c_Buttons.Stop.ToolTipText.Clone) With {.Enabled = True, .Name = "ttbStop"}
    ttbNext = New ThumbnailToolBarButton(c_Buttons.Next.Icon.Clone, c_Buttons.Next.ToolTipText.Clone) With {.Enabled = True, .Name = "ttbNext"}
    TaskbarManager.Instance.ThumbnailToolBars.AddButtons(c_MainHandle, ttbBack, ttbPlayPause, ttbStop, ttbNext)
    taskThumb = New TabbedThumbnail(c_MainHandle, c_MainHandle)
    Dim loc As Drawing.Point = GetExactLoc(Preview)
    TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(c_MainHandle, New Drawing.Rectangle(loc, Preview.Size))
    taskThumb.Title = c_Title.Clone
    taskThumb.Tooltip = c_Tooltip.Clone
    If c_Icon IsNot Nothing Then taskThumb.SetWindowIcon(c_Icon.Clone)
    'End SyncLock
    'End SyncLock
  End Sub

  Public Sub UpdatePreview(obj As Control)
    'SyncLock syncThumb
    'SyncLock syncTask
    If obj Is Nothing Then Return
    If Not clsGlass.IsCompositionEnabled Then Return
    If Not TaskbarManager.IsPlatformSupported Then Return
    Dim loc As Drawing.Point = GetExactLoc(obj)
    TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(c_MainHandle, New Drawing.Rectangle(loc, obj.Size))
    'End SyncLock
    'End SyncLock
  End Sub

  Public Property Title As String
    Get
      'SyncLock syncTask
      Return c_Title.Clone
      'End SyncLock
    End Get
    Set(value As String)
      'SyncLock syncTask
      c_Title = value.Clone
      taskThumb.Title = c_Title.Clone
      'End SyncLock
    End Set
  End Property

  Public Property Tooltip As String
    Get
      'SyncLock syncTask
      Return c_Tooltip.Clone
      'End SyncLock
    End Get
    Set(value As String)
      'SyncLock syncTask
      c_Tooltip = value.Clone
      taskThumb.Tooltip = c_Tooltip.Clone
      'End SyncLock
    End Set
  End Property

  Public Property Icon As Drawing.Icon
    Get
      'SyncLock syncTask
      Return c_Icon.Clone
      'End SyncLock
    End Get
    Set(value As Drawing.Icon)
      'SyncLock syncTask
      c_Icon = value.Clone
      taskThumb.SetWindowIcon(c_Icon.Clone)
      'End SyncLock
    End Set
  End Property

  Public Property BackEnabled As Boolean
    Get
      'SyncLock syncTask
      Return ttbBack.Enabled
      'End SyncLock
    End Get
    Set(value As Boolean)
      'SyncLock syncTask
      ttbBack.Enabled = value
      'End SyncLock
    End Set
  End Property

  Public Property PlayPauseEnabled As Boolean
    Get
      'SyncLock syncTask
      Return ttbPlayPause.Enabled
      'End SyncLock
    End Get
    Set(value As Boolean)
      'SyncLock syncTask
      ttbPlayPause.Enabled = value
      'End SyncLock
    End Set
  End Property

  Public Property StopEnabled As Boolean
    Get
      'SyncLock syncTask
      Return ttbStop.Enabled
      'End SyncLock
    End Get
    Set(value As Boolean)
      'SyncLock syncTask
      ttbStop.Enabled = value
      'End SyncLock
    End Set
  End Property

  Public Property NextEnabled As Boolean
    Get
      'SyncLock syncTask
      Return ttbStop.Enabled
      'End SyncLock
    End Get
    Set(value As Boolean)
      'SyncLock syncTask
      ttbStop.Enabled = value
      'End SyncLock
    End Set
  End Property

  Public Property IsPause As Boolean
    Get
      'SyncLock syncTask
      Return bIsPause
      'End SyncLock
    End Get
    Set(value As Boolean)
      'SyncLock syncTask
      bIsPause = value
      If bIsPause Then
        ttbPlayPause.Icon = c_Buttons.Pause.Icon.Clone
        ttbPlayPause.Tooltip = c_Buttons.Pause.ToolTipText.Clone
      Else
        ttbPlayPause.Icon = c_Buttons.Play.Icon.Clone
        ttbPlayPause.Tooltip = c_Buttons.Play.ToolTipText.Clone
      End If
      'End SyncLock
    End Set
  End Property

  Private Sub ttbBack_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbBack.Click
    RunEvent(sender, e)
  End Sub

  Private Sub ttbNext_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbNext.Click
    RunEvent(sender, e)
  End Sub

  Private Sub ttbPlayPause_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbPlayPause.Click
    RunEvent(sender, e)
  End Sub

  Private Sub ttbStop_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbStop.Click
    RunEvent(sender, e)
  End Sub

  Private Sub RunEvent(sender As Object, e As EventArgs)
    tmrEvent = Nothing
    tmrEvent = New Timer
    tmrEvent.Tag = {sender, e}
    tmrEvent.Interval = 1
    tmrEvent.Start()
  End Sub

  Private Sub tmrEvent_Tick(sender As Object, e As System.EventArgs) Handles tmrEvent.Tick
    tmrEvent.Stop()
    Dim eSender As Object = tmrEvent.Tag(0)
    Dim eE As EventArgs = tmrEvent.Tag(1)
    If eSender.GetType Is GetType(TabbedThumbnail) Then
      RaiseEvent AppEvent(eSender, eE)
    Else
      RaiseEvent Button_Click(eSender, eE)
    End If
    tmrEvent = Nothing
  End Sub

  Private Sub taskThumb_TabbedThumbnailActivated(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailActivated
    RunEvent(sender, New TabbedThumbnailAppEventArgs(e.WindowHandle, TabbedThumbnailAppEventArgs.Events.Activated))
    'Stop
  End Sub

  Private Sub taskThumb_TabbedThumbnailClosed(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailClosed
    RunEvent(sender, New TabbedThumbnailAppEventArgs(e.WindowHandle, TabbedThumbnailAppEventArgs.Events.Closed))
    'Stop
  End Sub

  Private Sub taskThumb_TabbedThumbnailMaximized(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailMaximized
    RunEvent(sender, New TabbedThumbnailAppEventArgs(e.WindowHandle, TabbedThumbnailAppEventArgs.Events.Maximized))
    'Stop
  End Sub

  Private Sub taskThumb_TabbedThumbnailMinimized(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailMinimized
    RunEvent(sender, New TabbedThumbnailAppEventArgs(e.WindowHandle, TabbedThumbnailAppEventArgs.Events.Minimized))
    'Stop
  End Sub

  Private Function GetExactLoc(obj As Control) As Drawing.Point
    If obj Is Nothing Then Return Drawing.Point.Empty
    Dim loc As Drawing.Point
    Dim parobj As Object = obj
    Do
      loc.X += parobj.Location.X
      loc.Y += parobj.Location.Y
      parobj = parobj.Parent
    Loop Until parobj Is Nothing OrElse parobj.Parent Is Nothing
    Return loc
  End Function

#Region "IDisposable Support"
  Private disposedValue As Boolean
  Protected Overridable Sub Dispose(disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        'SyncLock syncThumb
        'SyncLock syncTask
        If taskThumb IsNot Nothing Then
          If TaskbarManager.Instance.TabbedThumbnail.IsThumbnailPreviewAdded(taskThumb) Then
            Try
              TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(taskThumb)
            Catch ex As Exception

            End Try
          End If
          taskThumb.Dispose()
          taskThumb = Nothing
        End If
        TabbedThumbnailManager.ClearThumbnailClip(c_MainHandle)
        If ttbBack IsNot Nothing Then
          ttbBack.Dispose()
          ttbBack = Nothing
        End If
        If ttbPlayPause IsNot Nothing Then
          ttbPlayPause.Dispose()
          ttbPlayPause = Nothing
        End If
        If ttbStop IsNot Nothing Then
          ttbStop.Dispose()
          ttbStop = Nothing
        End If
        If ttbNext IsNot Nothing Then
          ttbNext.Dispose()
          ttbNext = Nothing
        End If
        'End SyncLock
        'End SyncLock
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

Public Class TaskbarController
  Private WithEvents tcImage As TaskbarController_Image
  Private WithEvents tcClip As TaskbarController_Clip

  Private c_MainHandle As IntPtr
  Private c_Buttons As ThumbnailToolBarButtons
  Private c_Icon As Drawing.Icon
  Private c_Title, c_Tooltip As String

  Public Event AppEvent(sender As Object, e As TabbedThumbnailAppEventArgs)
  Public Event Button_Click(sender As Object, e As ThumbnailButtonClickedEventArgs)

  Public ReadOnly Property ImageEnabled As Boolean
    Get
      Return tcImage IsNot Nothing
    End Get
  End Property

  Public Sub New(hWnd As IntPtr, Buttons As ThumbnailToolBarButtons, Icon As Drawing.Icon, Title As String, Tooltip As String)
    c_MainHandle = hWnd
    c_Buttons = Buttons
    c_Icon = Icon.Clone
    c_Title = Title.Clone
    c_Tooltip = Tooltip.Clone
  End Sub

  Public Property Title As String
    Get
      Return c_Title.Clone
    End Get
    Set(value As String)
      c_Title = value.Clone
      If tcClip IsNot Nothing Then tcClip.Title = c_Title.Clone
      If tcImage IsNot Nothing Then tcImage.Title = c_Title.Clone
    End Set
  End Property

  Public Property Tooltip As String
    Get
      Return c_Tooltip.Clone
    End Get
    Set(value As String)
      c_Tooltip = value.Clone
      If tcClip IsNot Nothing Then tcClip.Tooltip = c_Tooltip.Clone
      If tcImage IsNot Nothing Then tcImage.Tooltip = c_Tooltip.Clone
    End Set
  End Property

  Public Property Icon As Drawing.Icon
    Get
      Return c_Icon.Clone
    End Get
    Set(value As Drawing.Icon)
      c_Icon = value.Clone
      If tcClip IsNot Nothing Then tcClip.Icon = c_Icon.Clone
      If tcImage IsNot Nothing Then tcImage.Icon = c_Icon.Clone
    End Set
  End Property

  Public Property BackEnabled As Boolean
    Get
      If tcClip IsNot Nothing Then Return tcClip.BackEnabled
      If tcImage IsNot Nothing Then Return tcImage.BackEnabled
      Return False
    End Get
    Set(value As Boolean)
      If tcClip IsNot Nothing Then tcClip.BackEnabled = value
      If tcImage IsNot Nothing Then tcImage.BackEnabled = value
    End Set
  End Property

  Public Property PlayPauseEnabled As Boolean
    Get
      If tcClip IsNot Nothing Then Return tcClip.PlayPauseEnabled
      If tcImage IsNot Nothing Then Return tcImage.PlayPauseEnabled
      Return False
    End Get
    Set(value As Boolean)
      If tcClip IsNot Nothing Then tcClip.PlayPauseEnabled = value
      If tcImage IsNot Nothing Then tcImage.PlayPauseEnabled = value
    End Set
  End Property

  Public Property StopEnabled As Boolean
    Get
      If tcClip IsNot Nothing Then Return tcClip.StopEnabled
      If tcImage IsNot Nothing Then Return tcImage.StopEnabled
      Return False
    End Get
    Set(value As Boolean)
      If tcClip IsNot Nothing Then tcClip.StopEnabled = value
      If tcImage IsNot Nothing Then tcImage.StopEnabled = value
    End Set
  End Property

  Public Property NextEnabled As Boolean
    Get
      If tcClip IsNot Nothing Then Return tcClip.NextEnabled
      If tcImage IsNot Nothing Then Return tcImage.NextEnabled
      Return False
    End Get
    Set(value As Boolean)
      If tcClip IsNot Nothing Then tcClip.NextEnabled = value
      If tcImage IsNot Nothing Then tcImage.NextEnabled = value
    End Set
  End Property

  Public Property IsPause As Boolean
    Get
      If tcClip IsNot Nothing Then Return tcClip.IsPause
      If tcImage IsNot Nothing Then Return tcImage.IsPause
      Return False
    End Get
    Set(value As Boolean)
      If tcClip IsNot Nothing Then tcClip.IsPause = value
      If tcImage IsNot Nothing Then tcImage.IsPause = value
    End Set
  End Property

  Public Sub ShowClip(Preview As Object)
    If tcImage IsNot Nothing Then
      tcImage.Dispose()
      tcImage = Nothing
    End If
    If tcClip IsNot Nothing Then
      UpdateClip(Preview)
      Return
    End If
    tcClip = New TaskbarController_Clip(c_MainHandle, Preview, c_Buttons, c_Icon, c_Title, c_Tooltip)
  End Sub

  Public Sub ShowImage(Preview As Drawing.Image)
    If tcImage IsNot Nothing Then
      tcImage.Dispose()
      tcImage = Nothing
    End If
    tcImage = New TaskbarController_Image(c_MainHandle, Preview, c_Buttons, c_Icon, c_Title, c_Tooltip)
  End Sub

  Public Sub UpdateClip(Preview As Object)
    If tcClip IsNot Nothing Then tcClip.UpdatePreview(Preview)
  End Sub

  Private Sub tcClip_AppEvent(sender As Object, e As TabbedThumbnailAppEventArgs) Handles tcClip.AppEvent
    RaiseEvent AppEvent(sender, e)
  End Sub

  Private Sub tcClip_Button_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles tcClip.Button_Click
    RaiseEvent Button_Click(sender, e)
  End Sub

  Private Sub tcImage_AppEvent(sender As Object, e As TabbedThumbnailAppEventArgs) Handles tcImage.AppEvent
    RaiseEvent AppEvent(sender, e)
  End Sub

  Private Sub tcImage_Button_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles tcImage.Button_Click
    RaiseEvent Button_Click(sender, e)
  End Sub
End Class