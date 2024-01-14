using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace DGScope
{
    internal abstract class DCBButton : DCBMenuItem
    {
        int bordersize = 3;
        Rectangle internalrectangle;
        bool drawnvertically = false;
        bool drawnhorizontally = false;
        int textureID = 0;
        string drawntext;
        public event EventHandler Click;
        public bool Active { get; set; }
        public Color BackColorActive { get; set; } = Color.Green;
        public Color BackColorInactive { get; set; } = Color.FromArgb(0, 80, 0);
        public Color BackColorDisabled { get; set; } = Color.FromArgb(0, 40, 0);
        public Color ForeColor { get; set; } = Color.White;
        public Color ForeColorDwell { get; set; } = Color.Yellow;
        public Color ForeColorDisabled { get; set; } = Color.DarkGray;
        public Font Font { get; set; } = new Font("FixedDemiBold", 8);
        public StringFormat StringFormat { get; set; } = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        public string Text { get; set; }
        private void PaintText()
        {
            if (DrawnBounds.Size.IsEmpty)
                return;
            var img = new Bitmap(DrawnBounds.Width, DrawnBounds.Height);
            using (Graphics graphics = Graphics.FromImage(img))
            {
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
        }
        public override void Draw(bool vertical)
        {
            int width = RotateIfVertical && vertical ? Height : Width;
            int height = RotateIfVertical && vertical ? Width : Height;
            var drawactive = Active != (mousePressed && mouseInside);
            internalrectangle = new Rectangle(bordersize, bordersize, width - 2 * bordersize, height - 2 * bordersize);
            DrawnBounds = new Rectangle(Left, Top, width, height);
            GL.PushMatrix();
            GL.Translate(Left, Top, 0);

            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(!drawactive ? Color.DarkGray : Color.Black);
            GL.Vertex2(0, 0);
            GL.Vertex2(width, 0);
            GL.Vertex2(width - bordersize, bordersize);
            GL.Vertex2(bordersize, bordersize);
            GL.Vertex2(bordersize, height - bordersize);
            GL.Vertex2(0, height);
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(drawactive ? Color.DarkGray : Color.Black);
            GL.Vertex2(width, height);
            GL.Vertex2(width, 0);
            GL.Vertex2(width - bordersize, bordersize);
            GL.Vertex2(width - bordersize, height - bordersize);
            GL.Vertex2(bordersize, height - bordersize);
            GL.Vertex2(0, height);
            GL.End();
            GL.Begin(PrimitiveType.Polygon);
            if (Enabled)
                GL.Color4(drawactive ? BackColorActive : BackColorInactive);
            else
                GL.Color4(BackColorDisabled);
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
            if (drawnhorizontally == vertical || drawnvertically != vertical || drawntext != Text)
            {
                PaintText();
                drawnvertically = vertical;
                drawnhorizontally = !vertical;
            }
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.Begin(PrimitiveType.Quads);
            if (Enabled)
                GL.Color3(mouseInside ? ForeColorDwell : ForeColor);
            else
                GL.Color3(ForeColorDisabled);
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
            }
        }
        public override void MouseMove(Point position)
        {
            if (!Enabled) return;
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
            if (!Enabled) return;
            if (mouseInside)
            {
                mousePressed = true;
            }
        }
        public override void MouseUp()
        {
            if (!Enabled) return;
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
    }

    internal class DCBSubmenuButton : DCBButton
    {
        public DCBMenu Submenu { get; set; }
        public override void Draw(bool vertical)
        {
            base.Draw(vertical);
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
            Submenu.LayoutButtons(vertical);
            GL.PushMatrix();
            if (vertical)
                Submenu.Top = DrawnBounds.Bottom;
            else
                Submenu.Left = DrawnBounds.Right;
            Submenu.Draw(vertical);
            GL.PopMatrix();
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
            }
        }
    }

    internal class DCBActionButton : DCBButton
    {
     
    }

    internal class DCBRadioButton : DCBButton
    {
     
        public List<DCBRadioButton> OtherButtons { get; set; }
    }
}
