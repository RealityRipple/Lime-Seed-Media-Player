Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Windows.Forms.VisualStyles
Public Class ToolStripRadioButtonMenuItem
  Inherits ToolStripMenuItem
  Public Sub New()
    MyBase.New()
    Initialize()
  End Sub
  Public Sub New(ByVal text As String)
    MyBase.New(text, Nothing, CType(Nothing, EventHandler))
    Initialize()
  End Sub
  Public Sub New(ByVal image As Image)
    MyBase.New(Nothing, image, CType(Nothing, EventHandler))
    Initialize()
  End Sub
  Public Sub New(ByVal text As String, ByVal image As Image)
    MyBase.New(text, image, CType(Nothing, EventHandler))
    Initialize()
  End Sub
  Public Sub New(ByVal text As String, ByVal image As Image, ByVal onClick As EventHandler)
    MyBase.New(text, image, onClick)
    Initialize()
  End Sub
  Public Sub New(ByVal text As String, ByVal image As Image, ByVal onClick As EventHandler, ByVal name As String)
    MyBase.New(text, image, onClick, name)
    Initialize()
  End Sub
  Public Sub New(ByVal text As String, ByVal image As Image, ByVal ParamArray dropDownItems() As ToolStripItem)
    MyBase.New(text, image, dropDownItems)
    Initialize()
  End Sub
  Public Sub New(ByVal text As String, ByVal image As Image, ByVal onClick As EventHandler, ByVal shortcutKeys As Keys)
    MyBase.New(text, image, onClick)
    Initialize()
    Me.ShortcutKeys = shortcutKeys
  End Sub
  Private Sub Initialize()
    CheckOnClick = True
  End Sub
  Protected Overrides Sub OnCheckedChanged(ByVal e As EventArgs)
    MyBase.OnCheckedChanged(e)
    If Not Checked OrElse Me.Parent Is Nothing Then Return
    For Each item As ToolStripItem In Parent.Items
      Dim radioItem As ToolStripRadioButtonMenuItem = TryCast(item, ToolStripRadioButtonMenuItem)
      If radioItem IsNot Nothing AndAlso radioItem IsNot Me AndAlso radioItem.Checked Then
        radioItem.Checked = False
        Return
      End If
    Next
  End Sub
  Protected Overrides Sub OnClick(ByVal e As EventArgs)
    If Checked Then Return
    MyBase.OnClick(e)
  End Sub
  Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
    If Image IsNot Nothing Then
      MyBase.OnPaint(e)
      Return
    Else
      Dim currentState As CheckState = Me.CheckState
      Me.CheckState = CheckState.Unchecked
      MyBase.OnPaint(e)
      Me.CheckState = currentState
    End If
    If Checked Then
      If Enabled Then
        Dim BlockRect As New Rectangle(3, 0, 24, 22)
        Dim SelectionBG As New Drawing2D.LinearGradientBrush(New Rectangle(3, 0, BlockRect.Width - 3, BlockRect.Height), Color.FromArgb(16, SystemColors.Highlight), Color.FromArgb(32, SystemColors.Highlight), Drawing2D.LinearGradientMode.Vertical)
        Dim SelectionBorder As New Pen(Color.FromArgb(100, SystemColors.MenuHighlight))
        Dim SelectionCorner As Brush = New SolidBrush(Color.FromArgb(206, 225, 249))
        Dim SelectionCornerD As Brush = New SolidBrush(Color.FromArgb(239, 240, 241))
        If Selected Then
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
        If Not Selected Then
          e.Graphics.FillRectangle(SelectionCornerD, BlockRect.Width, 0, 1, 1)
          e.Graphics.FillRectangle(SelectionCornerD, BlockRect.Width, BlockRect.Height - 1, 1, 1)
        End If
        Dim RadioMark As Image = New Bitmap(8, 8)
        Using g As Graphics = Graphics.FromImage(RadioMark)
          g.Clear(Color.Transparent)
          g.FillRectangle(New SolidBrush(Color.FromArgb(118, 124, 171)), 2, 0, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(36, 39, 116)), 3, 0, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(61, 64, 132)), 4, 0, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(144, 151, 187)), 5, 0, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(37, 40, 117)), 1, 1, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(111, 113, 179)), 2, 1, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(159, 160, 214)), 3, 1, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(195, 196, 233)), 4, 1, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(177, 178, 218)), 5, 1, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(93, 96, 152)), 6, 1, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(125, 130, 174)), 0, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(79, 81, 153)), 1, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(129, 130, 193)), 2, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(161, 162, 212)), 3, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(204, 204, 234)), 4, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(204, 204, 237)), 5, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(157, 157, 207)), 6, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(123, 130, 175)), 7, 2, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(43, 45, 120)), 0, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(79, 80, 154)), 1, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(105, 107, 175)), 2, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(120, 121, 186)), 3, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(164, 166, 212)), 4, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(169, 170, 217)), 5, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(145, 147, 206)), 6, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(30, 34, 114)), 7, 3, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(54, 56, 128)), 0, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(56, 58, 135)), 1, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(82, 84, 156)), 2, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(97, 98, 168)), 3, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(111, 113, 179)), 4, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(126, 127, 191)), 5, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(120, 121, 186)), 6, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(40, 44, 121)), 7, 4, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(146, 149, 186)), 0, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(27, 28, 113)), 1, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(60, 62, 139)), 2, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(74, 75, 150)), 3, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(88, 90, 161)), 4, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(103, 104, 172)), 5, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(70, 72, 147)), 6, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(132, 140, 182)), 7, 5, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(76, 77, 141)), 1, 6, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(22, 24, 109)), 2, 6, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(41, 43, 124)), 3, 6, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(51, 53, 132)), 4, 6, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(44, 46, 127)), 5, 6, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(70, 75, 140)), 6, 6, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(151, 154, 190)), 2, 7, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(69, 71, 136)), 3, 7, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(65, 68, 134)), 4, 7, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(146, 150, 188)), 5, 7, 1, 1)
        End Using
        e.Graphics.DrawImageUnscaled(RadioMark, 10, 7)
      Else
        Dim BlockRect As New Rectangle(3, 0, 24, 22)
        Dim SelectionBG As New Drawing2D.LinearGradientBrush(New Rectangle(3, 0, BlockRect.Width - 6, BlockRect.Height), Color.FromArgb(16, SystemColors.ControlDark), Color.FromArgb(32, SystemColors.ControlDark), Drawing2D.LinearGradientMode.Vertical)
        Dim SelectionBorder As New Pen(Color.FromArgb(100, SystemColors.ControlDark))
        Dim SelectionCorner As Brush = New SolidBrush(Color.FromArgb(226, 226, 226))
        Dim SelectionCornerD As Brush = New SolidBrush(Color.FromArgb(240, 240, 240))
        If Selected Then
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
        If Not Selected Then
          e.Graphics.FillRectangle(SelectionCornerD, BlockRect.Width, 0, 1, 1)
          e.Graphics.FillRectangle(SelectionCornerD, BlockRect.Width, BlockRect.Height - 1, 1, 1)
        End If
        Dim RadioMark As Image = New Bitmap(8, 8)
        Using g As Graphics = Graphics.FromImage(RadioMark)
          g.Clear(Color.Transparent)
          g.FillRectangle(New SolidBrush(Color.FromArgb(186, 186, 186)), 2, 0, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(147, 147, 147)), 3, 0, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(160, 160, 160)), 4, 0, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(198, 198, 198)), 5, 0, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(149, 149, 149)), 1, 1, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(190, 190, 190)), 2, 1, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(214, 214, 214)), 3, 1, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(231, 231, 231)), 4, 1, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(221, 221, 221)), 5, 1, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(176, 176, 176)), 6, 1, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(189, 189, 189)), 0, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(173, 173, 173)), 1, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(199, 199, 199)), 2, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(214, 214, 214)), 3, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(234, 234, 234)), 4, 2, 2, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(211, 211, 211)), 6, 2, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(189, 189, 189)), 7, 2, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(151, 151, 151)), 0, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(173, 173, 173)), 1, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(187, 187, 187)), 2, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(195, 195, 195)), 3, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(215, 215, 215)), 4, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(218, 218, 218)), 5, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(208, 208, 208)), 6, 3, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(145, 145, 145)), 7, 3, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(157, 157, 157)), 0, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(160, 160, 160)), 1, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(174, 174, 174)), 2, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(182, 182, 182)), 3, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(190, 190, 190)), 4, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(198, 198, 198)), 5, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(195, 195, 195)), 6, 4, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(151, 151, 151)), 7, 4, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(199, 199, 199)), 0, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(145, 145, 145)), 1, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(163, 163, 163)), 2, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(170, 170, 170)), 3, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(177, 177, 177)), 4, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(185, 185, 185)), 5, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(168, 168, 168)), 6, 5, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(193, 193, 193)), 7, 5, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(168, 168, 168)), 1, 6, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(142, 142, 142)), 2, 6, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(153, 153, 153)), 3, 6, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(158, 158, 158)), 4, 6, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(154, 154, 154)), 5, 6, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(166, 166, 166)), 6, 6, 1, 1)

          g.FillRectangle(New SolidBrush(Color.FromArgb(201, 201, 201)), 2, 7, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(163, 163, 163)), 3, 7, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(162, 162, 162)), 4, 7, 1, 1)
          g.FillRectangle(New SolidBrush(Color.FromArgb(199, 199, 199)), 5, 7, 1, 1)
        End Using
        e.Graphics.DrawImageUnscaled(RadioMark, 10, 7)
      End If
    End If
  End Sub
  Private mouseHoverState As Boolean = False
  Protected Overrides Sub OnMouseEnter(ByVal e As EventArgs)
    mouseHoverState = True
    Invalidate()
    MyBase.OnMouseEnter(e)
  End Sub
  Protected Overrides Sub OnMouseLeave(ByVal e As EventArgs)
    mouseHoverState = False
    MyBase.OnMouseLeave(e)
  End Sub
  Private mouseDownState As Boolean = False
  Protected Overrides Sub OnMouseDown(ByVal e As MouseEventArgs)
    mouseDownState = True
    Invalidate()
    MyBase.OnMouseDown(e)
  End Sub
  Protected Overrides Sub OnMouseUp(ByVal e As MouseEventArgs)
    mouseDownState = False
    MyBase.OnMouseUp(e)
  End Sub
  Public Overrides Property Enabled() As Boolean
    Get
      Dim ownerMenuItem As ToolStripMenuItem = TryCast(OwnerItem, ToolStripMenuItem)
      If Not DesignMode AndAlso ownerMenuItem IsNot Nothing AndAlso ownerMenuItem.CheckOnClick Then
        Return MyBase.Enabled AndAlso ownerMenuItem.Checked
      Else
        Return MyBase.Enabled
      End If
    End Get
    Set(ByVal value As Boolean)
      MyBase.Enabled = value
    End Set
  End Property
  Protected Overrides Sub OnOwnerChanged(ByVal e As EventArgs)
    Dim ownerMenuItem As ToolStripMenuItem = TryCast(OwnerItem, ToolStripMenuItem)
    If ownerMenuItem IsNot Nothing AndAlso ownerMenuItem.CheckOnClick Then AddHandler ownerMenuItem.CheckedChanged, New EventHandler(AddressOf OwnerMenuItem_CheckedChanged)
    MyBase.OnOwnerChanged(e)
  End Sub
  Private Sub OwnerMenuItem_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
    Invalidate()
  End Sub
End Class