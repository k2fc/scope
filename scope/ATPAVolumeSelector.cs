using System;
using System.Collections.Generic;
using System.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.Design;
using System.Runtime.CompilerServices;

namespace DGScope
{
    public partial class ATPATwoPointFiveSelector : ATPAVolumeSelector
    {
        public ATPATwoPointFiveSelector(List<ATPAVolume> ATPAVolumes) 
        {
            InitializeComponent();
            base.ATPAVolumes = ATPAVolumes;
            LoadListBox();
        }


        private void CheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ATPAVolumes[e.Index].TwoPointFiveActive = e.NewValue == CheckState.Checked;
        }



        private void LoadListBox()
        {
            checkedListBox1.Items.Clear();
            foreach (ATPAVolume item in ATPAVolumes)
            {
                if (item.TwoPointFiveEnabled && item.Active)
                {
                    checkedListBox1.Items.Add(item, item.TwoPointFiveActive);
                }
            }
            checkedListBox1.ItemCheck += CheckedListBox1_ItemCheck;
        }

    }
    public partial class ATPAVolumeSelector : Form
    {
        protected List<ATPAVolume> ATPAVolumes;
        
        public ATPAVolumeSelector(List<ATPAVolume> ATPAVolumes)
        {
            InitializeComponent();
            this.ATPAVolumes = ATPAVolumes;
            LoadListBox();
        }
        public ATPAVolumeSelector() { }


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

    public class ATPATwoPointFiveSelectorEditor : UITypeEditor
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
                ATPATwoPointFiveSelector sel = new ATPATwoPointFiveSelector(adaptation.ATPA.Volumes);
                sel.Show();
            }
            return value;
        }
    }
}
