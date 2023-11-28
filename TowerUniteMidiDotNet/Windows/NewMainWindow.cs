using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TowerUniteMidiDotNet.Windows
{
    public partial class NewMainWindow : Form
    {
        public NewMainWindow()
        {
            InitializeComponent();
        }

        private void InputPortsChange(object sender, EventArgs e)
        {
            // Handle input port change event
        }

        private void InputButtonClick(object sender, EventArgs e)
        {
            // Handle input button click event
        }

        // Event handler for the velocity threshold trackbar
        private void VelocityThresholdTrack_ValueChanged(object sender, EventArgs e)
        {
            // Update your velocity threshold logic here
        }

        // Event handlers for tuning buttons
        private void TuneMinusButton_Click(object sender, EventArgs e)
        {
            // Decrease tuning
        }

        private void TunePlusButton_Click(object sender, EventArgs e)
        {
            // Increase tuning
        }

        // Event handler for the transpose checkbox
        private void TransposeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Enable/disable transposing out-of-range notes
        }
    }

}
