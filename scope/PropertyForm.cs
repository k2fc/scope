using System;
using System.IO;
using System.Windows.Forms;

namespace scope
{
    public partial class PropertyForm : Form
    {
        bool settingschanged => isChanged(oldhash);
        string directory = Directory.GetCurrentDirectory();
        bool loaded => propertyGrid1.SelectedObject != null;
        string oldhash;
        string filename;
        int Xmargin;
        int Ymargin;
        RadarWindow window;
        public PropertyForm(RadarWindow window)
        {
            this.window = window;
            InitializeComponent();
            Xmargin = Width - propertyGrid1.Width;
            Ymargin = Height - propertyGrid1.Height;
        }
        private void FormResize(object sender, EventArgs e)
        {
            propertyGrid1.Width = Width - Xmargin;
            propertyGrid1.Height = Height - Ymargin;
        }
        private void FormClose(object sender, FormClosingEventArgs e)
        {
        }

        private void PropertyForm_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = window;
        }

        private void OpenFile(string filename)
        {
            try
            {
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading " + filename + "\r\n" + ex.Message, "Error reading", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SaveFile()
        {
            if (filename == null)
                SaveFileAs();
            else
            {
                try
                {
                    //GlobalSettings.Instance.WriteSettingsToFile(filename);
                    //oldhash = GlobalSettings.Instance.GetXml();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error writing " + filename + "\r\n" + ex.Message, "Error saving", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            //timer1.Stop();
        }
        private void SaveFileAs()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = directory;
                saveFileDialog.Filter = "XML files (*.xml)|*.xml|All files|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.DefaultExt = "xml";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filename = saveFileDialog.FileName;
                    SaveFile();
                }
            }
        }
        private bool isChanged(string oldhash)
        {
            if (!loaded)
                return false;
            return false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (settingschanged)
            {
                var result = MessageBox.Show("You have unsaved changes. Do you want to save?", "Unsaved changes", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                    SaveFile();
                else if (result == DialogResult.Cancel)
                    return;
            }
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = directory;
                openFileDialog.Filter = "XML files (*.xml)|*.xml|All files|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    OpenFile(openFileDialog.FileName);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileAs();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            string str = "";
            if (isChanged(oldhash))
                str = "* ";
            if (filename != null)
                str += filename + " - ";
            else if (loaded)
                str += "(New file) - ";
            str += "Radar Options";
            Text = str;
        }

        

    }

}
