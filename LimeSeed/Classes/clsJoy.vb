Public Class clsJoy

  Private Structure JOYINFOEX
    Public dwSize As Integer                      ' size of structure
    Public dwFlags As Integer                     ' flags to indicate what to return
    Public dwXpos As Integer                      ' x position
    Public dwYpos As Integer                      ' y position
    Public dwZpos As Integer                      ' z position
    Public dwRpos As Integer                      ' rudder/4th axis position
    Public dwUpos As Integer                      ' 5th axis position
    Public dwVpos As Integer                      ' 6th axis position
    Public dwButtons As Integer                   ' button states
    Public dwButtonNumber As Integer              ' current button number pressed
    Public dwPOV As Integer                       ' point of view state
    Public dwReserved1 As Integer                 ' reserved for communication between winmm driver
    Public dwReserved2 As Integer                 ' reserved for future expansion
  End Structure

  Private Declare Function joyGetPosEx Lib "winmm.dll" (uJoyID As Integer, ByRef pji As JOYINFOEX) As Long

  Private Const JOYSTICKID1 = 0
  Private Const JOYSTICKID2 = 1
  Private Const JOY_RETURNBUTTONS = &H80&
  Private Const JOY_RETURNCENTERED = &H400&
  Private Const JOY_RETURNPOV = &H40&
  Private Const JOY_RETURNR = &H8&
  Private Const JOY_RETURNU = &H10
  Private Const JOY_RETURNV = &H20
  Private Const JOY_RETURNX = &H1&
  Private Const JOY_RETURNY = &H2&
  Private Const JOY_RETURNZ = &H4&
  Private Const JOY_RETURNALL = (JOY_RETURNX Or JOY_RETURNY Or JOY_RETURNZ Or JOY_RETURNR Or JOY_RETURNU Or JOY_RETURNV Or JOY_RETURNPOV Or JOY_RETURNBUTTONS)
  Private ji As JOYINFOEX
  Private jiC As JOYINFOEX
  Private Const mvarJoyID As Integer = 0

  Private mvarJoyX As Object
  Private mvarJoyY As Object
  Private mvarJoyZ As Object
  Private mvarJoyR As Object
  Private mvarJoyU As Object
  Private mvarJoyV As Object
  Private mvarJoyPOV As Object
  Private mvarJoyButState As Object
  Private mvarJoyCurBut As Object

  Public Sub New()
    'nothing to init... how nice
  End Sub
  
  Public Property JoyCurBut As Object
    Get
      ji = jiC
      ji.dwFlags = JOY_RETURNALL
      ji.dwSize = Len(ji)
      joyGetPosEx(mvarJoyID, ji) : mvarJoyCurBut = ji.dwButtonNumber
      JoyCurBut = mvarJoyCurBut
    End Get
    Set(value As Object)
      mvarJoyCurBut = value
    End Set
  End Property

  Public ReadOnly Property JoyNull As Boolean
    Get
      ji = jiC
      ji.dwFlags = JOY_RETURNALL
      ji.dwSize = Len(ji)
      joyGetPosEx(mvarJoyID, ji)
      Return (ji.dwButtons = 0 And ji.dwFlags = 0 And ji.dwPOV = 0 And ji.dwRpos = 0 And ji.dwUpos = 0 And ji.dwVpos = 0 And ji.dwXpos = 0 And ji.dwYpos = 0 And ji.dwZpos = 0 And ji.dwButtonNumber = 0)
    End Get
  End Property

  Public Property JoyButState As Object
    Get
      ji = jiC
      ji.dwFlags = JOY_RETURNALL
      ji.dwSize = Len(ji)
      joyGetPosEx(mvarJoyID, ji) : mvarJoyButState = ji.dwButtons
      JoyButState = mvarJoyButState
    End Get
    Set(value As Object)
      mvarJoyButState = value
    End Set
  End Property

  Public Property JoyPOV As Object
    Get
      ji = jiC
      ji.dwFlags = JOY_RETURNALL
      ji.dwSize = Len(ji)
      joyGetPosEx(mvarJoyID, ji) : mvarJoyPOV = ji.dwPOV
      JoyPOV = mvarJoyPOV
    End Get
    Set(value As Object)
      mvarJoyPOV = value
    End Set
  End Property

  Public Property JoyV As Object
    Get
      ji = jiC
      ji.dwFlags = JOY_RETURNALL
      ji.dwSize = Len(ji)
      joyGetPosEx(mvarJoyID, ji) : mvarJoyV = ji.dwVpos
      JoyV = mvarJoyV
    End Get
    Set(value As Object)
      mvarJoyV = value
    End Set
  End Property

  Public Property JoyU As Object
    Get
      ji = jiC
      ji.dwFlags = JOY_RETURNALL
      ji.dwSize = Len(ji)
      joyGetPosEx(mvarJoyID, ji) : mvarJoyU = ji.dwUpos
      JoyU = mvarJoyU
    End Get
    Set(value As Object)
      mvarJoyU = value
    End Set
  End Property

  Public Property JoyR() As Object
    Get
      ji = jiC
      ji.dwFlags = JOY_RETURNALL
      ji.dwSize = Len(ji)
      joyGetPosEx(mvarJoyID, ji) : mvarJoyR = ji.dwRpos
      JoyR = mvarJoyR
    End Get
    Set(value As Object)
      mvarJoyR = value
    End Set
  End Property

  Public Property JoyZ As Object
    Get
      ji = jiC
      ji.dwFlags = JOY_RETURNALL
      ji.dwSize = Len(ji)
      joyGetPosEx(mvarJoyID, ji) : mvarJoyZ = ji.dwZpos
        JoyZ = mvarJoyZ
    End Get
    Set(value As Object)
      mvarJoyZ = value
    End Set
  End Property

  Public Property JoyY As Object
    Get
      ji = jiC
      ji.dwFlags = JOY_RETURNALL
      ji.dwSize = Len(ji)
      joyGetPosEx(mvarJoyID, ji) : mvarJoyY = ji.dwYpos
      JoyY = mvarJoyY
    End Get
    Set(value As Object)
      mvarJoyY = value
    End Set
  End Property

  Public Property JoyX As Object
    Get
      ji = jiC
      ji.dwFlags = JOY_RETURNALL
      ji.dwSize = Len(ji)
      joyGetPosEx(mvarJoyID, ji) : mvarJoyX = ji.dwXpos
      JoyX = mvarJoyX
    End Get
    Set(value As Object)
      mvarJoyX = value
    End Set
  End Property
