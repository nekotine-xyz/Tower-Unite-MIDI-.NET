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

        // this method updates the KeyPressDuration based on the provided FPS
        public static void UpdateKeyPressDurationFromFPS(int fps)
        {
            // assuming 60 FPS is the baseline for no additional delay
            int baseDelay = MainWindow.KeyDelay;

            // calculate the scaling factor based on the FPS. 
            // the formula here is a placeholder, testing needed
            // if FPS is 30, the delay might be double the base delay.
            int scalingFactor = 60 / Math.Max(fps, 1); // avoid division by zero

            // apply the scaling factor
            int newDelay = baseDelay * scalingFactor;

            // set a minimum and maximum bounds for delay
            int minDelay = 8; // minimum delay in ms
            int maxDelay = 100; // maximum delay in ms, this value is arbitrary and should be adjusted based on testing

            // clamp the new delay between the min and max bounds
            Note.KeyPressDuration = Clamp(newDelay, minDelay, maxDelay);
        }

        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}