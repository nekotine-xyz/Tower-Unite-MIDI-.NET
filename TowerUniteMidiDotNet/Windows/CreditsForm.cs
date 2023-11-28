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
                // set the form's properties
                this.StartPosition = FormStartPosition.CenterParent;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;

                // label for the title
                Label titleLabel = new Label
                {
                    Text = "Credits",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    Size = new Size(this.ClientSize.Width, 30),
                    Location = new Point(0, 10)
                };
                this.Controls.Add(titleLabel);

                // label for the creators
                Label creatorsLabel = new Label
                {
                    Text = "Original Creator: Bailey Eaton (Yoshify)\nUpdater and Lead Programmer: Nadya (Xela)",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12),
                    Size = new Size(this.ClientSize.Width, 60),
                    Location = new Point(0, 50)
                };
                this.Controls.Add(creatorsLabel);

                // link label for contact
                LinkLabel contactLinkLabel = new LinkLabel
                {
                    Text = "Contact Me",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12),
                    Size = new Size(this.ClientSize.Width, 20),
                    Location = new Point(0, 110)
                };
                contactLinkLabel.Links.Add(0, contactLinkLabel.Text.Length, "https://twitter.com/XelaPilled");
                contactLinkLabel.LinkClicked += (sender, args) => Process.Start(args.Link.LinkData as string);
                this.Controls.Add(contactLinkLabel);

                // label for the support text
                Label supportLabel = new Label
                {
                    Text = "Want to support my work?",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12),
                    Size = new Size(this.ClientSize.Width, 20),
                    Location = new Point(0, 140)
                };
                this.Controls.Add(supportLabel);

                // link label for donations
                LinkLabel donateLinkLabel = new LinkLabel
                {
                    Text = "Donate",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12),
                    Size = new Size(this.ClientSize.Width, 20),
                    Location = new Point(0, 170)
                };
                donateLinkLabel.Links.Add(0, donateLinkLabel.Text.Length, "https://ko-fi.com/xelapilled");
                donateLinkLabel.LinkClicked += (sender, args) => Process.Start(args.Link.LinkData as string);
                this.Controls.Add(donateLinkLabel);

                // label for the license
                Label licenseLabel = new Label
                {
                    Text = "MIT License\nCopyright (c) 2019 Bailey Eaton",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10),
                    Size = new Size(this.ClientSize.Width, 40),
                    Location = new Point(0, 200)
                };
                this.Controls.Add(licenseLabel);
                licenseLabel.Visible = true;
                licenseLabel.BringToFront();

                // button for closing the form
                Button okButton = new Button
                {
                    Text = "OK",
                    Size = new Size(75, 30),
                    Location = new Point(this.ClientSize.Width / 2 - 75 / 2, this.ClientSize.Height - 40)
                };
                okButton.Click += (sender, args) => this.Close();
                this.Controls.Add(okButton);

                // set visibility
                titleLabel.Visible = true;
                creatorsLabel.Visible = true;
                contactLinkLabel.Visible = true;
                okButton.Visible = true;
                supportLabel.Visible = true;
                donateLinkLabel.Visible = true;

                // bring controls to front
                titleLabel.BringToFront();
                creatorsLabel.BringToFront();
                contactLinkLabel.BringToFront();
                okButton.BringToFront();
                supportLabel.BringToFront();
                donateLinkLabel.BringToFront();

                // refresh the form
                this.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}