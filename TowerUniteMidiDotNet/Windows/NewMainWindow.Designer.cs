namespace TowerUniteMidiDotNet.Windows
{
    partial class NewMainWindow
    {
        private System.Windows.Forms.GroupBox InputFrame;
        private System.Windows.Forms.ComboBox InputPorts;
        private System.Windows.Forms.Label InputLabel;
        private System.Windows.Forms.Button InputButton;
        private System.Windows.Forms.TrackBar velocityThresholdTrack;
        private System.Windows.Forms.Label velocityThresholdLabel;
        private System.Windows.Forms.Button tuneMinusButton;
        private System.Windows.Forms.Button tunePlusButton;
        private System.Windows.Forms.TrackBar tuneTrack;
        private System.Windows.Forms.CheckBox transposeCheckBox;
        private System.Windows.Forms.CheckBox muteCheckBox;
        private System.Windows.Forms.CheckBox stayOnTopCheckBox;
        // ... (other controls)

        private void InitializeComponent()
        {
            this.InputFrame = new System.Windows.Forms.GroupBox();
            this.InputPorts = new System.Windows.Forms.ComboBox();
            this.InputLabel = new System.Windows.Forms.Label();
            this.InputButton = new System.Windows.Forms.Button();
            this.EventListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.velocityThresholdTrack = new System.Windows.Forms.TrackBar();
            this.velocityThresholdLabel = new System.Windows.Forms.Label();
            this.tuneMinusButton = new System.Windows.Forms.Button();
            this.tunePlusButton = new System.Windows.Forms.Button();
            this.tuneTrack = new System.Windows.Forms.TrackBar();
            this.transposeCheckBox = new System.Windows.Forms.CheckBox();
            this.muteCheckBox = new System.Windows.Forms.CheckBox();
            this.stayOnTopCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.InputFrame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.velocityThresholdTrack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tuneTrack)).BeginInit();
            this.SuspendLayout();
            // 
            // InputFrame
            // 
            this.InputFrame.Controls.Add(this.InputPorts);
            this.InputFrame.Controls.Add(this.InputLabel);
            this.InputFrame.Controls.Add(this.InputButton);
            this.InputFrame.Location = new System.Drawing.Point(394, 79);
            this.InputFrame.Name = "InputFrame";
            this.InputFrame.Size = new System.Drawing.Size(394, 100);
            this.InputFrame.TabIndex = 0;
            this.InputFrame.TabStop = false;
            this.InputFrame.Text = "Input Devices";
            // 
            // InputPorts
            // 
            this.InputPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.InputPorts.Location = new System.Drawing.Point(80, 40);
            this.InputPorts.Name = "InputPorts";
            this.InputPorts.Size = new System.Drawing.Size(188, 28);
            this.InputPorts.TabIndex = 0;
            this.InputPorts.SelectedIndexChanged += new System.EventHandler(this.InputPortsChange);
            // 
            // InputLabel
            // 
            this.InputLabel.Location = new System.Drawing.Point(20, 40);
            this.InputLabel.Name = "InputLabel";
            this.InputLabel.Size = new System.Drawing.Size(60, 23);
            this.InputLabel.TabIndex = 1;
            this.InputLabel.Text = "Device:";
            // 
            // InputButton
            // 
            this.InputButton.Location = new System.Drawing.Point(280, 40);
            this.InputButton.Name = "InputButton";
            this.InputButton.Size = new System.Drawing.Size(80, 28);
            this.InputButton.TabIndex = 2;
            this.InputButton.Text = "Refresh";
            this.InputButton.Click += new System.EventHandler(this.InputButtonClick);
            // 
            // EventListView
            // 
            this.EventListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.EventListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.EventListView.HideSelection = false;
            this.EventListView.Location = new System.Drawing.Point(310, 351);
            this.EventListView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.EventListView.Name = "EventListView";
            this.EventListView.Size = new System.Drawing.Size(463, 232);
            this.EventListView.TabIndex = 2;
            this.EventListView.UseCompatibleStateImageBehavior = false;
            this.EventListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            this.columnHeader1.Width = 300;
            // 
            // velocityThresholdTrack
            // 
            this.velocityThresholdTrack.Location = new System.Drawing.Point(160, 208);
            this.velocityThresholdTrack.Name = "velocityThresholdTrack";
            this.velocityThresholdTrack.Size = new System.Drawing.Size(278, 69);
            this.velocityThresholdTrack.TabIndex = 0;
            // 
            // velocityThresholdLabel
            // 
            this.velocityThresholdLabel.Location = new System.Drawing.Point(16, 182);
            this.velocityThresholdLabel.Name = "velocityThresholdLabel";
            this.velocityThresholdLabel.Size = new System.Drawing.Size(100, 23);
            this.velocityThresholdLabel.TabIndex = 1;
            // 
            // tuneMinusButton
            // 
            this.tuneMinusButton.Location = new System.Drawing.Point(56, 292);
            this.tuneMinusButton.Name = "tuneMinusButton";
            this.tuneMinusButton.Size = new System.Drawing.Size(75, 28);
            this.tuneMinusButton.TabIndex = 2;
            // 
            // tunePlusButton
            // 
            this.tunePlusButton.Location = new System.Drawing.Point(12, 79);
            this.tunePlusButton.Name = "tunePlusButton";
            this.tunePlusButton.Size = new System.Drawing.Size(75, 23);
            this.tunePlusButton.TabIndex = 3;
            // 
            // tuneTrack
            // 
            this.tuneTrack.Location = new System.Drawing.Point(118, 94);
            this.tuneTrack.Name = "tuneTrack";
            this.tuneTrack.Size = new System.Drawing.Size(203, 69);
            this.tuneTrack.TabIndex = 4;
            // 
            // transposeCheckBox
            // 
            this.transposeCheckBox.Location = new System.Drawing.Point(12, 239);
            this.transposeCheckBox.Name = "transposeCheckBox";
            this.transposeCheckBox.Size = new System.Drawing.Size(104, 24);
            this.transposeCheckBox.TabIndex = 5;
            // 
            // muteCheckBox
            // 
            this.muteCheckBox.Location = new System.Drawing.Point(237, 292);
            this.muteCheckBox.Name = "muteCheckBox";
            this.muteCheckBox.Size = new System.Drawing.Size(104, 24);
            this.muteCheckBox.TabIndex = 6;
            // 
            // stayOnTopCheckBox
            // 
            this.stayOnTopCheckBox.Location = new System.Drawing.Point(141, 395);
            this.stayOnTopCheckBox.Name = "stayOnTopCheckBox";
            this.stayOnTopCheckBox.Size = new System.Drawing.Size(104, 24);
            this.stayOnTopCheckBox.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(306, 326);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Event Log";
            // 
            // NewMainWindow
            // 
            this.ClientSize = new System.Drawing.Size(800, 597);
            this.Controls.Add(this.velocityThresholdTrack);
            this.Controls.Add(this.velocityThresholdLabel);
            this.Controls.Add(this.tuneMinusButton);
            this.Controls.Add(this.tunePlusButton);
            this.Controls.Add(this.tuneTrack);
            this.Controls.Add(this.transposeCheckBox);
            this.Controls.Add(this.muteCheckBox);
            this.Controls.Add(this.stayOnTopCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.EventListView);
            this.Controls.Add(this.InputFrame);
            this.Name = "NewMainWindow";
            this.Text = "Tower Unite MIDI .NET";
            this.InputFrame.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.velocityThresholdTrack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tuneTrack)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ListView EventListView;
        public System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label1;
    }

}