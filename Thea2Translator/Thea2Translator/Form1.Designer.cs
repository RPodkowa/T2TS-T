namespace Thea2Translator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.bttnStep1 = new System.Windows.Forms.Button();
            this.bttnStep2 = new System.Windows.Forms.Button();
            this.textBoxDir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonDir = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bttnStep1
            // 
            this.bttnStep1.Location = new System.Drawing.Point(12, 38);
            this.bttnStep1.Name = "bttnStep1";
            this.bttnStep1.Size = new System.Drawing.Size(253, 23);
            this.bttnStep1.TabIndex = 0;
            this.bttnStep1.Text = "Step 1";
            this.bttnStep1.UseVisualStyleBackColor = true;
            this.bttnStep1.Click += new System.EventHandler(this.bttnStart_Click);
            // 
            // bttnStep2
            // 
            this.bttnStep2.Location = new System.Drawing.Point(12, 67);
            this.bttnStep2.Name = "bttnStep2";
            this.bttnStep2.Size = new System.Drawing.Size(253, 23);
            this.bttnStep2.TabIndex = 1;
            this.bttnStep2.Text = "Step 2";
            this.bttnStep2.UseVisualStyleBackColor = true;
            this.bttnStep2.Click += new System.EventHandler(this.bttnStart2_Click);
            // 
            // textBoxDir
            // 
            this.textBoxDir.Location = new System.Drawing.Point(41, 12);
            this.textBoxDir.Name = "textBoxDir";
            this.textBoxDir.Size = new System.Drawing.Size(190, 20);
            this.textBoxDir.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Dir:";
            // 
            // buttonDir
            // 
            this.buttonDir.Location = new System.Drawing.Point(237, 9);
            this.buttonDir.Name = "buttonDir";
            this.buttonDir.Size = new System.Drawing.Size(28, 23);
            this.buttonDir.TabIndex = 8;
            this.buttonDir.Text = "...";
            this.buttonDir.UseVisualStyleBackColor = true;
            this.buttonDir.Click += new System.EventHandler(this.buttonDir_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(277, 98);
            this.Controls.Add(this.buttonDir);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxDir);
            this.Controls.Add(this.bttnStep2);
            this.Controls.Add(this.bttnStep1);
            this.Name = "Form1";
            this.Text = "Translator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bttnStep1;
        private System.Windows.Forms.Button bttnStep2;
        private System.Windows.Forms.TextBox textBoxDir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonDir;
    }
}

