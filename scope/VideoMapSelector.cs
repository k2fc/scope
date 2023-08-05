using System;
using System.Collections.Generic;
using System.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.Design;
using System.Linq;

namespace DGScope
{
    
    public partial class VideoMapSelector : Form
    {
        private VideoMapList videoMaps;
        private List<VideoMap> sortedmaps;
        
        public VideoMapSelector(VideoMapList videoMaps)
        {
            InitializeComponent();
            this.videoMaps = videoMaps;
            this.KeyDown += VideoMapSelector_KeyDown;
            LoadListBox();
        }

        private void VideoMapSelector_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2 && e.Control)
                this.Close();
        }

        private void CheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            sortedmaps[e.Index].Visible = e.NewValue == CheckState.Checked;
        }

        

        private void LoadListBox()
        {
            sortedmaps = videoMaps.OrderBy(x => x.Number).ToList();
            checkedListBox1.Items.Clear();
            foreach (VideoMap item in sortedmaps)
                checkedListBox1.Items.Add(item, item.Visible);
            checkedListBox1.ItemCheck += CheckedListBox1_ItemCheck;
        }

        

        

        

        
        
    }
    public class OldVideoMapCollectionEditor : UITypeEditor
    {
        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            // Return the value if the value is not of type Int32, Double and Single.
            if (value.GetType() != typeof(VideoMapList))
                return value;

            // Uses the IWindowsFormsEditorService to display a
            // drop-down UI in the Properties window.
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc != null)
            {
                VideoMapSelector sel = new VideoMapSelector((VideoMapList)value);
                sel.ShowDialog();
            }
            return value;
        }
    }
}
