<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmArtList
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
  <System.Diagnostics.DebuggerNonUserCode()> _
  Protected Overrides Sub Dispose(disposing As Boolean)
    Try
      If disposing AndAlso components IsNot Nothing Then
        components.Dispose()
      End If
    Finally
      MyBase.Dispose(disposing)
    End Try
  End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
    Me.components = New System.ComponentModel.Container()
    Me.tmrArtwork = New System.Windows.Forms.Timer(Me.components)
    Me.artList = New LimeSeed.ctlArtList()
    Me.SuspendLayout()
    '
    'tmrArtwork
    '
    Me.tmrArtwork.Interval = 500
    '
    'artList
    '
    Me.artList.Dock = System.Windows.Forms.DockStyle.Fill
    Me.artList.Location = New System.Drawing.Point(0, 0)
    Me.artList.Name = "artList"
    Me.artList.Size = New System.Drawing.Size(484, 266)
    Me.artList.TabIndex = 4
    '
    'frmArtList
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(484, 266)
    Me.Controls.Add(Me.artList)
    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
    Me.MaximizeBox = False
    Me.MinimizeBox = False
    Me.MinimumSize = New System.Drawing.Size(400, 250)
    Me.Name = "frmArtList"
    Me.ShowIcon = False
    Me.ShowInTaskbar = False
    Me.Text = "Artwork Selection"
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents tmrArtwork As System.Windows.Forms.Timer
  Friend WithEvents artList As LimeSeed.ctlArtList
End Class
