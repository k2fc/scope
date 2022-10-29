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
    
    public partial class ATPAVolumeSelector : Form
    {
        private List<ATPAVolume> ATPAVolumes;
        
        public ATPAVolumeSelector(List<ATPAVolume> ATPAVolumes)
        {
            InitializeComponent();
            this.ATPAVolumes = ATPAVolumes;
            LoadListBox();
        }


        private void CheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ATPAVolumes[e.Index].Active = e.NewValue == CheckState.Checked;
        }

        

        private void LoadListBox()
        {
            checkedListBox1.Items.Clear();
            foreach (ATPAVolume item in ATPAVolumes)
                checkedListBox1.Items.Add(item, item.Active);
            checkedListBox1.ItemCheck += CheckedListBox1_ItemCheck;
        }

        

    }
    public class ATPAVolumeSelectorEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (value.GetType() != typeof(List<ATPAVolume>))
                return value;
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc != null)
            {
                RadarWindow adaptation = context.Instance as RadarWindow;
                ATPAVolumeSelector sel = new ATPAVolumeSelector(adaptation.ATPA.Volumes);
                sel.Show();
            }
            return value;
        }
    }
}
