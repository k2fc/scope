using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DGScope.MapImporter.CRC
{
    public partial class CRCFacilityPicker : Form
    {
        public string PickedFacility;
        public CRCFacilityPicker(List<string> facilities)
        {
            InitializeComponent();
            comboBox1.Items.Clear();
            foreach (string facility in facilities)
            {
                comboBox1.Items.Add(facility);
            }
        }

        private void CRCFacilityPicker_Load(object sender, EventArgs e)
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                PickedFacility = comboBox1.SelectedItem.ToString();
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
