namespace Pathfinding {
    partial class PathfindingForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.VelocityVal = new System.Windows.Forms.Label();
            this.TurnRateVal = new System.Windows.Forms.Label();
            this.StartSimButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(558, 624);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(576, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Velocity:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(576, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Turn Rate:";
            // 
            // VelocityVal
            // 
            this.VelocityVal.AutoSize = true;
            this.VelocityVal.Location = new System.Drawing.Point(641, 11);
            this.VelocityVal.Name = "VelocityVal";
            this.VelocityVal.Size = new System.Drawing.Size(24, 13);
            this.VelocityVal.TabIndex = 3;
            this.VelocityVal.Text = "n/a";
            this.VelocityVal.Click += new System.EventHandler(this.VelocityVal_Click);
            // 
            // TurnRateVal
            // 
            this.TurnRateVal.AutoSize = true;
            this.TurnRateVal.Location = new System.Drawing.Point(641, 25);
            this.TurnRateVal.Name = "TurnRateVal";
            this.TurnRateVal.Size = new System.Drawing.Size(24, 13);
            this.TurnRateVal.TabIndex = 4;
            this.TurnRateVal.Text = "n/a";
            // 
            // StartSimButton
            // 
            this.StartSimButton.Location = new System.Drawing.Point(576, 613);
            this.StartSimButton.Name = "StartSimButton";
            this.StartSimButton.Size = new System.Drawing.Size(113, 23);
            this.StartSimButton.TabIndex = 5;
            this.StartSimButton.Text = "Start Simulation";
            this.StartSimButton.UseVisualStyleBackColor = true;
            this.StartSimButton.Click += new System.EventHandler(this.StartSimButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 648);
            this.Controls.Add(this.StartSimButton);
            this.Controls.Add(this.TurnRateVal);
            this.Controls.Add(this.VelocityVal);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Airship Pathfinding Simulator";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label VelocityVal;
        private System.Windows.Forms.Label TurnRateVal;
        private System.Windows.Forms.Button StartSimButton;
    }
}

