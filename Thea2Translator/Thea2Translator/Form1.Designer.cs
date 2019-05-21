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
            this.checkBoxDataBaseStep1 = new System.Windows.Forms.CheckBox();
            this.checkBoxDataBaseStep2 = new System.Windows.Forms.CheckBox();
            this.checkBoxModulesStep2 = new System.Windows.Forms.CheckBox();
            this.checkBoxModulesStep1 = new System.Windows.Forms.CheckBox();
            this.checkBoxModulesStep3 = new System.Windows.Forms.CheckBox();
            this.checkBoxDataBaseStep3 = new System.Windows.Forms.CheckBox();
            this.bttnStep3 = new System.Windows.Forms.Button();
            this.checkBoxModulesStep4 = new System.Windows.Forms.CheckBox();
            this.checkBoxDataBaseStep4 = new System.Windows.Forms.CheckBox();
            this.bttnStep4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bttnStep1
            // 
            this.bttnStep1.Location = new System.Drawing.Point(181, 38);
            this.bttnStep1.Name = "bttnStep1";
            this.bttnStep1.Size = new System.Drawing.Size(159, 23);
            this.bttnStep1.TabIndex = 0;
            this.bttnStep1.Text = "ImportFromSteam";
            this.bttnStep1.UseVisualStyleBackColor = true;
            this.bttnStep1.Click += new System.EventHandler(this.bttnStart_Click);
            // 
            // bttnStep2
            // 
            this.bttnStep2.Location = new System.Drawing.Point(181, 67);
            this.bttnStep2.Name = "bttnStep2";
            this.bttnStep2.Size = new System.Drawing.Size(159, 23);
            this.bttnStep2.TabIndex = 1;
            this.bttnStep2.Text = "PrepareToMachineTranslate";
            this.bttnStep2.UseVisualStyleBackColor = true;
            this.bttnStep2.Click += new System.EventHandler(this.bttnStart2_Click);
            // 
            // textBoxDir
            // 
            this.textBoxDir.Location = new System.Drawing.Point(41, 12);
            this.textBoxDir.Name = "textBoxDir";
            this.textBoxDir.Size = new System.Drawing.Size(265, 20);
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
            this.buttonDir.Location = new System.Drawing.Point(312, 10);
            this.buttonDir.Name = "buttonDir";
            this.buttonDir.Size = new System.Drawing.Size(28, 23);
            this.buttonDir.TabIndex = 8;
            this.buttonDir.Text = "...";
            this.buttonDir.UseVisualStyleBackColor = true;
            this.buttonDir.Click += new System.EventHandler(this.buttonDir_Click);
            // 
            // checkBoxDataBaseStep1
            // 
            this.checkBoxDataBaseStep1.AutoSize = true;
            this.checkBoxDataBaseStep1.Location = new System.Drawing.Point(12, 42);
            this.checkBoxDataBaseStep1.Name = "checkBoxDataBaseStep1";
            this.checkBoxDataBaseStep1.Size = new System.Drawing.Size(73, 17);
            this.checkBoxDataBaseStep1.TabIndex = 9;
            this.checkBoxDataBaseStep1.Text = "DataBase";
            this.checkBoxDataBaseStep1.UseVisualStyleBackColor = true;
            // 
            // checkBoxDataBaseStep2
            // 
            this.checkBoxDataBaseStep2.AutoSize = true;
            this.checkBoxDataBaseStep2.Location = new System.Drawing.Point(12, 71);
            this.checkBoxDataBaseStep2.Name = "checkBoxDataBaseStep2";
            this.checkBoxDataBaseStep2.Size = new System.Drawing.Size(73, 17);
            this.checkBoxDataBaseStep2.TabIndex = 10;
            this.checkBoxDataBaseStep2.Text = "DataBase";
            this.checkBoxDataBaseStep2.UseVisualStyleBackColor = true;
            // 
            // checkBoxModulesStep2
            // 
            this.checkBoxModulesStep2.AutoSize = true;
            this.checkBoxModulesStep2.Location = new System.Drawing.Point(91, 71);
            this.checkBoxModulesStep2.Name = "checkBoxModulesStep2";
            this.checkBoxModulesStep2.Size = new System.Drawing.Size(66, 17);
            this.checkBoxModulesStep2.TabIndex = 11;
            this.checkBoxModulesStep2.Text = "Modules";
            this.checkBoxModulesStep2.UseVisualStyleBackColor = true;
            // 
            // checkBoxModulesStep1
            // 
            this.checkBoxModulesStep1.AutoSize = true;
            this.checkBoxModulesStep1.Location = new System.Drawing.Point(91, 42);
            this.checkBoxModulesStep1.Name = "checkBoxModulesStep1";
            this.checkBoxModulesStep1.Size = new System.Drawing.Size(66, 17);
            this.checkBoxModulesStep1.TabIndex = 12;
            this.checkBoxModulesStep1.Text = "Modules";
            this.checkBoxModulesStep1.UseVisualStyleBackColor = true;
            // 
            // checkBoxModulesStep3
            // 
            this.checkBoxModulesStep3.AutoSize = true;
            this.checkBoxModulesStep3.Location = new System.Drawing.Point(91, 100);
            this.checkBoxModulesStep3.Name = "checkBoxModulesStep3";
            this.checkBoxModulesStep3.Size = new System.Drawing.Size(66, 17);
            this.checkBoxModulesStep3.TabIndex = 15;
            this.checkBoxModulesStep3.Text = "Modules";
            this.checkBoxModulesStep3.UseVisualStyleBackColor = true;
            // 
            // checkBoxDataBaseStep3
            // 
            this.checkBoxDataBaseStep3.AutoSize = true;
            this.checkBoxDataBaseStep3.Location = new System.Drawing.Point(12, 100);
            this.checkBoxDataBaseStep3.Name = "checkBoxDataBaseStep3";
            this.checkBoxDataBaseStep3.Size = new System.Drawing.Size(73, 17);
            this.checkBoxDataBaseStep3.TabIndex = 14;
            this.checkBoxDataBaseStep3.Text = "DataBase";
            this.checkBoxDataBaseStep3.UseVisualStyleBackColor = true;
            // 
            // bttnStep3
            // 
            this.bttnStep3.Location = new System.Drawing.Point(181, 96);
            this.bttnStep3.Name = "bttnStep3";
            this.bttnStep3.Size = new System.Drawing.Size(159, 23);
            this.bttnStep3.TabIndex = 13;
            this.bttnStep3.Text = "ImportFromMachineTranslate";
            this.bttnStep3.UseVisualStyleBackColor = true;
            this.bttnStep3.Click += new System.EventHandler(this.bttnStep3_Click);
            // 
            // checkBoxModulesStep4
            // 
            this.checkBoxModulesStep4.AutoSize = true;
            this.checkBoxModulesStep4.Location = new System.Drawing.Point(91, 129);
            this.checkBoxModulesStep4.Name = "checkBoxModulesStep4";
            this.checkBoxModulesStep4.Size = new System.Drawing.Size(66, 17);
            this.checkBoxModulesStep4.TabIndex = 18;
            this.checkBoxModulesStep4.Text = "Modules";
            this.checkBoxModulesStep4.UseVisualStyleBackColor = true;
            // 
            // checkBoxDataBaseStep4
            // 
            this.checkBoxDataBaseStep4.AutoSize = true;
            this.checkBoxDataBaseStep4.Location = new System.Drawing.Point(12, 129);
            this.checkBoxDataBaseStep4.Name = "checkBoxDataBaseStep4";
            this.checkBoxDataBaseStep4.Size = new System.Drawing.Size(73, 17);
            this.checkBoxDataBaseStep4.TabIndex = 17;
            this.checkBoxDataBaseStep4.Text = "DataBase";
            this.checkBoxDataBaseStep4.UseVisualStyleBackColor = true;
            // 
            // bttnStep4
            // 
            this.bttnStep4.Location = new System.Drawing.Point(181, 125);
            this.bttnStep4.Name = "bttnStep4";
            this.bttnStep4.Size = new System.Drawing.Size(159, 23);
            this.bttnStep4.TabIndex = 16;
            this.bttnStep4.Text = "ExportToSteam";
            this.bttnStep4.UseVisualStyleBackColor = true;
            this.bttnStep4.Click += new System.EventHandler(this.bttnStep4_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 161);
            this.Controls.Add(this.checkBoxModulesStep4);
            this.Controls.Add(this.checkBoxDataBaseStep4);
            this.Controls.Add(this.bttnStep4);
            this.Controls.Add(this.checkBoxModulesStep3);
            this.Controls.Add(this.checkBoxDataBaseStep3);
            this.Controls.Add(this.bttnStep3);
            this.Controls.Add(this.checkBoxModulesStep1);
            this.Controls.Add(this.checkBoxModulesStep2);
            this.Controls.Add(this.checkBoxDataBaseStep2);
            this.Controls.Add(this.checkBoxDataBaseStep1);
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
        private System.Windows.Forms.CheckBox checkBoxDataBaseStep1;
        private System.Windows.Forms.CheckBox checkBoxDataBaseStep2;
        private System.Windows.Forms.CheckBox checkBoxModulesStep2;
        private System.Windows.Forms.CheckBox checkBoxModulesStep1;
        private System.Windows.Forms.CheckBox checkBoxModulesStep3;
        private System.Windows.Forms.CheckBox checkBoxDataBaseStep3;
        private System.Windows.Forms.Button bttnStep3;
        private System.Windows.Forms.CheckBox checkBoxModulesStep4;
        private System.Windows.Forms.CheckBox checkBoxDataBaseStep4;
        private System.Windows.Forms.Button bttnStep4;
    }
}

