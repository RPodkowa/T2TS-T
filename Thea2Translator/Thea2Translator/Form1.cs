using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Thea2Translator.Cache;
using Thea2Translator.Helpers;

namespace Thea2Translator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBoxDir.Text = @"C:\Program Files (x86)\Steam\steamapps\common\Thea 2 The Shattering\Thea2_Data\StreamingAssets";
            checkBoxDataBaseStep1.Checked = true;
            checkBoxModulesStep1.Checked = true;

            checkBoxDataBaseStep2.Checked = true;
            checkBoxModulesStep2.Checked = true;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            FileHelper.MainDir = textBoxDir.Text;
            if (checkBoxDataBaseStep1.Checked) ProcessFiles(FilesType.DataBase, 1);
            if (checkBoxModulesStep1.Checked) ProcessFiles(FilesType.Modules, 1);
        }
        private void bttnStart2_Click(object sender, EventArgs e)
        {
            FileHelper.MainDir = textBoxDir.Text;
            if (checkBoxDataBaseStep2.Checked) ProcessFiles(FilesType.DataBase, 2);
            if (checkBoxModulesStep2.Checked) ProcessFiles(FilesType.Modules, 2);
        }

        private void ProcessFiles(FilesType filesType, int step)
        {
            var cache = new DataCache(filesType);
            cache.MakeStep(step);
        }
        
        private void buttonDir_Click(object sender, EventArgs e)
        {
            searchFolder(textBoxDir, "Dir");
        }

        private void searchFolder(TextBox box, string description)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(box.Text))
                fbd.SelectedPath = box.Text;

            fbd.Description = description;
            if (fbd.ShowDialog() == DialogResult.OK)
                box.Text = fbd.SelectedPath;
        }
    }
}
