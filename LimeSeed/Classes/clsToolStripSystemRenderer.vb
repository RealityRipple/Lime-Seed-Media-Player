Imports System.Drawing
Imports System.Drawing.Drawing2D
Public Class ToolStripSystemRenderer
  Inherits ToolStripRenderer
  Private b_System As Boolean
  Private BGColor As Color
  Private FGColor As Color
  Private BorderColor As Color
  Private HoverColor As Color
  Public Sub New(SystemStyle As Boolean)
    b_System = SystemStyle
    If Not SystemStyle Then
      BGColor = Color.ForestGreen
      FGColor = Color.White
      BorderColor = Color.ForestGreen
      HoverColor = Color.DarkGreen
    End If
  End Sub
  Protected Overrides Sub OnRenderArrow(e As System.Windows.Forms.ToolStripArrowRenderEventArgs)
    If b_System Then
      e.ArrowColor = Color.Black
      e.Direction = ArrowDirection.Right
    Else
      e.ArrowColor = FGColor
      e.Direction = ArrowDirection.Right
    End If
    MyBase.OnRenderArrow(e)
  End Sub
  Protected Overrides Sub OnRenderImageMargin(e As System.Windows.Forms.ToolStripRenderEventArgs)
    If b_System Then

    Else
      Dim fillBrush As New LinearGradientBrush(e.AffectedBounds, BorderColor, HoverColor, LinearGradientMode.Horizontal)
      e.Graphics.FillRectangle(New SolidBrush(BGColor), e.ConnectedArea)
      e.Graphics.FillRectangle(fillBrush, e.AffectedBounds)
    End If
    MyBase.OnRenderImageMargin(e)
  End Sub
  Protected Overrides Sub OnRenderItemText(e As System.Windows.Forms.ToolStripItemTextRenderEventArgs)
    If e.Item.Enabled Then
      e.TextColor = FGColor
    Else
      e.TextColor = SystemColors.InactiveCaptionText
    End If
    MyBase.OnRenderItemText(e)
  End Sub
  Protected Overrides Sub OnRenderSeparator(e As System.Windows.Forms.ToolStripSeparatorRenderEventArgs)
    If b_System Then
      Dim BGColor As Color = Color.FromArgb(241, 241, 241)
      e.Graphics.FillRectangle(New SolidBrush(BGColor), 0, 0, e.Item.Width, e.Item.Height)
      Dim DarkBorder As New Pen(Color.FromArgb(226, 227, 227))
      Dim LightBorder As New Pen(Brushes.White)
      e.Graphics.DrawLine(DarkBorder, New Point(25, 0), New Point(25, e.Item.Height - 1))
      e.Graphics.DrawLine(LightBorder, New Point(26, 0), New Point(26, e.Item.Height - 1))
      e.Graphics.DrawLine(DarkBorder, 27, 2, e.Item.Width - 1, 2)
      e.Graphics.DrawLine(LightBorder, 27, 3, e.Item.Width - 1, 3)
    Else
      e.Graphics.DrawLine(New Pen(BorderColor), 23, 2, e.Item.Width - 1, 2)
    End If
    MyBase.OnRenderSeparator(e)
  End Sub
  Protected Overrides Sub OnRenderItemCheck(e As System.Windows.Forms.ToolStripItemImageRenderEventArgs)
    If e.Item.Image Is Nothing Then
      If b_System Then
        If e.Item.Enabled Then
          Dim BlockRect As New Rectangle(3, 0, 24, 22)
          Dim SelectionBG As New Drawing2D.LinearGradientBrush(New Rectangle(3, 0, BlockRect.Width - 3, BlockRect.Height), Color.FromArgb(16, SystemColors.Highlight), Color.FromArgb(32, SystemColors.Highlight), Drawing2D.LinearGradientMode.Vertical)
          Dim SelectionBorder As New Pen(Color.FromArgb(100, SystemColors.MenuHighlight))
          Dim SelectionCorner As Brush = New SolidBrush(Color.FromArgb(206, 225, 249))
          Dim SelectionCornerD As Brush = New SolidBrush(Color.FromArgb(239, 240, 241))
          If e.Item.Selected Then
            e.Graphics.DrawLine(SelectionBorder, New Point(BlockRect.Width, 1), New Point(BlockRect.Width, BlockRect.Height - 2))
            e.Graphics.FillRectangle(SelectionCorner, BlockRect.Width - 1, 1, 2, 1)
            e.Graphics.FillRectangle(SelectionCorner, BlockRect.Width - 1, BlockRect.Height - 2, 2, 1)
          Else
            e.Graphics.FillRectangle(SelectionBG, 3, 0, BlockRect.Width - 3, BlockRect.Height)
            e.Graphics.DrawRectangle(SelectionBorder, New Rectangle(3, 0, BlockRect.Width - 3, BlockRect.Height - 1))
            e.Graphics.FillRectangle(SelectionCorner, 3, 0, 2, 2)
            e.Graphics.FillRectangle(SelectionCorner, 3, BlockRect.Height - 2, 2, 2)
            e.Graphics.FillRectangle(SelectionCorner, BlockRect.Width - 1, 0, 2, 2)
            e.Graphics.FillRectangle(SelectionCorner, BlockRect.Width - 1, BlockRect.Height - 2, 2, 2)
          End If

          e.Graphics.FillRectangle(SelectionCornerD, 3, 0, 1, 1)
          e.Graphics.FillRectangle(SelectionCornerD, 3, BlockRect.Height - 1, 1, 1)
          If Not e.Item.Selected Then
            e.Graphics.FillRectangle(SelectionCornerD, BlockRect.Width, 0, 1, 1)
            e.Graphics.FillRectangle(SelectionCornerD, BlockRect.Width, BlockRect.Height - 1, 1, 1)
          End If
          Dim CheckMark As Image = New Bitmap(9, 11)
          Using g As Graphics = Graphics.FromImage(CheckMark)
            g.Clear(Color.Transparent)
            g.FillRectangle(New SolidBrush(Color.FromArgb(119, 125, 204)), 7, 0, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(109, 115, 200)), 8, 0, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(206, 212, 238)), 6, 1, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(12, 18, 161)), 7, 1, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(191, 198, 232)), 8, 1, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(66, 73, 183)), 6, 2, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(83, 89, 189)), 7, 2, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(190, 198, 232)), 5, 3, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(12, 18, 161)), 6, 3, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(190, 198, 232)), 7, 3, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(225, 233, 246)), 0, 4, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(230, 238, 249)), 1, 4, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(51, 58, 177)), 5, 4, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(82, 89, 189)), 6, 4, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(68, 74, 184)), 0, 5, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(66, 73, 183)), 1, 5, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(232, 240, 249)), 2, 5, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(173, 181, 225)), 4, 5, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(12, 18, 161)), 5, 5, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(189, 198, 232)), 6, 5, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(145, 152, 214)), 0, 6, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(12, 18, 161)), 1, 6, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(80, 86, 189)), 2, 6, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(216, 223, 242)), 3, 6, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(39, 44, 171)), 4, 6, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(82, 89, 189)), 5, 6, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(64, 71, 182)), 1, 7, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(12, 18, 161)), 2, 7, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(18, 24, 163)), 3, 7, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(12, 18, 161)), 4, 7, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(189, 198, 232)), 5, 7, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(199, 207, 237)), 1, 8, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(18, 24, 163)), 2, 8, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(12, 18, 161)), 3, 8, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(82, 89, 189)), 4, 8, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(118, 125, 204)), 2, 9, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(12, 18, 161)), 3, 9, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(188, 197, 232)), 4, 9, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(227, 234, 248)), 2, 10, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(112, 120, 202)), 3, 10, 1, 1)
          End Using
          e.Graphics.DrawImageUnscaled(CheckMark, 9, 5)
        Else
          Dim BlockRect As New Rectangle(3, 0, 24, 22)
          Dim SelectionBG As New LinearGradientBrush(New Rectangle(3, 0, BlockRect.Width - 6, BlockRect.Height), Color.FromArgb(16, SystemColors.ControlDark), Color.FromArgb(32, SystemColors.ControlDark), LinearGradientMode.Vertical)
          Dim SelectionBorder As New Pen(Color.FromArgb(100, SystemColors.ControlDark))
          Dim SelectionCorner As Brush = New SolidBrush(Color.FromArgb(226, 226, 226))
          Dim SelectionCornerD As Brush = New SolidBrush(Color.FromArgb(240, 240, 240))
          If e.Item.Selected Then
            e.Graphics.DrawLine(SelectionBorder, New Point(BlockRect.Width, 1), New Point(BlockRect.Width, BlockRect.Height - 2))
            e.Graphics.FillRectangle(SelectionCorner, BlockRect.Width - 1, 1, 2, 1)
            e.Graphics.FillRectangle(SelectionCorner, BlockRect.Width - 1, BlockRect.Height - 2, 2, 1)
          Else
            e.Graphics.FillRectangle(SelectionBG, 3, 0, BlockRect.Width - 3, BlockRect.Height)
            e.Graphics.DrawRectangle(SelectionBorder, New Rectangle(3, 0, BlockRect.Width - 3, BlockRect.Height - 1))
            e.Graphics.FillRectangle(SelectionCorner, 3, 0, 2, 2)
            e.Graphics.FillRectangle(SelectionCorner, 3, BlockRect.Height - 2, 2, 2)
            e.Graphics.FillRectangle(SelectionCorner, BlockRect.Width - 1, 0, 2, 2)
            e.Graphics.FillRectangle(SelectionCorner, BlockRect.Width - 1, BlockRect.Height - 2, 2, 2)
          End If
          e.Graphics.FillRectangle(SelectionCornerD, 3, 0, 1, 1)
          e.Graphics.FillRectangle(SelectionCornerD, 3, BlockRect.Height - 1, 1, 1)
          If Not e.Item.Selected Then
            e.Graphics.FillRectangle(SelectionCornerD, BlockRect.Width, 0, 1, 1)
            e.Graphics.FillRectangle(SelectionCornerD, BlockRect.Width, BlockRect.Height - 1, 1, 1)
          End If
          Dim CheckMark As Image = New Bitmap(9, 11)
          Using g As Graphics = Graphics.FromImage(CheckMark)
            g.Clear(Color.Transparent)
            g.FillRectangle(New SolidBrush(Color.FromArgb(182, 182, 184)), 7, 0, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(177, 178, 180)), 8, 0, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(222, 222, 223)), 6, 1, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(132, 133, 137)), 7, 1, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(216, 217, 217)), 8, 1, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(158, 159, 162)), 6, 2, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(165, 166, 168)), 7, 2, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(216, 217, 217)), 5, 3, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(132, 133, 137)), 6, 3, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(216, 217, 217)), 7, 3, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(233, 233, 233)), 0, 4, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(235, 235, 235)), 1, 4, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(151, 152, 155)), 5, 4, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(165, 166, 168)), 6, 4, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(159, 160, 163)), 0, 5, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(158, 159, 162)), 1, 5, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(236, 236, 236)), 2, 5, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(208, 209, 210)), 4, 5, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(132, 133, 137)), 5, 5, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(216, 217, 217)), 6, 5, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(195, 197, 197)), 0, 6, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(132, 133, 137)), 1, 6, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(165, 165, 168)), 2, 6, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(228, 228, 228)), 3, 6, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(144, 145, 149)), 4, 6, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(165, 166, 168)), 5, 6, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(157, 158, 161)), 1, 7, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(132, 133, 137)), 2, 7, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(135, 136, 140)), 3, 7, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(132, 133, 137)), 4, 7, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(216, 217, 217)), 5, 7, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(221, 221, 222)), 1, 8, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(135, 136, 140)), 2, 8, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(132, 133, 137)), 3, 8, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(165, 166, 168)), 4, 8, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(182, 183, 185)), 2, 9, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(132, 133, 137)), 3, 9, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(216, 217, 217)), 4, 9, 1, 1)

            g.FillRectangle(New SolidBrush(Color.FromArgb(234, 234, 234)), 2, 10, 1, 1)
            g.FillRectangle(New SolidBrush(Color.FromArgb(180, 181, 183)), 3, 10, 1, 1)
          End Using
          e.Graphics.DrawImageUnscaled(CheckMark, 9, 5)
        End If
      Else
        MyBase.OnRenderItemCheck(e)
      End If
    End If
  End Sub
  Protected Overrides Sub OnRenderToolStripBackground(e As System.Windows.Forms.ToolStripRenderEventArgs)
    If b_System Then
      If e.ToolStrip.Parent Is Nothing Then
        Dim BGColor As Color = Color.FromArgb(241, 241, 241)
        e.Graphics.FillRectangle(New SolidBrush(BGColor), 0, 0, e.AffectedBounds.Width, e.AffectedBounds.Height)
      Else
        Dim TopTwoFifths As Integer = (e.AffectedBounds.Height - 2) / 5 * 2
        Dim BottomThreeFifths As Integer = (e.AffectedBounds.Height - 2) / 5 * 3
        Dim TopRect As New Rectangle(0, 0, e.AffectedBounds.Width - 1, (e.AffectedBounds.Height - 2) / 5 * 2)
        Dim TopGradient As New LinearGradientBrush(TopRect, Color.White, Color.FromArgb(229, 234, 245), LinearGradientMode.Vertical)
        e.Graphics.FillRectangle(TopGradient, TopRect)
        Dim BottomRect As New Rectangle(0, (e.AffectedBounds.Height - 2) / 5 * 2, e.AffectedBounds.Width - 1, (e.AffectedBounds.Height - 2) / 5 * 3 - 1)
        e.Graphics.FillRectangle(New LinearGradientBrush(BottomRect, Color.FromArgb(212, 219, 237), Color.FromArgb(225, 230, 246), LinearGradientMode.Vertical), BottomRect)
        Dim BottomLineColor As New Pen(Color.FromArgb(182, 188, 204))
        Dim BottomLeft As New Point(0, e.AffectedBounds.Height - 1)
        Dim BottomRight As New Point(e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1)
        e.Graphics.DrawLine(BottomLineColor, BottomLeft, BottomRight)
      End If
    Else
      e.Graphics.FillRectangle(New SolidBrush(BGColor), e.AffectedBounds)
    End If
    MyBase.OnRenderToolStripBackground(e)
  End Sub
  Protected Overrides Sub OnRenderToolStripBorder(e As System.Windows.Forms.ToolStripRenderEventArgs)
    If b_System Then
      If e.ToolStrip.Parent Is Nothing Then
        e.Graphics.DrawRectangle(New Pen(Color.FromArgb(151, 151, 151)), New Rectangle(e.AffectedBounds.Left, e.AffectedBounds.Top, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1))
      Else
        e.Graphics.DrawRectangle(New Pen(Color.FromArgb(254, 254, 255)), New Rectangle(e.AffectedBounds.Left, e.AffectedBounds.Top, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 2))
        e.Graphics.DrawLine(New Pen(Color.FromArgb(182, 188, 204)), New Point(0, e.AffectedBounds.Height - 3), New Point(e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 3))
        e.Graphics.DrawLine(New Pen(Color.FromArgb(240, 240, 240)), New Point(0, e.AffectedBounds.Height - 2), New Point(e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 2))
        e.Graphics.DrawLine(New Pen(Color.FromArgb(240, 240, 240)), New Point(0, e.AffectedBounds.Height - 1), New Point(e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1))
      End If
    Else
      e.Graphics.DrawRectangle(New Pen(BorderColor), New Rectangle(e.AffectedBounds.Left, e.AffectedBounds.Top, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1))
      MyBase.OnRenderToolStripBorder(e)
    End If
  End Sub
  Protected Overloads Overrides Sub OnRenderMenuItemBackground(ByVal e As ToolStripItemRenderEventArgs)
    Try
      If e.Item.OwnerItem Is Nothing Then
        If b_System Then
          If e.Item.GetCurrentParent.GetType = GetType(ContextMenuStrip) Then
            Dim BGColor As Color = Color.FromArgb(241, 241, 241)
            e.Graphics.FillRectangle(New SolidBrush(BGColor), 0, 0, e.Item.Width, e.Item.Height)
            If e.ToolStrip.GetType = GetType(ContextMenuStrip) Then
              Dim cToolStrip As ContextMenuStrip = e.ToolStrip
              If Not cToolStrip.ShowImageMargin And Not cToolStrip.ShowCheckMargin Then
                'nope
              Else
                Dim DarkBorder As New Pen(Color.FromArgb(226, 227, 227))
                Dim LightBorder As New Pen(Brushes.White)
                e.Graphics.DrawLine(DarkBorder, New Point(27, 0), New Point(27, e.Item.Height - 1))
                e.Graphics.DrawLine(LightBorder, New Point(28, 0), New Point(28, e.Item.Height - 1))
              End If
            Else
              Dim DarkBorder As New Pen(Color.FromArgb(226, 227, 227))
              Dim LightBorder As New Pen(Brushes.White)
              e.Graphics.DrawLine(DarkBorder, New Point(27, 0), New Point(27, e.Item.Height - 1))
              e.Graphics.DrawLine(LightBorder, New Point(28, 0), New Point(28, e.Item.Height - 1))
            End If
            If e.Item.Selected Then
              If e.Item.Enabled Then
                Dim SelectionBG As New LinearGradientBrush(New Rectangle(3, 0, e.Item.Width - 6, e.Item.Height), Color.FromArgb(16, SystemColors.Highlight), Color.FromArgb(32, SystemColors.Highlight), LinearGradientMode.Vertical)
                Dim SelectionBorder As New Pen(Color.FromArgb(100, SystemColors.MenuHighlight))
                Dim SelectionCorner As Brush = New SolidBrush(Color.FromArgb(206, 225, 249))
                Dim SelectionCornerD As Brush = New SolidBrush(Color.FromArgb(239, 240, 241))
                e.Graphics.FillRectangle(SelectionBG, 3, 0, e.Item.Width - 6, e.Item.Height)
                e.Graphics.DrawRectangle(SelectionBorder, New Rectangle(3, 0, e.Item.Width - 6, e.Item.Height - 1))
                e.Graphics.FillRectangle(SelectionCorner, 3, 0, 2, 2)
                e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 4, 0, 2, 2)
                e.Graphics.FillRectangle(SelectionCorner, 3, e.Item.Height - 2, 2, 2)
                e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 4, e.Item.Height - 2, 2, 2)
                e.Graphics.FillRectangle(SelectionCornerD, 3, 0, 1, 1)
                e.Graphics.FillRectangle(SelectionCornerD, e.Item.Width - 3, 0, 1, 1)
                e.Graphics.FillRectangle(SelectionCornerD, 3, e.Item.Height - 1, 1, 1)
                e.Graphics.FillRectangle(SelectionCornerD, e.Item.Width - 3, e.Item.Height - 1, 1, 1)
              Else
                Dim SelectionBG As New LinearGradientBrush(New Rectangle(3, 0, e.Item.Width - 6, e.Item.Height), Color.FromArgb(16, SystemColors.ControlDark), Color.FromArgb(32, SystemColors.ControlDark), LinearGradientMode.Vertical)
                Dim SelectionBorder As New Pen(Color.FromArgb(100, SystemColors.ControlDark)) ' 213, 212, 212))
                Dim SelectionCorner As Brush = New SolidBrush(Color.FromArgb(226, 226, 226))
                Dim SelectionCornerD As Brush = New SolidBrush(Color.FromArgb(240, 240, 240))
                e.Graphics.FillRectangle(SelectionBG, 3, 0, e.Item.Width - 6, e.Item.Height - 1)
                e.Graphics.DrawRectangle(SelectionBorder, New Rectangle(3, 0, e.Item.Width - 6, e.Item.Height - 1))
                e.Graphics.FillRectangle(SelectionCorner, 3, 0, 2, 2)
                e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 4, 0, 2, 2)
                e.Graphics.FillRectangle(SelectionCorner, 3, e.Item.Height - 2, 2, 2)
                e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 4, e.Item.Height - 2, 2, 2)
                e.Graphics.FillRectangle(SelectionCornerD, 3, 0, 1, 1)
                e.Graphics.FillRectangle(SelectionCornerD, e.Item.Width - 3, 0, 1, 1)
                e.Graphics.FillRectangle(SelectionCornerD, 3, e.Item.Height - 1, 1, 1)
                e.Graphics.FillRectangle(SelectionCornerD, e.Item.Width - 3, e.Item.Height - 1, 1, 1)
              End If
            End If
          Else
            If e.Item.Pressed Then
              e.Graphics.FillRectangle(New LinearGradientBrush(New Rectangle(0, 0, e.Item.Width - 1, e.Item.Height - 2), Color.FromArgb(96, 64, 64, 64), Color.FromArgb(32, 64, 64, 64), LinearGradientMode.Vertical), New Rectangle(0, 0, e.Item.Width - 1, e.Item.Height - 1))
              e.Graphics.DrawRectangle(New Pen(Color.FromArgb(192, 88, 88, 89)), 0, 0, e.Item.Width - 1, e.Item.Height - 2)
              e.Graphics.DrawLines(New Pen(Color.FromArgb(159, 160, 162)), {New Point(1, e.Item.Height - 2), New Point(1, 1), New Point(e.Item.Width - 2, 1)})
              e.Graphics.DrawLines(New Pen(Color.FromArgb(182, 184, 188)), {New Point(2, e.Item.Height - 3), New Point(2, 2), New Point(e.Item.Width - 3, 2)})
              Dim SelectionCorner As Brush = New SolidBrush(Color.FromArgb(144, 146, 148))
              Dim SelectionCornerD As Brush = New SolidBrush(Color.FromArgb(252, 253, 254))
              Dim SelectionCornerD2 As Brush = New SolidBrush(Color.FromArgb(225, 230, 246))
              Dim SelectionCornerD3 As Brush = New SolidBrush(Color.FromArgb(158, 161, 173))
              e.Graphics.FillRectangle(SelectionCorner, 0, 0, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 2, 0, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, 0, e.Item.Height - 3, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 2, e.Item.Height - 3, 2, 2)
              e.Graphics.FillRectangle(SelectionCornerD, 0, 0, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD, e.Item.Width - 1, 0, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD3, 0, e.Item.Height - 2, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD2, e.Item.Width - 1, e.Item.Height - 2, 1, 1)
            ElseIf e.Item.Selected Then
              Dim TopSelectionRect As New Rectangle(0, 0, e.Item.Width - 1, e.Item.Height / 2 - 1)
              Dim TopSelectionBG As New LinearGradientBrush(TopSelectionRect, Color.FromArgb(128, 255, 255, 255), Color.FromArgb(48, 255, 255, 255), LinearGradientMode.Vertical)
              e.Graphics.FillRectangle(TopSelectionBG, TopSelectionRect)
              Dim BottomSelectionRect As New Rectangle(0, e.Item.Height / 2, e.Item.Width - 1, e.Item.Height / 2 - 2)
              Dim BottomSelectionBG As New LinearGradientBrush(BottomSelectionRect, Color.FromArgb(32, 64, 64, 64), Color.FromArgb(16, 64, 64, 64), LinearGradientMode.Vertical)
              e.Graphics.FillRectangle(BottomSelectionBG, BottomSelectionRect)
              Dim BorderColor As New Pen(Color.FromArgb(192, 127, 131, 142))
              e.Graphics.DrawRectangle(BorderColor, 0, 0, e.Item.Width - 1, e.Item.Height - 2)
              Dim SelectionCorner As Brush = New SolidBrush(Color.FromArgb(187, 188, 188))
              Dim SelectionCornerD As Brush = New SolidBrush(Color.FromArgb(252, 253, 254))
              Dim SelectionCornerD2 As Brush = New SolidBrush(Color.FromArgb(225, 230, 246))
              e.Graphics.FillRectangle(SelectionCorner, 0, 0, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 2, 0, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, 0, e.Item.Height - 3, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 2, e.Item.Height - 3, 2, 2)
              e.Graphics.FillRectangle(SelectionCornerD, 0, 0, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD, e.Item.Width - 1, 0, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD2, 0, e.Item.Height - 2, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD2, e.Item.Width - 1, e.Item.Height - 2, 1, 1)
            End If
          End If
        Else
          If e.Item.GetCurrentParent.GetType = GetType(ContextMenuStrip) Then
            Dim Left As Integer = 25
            Dim rc As New Rectangle(Left, 0, e.Item.Width - Left - 1, e.Item.Height - 1)
            Dim bgBrush As SolidBrush
            If Not e.Item.Enabled Then
              bgBrush = New SolidBrush(BGColor)
            ElseIf e.Item.Pressed Then
              bgBrush = New SolidBrush(HoverColor)
            ElseIf e.Item.Selected Then
              bgBrush = New SolidBrush(HoverColor)
            Else
              bgBrush = New SolidBrush(BGColor)
            End If
            e.Graphics.FillRectangle(bgBrush, rc)
          Else
            Dim rc As New Rectangle(0, 0, e.Item.Width - 1, e.Item.Height - 1)
            If Not e.Item.Enabled Then
              e.Graphics.FillRectangle(New SolidBrush(BGColor), rc)
            ElseIf e.Item.Pressed Then
              e.Graphics.FillRectangle(New LinearGradientBrush(rc, BGColor, HoverColor, LinearGradientMode.Vertical), rc)
              e.Graphics.DrawRectangle(New Pen(BorderColor), rc)
            ElseIf e.Item.Selected Then
              e.Graphics.FillRectangle(New SolidBrush(HoverColor), rc)
              e.Graphics.DrawRectangle(New Pen(BorderColor), rc)
            Else
              e.Graphics.FillRectangle(New SolidBrush(BGColor), rc)
            End If
          End If
        End If
      Else
        If b_System Then
          Dim BGColor As Color = Color.FromArgb(241, 241, 241)
          e.Graphics.FillRectangle(New SolidBrush(BGColor), 0, 0, e.Item.Width, e.Item.Height)
          Dim DarkBorder As New Pen(Color.FromArgb(226, 227, 227))
          Dim LightBorder As New Pen(Brushes.White)
          e.Graphics.DrawLine(DarkBorder, New Point(27, 0), New Point(27, e.Item.Height - 1))
          e.Graphics.DrawLine(LightBorder, New Point(28, 0), New Point(28, e.Item.Height - 1))
          If e.Item.Selected Then
            If e.Item.Enabled Then
              Dim SelectionBG As New LinearGradientBrush(New Rectangle(3, 0, e.Item.Width - 6, e.Item.Height), Color.FromArgb(16, SystemColors.Highlight), Color.FromArgb(32, SystemColors.Highlight), LinearGradientMode.Vertical)
              Dim SelectionBorder As New Pen(Color.FromArgb(100, SystemColors.MenuHighlight))
              Dim SelectionCorner As Brush = New SolidBrush(Color.FromArgb(206, 225, 249))
              Dim SelectionCornerD As Brush = New SolidBrush(Color.FromArgb(239, 240, 241))
              e.Graphics.FillRectangle(SelectionBG, 3, 0, e.Item.Width - 6, e.Item.Height)
              e.Graphics.DrawRectangle(SelectionBorder, New Rectangle(3, 0, e.Item.Width - 6, e.Item.Height - 1))
              e.Graphics.FillRectangle(SelectionCorner, 3, 0, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 4, 0, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, 3, e.Item.Height - 2, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 4, e.Item.Height - 2, 2, 2)
              e.Graphics.FillRectangle(SelectionCornerD, 3, 0, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD, e.Item.Width - 3, 0, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD, 3, e.Item.Height - 1, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD, e.Item.Width - 3, e.Item.Height - 1, 1, 1)
            Else
              Dim SelectionBG As New LinearGradientBrush(New Rectangle(3, 0, e.Item.Width - 6, e.Item.Height), Color.FromArgb(16, SystemColors.ControlDark), Color.FromArgb(32, SystemColors.ControlDark), LinearGradientMode.Vertical)
              Dim SelectionBorder As New Pen(Color.FromArgb(100, SystemColors.ControlDark)) ' 213, 212, 212))
              Dim SelectionCorner As Brush = New SolidBrush(Color.FromArgb(226, 226, 226))
              Dim SelectionCornerD As Brush = New SolidBrush(Color.FromArgb(240, 240, 240))
              e.Graphics.FillRectangle(SelectionBG, 3, 0, e.Item.Width - 6, e.Item.Height - 1)
              e.Graphics.DrawRectangle(SelectionBorder, New Rectangle(3, 0, e.Item.Width - 6, e.Item.Height - 1))
              e.Graphics.FillRectangle(SelectionCorner, 3, 0, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 4, 0, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, 3, e.Item.Height - 2, 2, 2)
              e.Graphics.FillRectangle(SelectionCorner, e.Item.Width - 4, e.Item.Height - 2, 2, 2)
              e.Graphics.FillRectangle(SelectionCornerD, 3, 0, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD, e.Item.Width - 3, 0, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD, 3, e.Item.Height - 1, 1, 1)
              e.Graphics.FillRectangle(SelectionCornerD, e.Item.Width - 3, e.Item.Height - 1, 1, 1)
            End If
          End If
        Else
          Dim Left As Integer = 25
          Dim rc As New Rectangle(Left, 0, e.Item.Width - Left - 1, e.Item.Height - 1)
          Dim bgBrush As SolidBrush
          If Not e.Item.Enabled Then
            bgBrush = New SolidBrush(BGColor)
          ElseIf e.Item.Pressed Then
            bgBrush = New SolidBrush(HoverColor)
          ElseIf e.Item.Selected Then
            bgBrush = New SolidBrush(HoverColor)
          Else
            bgBrush = New SolidBrush(BGColor)
          End If
          e.Graphics.FillRectangle(bgBrush, rc)
        End If
      End If
    Catch ex As Exception
    End Try
  End Sub
End Class