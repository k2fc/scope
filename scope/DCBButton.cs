using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace DGScope
{
    internal class DCBButton : DCBMenuItem
    {
        int bordersize = 3;
        Rectangle internalrectangle;
        bool drawnvertically = false;
        bool drawnhorizontally = false;
        int textureID = 0;
        string drawntext;
        bool enabled => Enabled && !Disabled;

        Font drawnfont;

        public event EventHandler Click;
        public bool Active { get; set; }
        public Color BackColorActive { get; set; } = Color.Green;
        public Color BackColorInactive { get; set; } = Color.FromArgb(0, 80, 0);
        public Color BackColorDisabled { get; set; } = Color.FromArgb(0, 40, 0);
        public Color ForeColor { get; set; } = Color.White;
        public Color ForeColorDwell { get; set; } = Color.Yellow;
        public Color ForeColorDisabled { get; set; } = Color.DarkGray;
        public override Font Font { get; set; }
        public StringFormat StringFormat { get; set; } = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip };
        public string Text { get; set; }
        private void PaintText()
        {
            if (DrawnBounds.Size.IsEmpty)
                return;
            var img = new Bitmap(DrawnBounds.Width, DrawnBounds.Height);
            using (Graphics graphics = Graphics.FromImage(img))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                using (SolidBrush brush = new SolidBrush(Color.White))
                    graphics.DrawString(Text, this.Font, brush, internalrectangle, StringFormat);
            }
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            var data = img.LockBits(new Rectangle(0,0,img.Width, img.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            img.UnlockBits(data);
            drawntext = Text;
            drawnfont = Font;
        }
        public override void Draw(Point location, bool vertical, int brightness)
        {
            int width = RotateIfVertical && vertical ? Height : Width;
            int height = RotateIfVertical && vertical ? Width : Height;
            var drawactive = Active != (mousePressed && mouseInside);
            internalrectangle = new Rectangle(0, 0, width, height);
            DrawnBounds = new Rectangle(location.X + Left, location.Y + Top, width, height);
            GL.PushMatrix();
            GL.Translate(Left, Top, 0);

            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(!drawactive ? RadarWindow.AdjustedColor(Color.DarkGray, brightness) : Color.Black);
            GL.Vertex2(0, 0);
            GL.Vertex2(width, 0);
            GL.Vertex2(width - bordersize, bordersize);
            GL.Vertex2(bordersize, bordersize);
            GL.Vertex2(bordersize, height - bordersize);
            GL.Vertex2(0, height);
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(drawactive ? RadarWindow.AdjustedColor(Color.DarkGray, brightness) : Color.Black);
            GL.Vertex2(width, height);
            GL.Vertex2(width, 0);
            GL.Vertex2(width - bordersize, bordersize);
            GL.Vertex2(width - bordersize, height - bordersize);
            GL.Vertex2(bordersize, height - bordersize);
            GL.Vertex2(0, height);
            GL.End();
            GL.Begin(PrimitiveType.Polygon);
            if (enabled)
                GL.Color4(drawactive ? RadarWindow.AdjustedColor(BackColorActive, brightness) : RadarWindow.AdjustedColor(BackColorInactive, brightness));
            else
                GL.Color4(RadarWindow.AdjustedColor(BackColorDisabled, brightness));
            GL.Vertex2(bordersize, bordersize);
            GL.Vertex2(width - bordersize, bordersize);
            GL.Vertex2(width - bordersize, height - bordersize);
            GL.Vertex2(bordersize, height - bordersize);
            GL.End();

            GL.Enable(EnableCap.Texture2D);
            if (textureID == 0)
            {
                textureID = GL.GenTexture();
            }
            if (drawnhorizontally == vertical || drawnvertically != vertical || drawntext != Text || drawnfont != Font)
            {
                PaintText();
                drawnvertically = vertical;
                drawnhorizontally = !vertical;
            }
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.Begin(PrimitiveType.Quads);
            if (enabled)
                GL.Color3(mouseInside ? RadarWindow.AdjustedColor(ForeColorDwell, brightness) : RadarWindow.AdjustedColor(ForeColor, brightness));
            else
                GL.Color3(RadarWindow.AdjustedColor(ForeColorDisabled, brightness));
            GL.TexCoord2(0, 0);
            GL.Vertex2(0, 0);
            GL.TexCoord2(1, 0);
            GL.Vertex2(DrawnBounds.Width, 0);
            GL.TexCoord2(1, 1);
            GL.Vertex2(DrawnBounds.Width, DrawnBounds.Height);
            GL.TexCoord2(0, 1);
            GL.Vertex2(0, DrawnBounds.Height);
            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);

            GL.PopMatrix();
        }
        bool mouseInside = false;
        bool mousePressed = false;
        public new bool Enabled
        {
            get => base.Enabled;
            set
            {
                if (!value)
                {
                    mouseInside = false;
                    mousePressed = false;
                }
                base.Enabled = value;
            }
        }
        public bool Disabled { get; set; }
        public override void MouseMove(Point position)
        {
            if (!enabled) return;
            if (DrawnBounds.Contains(position))
            {
                mouseInside = true;
            }
            else
            {
                mouseInside = false;
            }
        }
        public override void MouseDown()
        {
            if (!enabled) return;
            if (mouseInside)
            {
                mousePressed = true;
            }
        }
        public override void MouseUp()
        {
            if (!enabled) return;
            if (mouseInside && mousePressed) 
            {
                OnClick(new EventArgs());
            }
            mousePressed = false;
        }
        public virtual void OnClick(EventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }

    internal class DCBToggleButton : DCBButton
    {
        public event EventHandler ClickOff;
        public event EventHandler ClickOn;
        
        public override void OnClick(EventArgs e)
        {
            if (Active)
            {
                ClickOff?.Invoke(this, e);
            }
            else
            {
                ClickOn?.Invoke(this, e);
            }
            base.OnClick(e);
        }
    }

    internal class DCBAdjustmentButton : DCBButton
    {
        public event EventHandler Down;
        public event EventHandler Up;
        bool active;
        public override void OnClick(EventArgs e)
        {
            active = !Active;
            if (active)
            {
                ParentMenu.Enabled = false;
                Enabled = true;
                Active = true;
            }
            else
            {
                ParentMenu.Enabled = true;
                Active = false;
            }
            base.OnClick(e);
        }
        public override void MouseDown()
        {
            if (!Active)
                base.MouseDown();
        }
        public void MouseWheel(int delta)
        {
            if (!Active) return;
            if (delta < 0)
            {
                Down?.Invoke(this, null);
            }
            else
            {
                Up?.Invoke(this, null);
            }
        }
    }

    internal class DCBSubmenuButton : DCBButton
    {
        private Point windowLocation;
        
        public DCBMenu Submenu { get; set; }
        public override void Draw(Point location, bool vertical, int brightness)
        {
            base.Draw(location, vertical, brightness);
            if (!Active)
                return;
            if (vertical)
            {
                Submenu.Width = DrawnBounds.Width;
            }
            else
            {
                Submenu.Height = DrawnBounds.Height;
            }
            if (Submenu.Font != Font) 
                Submenu.Font = Font;
            Submenu.LayoutButtons(vertical);
            GL.PushMatrix();
            GL.Translate(Left, Top, 0);
            if (vertical)
            {
                GL.Translate(0, Height, 0);
                Submenu.Draw(new Point(location.X + Left, location.Y + Bottom), vertical, brightness);
            }
            else
            {
                GL.Translate(Width, 0, 0);
                Submenu.Draw(new Point(location.X + Right, location.Y + Top), vertical, brightness);
            }
            //Submenu.DrawnBounds = new Rectangle(new Point(DrawnBounds.Left + Submenu.Left, DrawnBounds.Top + Submenu.Top), Submenu.Size);
            var newpoint = new Point(windowLocation.X + Submenu.DrawnBounds.Location.X, windowLocation.Y + Submenu.DrawnBounds.Location.Y);
            System.Windows.Forms.Cursor.Clip = new Rectangle(newpoint, Submenu.DrawnBounds.Size);

            GL.PopMatrix();
            Submenu.SubmenuButton = this;
        }
        public override void MouseDown()
        {
            base.MouseDown();
            if (Active)
            {
                Submenu.MouseDown();
            }
        }
        public override void MouseUp()
        {
            base.MouseUp();
            if (Active)
            {
                Submenu.MouseUp();
            }
        }
        public override void MouseMove(Point position)
        {
            base.MouseMove(position);
            if (Active)
            {
                Submenu.MouseMove(position);
            }
        }
        public override void OnClick(EventArgs e)
        {
            Active = !Active;
            if (Active)
            {
                ParentMenu.Enabled = false;
            }
            else
            {
                ParentMenu.Enabled = true;
                System.Windows.Forms.Cursor.Clip = new Rectangle();
            }
            base.OnClick(e);
        }
        public void SetScreenLocation(Point point)
        {
            windowLocation = point;
        }
    }

    internal class DCBActionButton : DCBButton
    {
        public override void OnClick(EventArgs e)
        {
            Active = !Active;
            if (Active)
            {
                ParentMenu.Enabled = false;
                Enabled = true;
            }
            else
            {
                ParentMenu.Enabled = true;
            }
            base.OnClick(e);
        }
        public void ActionDone()
        {
            Active = false;
            ParentMenu.Enabled = true;
        }
    }

    internal class DCBRadioButton : DCBButton
    {
     
        public List<DCBRadioButton> OtherButtons { get; set; }
    }
}