End Class


Public Class clsJoyDetection
  Private WithEvents tmrCheck As Timer
  Private cJoy As clsJoy


  Private lastJoyX, lastJoyY, lastJoyZ, lastJoyR As Byte
  Private lastJoyPOV, lastJoyU, lastJoyV As Object
  Private lastJoyButton(15) As Boolean

  Event XAxisLeft()
  Event XAxisRight()
  Event XAxisCenter()

  Event YAxisTop()
  Event YAxisBottom()
  Event YAxisMiddle()

  Event ZAxisTop()
  Event ZAxisBottom()
  Event ZAxisMiddle()

  Event RAxisLeft()
  Event RAxisRight()
  Event RAxisCenter()

  Event POVSet(Degree As Integer)

  Event ButtonDown(Button As Integer)
  Event ButtonUp(Button As Integer)

  Event UChange(Value As Object)
  Event VChange(Value As Object)

  Private cTag As Object
  Public Property Tag As Object
    Get
      Return cTag
    End Get
    Set(value As Object)
      cTag = value
    End Set
  End Property

  Public Sub New()
    cJoy = New clsJoy
    tmrCheck = New Timer
    tmrCheck.Interval = 100
    Select Case cJoy.JoyX
      Case Is < 16384 : lastJoyX = 0
      Case Is < 49152 : lastJoyX = 1
      Case Else : lastJoyX = 2
    End Select
    Select Case cJoy.JoyY
      Case Is < 16384 : lastJoyY = 0
      Case Is < 49152 : lastJoyY = 1
      Case Else : lastJoyY = 2
    End Select
    Select Case cJoy.JoyZ
      Case Is < 16384 : lastJoyZ = 0
      Case Is < 49152 : lastJoyZ = 1
      Case Else : lastJoyZ = 2
    End Select
    Select Case cJoy.JoyR
      Case Is < 16384 : lastJoyR = 0
      Case Is < 49152 : lastJoyR = 1
      Case Else : lastJoyR = 2
    End Select

    lastJoyPOV = cJoy.JoyPOV
    For I As Integer = 0 To 15
      Dim Val As Long = 2 ^ I
      lastJoyButton(I) = CBool((Val And cJoy.JoyButState) = Val)
    Next
    lastJoyU = cJoy.JoyU
    lastJoyV = cJoy.JoyV
    tmrCheck.Start()
  End Sub

  Private Sub tmrCheck_Tick(sender As Object, e As System.EventArgs) Handles tmrCheck.Tick
    If cJoy.JoyNull Then Exit Sub

    Select Case cJoy.JoyX
      Case Is < 16384
        If Not lastJoyX = 0 Then
          RaiseEvent XAxisLeft()
          lastJoyX = 0
        End If
      Case Is < 49152
        If Not lastJoyX = 1 Then
          RaiseEvent XAxisCenter()
          lastJoyX = 1
        End If
      Case Else
        If Not lastJoyX = 2 Then
          RaiseEvent XAxisRight()
          lastJoyX = 2
        End If
    End Select

    Select Case cJoy.JoyY
      Case Is < 16384
        If Not lastJoyY = 0 Then
          RaiseEvent YAxisTop()
          lastJoyY = 0
        End If
      Case Is < 49152
        If Not lastJoyY = 1 Then
          RaiseEvent YAxisMiddle()
          lastJoyY = 1
        End If
      Case Else
        If Not lastJoyY = 2 Then
          RaiseEvent YAxisBottom()
          lastJoyY = 2
        End If
    End Select

    Select Case cJoy.JoyZ
      Case Is < 16384
        If Not lastJoyZ = 0 Then
          RaiseEvent ZAxisTop()
          lastJoyZ = 0
        End If
      Case Is < 49152
        If Not lastJoyZ = 1 Then
          RaiseEvent ZAxisMiddle()
          lastJoyZ = 1
        End If
      Case Else
        If Not lastJoyZ = 2 Then
          RaiseEvent ZAxisBottom()
          lastJoyZ = 2
        End If
    End Select

    Select Case cJoy.JoyR
      Case Is < 16384
        If Not lastJoyR = 0 Then
          RaiseEvent RAxisLeft()
          lastJoyR = 0
        End If
      Case Is < 49152
        If Not lastJoyR = 1 Then
          RaiseEvent RAxisCenter()
          lastJoyR = 1
        End If
      Case Else
        If Not lastJoyR = 2 Then
          RaiseEvent RAxisRight()
          lastJoyR = 2
        End If
    End Select
    If Not cJoy.JoyPOV = lastJoyPOV Then RaiseEvent POVSet(cJoy.JoyPOV / 100)
    For I As Integer = 0 To 15
      Dim Val As Long = 2 ^ I
      If (Val And cJoy.JoyButState) = Val Then
        If Not lastJoyButton(I) Then
          lastJoyButton(I) = True
          RaiseEvent ButtonDown(I + 1)
        End If
      Else
        If lastJoyButton(I) Then
          lastJoyButton(I) = False
          RaiseEvent ButtonUp(I + 1)
        End If
      End If
    Next
    If Not cJoy.JoyU = lastJoyU Then RaiseEvent UChange(cJoy.JoyU)
    If Not cJoy.JoyV = lastJoyV Then RaiseEvent VChange(cJoy.JoyV)
  End Sub
End Class