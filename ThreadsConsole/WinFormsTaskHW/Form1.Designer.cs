namespace WinFormsTaskHW
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtNumber = new System.Windows.Forms.TextBox();
            this.lbCounter = new System.Windows.Forms.Label();
            this.btnPerform = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // txtNumber
            // 
            this.txtNumber.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtNumber.Location = new System.Drawing.Point(72, 19);
            this.txtNumber.Multiline = true;
            this.txtNumber.Name = "txtNumber";
            this.txtNumber.Size = new System.Drawing.Size(506, 47);
            this.txtNumber.TabIndex = 5;
            // 
            // lbCounter
            // 
            this.lbCounter.AutoSize = true;
            this.lbCounter.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbCounter.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lbCounter.Location = new System.Drawing.Point(28, 403);
            this.lbCounter.Name = "lbCounter";
            this.lbCounter.Size = new System.Drawing.Size(42, 28);
            this.lbCounter.TabIndex = 4;
            this.lbCounter.Text = "0/0";
            // 
            // btnPerform
            // 
            this.btnPerform.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnPerform.Location = new System.Drawing.Point(658, 19);
            this.btnPerform.Name = "btnPerform";
            this.btnPerform.Size = new System.Drawing.Size(115, 47);
            this.btnPerform.TabIndex = 3;
            this.btnPerform.Text = "Perform";
            this.btnPerform.UseVisualStyleBackColor = true;
            this.btnPerform.Click += new System.EventHandler(this.btnPerform_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(28, 361);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(745, 39);
            this.progressBar.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.txtNumber);
            this.Controls.Add(this.lbCounter);
            this.Controls.Add(this.btnPerform);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox txtNumber;
        private Label lbCounter;
        private Button btnPerform;
        private ProgressBar progressBar;
    }
}