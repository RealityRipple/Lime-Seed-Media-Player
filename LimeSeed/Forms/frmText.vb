Imports System.Drawing
Public Class frmText
  Private mText As String
  Private mIcon As DragDropEffects

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
      Select Case mIcon
        Case DragDropEffects.All
          g.DrawImageUnscaled(My.Resources.tt_copy, 2, 2)
          DrawText(g, 18, 1)
        Case DragDropEffects.Move
          g.DrawImageUnscaled(My.Resources.tt_move, 2, 2)
          DrawText(g, 18, 1)
        Case DragDropEffects.Copy
          g.DrawImageUnscaled(My.Resources.tt_copy, 2, 2)
          DrawText(g, 18, 1)
        Case DragDropEffects.Link
          g.DrawImageUnscaled(My.Resources.tt_link, 2, 2)
          DrawText(g, 18, 1)
        Case DragDropEffects.Scroll
          DrawText(g, 2, 1)
        Case DragDropEffects.None
          DrawText(g, 2, 1)
      End Select
      g.DrawRectangle(New Pen(Color.FromArgb(118, 118, 118)), newRect)
      g.FillRectangles(Brushes.Fuchsia, {New Rectangle(0, 0, 1, 1), New Rectangle(0, newRect.Height, 1, 1), New Rectangle(newRect.Width, newRect.Height, 1, 1), New Rectangle(newRect.Width, 0, 1, 1)})
      Using borderBrush As New SolidBrush(Color.FromArgb(146, 146, 146))
        g.FillRectangles(borderBrush, {New Rectangle(1, 1, 1, 1), New Rectangle(1, newRect.Height - 1, 1, 1), New Rectangle(newRect.Width - 1, newRect.Height - 1, 1, 1), New Rectangle(newRect.Width - 1, 1, 1, 1)})
      End Using
    End Using
    Me.BackgroundImage = imgBG
  End Sub
  Private Sub DrawText(ByRef g As Graphics, X As Integer, Y As Integer)
    'Dim defStringFormat As New StringFormat(StringFormatFlags.FitBlackBox)

    'If lblText.Text.Contains("""") Then
    '  lblText.Visible = False
    '  Dim sTextA As String = lblText.Text
    '  Dim sTextB As String = Nothing

    '  sTextB = sTextA.Substring(sTextA.IndexOf("""") + 1)
    '  sTextB = sTextB.Substring(0, sTextB.IndexOf(""""))

    '  Dim xB As Integer = g.MeasureString(sTextA.Substring(0, sTextA.IndexOf("""")), lblText.Font, lblText.Size, New StringFormat(StringFormatFlags.MeasureTrailingSpaces)).Width - 5

    '  sTextA = Replace(sTextA, """", "")

    '  g.DrawString(sTextA, lblText.Font, New SolidBrush(Color.FromArgb(53, 51, 218)), New RectangleF(X, Y, lblText.Width, lblText.Height), defStringFormat)
    '  g.DrawString(sTextB, lblText.Font, New SolidBrush(Color.FromArgb(0, 0, 102)), New RectangleF(X + xB, Y, lblText.Width, lblText.Height), defStringFormat)
    'Else
    lblText.ForeColor = Color.FromArgb(53, 51, 218)
    lblText.BackColor = Color.Transparent
    lblText.Visible = True
    lblText.Location = New Point(X, Y)
    'g.DrawString(lblText.Text, lblText.Font, New SolidBrush(Color.FromArgb(53, 51, 218)), New Point(X, Y), defStringFormat)
    'End If
  End Sub

  Public Sub SetText(Message As String, Icon As DragDropEffects)
    tmrHide.Stop()
    If Not lblText.Text = Message Then
      lblText.Text = Message
      Do While lblText.Height > 350
        lblText.Text = lblText.Text.Substring(0, lblText.Text.LastIndexOf(vbNewLine)) & "..."
      Loop
    End If
    mText = lblText.Text
    ' lblText.Text = Replace(lblText.Text, """", "")
    mIcon = Icon
    Dim addIcon As Integer = 16
    If Icon = DragDropEffects.None Then addIcon = 0
    Dim TextPadSize As New Size(lblText.Size.Width + addIcon + 4, lblText.Size.Height + 3)
    If (Not Me.Size = TextPadSize) Or (Not Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None) Then
      Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
      Me.Size = TextPadSize
      lblText.Text = mText
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

  Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
    If m.Msg = &H84 Then tmrHide.Start()
    MyBase.WndProc(m)
  End Sub
End Class