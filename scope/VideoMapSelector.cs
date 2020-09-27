using System;
using System.Collections.Generic;
using System.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.Design;

namespace DGScope
{
    
    public partial class VideoMapSelector : Form
    {
        private List<VideoMap> videoMaps;
        
        public VideoMapSelector(List<VideoMap> videoMaps)
        {
            InitializeComponent();
            this.videoMaps = videoMaps;
            LoadListBox();
        }

        private void CheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            videoMaps[e.Index].Visible = e.NewValue == CheckState.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "VRC Sector Files (*.sct2)|*.sct2|VSTARS Video Maps (*.xml)|*.xml";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    switch (openFileDialog.FilterIndex)
                    {
                        case 1:
                            videoMaps.AddRange(VRCFileParser.GetMapsFromFile(filePath));
                            break;
                        case 2:
                            videoMaps.AddRange(VSTARSFileParser.GetMapsFromFile(filePath));
                            break;
                    }
                }
            }
            LoadListBox();
        }

        private void LoadListBox()
        {
            checkedListBox1.Items.Clear();
            foreach (VideoMap item in videoMaps)
                checkedListBox1.Items.Add(item, item.Visible);
            checkedListBox1.ItemCheck += CheckedListBox1_ItemCheck;
        }

        private void VideoMapSelector_Load(object sender, EventArgs e)
        {
            button5.Text = char.ConvertFromUtf32(0x2193);
            button4.Text = char.ConvertFromUtf32(0x2191);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            VideoMap selected = (VideoMap)checkedListBox1.SelectedItem;
            EditVideoMap(selected);
            LoadListBox();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            VideoMap selected = (VideoMap)checkedListBox1.SelectedItem;
            RenameVideoMap(selected);
            LoadListBox();
        }

        private void RenameVideoMap(VideoMap map)
        {
            string name = map.Name;
            if (Input.InputBox("Name", "Enter a name for the video map:", ref name) == DialogResult.OK)
                map.Name = name;
            LoadListBox();
        }
        private void EditVideoMap(VideoMap map)
        {
            PropertyDescriptor pd = TypeDescriptor.GetProperties(map)["Lines"];
            UITypeEditor editor = (UITypeEditor)pd.GetEditor(typeof(UITypeEditor));
            RuntimeServiceProvider serviceProvider = new RuntimeServiceProvider();
            editor.EditValue(serviceProvider, serviceProvider, map.Lines);
            LoadListBox();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            videoMaps.Remove((VideoMap)checkedListBox1.SelectedItem);
            LoadListBox();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            VideoMap newMap = new VideoMap();
            RenameVideoMap(newMap);
            EditVideoMap(newMap);
            videoMaps.Add(newMap);
            LoadListBox();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex >0)
                MoveMap(checkedListBox1.SelectedIndex, checkedListBox1.SelectedIndex - 1);
        }

        private void MoveMap(int oldIndex, int newIndex)
        {
            var item = videoMaps[oldIndex];

            videoMaps.RemoveAt(oldIndex);

            //if (newIndex > oldIndex) newIndex--;
            // the actual index could have shifted due to the removal

            videoMaps.Insert(newIndex, item);
            LoadListBox();
            checkedListBox1.SelectedIndex = newIndex;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex < checkedListBox1.Items.Count  - 1)
                MoveMap(checkedListBox1.SelectedIndex, checkedListBox1.SelectedIndex + 1);
        }
    }
    public class VideoMapCollectionEditor : UITypeEditor
    {
        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            // Return the value if the value is not of type Int32, Double and Single.
            if (value.GetType() != typeof(List<VideoMap>))
                return value;

            // Uses the IWindowsFormsEditorService to display a
            // drop-down UI in the Properties window.
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc != null)
            {
                VideoMapSelector sel = new VideoMapSelector((List<VideoMap>)value);
                sel.ShowDialog();
            }
            return value;
        }
    }
}
