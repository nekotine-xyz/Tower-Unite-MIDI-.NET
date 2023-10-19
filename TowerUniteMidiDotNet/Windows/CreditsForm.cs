using System.Windows.Forms;

namespace TowerUniteMidiDotNet
{
    public class CreditsForm : Form
    {
        private Label label1;

        public CreditsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreditsForm));
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(328, 255);
            this.label1.TabIndex = 0;
            this.label1.Text = "Original Developer: Yoshify Updated By: xelapilled";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.UseMnemonic = false;
            // 
            // CreditsForm
            // 
            this.ClientSize = new System.Drawing.Size(352, 273);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "CreditsForm";
            this.ShowInTaskbar = false;
            this.Text = "TUMDN 1.2";
            this.ResumeLayout(false);

        }
    }
}
