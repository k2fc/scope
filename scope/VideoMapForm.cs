using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace DGScope
{
    public partial class VideoMapForm : Form
    {
        VideoMapList maps;
        BindingSource source = new BindingSource();
        RadarWindow adaptation;
        bool changed = false;

        private string _filename;
        public string Filename
        {
            get
            {
                if (adaptation != null)
                    return adaptation.VideoMapFilename;
                return null;
            }
            set
            {
                if (adaptation != null)
                    adaptation.VideoMapFilename = value;
            }
        }
        public VideoMapForm(VideoMapList list, string filename, RadarWindow adaptation = null)
        {
            InitializeComponent();
            maps = list;
            this.adaptation = adaptation;
            Filename = filename;
            FormClosing += VideoMapForm_FormClosing;
        }

        private void VideoMapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckChange())
                e.Cancel = true;
        }

        private void VideoMapForm_Load(object sender, EventArgs e)
        {
            source.DataSource = new BindingList<VideoMap>(maps);
            dataGridView1.DataSource = source;
            dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            dataGridView1.AutoResizeColumns();
        }


        private void fromVRCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "VRC Sector Files (*.sct2)|*.sct2|All Files|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    switch (openFileDialog.FilterIndex)
                    {
                        case 1:
                            maps.AddRange(VRCFileParser.GetMapsFromFile(filePath));
                            break;
                    }
                }
            }
            changed = true;
            ResetGrid();
        }
        private void ResetGrid()
        {
            if (adaptation != null)
                adaptation.VideoMaps = maps;
            maps.OrderBy(x => x.Number);
            source.DataSource = new BindingList<VideoMap>(maps);
            dataGridView1.DataSource = source;
            source.ResetBindings(false);
            dataGridView1.AutoResizeColumns();
            propertyGrid1.PropertyValueChanged += PropertyGrid1_PropertyValueChanged;
        }
        private void PropertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            changed = true;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > dataGridView1.Rows.Count - 1)
            {
                var selectedmaps = dataGridView1.Rows
                                        .Cast<DataGridViewRow>()
                                        .Select(d => (VideoMap)d.DataBoundItem)
                                        .Where(d => d != null).
                                        ToArray();
                propertyGrid1.SelectedObjects = selectedmaps;
            }
            else if (dataGridView1.SelectedRows.Count > 1)
            {
                var selectedmaps = dataGridView1.Rows
                                        .Cast<DataGridViewRow>()
                                        .Where(r => r.Selected == true)
                                        .Select(d => (VideoMap)d.DataBoundItem).
                                        ToArray();
                propertyGrid1.SelectedObjects = selectedmaps;
            }
            else
            {
                propertyGrid1.SelectedObject = source.Current;
            }
        }

        private void SaveVideoMapsToFile(bool saveas = false)
        {
            if (Filename == null || Filename == "" || saveas)
            {
                using (SaveFileDialog s = new SaveFileDialog())
                {
                    s.Filter = "Video Maps (*.geojson;*.json)|*.geojson;*.json|All Files|*.*";
                    s.FilterIndex = 1;
                    s.RestoreDirectory = true;

                    if(s.ShowDialog() == DialogResult.OK)
                    {
                        Filename = s.FileName;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            if (Filename == null || Filename == "")
                return;
            try
            {
                VideoMapList.SerializeToJsonFile(maps, Filename);
                changed = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveVideoMapsToFile();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveVideoMapsToFile(true);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckChange())
                return;
            string filename;
            using (OpenFileDialog s = new OpenFileDialog())
            {
                s.Filter = "JSON video maps (*.geojson;*.json)|*.geojson;*.json|All Files|*.*";
                s.FilterIndex = 1;
                s.RestoreDirectory = true;

                if (s.ShowDialog() == DialogResult.OK)
                {
                    filename = s.FileName;
                }
                else
                {
                    return;
                }
            }
            try
            {
                maps = VideoMapList.DeserializeFromJsonFile(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            Filename = filename;
            ResetGrid();
        }
        private bool CheckChange()
        {
            if (changed)
            {
                switch (MessageBox.Show("Do you want to save your changes?", "Unsaved changes", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Cancel:
                        return false;
                    case DialogResult.Yes:
                        SaveVideoMapsToFile();
                        break;
                    case DialogResult.No:
                        return true;
                }
            }
            return !changed;
        }
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckChange())
                return;
            Filename = "";
            maps = new VideoMapList();
            ResetGrid();
        }

        private void fromFAAdatFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "FAA .dat Files (*.dat)|*.dat|All Files|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var map = FAAMapDATFileParser.GetMapFromFile(filePath);
                    map.Number = maps.Last().Number + 1;
                    maps.Add(map);
                }
            }
            changed = true;
            ResetGrid();
        }

        private void toGeoJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "GeoJSON Files (*.geojson; *.json)|*.geojson; *.json|All Files|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath = saveFileDialog.FileName;
                    if (dataGridView1.SelectedRows.Count != 1)
                        GeoJSONMapExporter.MapsToGeoJSONFile(new VideoMapList(propertyGrid1.SelectedObjects), filePath);
                    else if (propertyGrid1.SelectedObject.GetType() == typeof(VideoMap))
                        GeoJSONMapExporter.MapToGeoJSONFile(propertyGrid1.SelectedObject as VideoMap, filePath);
                }
            }
        }

        private void fromGeoJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "GeoJSON Files (*.geojson; *.json)|*.geojson;*.json|All Files|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var newmaps = GeoJSONMapExporter.GeoJSONFileToMaps(filePath);
                    maps.AddRange(newmaps);
                }
            }
            changed = true;
            ResetGrid();
        }
    }

    public class VideoMapCollectionEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (value.GetType() != typeof(VideoMapList))
                return value;
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc != null)
            {
                RadarWindow adaptation = context.Instance as RadarWindow;
                VideoMapForm sel = new VideoMapForm((VideoMapList)value, adaptation.VideoMapFilename, adaptation);
                sel.ShowDialog();
            }
            return value;
        }
    }
}
