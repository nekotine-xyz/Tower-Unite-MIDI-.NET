namespace TowerUniteMidiDotNet
{
    partial class FpsInputDialog
    {
        private System.Windows.Forms.Label fpsInputLabel;
        private System.Windows.Forms.TextBox fpsTextBox;
        private System.Windows.Forms.Button confirmButton;
        private System.Windows.Forms.Button cancelButton;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FpsInputDialog));
            this.fpsInputLabel = new System.Windows.Forms.Label();
            this.fpsTextBox = new System.Windows.Forms.TextBox();
            this.confirmButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // fpsInputLabel
            // 
            this.fpsInputLabel.Location = new System.Drawing.Point(15, 15);
            this.fpsInputLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.fpsInputLabel.Name = "fpsInputLabel";
            this.fpsInputLabel.Size = new System.Drawing.Size(420, 31);
            this.fpsInputLabel.TabIndex = 0;
            this.fpsInputLabel.Text = "Enter your FPS (20-59):";
            // 
            // fpsTextBox
            // 
            this.fpsTextBox.Location = new System.Drawing.Point(15, 62);
            this.fpsTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.fpsTextBox.Name = "fpsTextBox";
            this.fpsTextBox.Size = new System.Drawing.Size(418, 26);
            this.fpsTextBox.TabIndex = 1;
            // 
            // confirmButton
            // 
            this.confirmButton.Location = new System.Drawing.Point(75, 108);
            this.confirmButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.confirmButton.Name = "confirmButton";
            this.confirmButton.Size = new System.Drawing.Size(120, 38);
            this.confirmButton.TabIndex = 2;
            this.confirmButton.Text = "Confirm";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(210, 108);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(120, 38);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Clear";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(428, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "NOTE: This feature is unfinished! May not work as intended.";
            // 
            // FpsInputDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 169);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.fpsInputLabel);
            this.Controls.Add(this.fpsTextBox);
            this.Controls.Add(this.confirmButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FpsInputDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FPS Adjust Tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label label1;
    }
}