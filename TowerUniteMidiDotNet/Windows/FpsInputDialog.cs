using System;
using System.Windows.Forms;
using TowerUniteMidiDotNet.Windows;
using TowerUniteMidiDotNet.Core;
using System.Linq.Expressions;

namespace TowerUniteMidiDotNet
{
    public partial class FpsInputDialog : Form
    {
        public int Fps { get; private set; }
        public bool InputConfirmed { get; private set; }

        public FpsInputDialog()
        {
            InitializeComponent();

            this.AcceptButton = this.confirmButton;
            this.CancelButton = this.cancelButton;
            this.confirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            this.fpsTextBox.KeyPress += FpsTextBox_KeyPress;
        }

        private void FpsTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // allow only digits and control characters
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(fpsTextBox.Text, out int fps))
                {
                    if (fps < 20 || fps > 59)
                    {
                        MessageBox.Show("Please enter an FPS value between 20 and 59.", "Out of Range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        Fps = fps;
                        UpdateKeyPressDurationFromFPS(fps);
                        InputConfirmed = true;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid integer.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            InputConfirmed = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public static int CalculateKeyPressDuration(int fps)
        {
            int fps1 = 60;
            int keyDuration1 = 20;
            int fps2 = 30;
            int keyDuration2 = 40;

            // linear interpolation formula
            int interpolatedDuration = keyDuration1 + (keyDuration2 - keyDuration1) * (fps - fps1) / (fps2 - fps1);

            // clamp the value to a reasonable range if necessary
            int minDuration = 10; // minimum duration
            int maxDuration = 100; // maximum duration
            return Clamp(interpolatedDuration, minDuration, maxDuration);
        }

        public static int UpdateKeyPressDurationFromFPS(int fps)
        {
            int newKeyPressDuration = CalculateKeyPressDuration(fps);
            Note.KeyPressDuration = newKeyPressDuration;
            return newKeyPressDuration;
        }

        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}