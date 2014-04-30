Imports System.Drawing
Public Class frmText

  Private Sub RedrawWindow()
    Me.BackColor = SystemColors.Info
    Me.BackgroundImageLayout = ImageLayout.Center
    Dim imgBG As New Bitmap(Me.Width, Me.Height)
    Using g As Graphics = Graphics.FromImage(imgBG)
      g.Clear(Color.Fuchsia)
      Dim newRect As New Rectangle(Me.DisplayRectangle.Location, New Size(Me.DisplayRectangle.Width - 1, Me.DisplayRectangle.Height - 1))
      Using bgBrush As New Drawing2D.LinearGradientBrush(newRect, Color.White, Color.FromArgb(228, 229, 240), 90)
        g.FillRectangle(bgBrush, newRect)
      End Using
      g.DrawRectangle(New Pen(Color.FromArgb(118, 118, 118)), newRect)
      g.FillRectangles(Brushes.Fuchsia, {New Rectangle(0, 0, 1, 1), New Rectangle(0, newRect.Height, 1, 1), New Rectangle(newRect.Width, newRect.Height, 1, 1), New Rectangle(newRect.Width, 0, 1, 1)})
      Using borderBrush As New SolidBrush(Color.FromArgb(146, 146, 146))
        g.FillRectangles(borderBrush, {New Rectangle(1, 1, 1, 1), New Rectangle(1, newRect.Height - 1, 1, 1), New Rectangle(newRect.Width - 1, newRect.Height - 1, 1, 1), New Rectangle(newRect.Width - 1, 1, 1, 1)})
      End Using
    End Using
    Me.BackgroundImage = imgBG
    lblText.ForeColor = Color.FromArgb(118, 118, 118)
    lblText.BackColor = Color.Transparent
    lblText.Location = New Point(2, 1)
  End Sub

  Public Sub SetText(Message As String)
    tmrHide.Stop()
    If Not lblText.Text = Message Then
      lblText.Text = Message
      Do While lblText.Height > 350
        lblText.Text = lblText.Text.Substring(0, lblText.Text.LastIndexOf(vbNewLine)) & "..."
      Loop
    End If
    Dim TextPadSize As New Size(lblText.Size.Width + 4, lblText.Size.Height + 3)
    If (Not Me.Size = TextPadSize) Or (Not Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None) Then
      Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
      Me.Size = TextPadSize
      RedrawWindow()
    End If
    Dim meBounds As New Rectangle(Cursor.Position.X + 24, Cursor.Position.Y + 16, Me.Width, Me.Height)
    Dim MyScreen As Screen = Screen.FromPoint(Cursor.Position)
    If Not MyScreen.Bounds.Contains(meBounds) Then
      If meBounds.Top < MyScreen.Bounds.Top Then meBounds.Y = MyScreen.Bounds.Top
      If meBounds.Left < MyScreen.Bounds.Left Then meBounds.X = MyScreen.Bounds.Left
      If meBounds.Bottom > MyScreen.Bounds.Bottom Then meBounds.Y = MyScreen.Bounds.Bottom - Me.Height
      If meBounds.Right > MyScreen.Bounds.Right Then meBounds.X = Cursor.Position.X - Me.Width - 8
    End If
    Me.Location = meBounds.Location
    If Not Me.Visible Then Me.Show()
    tmrHide.Start()
  End Sub

  Private Sub tmrHide_Tick(sender As System.Object, e As System.EventArgs) Handles tmrHide.Tick
    tmrHide.Stop()
    Me.Hide()
  End Sub
End Class