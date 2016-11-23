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


Public Class TaskbarController
  Private WithEvents ttbClipBack, ttbClipPlayPause, ttbClipStop, ttbClipNext As ThumbnailToolBarButton
  Private WithEvents ttbBmpBack, ttbBmpPlayPause, ttbBmpStop, ttbBmpNext As ThumbnailToolBarButton
  Private tmpObj As Control
  Private WithEvents taskThumb As TabbedThumbnail
  Private c_MainHandle As IntPtr
  Private c_Title, c_Tooltip As String
  Private c_Icon As Drawing.Icon
  Public Event AppEvent(sender As Object, e As TabbedThumbnailEventArgs)
  Public Event Button_Click(sender As Object, e As ThumbnailButtonClickedEventArgs)
  Private WithEvents tmrEvent As Timer
  Private icoBack, icoPlay, icoPause, icoStop, icoNext As Drawing.Icon
  Private sBack, sPlay, sPause, sStop, sNext As String
  Private bIsPause As Boolean

  Public Property Title As String
    Get
      Return c_Title
    End Get
    Set(value As String)
      c_Title = value.Clone
      If taskThumb IsNot Nothing Then taskThumb.Title = c_Title
    End Set
  End Property

  Public Property Tooltip As String
    Get
      Return c_Tooltip
    End Get
    Set(value As String)
      c_Tooltip = value.Clone
      If taskThumb IsNot Nothing Then taskThumb.Tooltip = c_Tooltip
    End Set
  End Property


  Public Property Icon As Drawing.Icon
    Get
      Return c_Icon.Clone
    End Get
    Set(value As Drawing.Icon)
      c_Icon = value.Clone
      If taskThumb IsNot Nothing Then taskThumb.SetWindowIcon(c_Icon.Clone)
    End Set
  End Property

  Public Property BackIcon As Drawing.Icon
    Get
      Return icoBack
    End Get
    Set(value As Drawing.Icon)
      icoBack = value
      ttbClipBack.Icon = icoBack
      If ttbBmpBack IsNot Nothing Then ttbBmpBack.Icon = icoBack
    End Set
  End Property
  Public Property BackText As String
    Get
      Return sBack
    End Get
    Set(value As String)
      sBack = value
      ttbClipBack.Tooltip = sBack
      If ttbBmpBack IsNot Nothing Then ttbBmpBack.Tooltip = sBack
    End Set
  End Property
  Public Property BackEnabled As Boolean
    Get
      Return ttbClipBack.Enabled
    End Get
    Set(value As Boolean)
      ttbClipBack.Enabled = value
      If ttbBmpBack IsNot Nothing Then ttbBmpBack.Enabled = value
    End Set
  End Property

  Public Property PlayIcon As Drawing.Icon
    Get
      Return icoPlay
    End Get
    Set(value As Drawing.Icon)
      icoPlay = value
      If Not bIsPause Then
        ttbClipPlayPause.Icon = icoPlay
        If ttbBmpPlayPause IsNot Nothing Then ttbBmpPlayPause.Icon = icoPlay
      End If
    End Set
  End Property
  Public Property PlayText As String
    Get
      Return sPlay
    End Get
    Set(value As String)
      sPlay = value
      If Not bIsPause Then
        ttbClipPlayPause.Tooltip = sPlay
        If ttbBmpPlayPause IsNot Nothing Then ttbBmpPlayPause.Tooltip = sPlay
      End If
    End Set
  End Property
  Public Property PauseIcon As Drawing.Icon
    Get
      Return icoPause
    End Get
    Set(value As Drawing.Icon)
      icoPause = value
      If bIsPause Then
        ttbClipPlayPause.Icon = icoPause
        If ttbBmpPlayPause IsNot Nothing Then ttbBmpPlayPause.Icon = icoPause
      End If
    End Set
  End Property
  Public Property PauseText As String
    Get
      Return sPause
    End Get
    Set(value As String)
      sPause = value
      If bIsPause Then
        ttbClipPlayPause.Tooltip = sPause
        If ttbBmpPlayPause IsNot Nothing Then ttbBmpPlayPause.Tooltip = sPause
      End If
    End Set
  End Property
  Public Property PlayPauseEnabled As Boolean
    Get
      Return ttbClipPlayPause.Enabled
    End Get
    Set(value As Boolean)
      ttbClipPlayPause.Enabled = value
      If ttbBmpPlayPause IsNot Nothing Then ttbBmpPlayPause.Enabled = value
    End Set
  End Property

  Public Property StopIcon As Drawing.Icon
    Get
      Return icoStop
    End Get
    Set(value As Drawing.Icon)
      icoStop = value
      ttbClipStop.Icon = icoStop
      If ttbBmpStop IsNot Nothing Then ttbBmpStop.Icon = icoStop
    End Set
  End Property
  Public Property StopText As String
    Get
      Return sStop
    End Get
    Set(value As String)
      sStop = value
      ttbClipStop.Tooltip = sStop
      If ttbBmpStop IsNot Nothing Then ttbBmpStop.Tooltip = sStop
    End Set
  End Property
  Public Property StopEnabled As Boolean
    Get
      Return ttbClipStop.Enabled
    End Get
    Set(value As Boolean)
      ttbClipStop.Enabled = value
      If ttbBmpStop IsNot Nothing Then ttbBmpStop.Enabled = value
    End Set
  End Property

  Public Property NextIcon As Drawing.Icon
    Get
      Return icoNext
    End Get
    Set(value As Drawing.Icon)
      icoNext = value
      ttbClipNext.Icon = icoNext
      If ttbBmpNext IsNot Nothing Then ttbBmpNext.Icon = icoNext
    End Set
  End Property
  Public Property NextText As String
    Get
      Return sNext
    End Get
    Set(value As String)
      sNext = value
      ttbClipNext.Tooltip = sNext
      If ttbBmpNext IsNot Nothing Then ttbBmpNext.Tooltip = sNext
    End Set
  End Property
  Public Property NextEnabled As Boolean
    Get
      Return ttbClipNext.Enabled
    End Get
    Set(value As Boolean)
      ttbClipNext.Enabled = value
      If ttbBmpNext IsNot Nothing Then ttbBmpNext.Enabled = value
    End Set
  End Property

  Public Property IsPause As Boolean
    Get
      Return bIsPause
    End Get
    Set(value As Boolean)
      bIsPause = value
      If bIsPause Then
        ttbClipPlayPause.Icon = icoPause
        ttbClipPlayPause.Tooltip = sPause
        If ttbBmpPlayPause IsNot Nothing Then
          ttbBmpPlayPause.Icon = icoPause
          ttbBmpPlayPause.Tooltip = sPause
          ttbBmpPlayPause.Enabled = ttbClipPlayPause.Enabled
        End If
      Else
        ttbClipPlayPause.Icon = icoPlay
        ttbClipPlayPause.Tooltip = sPlay
        If ttbBmpPlayPause IsNot Nothing Then
          ttbBmpPlayPause.Icon = icoPlay
          ttbBmpPlayPause.Tooltip = sPlay
          ttbBmpPlayPause.Enabled = ttbClipPlayPause.Enabled
        End If
      End If
    End Set
  End Property

  Public Sub RemovePreview()
    If tmpObj IsNot Nothing Then
      If tmpObj.InvokeRequired Then
        tmpObj.BeginInvoke(New MethodInvoker(AddressOf RemovePreview))
      Else
        If TaskbarManager.Instance.TabbedThumbnail.IsThumbnailPreviewAdded(tmpObj) Then TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(tmpObj)
        TabbedThumbnailManager.ClearThumbnailClip(c_MainHandle)
        ttbBmpBack = Nothing
        ttbBmpPlayPause = Nothing
        ttbBmpStop = Nothing
        ttbBmpNext = Nothing
        tmpObj = Nothing
        If taskThumb IsNot Nothing Then
          taskThumb = Nothing
        End If
      End If
    End If
  End Sub

  Public Sub CreatePreview(Obj As Drawing.Image)
    If Obj Is Nothing Then Return
    If Not clsGlass.IsCompositionEnabled Then Return
    If tmpObj Is Nothing Then
      RemovePreview()
      CreateTmpObj()
    End If
    If taskThumb Is Nothing Then
      taskThumb = New TabbedThumbnail(c_MainHandle, tmpObj)
      TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(taskThumb)
    End If
    Dim preview As TabbedThumbnail = TaskbarManager.Instance.TabbedThumbnail.GetThumbnailPreview(tmpObj)
    Using bmpPreview As New Drawing.Bitmap(Obj)
      If preview IsNot Nothing Then preview.SetImage(bmpPreview)
    End Using
    taskThumb.Title = c_Title
    If c_Icon IsNot Nothing Then taskThumb.SetWindowIcon(c_Icon.Clone)
  End Sub

  Public Sub CreatePreview(obj As Control)
    If Not clsGlass.IsCompositionEnabled Then Return
    If TaskbarManager.IsPlatformSupported AndAlso obj IsNot Nothing Then
      If tmpObj IsNot Nothing Then RemovePreview()
      Dim loc As Drawing.Point = GetExactLoc(obj)
      If taskThumb Is Nothing Then
        taskThumb = New TabbedThumbnail(c_MainHandle, c_MainHandle)
        'Try
        '  TaskbarManager.Instance.ThumbnailToolbars.AddButtons(c_MainHandle, ttbClipBack, ttbClipPlayPause, ttbClipStop, ttbClipNext)
        'Catch ex As Exception

        'End Try
      End If
      Try
        TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(c_MainHandle, New Drawing.Rectangle(loc, obj.Size))
      Catch ex As Exception

      End Try
      Try
        taskThumb.Title = c_Title
      Catch ex As Exception
        Try
          If taskThumb Is Nothing Then
            taskThumb = New TabbedThumbnail(c_MainHandle, c_MainHandle)
          End If
          taskThumb.Title = c_Title
        Catch ex2 As Exception
          Return
        End Try
      End Try
      If c_Icon IsNot Nothing Then taskThumb.SetWindowIcon(c_Icon.Clone)
    End If
  End Sub

  Public Sub UpdatePreview(obj As Control)
    If Not clsGlass.IsCompositionEnabled Then Return
    If TaskbarManager.IsPlatformSupported AndAlso obj IsNot Nothing Then
      Dim loc As Drawing.Point = GetExactLoc(obj)
      TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(c_MainHandle, New Drawing.Rectangle(loc, obj.Size))
    End If
  End Sub

  Private Function GetExactLoc(obj As Control) As Drawing.Point
    Dim loc As Drawing.Point
    Dim parobj = obj
    Do
      loc.X += parobj.Location.X
      loc.Y += parobj.Location.Y
      parobj = parobj.Parent
    Loop Until parobj Is Nothing OrElse parobj.Parent Is Nothing
    Return loc
  End Function

  Private Sub CreateTmpObj()
    tmpObj = New Control
    ttbBmpBack = New ThumbnailToolBarButton(icoBack, sBack) With {.Enabled = True}
    ttbBmpPlayPause = New ThumbnailToolBarButton(icoPause, sPause) With {.Enabled = True}
    ttbBmpStop = New ThumbnailToolBarButton(icoStop, sStop) With {.Enabled = True}
    ttbBmpNext = New ThumbnailToolBarButton(icoNext, sNext) With {.Enabled = True}
    TaskbarManager.Instance.ThumbnailToolBars.AddButtons(tmpObj.Handle, ttbBmpBack, ttbBmpPlayPause, ttbBmpStop, ttbBmpNext)
  End Sub

  Public Sub New(hWnd As IntPtr, BackIcon As Drawing.Icon, BackText As String, PlayIcon As Drawing.Icon, PlayText As String, PauseIcon As Drawing.Icon, PauseText As String, StopIcon As Drawing.Icon, StopText As String, NextIcon As Drawing.Icon, NextText As String)
    c_MainHandle = hWnd
    icoBack = BackIcon
    sBack = BackText
    icoPlay = PlayIcon
    sPlay = PlayText
    icoPause = PauseIcon
    sPause = PauseText
    icoStop = StopIcon
    sStop = StopText
    icoNext = NextIcon
    sNext = NextText
    ttbClipBack = New ThumbnailToolBarButton(BackIcon, BackText)
    ttbClipPlayPause = New ThumbnailToolBarButton(PlayIcon, PlayText)
    ttbClipStop = New ThumbnailToolBarButton(StopIcon, StopText)
    ttbClipNext = New ThumbnailToolBarButton(NextIcon, NextText)
    TaskbarManager.Instance.ThumbnailToolBars.AddButtons(c_MainHandle, ttbClipBack, ttbClipPlayPause, ttbClipStop, ttbClipNext)
  End Sub

  Private Sub ttbBack_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbClipBack.Click, ttbBmpBack.Click
    RunEvent("Back", e)
  End Sub

  Private Sub ttbNext_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbClipNext.Click, ttbBmpNext.Click
    RunEvent("Next", e)
  End Sub

  Private Sub ttbPlayPause_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbClipPlayPause.Click, ttbBmpPlayPause.Click
    RunEvent("PlayPause", e)
  End Sub

  Private Sub ttbStop_Click(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles ttbClipStop.Click, ttbBmpStop.Click
    RunEvent("Stop", e)
  End Sub

  Private Sub RunEvent(Name As String, e As EventArgs)
    tmrEvent = Nothing
    tmrEvent = New Timer
    tmrEvent.Tag = {Name, e}
    tmrEvent.Interval = 1
    tmrEvent.Start()
  End Sub

  Private Sub tmrEvent_Tick(sender As Object, e As System.EventArgs) Handles tmrEvent.Tick
    tmrEvent.Stop()
    Dim sSender As String = tmrEvent.Tag(0)
    Debug.Print(sSender)
    If sSender.StartsWith("APP_") Then
      Dim aE As TabbedThumbnailEventArgs = tmrEvent.Tag(1)
      RaiseEvent AppEvent(sSender, aE)
    Else
      Dim aE As ThumbnailButtonClickedEventArgs = tmrEvent.Tag(1)
      RaiseEvent Button_Click(sSender, aE)
    End If

    tmrEvent = Nothing
  End Sub

  Private Sub taskThumb_TabbedThumbnailActivated(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailActivated
    RunEvent("APP_Activated", e)
    'Stop
  End Sub

  Private Sub taskThumb_TabbedThumbnailBitmapRequested(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailBitmapRequestedEventArgs) Handles taskThumb.TabbedThumbnailBitmapRequested
    'Stop
  End Sub

  Private Sub taskThumb_TabbedThumbnailClosed(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailClosed
    RunEvent("APP_Close", e)
    'Stop
  End Sub

  Private Sub taskThumb_TabbedThumbnailMaximized(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailMaximized
    RunEvent("APP_Maximize", e)
    'Stop
  End Sub

  Private Sub taskThumb_TabbedThumbnailMinimized(sender As Object, e As Microsoft.WindowsAPICodePack.Taskbar.TabbedThumbnailEventArgs) Handles taskThumb.TabbedThumbnailMinimized
    RunEvent("APP_Minimize", e)
    'Stop
  End Sub

  Private Sub taskThumb_TitleChanged(sender As Object, e As System.EventArgs) Handles taskThumb.TitleChanged
    'Debug.Print("Title Change")
    'Stop
  End Sub

  Private Sub taskThumb_TooltipChanged(sender As Object, e As System.EventArgs) Handles taskThumb.TooltipChanged
    'Debug.Print("Tooltip Change")
    'Stop
  End Sub

End Class