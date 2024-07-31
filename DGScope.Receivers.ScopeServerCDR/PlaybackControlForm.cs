using System;
using System.Windows.Forms;

namespace DGScope.Receivers.Falcon
{
    public partial class PlaybackControlForm : Form
    {
        private ScopeServerCDRReceiver rx;
        public PlaybackControlForm(ScopeServerCDRReceiver receiver)
        {
            InitializeComponent();
            Timer timer = new Timer();
            timer.Interval = 50;
            timer.Enabled = true;
            timer.Tick += TimerCallback;
            rx = receiver;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "ScopeServer CDR Playback File (*.cdr)|*.cdr|All files (*.*)|*.*";
                fd.FilterIndex = 1;
                fd.CheckFileExists = true;
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var ff = CDRFile.FromFile(fd.FileName);
                        textBox1.Text = fd.FileName;
                        rx.File = ff;
                        reInitButtons();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not parse playback file.\r\n{0}", ex.Message);
                    }
                }
            }
        }

        private void reInitButtons()
        {
            if (rx.File == null)
            {
                trackBar1.Enabled = false;
                trackBar2.Enabled = false;
                btnPlayPause.Enabled = false;
            }
            else
            {
                trackBar1.Minimum = 0;
                trackBar1.Maximum = (int)rx.File.LengthOfData.TotalMilliseconds + 1;
                trackBar1.Value = 0;
                trackBar1.Enabled = true;

                btnPlayPause.Enabled = true;

                comboBox1.Items.Clear();
                comboBox1.Items.Add("(All)");
                foreach (var site in rx.Sites)
                {
                    comboBox1.Items.Add(site);
                }
                comboBox1.SelectedIndex = 0;

                trackBar2.Enabled = true;
            }

            trackBar2.Value = 0;
            trackBar1.Value = 0;
        }

        private void btnPlayPause_Click(object sender, EventArgs e)
        {
            if (rx.File != null)
            {
                if (rx.Playing)
                {
                    rx.Pause();
                    btnPlayPause.Text = "Run";
                }
                else
                {
                    rx.Play();
                    btnPlayPause.Text = "Stop";
                }
            }
        }

        
        internal void UpdateCallback()
        {

        }
        private void TimerCallback(object sender, EventArgs args)
        {
            if (rx != null)
            {
                label1.Text = rx.CurrentTime.ToString();
                checkBox1.Checked = rx.IncludeUncorrelated;
                var elapsed = rx.CurrentTime - rx.StartOfData;
                if (elapsed.HasValue && (int)(elapsed.Value.TotalMilliseconds) <= trackBar1.Maximum && (int)(elapsed.Value.TotalMilliseconds) >= trackBar1.Minimum && rx.Playing) 
                {
                    trackBar1.Value = (int)elapsed.Value.TotalMilliseconds;
                }
                label2.Text = rx.Speed.ToString("0.0") + "x";
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            rx.Pause();
            if (rx != null && rx.StartOfData.HasValue)
            {
                rx.CurrentTime = rx.StartOfData.Value + TimeSpan.FromMilliseconds(trackBar1.Value);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            rx.IncludeUncorrelated = checkBox1.Checked;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            if (rx != null)
            {
                rx.Speed = Math.Pow(10, trackBar2.Value / 10.0d);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            rx.SelectedSite = comboBox1.SelectedItem.ToString();
        }
    }
}
