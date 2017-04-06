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
    Dim colorNORM As Color = Color.FromArgb(53, 51, 218)
    Dim colorFOCUS As Color = Color.FromArgb(0, 0, 102)
    Dim dispText As String = mText
    Dim inQuote As Boolean = False
    Dim onNextLine As Boolean = False
    Dim cI As Integer = 0
    For P As Integer = 0 To dispText.Length - 1 Step 32
      Dim partText As String = dispText.Substring(P, IIf(P + 32 > dispText.Length - 1, dispText.Length - P, 32))
      Dim partText2 As String = partText.Replace("""", "")
      Dim partColors(partText2.Length - 1) As Color
      Dim C As Integer = 0
      For I As Integer = 0 To partText.Length - 1
        If partText(I) = """" Then
          If Not inQuote Then
            inQuote = True
          Else
            inQuote = False
          End If
        ElseIf partText(I) = vbCr Then

          If I + 8 < partText.Length - 1 AndAlso partText.Substring(I, 8) = vbNewLine & "  And " Then
            onNextLine = False
            partColors(C) = colorNORM
            C += 1
          Else
            onNextLine = True
            partColors(C) = colorFOCUS
            C += 1
          End If
        ElseIf inQuote Or onNextLine Then
          partColors(C) = colorFOCUS
          C += 1
        Else
          partColors(C) = colorNORM
          C += 1
        End If
      Next

      Dim partFormat As New StringFormat(StringFormatFlags.FitBlackBox)
      Dim partChars(partText2.Length - 1) As CharacterRange
      For I As Integer = 0 To partText2.Length - 1
        partChars(I) = New CharacterRange(cI, 1)
        cI += 1
      Next
      partFormat.SetMeasurableCharacterRanges(partChars)
      Dim partRegs As Region() = g.MeasureCharacterRanges(dispText.Replace("""", ""), SystemFonts.CaptionFont, New Rectangle(0, 0, 300, 250), partFormat)

      For I As Integer = 0 To partChars.Length - 1
        If partText2(I) = " " Or partText2(I) = vbCr Or partText2(I) = vbLf Then Continue For
        g.DrawString(partText2(I), SystemFonts.CaptionFont, New SolidBrush(partColors(I)), X + partRegs(I).GetBounds(g).X, Y + partRegs(I).GetBounds(g).Y, partFormat)
      Next
    Next
  End Sub

  Public Sub SetText(Message As String, Icon As DragDropEffects)
    If Not Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None Then Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
    tmrHide.Stop()
    Dim lines As Integer = ((Message.Length - Message.Replace(vbNewLine, "").Length) / 2) + 1
    If lines > 6 Then
      Dim origLines As Integer = lines
      Do Until lines <= 5
        Message = Message.Substring(0, Message.LastIndexOf(vbNewLine))
        lines = ((Message.Length - Message.Replace(vbNewLine, "").Length) / 2) + 1
      Loop
      Message &= vbNewLine & "  And " & (origLines - lines) & " other files..."
    End If

    mText = Message
    mIcon = Icon
    Dim addIcon As Integer = 20
    If Icon = DragDropEffects.None Then addIcon = 0
    Dim imgBG As New Bitmap(300, 250)
    Dim textPadSize As SizeF = Nothing
    Using g As Graphics = Graphics.FromImage(imgBG)
      textPadSize = g.MeasureString(mText.Replace("""", ""), SystemFonts.CaptionFont, 300)
      textPadSize.Height += 4
      If textPadSize.Height < 20 Then textPadSize.Height = 20
      textPadSize.Width += addIcon
    End Using
    If (Not Me.Size = textPadSize) Or (Not Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None) Then
      Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
      Me.Size = New Size(textPadSize.Width, textPadSize.Height)
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