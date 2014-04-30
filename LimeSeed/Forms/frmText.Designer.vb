<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmText
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
    Me.lblText = New System.Windows.Forms.Label()
    Me.tmrHide = New System.Windows.Forms.Timer(Me.components)
    Me.SuspendLayout()
    '
    'lblText
    '
    Me.lblText.AutoSize = True
    Me.lblText.BackColor = System.Drawing.Color.Transparent
    Me.lblText.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.lblText.ForeColor = System.Drawing.Color.Navy
    Me.lblText.Location = New System.Drawing.Point(0, 0)
    Me.lblText.Margin = New System.Windows.Forms.Padding(0)
    Me.lblText.MaximumSize = New System.Drawing.Size(300, 0)
    Me.lblText.Name = "lblText"
    Me.lblText.Padding = New System.Windows.Forms.Padding(3)
    Me.lblText.Size = New System.Drawing.Size(89, 21)
    Me.lblText.TabIndex = 0
    Me.lblText.Text = "[No Message]"
    Me.lblText.UseMnemonic = False
    '
    'tmrHide
    '
    '
    'frmText
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.BackColor = System.Drawing.SystemColors.Info
    Me.ClientSize = New System.Drawing.Size(154, 47)
    Me.ControlBox = False
    Me.Controls.Add(Me.lblText)
    Me.DoubleBuffered = True
    Me.ForeColor = System.Drawing.SystemColors.InfoText
    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
    Me.MaximizeBox = False
    Me.MinimizeBox = False
    Me.Name = "frmText"
    Me.ShowIcon = False
    Me.ShowInTaskbar = False
    Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
    Me.TopMost = True
    Me.TransparencyKey = System.Drawing.Color.Fuchsia
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents lblText As System.Windows.Forms.Label
  Friend WithEvents tmrHide As System.Windows.Forms.Timer
End Class
