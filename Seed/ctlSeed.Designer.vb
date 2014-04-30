<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlSeed
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
    Me.imgLogo = New System.Windows.Forms.PictureBox()
    Me.tmrJustBefore = New System.Windows.Forms.Timer(Me.components)
    CType(Me.imgLogo, System.ComponentModel.ISupportInitialize).BeginInit()
    Me.SuspendLayout()
    '
    'imgLogo
    '
    Me.imgLogo.Image = Global.Seed.My.Resources.Resources.Logo
    Me.imgLogo.Location = New System.Drawing.Point(0, 0)
    Me.imgLogo.Name = "imgLogo"
    Me.imgLogo.Size = New System.Drawing.Size(146, 112)
    Me.imgLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
    Me.imgLogo.TabIndex = 0
    Me.imgLogo.TabStop = False
    '
    'tmrJustBefore
    '
    Me.tmrJustBefore.Interval = 1
    '
    'ctlSeed
    '
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
    Me.BackColor = System.Drawing.Color.Black
    Me.Controls.Add(Me.imgLogo)
    Me.Name = "ctlSeed"
    CType(Me.imgLogo, System.ComponentModel.ISupportInitialize).EndInit()
    Me.ResumeLayout(False)

  End Sub
  Friend WithEvents imgLogo As System.Windows.Forms.PictureBox
  Friend WithEvents tmrJustBefore As System.Windows.Forms.Timer
End Class

