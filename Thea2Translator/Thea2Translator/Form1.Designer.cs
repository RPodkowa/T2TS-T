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
            this.btnStep1 = new System.Windows.Forms.Button();
            this.bttnStep2 = new System.Windows.Forms.Button();
            this.textBoxDir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonDir = new System.Windows.Forms.Button();
            this.checkBoxDataBaseStep1 = new System.Windows.Forms.CheckBox();
            this.checkBoxDataBaseStep2 = new System.Windows.Forms.CheckBox();
            this.checkBoxModulesStep2 = new System.Windows.Forms.CheckBox();
            this.checkBoxModulesStep1 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnStep1
            // 
            this.btnStep1.Location = new System.Drawing.Point(181, 38);
            this.btnStep1.Name = "btnStep1";
            this.btnStep1.Size = new System.Drawing.Size(84, 23);
            this.btnStep1.TabIndex = 0;
            this.btnStep1.Text = "Step 1";
            this.btnStep1.UseVisualStyleBackColor = true;
            this.btnStep1.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // bttnStep2
            // 
            this.bttnStep2.Location = new System.Drawing.Point(181, 67);
            this.bttnStep2.Name = "bttnStep2";
            this.bttnStep2.Size = new System.Drawing.Size(84, 23);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(287, 100);
            this.Controls.Add(this.checkBoxModulesStep1);
            this.Controls.Add(this.checkBoxModulesStep2);
            this.Controls.Add(this.checkBoxDataBaseStep2);
            this.Controls.Add(this.checkBoxDataBaseStep1);
            this.Controls.Add(this.buttonDir);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxDir);
            this.Controls.Add(this.bttnStep2);
            this.Controls.Add(this.btnStep1);
            this.Name = "Form1";
            this.Text = "Translator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStep1;
        private System.Windows.Forms.Button bttnStep2;
        private System.Windows.Forms.TextBox textBoxDir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonDir;
        private System.Windows.Forms.CheckBox checkBoxDataBaseStep1;
        private System.Windows.Forms.CheckBox checkBoxDataBaseStep2;
        private System.Windows.Forms.CheckBox checkBoxModulesStep2;
        private System.Windows.Forms.CheckBox checkBoxModulesStep1;
    }
}

