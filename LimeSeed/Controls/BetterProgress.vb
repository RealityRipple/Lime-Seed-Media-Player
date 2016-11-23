Imports System.Drawing
Public Class BetterProgress
  Private min As Double = 0               ' Minimum value for progress range
  Private max As Double = 100             ' Maximum value for progress range
  Private val As Double = 0               ' Current progress
  Private lastVal As Double = 0           ' Last Progress
  Public Enum BetterProgressStyle
    VisualStyleDrawn
    CustomColors
    CustomColorsNoBorder
    CustomColorsSlanted
  End Enum
  Private mBarStyle As BetterProgressStyle
  Private mBarBG As Color
  Private mBarFG As Color
  Private mBarBorder As Color
  Private mBarProg As ProgressBarStyle
  'Private TransparentBG As Boolean
  Private marqueeLoc As Long

  Public Event ValueChanged(sender As Object, e As EventArgs)

  Property BarBackground As Color
    Get
      Return mBarBG
    End Get
    Set(value As Color)
      If mBarBG <> value Then
        mBarBG = value
        Me.Invalidate()
      End If
    End Set
  End Property

  Property BarStyle As BetterProgressStyle
    Get
      Return mBarStyle
    End Get
    Set(value As BetterProgressStyle)
      If Not mBarStyle = value Then
        mBarStyle = value
        'TransparentBG = (mBarStyle = BetterProgressStyle.CustomColorsSlanted And MyBase.BackColor = Color.Transparent)
        Me.Invalidate()
      End If
    End Set
  End Property

  Property Style As ProgressBarStyle
    Get
      Return mBarProg
    End Get
    Set(value As ProgressBarStyle)
      If Not mBarProg = value Then
        mBarProg = value
        Me.Invalidate()
      End If
    End Set
  End Property

  Public Overridable Property BarForeground As Color
    Get
      Return mBarFG
    End Get
    Set(value As Color)
      If mBarFG <> value Then
        mBarFG = value
        Me.Invalidate()
      End If
    End Set
  End Property

  Public Overridable Property BarBorder As Color
    Get
      Return mBarBorder
    End Get
    Set(value As Color)
      If mBarBorder <> value Then
        mBarBorder = value
        Me.Invalidate()
      End If
    End Set
  End Property

  Public Overrides Property BackColor As System.Drawing.Color
    Get
      Return MyBase.BackColor
    End Get
    Set(value As System.Drawing.Color)
      MyBase.BackColor = value
      'TransparentBG = (mBarStyle = BetterProgressStyle.CustomColorsSlanted And MyBase.BackColor = Color.Transparent)
      Me.Invalidate()
    End Set
  End Property


  Public Sub New()
    ' This call is required by the designer.
    InitializeComponent()
    ' Add any initialization after the InitializeComponent() call.
    mBarStyle = BetterProgressStyle.VisualStyleDrawn
    'TransparentBG = False
    mBarBG = SystemColors.Control
    mBarFG = SystemColors.Highlight
    mBarBorder = SystemColors.ActiveBorder
    mBarProg = ProgressBarStyle.Continuous
  End Sub

  Protected Overrides Sub OnResize(e As EventArgs)
    ' Invalidate the control to get a repaint.
    Me.Invalidate()
  End Sub

  Protected Overrides Sub OnPaint(e As PaintEventArgs)
    If e.Graphics Is Nothing Then Return
    Me.SuspendLayout()
    Using bgBrush As New SolidBrush(mBarBG), fgBrush As New SolidBrush(mBarFG), backBrush As New SolidBrush(Me.BackColor), borderPen As New Pen(mBarBorder)
      Using g As Graphics = e.Graphics
        Select Case mBarStyle
          Case BetterProgressStyle.VisualStyleDrawn
            If val = 0 And min = 0 And max = 0 Then
              If ProgressBarRenderer.IsSupported Then
                ProgressBarRenderer.DrawHorizontalBar(g, Me.ClientRectangle)
              Else
                g.FillRectangle(SystemBrushes.ButtonFace, Me.ClientRectangle)
                g.DrawLines(SystemPens.ButtonHighlight, {New Point(0, Me.ClientRectangle.Height), New Point(Me.ClientRectangle.Width, Me.ClientRectangle.Height), New Point(Me.ClientRectangle.Width, 0)})
                g.DrawLines(SystemPens.ButtonShadow, {New Point(0, Me.ClientRectangle.Height), New Point(0, 0), New Point(Me.ClientRectangle.Width, 0)})
              End If
            Else
              If mBarProg = ProgressBarStyle.Marquee Then
                Dim rect As Rectangle = Me.ClientRectangle
                If ProgressBarRenderer.IsSupported Then
                  ProgressBarRenderer.DrawHorizontalBar(g, rect)
                Else
                  g.FillRectangle(SystemBrushes.ButtonFace, rect)
                  g.DrawLines(SystemPens.ButtonHighlight, {New Point(0, Me.ClientRectangle.Height), New Point(Me.ClientRectangle.Width, Me.ClientRectangle.Height), New Point(Me.ClientRectangle.Width, 0)})
                  g.DrawLines(SystemPens.ButtonShadow, {New Point(0, Me.ClientRectangle.Height), New Point(0, 0), New Point(Me.ClientRectangle.Width, 0)})
                End If
                marqueeLoc += 1
                If marqueeLoc >= rect.Right + rect.Height Then marqueeLoc = rect.Left - rect.Height
                rect.Y += 1
                rect.X = marqueeLoc + rect.Height
                rect.Width = rect.Height * 2
                rect.Height -= 2
                If ProgressBarRenderer.IsSupported Then

                  ProgressBarRenderer.DrawHorizontalChunks(g, rect)

                  g.FillRectangles(backBrush, {New Rectangle(0, 0, 1, 1), New Rectangle(Me.ClientRectangle.Width - 1, 0, 1, 1), New Rectangle(0, Me.ClientRectangle.Height - 1, 1, 1), New Rectangle(Me.ClientRectangle.Width - 1, Me.ClientRectangle.Height - 1, 1, 1)})
                Else
                  g.FillRectangle(SystemBrushes.MenuHighlight, rect)
                End If

              Else
                Dim percent As Decimal = (val - min) / (max - min)
                Dim rect As Rectangle = Me.ClientRectangle
                If ProgressBarRenderer.IsSupported Then
                  ProgressBarRenderer.DrawHorizontalBar(g, rect)
                Else
                  g.FillRectangle(SystemBrushes.ButtonFace, rect)
                  g.DrawLines(SystemPens.ButtonHighlight, {New Point(0, Me.ClientRectangle.Height), New Point(Me.ClientRectangle.Width, Me.ClientRectangle.Height), New Point(Me.ClientRectangle.Width, 0)})
                  g.DrawLines(SystemPens.ButtonShadow, {New Point(0, Me.ClientRectangle.Height), New Point(0, 0), New Point(Me.ClientRectangle.Width, 0)})
                End If
                rect.Y += 1
                rect.X += 1
                rect.Width -= 2
                rect.Width *= percent
                rect.Height -= 2
                If ProgressBarRenderer.IsSupported Then
                  ProgressBarRenderer.DrawHorizontalChunks(g, rect)
                  g.FillRectangles(backBrush, {New Rectangle(0, 0, 1, 1), New Rectangle(Me.ClientRectangle.Width - 1, 0, 1, 1), New Rectangle(0, Me.ClientRectangle.Height - 1, 1, 1), New Rectangle(Me.ClientRectangle.Width - 1, Me.ClientRectangle.Height - 1, 1, 1)})
                Else
                  g.FillRectangle(SystemBrushes.MenuHighlight, rect)
                End If
              End If
            End If
          Case BetterProgressStyle.CustomColorsSlanted
            If val = 0 And min = 0 And max = 0 Then
              g.FillPolygon(bgBrush, {New Point(0, Me.ClientRectangle.Height - 1), New Point(Me.ClientRectangle.Width - 1, 0), New Point(Me.ClientRectangle.Width - 1, Me.ClientRectangle.Height - 1)})
            Else
              Dim percent As Decimal = (val - min) / (max - min)
              Dim rect As Rectangle = Me.ClientRectangle
              g.FillPolygon(bgBrush, {New Point(0, rect.Height - 1), New Point(rect.Width - 1, 0), New Point(rect.Width - 1, rect.Height - 1)})
              rect.Y += 1
              rect.X += 1
              rect.Width -= 2
              rect.Width *= percent
              rect.Height -= 2
              Dim iTopRight As Integer = ((Me.ClientRectangle.Height) / (Me.ClientRectangle.Width)) * (rect.Width + 1)
              g.FillPolygon(fgBrush, {New Point(rect.X - 1, rect.Height + 1), New Point(rect.Width + 1, Me.ClientRectangle.Height - iTopRight), New Point(rect.Width + 1, rect.Height + 1)})
              g.DrawPolygon(borderPen, {New Point(0, Me.ClientRectangle.Height - 1), New Point(Me.ClientRectangle.Width - 1, 0), New Point(Me.ClientRectangle.Width - 1, Me.ClientRectangle.Height - 1)})
            End If
          Case BetterProgressStyle.CustomColors
            If val = 0 And min = 0 And max = 0 Then
              g.FillRectangle(bgBrush, Me.ClientRectangle)
            Else
              Dim percent As Decimal = (val - min) / (max - min)
              Dim rect As Rectangle = Me.ClientRectangle
              g.FillRectangle(bgBrush, rect)
              rect.Y += 1
              rect.X += 1
              rect.Width -= 2
              rect.Width *= percent
              rect.Height -= 2
              g.FillRectangle(fgBrush, rect)
              rect = Me.ClientRectangle
              rect.Width -= 1
              rect.Height -= 1
              g.DrawRectangle(borderPen, rect)
            End If
          Case BetterProgressStyle.CustomColorsNoBorder
            If val = 0 And min = 0 And max = 0 Then
              g.FillRectangle(bgBrush, Me.ClientRectangle)
            Else
              Dim percent As Decimal = (val - min) / (max - min)
              Dim rect As Rectangle = Me.ClientRectangle
              g.FillRectangle(bgBrush, rect)
              rect.Y += 1
              rect.X += 1
              rect.Width -= 2
              rect.Width *= percent
              rect.Height -= 2
              g.FillRectangle(fgBrush, rect)
              rect = Me.ClientRectangle
              rect.Width -= 1
              rect.Height -= 1
            End If
        End Select
      End Using
    End Using
    Me.ResumeLayout(False)
  End Sub

  Public Property Minimum() As Double
    Get
      Return min
    End Get

    Set(Value As Double)
      ' Prevent a negative value.
      If (Value < 0) Then
        min = 0
      End If

      ' Make sure that the minimum value is never set higher than the maximum value.
      If (Value > max) Then
        min = Value
        min = Value
      End If

      ' Make sure that the value is still in range.
      If (val < min) Then
        val = min
      End If



      ' Invalidate the control to get a repaint.
      Me.Invalidate()
    End Set
  End Property

  Public Property Maximum() As Double
    Get
      Return max
    End Get

    Set(Value As Double)
      ' Make sure that the maximum value is never set lower than the minimum value.
      If (Value < min) Then
        min = Value
      End If

      max = Value

      ' Make sure that the value is still in range.
      If (val > max) Then
        val = max
      End If

      ' Invalidate the control to get a repaint.
      Me.Invalidate()
    End Set
  End Property

  Public Property Value() As Double
    Get
      Return val
    End Get

    Set(Value As Double)
      Static lastPercent, lastOldPercent As Integer
      If val = Value Then Exit Property
      lastVal = val
      If (Value < min) Then
        val = min
      ElseIf (Value > max) Then
        val = max
      Else
        val = Value
      End If
      If val = 0 And min = 0 And max = 0 Then Exit Property
      Dim percent As Single = (val - min) / (max - min)
      Dim oldPercent As Single = (lastVal - min) / (max - min)
      If Math.Floor(Me.ClientRectangle.Width * percent) = lastPercent And Math.Floor(Me.ClientRectangle.Width * oldPercent) = lastOldPercent Then Exit Property
      lastPercent = Math.Floor(Me.ClientRectangle.Width * percent)
      lastOldPercent = Math.Floor(Me.ClientRectangle.Width * oldPercent)
      Dim updateRect As New Rectangle(0, 0, Me.ClientRectangle.Width, Me.ClientRectangle.Height)
      If lastVal > val Then
        updateRect.X = Math.Floor(Me.ClientRectangle.Width * percent) - 1
        updateRect.Width = Math.Ceiling(Me.ClientRectangle.Width * oldPercent) - updateRect.X + 2 ' Me.ClientRectangle.Width - updateRect.X
      ElseIf val > lastVal Then
        updateRect.X = Math.Floor(Me.ClientRectangle.Width * oldPercent) - 1
        updateRect.Width = Math.Ceiling(Me.ClientRectangle.Width * percent) - updateRect.X + 2
      Else
        Exit Property
      End If
      If updateRect.X < 0 Then updateRect.X = 0
      If updateRect.Width > Me.ClientRectangle.Width Then updateRect.Width = Me.ClientRectangle.Width
      Me.Invalidate(updateRect)
      RaiseEvent ValueChanged(Me, New EventArgs)
    End Set
  End Property

  'Protected Overrides ReadOnly Property CreateParams() As System.Windows.Forms.CreateParams
  '  Get
  '    If TransparentBG Then
  '      Dim cp As CreateParams = MyBase.CreateParams
  '      cp.ExStyle = &H20
  '      Return cp
  '    Else
  '      Return MyBase.CreateParams
  '    End If
  '  End Get
  'End Property

  'Protected Overrides Sub OnPaintBackground(ByVal e As System.Windows.Forms.PaintEventArgs)
  '  If TransparentBG Then
  '    'do nothing here
  '  Else
  '    MyBase.OnPaintBackground(e)
  '  End If
  'End Sub

  'Public Overrides Sub Refresh()
  '  If TransparentBG Then Parent.Invalidate(New Rectangle(Me.Location, Me.Size), True)
  'End Sub

End Class
