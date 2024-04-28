using System;
using System.Windows.Forms;

namespace DGScope.Receivers.Falcon
{
    public partial class PlaybackControlForm : Form
    {
        private FalconReceiver rx;
        public PlaybackControlForm(FalconReceiver receiver)
        {
            InitializeComponent();
            Timer timer = new Timer();
            timer.Interval = 50;
            timer.Enabled = true;
            timer.Tick += TimerCallback;
            rx = receiver;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                fd.Filter = "Falcon Playback File (*.ppb)|*.ppb|All files (*.*)|*.*";
                fd.FilterIndex = 1;
                fd.CheckFileExists = true;
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var ff = FalconFile.FromFile(fd.FileName);
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
                btnPlayPause.Enabled = false;
            }
            else
            {
                trackBar1.Minimum = 0;
                trackBar1.Maximum = (int)rx.File.LengthOfData.TotalMilliseconds + 1;
                trackBar1.Value = 0;
                trackBar1.Enabled = true;

                btnPlayPause.Enabled = true;
            }
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
                label1.Text = rx.CurrentTime.ToShortTimeString();
                var elapsed = rx.CurrentTime - rx.StartOfData;
                if (elapsed.HasValue && (int)(elapsed.Value.TotalMilliseconds) <= trackBar1.Maximum) 
                {
                    trackBar1.Value = (int)elapsed.Value.TotalMilliseconds;
                }
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (rx != null && rx.StartOfData.HasValue)
            {
                rx.CurrentTime = rx.StartOfData.Value + TimeSpan.FromMilliseconds(trackBar1.Value);
            }
        }
    }
}
