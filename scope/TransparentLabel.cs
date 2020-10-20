using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace DGScope
{
    /// <summary>
    /// A label that can be transparent.
    /// </summary>
    public class TransparentLabel : Control, IScreenObject
    {
        bool _flashing = false;
        bool flashOn = true;
        System.Threading.Timer flashtimer;
        
        public bool Flashing {
            get
            {
                return _flashing;
            }
            set
            {
                if (value && value != _flashing)
                {
                    flashtimer = new System.Threading.Timer(new TimerCallback(cbFlashTimerElapsed), null, 750, 750);
                }
                else if (!value)
                {
                    if (flashtimer != null)
                    {
                        flashtimer.Dispose();
                    }
                }
                _flashing = value;
            }
        }

        public override Color ForeColor 
        {
            get
            {
                if (!Flashing || flashOn)
                {
                    return base.ForeColor;
                }
                else
                {
                    return Color.Transparent;
                }
            }
            set
            {
                base.ForeColor = value;
            }
        }
        public bool InBoundsF => LocationF.X > -1 && LocationF.X < 1 && LocationF.Y > -1 && LocationF.Y < 1;
        /// <summary>
        /// Creates a new <see cref="TransparentLabel"/> instance.
        /// </summary>
        public TransparentLabel()
        {
            TabStop = false;
            this.SetStyle(
  //ControlStyles.AllPaintingInWmPaint |
  ControlStyles.UserPaint |
  ControlStyles.DoubleBuffer, false);
        }

        /// <summary>
        /// Gets the creation parameters.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        /// <summary>
        /// Paints the background.
        /// </summary>
        /// <param name="e">E.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // do nothing
        }

        /// <summary>
        /// Paints the control.
        /// </summary>
        /// <param name="e">E.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            DrawText();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            DrawText();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Redraw = true;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x000F)
            {
                DrawText();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            DrawText();
        }

        private void cbFlashTimerElapsed(object state)
        {
            flashOn = flashOn ? false : true;
        }

        private Bitmap _backBuffer;
        public void DrawText()
        {

            using (Graphics g = CreateGraphics())
            {

                //_backBuffer = TextBitmap();

                //g.DrawImageUnscaled(_backBuffer, 0, 0);
            }
        }

        public int TextureID { get; set; }
        public PointF LocationF { get; set; }
        public PointF NewLocation { get; set; }
        public SizeF SizeF { get; set; }
        public Aircraft ParentAircraft { get; set; }
        public RectangleF BoundsF
        {
            get
            {
                return new RectangleF(LocationF.X, LocationF.Y, SizeF.Width, SizeF.Height);
            }
        }
        public bool Redraw { get; set; } = true;
        public Bitmap TextBitmap()
        {
            SizeF size;
            using (Graphics graphics = CreateGraphics())
            {
                size = graphics.MeasureString(Text, Font);
                if (AutoSize)
                {
                    Width = (int)size.Width;
                    Height = (int)size.Height;
                }
            }
            Bitmap _backBuffer = new Bitmap(Width + 1, Height + 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(_backBuffer))
            {
                
                
                using(SolidBrush brush = new SolidBrush(Color.Transparent))
                {
                    graphics.FillRectangle(brush, 0, 0, Width, Height);
                }
                using (SolidBrush brush = new SolidBrush(ForeColor))
                {

                    // first figure out the top
                    float top = 0;
                    switch (textAlign)
                    {
                        case ContentAlignment.MiddleLeft:
                        case ContentAlignment.MiddleCenter:
                        case ContentAlignment.MiddleRight:
                            top = (Height - size.Height) / 2;
                            break;
                        case ContentAlignment.BottomLeft:
                        case ContentAlignment.BottomCenter:
                        case ContentAlignment.BottomRight:
                            top = Height - size.Height;
                            break;
                    }

                    float left = -1;
                    switch (textAlign)
                    {
                        case ContentAlignment.TopLeft:
                        case ContentAlignment.MiddleLeft:
                        case ContentAlignment.BottomLeft:
                            if (RightToLeft == RightToLeft.Yes)
                                left = Width - size.Width;
                            else
                                left = -1;
                            break;
                        case ContentAlignment.TopCenter:
                        case ContentAlignment.MiddleCenter:
                        case ContentAlignment.BottomCenter:
                            left = (Width - size.Width) / 2;
                            break;
                        case ContentAlignment.TopRight:
                        case ContentAlignment.MiddleRight:
                        case ContentAlignment.BottomRight:
                            if (RightToLeft == RightToLeft.Yes)
                                left = -1;
                            else
                                left = Width - size.Width;
                            break;
                    }
                    graphics.DrawString(Text, Font, brush, left, top);
                }
            }
            return _backBuffer;
        }
        /// <summary>
        /// Gets or sets the text associated with this control.
        /// </summary>
        /// <returns>
        /// The text associated with this control.
        /// </returns>
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                if (value == base.Text)
                    return;
                base.Text = value;
                RecreateHandle();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether control's elements are aligned to support locales using right-to-left fonts.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// One of the <see cref="T:System.Windows.Forms.RightToLeft"/> values. The default is <see cref="F:System.Windows.Forms.RightToLeft.Inherit"/>.
        /// </returns>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
        /// The assigned value is not one of the <see cref="T:System.Windows.Forms.RightToLeft"/> values.
        /// </exception>
        public override RightToLeft RightToLeft
        {
            get
            {
                return base.RightToLeft;
            }
            set
            {
                base.RightToLeft = value;
                RecreateHandle();
            }
        }

        /// <summary>
        /// Gets or sets the font of the text displayed by the control.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The <see cref="T:System.Drawing.Font"/> to apply to the text displayed by the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultFont"/> property.
        /// </returns>
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                RecreateHandle();
            }
        }

        private ContentAlignment textAlign = ContentAlignment.TopLeft;
        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        public ContentAlignment TextAlign
        {
            get { return textAlign; }
            set
            {
                textAlign = value;
                RecreateHandle();
            }
        }

        
        
    }
}