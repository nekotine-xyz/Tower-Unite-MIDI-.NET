using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TowerUniteMidiDotNet
{
    public partial class CreditsForm : Form
    {
        public CreditsForm()
        {
            InitializeComponent();
            InitializeCreditsLayout();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreditsForm));
            this.SuspendLayout();
            // 
            // CreditsForm
            // 
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CreditsForm";
            this.Text = "Credits";
            this.ResumeLayout(false);

        }

        private void InitializeCreditsLayout()
        {
            try
            {
                // Set the form's properties
                this.StartPosition = FormStartPosition.CenterParent;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;

                // Create a label for the title
                Label titleLabel = new Label
                {
                    Text = "Credits",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    Size = new Size(this.ClientSize.Width, 30),
                    Location = new Point(0, 10)
                };
                this.Controls.Add(titleLabel);

                // Create a label for the creators
                Label creatorsLabel = new Label
                {
                    Text = "Original Creator: Bailey Eaton (Yoshify)\nUpdater and Lead Programmer: Nadya (Xela)",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12),
                    Size = new Size(this.ClientSize.Width, 60),
                    Location = new Point(0, 50)
                };
                this.Controls.Add(creatorsLabel);

                // Create a link label for contact
                LinkLabel contactLinkLabel = new LinkLabel
                {
                    Text = "Contact Me",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12),
                    Size = new Size(this.ClientSize.Width, 20),
                    Location = new Point(0, 120)
                };
                contactLinkLabel.Links.Add(0, contactLinkLabel.Text.Length, "https://twitter.com/XelaPilled");
                contactLinkLabel.LinkClicked += (sender, args) => Process.Start(args.Link.LinkData as string);
                this.Controls.Add(contactLinkLabel);

                // Create a label for the license
                Label licenseLabel = new Label
                {
                    Text = "MIT License\nCopyright (c) 2019 Bailey Eaton",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10),
                    Size = new Size(this.ClientSize.Width, 40),
                    Location = new Point(0, 150)
                };
                this.Controls.Add(licenseLabel);
                licenseLabel.Visible = true;
                licenseLabel.BringToFront();

                // Create a button for closing the form
                Button okButton = new Button
                {
                    Text = "OK",
                    Size = new Size(75, 30),
                    Location = new Point(this.ClientSize.Width / 2 - 75 / 2, this.ClientSize.Height - 40)
                };
                okButton.Click += (sender, args) => this.Close();
                this.Controls.Add(okButton);

                // Set visibility
                titleLabel.Visible = true;
                creatorsLabel.Visible = true;
                contactLinkLabel.Visible = true;
                okButton.Visible = true;

                // Bring controls to front if necessary
                titleLabel.BringToFront();
                creatorsLabel.BringToFront();
                contactLinkLabel.BringToFront();
                okButton.BringToFront();

                // Refresh the form to redraw the controls
                this.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}