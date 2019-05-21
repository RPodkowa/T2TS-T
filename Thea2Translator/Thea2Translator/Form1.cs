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

            checkBoxDataBaseStep3.Checked = true;
            checkBoxModulesStep3.Checked = true;

            checkBoxDataBaseStep4.Checked = true;
            checkBoxModulesStep4.Checked = true;
        }
                
        private void bttnStart_Click(object sender, EventArgs e)
        {
            FileHelper.MainDir = textBoxDir.Text;
            if (checkBoxDataBaseStep1.Checked) ProcessFiles(FilesType.DataBase, AlgorithmStep.ImportFromSteam);
            if (checkBoxModulesStep1.Checked) ProcessFiles(FilesType.Modules, AlgorithmStep.ImportFromSteam);
        }
        private void bttnStart2_Click(object sender, EventArgs e)
        {
            FileHelper.MainDir = textBoxDir.Text;
            if (checkBoxDataBaseStep2.Checked) ProcessFiles(FilesType.DataBase, AlgorithmStep.PrepareToMachineTranslate);
            if (checkBoxModulesStep2.Checked) ProcessFiles(FilesType.Modules, AlgorithmStep.PrepareToMachineTranslate);
        }

        private void bttnStep3_Click(object sender, EventArgs e)
        {
            FileHelper.MainDir = textBoxDir.Text;
            if (checkBoxDataBaseStep2.Checked) ProcessFiles(FilesType.DataBase, AlgorithmStep.ImportFromMachineTranslate);
            if (checkBoxModulesStep2.Checked) ProcessFiles(FilesType.Modules, AlgorithmStep.ImportFromMachineTranslate);
        }

        private void bttnStep4_Click(object sender, EventArgs e)
        {
            FileHelper.MainDir = textBoxDir.Text;
            if (checkBoxDataBaseStep2.Checked) ProcessFiles(FilesType.DataBase, AlgorithmStep.ExportToSteam);
            if (checkBoxModulesStep2.Checked) ProcessFiles(FilesType.Modules, AlgorithmStep.ExportToSteam);
        }

        private void ProcessFiles(FilesType filesType, AlgorithmStep step)
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
